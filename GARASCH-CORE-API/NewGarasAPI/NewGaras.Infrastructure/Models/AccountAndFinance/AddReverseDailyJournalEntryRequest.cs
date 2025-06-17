namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AddReverseDailyJournalEntryRequest
    {
        long? dailyJournalEntryId;
        bool? isReverse;

        [DataMember]
        public long? DailyJournalEntryId
        {
            get
            {
                return dailyJournalEntryId;
            }

            set
            {
                dailyJournalEntryId = value;
            }
        }

        [DataMember]
        public bool? IsReverse
        {
            get
            {
                return isReverse;
            }

            set
            {
                isReverse = value;
            }
        }
    }
}
