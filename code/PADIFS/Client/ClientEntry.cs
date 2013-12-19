using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using SharedLib;
using SharedLib.DataserverObjects;
using SharedLib.Exceptions;
using SharedLib.MetadataObjects;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using System.Threading;



namespace Client
{


    // Classe que representa uma  Aplicação cliente que contacta os Servidores de Dados e Metadados no entanto oferece uma interface
    // para o puppet master que envoca as operaçoes Remotamente
    public class ClientEntry : MarshalByRefObject, IClientToPuppet, IClientToMeta
    {

        public MetadataEntry[] FileRegister = new MetadataEntry[10];
        public byte[][] DataRegister = new byte[10][];
        public static MetaserverAsyncClient MetaserverClient;

        private readonly String _baseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);


        public static int clientId;

        public ClientEntry()
        {
        }


        static void Main(String[] args)
        {
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();
            //  Debugger.Break();

            int clientPort;
            String clientHostName;
            List<ServerId> metaserverList = new List<ServerId>();

            int i = 0;
            for (i = 0; i < (args.Length - 2); i++)
            {
                ServerId metaServer = new ServerId();
                metaServer.hostname = args[i++];
                metaServer.port = Convert.ToInt32(args[i]);
                metaServer.id = Convert.ToString(i / 2);
                metaserverList.Add(metaServer);
            }
            clientPort = Convert.ToInt32(args[i++]);
            clientId = Convert.ToInt32(args[i]);
            clientHostName = ClientEntry.GetCurrentIp();

            RemotingConfiguration.Configure("../../App.config", true);
            ClientEntry client = new ClientEntry();

            Console.WriteLine("Start client: " + clientId);

            //Create interfce to metaserver
            MetaserverClient = new MetaserverAsyncClient(clientHostName, clientPort, metaserverList, clientId);

            // TCP Channel
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            IDictionary props = new Hashtable();
            props["port"] = MetaserverClient.ClientPort;
            //TODO Descomentar
            //  props["timeout"] = 1000; // in milliseconds
            TcpChannel channel = new TcpChannel(props, null, provider);



            ChannelServices.RegisterChannel(channel, false);
            Console.WriteLine("Starting Client Socket for puppet master at port: " + MetaserverClient.ClientPort);
            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(ClientEntry),
            "PADIConnection",
            WellKnownObjectMode.Singleton);

            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
        }




        public String Dump()
        {
            Console.WriteLine("#DUMP Sending System Dump to PuppetMaster");
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("DUMP of Client \r\n");

            builder.AppendLine("FileRegister \r\n");
            for (int i = 0; i < 10; i++)
            {
                builder.Append("File Number: " + i + "  |  ");

                if (FileRegister[i] != null)
                {
                    builder.Append(FileRegister[i] + "\r\n");
                }
                else
                {
                    builder.Append(" Empty \r\n");
                }
            }


            builder.AppendLine("DataRegister");
            for (int i = 0; i < 10; i++)
            {
                builder.Append("File Number: " + i + "  |  ");

                if (DataRegister[i] == null)
                {
                    builder.Append(" Empty \r\n");
                }
                else
                {
                    builder.Append(Encoding.ASCII.GetString(DataRegister[i]));
                }
            }

            return builder.ToString();
        }



        //////////////////////////////////// CLient Interface //////////////////////////////////
        public String Create(String filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            Console.WriteLine("#CREATE Create file: " + filename + " nbDataServer: " + nbDataServers + " readQuorum: " + readQuorum + " writeQuorum: " + writeQuorum);
            RequestCreate createRequest = new RequestCreate(filename, nbDataServers, readQuorum, writeQuorum);
            MetaserverResponse response = MetaserverClient.SendRequestToMetaserver(createRequest);
            //Processar a resposta
            MetadataEntry metaEntry = response.MetaEntry;

            if (metaEntry != null)
            {
                Console.WriteLine("#CREATE Create Successeful");
                return metaEntry.ToString();
            }
            else
            {
                throw new PadiException(PadiExceptiontType.CreateFile, "Response without entry");
            }
        }



        public String Open(String filename)
        {
            Console.WriteLine("#OPEN  Open: " + filename);
            //            if (GetFileMetadataEntry(filename) != null)
            //              throw new PadiException(PadiExceptiontType.OpenFile, "ClientCore: File already opened: " + filename);

            RequestOpen openRequest = new RequestOpen(filename);
            MetaserverResponse response = MetaserverClient.SendRequestToMetaserver(openRequest);
            MetadataEntry metaEntry = response.MetaEntry;

            if (metaEntry == null)
            {
                throw new PadiException(PadiExceptiontType.OpenFile, "Response without entry");
            }

            if (GetFileMetadataEntry(filename) != null)
            {
                UpdateMetadataEntry(metaEntry);
                Console.WriteLine("#Open: Value Updated");
                return metaEntry.ToString();
            }

            AddFileMetadataEntry(metaEntry);
            Console.WriteLine("#Open: Complete");
            return metaEntry.ToString();
        }


        public String Close(String filename)
        {
            Console.WriteLine("#Close: " + filename);

            MetadataEntry entry = this.GetFileMetadataEntry(filename);
            if (entry == null)
            {
                Console.WriteLine("#CLOSE File was not open on this client " + filename);
                return "#CLOSE File was not open on this client " + filename;
            }

            RequestClose closeRequest = new RequestClose(filename);
            MetaserverResponse response = MetaserverClient.SendRequestToMetaserver(closeRequest);
            if (response.Status != ResponseStatus.Success)
            {
                Console.WriteLine("#CLOSE  Error closing on server " + filename);
                return "#CLOSE  Error closing on server " + filename;
            }
            else
            {
                Console.WriteLine("#CLOSE: Success: " + filename);
                return "#CLOSE: Success: " + filename;
            }

        }

        public String Delete(String filename)
        {
            Console.WriteLine("#Delete: " + filename);
            RequestDelete deleteRequest = new RequestDelete(filename);
            MetaserverResponse response = MetaserverClient.SendRequestToMetaserver(deleteRequest);
            if (response.Status != ResponseStatus.Success)
            {
                Console.WriteLine("#DELETE  Error closing on server " + filename);
                return "#DELETE  Error deleting on server " + filename;
            }
            Console.WriteLine("#DELETE: Success: " + filename);
            return "#DELETE: Success: " + filename;
        }

        //////////////////////////////////////////////// Async GetLast Version
        static object monGetVersionLock = new object();
        private static int targetGetVersionQuorum = 0;
        private static List<String> requestedGetVersionIDs = new List<String>();
        private static List<String> returnedGetVersionIDs = new List<String>();
        private static List<int> returnedGetVersionFiles = new List<int>();

        public delegate VersionFile AsyncGetVersionDel(String localFileName);

        public static void CBDoWhenReturnFromGetVersion(IAsyncResult ar)
        {
            AsyncGetVersionDel del = (AsyncGetVersionDel)((AsyncResult)ar).AsyncDelegate;
            //Read
            VersionFile resp = del.EndInvoke(ar);
            System.Console.WriteLine("#GETVERSION *Data Server " + resp.responseServerId + " Returned from GetVersion");
            returnedGetVersionIDs.Add(resp.responseServerId);
            returnedGetVersionFiles.Add(resp.versionNumber);

            // Se somos o ultimo vamos informar a thread em espera no monitor


            if (returnedGetVersionIDs.Count.Equals(targetGetVersionQuorum))
            {
                System.Console.WriteLine("#GETVERSION *Last Data Server,  Realeasing Main Thread");
                Monitor.Enter(monGetVersionLock);
                Monitor.Pulse(monGetVersionLock);
                Monitor.Exit(monGetVersionLock);
            }
            return;
        }


        private bool GetVersionAux(string serverIP, int serverPort, string localFileName)
        {

            IDataToClient dataServer = (IDataToClient)Activator.GetObject(
                typeof(IDataToClient),
                "tcp://" + serverIP + ":" + serverPort + "/PADIConnection");

            AsyncGetVersionDel RemoteDel = new AsyncGetVersionDel(dataServer.GetVersion);
            AsyncCallback RemoteCallback = new AsyncCallback(CBDoWhenReturnFromGetVersion);

            try
            {
                IAsyncResult RemAr = RemoteDel.BeginInvoke(localFileName, RemoteCallback, null);
            }
            catch (SocketException)
            {
                System.Console.WriteLine("#GETVERSION Could not locate server");
                return false;
            }
            return true;
        }

        public int GetLastVersion(int fileRegister)
        {

            Console.WriteLine("#GETVERSION  Register: " + fileRegister);
            MetadataEntry entry;

            if (FileRegister[fileRegister] != null)
            {
                entry = FileRegister[fileRegister];
            }
            else
            {
                throw new PadiException(PadiExceptiontType.ReadFile, "#GETVERSION Client: File Registry is empty, no metadata");
            }

            targetGetVersionQuorum = entry.WriteQuorum;
            requestedGetVersionIDs = new List<String>();
            returnedGetVersionIDs = new List<String>();
            returnedGetVersionFiles = new List<int>();

            // Exectar ate obter um Quorm de leituras
            // Serviodres podem recuperar temos que passar a lista varias vezes
            while (true)
            {
                foreach (KeyValuePair<ServerId, String> server in entry.ServerFileList)
                {
                    ServerId serverId = server.Key;
                    String localFileName = server.Value;

                    if (requestedGetVersionIDs.Contains(serverId.id))
                    {
                        continue;
                    }

                    Boolean sent = GetVersionAux(serverId.hostname, serverId.port, localFileName);
                    if (sent)
                    {
                        requestedGetVersionIDs.Add(serverId.id);
                    }

                    // Se já enviamos a um quorum de servidores ficamos a espera no monitor
                    if (requestedGetVersionIDs.Count.Equals(entry.WriteQuorum))
                    {
                        System.Console.WriteLine("#GETVERSION Main Thread is Waiting for the Dataservers to Respond");
                        Monitor.Enter(monGetVersionLock);
                        Monitor.Wait(monGetVersionLock);
                        Monitor.Exit(monGetVersionLock);
                        System.Console.WriteLine("#GETVERSION Client Released from Wait");
                    }

                    // Se todos retornaram vamos sair do ciclo
                    if (returnedGetVersionFiles.Count.Equals(entry.WriteQuorum))
                    {
                        break;
                    }
                }

                if (returnedGetVersionFiles.Count.Equals(entry.WriteQuorum))
                {
                    break;
                }
            }

            int version = -1;
            foreach (int t in returnedGetVersionFiles)
            {
                if (t > version)
                {
                    version = t;
                }
            }

            System.Console.WriteLine("#GETVERSION Read Complete Returning Read data at Version: " + version);
            return version;
        }



        //////////////////////////////////////////////// Async READ
        static object monReadLock = new object();
        private static int targetReadQuorum = 0;
        private static List<String> requestedReadIDs = new List<String>();
        private static List<String> returnedReadIDs = new List<String>();
        private static List<TFile> returnedReadFiles = new List<TFile>();

        public delegate TFile AsyncReadDel(String localFileName, SemanticType semantic, int minVersion);


        public static void CBDoWhenReturnFromRead(IAsyncResult ar)
        {
            AsyncReadDel del = (AsyncReadDel)((AsyncResult)ar).AsyncDelegate;
            //Read
            TFile resp = del.EndInvoke(ar);
            System.Console.WriteLine("#READ *Data Server " + resp.responseServerId + " Returned from Read");
            returnedReadIDs.Add(resp.responseServerId);
            returnedReadFiles.Add(resp);

            // Se somos o ultimo vamos informar a thread em espera no monitor
            if (returnedReadIDs.Count.Equals(targetReadQuorum))
            {
                System.Console.WriteLine("#READ *Last Data Server,  Realeasing Main Thread");
                Monitor.Enter(monReadLock);
                Monitor.Pulse(monReadLock);
                Monitor.Exit(monReadLock);
            }
            return;
        }

        private bool ReadAux(string serverIP, int serverPort, string localFileName, SemanticType semantic, int minVersion)
        {

            IDataToClient dataServer = (IDataToClient)Activator.GetObject(
                typeof(IDataToClient),
                "tcp://" + serverIP + ":" + serverPort + "/PADIConnection");

            AsyncReadDel RemoteDel = new AsyncReadDel(dataServer.Read);
            AsyncCallback RemoteCallback = new AsyncCallback(CBDoWhenReturnFromRead);

            try
            {
                IAsyncResult RemAr = RemoteDel.BeginInvoke(localFileName, semantic, minVersion, RemoteCallback, null);
            }
            catch (SocketException)
            {
                System.Console.WriteLine("#READ Could not locate server");
                return false;
            }
            return true;
        }

        public TFile Read(int fileRegister, SemanticType semantic)
        {

            Console.WriteLine("#READ Read register: " + fileRegister + " Semantic: " + semantic);
            MetadataEntry entry;

            if (FileRegister[fileRegister] != null)
            {
                entry = FileRegister[fileRegister];
            }
            else
            {
                throw new PadiException(PadiExceptiontType.ReadFile, "#READ Client: File Registry is empty, no metadata");
            }

            if (entry.ServerFileList.Values.Count < entry.ReadQuorum)
            {
                // Update the Registry
                Open(entry.FileName);
                entry = FileRegister[fileRegister];

                if (entry.ServerFileList.Values.Count < entry.ReadQuorum)
                {
                    throw new PadiException(PadiExceptiontType.OpenFile, "#READ Client: Cannot read file: Servers available are not enought");
                }
            }

            targetReadQuorum = entry.ReadQuorum;
            requestedReadIDs = new List<String>();
            returnedReadIDs = new List<String>();
            returnedReadFiles = new List<TFile>();

            // Exectar ate obter um Quorm de leituras
            // Serviodres podem recuperar temos que passar a lista varias vezes
            while (true)
            {
                foreach (KeyValuePair<ServerId, String> server in entry.ServerFileList)
                {
                    ServerId serverId = server.Key;
                    String localFileName = server.Value;

                    if (requestedReadIDs.Contains(serverId.id))
                    {
                        continue;
                    }

                    Boolean sent = ReadAux(serverId.hostname, serverId.port, localFileName, semantic, entry.monotonicReadVersion);
                    if (sent)
                    {
                        requestedReadIDs.Add(serverId.id);
                    }

                    // Se já enviamos a um quorum de servidores ficamos a espera no monitor
                    if (requestedReadIDs.Count.Equals(entry.ReadQuorum))
                    {
                        System.Console.WriteLine("#READ Main Thread is Waiting for the Dataservers to Respond");
                        Monitor.Enter(monReadLock);
                        Monitor.Wait(monReadLock);
                        Monitor.Exit(monReadLock);
                        System.Console.WriteLine("#READ Client Released from Wait");
                    }

                    // Se todos retornaram vamos sair do ciclo
                    if (returnedReadIDs.Count.Equals(entry.ReadQuorum))
                    {
                        break;
                    }
                }

                if (returnedReadIDs.Count.Equals(entry.ReadQuorum))
                {
                    break;
                }
            }

            int version = -1;
            TFile file = null;
            foreach (TFile t in returnedReadFiles)
            {

                if (t.VersionNumber > version)
                {
                    version = t.VersionNumber;
                    file = t;
                }
            }

            // Actualizar valor util para leituras monotonicas
            FileRegister[fileRegister].monotonicReadVersion = version;
            System.Console.WriteLine("#READ Read Complete Returning Read data at Version: " + version);

            return file;
        }

        public String Read(int fileRegister, SemanticType semantic, int dataRegister)
        {
            TFile file = Read(fileRegister, semantic);
            DataRegister[dataRegister] = file.Data;
            Console.WriteLine("#READ Read Action Saved At DataRegister " + dataRegister);

            return "#READ Result" + Encoding.ASCII.GetString(file.Data) + "froam Server " + file.responseServerId + " at Version " + file.VersionNumber;
        }


        //////////////////////////////////////////////// Async WRITE
        static object monWriteLock = new object();
        private static int targetWriteQuorum = 0;
        private static List<String> requestedWriteIDs = new List<String>();
        private static List<String> returnedWriteIDs = new List<String>();

        public delegate String AsyncWriteDel(String localFileName, byte[] data, int version);

        // This is the call that the AsyncCallBack delegate will reference.
        public static void CBDoWhenReturnFromWrite(IAsyncResult ar)
        {
            AsyncWriteDel del = (AsyncWriteDel)((AsyncResult)ar).AsyncDelegate;

            String id = del.EndInvoke(ar);
            System.Console.WriteLine("#WRITE *Data Server " + id + " Returned from Write");
            returnedWriteIDs.Add(id);

            // Se somos o ultimo vamos informat a thread em espera no monitor
            if (returnedWriteIDs.Count.Equals(targetWriteQuorum))
            {
                System.Console.WriteLine("#WRITE *Last Data Server Realeasing Main Thread");
                Monitor.Enter(monWriteLock);
                Monitor.Pulse(monWriteLock);
                Monitor.Exit(monWriteLock);
            }

            return;
        }

        // Metodo que faz a invocação assincrona, devolve false se não encontrar o servidor
        private Boolean WriteAux(String serverIP, int serverPort, String localFileName, byte[] data, int version)
        {

            IDataToClient dataServer = (IDataToClient)Activator.GetObject(
                typeof(IDataToClient),
                "tcp://" + serverIP + ":" + serverPort + "/PADIConnection");

            AsyncWriteDel RemoteDel = new AsyncWriteDel(dataServer.Write);
            AsyncCallback RemoteCallback = new AsyncCallback(ClientEntry.CBDoWhenReturnFromWrite);

            try
            {
                IAsyncResult RemAr = RemoteDel.BeginInvoke(localFileName, data, version, RemoteCallback, null);
            }
            catch (SocketException)
            {
                System.Console.WriteLine("#WRITE Could not locate server");
                return false;
            }
            return true;
        }

        public String Write(int fileRegister, int byteArrayRegister)
        {
            Console.WriteLine("#WRITE Write: fileRegister" + fileRegister + " byteArrayRegister: " + byteArrayRegister);
            //TODO tratar excepcao de array out of bounds
            byte[] data = DataRegister[byteArrayRegister];

            return Write(fileRegister, data);
        }

        public String Write(int fileRegister, byte[] data)
        {
            String txt = Encoding.ASCII.GetString(data);
            Console.WriteLine("#WRITE Write: fileRegister: " + fileRegister + ",  Data: " + txt);
            MetadataEntry entry;

            if (FileRegister[fileRegister] != null)
            {
                entry = FileRegister[fileRegister];
            }
            else
            {
                throw new PadiException(PadiExceptiontType.WriteFile, "#WRITE Client: Cannot write file: File registry not exists");
            }

            // Se não conseguiremos satisfazer o Qorum vamos tentar actualizar a entrada
            if (entry.ServerFileList.Values.Count < entry.WriteQuorum)
            {
                // Update the Registry
                Open(entry.FileName);
                entry = FileRegister[fileRegister];

                if (entry.ServerFileList.Values.Count < entry.WriteQuorum)
                {
                    throw new PadiException(PadiExceptiontType.OpenFile, "#WRITE Client: Cannot write file: Servers available are not enought");
                }
            }

            targetWriteQuorum = entry.WriteQuorum;
            requestedWriteIDs = new List<String>();
            returnedWriteIDs = new List<String>();

            // Buscar o ultimo valor de Leitura se existir
            int lastReadVersion = GetLastVersion(fileRegister);


            // Exectar ate obter um Quorm de escritas
            // Serviodres podem recuperar temos que passar a lista varias vezes
            while (true)
            {
                foreach (KeyValuePair<ServerId, String> server in entry.ServerFileList)
                {
                    ServerId serverId = server.Key;
                    String localFileName = server.Value;

                    if (requestedWriteIDs.Contains(serverId.id))
                    {
                        continue;
                    }

                    Boolean sent = WriteAux(serverId.hostname, serverId.port, localFileName, data, lastReadVersion + 1);

                    if (sent)
                    {
                        requestedWriteIDs.Add(serverId.id);
                    }

                    // Se já enviamos a um quorum de servidores ficamos a espera no monitor
                    if (requestedWriteIDs.Count.Equals(entry.WriteQuorum))
                    {
                        System.Console.WriteLine("#WRITE Main Thread is Waiting for the Dataservers to respond");
                        Monitor.Enter(monWriteLock);
                        Monitor.Wait(monWriteLock);
                        Monitor.Exit(monWriteLock);
                        System.Console.WriteLine("#WRITE Client Released from Wait");
                    }

                    // Se todos retornaram vamos sair do ciclo
                    if (returnedWriteIDs.Count.Equals(entry.WriteQuorum))
                    {
                        break;
                    }
                }

                if (returnedWriteIDs.Count.Equals(entry.WriteQuorum))
                {
                    break;
                }
            }
            System.Console.WriteLine("#WRITE Write Complete Returning");
            return "#WRITE Client: fileRegister " + fileRegister + " with success";
        }

        public String Copy(int fileRegisterRead, SemanticType semantic, int fileRegisterWrite, byte[] stringSalt)
        {
            Console.WriteLine("#COPY From File at Register: " + fileRegisterRead + " with " + semantic.ToString() + " Semantic  and Writes to File at Register: " +
                fileRegisterWrite + " adding the salt: " + (new ASCIIEncoding()).GetString(stringSalt));

            TFile readFile = Read(fileRegisterRead, semantic);
            String parte1 = Encoding.ASCII.GetString(readFile.Data);
            String parte2 = Encoding.ASCII.GetString(stringSalt);
            String data = parte1 + parte2;
            Write(fileRegisterWrite, (new ASCIIEncoding()).GetBytes(data));

            return "#COPY Complete";
        }











        //  The client may ignore the PROCESS parameter in all commands.
        public String ExeScript(Queue<String> commandList)
        {
            Console.WriteLine("#EXESCRIPT ExeScript: ");
            ASCIIEncoding ascii = new ASCIIEncoding();
            String command;
            while ((command = commandList.Dequeue()) != null)
            {
                String[] words = command.Split(' ', ',');
                switch (words[0])
                {
                    case "CREATE":
                        Console.Write("####EXESCRIPT CREATE");
                        Console.WriteLine(Create(words[3], Convert.ToInt32(words[5]), Convert.ToInt32(words[7]), Convert.ToInt32(words[9])));
                        break;
                    case "OPEN":
                        Console.Write("####EXESCRIPT OPEN");
                        Console.WriteLine(Open(words[3]));
                        break;
                    case "CLOSE":
                        Console.Write("####EXESCRIPT CLOSE");
                        Console.WriteLine(Close(words[3]));
                        break;
                    case "READ":
                        Console.Write("####EXESCRIPT READ");
                        SemanticType type = (words[5].Equals("monotonic")) ? SemanticType.Monotonic : SemanticType.Default;
                        Console.WriteLine(Read(Convert.ToInt32(words[3]), type, Convert.ToInt32(words[7])));
                        break;
                    case "WRITE":
                        Console.Write("####EXESCRIPT WRITE");
                        if (command.Split('"').Length > 1)
                        {
                            Console.WriteLine(Write(Convert.ToInt32(words[3]), ascii.GetBytes(command.Split('"')[1])));
                        }
                        else
                        {
                            Console.WriteLine(Write(Convert.ToInt32(words[3]), Convert.ToInt32(words[5])));
                        }
                        break;
                    case "COPY":
                        Console.Write("####EXESCRIPT COPY");
                        SemanticType type2 = (words[5].Equals("monotonic")) ? SemanticType.Monotonic : SemanticType.Default;
                        Console.WriteLine(Copy(Convert.ToInt32(words[3]), type2, Convert.ToInt32(words[7]), ascii.GetBytes(command.Split('"')[1])));
                        break;
                    case "DUMP":
                        Console.Write("####EXESCRIPT DUMP");
                        Console.WriteLine(Dump());
                        break;
                }
            }
            return "ExecScript Finish";
        }


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
            throw new Exception("Client  cannot get LocalIP");
        }



        // Manipulação dos Registos 

        private void AddFileMetadataEntry(MetadataEntry metaEntry)
        {
            for (int x = 0; x < 10; x++)
            {
                if (FileRegister[x] == null)
                {
                    FileRegister[x] = metaEntry;
                    return;
                }
            }
            throw new PadiException(PadiExceptiontType.Registry, "FileRegisters Are Full, cant open more than 10 Files");

        }


        private MetadataEntry GetFileMetadataEntry(String filename)
        {
            foreach (MetadataEntry entry in FileRegister)
            {

                if (entry != null && entry.FileName.Equals(filename))
                {
                    return entry;
                }

            }
            return null;
        }

        private void UpdateMetadataEntry(MetadataEntry meta)
        {
            for (int x = 0; x < 10; x++)
            {
                if (FileRegister[x] != null && FileRegister[x].FileName.Equals(meta.FileName))
                {
                    FileRegister[x] = meta;
                    return;
                }
            }
        }


        // Remove a referencia para o Registo de metadata
        private Boolean RemoveFileMetadataEntry(String filename)
        {
            for (int x = 0; x < 10; x++)
            {
                if (FileRegister[x] != null && FileRegister[x].FileName.Equals(filename))
                {
                    FileRegister[x] = null;
                    return true;
                }
            }
            return false;
        }


        public override object InitializeLifetimeService()
        {
            return null;

        }

        public bool ReceiveMetaserverResponse(MetaserverResponse response)
        {
            return MetaserverClient.ReceiveMetaserverResponse(response);
        }
    }
}