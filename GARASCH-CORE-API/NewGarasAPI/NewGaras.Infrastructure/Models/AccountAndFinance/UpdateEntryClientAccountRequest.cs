namespace NewGarasAPI.Models.AccountAndFinance
{
    public class UpdateEntryClientAccountRequest
    {
        long? clientAccountId;
        long? accountId;
        long? entryId;
        long? clientId;
        long? projectId;
        string amountSign;
        decimal? amount;
        string description;

        [DataMember]
        public long? ClientAccountId
        {
            get
            {
                return clientAccountId;
            }

            set
            {
                clientAccountId = value;
            }
        }

        [DataMember]
        public long? AccountId
        {
            get
            {
                return accountId;
            }

            set
            {
                accountId = value;
            }
        }

        [DataMember]
        public long? EntryId
        {
            get
            {
                return entryId;
            }

            set
            {
                entryId = value;
            }
        }

        [DataMember]
        public long? ClientId
        {
            get
            {
                return clientId;
            }

            set
            {
                clientId = value;
            }
        }

        [DataMember]
        public long? ProjectId
        {
            get
            {
                return projectId;
            }

            set
            {
                projectId = value;
            }
        }

        [DataMember]
        public string AmountSign
        {
            get
            {
                return amountSign;
            }

            set
            {
                amountSign = value;
            }
        }


        [DataMember]
        public decimal? Amount
        {
            get
            {
                return amount;
            }

            set
            {
                amount = value;
            }
        }


        [DataMember]
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }
    }
}
