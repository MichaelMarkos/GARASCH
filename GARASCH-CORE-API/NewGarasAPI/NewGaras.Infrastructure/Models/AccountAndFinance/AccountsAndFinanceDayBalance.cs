using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsAndFinanceDayBalance
    {
        private string entryName;
        private string creationDate;
        private List<AccountOfJournalEntryDetails> entryDetails;

        [DataMember]
        public string EntryName { get => entryName; set => entryName = value; }
        [DataMember]
        public string CreationDate { get => creationDate; set => creationDate = value; }
        [DataMember]
        public List<AccountOfJournalEntryDetails> EntryDetails { get => entryDetails; set => entryDetails = value; }
    }
}
