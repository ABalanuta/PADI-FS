using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using System.Text;
using PuppetMaster.Exceptions;
using SharedLib;
using SharedLib.Exceptions;


namespace PuppetMaster.Proxies
{
    public class DataProxy
    {
        private PuppetMasterCore _core;

        public DataProxy(PuppetMasterCore core)
        {
            _core = core;
            RemotingConfiguration.Configure("../../App.config", true);
            
        }
         

        public IDataToPuppet ConnectToDataServer(int dataserverNumber)
        {
            String hostname;
            int port;
            if (!_core.AppManager.GetProcessIPandPort(ProcessType.DataServer, dataserverNumber, false, out hostname, out port))
                throw new CommandException("Can not get dataserver IP and port. Dataserver: " + dataserverNumber);

            IDataToPuppet server = null;
            try
            {
                server = (IDataToPuppet)Activator.GetObject(
                    typeof(IDataToPuppet), "tcp://" + hostname + ":" + port + "/PADIConnection");
                //_core.DisplayMessage("Connected to Dataserver: " + dataserverNumber);
                return server;
            }
            catch (SocketException e)
            {
                throw new CommandException("Error connecting to Dataserver" + e.Message);
            }
            catch (RemotingException e)
            {
                throw new CommandException("Error connecting to Dataserver" + e.Message);
            }
        }


        public void NewCommand(string fullCommand)
        {
            String[] words = fullCommand.Split(' ');
            String res = words[1];
            String[] res2 = res.Split('-');
            IDataToPuppet server;
            int serverNumber;
            String toPuppet;
            try
            {
                serverNumber = Convert.ToInt32(res2[1]);
            }
            catch (Exception e)
            {
                throw new CommandException("Data Proxy: Invalid server number" + fullCommand + " " + e.Message);
            }
            server = ConnectToDataServer(serverNumber);
            String command = words[0];
            try
            {
                switch (command)
                {
                   case "FAIL":
                        toPuppet = server.Fail();
                       break;
                    case "RECOVER":
                        toPuppet = server.Recover();
                        break;
                    case "FREEZE":
                        toPuppet = server.Freeze();
                        break;
                    case "UNFREEZE":
                        toPuppet = server.UnFreeze();
                        break;
                    case "DUMP":
                        toPuppet = server.Dump();
                        break;
                    default:
                        throw new CommandException("Data Proxy: Invalid Command: " + fullCommand);
                }
            }
            catch (SocketException e)
            {
                throw new CommandException("Data Proxy: SocketException: " + fullCommand + " " + e.Message);
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

        /*
        /////////////////////////////////// FAIL ////////////////////////////////////////////
        public delegate Boolean DelFail();
        public void CBFail(IAsyncResult ar)
        {
            DelFail del = (DelFail)((AsyncResult)ar).AsyncDelegate;
            Boolean result = del.EndInvoke(ar);
            _core.DisplayMessage(result.ToString());
        }
        private void Fail(IDataToPuppet server, String[] words, String fullCommand)
        {
            if (words.Length < 2)
            {
                throw new CommandException("DataProxy: Fail: Invalid number of arguments " + fullCommand);
            }
            try
            {
                DelFail remoteDelFail = new DelFail(server.Fail);
                AsyncCallback remoteCallBack = new AsyncCallback(CBFail);
                remoteDelFail.BeginInvoke(remoteCallBack, null);
            }
            catch (InvalidCastException ex)
            {
                throw new CommandException("DataProxy: Fail: Invalid arguments " + fullCommand + "Error: " + ex.Message);
            }
        }

        /////////////////////////////////// RECOVER ////////////////////////////////////////////
        public delegate Boolean DelRecover();
        public void CBRecover(IAsyncResult ar)
        {
            DelRecover del = (DelRecover)((AsyncResult)ar).AsyncDelegate;
            Boolean result = del.EndInvoke(ar);
            _core.DisplayMessage(result.ToString());
        }
        private void Recover(IDataToPuppet server, String[] words, String fullCommand)
        {
            if (words.Length < 2)
            {
                throw new CommandException("DataProxy: Recover: Invalid number of arguments " + fullCommand);
            }
            try
            {
                DelRecover remoteDelRecover = new DelRecover(server.Recover);
                AsyncCallback remoteCallBack = new AsyncCallback(CBRecover);
                remoteDelRecover.BeginInvoke(remoteCallBack, null);
            }
            catch (InvalidCastException ex)
            {
                throw new CommandException("DataProxy: Recover: Invalid arguments " + fullCommand + "Error: " + ex.Message);
            }
        }

        /////////////////////////////////// FREEZE ////////////////////////////////////////////
        public delegate Boolean DelFreeze();
        public void CBFreeze(IAsyncResult ar)
        {
            DelFreeze del = (DelFreeze)((AsyncResult)ar).AsyncDelegate;
            Boolean result = del.EndInvoke(ar);
            _core.DisplayMessage(result.ToString());
        }
        private void Freeze(IDataToPuppet server, String[] words, String fullCommand)
        {
            if (words.Length < 2)
            {
                throw new CommandException("DataProxy: Freeze: Invalid number of arguments " + fullCommand);
            }
            try
            {
                DelFreeze remoteDelFreeze = new DelFreeze(server.Freeze);
                AsyncCallback remoteCallBack = new AsyncCallback(CBFreeze);
                remoteDelFreeze.BeginInvoke(remoteCallBack, null);
            }
            catch (InvalidCastException ex)
            {
                throw new CommandException("DataProxy: Freeze: Invalid arguments " + fullCommand + "Error: " + ex.Message);
            }
        }

        /////////////////////////////////// UNFREEZE ////////////////////////////////////////////
        public delegate Boolean DelUnFreeze();
        public void CBUnFreeze(IAsyncResult ar)
        {
            DelUnFreeze del = (DelUnFreeze)((AsyncResult)ar).AsyncDelegate;
            Boolean result = del.EndInvoke(ar);
            _core.DisplayMessage(result.ToString());
        }
        private void UnFreeze(IDataToPuppet server, String[] words, String fullCommand)
        {
            if (words.Length < 2)
            {
                throw new CommandException("DataProxy: UnFreeze: Invalid number of arguments " + fullCommand);
            }
            try
            {
                DelUnFreeze remoteDelUnfreeze = new DelUnFreeze(server.UnFreeze);
                AsyncCallback remoteCallBack = new AsyncCallback(CBUnFreeze);
                remoteDelUnfreeze.BeginInvoke(remoteCallBack, null);
            }
            catch (InvalidCastException ex)
            {
                throw new CommandException("DataProxy: UnFreeze: Invalid arguments " + fullCommand + "Error: " + ex.Message);
            }
        }
        */
    }
}







