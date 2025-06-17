using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetCostTypeResponse
    {
        bool result;
        List<Error> errors;
        List<CostTypeData> costTypeList;



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

        [DataMember]
        public List<CostTypeData> CostTypeList
        {
            get
            {
                return costTypeList;
            }

            set
            {
                costTypeList = value;
            }
        }
    }
}
