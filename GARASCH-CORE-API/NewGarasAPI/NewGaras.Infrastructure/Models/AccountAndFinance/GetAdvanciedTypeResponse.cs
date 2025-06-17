namespace NewGarasAPI.Models.AccountAndFinance
{
    public class GetAdvanciedTypeResponse
    {
        List<GetAccountCategory> accountCategory;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<GetAccountCategory> AccountCategory
        {
            get
            {
                return accountCategory;
            }

            set
            {
                accountCategory = value;
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
