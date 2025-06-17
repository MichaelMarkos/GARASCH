using NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets
{
    [DataContract]
    public class AddSalesBranchUserTargetResponse
    {
        int targetId;
        int branchId;
        List<AddSalesBranchUserTarget> salesBranchUserTargets;

        [DataMember]
        public List<AddSalesBranchUserTarget> SalesBranchUserTargets
        {
            get
            {
                return salesBranchUserTargets;
            }

            set
            {
                salesBranchUserTargets = value;
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
