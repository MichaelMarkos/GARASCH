using NewGaras.Infrastructure.Entities;

namespace NewGarasAPI.Models.AccountAndFinance
{
    public class DailyJournalEntryResponse
    {
        List<DailyJournalEntryView> dailyJournalEntryList;
        PaginationHeader paginationHeader;
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
        public List<DailyJournalEntryView> DailyJournalEntryList
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

        [DataMember]
        public PaginationHeader PaginationHeader
        {
            get
            {
                return paginationHeader;
            }

            set
            {
                paginationHeader = value;
            }
        }
    }
}
