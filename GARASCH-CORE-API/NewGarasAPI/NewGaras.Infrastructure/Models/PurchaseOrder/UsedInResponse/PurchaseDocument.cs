using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder.UsedInResponse
{
    public class PurchaseDocument
    {
        public long? Id { get; set; }
        public IFormFile FileContent { get; set; }
        public string Category { get; set; }

        public string ReceivedIn { get; set; }

        public decimal? Amount { get; set; }

        public int? CurrencyID { get; set; }
    }
}
