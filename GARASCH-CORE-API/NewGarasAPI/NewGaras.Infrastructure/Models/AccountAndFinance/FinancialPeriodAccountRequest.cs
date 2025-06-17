namespace NewGarasAPI.Models.AccountAndFinance
{
    public class FinancialPeriodAccountRequest
    {
        long? iD;
        string startDate;
        string endDate;
        string description;
        bool? isCurrent;
        bool? isClosed;


        [DataMember]
        public long? ID
        {
            get
            {
                return iD;
            }

            set
            {
                iD = value;
            }
        }

        [DataMember]
        public string StartDate
        {
            get
            {
                return startDate;
            }

            set
            {
                startDate = value;
            }
        }

        [DataMember]
        public string EndDate
        {
            get
            {
                return endDate;
            }

            set
            {
                endDate = value;
            }
        }


        [DataMember]
        public bool? IsCurrent
        {
            get
            {
                return isCurrent;
            }

            set
            {
                isCurrent = value;
            }
        }

        [DataMember]
        public bool? IsClosed
        {
            get
            {
                return isClosed;
            }

            set
            {
                isClosed = value;
            }
        }

        [DataMember]
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }
    }
}
