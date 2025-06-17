namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryMatrialReleaseInfo
    {
        public long ID { get; set; }
        public string InventoryMatrialReleasetNo { get; set; }
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }
        public string Status { get; set; }
        public string StoreName { get; set; }
        public string CreatorName { get; set; }
    }
}