namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetInvoiceData
    {
        public long? Id { get; set; }
        public string Serial { get; set; }
        public int Revision { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceType { get; set; }

        public long? PayerClientId { get; set; }
        public string PayerClientName { get; set; }

        public bool IsClosed { get; set; }
        public string CreationType { get; set; }
        public string InvoiceFor { get; set; }
        public string eInvoiceId { get; set; }
        public string eInvoiceStatus { get; set; }
        public string eInvoiceReason { get; set; }
        public string eInvoiceAcceptDate { get; set; }
        public string eInvoiceJsonBody { get; set; }
        //public Document eInvoiceJsonBodyInvoice { get; set; }
        //public CreaditOrDebitDocument eInvoiceJsonBodyCorD { get; set; }
        public bool? eInvoiceRequestToSend { get; set; }
        public long? SalesOfferId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool? Active { get; set; }
        public decimal? TotalInvoiceAmount { get; set; }
        public List<InvoiceItemData> InvoiceItemsList { get; set; }
    }
}