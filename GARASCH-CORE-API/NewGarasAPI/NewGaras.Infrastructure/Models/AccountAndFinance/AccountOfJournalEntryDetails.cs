using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountOfJournalEntryDetails
    {
        private string accountName;
        private decimal balance;
        private string currencyName;
        private long entryID;

        [DataMember]
        public string AccountName { get => accountName; set => accountName = value; }
        [DataMember]
        public decimal Balance { get => balance; set => balance = value; }
        [DataMember]
        public string CurrencyName { get => currencyName; set => currencyName = value; }
        [DataMember]
        public long EntryID { get => entryID; set => entryID = value; }
        // public string CreationMonth;
        // public string CreationYear;
    }
}
