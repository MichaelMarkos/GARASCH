using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Inventory
{
    public class InventoryItem
    {
        private long iD;
        private string name;
        private string itemCode;
        private string partNo;
        private long? serialCounter;
        private string uOP;
        private string uOR;
        private decimal? exchangeFactor;

        [DataMember]
        public long ID { get => iD; set => iD = value; }
        [DataMember]
        public string Name { get => name; set => name = value; }
        [DataMember]
        public string ItemCode { get => itemCode; set => itemCode = value; }
        [DataMember]
        public string PartNo { get => partNo; set => partNo = value; }
        [DataMember]
        public long? SerialCounter { get => serialCounter; set => serialCounter = value; }
        [DataMember]
        public string UOP { get => uOP; set => uOP = value; }
        [DataMember]
        public string UOR { get => uOR; set => uOR = value; }
        [DataMember]
        public decimal? ExchangeFactor { get => exchangeFactor; set => exchangeFactor = value; }
    }
}
