namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AddNewAccountRequest
    {
        long? iD;
        string accountName;
        string description;
        long? parentCategoryId;
        string accountType;
        int? accountCategoryId;
        int? currencyId;
        bool? active;
        bool? haveChild;
        //decimal? accumulativeBalance;
        //string balanceType;
        //bool? haveTax;
        //long? taxId;
        bool haveAdvancedSetting;
        long? advanciedTypeID;
        // advanced setting
        string advancedSettingName;
        string advancedSettingLocation;
        string advancedSettingDescription;
        List<long> advancedSettingKeepersList;


        [DataMember]
        public long? ID
        {
            get
            {
                return iD;
            }

            set
            {
                iD = value;
            }
        }
        [DataMember]
        public string AccountName
        {
            get
            {
                return accountName;
            }

            set
            {
                accountName = value;
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
        [DataMember]
        public long? ParentCategoryId
        {
            get
            {
                return parentCategoryId;
            }

            set
            {
                parentCategoryId = value;
            }
        }

        [DataMember]
        public string AccountType
        {
            get
            {
                return accountType;
            }

            set
            {
                accountType = value;
            }
        }

        [DataMember]
        public int? AccountCategoryId
        {
            get
            {
                return accountCategoryId;
            }

            set
            {
                accountCategoryId = value;
            }
        }
        [DataMember]
        public int? CurrencyId
        {
            get
            {
                return currencyId;
            }

            set
            {
                currencyId = value;
            }
        }
        [DataMember]
        public bool? Active
        {
            get
            {
                return active;
            }

            set
            {
                active = value;
            }
        }
        [DataMember]
        public bool? HaveChild
        {
            get
            {
                return haveChild;
            }

            set
            {
                haveChild = value;
            }
        }

        //[DataMember]
        //public decimal? AccumulativeBalance
        //{
        //    get
        //    {
        //        return accumulativeBalance;
        //    }

        //    set
        //    {
        //        accumulativeBalance = value;
        //    }
        //}

        //[DataMember]
        //public string BalanceType
        //{
        //    get
        //    {
        //        return balanceType;
        //    }

        //    set
        //    {
        //        balanceType = value;
        //    }
        //}

        //[DataMember]
        //public bool? HaveTax
        //{
        //    get
        //    {
        //        return haveTax;
        //    }

        //    set
        //    {
        //        haveTax = value;
        //    }
        //}

        //[DataMember]
        //public long? TaxId
        //{
        //    get
        //    {
        //        return taxId;
        //    }

        //    set
        //    {
        //        taxId = value;
        //    }
        //}

        [DataMember]
        public bool HaveAdvancedSetting
        {
            get
            {
                return haveAdvancedSetting;
            }

            set
            {
                haveAdvancedSetting = value;
            }
        }

        [DataMember]
        public long? AdvanciedTypeID
        {
            get
            {
                return advanciedTypeID;
            }

            set
            {
                advanciedTypeID = value;
            }
        }


        [DataMember]
        public string AdvancedSettingName
        {
            get
            {
                return advancedSettingName;
            }

            set
            {
                advancedSettingName = value;
            }
        }

        [DataMember]
        public string AdvancedSettingLocation
        {
            get
            {
                return advancedSettingLocation;
            }

            set
            {
                advancedSettingLocation = value;
            }
        }

        [DataMember]
        public string AdvancedSettingDescription
        {
            get
            {
                return advancedSettingDescription;
            }

            set
            {
                advancedSettingDescription = value;
            }
        }


        [DataMember]
        public List<long> AdvancedSettingKeepersList
        {
            get
            {
                return advancedSettingKeepersList;
            }

            set
            {
                advancedSettingKeepersList = value;
            }
        }
    }
}
