using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metadata.ViewStatus;
using SharedLib.MetadataObjects;

namespace Metadata.ViewStates
    {

    public abstract class ViewState
        {
        public ServerStatus Status;
        public MetaViewManager Manager;

        public ViewState( MetaViewManager manager, ServerStatus status )
            {
            Status = status;
            Manager = manager;
            }
        public abstract BullyMsg NewRowdyMsgRequest(int source);
        public abstract void ViewStatusChanged(int lastMaster,long lastRequestId);

        public abstract BullyMsg AreYouBiggerMsgRequest( int source );
        public abstract void ShutUpMsgReply( int source );
        public abstract void ImSmallerMsgReply( int source );


        public abstract BullyMsg ImBossMsgRequest(int source);
        public abstract void AckBossMsgReply(int source);

        public abstract void Start( );
        }
    }
