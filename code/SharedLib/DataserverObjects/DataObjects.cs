using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.DataserverObjects
{

    [Serializable]
    public class VersionFile
    {
        public int versionNumber;
        public String responseServerId;

        public VersionFile() { }
        public VersionFile(string serverID, int version)
        {
            versionNumber = version;
            responseServerId = serverID;
        }

    }


    [Serializable]
    public class TFile
    {
        public int VersionNumber;
        public long Size;
        public byte[] Data;
        public String responseServerId;


        public TFile()
        {

        }
        public TFile(int versionNumber, byte[] data)
        {
            VersionNumber = versionNumber;
            Data = data;
            this.Size = data.Length * 8;
        }

        public override string ToString()
        {
            String txt = Encoding.ASCII.GetString(Data);
            StringBuilder builder = new StringBuilder();
            builder.Append("VersionNumber: " + VersionNumber);
            builder.Append(", Size: " + Size);
            builder.Append(", Data: " + txt);
            return builder.ToString();
        }
    }


    [Serializable]
    public class InvalidVersionException : ApplicationException
    {
        public String campo;

        public InvalidVersionException()
        {

        }

        public InvalidVersionException(String c)
        {
            campo = c;
        }

        public InvalidVersionException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            campo = info.GetString("campo");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("campo", campo);
        }
    }

}
