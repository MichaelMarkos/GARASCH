using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.ItemsPricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IItemsPricingService
    {
        public BaseResponse ManageInventoryStoreItemPricinig(AddOneInventoryStoreItemPricing Request, long UserID);
        public BaseResponse AddInventoryStoreItemPricinig(AddInventoryStoreItemPricing Request);
        public PurchasePoInventoryItemPriceListResponse GetInventoryItemPriceHistoryList(long InventoryItemId);
    }
}
