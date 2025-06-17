namespace NewGarasAPI.Models.AccountAndFinance
{
    public class UpdateEntrySupplierAccountRequest
    {
        long? supplierAccountId;
        long? accountId;
        long? entryId;
        long? supplierId;
        long? pOId;
        string amountSign;
        decimal? amount;
        string description;

        [DataMember]
        public long? SupplierAccountId
        {
            get
            {
                return supplierAccountId;
            }

            set
            {
                supplierAccountId = value;
            }
        }

        [DataMember]
        public long? SupplierId
        {
            get
            {
                return supplierId;
            }

            set
            {
                supplierId = value;
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
        public long? POId
        {
            get
            {
                return pOId;
            }

            set
            {
                pOId = value;
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
