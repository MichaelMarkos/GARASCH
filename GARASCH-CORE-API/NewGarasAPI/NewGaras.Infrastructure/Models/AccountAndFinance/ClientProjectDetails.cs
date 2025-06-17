using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class ClientProjectDetails
    {
        private string creationDate;
        private string projectName;
        private long projectID;
        private decimal volume;
        private decimal collected;
        private decimal remain;
        private decimal collectedPercent;

        [DataMember]
        public string CreationDate { get => creationDate; set => creationDate = value; }
        [DataMember]
        public string ProjectName { get => projectName; set => projectName = value; }
        [DataMember]
        public long ProjectID { get => projectID; set => projectID = value; }
        [DataMember]
        public decimal Volume { get => volume; set => volume = value; }
        [DataMember]
        public decimal Collected { get => collected; set => collected = value; }
        [DataMember]
        public decimal Remain { get => remain; set => remain = value; }
        [DataMember]
        public decimal CollectedPercent { get => collectedPercent; set => collectedPercent = value; }
    }
}
