using NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    public class AccountAndFinanceInventoryItemStockBalanceResponse
    {
        List<InventoryItemStockBlance> inventoryItemMovementList;
        bool result;
        List<Error> errors;
        decimal availableStock;
        decimal totalBalance;
        string uOR;
        [DataMember]
        public decimal TotalBalance
        {
            get
            {
                return totalBalance;
            }

            set
            {
                totalBalance = value;
            }
        }
        [DataMember]
        public decimal AvailableStock
        {
            get
            {
                return availableStock;
            }

            set
            {
                availableStock = value;
            }
        }

        [DataMember]
        public string UOR
        {
            get
            {
                return uOR;
            }

            set
            {
                uOR = value;
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
        public List<InventoryItemStockBlance> InventoryItemMovementList
        {
            get
            {
                return inventoryItemMovementList;
            }

            set
            {
                inventoryItemMovementList = value;
            }
        }

    }
}
