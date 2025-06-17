using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses
{
    public class ViewSalesBranchUserTarget
    {

        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public List<SalesBranchUserTargetObj> SalesBranchUserTargetList { get; set; }

    }
}
