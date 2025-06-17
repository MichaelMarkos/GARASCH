namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesPersonTotalSalesOffer
    {
        public long SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public decimal? SumFinalOfferPrice { get; set; }
        public long SalesOfferCount { get; set; }
    }
}