namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesPersonSalesOfferDetails
    {
        public List<SalesPersonTotalSalesOffer> TotalSalesOfferPerSalesPersonList { get; set; }
        public long TotalOffersCount { get; set; }
        public decimal TotalOffersPrice { get; set; }
    }
}