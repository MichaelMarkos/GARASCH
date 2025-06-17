using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetExpensisTypeResponse
    {
        bool result;
        List<Error> errors;
        List<ExpensisTypeData> expensisTypeList;



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
        public List<ExpensisTypeData> ExpensisTypeList
        {
            get
            {
                return expensisTypeList;
            }

            set
            {
                expensisTypeList = value;
            }
        }
    }
}
