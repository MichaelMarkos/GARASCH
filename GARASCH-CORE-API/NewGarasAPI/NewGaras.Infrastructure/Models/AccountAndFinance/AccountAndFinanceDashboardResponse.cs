namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountAndFinanceDashboardResponse
    {
        AccountAndFinanceDashboardInfo data;
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
        public AccountAndFinanceDashboardInfo Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }
    }
}
