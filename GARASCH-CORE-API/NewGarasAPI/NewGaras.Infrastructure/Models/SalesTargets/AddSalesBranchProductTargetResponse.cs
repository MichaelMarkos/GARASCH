using NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets
{
    [DataContract]
    public class AddSalesBranchProductTargetResponse
    {
        int targetId;
        int branchId;
        List<AddSalesBranchProductTarget> salesBranchProductTargets;

        [DataMember]
        public List<AddSalesBranchProductTarget> SalesBranchProductTargets
        {
            get
            {
                return salesBranchProductTargets;
            }

            set
            {
                salesBranchProductTargets = value;
            }
        }
        [DataMember]
        public int TargetId
        {
            get
            {
                return targetId;
            }

            set
            {
                targetId = value;
            }
        }
        [DataMember]
        public int BranchId
        {
            get
            {
                return branchId;
            }

            set
            {
                branchId = value;
            }
        }
    }
}
