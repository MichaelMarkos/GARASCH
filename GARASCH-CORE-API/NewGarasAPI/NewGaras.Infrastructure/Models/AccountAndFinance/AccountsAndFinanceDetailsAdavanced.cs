using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsAndFinanceDetailsAdavanced
    {

        private long accountID;
        private string accountName;
        private string balanceAmount;
        private string parentCategory;
        private string currencyName;

        [DataMember]
        public long AccountID { get => accountID; set => accountID = value; }
        [DataMember]
        public string AccountName { get => accountName; set => accountName = value; }
        [DataMember]
        public string BalanceAmount { get => balanceAmount; set => balanceAmount = value; }
        [DataMember]
        public string ParentCategory { get => parentCategory; set => parentCategory = value; }
        [DataMember]
        public string CurrencyName { get => currencyName; set => currencyName = value; }
        // public string BalanceLE;
    }
}
