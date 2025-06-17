using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsAndFinanceCashAndBanksResponse
    {
        List<AccountsAndFinanceDetailsAdavanced> banks;
        List<AccountsAndFinanceDetailsAdavanced> treasury;
        List<AccountsAndFinanceDetailsAdavanced> promissory;
        string totalBanksAmount;
        decimal totalBanksAmountAcumelative;
        string totalTreasuryAmount;
        string totalPromissoryAmount;
        string totalAmountSum;
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
        public List<AccountsAndFinanceDetailsAdavanced> Banks
        {
            get
            {
                return banks;
            }

            set
            {
                banks = value;
            }
        }
        [DataMember]
        public List<AccountsAndFinanceDetailsAdavanced> Treasury
        {
            get
            {
                return treasury;
            }

            set
            {
                treasury = value;
            }
        }
        [DataMember]
        public List<AccountsAndFinanceDetailsAdavanced> Promissory
        {
            get
            {
                return promissory;
            }

            set
            {
                promissory = value;
            }
        }
        [DataMember]
        public string TotalBanksAmount
        {
            get
            {
                return totalBanksAmount;
            }

            set
            {
                totalBanksAmount = value;
            }
        }

        [DataMember]
        public string TotalTreasuryAmount
        {
            get
            {
                return totalTreasuryAmount;
            }

            set
            {
                totalTreasuryAmount = value;
            }
        }

        [DataMember]
        public string TotalPromissoryAmount
        {
            get
            {
                return totalPromissoryAmount;
            }

            set
            {
                totalPromissoryAmount = value;
            }
        }

        [DataMember]
        public string TotalAmountSum
        {
            get
            {
                return totalAmountSum;
            }

            set
            {
                totalAmountSum = value;
            }
        }
        [DataMember]
        public decimal TotalBanksAmountAcumelative
        {
            get
            {
                return totalBanksAmountAcumelative;
            }

            set
            {
                totalBanksAmountAcumelative = value;
            }
        }


    }
}
