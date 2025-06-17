using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class ProjectDDL
    {
        [DataMember] public long ID { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public string ProjectSerial { get; set; }
        [DataMember] public long SalesOfferId { get; set; }
    }
}

