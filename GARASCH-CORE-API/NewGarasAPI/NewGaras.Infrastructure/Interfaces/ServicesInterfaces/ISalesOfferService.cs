using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.SalesOffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.SalesOffer.Filters;
using NewGarasAPI.Models.Account;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface ISalesOfferService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithData<string> SalesOfferReport(SalesOfferReportFilter filters, string CompanyName);

        public BaseResponseWithData<string> CalculateOfferFinalPrice(long SalesOfferId);
        public BaseResponseWithData<bool> ValidateProductsPrices(List<OfferProductValidation> OfferProducts);
        public decimal CalculateProductFinalPrice(List<GetTax> OfferTaxes, decimal PriceBeforeTax);
        public BaseResponseWithData<bool> UpdateProductsPricesInDB(long OfferID);
        public BaseResponseWithData<string> GetClientFromSalesOfferReport(AccountSalesOfferReportFilter filters, string CompanyName);
        public BaseResponseWithData<string> GetSupplierFromSalesOfferReport(AccountSalesOfferReportFilter filters, string CompanyName);
        public BaseResponseWithData<string> GetAccountAmountByAdvancedType(string CompanyName, int Year, bool ByEntryDate, long AccountCategoryId, long? AdvancedTypeId, int?BranchId);
        public BaseResponseWithData<string> GetAccountAmountByCategoryName(string CompanyName, int Year, bool ByEntryDate, int? BranchId, string AccountsIds);

        public List<GetInvoiceData> GetSalesOfferInvoicesList(long SalesOfferId);
        public GetSalesOfferListResponse GetSalesOfferList(GetSalesOfferListFilters filters, string OfferStatusParam);

        public GetSalesOffer GetSalesOfferInfo(long SalesOfferId);

        public List<Attachment> GetSalesOfferAttachmentsList(long SalesOfferId);

        public List<GetSalesOfferProduct> GetSalesOfferProductsList(long SalesOfferId);

        public List<GetTax> GetSalesOfferTaxList(long SalesOfferId);

        public List<GetSalesOfferDiscount> GetSalesOfferDiscountList(long SalesOfferId);

        public List<ExtraCost> GetSalesOfferExtraCostList(long SalesOfferId);

        public GetSalesOfferDetailsResponse GetSalesOfferDetails(long SalesOfferId);

        public GetSalesOfferProductsDetailsResponse GetSalesOfferProductsDetails(long SalesOfferId);



        public GetOfferExtraCostTypesListResponse GetOfferExtraCostTypesList();

        public GetOfferTaxListResponse GetOfferTaxTypesList();

        public GetOfferTermsAndConditionsListResponse GetOfferTermsAndConditionsList();

        public Task<GetSalesOfferListDDLResponse> GetSalesOfferListDDLData(long ClientId, string SearchKey, bool? StatusIsOpenFilter);
        public GetSalesOfferProductListDDLResponse GetSalesOfferProductListDDLData(long SalesOfferId);
        public BaseResponseWithData<string> GetSalesOfferDueClientPOS(string Datefrom, string Dateto, string CompName);
        public BaseResponseWithData<string> GetSalesOfferDueForStore(string date,string CompName, bool type = false);
        public ExcelWorksheet MergeCells(ExcelWorksheet worksheet, string from);
        public BaseResponseWithData<ExcelWorksheet> SalesOfferItemsReport(List<long> offersId, string CompanyName, DateTime From, DateTime To);

        public BaseResponseWithData<string> SalesOfferReportForVehicle(SalesOfferReportFilter filters, string CompanyName);

        public BaseResponseWithData<string> SalesOfferItemsCategoryReport(string CompanyName, int Year, int Month, long SalesPersonId);

        public bool GetParentCategory(InventoryItemCategory category, long parentId);

        public Task<RejectedOfferScreenDataResponse> RejectedOfferScreenData(long? POID, long? PRID);

        public bool CloseSalesOffer(long SalesOfferId, string CompanyName, long CreatorId);

        public BaseResponseWithId<long> AddNewSalesOffer(AddNewSalesOfferData Request, long creator, string CompanyName);

        public BaseResponseWithId<long> AddNewSalesOfferForInternalTicket(AddNewSalesOfferForInternalTicketRequest Request, string companyname, long creator);

        public BaseResponseWithId<long> ClosingSalesOffer(ClosingSalesOfferData Request, string CompanyName);

        public GetSalesOfferDashboardResponse GetSalesOffersDashboard();

        public GetSalesOfferInternalApprovalResponse GetSalesOfferInternalApproval([FromHeader] long SalesOfferId);

        public BaseResponseWithId<long> AddSalesOfferPricingDetails(AddNewSalesPricingDetailsData Request);

        public BaseResponseWithId<long> AddSalesOfferInternalApproval(AddNewSalesOfferInternalApprovalData Request);

        public BaseResponseWithId<long> DeleteInvoices(DeletedInvoices Request, long creator);
        public BaseResponseWithId<long> RejectClosedSalesOffer(RejectClosedSalesOfferData Request, long creator);

        public GetSalesPersonSalesOfferListResponse GetSalesPersonSalesOfferList(GetSalesPersonSalesOfferListFilters filters, string OfferStatusParam);

        public BaseResponseWithData<List<GetClientJobOrderRate>> ClientsJobOrderRate(SalesOfferReportFilter filters);

        public BaseResponseWithData<string> GetClientOrdeRate(SalesOfferReportFilter filters, string CompanyName);

        public Task<GetTargetOfLast5YearsResponse> GetTargetOfLast5Years();

        public Task<GetRejectedOfferResponse> GetRejectedOffer(GetRejectedOfferFilters filters);
        public Task<GetPrOfferItemHistoryResponse> GetPrOfferItemHistory(long? InventoryItemId);
        public Task<BaseResponseWithID> AddEditRejectedSupplierOffer(AddEditSupplierOfferResponse Request, long UserID);

        public Task<BaseMessageResponse> GetSalesPersonsClientsDetailsExcel([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] bool WithProjectExtraModifications);

        //public BaseResponseWithData<string> GetSalesOfferTicketsForStore(string From, string To, string UserId, string CompName, long createdBy);

        public Task<BaseMessageResponse> GetSalesOfferDetailsPDF([FromHeader] long SalesOfferId);
        public Task<BaseMessageResponse> GetInvoiceDetailsPDF([FromHeader] long SalesOfferId);

        public BaseResponseWithData<string> GetAllClientsAccumulativeByMonths(AccountSalesOfferReportFilter filters);
        public BaseResponseWithData<string> GetAllSuppliersAccumulativeByMonths(AccountSalesOfferReportFilter filters);
        public GetReportStatiscsGroupbyDateResponse GetSalesReportLineStatisticsPerDate(GetSalesReportLineStatisticsPerDateFilters filters);
        public ClientsSalesReportsDetailsResponse SalesReportsDetails(SalesReportsDetailsFilters filters);
        public Task<GetSalesOfferListDDLForReleaseResponse> GetSalesOfferListDDLForRelease(long ClientId, string SearchKey, bool? StatusIsOpenFilter, int CurrentPage = 1, int NumberOfItemsPerPage = 10);

        public BaseResponseWithData<string> GetTotalAmountForEachCategory(GetTotalAmountForEachCategoryFilters filters, string CompName);

        //public BaseResponseWithId<long> AddSalesOfferProductList(AddSalesOfferProductListForMainenance salesOfferProducts, long creator);

        public Task<BaseMessageResponse> SalesOfferExcel(SalesOfferExcelfilters filter);
    }
}
