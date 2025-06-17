using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Domain.Models;
using NewGarasAPI.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGarasAPI.Models.Account;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IInventoryMateriaReleaseService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public InventoryItemMatrialReleaseResponse GetInventoryItemMatrialReleaseList(GetInventoryItemMatrialReleaseListFilters filters);

        public InventoryItemMatrialReleasePagingResponse GetInventoryItemMatrialReleaseListPaging(GetInventoryItemMatrialReleaseListPagingFilters filters);


        public InventoryItemMatrialReleaseInfoResponse GetInventoryItemMatrialReleasetInfo([FromHeader] long MatrialReleaseOrderID);

        public GetInventoryStoreItemBatchWithExpDateResponse GetInventoryStoreItemBatchWithExpDate(GetInventoryStoreItemBatchWithExpDateFilters filters);

        public BaseResponseWithId<long> AddInventoryItemMatrialRelease(AddInventoryItemMatrialReleaseRequest Request, long creator);

        public Task<BaseResponseWithId<long>> AddInventoryItemMatrialReleasePrintInfo(AddInventoryItemMatrialReleasePrintInfoRequest Request, long creator);


        public InventoryMatrialReleasePrintInfoResponse GetInventoryItemMatrialReleasetPrintInfo([FromHeader] long MatrialReleaseOrderID);

        public Task<GetMatrialReleaseShippingAddressContactResponse> GetMatrialReleaseShippingAddressContact([FromHeader] long MatrialReleaseID);

        public Task<BaseMessageResponse> MatrialReleasePDFReport(MatrialReleasePDFFilters filters, GetMatrialReleaseDataResponse Request);
        public InventoryItemMatrialReleaseInfoResponse GetInventoryItemForCreateMatrialRelease(long MatrialRequestID, long UserID);

        public MatrialReleaseDDLResponse GetMatrialReleaseItemList(GetMatrialReleaseItemListFilters filters);
        
    }
}
