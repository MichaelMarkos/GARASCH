using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetRemainInventoryItemRequestedQtyResponse
    {
        long inventoryItemId;
        List<OpenProjectRemainRequestedItem> openProjectsRemainRequestedItem;
        decimal totalOpenProfjectsRemainRequestedItemsQty;
        List<OpenSalesOfferRequestedItem> openSalesOffersRequestedItem;
        decimal totalOpenSalesOffersRequestedItemsQty;
        decimal totalInventoryItemRequestedQty;
        decimal totalStocksHoldItemsQty;
        decimal totalStocksAvailableItemsQty;
        decimal totalAvailableItemsQty;
        string message;

        bool result;
        List<Error> errors;

        [DataMember]
        public long InventoryItemId
        {
            get
            {
                return inventoryItemId;
            }

            set
            {
                inventoryItemId = value;
            }
        }
        [DataMember]
        public string Message
        {
            get
            {
                return message;
            }

            set
            {
                message = value;
            }
        }
        [DataMember]
        public List<OpenProjectRemainRequestedItem> OpenProjectsRemainRequestedItem
        {
            get
            {
                return openProjectsRemainRequestedItem;
            }

            set
            {
                openProjectsRemainRequestedItem = value;
            }
        }

        [DataMember]
        public decimal TotalOpenProfjectsRemainRequestedItemsQty
        {
            get
            {
                return totalOpenProfjectsRemainRequestedItemsQty;
            }

            set
            {
                totalOpenProfjectsRemainRequestedItemsQty = value;
            }
        }

        [DataMember]
        public List<OpenSalesOfferRequestedItem> OpenSalesOffersRequestedItem
        {
            get
            {
                return openSalesOffersRequestedItem;
            }

            set
            {
                openSalesOffersRequestedItem = value;
            }
        }

        [DataMember]
        public decimal TotalOpenSalesOffersRequestedItemsQty
        {
            get
            {
                return totalOpenSalesOffersRequestedItemsQty;
            }

            set
            {
                totalOpenSalesOffersRequestedItemsQty = value;
            }
        }

        [DataMember]
        public decimal TotalInventoryItemRequestedQty
        {
            get
            {
                return totalInventoryItemRequestedQty;
            }

            set
            {
                totalInventoryItemRequestedQty = value;
            }
        }

        [DataMember]
        public decimal TotalStocksAvailableItemsQty
        {
            get
            {
                return totalStocksAvailableItemsQty;
            }

            set
            {
                totalStocksAvailableItemsQty = value;
            }
        }

        [DataMember]
        public decimal TotalStocksHoldItemsQty
        {
            get
            {
                return totalStocksHoldItemsQty;
            }

            set
            {
                totalStocksHoldItemsQty = value;
            }
        }

        [DataMember]
        public decimal TotalAvailableItemsQty
        {
            get
            {
                return totalAvailableItemsQty;
            }

            set
            {
                totalAvailableItemsQty = value;
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
