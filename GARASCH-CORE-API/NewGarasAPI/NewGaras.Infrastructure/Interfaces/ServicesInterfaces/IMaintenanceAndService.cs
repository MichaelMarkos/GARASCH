using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Maintenance;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IMaintenanceAndService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public Task<VisitsScheduleMaintenanceByDayResponse> GetMaintenanceByDay(GetMaintenanceByDayFilters filters, string companyname);

        public Task<VisitsScheduleMaintenanceByAreaResponse> GetMaintenanceByArea(GetMaintenanceByAreaFilters filters);
        public Task<VisitsScheduleMaintenanceByYearResponse> GetMaintenanceByMonth(string SearchKey, int Year = 0);
        public Task<GetMaintenanceForByIDResponse> GetMaintenanceByID(string companyname, int ID = 0);
        public string GetInventoryItemCategory(int id);
        public Task<GetMaintenanceForByClientResponse> GetMaintenanceByClient(int ClientID=0);

        public Task<BaseResponseWithID> AddMaintenanceFor(MaintenanceForData Request,long creator);
        public Task<SalesOfferIDAndProjectID> CreateNewSalesOfferAndProject(long LoginUserID, long ClientID, long? InventoryItemID, string ProjectName, string ProjectLocation, ProjectLocationDetails ProjectLocationDetails);

        public Task<BaseResponseWithID> DeleteMaintenanceFor(DeleteMaintenanceRequest Request);

        public Task<MaintenanceForDetailsResponse> GetMaintenanceForDetailsList(MaintenanceDetailsListCallFilters filters, string CompanyName);

        public Task<MaintenanceForDetailsResponse> MaintenanceDetailsListCall(MaintenanceDetailsListCallFilters filters, string CompanyName);
        public List<long> GetNearestSalesOffers(decimal Latitude, decimal Longitude, decimal? Radius);
        public Task<GetManagementOfMaintenanceOrder> GetManagementOfMaintenanceOrderByID(int MaintenanceForID);

        public Task<BaseResponseWithID> AddManagementOfMaintenanceOrder(ManagementOfMaintenanceOrderData Request, long UserID, string CompanyName);

        public Task<BaseMessageResponse> MaintenanceDetailsListCallExcel(MaintenanceDetailsListCallFilters filters, string CompanyName);

        public MaintenanceProductResponse GetMaintenanceList([FromHeader] string Serial);

        public Task<GetVisitsScheduleOfMaintenanceResponse> GetVisitsScheduleOfMaintenanceByID([FromHeader] int ManagementMaintenanceOrderID);

        public Task<GetVisitsScheduleOfMaintenanceResponse> GetPreviousVisitsList([FromHeader] int MaintenanceForID);

        public Task<GetVisitsMaintenanceReportDetailsResponse> GetVisitsReportDetailsList([FromHeader] long MaintVisitID);

        public Task<GetMaintenanceReportExpensesDetailsResponse> GetMaintenanceReportExpensesDetailsList([FromHeader] long MaintenanceReportId);

        public Task<GetMaintenanceVisitsWithoutContractResponse> GetMaintenanceVisitWithoutContract([FromHeader] long MaintenanceVisitId);

        public Task<VisitsScheduleOfMaintenanceWithoutContractResponse> VisitsScheduleOfMaintenanceWithoutContract([FromHeader] long ClientID);

        public Task<GetVisitsScheduleOfMaintenanceResponse> GetVisitsScheduleOfMaintenanceWithoutContractByMaintenanceForID([FromHeader] int MaintenanceForID);

        public VehicleMaintenanceTypeBOM GetVehicleMaintenanceTypeBOM(long BOMId);

        public GetAllMaintenanceTypesResponse GetAllMaintenanceTypes([FromHeader] int VehicleModelId, [FromHeader] int RateId, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10);

        public GetMaintenanceTypeItemResponse GetMaintenanceTypeItemData([FromHeader] int MaintenanceTypeItemId);

        public Task<SelectDDLResponse> GetMaintenanceBrandsList();

        public Task<WorkerStatisticsResponse> GetWorkerstatistics([FromHeader] long AssignToID, [FromHeader] DateTime? VisitDateFrom, [FromHeader] DateTime? VisitDateTo);

        public Task<ClientMaintenanceDetailsResponse> GetClientMaintenanceDetails([FromHeader] long ClientID);

        public NearestClientVisitMaintenanceDetailsResponse GetNearestClientVisitMaintenanceDetails(NearestClientVisitFilters filters);

        public Task<SelectDDLResponse> GetMaintenanceCategpryList();

        public Task<BaseResponseWithId<long>> AddVisitsScheduleOfMaintenance(AddVisitsScheduleOfMaintenanceRequest Request, long userID);

        public Task<BaseResponse> DeleteVisitsScheduleOfMaintenance(DeleteVisitScheduleOfMaintenanceRequest Request);

        public Task<BaseResponseWithId<long>> AddVisitsReportDetailsList(AddVisitsMaintenanceReportDetailsData Request, long userID, string CompanyName);

        public Task<BaseResponseWithId<long>> AddMaintenanceReportExpenses(AddMaintenanceReportExpensesRequest Request, long userID, string CompanyName);

        public Task<BaseResponseWithId<long>> AddEditMaintenanceVisitsWithoutContract(AddVisitsScheduleOfMaintenance Request, long userID, string CompanyName);

        public Task<bool> AddImportantDateForMaintenance(DateTime ImpDate, string Comment);

        public Task<BaseResponseWithId<long>> UpdateReminderDateVisitOfMaintenance(UpdateReminderDateVisitOfMaintenanceRequest Request);

        public BaseResponseWithID AddEditMaintenanceType(AddEditMaintenanceTypeRequest Request, long UserID);

        public ViewAllMaintenanceTypesResponse ViewAllMaintenanceTypes();

        public Task<GetMaintenanceClientsCardsData> GetClientsCardsDataResponse(GetClientsCardsDataResponseFilters filters);

        public BaseResponse DeleteMaintenanceVisit([FromHeader] long VisitId);

        public BaseResponseWithData<string> GetVisitScheduleReport(GetVisitScheduleReportFilters filters);

        public BaseResponse DeleteManagementOfMaintenancee([FromHeader] long ContractId, [FromHeader] bool DeleteContract);

        public BaseResponseWithId<long> AddSalesOfferForMAintenance(MaintenanceOfferDTO dto);

        public BaseResponseWithId<long> UpdateSalesOfferForMAintenance(MaintenanceOfferDTO dto);

        public BaseResponseWithData<List<SelectDDL>> ProductFabricatorDDL();

        public BaseResponseWithData<MaintenanceOfferDTO> GetSalesOfferOfMaintenance([FromHeader] long OfferId);
        public BaseResponseWithId<long> AddSalesOfferProductList(AddSalesOfferProductListForMainenance salesOfferProducts, long creator);
        public BaseResponseWithId<long> EditSalesOfferProductList(EditSalesOfferProductListForMainenance salesOfferProducts, long creator);

        public BaseResponseWithDataAndHeader<List<MaintenanceOfferCardDTO>> GetSalesOfferOfMaintenanceList(GetSalesOfferOfMaintenanceFilters filters);

        public Task<BaseResponseWithData<string>> MaintenanceContractDetailsListExcel(MaintenanceContractDetailsListFilters filters, string CompanyName);

        public BaseResponseWithData<List<string>> GetMaintenanceNameList();

        public BaseResponseWithData<GetOfferStatusSummaryModdel> GetOfferStatusSummary(GetSalesOfferOfMaintenanceFilters filters);

        public BaseResponseWithId<long> AddSalesOfferMaintenanceVisits(AddSalesOfferMaintenanceVisitsDTO dto, long userID);
        public BaseResponseWithData<List<string>> GetVisitsDatesOfScheduleOfMaintenance(long offerID);
    }
}
