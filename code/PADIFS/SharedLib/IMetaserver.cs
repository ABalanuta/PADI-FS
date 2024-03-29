﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLib.MetadataObjects;

namespace SharedLib
{

    public enum MetaCallType
    {
        ToMaster, ToSlave, FromClient
    }

    public interface IMetaToClient
    {
         MetaserverResponse ClientProcessRequest( MetaRequest request );

    }

    public interface  IMetaToMeta
    {
        MetaserverResponse MasterReceiveRequest( MetaRequest request );
        MetaserverResponse SlaveReceiveRequest( MetaRequest request );
        //Leader election and create new view
        BullyMsg BullyRequestsRetrival( BullyMsg msg );

        //Copy 
        CopyStructMetadata RequestUpdate( );
    }



    public interface IMetaToPuppet
    {
        String Dump();
        String Fail();
    }



     public interface IMetaToData
     {
         //The serverId will be the unique identifier
         Boolean Registry(String serverId,String serverIp,int serverPort);

     }

    public interface IRecovery
    {
        String Recover();
    }

 

}
