namespace NewGarasAPI.Models.AccountAndFinance
{
    public class DailyJournalEntryGroupingByDate
    {
        public string DateMonth { set; get;}
        public string DateToGetList { set; get;}
        public int CountOfEntry { set; get;}
        public decimal? TotalCreditSum { set; get;}
        public decimal? TotalDebitSum { set; get;}
    }
}