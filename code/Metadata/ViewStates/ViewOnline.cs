using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metadata.ViewStatus;
using SharedLib.MetadataObjects;

namespace Metadata.ViewStates
{
    /// <summary>
    /// The server is running as master or slave
    /// </summary>
    public class ViewOnline : ViewState
    {
        public ViewOnline(MetaViewManager manager, Boolean master) : base(manager, ServerStatus.Slave)
    {
            if(master)
                Status = ServerStatus.Master;
    }

        public override void Start()
        {
            
        }


        public override void ViewStatusChanged( int lastMaster, long lastId )
        {
            //TODO Fazer
            return;
        }

        /// <summary>
        /// Se estamos online e ha algum novo, vamos parar os processos 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override BullyMsg NewRowdyMsgRequest( int source )
        {
            //If master, stop receiving requests and send last ID to newbie
            long lastId = -1;
            if (MetadataServer.IsMaster())
            {
                  lastId = MetadataServer.LockSeriesRequestsId( );
            }

            BullyMsg resp = new BullyMsg( BullyType.StatusMsg, Manager.ThisMetaserverId, Manager.GetStatus( ) );
            resp.Status = ServerStatus.Paused;
            Console.WriteLine("Get master:"+Manager.GetMaster());
            resp.LastMaster = Manager.GetMaster();
            resp.LastRequestId = lastId;

            Manager.ToPause( );
            return resp;
            }

        public override BullyMsg AreYouBiggerMsgRequest(int source)
        {
            Console.WriteLine("Strange: Online server recived Bully Are you bigger");
            //Parar o servidor
            Manager.ToPause( );
            //Paramos os pedidos, se formos maiores, passamos a bully
            BullyMsg resp;
            if (source < Manager.ThisMetaserverId)
            {
            resp = new BullyMsg( BullyType.ShutUp, Manager.ThisMetaserverId, Manager.GetStatus( ) );
                Manager.ToBully();
            }
            else
            {
            resp = new BullyMsg( BullyType.ImSmaller, Manager.ThisMetaserverId, Manager.GetStatus( ) );
            }
            return resp;
        }

        public override void ShutUpMsgReply(int source)
        {
             Console.WriteLine( "Strange: Online server received: ShutUpMsg" );
        }

        public override void ImSmallerMsgReply( int source )
        {
        Console.WriteLine( "Strange: Online server received: ImSmaller" );
        }

        public override BullyMsg ImBossMsgRequest( int source )
        {
           Console.WriteLine( "Strange: Online server received: ImBOSS" );
            return null;
        }

        public override void AckBossMsgReply(int source)
        {
            Console.WriteLine( "Strange: Online server received: AckBossMsg" );
        }
    }
}
