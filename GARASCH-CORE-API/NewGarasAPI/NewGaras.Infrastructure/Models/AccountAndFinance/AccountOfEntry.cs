namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountOfEntry
    {
        public long? POId { get; set; }
        public long? projectId { get; set; }
        public long? offerId { get; set; }
        public long entryId { get; set; }
        public string entrySerial { get; set; }
        public string accountName { get; set; }
        public decimal amount { get; set; }
        public List<AccountOfEntry> AccountEntryList { get; set; }
    }
}