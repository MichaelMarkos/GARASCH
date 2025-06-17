namespace NewGarasAPI.Models.AccountAndFinance
{
    public class GetAccountEntryListByFilterResponse
    {
        List<AccountOfEntry> accountOfEntryList;
        //PaginationHeader paginationHeader;
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
        public List<AccountOfEntry> AccountOfEntryList
        {
            get
            {
                return accountOfEntryList;
            }

            set
            {
                accountOfEntryList = value;
            }
        }
    }
}
