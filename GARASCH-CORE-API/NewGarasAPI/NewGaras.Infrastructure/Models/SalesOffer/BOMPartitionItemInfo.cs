using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.AccountAndFinance;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class BOMPartitionItemInfo : InventoryItemInfo
    {
        public long BOMPartitionItemId { get; set; }
        public long BOMPartitionID { get; set; }
        public int? StoreCategoryId { get; set; }
        public int? ItemCategoryID { get; set; }
        public int ItemOrder { get; set; }
        public decimal RequiredQty { get; set; }
        public decimal ItemQtyPrice { get; set; }
        public string BOMPartitionItemPriceType { get; set; }
        public bool IsAlternative { get; set; }
        public long? AlternativeItem { get; set; }
        public bool ActiveToUse { get; set; }
        public string BOMPartitionItemDescription { get; set; }
        public string AvailableQty { get; set; }
        public List<GetInventoryItemPrice> PricesList { get; set; }
        public List<StoreStockModel> StoresStockList { get; set; }
        public decimal TotalBalance { get; set; }
    }
}