using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class AccountsAndFinanceSupplierReportResponse
    {
        List<SupplierReportDetails> supplierList;
        decimal totalSalesVolume;
        decimal totalCollected;
        decimal remain;
        bool result;
        int totalPOCount;
        decimal totalCreationPOYTD;
        List<Error> errors;
        decimal totalCollectedPercent;
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
        public int TotalPOCount
        {
            get
            {
                return totalPOCount;
            }

            set
            {
                totalPOCount = value;
            }
        }
        [DataMember]
        public List<SupplierReportDetails> SupplierList
        {
            get
            {
                return supplierList;
            }

            set
            {
                supplierList = value;
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

        [DataMember]
        public decimal TotalCreationPOYTD
        {
            get
            {
                return totalCreationPOYTD;
            }

            set
            {
                totalCreationPOYTD = value;
            }
        }

    }


}
