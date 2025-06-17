using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryStoreItemForReport : InventoryStoreItemView
    {
        public decimal? HoldQTY { get; set; }
        public decimal? OpenPOQTY { get; set; }
        public decimal StockBalance { get; set; }
        public decimal StockBalanceValue { get; set; }
        public string RequestionUOMShortName { get; set; }
        public string ExpDate { get; set; }
        public long InventoryStoreId { get; set; }

        public string RUOM { get; set; }
        public string CommercialName { get; set; }
        public string MarketName { get; set; }
        public decimal? MaxStock { get; set; }
        public decimal? MinStock { get; set; }
    }
}