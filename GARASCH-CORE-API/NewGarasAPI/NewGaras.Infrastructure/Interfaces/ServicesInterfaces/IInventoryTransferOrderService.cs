using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IInventoryTransferOrderService
    {
        public BaseResponseWithId<long> AddInvnetoryInternalTransferOrder(AddInventoryInternalTransferOrderRequest Request, long creator, string compName);
        public Task<BaseResponseWithId<long>> AddInvnetoryInternalTransferOrder2(AddInventoryInternalTransferOrderRequest request, long creator, string compName);
        public Task<BaseResponseWithId<long>> AddInvnetoryInternalTransferOrderPOS(AddInventoryInternalTransferOrderRequest Request, long creator, string compName);
        public Task<BaseResponseWithId<long>> AddInvnetoryInternalTransferOrderItemsPOS(AddInventoryInternalTransferOrderItemsRequest Request, long creator, string compName);
        public BaseResponseWithId<long> AddError(BaseResponseWithId<long> response, string errorCode, string errorMessage);
        public Task<BaseResponseWithId<long>> HandleExceptionAsync(BaseResponseWithId<long> response, Exception ex, long creator, string compName);
        public InventoryInternalTransferOrderResponse GetInventoryInternalTransferItemList(GetInventoryInternalTransferFilters filters, long userId, string compName);

        public InventoryInternalTransferOrderItemInfoResponse GetInventoryIntenralTransferItemInfo(long InternalTransferOrderID, long userId, string compName);

        public BaseResponseWithId<long> ReverseInvnetoryInternalTransferOrder(ReverseInvnetoryInternalTransferOrder Request, long creator, string compName);
        public BaseResponseWithDataAndHeader<List<InventoryInternalTransferOrderInfo>> GetInventoryInternalTransferItemListForWeb(GetInventoryInternalTransferForWebFilters filters, long creator, string compName);

    }
}
