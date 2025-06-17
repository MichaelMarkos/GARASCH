namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountOfMovement
    {
        public long? AccountID { get; set; }
        public string CreationDate { get; set; }
        public string EntryDate { get; set; }

        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string Serial { get; set; }
        public string AccountName { get; set; }
        public string AccountCode { get; set; }
        public string AccountCategory { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }
        public decimal? Credit { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Accumulative { get; set; }
        public long? DailyJournalId { get; set; }
        public string Description { get; set; }
        public string Document { get; set; }
        public string FromOrTo { get; set; }
        public string CreatedBy { get; set; }
        public string MethodName { get; set; }
        public string ReleatedAccount { get; set; }
        public List<string> ReleatedAccountList { get; set; }

        public long? ClientID { get; set; }
        public string ClientName { get; set; }
        public long? ProjectID { get; set; }
        public string ProjectName { get; set; }
        public long? SupplierID { get; set; }
        public string SupplierName { get; set; }
        public long? POID { get; set; }
        public string AccountDescription { get; set; }
        public string AccountEntryComment { get; set; }
        public decimal? RateToLocalCU { get; set; }
        public decimal? CreditOtherCU { get; set; }
        public decimal? DebitOtherCU { get; set; }
        public decimal? AmountOtherCU { get; set; }
        public decimal? ACCOtherCU { get; set; }
    }
}