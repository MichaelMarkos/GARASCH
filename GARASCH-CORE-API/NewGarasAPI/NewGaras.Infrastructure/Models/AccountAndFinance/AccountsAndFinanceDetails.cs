using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsAndFinanceDetails
    {
        public bool HaveChild {  get; set; }
        public long AccountID { get; set; }
        public string AccountName { get; set; }
        public string CurrencyName { get; set; }
        public string BalanceAmount { get; set; }
        public decimal Accumulative {  get; set; }
        public long? ParentCategoryID { get; set; }
        public List<BalancePerMonth> BalancePerMonthList { get; set; }
        public List<AccountsAndFinanceDetails> AccountsAndFinanceChildList { get; set; }
    }
}
