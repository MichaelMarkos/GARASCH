using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder.UsedInResponse
{
    public class PRItemsQty
    {
        public long? PRItemID { get; set; }
        public decimal? ApprovedQty { get; set; }
        public decimal? EstimatedPrice { get; set; }
         
        public int? CurrencyID { get; set; }
    }
}
