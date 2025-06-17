namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class InvoiceDataModel : InvoiceData
    {
        public long? OfferClientId { get; set; }
        public string OfferClientName { get; set; }
        public string SalesOfferSerial { get; set; }
        public string SalesOfferType { get; set; }
        public decimal? TotalValue { get; set; }
    }
}