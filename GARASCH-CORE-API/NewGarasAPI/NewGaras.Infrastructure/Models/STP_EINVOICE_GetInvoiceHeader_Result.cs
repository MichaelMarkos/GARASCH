using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class STP_EINVOICE_GetInvoiceHeader_Result
    {
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public string Serial { get; set; }
        public string TrxType { get; set; }
        public Nullable<long> SalesOfferId { get; set; }
    }
}
