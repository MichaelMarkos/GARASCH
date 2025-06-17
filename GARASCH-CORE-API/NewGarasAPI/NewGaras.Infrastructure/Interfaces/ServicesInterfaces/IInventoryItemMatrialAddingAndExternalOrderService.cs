using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.InventoryItemMatrialAddingAndExternalOrder;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.InventoryItemMatrialAddingAndExternalOrder.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IInventoryItemMatrialAddingAndExternalOrderService
    {
        public InventoryItemSupplierMatrialAddingOrderInfoResponse GetInventoryItemSupplierMatrialAddingAndExternalOrderInfo(long MatrialAddingOrderID, long creator, string CompName);
        public InventoryItemMatrialAddingOrder GetInventoryItemMatrialAddingAndExternalOrderList(GetInventoryItemMatrialAddingAndExternalOrderListFilters filters, long creator, string CompName);
        public BaseResponseWithID AddInventoryAddingAndExternalBackOrder(AddSupplierAndStoreWithMatrialAddingAndExternalBackOrderRequest Request, long UserID, string CompanyName);
        public Task<BaseResponseWithID> AddPOToInventoryStoreItem([FromBody] AddPOToInventoryStoreItemRequest Request, long userId, string CompName);
        public BaseResponseWithId<long> ReverseInventoryAddingOrder(ReverseInventoryAddingOrderRequest Request, long UserID, string compName);
    }
}
