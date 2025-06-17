namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferProductsVM
    {
        public long ID { get; set; }
        public string InventoryItems { get; set; }
        public double? QTY { get; set; }
        public decimal? Price { get; set; }
        public decimal? NetPrice { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TotalTaxAmount { get; set; }
        public decimal? TotalFinalUnitPrice { get; set; }
        public decimal? TotalNetPrice { get; set; }
        public string Description { get; set; }
        public decimal? FinalUnitPrice { get; set; }
        public decimal TotalOfTotalPrice { get; set; }
    }
}