using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class ProjectFabItemDDL
    {
        [DataMember]
        public long ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string FabNo { get; set; }

        [DataMember]
        public string FabSerial { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public long ProjectID { get; set; }

        [DataMember]
        public string ProjectSerial { get; set; }

    }
}
