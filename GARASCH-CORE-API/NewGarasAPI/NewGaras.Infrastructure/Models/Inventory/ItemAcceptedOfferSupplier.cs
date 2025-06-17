namespace NewGaras.Infrastructure.Models.Inventory
{
    public class ItemAcceptedOfferSupplier
    {
        public long? SupplierID { get; set; }
        public string SupplierName { get; set; }
        public long? POID { get; set; }
        public decimal? ReqQuantity { get; set; }
        public decimal? RecivedQuantity { get; set; }
        public int? CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public decimal? RateToEGP { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualUnitCost_EGP { get; set; }
        public decimal? FinalUnitCost_EGP { get; set; }
        public string ItemComment { get; set; }
        public string SupplierOfferComment { get; set; }
        public string CreationDate { get; set; }
    }
}