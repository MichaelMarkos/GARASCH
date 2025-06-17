using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class AccountsAndFinanceInventoryStoreItemResponse
    {
        List<InventoryStoreItemForReport> inventoryStoreItemList;
        PaginationHeader paginationHeader;
        decimal totalStockBalance;
        decimal totalStockBalanceValue;
        int noOfItems;
        long totalItems;
        long totalPricedItems;

        bool result;
        List<Error> errors;

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
        public long TotalPricedItems
        {
            get
            {
                return totalPricedItems;
            }

            set
            {
                totalPricedItems = value;
            }
        }
        [DataMember]
        public long TotalItems
        {
            get
            {
                return totalItems;
            }

            set
            {
                totalItems = value;
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
        public List<InventoryStoreItemForReport> InventoryStoreItemList
        {
            get
            {
                return inventoryStoreItemList;
            }

            set
            {
                inventoryStoreItemList = value;
            }
        }
        [DataMember]
        public decimal TotalStockBalance
        {
            get
            {
                return totalStockBalance;
            }

            set
            {
                totalStockBalance = value;
            }
        }
        [DataMember]
        public decimal TotalStockBalanceValue
        {
            get
            {
                return totalStockBalanceValue;
            }

            set
            {
                totalStockBalanceValue = value;
            }
        }

        [DataMember]
        public int NoOfItems
        {
            get
            {
                return noOfItems;
            }

            set
            {
                noOfItems = value;
            }
        }
    }
}
