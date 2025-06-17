using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PoInvoice.UsedInResponse
{
    public class PurchasePOExtraFee
    {
        public long? Id { get; set; }
        public long? ExtraFeesTypeId { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Amount { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? RateToEgp { get; set; }
        public long? POItemId { get; set; }
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}
