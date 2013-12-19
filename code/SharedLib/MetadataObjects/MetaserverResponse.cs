using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLib.Exceptions;

namespace SharedLib.MetadataObjects
    {
    [Serializable]
    public class MetaserverResponse
    {

        public PadiException Exception;
        public ResponseStatus Status;
        public MetadataEntry MetaEntry;
        public MetaRequest OriginalRequest;

        public MetaserverResponse( ResponseStatus status, MetaRequest originalRequest )
        {
            Status = status;
            OriginalRequest = originalRequest;
        }

    }


        public enum ResponseStatus
        {
            AckMaster,AckClient,Success,InvalidRequest,Exception
        }
    }
