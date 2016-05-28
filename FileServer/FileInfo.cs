using System;
using System.Runtime.Serialization;

namespace FileServer
{
    [DataContract]
    public class FileInfo
    {
        [DataMember]
        public byte[] Icon { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public DateTime Modified { get; set; }

        [DataMember]
        public bool IsFolder { get; set; }

        [DataMember]
        public bool IsParent { get; set; }
    }
}
