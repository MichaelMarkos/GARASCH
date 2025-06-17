using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetSalesOfferProductTax
    {
        public long? ID { get; set; }
        public long TaxID { get; set; }
        //public long SalesOfferProductID;
        public decimal? Percentage { get; set; }
        public decimal? Value { get; set; }
        public string TaxName { get; set; }
        public string TaxCode { get; set; }
        public long ParentTaxID { get; set; }
        public GetTax ParentTax { get; set; }
    }
}