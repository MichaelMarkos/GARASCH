using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetIncomeTypeResponse
    {

        bool result;
        List<Error> errors;
        List<IncomeTypeData> incomeTypeList;



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
        public List<IncomeTypeData> IncomeTypeList
        {
            get
            {
                return incomeTypeList;
            }

            set
            {
                incomeTypeList = value;
            }
        }
    }
}
