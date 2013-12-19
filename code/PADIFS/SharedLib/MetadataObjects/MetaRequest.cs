using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.MetadataObjects
{

    //MetadataEntry Open(String filename,MetaCallType type);
    //Boolean Close(String filename, MetaCallType type);
    //MetadataEntry Create( String filename, int nbDataServers, int readQuorum, int writeQuorum, MetaCallType type );
    //Boolean Delete( String filename, MetaCallType type );

    [Serializable]
   public enum RequestType
    {
        Open,Close,Create,Delete,Registry
    }
 
        [Serializable]
public abstract class MetaRequest
        {
         public RequestType RequestType;
        public String Filename;
        public long MasterReqStamp;
       //Unique client request ID
        public int ClientId;
        public int ClientReqStamp;
        public int ClientPort;
        public String ClientHostname;
       //Numero de tentativas realizadas pelo cliente para ver se o pedido e repetido
        public int Attempt = 0;

        }

        [Serializable]

public class RequestOpen : MetaRequest
{
    public RequestOpen(String filename)
    {
        Filename = filename;
        RequestType = RequestType.Open;
    }
}

    [Serializable]
public class RequestClose : MetaRequest
    {
    public RequestClose( String filename)
        {
        Filename = filename;
        RequestType = RequestType.Close;
        }
    }

        [Serializable]
public class RequestCreate : MetaRequest
    {
    public int NbDataServer;
    public int ReadQuorum;
    public int WriteQuorum;
    public RequestCreate( String filename, int nbDataServer, int readQuorum, int writeQuorum)
        {
        Filename = filename;
        NbDataServer = nbDataServer;
        ReadQuorum = readQuorum;
        WriteQuorum = writeQuorum;
        RequestType = RequestType.Create;
        }
    }

        [Serializable]
public class RequestDelete : MetaRequest
    {
    public RequestDelete( String filename)
        {
        Filename = filename;
        RequestType = RequestType.Delete;
        }
    }

    [Serializable]
    public class RequestRegistry:MetaRequest
    {
        public String ServerId;
        public String ServerIp;
        public int ServerPort;

        public RequestRegistry( String serverId, String hostname, int serverPort )
        {
            ServerId = serverId;
            ServerIp = hostname;
            ServerPort = serverPort;
            RequestType = RequestType.Registry;
        }
   }
}
