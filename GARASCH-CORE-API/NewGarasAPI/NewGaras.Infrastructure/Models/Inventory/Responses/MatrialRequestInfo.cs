namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class MatrialRequestInfo
    {
        public long InventoryMatrialRequestID { get; set; }
        public long InventoryMatrialRequestItemID { get; set; }
        public bool MatrialRequestItemIsHold { get; set; }
        public string MatrialRequestStatus { get; set; }

        public long InventoryItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        // public string ExpDate;
        public string ReqQTY { get; set; }
        public string HoldReleaseQTY { get; set; }   // Hold Released
        public decimal? StockBalance { get; set; }
        public string UOM {  get; set; }
        //  public string InventoryItemSerial;
        public string Comment { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string FabOrderName { get; set; }
        public string ProductSerial { get; set; }
        public List<OrderReleaseObj> IDSOrderReleaseList { get; set; }
        public List<OrderReleaseObj> IDSpurchaseRequestList { get; set; }

        public List<long> InventoryStoreItemIDsList {  get; set; }
        public string ProjectSerial { get; set; }
        public long? ProjectId { get; set; }
        public long? FabricationOrderId { get; set; }
    }
}