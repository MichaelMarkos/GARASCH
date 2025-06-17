namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountInfoModel
    {
        public long ID { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public int? DataLevel { get; set; }
        public int? AccountOrder {  get; set; }
        public string Description { get; set; }
        public long? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public string AccountType { get; set; }
        public long? AccountCategoryId { get; set; }
        public string AccountCategoryName { get; set; }
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public bool? Active { get; set; }
        public bool? HaveChild { get; set; }
        //decimal? accumulativeBalance;
        //string balanceType;
        //bool? haveTax;
        //long? taxId;
        public bool HaveAdvancedSetting { get; set; }
        public long? AdvanciedTypeID { get; set; }
        // advanced setting
        public string AdvancedSettingName { get; set; }
        public string AdvancedSettingLocation { get; set; }
        public string AdvancedSettingDescription { get; set; }
        public List<long> AdvancedSettingKeepersList { get; set; }
    }
}