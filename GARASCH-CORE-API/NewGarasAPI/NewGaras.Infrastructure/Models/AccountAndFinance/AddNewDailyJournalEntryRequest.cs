namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AddNewDailyJournalEntryRequest
    {
        DailyJournalEntryPrimaryInfo data;

        [DataMember]
        public DailyJournalEntryPrimaryInfo Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }
    }
}
