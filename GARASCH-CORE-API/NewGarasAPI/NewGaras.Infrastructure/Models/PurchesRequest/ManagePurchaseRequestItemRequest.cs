using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest
{
    [DataContract]
    public class ManagePurchaseRequestItemRequest
    {
        long? id;
        bool? isDelete;
        long? inventoryItemId;
        decimal? quantity;
        string reason;
        //if called from PO set POItem == null for temp until update on POItem with new POItem
        bool? managedbyPO;
        [DataMember]
        public bool? ManagedbyPO
        {
            get
            {
                return managedbyPO;
            }
            set
            {
                managedbyPO = value;
            }
        }

        [DataMember]
        public long? Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        [DataMember]
        public bool? IsDelete
        {
            get
            {
                return isDelete;
            }
            set
            {
                isDelete = value;
            }
        }
        [DataMember]
        public long? InventoryItemId
        {
            get
            {
                return inventoryItemId;
            }
            set
            {
                inventoryItemId = value;
            }
        }
        [DataMember]
        public decimal? Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                quantity = value;
            }
        }
        [DataMember]
        public string Reason
        {
            get
            {
                return reason;
            }
            set
            {
                reason = value;
            }
        }

    }

}
