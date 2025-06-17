using NewGaras.Infrastructure.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryStoreReports
{
    [DataContract]
    public class AddInventoryStoreReportRequest
    {
        long? iD;
        string reportSubject;
        string reportDesc;
        string dateFrom;
        string dateTo;
        long? storeId;
        bool? closed;
        bool? approved;
        List<UploadAttachment> attachmentList;



        [DataMember]
        public bool? Closed
        {
            get
            {
                return closed;
            }

            set
            {
                closed = value;
            }
        }

        [DataMember]
        public bool? Approved
        {
            get
            {
                return approved;
            }

            set
            {
                approved = value;
            }
        }

        [DataMember]
        public List<UploadAttachment> AttachmentList
        {
            get
            {
                return attachmentList;
            }

            set
            {
                attachmentList = value;
            }
        }
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
        public string ReportSubject
        {
            get
            {
                return reportSubject;
            }

            set
            {
                reportSubject = value;
            }
        }
        [DataMember]
        public string ReportDesc
        {
            get
            {
                return reportDesc;
            }

            set
            {
                reportDesc = value;
            }
        }

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
        public long? StoreId
        {
            get
            {
                return storeId;
            }

            set
            {
                storeId = value;
            }
        }

    }

}
