namespace NewGaras.Infrastructure.Models.Inventory
{
    public class MatrialAddingOrderInfo
    {
        public long Id { get; set; }
        public long InventoryItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string ExpDate { get; set; }
        public string ReceivedOrReturnQTY { get; set; }
        public string ReceivedQTYUOP { get; set; }
        public string RequireQTY { get; set; }
        public string ReceivedQTYAfter {  get; set; }
        public string RemainQTY { get; set; }
        public string UOM {  get; set; }
        public string PurchaseUOM { get; set; }
        public long PONo {  get; set; }
        public string InventoryItemSerial { get; set; }
        public string SupplierItemSerial { get; set; }
        public string Comment { get; set; }
        public string POItemComment { get; set; }
        public long? QIReport {  get; set; }
        public decimal? Cost { get; set; }
        public string SupplierName { get; set; }
        public long? SupplierID { get; set; }
    }
}