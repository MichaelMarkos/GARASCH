using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest
{
    [DataContract]
    public class SendFinalPOToSelectedSupplierRequest
    {
        // Required
        long? poId;
        int? contactThroughId;
        string date;
        // Optional
        string clientEmailFrom;
        long? supplierContactPersonID;
        string supplietEmailTo;
        string remindDate;


        [DataMember]
        public long? PoId
        {
            get
            {
                return poId;
            }

            set
            {
                poId = value;
            }
        }

        [DataMember]
        public int? ContactThroughId
        {
            get
            {
                return contactThroughId;
            }

            set
            {
                contactThroughId = value;
            }
        }

        [DataMember]
        public string Date
        {
            get
            {
                return date;
            }

            set
            {
                date = value;
            }
        }




        [DataMember]
        public string ClientEmailFrom
        {
            get
            {
                return clientEmailFrom;
            }

            set
            {
                clientEmailFrom = value;
            }
        }

        [DataMember]
        public long? SupplierContactPersonID
        {
            get
            {
                return supplierContactPersonID;
            }

            set
            {
                supplierContactPersonID = value;
            }
        }

        [DataMember]
        public string SupplietEmailTo
        {
            get
            {
                return supplietEmailTo;
            }

            set
            {
                supplietEmailTo = value;
            }
        }
        [DataMember]
        public string RemindDate
        {
            get
            {
                return remindDate;
            }

            set
            {
                remindDate = value;
            }
        }
    }
}
