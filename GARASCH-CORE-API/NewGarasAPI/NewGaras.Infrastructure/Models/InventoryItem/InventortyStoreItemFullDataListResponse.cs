using NewGaras.Infrastructure.Models.ItemsPricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    [DataContract]
    public class InventortyStoreItemFullDataListResponse
    {
        List<InventoryStoreItemFullData> inventoryStorItemFullDataList;
        PaginationHeader paginationHeader;
        long totalCount;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<InventoryStoreItemFullData> InventoryStorItemFullDataList
        {
            get
            {
                return inventoryStorItemFullDataList;
            }

            set
            {
                inventoryStorItemFullDataList = value;
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
        public long TotalCount
        {
            get
            {
                return totalCount;
            }

            set
            {
                totalCount = value;
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

    }

}
