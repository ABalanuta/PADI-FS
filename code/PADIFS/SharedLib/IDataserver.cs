using System;
using SharedLib.DataserverObjects;
using SharedLib.MetadataObjects;

namespace SharedLib
{
    public interface IDataToClient
    {
        TFile Read(String localFileName, SemanticType semantic, int minVersion);
        String Write(String localFileName, byte[]  data, int version);
        VersionFile GetVersion(String localfileName);
    }


    public interface IDataToPuppet
    {
        String Freeze();
        String UnFreeze();
        String Fail();
        String Recover();
        String Dump();
    }

    
    public interface IDataToMeta
    {
        void CreateEmptyFile(String localFilename);
    }

}
