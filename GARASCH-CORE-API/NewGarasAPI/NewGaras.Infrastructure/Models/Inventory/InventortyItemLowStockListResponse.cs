using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Inventory
{
    public class InventortyItemLowStockListResponse
    {
        List<InventoryStoreItemLowStock> inventoryItemList;
        PaginationHeader paginationHeader;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<InventoryStoreItemLowStock> DDLList
        {
            get
            {
                return inventoryItemList;
            }

            set
            {
                inventoryItemList = value;
            }
        }
        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }


        [DataMember]
        public PaginationHeader PaginationHeader
        {
            get
            {
                return paginationHeader;
            }

            set
            {
                paginationHeader = value;
            }
        }
    }
}
