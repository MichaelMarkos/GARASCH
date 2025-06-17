using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PoInvoice.UsedInResponse
{
    public class PurchasePoInvoiceDirect
    {
        public string InvoiceDate { get; set; }
        public string InvoiceCollectionDueDate { get; set; }
        public decimal? TotalInvoicePrice { get; set; }
        public long PurchasePOInvoiceTypeID { get; set; }
        public decimal? TotalInvoiceCost { get; set; }
        public bool IsClosed { get; set; }
        public bool? IsFinalPriced { get; set; }
        // Extra 2023-12-31
        public bool? IsSentToACC { get; set; }
        public int? TransactionId { get; set; }

    }
}
