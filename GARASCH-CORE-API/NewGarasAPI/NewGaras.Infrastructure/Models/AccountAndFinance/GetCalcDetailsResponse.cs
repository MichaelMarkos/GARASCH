namespace NewGarasAPI.Models.AccountAndFinance
{
    public class GetCalcDetailsResponse
    {
        decimal? totalAmount;
        decimal? totalCollected;
        decimal? remain;
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
        public decimal? TotalAmount
        {
            get
            {
                return totalAmount;
            }

            set
            {
                totalAmount = value;
            }
        }
        [DataMember]
        public decimal? TotalCollected
        {
            get
            {
                return totalCollected;
            }

            set
            {
                totalCollected = value;
            }
        }
        [DataMember]
        public decimal? Remain
        {
            get
            {
                return remain;
            }

            set
            {
                remain = value;
            }
        }
    }
}
