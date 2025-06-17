using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class AddEInvoiceRequest
    {
        public long? ClientID { get; set; }
        public long? SalesOfferID { get; set; }
    }
}
