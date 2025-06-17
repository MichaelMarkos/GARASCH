using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    [DataContract]
    public class UpdatePurchaseOrderRequest
    {
        long pOID;
        long supplierID;
        int pOType;
        string requestDate;


        [DataMember]
        public long POID
        {
            get
            {
                return pOID;
            }
            set
            {
                pOID = value;
            }
        }

        [DataMember]
        public long SupplierID
        {
            get
            {
                return supplierID;
            }
            set
            {
                supplierID = value;
            }
        }
        [DataMember]
        public int POType
        {
            get
            {
                return pOType;
            }
            set
            {
                pOType = value;
            }
        }
        [DataMember]
        public string RequestDate
        {
            get
            {
                return requestDate;
            }
            set
            {
                requestDate = value;
            }
        }


    }


}
