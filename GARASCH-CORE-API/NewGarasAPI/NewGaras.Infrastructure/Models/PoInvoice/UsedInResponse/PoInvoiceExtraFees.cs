using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PoInvoice.UsedInResponse
{
    public class PoInvoiceExtraFees
    {
        public long? Id { get; set; }
        public long? POInvoiceID { get; set; }
        public long POInvoiceExtraFeesTypeID { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Amount { get; set; }
        public decimal? RateToEGP { get; set; }
        public int? CurrencyID { get; set; }
        public long? POItemId { get; set; }
        public string Comment { get; set; }
    }
}
