namespace NewGaras.Infrastructure.Models.AccountAndFinance
{
    public class InventoryItemInfo
    {
        public long? ID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string CommericalName { get; set; }
        public string PartNumber { get; set; }
        public string MarketName { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public string Category { get; set; }
        public int? CategoryId { get; set; }
        public string PurchasingUnit { get; set; }
        public string RequestionUnit { get; set; }
        public decimal? ConvertRateFromPurchasingToRequestionUnit { get; set; }
        public decimal? MaxBlanace {  get; set; }
        public decimal? MinBalance { get; set; }
        public string PriceCalculationMethod { get; set; }
        public decimal? Amount { get; set; }
        public bool Active { get; set; }
        public string ItemImage { get; set; }

        public decimal? Cost1 { get; set; }
        public decimal? Cost2 { get; set; }
        public decimal? Cost3 { get; set; }
        public string RequestionUOMShortName { get; set; }
    }
}