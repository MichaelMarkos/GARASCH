using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsAndFinanceIncomeChildrenCalcResponse
    {
        List<AccountsAndFinanceDetails> accountsAndFinanceDetailsList;

        decimal totalsumChildren;


        [DataMember]
        public List<AccountsAndFinanceDetails> AccountsAndFinanceDetailsList
        {
            get
            {
                return accountsAndFinanceDetailsList;
            }

            set
            {
                accountsAndFinanceDetailsList = value;
            }
        }

        [DataMember]
        public decimal TotalsumChildren
        {
            get
            {
                return totalsumChildren;
            }

            set
            {
                totalsumChildren = value;
            }
        }

    }

}
