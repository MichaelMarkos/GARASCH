using NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets
{
    [DataContract]
    public class GetSalesTargetDDLResponse
    {
        List<TargetDetailsDDL> salesTargets;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<TargetDetailsDDL> SalesTargets
        {
            get
            {
                return salesTargets;
            }

            set
            {
                salesTargets = value;
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
