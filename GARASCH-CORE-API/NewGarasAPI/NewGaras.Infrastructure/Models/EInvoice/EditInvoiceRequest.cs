using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class EditInvoiceRequest
    {
        public long? InvoiceID { get; set; }
        public string Serial {  get; set; }
        public string InvoiceDate { get; set; }
    }
}
