using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class ClientReportDetails
    {
        private string clientName;
        private long clientID;
        private decimal volume;
        private decimal collected;
        private decimal remain;
        private decimal collectedPercent;

        private List<ClientProjectDetails> clientProjectsList;
        [DataMember]
        public string ClientName { get => clientName; set => clientName = value; }
        [DataMember]
        public long ClientID { get => clientID; set => clientID = value; }
        [DataMember]
        public decimal Volume { get => volume; set => volume = value; }
        [DataMember]
        public decimal Collected { get => collected; set => collected = value; }
        [DataMember]
        public decimal Remain { get => remain; set => remain = value; }
        [DataMember]
        public decimal CollectedPercent { get => collectedPercent; set => collectedPercent = value; }
        [DataMember]
        public List<ClientProjectDetails> ClientProjectsList { get => clientProjectsList; set => clientProjectsList = value; }
    }
}
