using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsAndFinanceDayBalance_Grouped
    {
        private string dayName;
        // public string EntryName;
        private List<AccountOfJournalEntryDetails> entryDetails;
        [DataMember]
        public string DayName { get => dayName; set => dayName = value; }
        [DataMember]
        public List<AccountOfJournalEntryDetails> EntryDetails { get => entryDetails; set => entryDetails = value; }
    }
}
