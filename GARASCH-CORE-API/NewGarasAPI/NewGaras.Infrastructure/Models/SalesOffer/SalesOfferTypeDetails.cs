namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferTypeDetails
    {
        public List<GetSalesOffer> SalesOfferList { get; set; }
        public int TotalOffersCount { get; set; }
        public decimal TotalOffersPrice { get; set; }
    }
}