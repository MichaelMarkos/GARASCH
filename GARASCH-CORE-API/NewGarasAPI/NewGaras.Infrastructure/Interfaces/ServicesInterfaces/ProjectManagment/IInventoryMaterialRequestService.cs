using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.ProjectManagment
{
    public interface IInventoryMaterialRequestService
    {
        public List<long> GetInvStoreIDAvailbileToHold(long InventoryItemID, int StoreID, decimal QTY);

        public BaseResponseWithId<long> AddInventoryStoreWithMatrialRequest(AddInventoryStoreWithMatrialRequest Request, long creator);
        public List<long> GetInvStoreIDAvailbileToReleaseHold(long InventoryItemID, decimal QTY);

        public InventoryItemMatrialRequest GetInventoryItemMatrialRequestList(GetInventoryItemMatrialFilters filters);

        public Task<InventoryItemMatrialRequestPagingResponse> GetInventoryMatrialRequestListPaging(InventoryItemMatrialPagingFilters filters, long creator);

        public InventoryItemMatrialRequestInfoResponse GetInventoryItemMatrialRequestInfo([FromHeader] long MatrialRequestOrderID, long creator);
    }
}
