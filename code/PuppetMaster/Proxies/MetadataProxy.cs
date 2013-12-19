using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using PuppetMaster.Exceptions;
using SharedLib;
using SharedLib.Exceptions;


namespace PuppetMaster.Proxies
{
    public class MetadataProxy
    {
        private PuppetMasterCore _core;

        public MetadataProxy(PuppetMasterCore core)
        {
            _core = core;
            RemotingConfiguration.Configure("../../App.config", true);
          
        }

        public IRecovery ConnectToRecovery(int metaServerNumber)
        {
             String hostname;
            int port;
            if ( !_core.AppManager.GetProcessIPandPort( ProcessType.MetaServer, metaServerNumber, true, out hostname, out port ) )
            throw new CommandException( "Can not get metaServer IP and port. Metaserver: " + metaServerNumber );

            IRecovery server =
                (IRecovery) Activator.GetObject( typeof( IRecovery ), "tcp://" + hostname + ":" + port + "/PADIConnection" );
            return server;
        }

        public IMetaToPuppet ConnectToMetaDataServer(int metaServerNumber,Boolean recovery = false)
        {
            String hostname;
            int port;
            if (!_core.AppManager.GetProcessIPandPort(ProcessType.MetaServer, metaServerNumber,false, out hostname, out port))
                throw new CommandException("Can not get metaServer IP and port. Metaserver: " + metaServerNumber);

            IMetaToPuppet server = null;
            try
            {
                server = (IMetaToPuppet)Activator.GetObject(
                    typeof(IMetaToPuppet), "tcp://" + hostname + ":" + port + "/PADIConnection");
                return server;
            }
            catch (Exception e)
            {
                throw new CommandException("Error connecting to metaserver" + e.Message);
            }
        }


        public void NewCommand(string fullCommand)
        {
            String[] words = fullCommand.Split(' ');
            String res = words[1];
            String[] res2 = res.Split('-');
            IMetaToPuppet server;
            IRecovery recover;
            int serverNumber;
            String toPuppet;
            try
            {
                serverNumber = Convert.ToInt32(res2[1]);
            }
            catch (Exception e)
            {
                throw new CommandException("MetaData Proxy: Invalid server number" + fullCommand + " " + e.Message);
            }
            server = ConnectToMetaDataServer(serverNumber);
            recover = ConnectToRecovery(serverNumber);

            String command = words[0];
            try
            {
                switch (command)
                {
                    case "DUMP":
                        toPuppet = server.Dump();
                        break;
                    case "FAIL":
                        toPuppet = server.Fail();
                        break;
                    case "RECOVER":
                        toPuppet = recover.Recover();
                        break;

                    default:
                        throw new CommandException("MetaData Proxy: Invalid Command: " + fullCommand);
                }
            }
            catch (SocketException e)
            {
                throw new CommandException("MetaData Proxy: SocketException: " + fullCommand + " " + e.Message);
            }
            catch (PadiException exN)
            {
                _core.DisplayMessage(exN.Description);
                return;
            }
            catch (Exception ex)
            {
                _core.DisplayMessage(ex.Message);
                return;
            }
            // Return the String receved from the server to be Displayed
            _core.DisplayMessage(toPuppet);

        }

        //'     ____   _      _                                             _____            _       
        //'    / __ \ | |    | |     /\                                    / ____|          | |      
        //'   | |  | || |  __| |    /  \    ___  ___  _   _  _ __    ___  | |      ___    __| |  ___ 
        //'   | |  | || | / _` |   / /\ \  / __|/ __|| | | || '_ \  / __| | |     / _ \  / _` | / _ \
        //'   | |__| || || (_| |  / ____ \ \__ \\__ \| |_| || | | || (__  | |____| (_) || (_| ||  __/
        //'    \____/ |_| \__,_| /_/    \_\|___/|___/ \__, ||_| |_| \___|  \_____|\___/  \__,_| \___|
        //'                                            __/ |                                         
        //'                                           |___/                                          


        ///////////////////////////////////// FAIL ////////////////////////////////////////////
        //public delegate Boolean DelFail();
        //public void CBFail(IAsyncResult ar)
        //{
        //    DelFail del = (DelFail)((AsyncResult)ar).AsyncDelegate;
        //    Boolean result = del.EndInvoke(ar);
        //    _core.DisplayMessage(result.ToString());
        //}
        //private void Fail(IMetaToPuppet server, String[] words, String fullCommand)
        //{
        //    if (words.Length < 2)
        //    {
        //        throw new CommandException("MetaDataProxy: Fail: Invalid number of arguments " + fullCommand);
        //    }
        //    try
        //    {
        //        DelFail remoteDelFail = new DelFail(server.Fail);
        //        AsyncCallback remoteCallBack = new AsyncCallback(CBFail);
        //        remoteDelFail.BeginInvoke(remoteCallBack, null);
        //    }
        //    catch (InvalidCastException ex)
        //    {
        //        throw new CommandException("MetaDataProxy: Fail: Invalid arguments " + fullCommand + "Error: " + ex.Message);
        //    }
        //}

        ///////////////////////////////////// RECOVER ////////////////////////////////////////////
        //public delegate Boolean DelRecover();
        //public void CBRecover(IAsyncResult ar)
        //{
        //    DelRecover del = (DelRecover)((AsyncResult)ar).AsyncDelegate;
        //    Boolean result = del.EndInvoke(ar);
        //    _core.DisplayMessage(result.ToString());
        //}
        //private void Recover(IMetaToPuppet server, String[] words, String fullCommand)
        //{
        //    if (words.Length < 2)
        //    {
        //        throw new CommandException("MetaDataProxy: Recover: Invalid number of arguments " + fullCommand);
        //    }
        //    try
        //    {
        //        DelRecover remoteDelRecover = new DelRecover(server.Recover);
        //        AsyncCallback remoteCallBack = new AsyncCallback(CBRecover);
        //        remoteDelRecover.BeginInvoke(remoteCallBack, null);
        //    }
        //    catch (InvalidCastException ex)
        //    {
        //        throw new CommandException("MetaDataProxy: Recover: Invalid arguments " + fullCommand + "Error: " + ex.Message);
        //    }
        //}

    }
}
