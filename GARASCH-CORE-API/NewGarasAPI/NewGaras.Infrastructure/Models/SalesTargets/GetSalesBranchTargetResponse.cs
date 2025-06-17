using NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets
{
    [DataContract]
    public class GetSalesBranchTargetResponse
    {
        List<ViewSalesBranchTarget> salesBranchTargets;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<ViewSalesBranchTarget> SalesBranchTargets
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
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }

    }
}
