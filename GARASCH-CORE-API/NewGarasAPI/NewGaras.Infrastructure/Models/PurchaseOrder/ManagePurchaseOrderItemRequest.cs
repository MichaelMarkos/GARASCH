using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    [DataContract]
    public class ManagePurchaseOrderItemRequest
    {
        long? id;
        bool? isDelete;
        bool? applyOnPR;
        long? inventoryItemId;
        decimal? quantity;
        string reason;


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
        public bool? ApplyOnPR
        {
            get
            {
                return applyOnPR;
            }
            set
            {
                applyOnPR = value;
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
