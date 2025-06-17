namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountMovementResponse
    {
        string dateFrom;
        string dateTo;
        bool result;
        List<Error> errors;
        List<AccountOfMovement> getAccountMovementList;
        List<AccumulativePerAccount> accumulativePerAccountList;



        [DataMember]
        public string DateFrom
        {
            get
            {
                return dateFrom;
            }

            set
            {
                dateFrom = value;
            }
        }
        [DataMember]
        public string DateTo
        {
            get
            {
                return dateTo;
            }

            set
            {
                dateTo = value;
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

        [DataMember]
        public List<AccountOfMovement> GetAccountMovementList
        {
            get
            {
                return getAccountMovementList;
            }

            set
            {
                getAccountMovementList = value;
            }
        }
        [DataMember]
        public List<AccumulativePerAccount> AccumulativePerAccountList
        {
            get
            {
                return accumulativePerAccountList;
            }

            set
            {
                accumulativePerAccountList = value;
            }
        }
    }
}
