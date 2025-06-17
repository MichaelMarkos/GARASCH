using NewGarasAPI.Models.Inventory.Requests;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class InternalTransferOrderItem
    {
        public long InventoryItemID { get; set; }
        public decimal TransferredQTY { get; set; }
        public string Comment { get; set; }
        public int? StoreLocationID { get; set; }
        public string expDate { get; set; }
        public string serial {  get; set; }
        public decimal? StockQTY { get; set; }
        public bool? IsFIFO { get; set; }
        public List<InventoryStoreItemIDWithQTY> StockBalanceList { get; set; }
    }
}