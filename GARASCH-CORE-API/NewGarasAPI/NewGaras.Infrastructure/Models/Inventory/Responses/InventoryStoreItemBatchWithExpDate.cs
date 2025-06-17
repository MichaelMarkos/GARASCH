using NewGarasAPI.Models.Inventory.Requests;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryStoreItemBatchWithExpDate
    {
        public string serial {get;set;}
        public string expDate {get;set;}
        public string CreationDate {get;set;}
        public int storeId {get;set;}
        public int? storeLocationId {get;set;}
        public long InventoryItemID {get;set;}
        public string ItemName {get;set;}
        public string ItemCode {get;set;}
        public string PurchasingUOM {get;set;}
        public string storeName {get;set;}
        public string storelocation {get;set;}
        public decimal totalStockBalanceWithHold {get;set;}
        public decimal availableStockBalance {get;set;}
        public decimal holdQty {get;set;}
        public List<InventoryStoreItemIDWithQTY> stockBalanceList {get;set;}
    }
}