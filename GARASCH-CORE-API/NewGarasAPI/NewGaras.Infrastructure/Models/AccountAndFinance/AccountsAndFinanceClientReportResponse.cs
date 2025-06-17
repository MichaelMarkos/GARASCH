using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsAndFinanceClientReportResponse
    {
        List<ClientReportDetails> clientList;
        decimal totalSalesVolume;
        decimal totalCollected;
        decimal remain;
        decimal totalCollectedPercent;
        decimal totalCreationProjectYTD;
        decimal totalCreationProjectYTDTotalCollected;
        decimal totalCreationProjectYTDTotalCollectedPercent;
        int totalProjectCount;
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
        public int TotalProjectCount
        {
            get
            {
                return totalProjectCount;
            }

            set
            {
                totalProjectCount = value;
            }
        }
        [DataMember]
        public decimal TotalCreationProjectYTD
        {
            get
            {
                return totalCreationProjectYTD;
            }

            set
            {
                totalCreationProjectYTD = value;
            }
        }
        [DataMember]
        public decimal TotalCreationProjectYTDTotalCollected
        {
            get
            {
                return totalCreationProjectYTDTotalCollected;
            }

            set
            {
                totalCreationProjectYTDTotalCollected = value;
            }
        }
        [DataMember]
        public decimal TotalCreationProjectYTDTotalCollectedPercent
        {
            get
            {
                return totalCreationProjectYTDTotalCollectedPercent;
            }

            set
            {
                totalCreationProjectYTDTotalCollectedPercent = value;
            }
        }
        [DataMember]
        public List<ClientReportDetails> ClientList
        {
            get
            {
                return clientList;
            }

            set
            {
                clientList = value;
            }
        }
        [DataMember]
        public decimal TotalSalesVolume
        {
            get
            {
                return totalSalesVolume;
            }

            set
            {
                totalSalesVolume = value;
            }
        }
        [DataMember]
        public decimal TotalCollected
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
        public decimal Remain
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


        [DataMember]
        public decimal TotalCollectedPercent
        {
            get
            {
                return totalCollectedPercent;
            }

            set
            {
                totalCollectedPercent = value;
            }
        }

    }

}
