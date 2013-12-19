using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Tcp;

using SharedLib.DataserverObjects;
using SharedLib.MetadataObjects;
using SharedLib;
using SharedLib.Exceptions;

namespace DataServer
{
    class DataServer : MarshalByRefObject, IDataToClient, IDataToMeta, IDataToPuppet, IClientToMeta
    {
        static public List<ServerId> MetadataServerList = new List<ServerId>();
        static public int ServerId;
        static public String ServerIp;
        static public int ServerPort;
        static public int RecoveryPort;
        static public StorageManager storage;
        static private Boolean isFreezed = true;
        static private Boolean isFailed = false;
        static private object freezeMon = new object();
        //LocalFilename/Version
        private ConcurrentDictionary<String, int> _localFileNameList = new ConcurrentDictionary<String, int>(); 

        public static MetaserverAsyncClient MetaserverClient;

        public DataServer()
        {
        }



        static void Main(string[] args)
        {
            System.Console.WriteLine("Dataserver Init....");

           //if (!Debugger.IsAttached)
             //   Debugger.Launch();


            int i = 0;
            for (i = 0; i < (args.Length - 4); i++)
            {
                ServerId metaServer = new ServerId();
                metaServer.hostname = args[i++];
                metaServer.port = Convert.ToInt32(args[i]);
                metaServer.id = Convert.ToString(i / 2);
                MetadataServerList.Add(metaServer);
            }
            ServerPort = Convert.ToInt32(args[i++]);
            ServerId = Convert.ToInt32(args[i++]);
            RecoveryPort = Convert.ToInt32(args[i++]);
            ServerIp = DataServer.GetCurrentIp();
            storage = new StorageManager(ServerId);


            System.Console.WriteLine("System ID: " + ServerId
                );
            System.Console.WriteLine("System IP Adress: " + ServerIp + ":" + ServerPort);

            // Cria objecto remoto para ser envocado apartir do cliente
            RemotingConfiguration.Configure("../../App.config", true);


            TcpChannel channel = new TcpChannel(ServerPort);
            RemotingConfiguration.RegisterWellKnownServiceType(
              typeof(DataServer),
              "PADIConnection",
              WellKnownObjectMode.Singleton);


            // Espera ate receber unfreze do puppet Master
            System.Console.WriteLine("DataServer Started, In Freeze Mode");
            while (isFreezed)
            {
                Thread.Sleep(250);
            }
            System.Console.WriteLine("DataServer Started Defrost!!!");


            // Regista-se no Metadata e inicia Objectos Remotos
            MetaserverClient = new MetaserverAsyncClient(ServerIp, ServerPort, MetadataServerList, ServerId);
            RegisterAtMetadata();

            // Releses the Request Thread and Returns to Puppet
            Monitor.Enter(freezeMon);
            Monitor.PulseAll(freezeMon);
            Monitor.Exit(freezeMon);


            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }






        // Bloqueante ate se rgistar
        public static void RegisterAtMetadata()
        {
            RequestRegistry request = new RequestRegistry(ServerId.ToString(), ServerIp, ServerPort);
            try
            {
                MetaserverResponse response = MetaserverClient.SendRequestToMetaserver(request);
                if (response.Status == ResponseStatus.Success)
                    Console.WriteLine("Registered on Metadata Servers  ");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dataserver: Error: Registry at metaserve" + ex.Message);
            }
        }


        // Evita que o objecto é reciclado
        public override object InitializeLifetimeService()
        {
            return null;

        }


        // Devolve IP local da maquina
        public static String GetCurrentIp()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Dataserver " + ServerId + "cannot get LocalIP");
        }















        public Boolean ReceiveMetaserverResponse(MetaserverResponse response)
        {
            return MetaserverClient.ReceiveMetaserverResponse(response);
        }



        // Operaçoes De Storage

        public String Write(string localFileName, byte[] data, int version)
        {
            Monitor.Enter(freezeMon);
            Console.WriteLine("Write: " + localFileName + " TFile: " + data + " at Version " + version);
            if (isFreezed)
            {
                Console.WriteLine("Write: Freeze");
                Monitor.Wait(freezeMon);
                Console.WriteLine("Write: Defrost");

            }

            TFile t = new TFile(version, data);
            int currentVersion;
            //TODO colocar locks aqui
            _localFileNameList.TryRemove(localFileName, out  currentVersion);
            _localFileNameList.TryAdd(localFileName, version);

            storage.WriteFile(localFileName, t);
            Monitor.Exit(freezeMon);
            return ServerId.ToString();

        }

        public TFile Read(string localFileName, SemanticType semantic, int minVersion)
        {
            Monitor.Enter(freezeMon);
            Console.WriteLine("Read: " + localFileName + " Semantic: " + semantic + " MinVersion: " + minVersion);

            if (isFreezed)
            {
                Console.WriteLine("Read: Freeze");
                Monitor.Wait(freezeMon);
                Console.WriteLine("Read: Defrost");
            }

            if (semantic.Equals(SemanticType.Monotonic))
            {
                TFile t = storage.ReadFile(localFileName);
                if (t.VersionNumber >= minVersion)
                {
                    t.responseServerId = ServerId.ToString();
                    Monitor.Exit(freezeMon);
                    return t;
                }
                else
                {
                    Monitor.Exit(freezeMon);
                    throw new PadiException(PadiExceptiontType.ReadFile, "DataServer: Monotonic Read Cannot Be Made");
                }
            }

            if (semantic.Equals(SemanticType.Default))
            {
                TFile t = storage.ReadFile(localFileName);
                t.responseServerId = ServerId.ToString();
                Monitor.Exit(freezeMon);
                return t;
            }

            else
            {
                Monitor.Exit(freezeMon);
                throw new PadiException(PadiExceptiontType.ReadFile, "DataServer: invalid SemanticType Read");
            }
        }

        public VersionFile GetVersion(String localfileName)
        {
            Monitor.Enter(freezeMon);
            Console.WriteLine("GetVersion: " + localfileName);

            if (isFreezed)
            {
                Console.WriteLine("GetVersion: Freeze");
                Monitor.Wait(freezeMon);
                Console.WriteLine("GetVersion: Defrost");
            }

            TFile t = storage.ReadFile(localfileName);
            Monitor.Exit(freezeMon);
            return new VersionFile(ServerId.ToString(), t.VersionNumber);
        }


        public void CreateEmptyFile(string localFilename)
        {
            TFile t = new TFile(0, new byte[0]);
            storage.WriteFile(localFilename, t);
            Console.WriteLine("Create new Empty File: " + localFilename);
            _localFileNameList.TryAdd(localFilename, 0);
        }

        public string Freeze()
        {
            isFreezed = true;
            Console.WriteLine("DataServer " + ServerId + " Freezed State");
            return "DataServer " + ServerId + " Freezed State";
        }

        public string UnFreeze()
        {
            if (isFreezed)
            {
                isFreezed = false;
                // Releses the Request Thread and Returns to Puppet
                Monitor.Enter(freezeMon);
                Monitor.Pulse(freezeMon);
                Monitor.Exit(freezeMon);
                return "DataServer " + ServerId + " Unfreezed";
            }
            else
            {
                return "Server was not in Freeze State";
            }
        }

        public string Fail()
        {
            isFailed = true;
            return "DataServer " + ServerId + " Failed";
        }

        public string Recover()
        {
            isFailed = false;
            return "DataServer " + ServerId + " Recovered";
        }

        public string Dump()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("|  Name  |  Version  |   Size   |     Content    |");
            foreach (KeyValuePair<string, int> keyValuePair in _localFileNameList)
            {
                TFile file = storage.ReadFile(keyValuePair.Key);
                builder.AppendLine(value: "|" + keyValuePair.Key + "|" + file.VersionNumber + "|" + file.Size + "|" + Encoding.ASCII.GetString(file.Data));
            }
            return builder.ToString();
        }
    }
}


