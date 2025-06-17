using NewGaras.Domain.Models;
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
    public interface IInventoryInternalBackOrderService
    {
        public string GetMatrialRelease(long ID);
        public string GetInventoryStoreItemUOMName(long id);
        public InventoryInternalBackOrderItemInfoResponse GetInventoryIntenralBackOrdertInfo([FromHeader] long MatrialInternalBackOrderID);

        public InventoryInternalBackOrderItemResponse GetInventoryInternalBackOrderItemList(GetInventoryInternalBackOrderFilters filters);

        public BaseResponseWithId<long> AddInventoryInternalBackOrder(AddInventoryInternalBackOrderRequest Request, long creator);
    }
}
