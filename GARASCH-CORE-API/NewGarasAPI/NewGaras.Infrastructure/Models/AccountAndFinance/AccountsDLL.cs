using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsDLL
    {
        private long iD;
        private string accountName;
        private string categoryName;
        private string accountTypeName;
        private bool haveChild;
        [DataMember]
        public long ID { get => iD; set => iD = value; }
        [DataMember]
        public string AccountName { get => accountName; set => accountName = value; }
        [DataMember]
        public string CategoryName { get => categoryName; set => categoryName = value; }
        [DataMember]
        public string AccountTypeName { get => accountTypeName; set => accountTypeName = value; }
        [DataMember]
        public bool HaveChild { get => haveChild; set => haveChild = value; }
    }
}
