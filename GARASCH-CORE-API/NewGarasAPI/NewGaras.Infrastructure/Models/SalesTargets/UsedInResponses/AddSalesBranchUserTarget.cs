using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses
{
    public class AddSalesBranchUserTarget
    {
        public long? ID { get; set; }
        public long UserId { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public bool Active { get; set; }
        public int CurrencyID { get; set; }
    }
}
