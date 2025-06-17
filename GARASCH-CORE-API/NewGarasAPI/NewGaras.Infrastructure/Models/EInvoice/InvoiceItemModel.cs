using NewGaras.Infrastructure.Models.SalesOffer;

namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class InvoiceItemModel : InvoiceItemData
    {
        public long? clientID { get; set; }
    }
}