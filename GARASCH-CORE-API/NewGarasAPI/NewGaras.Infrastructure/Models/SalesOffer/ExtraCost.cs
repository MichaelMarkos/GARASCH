namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class ExtraCost
    {
        public long? Id { get; set; }
        public long? ExtraCostTypeId { get; set; }
        public decimal? Amount { get; set; }
        public long? InvoicePayerClientId { get; set; }
        public string InvoicePayerClientName { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool? Active { get; set; }
    }
}