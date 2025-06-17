using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.InventoryItemMatrialAddingAndExternalOrder
{
    public class AddSupplierAndStoreWithMatrialAddingAndExternalBackOrderRequest
    {
        long supplierID;
        int inventoryStoreID;
        string recevingData;
        string orderType;

        List<MatrialAddingOrderItem> matrialAddingOrderItemList;

        [DataMember]
        public List<MatrialAddingOrderItem> MatrialAddingOrderItemList
        {
            get
            {
                return matrialAddingOrderItemList;
            }

            set
            {
                matrialAddingOrderItemList = value;
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
        public string RecevingData
        {
            get
            {
                return recevingData;
            }

            set
            {
                recevingData = value;
            }
        }


        [DataMember]
        public string OrderType
        {
            get
            {
                return orderType;
            }

            set
            {
                orderType = value;
            }
        }
    }
}
