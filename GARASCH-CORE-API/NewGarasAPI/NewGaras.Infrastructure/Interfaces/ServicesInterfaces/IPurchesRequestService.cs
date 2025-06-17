using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.PurchesRequest;
using NewGaras.Infrastructure.Models.PurchesRequest.Filters;
using NewGaras.Infrastructure.Models.PurchesRequest.Responses;
using NewGarasAPI.Models.Inventory.Requests;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IPurchesRequestService
    {
        public ManageAssignedPRItemsResponse GetManageAssignedPRItems(ManageAssignedPRItemsFilters filters, string CompName);
        public UserDDLResponse GetPurchasingPersonsList(string SearchKey, string CompName);
        public BaseResponseWithID AddMatrialDirectPR(AddMatrialDirectPRRequest Request, long UserID);
        public BaseResponseWithID RemoveAssignedToPRItem(RemoveAssignedToPRItemsRRequest Request);
        public SelectPRItemsForAssignResponse GetSelectPRItemsForAssign(long? InventoryItemID);
        public BaseResponseWithID AddAssignPRItem(AssignPRItemRequest Request);
        public SelectPRItemsForAddPOResponse GetSelectPRItemsForAddPO(long? InventoryItemID, long UserID);
        public Task<BaseResponseWithID> AddPurchaseOrder(AddPurchaseOrderRequest Request, long UserID);
        public InventoryMatrialPurchaseRequestResponse GetMatrialPurchaseRequestList(InventoryMatrialPurchaseFilters filters);
        public InventoryMatrialPurchaseRequestResponse2 GetMatrialPurchaseRequestListForWeb(InventoryMatrialPurchaseFilters filters);
        public PurchaseRequestWithItemsInfoResponse GetPurchaseRequestWithItemsInfo(long PurchaseRequestID);
        public GetPurchasePOResponse GetMangePurchasePOList(long? InventoryItemID, long? CreatorUserID, string RequestDatestr,bool? WithJE);
        public GetPurchasePOWebResponse GetMangePurchasePOWebList(GetMangePurchasePOWebListFilters filters);
        public Task<ViewPurchaseOrderResponse> ViewPurchaseOrder(long? PoId, string SupplierInvoiceSerial);
        public Task<GetPurchaseItemListResponse> GetPurchaseItemList(string SupplierInvoiceSerial);
        public InventoryItemMatrialReleaseInfoResponse GetItemsForCreatePurchaseRequest(long MatrialRequestID, long UserID);
        public BaseResponseWithID AddInventoryItemPurchaseRequest(AddInventoryItemMatrialReleaseRequest Request, long UserID);
        public Task<BaseResponseWithID> ManagePurchaseRequestItem(ManagePurchaseRequestItemRequest Request, long UserID);
    }
}
