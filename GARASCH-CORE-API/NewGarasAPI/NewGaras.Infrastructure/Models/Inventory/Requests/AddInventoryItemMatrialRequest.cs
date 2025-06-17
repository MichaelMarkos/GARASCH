using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Models.Inventory.Responses;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{

    public class AddInventoryItemMatrialRequest
    {
        int inventoryStoreID;
        long fromUserId;

        long creatorId;

        List<MatrialRequestItem> matrialRequestItemList;


        [DataMember]
        public long FromUserId
        {
            get
            {
                return fromUserId;
            }

            set
            {
                fromUserId = value;
            }
        }

        [DataMember]
        public long CreatorId
        {
            get
            {
                return creatorId;
            }

            set
            {
                creatorId = value;
            }
        }
        [DataMember]
        public List<MatrialRequestItem> MatrialRequestItemList
        {
            get
            {
                return matrialRequestItemList;
            }

            set
            {
                matrialRequestItemList = value;
            }
        }

        [DataMember]
        public int InventoryStoreID
        {
            get
            {
                return inventoryStoreID;
            }

            set
            {
                inventoryStoreID = value;
            }
        }
    }

}
