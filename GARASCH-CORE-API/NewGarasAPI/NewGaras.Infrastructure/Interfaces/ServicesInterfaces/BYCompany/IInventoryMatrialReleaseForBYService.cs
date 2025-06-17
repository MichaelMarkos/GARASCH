using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGarasAPI.Models.Inventory.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany
{
    public interface IInventoryMatrialReleaseForBYService
    {
        public BaseResponseWithID AddInventoryItemMatrialRequest([FromBody] AddInventoryItemMatrialReleaseWithMatrialRequest BodyRequest,long creator);
        public BaseResponseWithID AddInventoryItemMatrialReleaseFromMatrialRequest([FromBody] AddInventoryItemMatrialReleaseFromMatrialRequest BodyRequest,long creator);
        public BaseResponseWithID AggregateMatrialReleaseItem([FromBody] AggregateMatrialReleaseItemRequest BodyRequest,long creator);
        public BaseResponseWithID AddInventoryItemMatrialRelease([FromBody] AddInventoryItemMatrialReleaseWithMatrialRequest BodyRequest,long creator);
        public List<InventoryStoreItemIDWithQTY> GetParentReleaseIDWithSettingStore(List<InventoryStoreItem> InventoryStoreItemList, long InventoryItemID, int StoreID, int? StoreLocationID, decimal QTY, bool? IsFIFO);
        public BaseResponseWithID UpdateMatrialReleaseStatus([FromBody] UpdateMatrialReleaseStatusRequest BodyRequest,long creator);
        public BaseResponseWithDataAndHeader<List<MatrialReleaseIndex>> GetMatrialReleaseIndexList([FromHeader] GetMatrialReleaseIndexListFilters filters);
        public BaseResponseWithData<List<MatrialReleaseInfo>> GetMatrialReleaseCardsListWithItems([FromHeader] long? UserId, [FromHeader] string? FromDate, [FromHeader] string? ToDate);
        public BaseResponseWithData<MatrialRequestInfoDetails> ViewMatrialRequestWithItems([FromHeader] long? MatrialRequestId);
        public BaseResponseWithData<MatrialReleaseInfoDetails> ViewMatrialReleaseWithItems([FromHeader] long? MatrialReleaseId);
        public BaseResponseWithData<List<StoresListForKeeper>> GetStoresIdListForKeeper(long creator);
    }
}
