namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountsEntryDDL
    {
        public long ID { get; set; }
        public string AccountName { get; set; }
        public long CategoryID { get; set; } // DTmainType
        public string CategoryName { get; set; } // DTmainType
        public string AccountNumber { get; set; }
        public string AccountTypeName { get; set; }
        public long? AdvancedTypeId { get; set; }
        public string AdvancedTypeName { get; set; }
        public int? CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public bool HaveChild {  get; set; }
    }
}