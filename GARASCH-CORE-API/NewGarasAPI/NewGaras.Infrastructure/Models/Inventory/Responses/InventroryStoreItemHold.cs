namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventroryStoreItemHold
    {
        public long InvStoreItemID { get; set; }
        public decimal? HoldQTY { get; set; }
        public string HoldReason { get; set; }
        public string UOM { get; set; }
    }
}