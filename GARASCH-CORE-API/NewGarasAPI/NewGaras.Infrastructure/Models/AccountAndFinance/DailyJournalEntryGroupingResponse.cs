namespace NewGarasAPI.Models.AccountAndFinance
{
    public class DailyJournalEntryGroupingResponse
    {
        List<DailyJournalEntryGroupingByDate> dailyJournalEntryList;
        bool result;
        List<Error> errors;
        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }

        [DataMember]
        public List<DailyJournalEntryGroupingByDate> DailyJournalEntryList
        {
            get
            {
                return dailyJournalEntryList;
            }

            set
            {
                dailyJournalEntryList = value;
            }
        }
    }
}
