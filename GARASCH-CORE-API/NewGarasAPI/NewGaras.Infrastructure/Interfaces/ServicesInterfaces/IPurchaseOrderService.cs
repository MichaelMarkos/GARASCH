using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.PurchaseOrder;
using NewGaras.Infrastructure.Models.PurchesRequest;
using NewGaras.Infrastructure.Models.PurchesRequest.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IPurchaseOrderService
    {
        public decimal GetPurchasingTotalPaidAmount();

        public decimal GetTotalAmountInventoryItemByListOfItemExpired(List<long> ListOFItem);

        public Tuple<long, decimal> GetTotalAmountWithNoOFLowStockInventoryItem(long? InventoryStoreID);

        public int GetNoOfSupplierAddingAndExternalBackOrder(long InventoryStoreID, string OperationType);

        public int GetCountOfSupplier();

        public PurchasingAndSuppliersDashboardResponse GetPurchasingAndSuppliersDashboard([FromHeader] long InventoryStoreID);
        public SelectPRItemsForAddPOResponse GetSelectPRItemsForAddPO(long? InventoryItemID, long UserID);
        public Task<BaseResponseWithID> AddNewPurchaseOrder(AddNewPurchaseOrderRequest Request, long UserID);
        public Task<BaseResponse> UpdatePurchaseOrder(UpdatePurchaseOrderRequest Request);

        public BaseResponse AddImportPoSettings(ImportPoSettings Request);

        public BaseResponse SentToSupplier(SentToSupplier sent, long UserID, string compName);
        public BaseResponse AddShippmentDocuments(AddShippmentDocuments doc, long UserID, string CompName);

        public BaseResponse AddShippingMethodDetails(ShippingMethodDetails details);
        public ViewPOApprovalStatusResponse GetPOApprovalStatus(long POID);

        public Task<BaseResponseWithID> SendFinalPOToSelectedSupplier(SendFinalPOToSelectedSupplierRequest Request, long UserID);

        public Task<BaseResponse> ManagePurchaseOrderItem(ManagePurchaseOrderItemRequest Request, long UserID);
        public Task<BaseResponse> ManageAddingOrderItemPO(ManageAddingOrderItemPORequest Request, long UserID);

        public ActivePODDLResponse GetActivePOList(long InventoryItemID, long ToSupplierID);

        public ActivePODDLResponse GetExternalPOList(long InventoryItemID, long ToSupplierID);

    }
}
