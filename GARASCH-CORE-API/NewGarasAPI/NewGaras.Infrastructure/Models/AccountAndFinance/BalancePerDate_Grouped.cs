using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class BalancePerDate_Grouped
    {
        private string creationDate;
        private List<AccountsAndFinanceDayBalance_Grouped> entryDetailsPerDay;

        [DataMember]
        public string CreationDate { get => creationDate; set => creationDate = value; }
        [DataMember]
        public List<AccountsAndFinanceDayBalance_Grouped> EntryDetailsPerDay { get => entryDetailsPerDay; set => entryDetailsPerDay = value; }
    }
}
