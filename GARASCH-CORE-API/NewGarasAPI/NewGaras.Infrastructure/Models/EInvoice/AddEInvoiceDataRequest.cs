using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class AddEInvoiceDataRequest
    {
        public long InvoiceId { get; set; }
        public string EInvoiceJsonBody { get; set; }
        public bool? EInvoiceRequestToSend { get; set; }

        public string EInvoiceId {  get; set; }
        public string EInvoiceStatus { get; set; }
        public string EInvoiceAcceptDate { get; set; }
    }
}
