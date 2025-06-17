using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses
{
    public class SalesBranchProductTargetObj
    {
        public long ID { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }
        public string CurrencyName { get; set; }
    }
}
