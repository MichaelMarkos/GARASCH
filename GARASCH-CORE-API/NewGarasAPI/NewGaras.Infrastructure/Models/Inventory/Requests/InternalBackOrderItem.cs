namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class InternalBackOrderItem
    {
        public long InventoryItemID { get; set; }
        public long PojectID { get; set; }
        public decimal ReqQTY { get; set; }
        public string Comment { get; set; }
        public long MatrialReleaseID { get; set; }
        public long? InventoryStorItemMRItemID { get; set; }
        public int? StoreLocationID { get; set; }
        public string ExpDate { get; set; }
        public string Serial {  get; set; }
    }
}