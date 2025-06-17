namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryStoreHoldItemByStore
    {
        public string StoreName { get; set; }
        public decimal TotalHoldQTY { get; set; }
        public List<InventroryStoreItemHold> InventoryMatrialRequestInfoList;
    }
}