using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetTax
    {
        public long? Id { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal? TaxValue { get; set; }
        public string TaxName { get; set; }
        public string TaxCode { get; set; }
        public string TaxType { get; set; }
        public long? InvoicePayerClientId { get; set; }
        public string InvoicePayerClientName { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool? Active { get; set; }
        public bool? IsPercentage { get; set; }
        public List<GetTax> TaxChildList { get; set; }
    }
}