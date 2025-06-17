namespace NewGarasAPI.Models.AccountAndFinance
{
    public class GetAdvanciedType
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public List<GetAdvanciedSettingAccount> SettingAccounts { get; set; }
    }
}