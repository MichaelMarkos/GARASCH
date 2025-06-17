namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryInternalTransferOrderInfo
    {
        public string InventoryInternalTransferOrderNo { get; set; }
        public string FromInventoryStoreName { get; set; }
        public string ToInventoryStoreName { get; set; }
        public string RecivingDate { get; set; }
        public string CreationDate { get; set; }
        public string CreatorName { get; set; }
    }
}