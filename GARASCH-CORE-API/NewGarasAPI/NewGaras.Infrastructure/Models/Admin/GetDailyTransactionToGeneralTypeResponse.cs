using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetDailyTransactionToGeneralTypeResponse
    {
        bool result;
        List<Error> errors;
        List<DailyTransactionToGeneralTypeData> dailyTransactionToGeneralTypeList;



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
        public List<DailyTransactionToGeneralTypeData> DailyTransactionToGeneralTypeList
        {
            get
            {
                return dailyTransactionToGeneralTypeList;
            }

            set
            {
                dailyTransactionToGeneralTypeList = value;
            }
        }
    }
}
