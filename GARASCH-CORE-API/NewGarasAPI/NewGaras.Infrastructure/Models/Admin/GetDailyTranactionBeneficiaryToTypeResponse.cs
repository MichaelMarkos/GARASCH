using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetDailyTranactionBeneficiaryToTypeResponse
    {
        bool result;
        List<Error> errors;
        List<DailyTranactionBeneficiaryToTypeData> dailyTransactionToTypeList;



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
        public List<DailyTranactionBeneficiaryToTypeData> DailyTransactionToTypeList
        {
            get
            {
                return dailyTransactionToTypeList;
            }

            set
            {
                dailyTransactionToTypeList = value;
            }
        }

    }
}
