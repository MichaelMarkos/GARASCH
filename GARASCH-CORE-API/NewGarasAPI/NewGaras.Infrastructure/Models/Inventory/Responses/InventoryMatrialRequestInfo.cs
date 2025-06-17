namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryMatrialRequestInfo
    {
        public long ID { get; set; }
        public string InventoryMatrialRequestNo { get; set; }
        public string FromUserName { get; set; }
        public string Status { get; set; }
        public string StoreName { get; set; }
        public bool IsHold { get; set; }
    }
}