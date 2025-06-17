using NewGarasAPI.Models.Inventory.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{

    public class AddInventoryItemMatrialReleaseWithMatrialRequest
    {
        long? matrialRequestOrderId;
        int inventoryStoreID;
        long fromUserId;
        string status;
        int? userInsuranceId;
        string? transactionDate;

        List<MatrialReleaseItemWithRequestItem> matrialReleaseItemWithRequestItemList;


        [DataMember]
        public List<MatrialReleaseItemWithRequestItem> MatrialReleaseItemWithRequestItemList
        {
            get
            {
                return matrialReleaseItemWithRequestItemList;
            }

            set
            {
                matrialReleaseItemWithRequestItemList = value;
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
        public long? MatrialRequestOrderId
        {
            get
            {
                return matrialRequestOrderId;
            }

            set
            {
                matrialRequestOrderId = value;
            }
        }


        [DataMember]
        public string Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
            }
        }


        [DataMember]
        public int? UserInsuranceId
        {
            get
            {
                return userInsuranceId;
            }

            set
            {
                userInsuranceId = value;
            }
        }

        [DataMember]
        public string? TransactionDate
        {
            get
            {
                return transactionDate;
            }

            set
            {
                transactionDate = value;
            }
        }

    }

}
