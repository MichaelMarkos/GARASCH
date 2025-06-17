using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsAndFinanceIncomeStatmentResponse
    {
        List<AccountsAndFinanceDetails> incomeAccountsAndFinanceList;
        List<AccountsAndFinanceDetails> expensesAccountsAndFinanceList;

        //List<AccountsAndFinanceJustForCalculation> incomeJustForCalculation;
        //List<AccountsAndFinanceJustForCalculation> expensesJustForCalculation;


        decimal netProfit;
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
        public List<AccountsAndFinanceDetails> IncomeAccountsAndFinanceList
        {
            get
            {
                return incomeAccountsAndFinanceList;
            }

            set
            {
                incomeAccountsAndFinanceList = value;
            }
        }
        [DataMember]
        public List<AccountsAndFinanceDetails> ExpensesAccountsAndFinanceList
        {
            get
            {
                return expensesAccountsAndFinanceList;
            }

            set
            {
                expensesAccountsAndFinanceList = value;
            }
        }
        [DataMember]
        public decimal NetProfit
        {
            get
            {
                return netProfit;
            }

            set
            {
                netProfit = value;
            }
        }

    }


}
