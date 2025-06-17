using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Inventory
{
    public class InventortyItemListResponse
    {
        List<InventoryItemWithCheckOpeningBalance> inventoryItemList;
        PaginationHeader paginationHeader;
        long totalItemCount;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<InventoryItemWithCheckOpeningBalance> DDLList
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
        [DataMember]
        public long TotalItemCount
        {
            get
            {
                return totalItemCount;
            }

            set
            {
                totalItemCount = value;
            }
        }
    }

}
