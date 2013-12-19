using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.MetadataObjects
    {
    [Serializable]
    public class BullyMsg
    {
        public BullyType Type;
        public int Source;
        public int Destination;
        public ServerStatus Status;
        public int LastMaster = -1;
        public long LastRequestId = -1;

        public BullyMsg( BullyType type, int source,ServerStatus originStatus)
        {
            Type = type;
            Source = source;
            Status = originStatus;
        }

 
    }

    public enum BullyType
        {
            NewRowdy, StatusMsg, AreYouBigger, ShutUp,ImBoss,ImSmaller,AckBoss,
        }

    public enum ServerStatus
        {
        Unknown, Master, Slave, Off, New, Paused, Bully
        }


    }
