namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryItemInternalTransferOrderInfo
    {
        public long InventoryInternalTransferOrderID { get; set; }
        public string FromStoreName { get; set; }
        public string ToStoreName { get; set; }
        public string RecivingDate { get; set; }
        public string CreatorName { get; set; }
        public List<InternalTransferOrderInfo> InternalBackOrderItemInfoList { get; set; }
    }
}