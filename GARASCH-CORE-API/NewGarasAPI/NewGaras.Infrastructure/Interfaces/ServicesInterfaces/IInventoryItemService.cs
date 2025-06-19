using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.InventoryItem;
using NewGaras.Infrastructure.Models.InventoryItem.Filters;
using NewGaras.Infrastructure.Models.User.Filters;
using NewGaras.Infrastructure.Models.User.Response;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.Inventory.Requests;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Models.Common;
using NewGarasAPI.Models.Inventory;


namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IInventoryItemService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithData<string> GetInventoryItemExcelTemplete(string CompName);

        public BaseResponseWithMessage<string> UploadInventoryItemExcel(AddAttachment dto, long UserID);




        public GetOfferInventoryItemsListForPOSResponse GetOfferInventoryItemsListForPOS(GetOfferInventoryItemsFilters filters, string companyname);
        public List<InventoryStoreItemIDWithQTY> GetParentReleaseIDWithSettingStore(List<InventoryStoreItem> InventoryStoreItemList, long InventoryItemID, int StoreID, int? StoreLocationID, decimal QTY, bool? IsFIFO);

        public bool AddInventoryStoreItemWithReturn(InventoryStoreItem ParentInventoryStoreItem, long InventoryItemId, decimal QTY, long SalesOfferId, long? SalesOfferProductId, long ValidateUserId);



        public void AddInvntoryStoreItemWithRelease(List<InventoryStoreItem> InventoryStoreItemList, long InventoryItemId, int StoreID, decimal QTY, bool IsFIFO, long SalesOfferId, long? SalesOfferProductId, long ValidateUserId);

        public BaseResponseWithData<string> InventoryStoreItemReportWithTabs(string companyname);

        public ExcelWorksheet MergeCells(ExcelWorksheet worksheet);

        public BaseResponseWithData<string> GetPurchaseForStoreReport(int? inventoryStoreID, string DateFrom, string DateTo, bool? internalTransferFlag, string CompName);

        public AccountAndFinanceInventoryItemStockBalanceResponse GetAccountAndFinanceInventoryItemStockBalance(long InventoryItemID);

        public BaseResponseWithData<string> GetInventoryItemMovementReport(long InventoryItemID, string DateFrom, string DateTo, long? storeID, string CompName);
        public BaseResponseWithDataAndHeader<AccountAndFinanceInventoryItemMovementResponse> GetAccountAndFinanceInventoryItemMovementListV2(AccountAndFinanceInventoryItemMovementListV2Filters filters);

        public BaseResponseWithData<string> GetInventoryItemRelaseRate(string DateFrom, string DateTo, string CompName);

        public BaseResponseWithId<long> EditInventoryStorePerID(EditInventoryStoreData Request, long creator);

        public Task<InventoryItemStockBalanceHoldResponse> GetAccountAndFinanceInventoryItemStockBalanceHold(long InventoryItemID);

        public BaseResponseWithData<InventoryStoreItemByOrderResponse> GetInventoryStoreItemByOrder(long OrderId, string OperationType);

        public Task<decimal> GetTotalAmountInventoryItemByListOfItemExpired(List<long> ListOFItem);

        public Tuple<long, decimal> GetTotalAmountWithNoOFLowStockInventoryItem(long? InventoryStoreID);

        public int GetNoOfSupplierAddingAndExternalBackOrder(long InventoryStoreID, string OperationType);

        public Task<InventoryAndStoresDashboardResponse> GetInventoryAndStoresDashboard([FromHeader] long InventoryStoreID, [FromHeader] DateTime? DateTo);

        public InventoryStoreItemTotalPriceResponse GetInventoryStoreItemTotalPricesAndCosts([FromHeader] long InventoryStoreID, [FromHeader] DateTime? DateTo);

        public Task<GetInventoryStoreKeeperDDLResponse> GetInventoryStoreKeeperDDL();

        public Task<GetInventoryStoreLocationsDDLResponse> GetInventoryStoreLocationsDDL();

        public Task<GetBranchProductResponse> GetBranchProduct();
        public Task<SelectDDLResponse> GetInventoryItemListDDL([FromHeader] int InventoryItemCategoryId, [FromHeader] int CurrentPage, [FromHeader] int NumberOfItemsPerPage);

        public BaseResponseWithId<long> DeleteInventoryStoreKeeper(AddInventoryStoreData Request);

        public string Arabic1256ToUtf8(string data);

        public BaseMessageResponse InventoryStoreItemReportWithoutPrices([FromHeader] string FileExtension);

        public Task<decimal> CurrencyConverterAsync(string from, string to, decimal amount);

        public BaseMessageResponse InventoryStoreItemReportWithProfitCalc([FromHeader] decimal Profit, [FromHeader] string FileExtension);

        public Task<BaseMessageResponse> HoldItemsExcel();

        public BaseMessageResponse InventoryStoreItemExcelsheet([FromHeader] string FileExtension);

        public GetRemainInventoryItemRequestedQtyResponse RemainInventoryItemRequestedQTYReport([FromHeader] string FileExtension);

        public GetInventoryStoreItemMovementReportResponse InventoryStoreItemMovementReportPDF([FromHeader] string FileExtension, [FromHeader] long InventoryItemID);

        public AccountsAndFinanceInventoryStoreItemReportResponse GetInventoryStoreItemMovementReportList(GetAccountAndFinanceInventoryStoreItemMovementReportListFilters filters);

        public InventoryItemSupplierResponse GetInventoryItemSupplierList(long InventoryItemID, string SupplierItemSerial, string OrderType);
        public GetContractTypeListResponse GetContractTypeList();
        public Task<BaseResponse> HoldReleaseInventoryMatrialRequest(AddInventoryStoreWithMatrialRequestt Request);

        public Task<InventoryItemHoldDetailsResponse> GetInventoryItemHoldDetails(long InventoryItemID);
        public AccountsAndFinanceInventoryStoreItemResponse GetAccountAndFinanceInventoryStoreItemReportList(GetAccountAndFinanceInventoryStoreItemReportListFilters filters);

        public InventortyStoreItemFullDataListResponse GetInventoryStoreItemFullDataList(GetInventoryStoreItemFullDataListFilters filters);
        public InventortyStoreListResponse GetInventoryStoreList(string Type, long userId);
        public InventortyStoreLocationListResponse GetInventoryStoreLocationList(long InventoryStoreID, long? InventoryItemID);

        public AccountsAndFinanceInventoryItemInfoResponse GetAccountAndFinanceInventoryStoreItemInfo([FromHeader] long InventoryItemID, [FromHeader] string InventoryItemCode);

        public Task<GetInventoryItemContentTreeResponse> GetInventoryItemContentTree([FromHeader] long? InventoryItemId);

        public BaseResponseWithId<long> DeleteInventoryItemContent([FromHeader] long InventoryItemContentId);

        public Task<BaseResponseWithId<long>> AddInventoryItemContent(AddInventoryItemContentDto request);

        public Task<BaseResponseWithId<long>> UpdateInventoryItemContent(UpdateInventoryItemContentDto request);

        public InventortyItemListResponse GetInventoryItemList(GetInventoryItemListFilters filters);

        public BaseResponseWithId<long> DeleteInvenotryItem([FromHeader] long InventoryItemId, [FromHeader] bool Active);
        public InventortyStoreIncludeLocationListResponse GetInventoryStoresIncludeLocationsListForBY([FromHeader] long userId);

        public InventortyItemLowStockListResponse GetInventoryStoreItemLowStockList([FromHeader] string SearchKey, [FromHeader] long InventoryStoreID, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10);

        public GetInventoryItemResponse GetInventoryItem([FromHeader] long InventoryItemID);

        public Task<GetInventoryStoreResponse> GetInventoryStore();

        public Task<GetInventoryStorePerIDResponse> GetInventoryStorePerID([FromHeader] long StoreID);

        public BaseResponseWithId<long> AddNewInventoryItem([FromForm] AddNewInventoryItemRequest request);

        public InventoryItemRejectedOfferSupplierResponse GetInventoryItemRejectedOfferSupplierList([FromHeader] long InventoryItemID, [FromHeader] long POID, [FromHeader] long SupplierID);

        public GetRemainInventoryItemRequestedQtyResponse GetRemainInventoryItemRequestedQty([FromHeader] long InventoryItemId);

        public AccountAndFinanceInventoryItemMovementResponse GetAccountAndFinanceInventoryItemMovementListV2(GetInventoryItemMovementListV2Filters filters);

        public SelectDDLResponse GetInventoryItemLocationList([FromHeader] long InventoryItemId, [FromHeader] int StoreId);
    }
}
