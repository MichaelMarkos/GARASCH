namespace NewGarasAPI.Models.AccountAndFinance
{
    public class DailyJournalEntryView
    {
        public long ID { get; set; }
        public bool Active { get; set; }
        public string DocumentNumber { get; set; }
        public string Serial { get; set; }
        public bool Status { get; set; }
        public string CreationDate { get; set; }
        public string EntryDate { get; set; }
        public string CreationUser { get; set; }
        public string CreationUserImg { get; set; }
        public string Description { get; set; }
        public decimal? AmountTranaction { get; set; }
        public long? ParentEntryId { get; set; }
        public string ParentEntrySerial { get; set; }
        public long? ChildEntryId { get; set; }
        public string ChildEntrySerial { get; set; }
        public bool? IsPublic { get; set; }
        public List<EntryAccount> AccountEntryList { get; set; }
        public int? BranchID { get; set; }
        public string BranchName { get; set; }
        public bool Approval { get; set; } = true;
    }
}
