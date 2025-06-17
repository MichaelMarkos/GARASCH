using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest
{
    [DataContract]
    public class RemoveAssignedToPRItemsRRequest
    {
        long purchaseRequestID;
        long inventoryMatrialRequestItemID;


        [DataMember]
        public long PurchaseRequestID
        {
            get
            {
                return purchaseRequestID;
            }

            set
            {
                purchaseRequestID = value;
            }
        }


        [DataMember]
        public long InventoryMatrialRequestItemID
        {
            get
            {
                return inventoryMatrialRequestItemID;
            }

            set
            {
                inventoryMatrialRequestItemID = value;
            }
        }

    }

}
