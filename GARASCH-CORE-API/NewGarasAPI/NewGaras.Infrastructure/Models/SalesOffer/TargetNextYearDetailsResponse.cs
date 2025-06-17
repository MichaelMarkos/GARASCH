using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class TargetNextYearDetailsResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public SalesTargetData SalesTarget {  get; set; }
        public List<SalesTargetBranch> SalesTargetBranchList { get; set; }
        public List<SalesTargetProduct> SalesTargetProductList { get; set; }
    }
}
