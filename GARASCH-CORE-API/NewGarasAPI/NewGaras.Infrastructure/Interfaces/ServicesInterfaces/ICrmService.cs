using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.CRM;
using NewGaras.Infrastructure.Models.CRM.Filters;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGaras.Infrastructure.Models.Task.Filters;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface ICrmService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public Task<MyClientsCRMDashboardResponse> GetMyClientsDetailsCRM(GetMyClientsDetailsCRMFilters filters);

        public Task<SalesPersonsClientsDetailsResponse> GetSalesPersonsClientsDetails(GetMyClientsDetailsCRMFilters filters);

        public Task<SalesPersonClientsListResponse> SalesPersonClientsList([FromHeader] long SalesPersonId, [FromHeader] string SupportedBy, [FromHeader] int Month, [FromHeader] int Year);

        public Task<SalesPersonsProductResponse> GetProductsSalesPersons([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] long ProductId);

        public Task<MyOffersCRMDashboardResponse> GetMyOffersDetailsCRM([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] long SalesPersonId);

        public Task<SalesPersonsOffersDetailsResponse> GetSalesPersonOffersDetails([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] long SalesPersonId);

        public Task<MyReportsCRMDashboardResponse> GetMyReportsDetailsCRM(GetMyReportsDetailsCRMFilters filters);

        public ClientsSalesAndCrmReportsDetailsResponse SalesAndCRMReportsDetails(SalesAndCRMReportsFilters filters);

        public ClientsCrmReportsDetailsResponse CRMReportsDetails(CRMReportsDetailsFilters filters);

        public SalesAndCRMAddNewReportScreenData GetSalesAndCRMAddNewReportScreenData();

        public BaseResponseWithData<List<SelectDDL>> GetCrmContactTypesDDL();

        public BaseResponseWithData<List<SelectDDL>> GetCrmRecievedTypesDDL();

        public BaseResponseWithData<List<SelectDDL>> GetDailyReportThroughDDL();

        public BaseResponseWithData<List<SelectDDL>> GeCrmReportReasonsDDL();

        public BaseResponseWithId<long> AddNewCRMReport(AddNewCRMReport Request, long creator);

        public BaseResponseWithId<long> AddNewSalesReport(AddNewSalesReport Request, long creator, string CompanyName);

        public GetSalesReportsListResponse GetSalesReportList(GetSalesReportFilters filters);

        public GetSalesReportsListStatisticsResponse GetSalesReportListStatistics(GetSalesReportFilters filters);

        public GetSalesReportLinesListResponse GetSalesReportLinesList([FromHeader] long SalesReportId);

        public Task<MapSaleReportLinesResponse> GetMapSalesReportLinesList([FromHeader] long SalesReportId, [FromHeader] long SalesPersonId, [FromHeader] int BranchId, [FromHeader] DateTime? CreationFrom, [FromHeader] DateTime? CreationTo);

        public BaseResponseWithId<long> EditSalesReportExpenses(SalesReportLineExpense Request, long creator, string CompanyName);

        public BaseResponse DeleteSalesReportExpenses([FromHeader] long SalesReportExpenseId);

        public BaseResponse DeleteSalesReportLines([FromHeader] long SalesReportlineId);

        public Task<GetCRMReportReasonsResponse> GetCRMReportReasonsList();

        public BaseResponseWithId<long> AddEditCRMReportReasons(GetCRMReportReasonsResponseVM Request);
        public GetReportStatiscsGroupbyDateResponse GetCRMReportLineStatisticsPerDate(GetCRMReportLineStatisticsPerDateFilter filters);

        public GetSalesAndCRMReportStatiscsGroupbyDateResponse GetSalesAndCRMReportLineStatisticsPerDate(GetSalesAndCRMReportLineStatisticsPerDateFilters filters);
        public BaseResponse CRMReportEditReminderStatus([FromHeader] long CrmReportId, [FromHeader] bool Status);
    }
}
