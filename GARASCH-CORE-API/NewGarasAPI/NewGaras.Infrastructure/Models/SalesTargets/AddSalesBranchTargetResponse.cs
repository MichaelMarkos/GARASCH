using NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets
{
    [DataContract]
    public class AddSalesBranchTargetResponse
    {
        int targetId;
        List<AddSalesBranchTarget> salesBranchTargets;

        [DataMember]
        public List<AddSalesBranchTarget> SalesBranchTargets
        {
            get
            {
                return salesBranchTargets;
            }

            set
            {
                salesBranchTargets = value;
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
    }
}
