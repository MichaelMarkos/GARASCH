namespace NewGarasAPI.Models.AccountAndFinance
{
    public class FinancialPeriodAccountResponse
    {
        List<FinancialPeriod> financialPeriodList;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<FinancialPeriod> FinancialPeriodList
        {
            get
            {
                return financialPeriodList;
            }

            set
            {
                financialPeriodList = value;
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
