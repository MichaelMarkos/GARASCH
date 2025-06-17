using NewGarasAPI.Models.Account;

namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountBalancePerExpOrIncType
    {
        public int? ExpOrIncTypeID { get; set; }
        public string ExpOrIncTypeName { get; set; }
        public decimal TotalAmount { get; set; }
        public List<BalancePerMonth> BalancePerMonthList { get; set; }
    }
}
