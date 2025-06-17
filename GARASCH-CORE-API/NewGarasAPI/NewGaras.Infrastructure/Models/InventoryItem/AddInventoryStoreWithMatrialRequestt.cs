using NewGaras.Infrastructure.Models.Inventory.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    [DataContract]
    public class AddInventoryStoreWithMatrialRequestt
    {
        long? matrialRequestId;
        long requestTypeID;
        int inventoryStoreID;
        long fromUserId;

        string requestDate;
        bool? isFinish;
        string holdReason;

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
        public long? MatrialRequestId
        {
            get
            {
                return matrialRequestId;
            }

            set
            {
                matrialRequestId = value;
            }
        }
        [DataMember]
        public string HoldReason
        {
            get
            {
                return holdReason;
            }

            set
            {
                holdReason = value;
            }
        }

        [DataMember]
        public bool? IsFinish
        {
            get
            {
                return isFinish;
            }

            set
            {
                isFinish = value;
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
        public long RequestTypeID
        {
            get
            {
                return requestTypeID;
            }

            set
            {
                requestTypeID = value;
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
