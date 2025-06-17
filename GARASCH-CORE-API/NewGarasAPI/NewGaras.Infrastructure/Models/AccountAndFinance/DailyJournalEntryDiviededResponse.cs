using NewGaras.Infrastructure.Entities;

namespace NewGarasAPI.Models.AccountAndFinance
{
    public class DailyJournalEntryDiviededResponse
    {
        List<DailyJournalEntryView> dailyJournalEntryList;
        List<DailyJournalEntryView> autoSalesJEList;
        List<DailyJournalEntryView> deleteAndReverseJEList;
        List<DailyJournalEntryView> otherJEList;
        decimal totalAmount;
        long count;
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
        public long Count
        {
            get
            {
                return count;
            }

            set
            {
                count = value;
            }
        }

        [DataMember]
        public decimal TotalAmount
        {
            get
            {
                return totalAmount;
            }

            set
            {
                totalAmount = value;
            }
        }



        [DataMember]
        public List<DailyJournalEntryView> AutoSalesJEList
        {
            get
            {
                return autoSalesJEList;
            }

            set
            {
                autoSalesJEList = value;
            }
        }

        [DataMember]
        public List<DailyJournalEntryView> DeleteAndReverseJEList
        {
            get
            {
                return deleteAndReverseJEList;
            }

            set
            {
                deleteAndReverseJEList = value;
            }
        }

        [DataMember]
        public List<DailyJournalEntryView> OtherJEList
        {
            get
            {
                return otherJEList;
            }

            set
            {
                otherJEList = value;
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
    }
}
