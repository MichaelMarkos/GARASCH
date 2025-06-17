using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsAndFinanceSpecificTreassuryResponse
    {
        List<AccountsAndFinanceDayBalance> dayBalanceList;
        string cumulativeBalanceBefore;
        string cumulativeBalanceAfter;
        string totalSumAmount;

        bool result;
        List<Error> errors;

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
        public List<AccountsAndFinanceDayBalance> DayBalanceList
        {
            get
            {
                return dayBalanceList;
            }

            set
            {
                dayBalanceList = value;
            }
        }
        [DataMember]
        public string CumulativeBalanceBefore
        {
            get
            {
                return cumulativeBalanceBefore;
            }

            set
            {
                cumulativeBalanceBefore = value;
            }
        }
        [DataMember]
        public string CumulativeBalanceAfter
        {
            get
            {
                return cumulativeBalanceAfter;
            }

            set
            {
                cumulativeBalanceAfter = value;
            }
        }

        [DataMember]
        public string TotalSumAmount
        {
            get
            {
                return totalSumAmount;
            }

            set
            {
                totalSumAmount = value;
            }
        }

    }

}
