namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryInternalBackOrderInfo
    {
        public string InventoryInternalBackOrderNo { get; set; }
        public string FromUserName { get; set; }
        public bool Custody {  get; set; }
        public string StoreName { get; set; }
        public string OperationType { get; set; }
        public string RecivingDate { get; set; }
        public string CreatorName { get; set; }
    }
}