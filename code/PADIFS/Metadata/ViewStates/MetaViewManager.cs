using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using Metadata.ViewStates;
using Metadata.ViewStatus;
using SharedLib;
using SharedLib.MetadataObjects;

namespace Metadata.ViewStates
    {
    public class MetaViewManager
        {
        private ServerStatus[] ViewElements = { ServerStatus.Unknown, ServerStatus.Unknown, ServerStatus.Unknown };
        public int CurrentMaster = -1;
        public int ThisMetaserverId;
        public Dictionary<int, MetaserverId> MetadataServerList;
        public static Mutex ViewElementsMutex = new Mutex();
        public ViewState ServerViewState;



        public MetaViewManager( int serverId, Dictionary<int, MetaserverId> metaserverList )
        {
            ThisMetaserverId = serverId;
            MetadataServerList = metaserverList;
            ServerViewState = new ViewPause( this );
            ServerViewState.Start( );
            }



        ///////////////////////////////////////////////// Input //////////////////////////////////////////////
        /// <summary>
        /// Remote Requests: Aqui recebo os pedidos remotos
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public BullyMsg BullyRequestsRetrival( BullyMsg msg )
            {
            switch ( msg.Type )
                {
                case BullyType.NewRowdy:
                    Console.WriteLine("Retrieve: New Rowdy from: "+msg.Source);
                    return ServerViewState.NewRowdyMsgRequest( msg.Source );
                case BullyType.AreYouBigger:
                    Console.WriteLine( "Retrieve: Are you bigger from: " + msg.Source );
                    return ServerViewState.AreYouBiggerMsgRequest( msg.Source );
                 case BullyType.ImBoss:
                    UpdateViewServerState( msg.Status, msg.Source,msg.LastMaster,msg.LastRequestId);
                    Console.WriteLine( "Retrieve: Im Boss from: " + msg.Source );
                    return ServerViewState.ImBossMsgRequest( msg.Source );
                default:
                    Console.WriteLine("Invalide Request type:"+msg.Type+" from "+msg.Source);
                    throw new Exception( "Invalid Request msg: "+msg.Type );
                }
            }

        /////////////////////////////////////////// STATE SET //////////////////////////////////////////////
        public void ToPause()
        {
            //TODO Parar todos os processos pendentes
            ChangeState( new ViewPause( this ), ServerStatus.Paused );
        }

        public void ToMaster()
        {
            CurrentMaster = ThisMetaserverId;
            ChangeState(new ViewOnline( this,true ),ServerStatus.Master);
            Console.WriteLine( "Change to Master" );
            Console.WriteLine( ToString( ) );
        }

        public void ToSlave(int newMaster)
        {
            CurrentMaster = newMaster;
            ChangeState( new ViewOnline( this, false ), ServerStatus.Slave );
            Console.WriteLine( "Change to Slave" );
            Console.WriteLine( ToString( ) );
        }

        public void ToBully()
        {
            Console.WriteLine("Change To Bully Mode");
            ChangeState( new ViewBully( this ), ServerStatus.Bully );
        }
        public void ToNew()
        {
            Console.WriteLine("Change to New mode");
        ChangeState( new ViewNew( this ), ServerStatus.New );
        }



        ///////////////////////////////////////////////// OUTPUT //////////////////////////////////////////////
        private delegate void ProcessResponseDel(BullyMsg msg);

        private delegate BullyMsg BullyDel( BullyMsg msg );

        /// <summary>
        /// Aqui recebo as respostas
        /// </summary>
        /// <param name="ar"></param>
        private void BullyResponse(IAsyncResult ar)
        {
            BullyDel del = (BullyDel) ((AsyncResult) ar).AsyncDelegate;
            BullyMsg msg = del.EndInvoke(ar);

            if (!MetadataServerList.ContainsKey(msg.Source))
                Console.WriteLine("Ping Server: got invalid server Id: " + msg.Source);

            ProcessResponseDel responseDelegater = new ProcessResponseDel(ProcessResponse);
            responseDelegater.Invoke(msg);
        }


        private void ProcessResponse(BullyMsg msg)
        {
        switch ( msg.Type )
            {
            case BullyType.StatusMsg:
                Console.WriteLine( "Response: StatusMsg from: " + msg.Source );
                UpdateViewServerState( msg.Status, msg.Source ,msg.LastMaster, msg.LastRequestId);
                break;
            case BullyType.ShutUp:
                Console.WriteLine( "Response: Shutup: " + msg.Source );
                ServerViewState.ShutUpMsgReply( msg.Source );
                break;
            case BullyType.ImSmaller:
                Console.WriteLine( "Response: Smaller: " + msg.Source );
                ServerViewState.ImSmallerMsgReply( msg.Source );
                break;
            case BullyType.AckBoss:
                Console.WriteLine( "Response: Ackboss: " + msg.Source );
                //If it agree with boss, so it is slave
                UpdateViewServerState( msg.Status, msg.Source,msg.LastMaster,msg.LastRequestId );
                ServerViewState.AckBossMsgReply( msg.Source );
                break;
            default:
                Console.WriteLine( "Invalide Response type:" + msg.Type + " from " + msg.Source );
                throw new Exception( "Invalid msg type received:" + msg.Type );
            }
        }

   

        ////////////////////////////////////////////////////////////////////////////////////////
         /// <summary>
         /// Change Current Status to Update to a new View. Start the state chain
         /// </summary>
         public void UpdateView( )
         {
                 ResetView( );
                 ToNew();                 
             }


            ///////////////////////////////////// Auxiliares /////////////////////////////////////////////        
        private void ResetView()
        {
             ViewElementsMutex.WaitOne( );
            {
                for (int i = 0; i < ViewElements.Length; i++)
                {
                    ViewElements[i] = ServerStatus.Unknown;
                }
            }
            ViewElementsMutex.ReleaseMutex( );
        }

        public void MulticastMsg( BullyMsg msg, int minServerId, int maxServerId )
            {
            //Contact other servers
            for ( int id = minServerId; id <= maxServerId; id++ )
                {
                if ( id == ThisMetaserverId )
                    continue;

                IMetaToMeta server = MetadataServer.ConnectToMetaserver( id );
                BullyDel invokeDel = new BullyDel( server.BullyRequestsRetrival );
                AsyncCallback callback = new AsyncCallback( BullyResponse );
                msg.Destination = id;
                try
                    {
                    invokeDel.BeginInvoke( msg, callback, null );
                    }
                catch ( SocketException )
                    {
                    UpdateViewServerState( ServerStatus.Off, id,-1,-1 );
                    }
                }
            }

        /// <summary>
        /// Count servers where status is not unknown
        /// </summary>
        /// <returns></returns>
        public int CountKnownServerState( )
            {
            int knownstatus = 0;
            ViewElementsMutex.WaitOne( );
            foreach ( ServerStatus status in ViewElements )
                {
                if ( status != ServerStatus.Unknown )
                    knownstatus++;
                }
            ViewElementsMutex.ReleaseMutex( );
            return knownstatus;
        }

        public void UpdateViewServerState(ServerStatus status, int serverId, int lastMaster, long lastRequestId)
            {
            ViewElementsMutex.WaitOne( );
                ViewElements[serverId] = status;
           ViewElementsMutex.ReleaseMutex( );

                //Notify
                ServerViewState.ViewStatusChanged(lastMaster,lastRequestId);
            }

        public int CountOnlineServers( )
            {
            int online = 0;
            ViewElementsMutex.WaitOne( );
            foreach ( ServerStatus status in ViewElements )
                {
                if ( status != ServerStatus.Off && status != ServerStatus.Unknown )
                    online++;
                }
            ViewElementsMutex.ReleaseMutex( );
            return online;
            }

        public int CountBiggerServerOnline( )
            {
            int online = 0;
            ViewElementsMutex.WaitOne( );
            for ( int i = ThisMetaserverId + 1; i < ViewElements.Length; i++ )
                {
                ServerStatus status = ViewElements[i];
                if ( status != ServerStatus.Off && status != ServerStatus.Unknown )
                    online++;
                }
             ViewElementsMutex.ReleaseMutex( );
            return online;
           }

        public List<int> GetSlaveList()
        {
                 ViewElementsMutex.WaitOne( );
                List<int> slaveServers = new List<int>();
                for (int i = 0; i < ViewElements.Length; i++)
                {
                    if (ViewElements[i] == ServerStatus.Slave)
                    {
                        slaveServers.Add(i);
                    }
                }
                        ViewElementsMutex.ReleaseMutex( );
                return slaveServers;
        }

        private void ChangeState(ViewState state,ServerStatus status)
        {
            ViewElementsMutex.WaitOne( );
                 ViewElements[ThisMetaserverId] = status;
           ViewElementsMutex.ReleaseMutex( );

            lock (ServerViewState)
            {
                ServerViewState = state;
            }
            ServerViewState.Start();
        }

        public override String ToString( )
            {
            StringBuilder str = new StringBuilder( );
            str.AppendLine( "Current View:" );
            ViewElementsMutex.WaitOne( );
                for ( int i = 0; i < ViewElements.Length; i++ )
                    {
                    str.AppendLine( "Server: " + i + " is " + ViewElements[i] );
                    }
                str.AppendLine( "Current master: " + CurrentMaster );
           ViewElementsMutex.ReleaseMutex( );
                return str.ToString( );
                }

        public ServerStatus GetStatus()
        {
             ViewElementsMutex.WaitOne( );
             ServerStatus status = ViewElements[ThisMetaserverId];
            ViewElementsMutex.ReleaseMutex( );
            return status;
        }

        public int GetMaster()
        {
            return CurrentMaster;
        }
        }

    }
