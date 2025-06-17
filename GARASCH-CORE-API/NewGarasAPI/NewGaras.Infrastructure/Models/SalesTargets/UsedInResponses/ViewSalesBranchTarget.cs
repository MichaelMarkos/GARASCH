using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses
{
    public class ViewSalesBranchTarget
    {
        public long ID { get; set; }
        public int TargetID { get; set; }
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }
        public string CurrencyName { get; set; }
    }
}
