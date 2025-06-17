using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class MyProjectsCRM
    {
        [DataMember]
        public decimal TotalCollected { get; set; }
        [DataMember]
        public decimal TotalPrice { get; set; }
        [DataMember]
        public string ProjectType { get; set; }
        [DataMember]
        public string CollectedPercentage { get; set; }
        [DataMember]
        public int TotalOpen { get; set; }
        [DataMember]
        public int TotalClosed { get; set; }
        [DataMember]
        public int TotalDeactivated { get; set; }
    }
}
