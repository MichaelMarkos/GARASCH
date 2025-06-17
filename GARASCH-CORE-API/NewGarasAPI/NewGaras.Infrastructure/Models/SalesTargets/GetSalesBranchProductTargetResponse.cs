using NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets
{
    [DataContract]
    public class GetSalesBranchProductTargetResponse
    {
        List<ViewSalesBranchProductTarget> salesBranchProductTargets;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<ViewSalesBranchProductTarget> SalesBranchProductTargets
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
