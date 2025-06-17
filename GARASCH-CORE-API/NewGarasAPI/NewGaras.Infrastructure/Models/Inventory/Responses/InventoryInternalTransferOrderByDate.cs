namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryInternalTransferOrderByDate
    {
        public string DateMonth { get; set; }
        public List<InventoryInternalTransferOrderInfo> InventoryInternalTransferOrderInfoList { get; set; }
    }
}