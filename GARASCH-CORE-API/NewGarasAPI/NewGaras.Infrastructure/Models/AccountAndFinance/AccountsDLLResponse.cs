using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsDLLResponse
    {
        List<AccountsDLL> accountList;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<AccountsDLL> AccountList
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
