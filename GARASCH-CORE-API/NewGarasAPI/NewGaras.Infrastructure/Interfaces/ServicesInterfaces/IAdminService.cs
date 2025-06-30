using Microsoft.Extensions.Configuration;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Admin;
using NewGaras.Infrastructure.Models.Admin.Responses;
using NewGarasAPI.Models.Admin;
using NewGarasAPI.Models.Admin.Responses;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.Project.Headers;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IAdminService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public GetCurrencyResponse GetCurrencyList(string CompanyName = "");

        public Task<BaseResponseWithID> AddNewCurrency(CurrencyData request, long UserID);
        public Task<BaseResponseWithID> AddEditTeamIndex(TeamData request, long UserId);
        public Task<GetExpensisTypeResponse> GetExpensisType();
        public Task<BaseResponseWithID> AddEditExpensisType(ExpensisTypeData request, long UserID);
        public Task<GetIncomeTypeResponse> GetIncomeType();
        public Task<BaseResponseWithID> AddEditIncomeType(IncomeTypeData request, long UserID);
        public Task<GetShippingMethodResponse> GetShippingMethod();
        public Task<BaseResponseWithID> AddEditShippingMethod(ShippingMethodData request);
        public Task<GetDailyReportThroughResponse> GetDailyReportThrough();
        public BaseResponseWithID AddEditDailyReportThrough(DailyReportThroughData request, long UserID);
        public Task<GetDeliveryAndShippingMethodResponse> GetDeliveryAndShippingMethod();
        public BaseResponseWithID AddEditDeliveryAndShippingMethod(DeliveryAndShippingMethodData request, long UserID);
        public Task<GetSalesExtraCostTypeResponse> GetSalesExtraCostType();
        public BaseResponseWithID AddEditSalesExtraCostType(SalesExtraCostTypeData request);
        public Task<GetDailyTransactionToGeneralTypeResponse> GetDailyTransactionToGeneralType();
        public BaseResponseWithID AddEditDailyTransactionToGeneralType(DailyTransactionToGeneralTypeData request, long UserID);
        public BaseResponseWithID AddEditDailyTranactionBeneficiaryToType(DailyTranactionBeneficiaryToTypeData request, long UserID);
        public Task<GetPurchasePOInvoiceTaxIncludedTypeResponse> GetPurchasePOInvoiceTaxIncludedType();
        public Task<GetDailyTranactionBeneficiaryToTypeResponse> GetDailyTranactionBeneficiaryToType();
        public BaseResponseWithID AddEditPurchasePOInvoiceTaxIncludedType(PurchasePOInvoiceTaxIncludedTypeData request);
        public Task<GetPurchasePOInvoiceNotIncludeTaxTypeResponse> GetPurchasePOInvoiceNotIncludeTaxType();
        public BaseResponseWithID AddEditPurchasePOInvoiceNotIncludeTaxType(PurchasePOInvoiceNotIncludeTaxTypeData request);
        public Task<GetPurchasePOInvoiceExtraFeesTypeResponse> GetPurchasePOInvoiceExtraFeesType();
        public BaseResponseWithID AddEditPurchasePOInvoiceExtraFeesType(PurchasePOInvoiceExtraFeesTypeData request);
        public Task<GetPurchasePOInvoiceDeductionTypeResponse> GetPurchasePOInvoiceDeductionType();
        public BaseResponseWithID AddEditPurchasePOInvoiceDeductionType(PurchasePOInvoiceDeductionTypeData request);
        public Task<GetPurchasePaymentMethodResponse> GetPurchasePaymentMethod();
        public BaseResponseWithID AddEditPurchasePaymentMethode(PurchasePaymentMethodData request);
        public Task<GetSpecialityForClientResponse> GetSpecialityForClient();
        public BaseResponseWithID AddEditSpecialityforClient(SpecialityForClientDataList request, long UserId);
        public Task<GetSpecialitySupplierResponse> GetSpecialitySupplier();
        public BaseResponseWithID AddEditSpecialitySupplier(SpecialitySupplierResponseDataList request, long UserId);
        public GetDepartmentResponse GetDepartment([FromHeader] string DepartmentName, [FromHeader] int? BranchID);
        public BaseResponseWithID AddEditDepartment(DepartmentData request, long UserId);
        public GetBranchesResponse GetBranches();
        public BaseResponseWithID AddEditBranches(BranchData request, long UserId);
        public Task<GetTermsAndConditionsResponse> GetTermsAndConditions();
        public BaseResponseWithID AddEditTermsAndConditions(TermsAndConditionsData request, long userId);
        public Task<SelectDDLResponse> GetAreasList(int GovernorateId);
        public BaseResponseWithID AddEditArea(AreaData request, long UserId);
        public BaseResponseWithID AddEditCountry(CountryData request, long UserId);
        public BaseResponseWithID AddEditGovernorate(GovernorateData request, long UserId);
        public  Task<GetCountryGovernorateAreaResponse> GetCountryGovernorateArea(bool allData = true);
        public Task<GetRoleResponse> GetRole();
        public BaseResponseWithID AddGroupRole(AddGroupRoleData request, long UserId);
        public BaseResponseWithID EditGroupRole(EditGroupData request, long UserId);
        public BaseResponseWithID AddEditGroup(GroupData request, long userID);
        public Task<GetGroupRoleResponse> GetGroupDetails([FromHeader] long GroupID = 0);
        public GetGenderResponse GetGender();
        public Task<GetImportantDateResponse> GetImportantDateList([FromHeader] int ImpDateId = 0);
        public Task<BaseResponseWithID> AddImportantDate(AddImportantDateRequest request, long UserId, string CompanyName);
        public GetRoleModuleResponse GetRoleModule(long ModuleID = 0);
        public BaseResponseWithID AddEditRoleModule(RoleModuleData request, long UserId);
        public Task<GetModuleResponse> GetModule();
        public Task<GetTaxResponse> GetTax();
        public BaseResponseWithID AddEditTax(TaxData request, long UserId);
        public GetDBTablesNameResponse GetDBTablesName();
        public GetTablesCloumnsResponse GetTablesCloumns();
        public GetUserListDDLResponse GetUserListDDL(int GroupId, long projectId);
        public BaseResponseWithID AssignPManagerToProject(long projectId, long pManagerId);
        public Task<UserDDLResponse> GetUserList(int BranchId = 0, int RoleId = 0, long GroupId = 0,bool WithTeam=false);
        public Task<GetGroupResponse> GetGroup();
        public Task<BaseResponseWithID> EditEmployeeGroup(EmployeeGroupData request, long UserId);
        public Task<BaseResponseWithID> EditEmployeeGroupNew(EmployeeGroupData request, long UserId);
        public Task<BaseResponseWithData<List<SelectDDL>>> GetTeamList(int? DepartmentId);
        public DashboardResponse GetDashboard();

        public BaseResponseWithId<long> DeleteGroupRole([FromHeader] long GroupId);

        public GetJobTitleResponse GetJobTitle();

        public GetAddEmployeeScreenDataResponse GetAddEmployeeScreenData();

        public BaseResponseWithId<long> AddEditBundleModule(BundleModuleData Request, long creator);
        public Task<GetTermsCategoryResponse> GetTermsCategoryDDL();

        public BaseResponseWithId<int> AddEditTermsCategory(CategoryofTermsandConditionsData Request, long creator);

        public Task<GetSupportedByResponse> GetSupportedByList();

        public Task<BaseResponseWithId<long>> AddEditDeleteSupportedBy(AddEditDeleteSupportedByRequest Request, long creator);

        public BaseResponseWithId<long> DeleteArea([FromHeader] long AreaId);

        public BaseResponseWithId<long> UserSoftDelete([FromHeader] long UserId, [FromHeader] bool Active);

        public BaseResponseWithId<long> AddEditRole(RoleData request);


        public Task<GoogleMapsRespone> MapPlaceDetails([FromHeader] string Url);

        public GetEmployeeListResponse GetEmployeeList(GetEmployeeListFilters filters);

        public Task<BaseResponseWithMessage<string>> TopSellingProductExcel(GetMyProjectsDetailsCRMHeaders headers);

        public Task<GetCostTypeResponse> GetCostType();

        public Task<BaseResponseWithID> AddEditCostType(CostTypeData Request);


    }
}
