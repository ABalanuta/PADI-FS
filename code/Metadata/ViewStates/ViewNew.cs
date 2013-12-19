using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metadata.ViewStates;
using Metadata.ViewStatus;
using SharedLib;
using SharedLib.MetadataObjects;

namespace Metadata.ViewStates
    {
    /// <summary>
    /// Its a new server, request to stop the cluster to copy the data. After it will move to Bully
    /// </summary>
    public class ViewNew : ViewState
    {
        private object locker = new Object();
        private int _lastMaster = -1;
        private long _lastId = -1;

        public ViewNew(MetaViewManager manager)
            : base(manager, ServerStatus.New)
        {
           
        }

        public override void Start()
        {
        //Multicast All to STOP. All servers will change to Paused.
        Manager.MulticastMsg( new BullyMsg( BullyType.NewRowdy, Manager.ThisMetaserverId,Manager.GetStatus() ), 0, 2 );

        //ViewStatusChanged will finish and change status
            while (!CheckIfStatusCompleted())
            {
                lock ( locker )
                {
                    Monitor.Wait(locker, 1000);
                }
             }


        if ( Manager.CountOnlineServers( ) == 1 )
            {
            Manager.ToMaster( );
            Console.WriteLine( "Current Master: " + Manager.CurrentMaster );
            return;
            }

        if (_lastMaster == -1 || _lastId == -1)
        {
            Console.WriteLine( "ERROR: Newbie lastMaster: "+_lastMaster+" lastId: "+_lastId);
        }

        //O master ja parou de receber pedidos. Os slave tambem vao acabar os que tem. Vamos pedir para copiar.

        IMetaToMeta server = MetadataServer.ConnectToMetaserver( _lastMaster );
            //Request copy from master
            CopyStructMetadata dataStruct = server.RequestUpdate();
            //TODO Create timeout
            //Update the server
            MetadataServer.UpdateServer(dataStruct);

        //I know all server status, Start bulling protocol:
            Manager.ToBully();
        }

        ////////////////////////////// Msg Received /////////////////////////////////////////////////////
        /// <summary>
        /// Se ha outro rowdy, ele apenas tem de saber que nao pode copiar de nos. Depois vamos a porrada no bully alg
        /// </summary>
        /// <param name="source"></param> 
        public override BullyMsg NewRowdyMsgRequest( int source )
        {
             BullyMsg resp = new BullyMsg( BullyType.StatusMsg, Manager.ThisMetaserverId, Manager.GetStatus( ) );
             resp.Status = Status;
          return resp;
        }

        /// <summary>
        /// Alguem esta a tentar ganhar mas ainda nao estamos estaveis. Vamos para-lo
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override BullyMsg AreYouBiggerMsgRequest(int source)
        {
        return new BullyMsg( BullyType.ShutUp, Manager.ThisMetaserverId, Manager.GetStatus( ) );
        }

        public override BullyMsg ImBossMsgRequest( int source )
            {
            Console.WriteLine( "WARNING: Newbie " + Manager.ThisMetaserverId + " Received: ImBoss Msg from: " + source );
            return new BullyMsg( BullyType.ShutUp, Manager.ThisMetaserverId, Manager.GetStatus( ) );
            }


        public override void ShutUpMsgReply(int source)
        {
        Console.WriteLine( "WARNING: Newbie "+Manager.ThisMetaserverId+ " Received: ShutUp Msg from: "+source );
        }

        public override void ImSmallerMsgReply(int source)
        {
        Console.WriteLine( "WARNING: Newbie " + Manager.ThisMetaserverId + " Received: ShutUp Msg from: " + source );
        }

        public override void AckBossMsgReply(int source)
        {
            Console.WriteLine( "WARNING: Newbie " + Manager.ThisMetaserverId + " Received: Ack Boss Msg from: " + source );
        }


        ////////////////////////////////////// Manager Notifications /////////////////////////////////////////////
        private Boolean CheckIfStatusCompleted()
        {
        //Wait get an consistent view
            if (Manager.CountKnownServerState() != 3)
                    return false;

            lock ( locker )
                {
                Monitor.PulseAll( locker );
                }
            return true;
        }

        public override void ViewStatusChanged(int lastMaster,long lastId)
        {
             if ( _lastMaster != -1 && _lastMaster != lastMaster )
                Console.WriteLine("WARNING: servers have different master opinion: "+_lastMaster+" and "+lastMaster);

            if ( lastMaster != -1 )
            {
                _lastMaster = lastMaster;
                Console.WriteLine("Last Master: "+_lastMaster);
            }

            if (lastId != -1)
            {
                 _lastId = lastId;
                 Console.WriteLine( "Last id: " + _lastId );
            }

        }
     }
 }
