using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Inventory
{
    public class InventoryStoreItemLowStock : InventoryItem
    {
        private long storeID;
        private bool havePRorPO;
        private decimal? minBalance;
        private decimal? maxBalance;
        private decimal? currentBalance;

        [DataMember]
        public long StoreID { get => storeID; set => storeID = value; }

        [DataMember]
        public bool HavePRorPO { get => havePRorPO; set => havePRorPO = value; }

        [DataMember]
        public decimal? MinBalance { get => minBalance; set => minBalance = value; }

        [DataMember]
        public decimal? MaxBalance { get => maxBalance; set => maxBalance = value; }

        [DataMember]
        public decimal? CurrentBalance { get => currentBalance; set => currentBalance = value; }
    }
}
