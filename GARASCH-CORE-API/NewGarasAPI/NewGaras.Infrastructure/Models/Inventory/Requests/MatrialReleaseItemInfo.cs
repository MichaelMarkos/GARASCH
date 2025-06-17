namespace NewGarasAPI.Models.Inventory.Requests
{
    public class MatrialReleaseItemInfo
    {
        public long ID { get; set; }
        public long MatrialRequestItemID { get; set; }
        public long? InventoryItemID { get; set; }
        public string ItemName { get; set; }
        public string Comment { get; set; }
        public string ParentItemComment { get; set; }
        public string ItemCode { get; set; }
        // public string ExpDate { get; set; }
        public decimal? ReqQTY { get; set; }
        public decimal? RecivedQTY { get; set; }
        public decimal? RemQTY { get; set; }
        public string UOM { get; set; }
        //  public string InventoryItemSerial { get; set; }
        public string ProjectName { get; set; }
        public long? ProjectID { get; set; }
        public string FabOrderName { get; set; }
        // for create release
        public int? StoreLocationID { get; set; }
        public string StoreLocationName { get; set; }
        public string NewComment { get; set; }
        public decimal? NewRecivedQTY { get; set; }
        public string ExpirationDate { get; set; }
        public string Serial { get; set; }
        public bool? IsFIFO { get; set; }

        public decimal? StockQTY { get; set; }
        public List<long> InventoryStoreItemIDsList { get; set; }

        // public long? ParentReleaseItemID { get; set; }//for Bactch -ExpDate
        public List<InventoryStoreItemIDWithQTY> StockBalanceList { get; set; }//for Bactch -ExpDate
    }
}
