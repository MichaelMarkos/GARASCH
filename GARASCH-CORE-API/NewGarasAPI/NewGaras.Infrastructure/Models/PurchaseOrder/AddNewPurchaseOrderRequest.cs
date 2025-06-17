using NewGaras.Infrastructure.Models.PurchaseOrder.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    [DataContract]
    public class AddNewPurchaseOrderRequest
    {
        long supplierID;
        int pOType;
        string requestDate;
        List<PRItemsQty> purchaseRequestItmesList;


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
        [DataMember]
        public List<PRItemsQty> PurchaseRequestItmesList
        {
            get
            {
                return purchaseRequestItmesList;
            }

            set
            {
                purchaseRequestItmesList = value;
            }
        }

    }


}
