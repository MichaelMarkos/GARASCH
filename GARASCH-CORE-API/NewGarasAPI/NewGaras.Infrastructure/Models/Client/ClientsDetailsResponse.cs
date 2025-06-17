using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    [DataContract]
    public class ClientsDetailsResponse
    {
        long salesPersonId;
        string salesPersonName;
        string lastReportDate;
        string clientStatus;
        string clientClassification;
        int? clientClassificationId;
        string lastSalesReportDate;
        long lastSalesReportId;
        long lastSalesReportLineId;
        string lastCRMReportDate;
        long lastCRMReportId;
        List<ContactPersonDetails> contactPersonsList;

        bool result;
        List<Error> errors;

        [DataMember]
        public long SalesPersonId
        {
            get
            {
                return salesPersonId;
            }

            set
            {
                salesPersonId = value;
            }
        }

        [DataMember]
        public string SalesPersonName
        {
            get
            {
                return salesPersonName;
            }

            set
            {
                salesPersonName = value;
            }
        }

        [DataMember]
        public string LastReportDate
        {
            get
            {
                return lastReportDate;
            }

            set
            {
                lastReportDate = value;
            }
        }

        [DataMember]
        public string ClientStatus
        {
            get
            {
                return clientStatus;
            }

            set
            {
                clientStatus = value;
            }
        }

        [DataMember]
        public string ClientCalssification
        {
            get
            {
                return clientClassification;
            }

            set
            {
                clientClassification = value;
            }
        }

        [DataMember]
        public int? ClientClassificationId
        {
            get
            {
                return clientClassificationId;
            }

            set
            {
                clientClassificationId = value;
            }
        }
        [DataMember]
        public string LastSalesReportDate
        {
            get
            {
                return lastSalesReportDate;
            }

            set
            {
                lastSalesReportDate = value;
            }
        }

        [DataMember]
        public string LastCRMReportDate
        {
            get
            {
                return lastCRMReportDate;
            }

            set
            {
                lastCRMReportDate = value;
            }
        }

        [DataMember]
        public long LastCRMReportId
        {
            get
            {
                return lastCRMReportId;
            }

            set
            {
                lastCRMReportId = value;
            }
        }

        [DataMember]
        public long LastSalesReportId
        {
            get
            {
                return lastSalesReportId;
            }

            set
            {
                lastSalesReportId = value;
            }
        }
        [DataMember]
        public long LastSalesReportLineId
        {
            get
            {
                return lastSalesReportLineId;
            }

            set
            {
                lastSalesReportLineId = value;
            }
        }
        [DataMember]
        public List<ContactPersonDetails> ContactPersonsList
        {
            get
            {
                return contactPersonsList;
            }

            set
            {
                contactPersonsList = value;
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
