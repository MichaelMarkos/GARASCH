namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountInfoResponse
    {
        bool result;
        List<Error> errors;
        AccountInfoModel accountInfoModel;


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
        public AccountInfoModel AccountInfoModel
        {
            get
            {
                return accountInfoModel;
            }

            set
            {
                accountInfoModel = value;
            }
        }
    }
}
