namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountsEntryDDLResponse
    {
        List<AccountsEntryDDL> accountList;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<AccountsEntryDDL> AccountList
        {
            get
            {
                return accountList;
            }

            set
            {
                accountList = value;
            }
        }
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

    }
}
