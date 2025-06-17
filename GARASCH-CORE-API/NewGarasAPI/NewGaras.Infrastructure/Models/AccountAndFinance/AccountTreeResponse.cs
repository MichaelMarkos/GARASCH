using NewGarasAPI.Models.Account;

namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountTreeResponse
    {
        bool result;
        List<Error> errors;
        List<TreeViewAccount> getAccountTreeList;



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
        public List<TreeViewAccount> GetAccountTreeList
        {
            get
            {
                return getAccountTreeList;
            }

            set
            {
                getAccountTreeList = value;
            }
        }

    }
}
