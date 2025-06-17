using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PoInvoice.UsedInResponse
{
    public class PurchasePOExtraFees
    {
        public long? POId { get; set; }
        public long? POInvoiceId { get; set; }
        public string CreatedBy { get; set; }
        public List<PurchasePOExtraFee> PurchasePOExtraFeesList { get; set; }
    }
}
