namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InternalTransferOrderInfo
    {
        public long InventoryItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string TransferedQTY { get; set; }
        public string UOM {  get; set; }
    }
}