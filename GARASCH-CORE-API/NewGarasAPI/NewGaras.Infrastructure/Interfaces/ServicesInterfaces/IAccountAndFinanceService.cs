using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.AccountAndFinance;
using OfficeOpenXml;


namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IAccountAndFinanceService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public AccountsAndFinanceIncomeStatmentResponse GetAccountsAndFinanceIncomeStatment(int? year,int? month,int? day);
        public AccountsAndFinanceIncomeChildrenCalcResponse LoadChildAccount(List<Account> ALLAccountsListDB, int AccCategory, int Year, int Month, int Day);

        public AccountsAndFinanceCashAndBanksResponse GetAccountsAndFinanceCashAndBank(int? year, int? month, int? day);
        public Task<decimal> CurrencyConverterAsync(string from, string to, decimal amount);
        public AccountsDLLResponse GetAccountsList();
        public AccountsDLLResponse GetAccountAdvancedsList();
        public AccountsAndFinanceSpecificTreassuryResponse GetAccountAndFinanceSpecificTreasuryList(long? AccountID);
        public AccountsAndFinanceSpecificTreassury_GroupedResponse GetAccountAndFinanceSpecificTreasuryList_GroupedBy(long? AccountID);
        public AccountsAndFinanceClientReportResponse GetAccountAndFinanceClientReportList(long? ClientID,long? ProjectID, long? ProjectTypeID,DateTime? ProjectCreationFrom,DateTime? ProjectCreationTo);
        public AccountsAndFinanceSupplierReportResponse GetAccountAndFinanceSupplierReportList(long? SupplierID, long? POID, DateTime? POCreationFrom, DateTime? POCreationTo);
        public Task<BaseMessageResponse> AccountTreeExcel([FromHeader] AccountTreeExcelHeader header);
        public Task<DailyJournalEntryGroupingResponse> GetDailyJournalEntryDateList([FromHeader] GetDailyJournalEntryDateListHeader header,long creator);
        public Task<DailyJournalEntryResponse> GetDailyJournalEntryPerDateList([FromHeader] GetDailyJournalEntryPerDateListHader header,long creator);
        public Task<GetCalcDetailsResponse> GetCalcProjectDetails([FromHeader] long ProjectId = 0);
        public Task<GetCalcDetailsResponse> GetCalcPODetails([FromHeader] long POId = 0);
        public Task<DailyJournalEntryDiviededResponse> GetDailyJournalEntryWithFilterList([FromHeader] GetDailyJournalEntryWithFilterListHeader header);

        public void UpdateParentAccountAccumilative(long ParentAccountID, decimal Credit, decimal Debit, decimal Balance);

        public Task<BaseResponseWithID> AddNewDailyJournalEntry(AddNewDailyJournalEntryRequest request,long creator);

        public void UpdateParentAccountAccumilativetest(long ParentAccountID, decimal Balance);
        public BaseResponseWithId UpdateJournalEntryWithAccountAcc();
        public Task<GetCalcDetailsResponse> GetCalcClientCollectedDetails([FromHeader] long ClientId = 0);
        public Task<GetCalcDetailsResponse> GetCalcSupplierCollectedDetails([FromHeader] long SupplierId = 0);
        public Task<BaseResponseWithID> AddReverseDailyJournalEntry(AddReverseDailyJournalEntryRequest request,long creator);
        public Task<BaseResponseWithID> SoftUpdateDailyJournalEntry(AddNewDailyJournalEntryRequest request,long creator);
        public Task<BaseResponseWithId> PermanentDeleteDailyJournalEntry(AddNewDailyJournalEntryRequest request);
        public Task<BaseResponseWithID> UpdateEntryClientAccount(UpdateEntryClientAccountRequest request,long creator);
        public Task<BaseResponseWithID> UpdateEntrySupplierAccount(UpdateEntrySupplierAccountRequest request,long creator);
        public Task<DailyJournalEntryInfoResponse> GetDailyJournalEntryDataInfo([FromHeader] long DailyJournalEntryID = 0, [FromHeader] bool Active = true);

        public bool CheckParentAccountIsActive(List<Account> ParentActiveAccounts, long ParentCategoryID);
        public Task<AccountsEntryDDLResponse> GetAccountsEntryList([FromHeader] long AdvancedTypeId, long creator);

        public Task<GetAccountEntryListByFilterResponse> GetAccountEntryListByFilter([FromHeader] long POID = 0, [FromHeader] long ProjectID = 0, [FromHeader] long OfferID = 0);

        public List<long> GetParentIDSAccounts(List<VAccount> AccountList, long AccountID, List<long> IDSAcc);
        public Task<AccountTreeResponse> GetAccountsTree([FromHeader] bool CalcWithoutPrivate, [FromHeader] bool OrderByCreationDate, [FromHeader] long AccountID, [FromHeader] long AccountCategoryID, [FromHeader] string FromDate, [FromHeader] string DateTo = null);
        public Task<AccountMovementResponse> GetAccountsMovement([FromHeader] GetAccountsMovementHeader header);
        public Task<BaseResponseWithID> AddNewAccount(AddNewAccountRequest request,long creator);
        public Task<BaseResponseWithID> EditAccount(AddNewAccountRequest request,long creator);
        public Task<AccountInfoResponse> GetAccountInfo([FromHeader] long AccountID = 0);
        public BaseResponseWithId DeleteAccount([FromHeader] long AccountId = 0);
        public Task<BaseResponseWithId> AddAndEditFinancialPeriod(FinancialPeriodAccountRequest request,long creator);
        public Task<FinancialPeriodAccountResponse> GetFinancialPeriodList();
        public SelectDDLResponse GetMethodTypeList();
        public SelectDDLResponse GetIncOrExpTypeList([FromHeader] long AccountID = 0);
        public SelectDDLResponse GetBeneficiaryList();
        public SelectDDLResponse GetCostCenterList();
        public SelectAdvancedTypeDDLResponse GetAdvancedTypeList();
        public GetAdvanciedTypeResponse GetAdvancedTypeAccountSettingsList();
        public DailyJournalEntryResponse GetDailyJournalEntryList([FromHeader] long InventoryStoreID = 0, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10, [FromHeader] string ItemSerial = null, [FromHeader] string SearchKey = null);
        public BaseMessageResponse GetAccountsAndFinanceIncomeStatmentPDF(string companyname,[FromHeader] string year = null, [FromHeader] string month = null, [FromHeader] string day = null);

        public List<AccountBalancePerExpOrIncType> AccountBalancePerExpOrIncType(List<long> AccountIdList, int Year, int CategoryType);

        public BaseMessageResponse GetAccountAnnualReportPDF(string companyname,[FromHeader] int year = 0, [FromHeader] string AccountID = null);

        public Task<BaseMessageResponse> BalanceSheetPDF([FromHeader] AccountTreeExcelHeader header,long userID, string companyname);

        public Task<BaseMessageResponse> JournalEntryPDFReport(string companyname,[FromHeader] long DailyJournalEntryID = 0);

        public BaseMessageResponse AccountMovementReports([FromHeader] AccountMovementReportsHeader header, HttpRequest Request);

        public ExcelWorksheet MergeCells(ExcelWorksheet worksheet,string from);
        public BaseResponseWithData<string> SalesOfferItemsReport(string CompanyName, int Year, int Month, long SalesPersonId);
        public BaseResponseWithData<string> AccoutingStatementOfSalesOffers(string CompanyName, int BranchId, int Year);
        public BaseResponseWithData<string> RentalEquipmentsReport(string CompanyName, int Year, int Month);

        public BaseResponseWithData<string> Vault10Percent(string CompanyName, int Year, int Month);

        public AccountAndFinanceDashboardResponse GetAccountAndFinanceDashboard();
    }
}
