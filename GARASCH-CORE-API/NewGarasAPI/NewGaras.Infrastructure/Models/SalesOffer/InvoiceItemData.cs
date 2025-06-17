namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class InvoiceItemData
    {
        public long? Id { get; set; }
        public long InvoiceIe { get; set; }
        public long? SalesOfferProductId { get; set; }
        public string Comment { get; set; }
        public string eInvoiceId { get; set; }
        public string eInvoiceStatus { get; set; }
        public string eInvoiceAcceptDate { get; set; }
        public double? Qty { get; set; }
        public decimal? ItemPrice { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public decimal? ItemTotalTax { get; set; }
    }
}