namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryItemCategoryVM
    {
        public int ID { get; set; }
        public int Count { get; set; }
        public decimal? RemainBalanceCostwithMainCu { get; set; }
        public decimal? RemainBalanceCostwithEgp { get; set; }
        public decimal? RateToEGP { get; set; }
        public decimal? FinalBalance { get; set; }
        public decimal? POInvoiceTotalCost { get; set; }
    }
}