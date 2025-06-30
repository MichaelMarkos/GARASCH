using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.Common;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using NewGarasAPI.Models.Supplier;
using System.Data;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.Data.SqlClient;
using NewGarasAPI.Models.HR;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NewGarasAPI.Models.AccountAndFinance;
using Font = iTextSharp.text.Font;
using Paragraph = iTextSharp.text.Paragraph;
using Image = iTextSharp.text.Image;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Web;
using NewGarasAPI.Models.User;
using NewGarasAPI.Models.Purchase;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Models.AccountAndFinance;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Helper.TenantService;
using System.IO;
using MimeKit;
using NewGaras.Domain.Models;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Identity.Client;
using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.DTO.AssetDepreciation;
using NewGaras.Infrastructure.DTO.Family.Filters;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountAndFinanceController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        static string key = "SalesGarasPass";
        public string BaseCurrencyConverterApiAddress;
        public string CurrencyConvertorAddress;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService; 
        private readonly IAccountAndFinanceService _accountAndFinance;
        private readonly IAssetDepreciationService _assetDepreciationService;
        public AccountAndFinanceController(IWebHostEnvironment host,ITenantService tenantService,IAccountAndFinanceService accountAndFinance, IAssetDepreciationService assetDepreciationService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            BaseCurrencyConverterApiAddress = "https://api.exchangerate.host/";
            CurrencyConvertorAddress = "convert?format=json";
            _host = host;
            _accountAndFinance = accountAndFinance;
            _assetDepreciationService = assetDepreciationService;
        }

        [HttpGet("GetAccountsAndFinanceIncomeStatment")]
        public AccountsAndFinanceIncomeStatmentResponse GetAccountsAndFinanceIncomeStatment([FromHeader] int? year, [FromHeader] int? month, [FromHeader] int? day)
        {
            AccountsAndFinanceIncomeStatmentResponse Response = new AccountsAndFinanceIncomeStatmentResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountsAndFinanceIncomeStatment(year, month, day);
                }

                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAccountsAndFinanceCashAndBank")]
        public AccountsAndFinanceCashAndBanksResponse GetAccountsAndFinanceCashAndBank([FromHeader] int? year, [FromHeader] int? month, [FromHeader] int? day)
        {
            AccountsAndFinanceCashAndBanksResponse Response = new AccountsAndFinanceCashAndBanksResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountsAndFinanceCashAndBank(year, month, day);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAccountsList")]
        public AccountsDLLResponse GetAccountsList()
        {
            AccountsDLLResponse Response = new AccountsDLLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountsList();
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAccountAdvancedsList")]
        public AccountsDLLResponse GetAccountAdvancedsList()
        {
            AccountsDLLResponse Response = new AccountsDLLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                var AccountList = new List<AccountsDLL>();
                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountAdvancedsList();
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAccountAndFinanceSpecificTreasuryList")]
        public AccountsAndFinanceSpecificTreassuryResponse GetAccountAndFinanceSpecificTreasuryList([FromHeader] long? AccountID)
        {
            AccountsAndFinanceSpecificTreassuryResponse Response = new AccountsAndFinanceSpecificTreassuryResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountAndFinanceSpecificTreasuryList(AccountID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }
        [HttpGet("GetAccountAndFinanceSpecificTreasuryList_GroupedBy")]
        public AccountsAndFinanceSpecificTreassury_GroupedResponse GetAccountAndFinanceSpecificTreasuryList_GroupedBy([FromHeader] long? AccountID)
        {
            AccountsAndFinanceSpecificTreassury_GroupedResponse Response = new AccountsAndFinanceSpecificTreassury_GroupedResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountAndFinanceSpecificTreasuryList_GroupedBy(AccountID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAccountAndFinanceClientReportList")]
        public AccountsAndFinanceClientReportResponse GetAccountAndFinanceClientReportList([FromHeader] long? ClientID, [FromHeader] long? ProjectID, [FromHeader] long? ProjectTypeID, [FromHeader] DateTime? ProjectCreationFrom, [FromHeader] DateTime? ProjectCreationTo)
        {
            AccountsAndFinanceClientReportResponse Response = new AccountsAndFinanceClientReportResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountAndFinanceClientReportList(ClientID, ProjectID, ProjectTypeID, ProjectCreationFrom, ProjectCreationTo);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAccountAndFinanceSupplierReportList")]
        public AccountsAndFinanceSupplierReportResponse GetAccountAndFinanceSupplierReportList([FromHeader]long? SupplierID, [FromHeader]long? POID,[FromHeader] DateTime? POCreationFrom, [FromHeader] DateTime? POCreationTo)
        {
            AccountsAndFinanceSupplierReportResponse Response = new AccountsAndFinanceSupplierReportResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountAndFinanceSupplierReportList(SupplierID, POID, POCreationFrom, POCreationTo);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        /*private AccountsAndFinanceIncomeChildrenCalcResponse LoadChildAccount(List<NewGarasAPI.NewGaras.Infrastructure.Entities.Account> ALLAccountsListDB, long parentID, int Year, int Month, int Day)
        {

            var AccountAndFinanceIncome = new List<AccountsAndFinanceDetails>();
            var ParentCategoryIDList = ALLAccountsListDB.Where(x => x.ParentCategory == 0 && x.AccountCategoryId == parentID).Select(x => x.Id).ToList();
            // modified 2023-5-10-mic
            var AccountsListDB = ALLAccountsListDB.Where(x => x.ParentCategory != null  && ParentCategoryIDList.Contains((long)x.ParentCategory) && x.AccountCategoryId == parentID).ToList();
            //var AccountsListDB = _Context.proc_AccountsLoadAll().Where(x => x.ParentCategory == parentID).ToList();
            //if (Month != 0)
            //{
            //    AccountsListDB = AccountsListDB.Where(x => x.CreationDate.Month == Month).ToList();
            //}
            //if (Day != 0)
            //{
            //    AccountsListDB = AccountsListDB.Where(x => x.CreationDate.Day == Day).ToList();
            //}
            decimal TotalSumChildrentToParent = 0;
            if (AccountsListDB.Count > 0)
            {
                // 2023-5-10 mic
                var TotalAccountOfJournalEntry = _Context.AccountOfJournalEntries.Where(x => x.EntryDate.Year == Year && x.Active == true &&
                                                                                        (x.DtmainType == "Income" || x.DtmainType == "Expenses")
                                                                                        ).ToList();
                foreach (var account in AccountsListDB)
                {
                    var AccountsAndFinanceObj = new AccountsAndFinanceDetails();
                    var BalancePerMonthList = new List<BalancePerMonth>();
                    decimal Accumulative = 0;
                    string AccumulativeType = "";
                    if (!account.Haveitem)
                    {
                        var AcumelativeListFromEntry = TotalAccountOfJournalEntry.Where(x => x.AccountId == account.Id).ToList();
                        //var AcumelativeListFromEntry = _Context.AccountOfJournalEntries.Where(x => x.EntryDate.TargetYear == TargetYear && x.AccountID == account.ID
                        //&& x.Active == true &&
                        //                                                                            (x.DTMainType == "Income" || x.DTMainType == "Expenses")).ToList();
                        for (int MonthNO = 1; MonthNO <= 12; MonthNO++)
                        {
                            var AcumelativeListFromEntryPerMonth = AcumelativeListFromEntry.Where(x => x.EntryDate.Month == MonthNO).ToList();
                            var AccumulativePerMonth = AcumelativeListFromEntryPerMonth.Select(x => x.Amount).Sum();

                            string AccumulativeTypePerMonth = "";

                            if (AccumulativePerMonth > 0)
                            {
                                AccumulativeTypePerMonth = "C";
                            }
                            else if (AccumulativePerMonth < 0)
                            {
                                AccumulativeTypePerMonth = "D";
                            }

                            var BalanceAmountPerMonth = Math.Abs(AccumulativePerMonth).ToString() + " " + AccumulativeTypePerMonth;
                            BalancePerMonthList.Add(new BalancePerMonth
                            {
                                Accumulative = AccumulativePerMonth,
                                BalanceAmount = BalanceAmountPerMonth,
                                MonthNo = MonthNO
                            });


                        }



                        if (Month != 0)
                        {
                            AcumelativeListFromEntry = AcumelativeListFromEntry.Where(x => x.EntryDate.Month == Month).ToList();
                        }
                        if (Day != 0)
                        {
                            AcumelativeListFromEntry = AcumelativeListFromEntry.Where(x => x.EntryDate.Day == Day).ToList();
                        }
                        Accumulative = AcumelativeListFromEntry.Select(x => x.Amount).Sum();

                    }
                    else if (account.Haveitem)
                    {
                        var Request = LoadChildAccount(ALLAccountsListDB, account.Id, Year, Month, Day);
                        AccountsAndFinanceObj.AccountsAndFinanceChildList = Request.AccountsAndFinanceDetailsList;
                        Accumulative = Request.TotalsumChildren;
                    }

                    if (Accumulative > 0)
                    {
                        AccumulativeType = "C";
                    }
                    else if (Accumulative < 0)
                    {
                        AccumulativeType = "D";
                    }
                    AccountsAndFinanceObj.BalancePerMonthList = BalancePerMonthList;
                    AccountsAndFinanceObj.AccountID = account.Id;
                    AccountsAndFinanceObj.AccountName = account.AccountName;
                    AccountsAndFinanceObj.HaveChild = account.Haveitem;
                    AccountsAndFinanceObj.CurrencyName = account.CurrencyId != null ? Common.GetCurrencyName((int)account.CurrencyId, _Context) : "";
                    AccountsAndFinanceObj.BalanceAmount = Math.Abs(Accumulative).ToString() + " " + AccumulativeType;
                    AccountsAndFinanceObj.Accumulative = Accumulative;
                    AccountsAndFinanceObj.ParentCategoryID = account.ParentCategory;
                    TotalSumChildrentToParent += Accumulative;
                    AccountAndFinanceIncome.Add(AccountsAndFinanceObj);
                }
            }
            var Response = new AccountsAndFinanceIncomeChildrenCalcResponse();
            Response.AccountsAndFinanceDetailsList = AccountAndFinanceIncome;
            Response.TotalsumChildren = TotalSumChildrentToParent;
            return Response;
        }*/
        private AccountsAndFinanceIncomeChildrenCalcResponse LoadChildAccount(List<Account> ALLAccountsListDB, int AccCategory, int Year, int Month, int Day)
        {
            var AccountAndFinanceIncome = new List<AccountsAndFinanceDetails>();
            var ParentCategoryIDList = ALLAccountsListDB.Where(x => x.ParentCategory == 0 && x.AccountCategoryId == AccCategory).Select(x => x.Id).ToList();
            //if (ParentAccountID != null)
            //{
            //    ParentCategoryIDList = ALLAccountsListDB.Where(x => x.ParentCategory == ParentAccountID).Select(x => x.ID).ToList();
            //}
            var AccountsListDB = ALLAccountsListDB.Where(x => x.ParentCategory != null && ParentCategoryIDList.Contains((long)x.ParentCategory) && x.AccountCategoryId == AccCategory).ToList();
            //if (Month != 0)   
            //{
            //    AccountsListDB = AccountsListDB.Where(x => x.CreationDate.Month == Month).ToList();
            //}
            //if (Day != 0)
            //{
            //    AccountsListDB = AccountsListDB.Where(x => x.CreationDate.Day == Day).ToList();
            //}
            decimal TotalSumChildrentToParent = 0;
            if (AccountsListDB.Count > 0)
            {
                // 2023-5-10 mic
                var TotalAccountOfJournalEntry = _Context.AccountOfJournalEntries.Where(x => x.EntryDate.Year == Year && x.Active == true &&
                                                                                        (x.DtmainType == "Income" || x.DtmainType == "Expenses")).ToList();
                foreach (var account in AccountsListDB)
                {
                    var AccountsAndFinanceObj = new AccountsAndFinanceDetails();
                    var BalancePerMonthList = new List<BalancePerMonth>();
                    decimal Accumulative = 0;
                    string AccumulativeType = "";
                    //if (!account.Haveitem)
                    //{
                    var AcumelativeListFromEntry = TotalAccountOfJournalEntry.Where(x => x.AccountId == account.Id).ToList();

                    //var AcumelativeListFromEntry = _Context.AccountOfJournalEntries.Where(x => x.EntryDate.Year == Year && x.AccountID == account.ID && x.Active == true &&
                    //                                                                            (x.DTMainType == "Income" || x.DTMainType == "Expenses")).ToList();
                    if (account.Haveitem)
                    {
                        var ListAccountChildrenList = ALLAccountsListDB.Where(x => x.ParentCategory == account.Id).Select(x => x.Id).ToList();
                        AcumelativeListFromEntry = TotalAccountOfJournalEntry.Where(x => ListAccountChildrenList.Contains(x.AccountId)).ToList();

                        //var Request = LoadChildAccount(ALLAccountsListDB, account.ID, AccCategory, Year, Month, Day);
                        //AccountsAndFinanceObj.AccountsAndFinanceChildList = Request.AccountsAndFinanceDetailsList;
                        //Accumulative = Request.TotalsumChildren;
                    }
                    for (int MonthNO = 1; MonthNO <= 12; MonthNO++)
                    {
                        var AcumelativeListFromEntryPerMonth = AcumelativeListFromEntry.Where(x => x.EntryDate.Month == MonthNO).ToList();
                        var AccumulativePerMonth = AcumelativeListFromEntryPerMonth.Select(x => x.Amount).Sum();

                        string AccumulativeTypePerMonth = "";

                        if (AccumulativePerMonth > 0)
                        {
                            AccumulativeTypePerMonth = "C";
                        }
                        else if (AccumulativePerMonth < 0)
                        {
                            AccumulativeTypePerMonth = "D";
                        }

                        var BalanceAmountPerMonth = Math.Abs(AccumulativePerMonth).ToString() + " " + AccumulativeTypePerMonth;
                        BalancePerMonthList.Add(new BalancePerMonth
                        {
                            Accumulative = AccumulativePerMonth,
                            BalanceAmount = BalanceAmountPerMonth,
                            MonthNo = MonthNO
                        });


                    }



                    if (Month != 0)
                    {
                        AcumelativeListFromEntry = AcumelativeListFromEntry.Where(x => x.EntryDate.Month == Month).ToList();
                    }
                    if (Day != 0)
                    {
                        AcumelativeListFromEntry = AcumelativeListFromEntry.Where(x => x.EntryDate.Day == Day).ToList();
                    }
                    Accumulative = AcumelativeListFromEntry.Select(x => x.Amount).Sum();

                    //}
                    //else if (account.Haveitem)
                    //{
                    //    var Request = LoadChildAccount(ALLAccountsListDB, account.ID,AccCategory, Year, Month, Day);
                    //    AccountsAndFinanceObj.AccountsAndFinanceChildList = Request.AccountsAndFinanceDetailsList;
                    //    Accumulative = Request.TotalsumChildren;
                    //}


                    if (Accumulative > 0)
                    {
                        AccumulativeType = "C";
                    }
                    else if (Accumulative < 0)
                    {
                        AccumulativeType = "D";
                    }
                    AccountsAndFinanceObj.BalancePerMonthList = BalancePerMonthList;
                    AccountsAndFinanceObj.AccountID = account.Id;
                    AccountsAndFinanceObj.AccountName = account.AccountName;
                    AccountsAndFinanceObj.HaveChild = account.Haveitem;
                    AccountsAndFinanceObj.CurrencyName = account.CurrencyId != null ? Common.GetCurrencyName((int)account.CurrencyId,_Context) : "";
                    AccountsAndFinanceObj.BalanceAmount = Math.Abs(Accumulative).ToString() + " " + AccumulativeType;
                    AccountsAndFinanceObj.Accumulative = Accumulative;
                    AccountsAndFinanceObj.ParentCategoryID = account.ParentCategory;
                    TotalSumChildrentToParent += Accumulative;
                    AccountAndFinanceIncome.Add(AccountsAndFinanceObj);
                }
            }
            var Response = new AccountsAndFinanceIncomeChildrenCalcResponse();
            Response.AccountsAndFinanceDetailsList = AccountAndFinanceIncome;
            Response.TotalsumChildren = TotalSumChildrentToParent;
            return Response;
        }

        private async Task<decimal> CurrencyConverterAsync(string from, string to, decimal amount)
        {
            decimal result = 0;
            try
            {


                #region for exchangerate api with URL "https://exchangerate.host/"
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //using (var client = new HttpClient())
                //{
                //    client.BaseAddress = new Uri(BaseExchangeRateAddress);
                //    var GetRequest = client.GetAsync(ExchangeRateConvertorAddress + "?from=" + from + "&to=" + to + "&amount=" + amount).Result;

                //    if (GetRequest.IsSuccessStatusCode)
                //    {
                //        var ResponseJsonString = GetRequest.Content.ReadAsStringAsync().Result;
                //        var ResponseJsonObject = JsonConvert.DeserializeObject<CurrencyConvertorVM>(ResponseJsonString);
                //        if (ResponseJsonObject.result != 0)
                //        {
                //            result = (decimal)ResponseJsonObject.result;
                //            return result;
                //        }
                //    }
                //}
                //return 0;
                #endregion

                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(BaseCurrencyConverterApiAddress + CurrencyConvertorAddress + "&from=" + from + "&to=" + to + "&amount=" + amount),
                    //Headers =
                    //            {
                    //                { "x-rapidapi-key", "c37692046bmshb7315005e259134p193ce9jsnbb7c59a57b4f" },
                    //                { "x-rapidapi-host", "currency-converter5.p.rapidapi.com" }
                    //            },
                };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    dynamic d = new { value1 = to };

                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var ResponseJsonObject = JsonConvert.DeserializeObject<CurrencyConverter>(body);
                    //  var preresult = ResponseJsonObject.rates.GetType().GetProperty(to).GetValue(ResponseJsonObject.rates, null).GetType().GetProperty("rate_for_amount").GetValue(ResponseJsonObject.rates.GetType().GetProperty(to).GetValue(ResponseJsonObject.rates, null), null);
                    var preresult = ResponseJsonObject.result;
                    result = decimal.Parse(preresult.ToString());
                    return result;
                }



            }
            catch (Exception ex)
            {

                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                return 0;
            }
            return result;
        }
        [HttpGet("AccountTreeExcel")]
        public async Task<BaseMessageResponse> AccountTreeExcel([FromHeader] AccountTreeExcelHeader header)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response =await _accountAndFinance.AccountTreeExcel(header);
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetDailyJournalEntryDateList")]
        public async Task<ActionResult<DailyJournalEntryGroupingResponse>> GetDailyJournalEntryDateList([FromHeader] GetDailyJournalEntryDateListHeader header)
        {
            var Response = new DailyJournalEntryGroupingResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                //var DailyJournalEntryList = new List<DailyJournalEntry>();
                var DailyJournalEntryListByDate = new List<DailyJournalEntryGroupingByDate>();
                //int NoOfItems = 0;
                if (Response.Result)
                {
                    _accountAndFinance.Validation = validation;
                    Response =await _accountAndFinance.GetDailyJournalEntryDateList(header, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetDailyJournalEntryPerDateList")]
        public async Task<ActionResult<DailyJournalEntryResponse>> GetDailyJournalEntryPerDateList([FromHeader] GetDailyJournalEntryPerDateListHader header)
        {

            DailyJournalEntryResponse Response = new DailyJournalEntryResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response =await _accountAndFinance.GetDailyJournalEntryPerDateList(header, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetCalcProjectDetails")]
        public async Task<GetCalcDetailsResponse> GetCalcProjectDetails([FromHeader] long ProjectId = 0)
        {
            GetCalcDetailsResponse Response = new GetCalcDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response =await _accountAndFinance.GetCalcProjectDetails(ProjectId);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetCalcPODetails")]
        public async Task<GetCalcDetailsResponse> GetCalcPODetails([FromHeader] long POId = 0)
        {
            GetCalcDetailsResponse Response = new GetCalcDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response =await _accountAndFinance.GetCalcPODetails(POId);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetDailyJournalEntryWithFilterList")]
        public async Task<DailyJournalEntryDiviededResponse> GetDailyJournalEntryWithFilterList([FromHeader] GetDailyJournalEntryWithFilterListHeader header)
        {
            DailyJournalEntryDiviededResponse Response = new DailyJournalEntryDiviededResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                var AutoJEList = new List<DailyJournalEntryView>();
                var ReverseAndDeleteJEList = new List<DailyJournalEntryView>();
                var GeneralJEList = new List<DailyJournalEntryView>();
                //int NoOfItems = 0;

                if (Response.Result)
                {
                    Response =await _accountAndFinance.GetDailyJournalEntryWithFilterList(header);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [NonAction]
        public void UpdateParentAccountAccumilative(long ParentAccountID, decimal Credit, decimal Debit, decimal Balance)
        {
            var ParentAccountDB = _Context.Accounts.Where(x => x.Id == ParentAccountID).FirstOrDefault();
            if (ParentAccountDB != null)
            {
                ParentAccountDB.Credit += Credit;
                ParentAccountDB.Debit += Debit;
                ParentAccountDB.Accumulative += Balance;

                _Context.SaveChanges();

                UpdateParentAccountAccumilative(ParentAccountDB.ParentCategory ?? 0, Credit, Debit, Balance);
            }
        }
        [HttpPost("AddNewDailyJournalEntry")]
        public async Task<BaseResponseWithID> AddNewDailyJournalEntry(AddNewDailyJournalEntryRequest request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _accountAndFinance.AddNewDailyJournalEntry(request, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [NonAction]
        public void UpdateParentAccountAccumilativetest(long ParentAccountID, decimal Balance)
        {
            var ParentAccountDB = _Context.Accounts.Where(x => x.Id == ParentAccountID).FirstOrDefault();
            if (ParentAccountDB != null)
            {
                ParentAccountDB.Accumulative += Balance;

                _Context.SaveChanges();

                UpdateParentAccountAccumilativetest(ParentAccountDB.ParentCategory ?? 0, Balance);
            }
        }
        [HttpPost("UpdateJournalEntryWithAccountAcc")]
        public BaseResponseWithId UpdateJournalEntryWithAccountAcc()
        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {



                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                //long AccountId = 0;
                //if (!string.IsNullOrEmpty(headers["AccountId"]) && long.TryParse(headers["AccountId"], out AccountId))
                //{
                //    long.TryParse(headers["AccountId"], out AccountId);
                //}
                if (Response.Result)
                {

                    var AccountList = _Context.Accounts.Where(x => x.Active == true).ToList();
                    foreach (var AccountId in AccountList.Select(x => x.Id).ToList())
                    {
                        var OldDateTime = DateTime.Now.AddYears(-4);
                        var FuterDateTime = DateTime.Now.AddMonths(1);
                        var AccountID = new SqlParameter("AccountID", System.Data.SqlDbType.BigInt);
                        AccountID.Value = AccountId;
                        var CalcWithoutPrivateJE = new SqlParameter("CalcWithoutPrivateJE", System.Data.SqlDbType.Bit);
                        CalcWithoutPrivateJE.Value = true;
                        var OrderByCreationDate = new SqlParameter("OrderByCreationDate", System.Data.SqlDbType.Bit);
                        OrderByCreationDate.Value = true;
                        var CreationDateFrom = new SqlParameter("CreationDateFrom", System.Data.SqlDbType.DateTime);
                        CreationDateFrom.Value = OldDateTime;
                        var CreationDateTo = new SqlParameter("CreationDateTo ", System.Data.SqlDbType.DateTime);
                        CreationDateTo.Value = FuterDateTime;
                        object[] param = new object[] { AccountID, CalcWithoutPrivateJE, OrderByCreationDate, CreationDateFrom, CreationDateTo };
                        var ListAcc = _Context.Database.SqlQueryRaw<STP_AccountMovement_Result>("Exec STP_AccountMovement @AccountID ,@CalcWithoutPrivateJE ,@OrderByCreationDate ,@CreationDateFrom ,@CreationDateTo", param).AsEnumerable();
                        var AccountObj = AccountList.Where(x => x.Id == AccountId).FirstOrDefault();
                        AccountObj.Accumulative = ListAcc.OrderByDescending(a=>a.CreationDate).FirstOrDefault()?.Acc_Calc ?? 0;
                        var res = _Context.SaveChanges();
                        if (AccountObj.ParentCategory != null && AccountObj.ParentCategory != 0 && res > 0)
                        {
                            UpdateParentAccountAccumilativetest(AccountObj.ParentCategory ?? 0, AccountObj.Accumulative);
                        }
                        //var AccOfJounalEntry =  _Context.AccountOfJournalEntries.Where(x => x.AccountID == AccountId && x.Active == true).OrderBy(x => x.CreationDate).ToList();
                        //decimal Accumulative = 0;
                        // var AccountObjDB =  _Context.Accounts.Where(x => x.ID == AccountId).FirstOrDefault();
                        //foreach (var item in AccOfJounalEntry)
                        //{

                        //    //if (item.Account != null)
                        //    //{
                        //    //    string FromOrTo = "";
                        //    //    decimal Credit = 0;
                        //    //    decimal Debit = 0;
                        //    //    decimal Amount = 0;
                        //    //    decimal AmountForAccumulative = 0;
                        //    //    decimal AccBalance = item.Account.Accumulative;
                        //    //    if (item.SignOfAccount == "minus")
                        //    //    {
                        //    //        //AmountForAccumulative = item.Amount;
                        //    //        FromOrTo = "From";
                        //    //    }
                        //    //    else if (item.SignOfAccount == "plus")
                        //    //    {
                        //    //        // AmountForAccumulative = -1 * item.Amount;
                        //    //        FromOrTo = "To";
                        //    //    }

                        //    //    if (item.Account.AccountTypeName == "Credit")
                        //    //    {
                        //    //        if (item.SignOfAccount == "plus")
                        //    //        {
                        //    //            Credit = Math.Abs(item.Amount);
                        //    //            Debit = 0;
                        //    //            Amount = -1 * Math.Abs(item.Amount);

                        //    //        }
                        //    //        else if (item.SignOfAccount == "minus")
                        //    //        {
                        //    //            Credit = 0;
                        //    //            Debit = Math.Abs(item.Amount);
                        //    //            Amount = Math.Abs(item.Amount);
                        //    //        }
                        //    //    }
                        //    //    else if (item.Account.AccountTypeName == "Debit")
                        //    //    {
                        //    //        if (item.SignOfAccount == "plus")
                        //    //        {
                        //    //            Credit = 0;
                        //    //            Debit = Math.Abs(item.Amount);
                        //    //            Amount = Math.Abs(item.Amount);
                        //    //        }
                        //    //        else if (item.SignOfAccount == "minus")
                        //    //        {
                        //    //            Credit = Math.Abs(item.Amount);
                        //    //            Debit = 0;
                        //    //            Amount = -1 * Math.Abs(item.Amount);
                        //    //        }
                        //    //    }
                        //    //    AmountForAccumulative = Amount;
                        //    //    AccBalance = AccBalance + AmountForAccumulative;


                        //    //    item.AccBalance = AccBalance;




                        //    //    _Context.SaveChanges();

                        //    //    Accumulative += AccBalance;
                        //    //}



                        //}
                        //       // Update Accounts
                        //       AccountObjDB.Accumulative = Accumulative;
                        //       // AccountObjDB.ModifiedBy = validation.userID;
                        //       AccountObjDB.ModifiedDate = DateTime.Now;
                        //_Context.SaveChanges();

                        // UpdateParentAccountAccumilativetest(AccountObjDB.ParentCategory ?? 0, Accumulative);

                    }

                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetCalcClientCollectedDetails")]
        public async Task<GetCalcDetailsResponse> GetCalcClientCollectedDetails([FromHeader] long ClientId = 0)
        {
            GetCalcDetailsResponse Response = new GetCalcDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _accountAndFinance.GetCalcClientCollectedDetails(ClientId);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetCalcSupplierCollectedDetails")]
        public async Task<GetCalcDetailsResponse> GetCalcSupplierCollectedDetails([FromHeader] long SupplierId = 0)
        {
            GetCalcDetailsResponse Response = new GetCalcDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    //long SupplierId = 0;
                    if (SupplierId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Supplier Id";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var SupplierObjDB = await _Context.Suppliers.Where(x => x.Id == SupplierId).FirstOrDefaultAsync();
                    if (SupplierObjDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P212";
                        error.ErrorMSG = "Invalid Supplier Id";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var TotalPOAmount = SupplierObjDB.PurchasePos.Sum(x => x.PurchasePoinvoices.Where(y => y.Active == true).Select(t => t.TotalInvoiceCost).Sum());
                    Response.TotalCollected = SupplierObjDB.SupplierAccounts.Where(x => x.Active == true).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                    Response.TotalAmount = TotalPOAmount ?? 0;
                    Response.Remain = Response.TotalAmount > Response.TotalCollected ? Response.TotalAmount - Response.TotalCollected : 0;



                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("AddReverseDailyJournalEntry")]
        public async Task<BaseResponseWithID> AddReverseDailyJournalEntry(AddReverseDailyJournalEntryRequest request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response =await _accountAndFinance.AddReverseDailyJournalEntry(request, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("SoftUpdateDailyJournalEntry")]
        public async Task<BaseResponseWithID> SoftUpdateDailyJournalEntry(AddNewDailyJournalEntryRequest request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _accountAndFinance.SoftUpdateDailyJournalEntry(request, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("PermanentDeleteDailyJournalEntry")]
        public async Task<BaseResponseWithId> PermanentDeleteDailyJournalEntry(AddNewDailyJournalEntryRequest request)
        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = false;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response =await _accountAndFinance.PermanentDeleteDailyJournalEntry(request);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? (ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message) : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("UpdateEntryClientAccount")]
        public async Task<BaseResponseWithID> UpdateEntryClientAccount(UpdateEntryClientAccountRequest request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _accountAndFinance.UpdateEntryClientAccount(request, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("UpdateEntrySupplierAccount")]
        public async Task<BaseResponseWithID> UpdateEntrySupplierAccount(UpdateEntrySupplierAccountRequest request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response =await _accountAndFinance.UpdateEntrySupplierAccount(request, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetDailyJournalEntryDataInfo")]
        public async Task<DailyJournalEntryInfoResponse> GetDailyJournalEntryDataInfo([FromHeader] long DailyJournalEntryID = 0, [FromHeader] bool Active = true)
        {
            DailyJournalEntryInfoResponse Response = new DailyJournalEntryInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = await _accountAndFinance.GetDailyJournalEntryDataInfo(DailyJournalEntryID, Active);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [NonAction]
        public bool CheckParentAccountIsActive(List<Account> ParentActiveAccounts, long ParentCategoryID)
        {
            bool Res = false;
            if (ParentActiveAccounts.Count > 0)
            {
                if (ParentCategoryID == 0)
                {
                    Res = true;

                }
                else
                {
                    var ParentAccountObj = ParentActiveAccounts.Where(x => x.Id == ParentCategoryID).FirstOrDefault();
                    if (ParentAccountObj != null)
                    {
                        if (ParentAccountObj.ParentCategory == 0)
                        {
                            Res = true;
                        }
                        else
                        {
                            Res = CheckParentAccountIsActive(ParentActiveAccounts, ParentAccountObj.ParentCategory ?? 0);
                        }
                    }
                }
            }
            return Res;
        }
        [HttpGet("GetAccountsEntryList")]
        public async Task<AccountsEntryDDLResponse> GetAccountsEntryList([FromHeader] long AdvancedTypeId)
        {
            AccountsEntryDDLResponse Response = new AccountsEntryDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _accountAndFinance.GetAccountsEntryList(AdvancedTypeId,validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAccountEntryListByFilter")]
        public async Task<GetAccountEntryListByFilterResponse> GetAccountEntryListByFilter([FromHeader] long POID = 0, [FromHeader] long ProjectID = 0, [FromHeader] long OfferID = 0)
        {
            GetAccountEntryListByFilterResponse Response = new GetAccountEntryListByFilterResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                
                if (Response.Result)
                {
                    Response = await _accountAndFinance.GetAccountEntryListByFilter(POID, ProjectID, OfferID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [NonAction]
        public List<long> GetParentIDSAccounts(List<VAccount> AccountList, long AccountID, List<long> IDSAcc)
        {
            //List<long> IDSAcc = new List<long>();
            if (AccountID != 0)
            {
                IDSAcc.Add(AccountID);
                var IDAccount = AccountList.Where(x => x.Id == AccountID).Select(x => x.ParentCategory ?? 0).FirstOrDefault();
                IDSAcc.Add(IDAccount);
                if (IDAccount != 0)
                {
                    IDSAcc.AddRange(GetParentIDSAccounts(AccountList, IDAccount, IDSAcc));
                }
            }
            return IDSAcc;
        }

        [HttpGet("GetAccountsTree")]
        public async Task<AccountTreeResponse> GetAccountsTree([FromHeader] bool CalcWithoutPrivate , [FromHeader] bool OrderByCreationDate , [FromHeader] long AccountID, [FromHeader] long AccountCategoryID, [FromHeader] string FromDate, [FromHeader] string DateTo)
        {

            AccountTreeResponse response = new AccountTreeResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _accountAndFinance.GetAccountsTree(CalcWithoutPrivate, OrderByCreationDate, AccountID, AccountCategoryID,FromDate,DateTo);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        [HttpGet("GetAccountsMovement")]
        public async Task<AccountMovementResponse> GetAccountsMovement([FromHeader] GetAccountsMovementHeader header)
        {
            AccountMovementResponse response = new AccountMovementResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var GetAccountMovementList = new List<AccountOfMovement>();
                if (response.Result)
                {
                    response =await _accountAndFinance.GetAccountsMovement(header);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        [HttpPost("AddNewAccount")]
        public async Task<BaseResponseWithID> AddNewAccount(AddNewAccountRequest request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _accountAndFinance.AddNewAccount(request, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("EditAccount")]
        public async Task<BaseResponseWithID> EditAccount(AddNewAccountRequest request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _accountAndFinance.EditAccount(request, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAccountInfo")]
        public async Task<AccountInfoResponse> GetAccountInfo([FromHeader] long AccountID = 0)
        {
            AccountInfoResponse Response = new AccountInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response =await _accountAndFinance.GetAccountInfo(AccountID);
                }


                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("DeleteAccount")]
        public BaseResponseWithId DeleteAccount([FromHeader] long AccountId = 0)
        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _accountAndFinance.DeleteAccount(AccountId);
                }
                Response.Result = true;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("AddAndEditFinancialPeriod")]
        public async Task<BaseResponseWithId> AddAndEditFinancialPeriod(FinancialPeriodAccountRequest request)
        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _accountAndFinance.AddAndEditFinancialPeriod(request, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetFinancialPeriodList")]
        public async Task<FinancialPeriodAccountResponse> GetFinancialPeriodList()
        {
            FinancialPeriodAccountResponse Response = new FinancialPeriodAccountResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response =await _accountAndFinance.GetFinancialPeriodList();
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetMethodTypeList")]
        public SelectDDLResponse GetMethodTypeList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _accountAndFinance.GetMethodTypeList();
                }


                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetIncOrExpTypeList")]
        public SelectDDLResponse GetIncOrExpTypeList([FromHeader] long AccountID = 0)
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                //long AccountID = 0;
                if (AccountID == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err506";
                    error.ErrorMSG = "Invalid AccountID";
                    Response.Errors.Add(error);
                    return Response;
                }

                var ItemList = new List<SelectDDL>();

                if (Response.Result)
                {
                    Response = _accountAndFinance.GetIncOrExpTypeList(AccountID);
                }


                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetBeneficiaryList")]
        public SelectDDLResponse GetBeneficiaryList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                var ItemList = new List<SelectDDL>();

                if (Response.Result)
                {
                    Response = _accountAndFinance.GetBeneficiaryList();
                }


                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetCostCenterList")]
        public SelectDDLResponse GetCostCenterList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _accountAndFinance.GetCostCenterList();
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAdvancedTypeList")]
        public SelectAdvancedTypeDDLResponse GetAdvancedTypeList()
        {
            SelectAdvancedTypeDDLResponse Response = new SelectAdvancedTypeDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAdvancedTypeList();
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAdvancedTypeAccountSettingsList")]
        public GetAdvanciedTypeResponse GetAdvancedTypeAccountSettingsList()
        {
            GetAdvanciedTypeResponse Response = new GetAdvanciedTypeResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();


            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAdvancedTypeAccountSettingsList();
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetDailyJournalEntryList")]
        public DailyJournalEntryResponse GetDailyJournalEntryList([FromHeader] long InventoryStoreID = 0, [FromHeader] int CurrentPage = 1,[FromHeader] int NumberOfItemsPerPage = 10, [FromHeader] string ItemSerial = null, [FromHeader] string SearchKey =null)
        {
            DailyJournalEntryResponse Response = new DailyJournalEntryResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _accountAndFinance.GetDailyJournalEntryList(InventoryStoreID, CurrentPage, NumberOfItemsPerPage, ItemSerial, SearchKey);
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAccountsAndFinanceIncomeStatmentPDF")]
        public BaseMessageResponse GetAccountsAndFinanceIncomeStatmentPDF([FromHeader] string Year, [FromHeader] string Month, [FromHeader] string Day)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountsAndFinanceIncomeStatmentPDF(validation.CompanyName,Year, Month,Day);
                }

                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        private List<AccountBalancePerExpOrIncType> AccountBalancePerExpOrIncType(List<long> AccountIdList, int Year, int CategoryType)
        {
            var AccountBalancePerExpOrIncTypeList = new List<AccountBalancePerExpOrIncType>();
            var ListAccountOfJournalList = new List<BalancePerMonth>();

            var ListOfAccountJournalEntryList = _Context.AccountOfJournalEntries
            .Where(x => x.Active == true && AccountIdList.Contains(x.AccountId) && x.EntryDate.Year == Year && x.Account.AccountCategoryId == CategoryType).ToList().GroupBy(x => x.ExpOrIncTypeName).OrderByDescending(x => x.Key);
            foreach (var item in ListOfAccountJournalEntryList)
            {
                var AccountBalancePerExpOrIncType = new AccountBalancePerExpOrIncType();
                AccountBalancePerExpOrIncType.ExpOrIncTypeName = item.Key;
                var BalancePerMonthList = new List<BalancePerMonth>();

                for (int MonthNO = 1; MonthNO <= 12; MonthNO++)
                {
                    var BalanceAmountPerMonth = item.Where(x => x.EntryDate.Month == MonthNO).Select(x => x.Amount).DefaultIfEmpty(0).Sum();

                    BalancePerMonthList.Add(new BalancePerMonth
                    {
                        Accumulative = BalanceAmountPerMonth,
                        MonthNo = MonthNO
                    });

                }
                AccountBalancePerExpOrIncType.BalancePerMonthList = BalancePerMonthList;
                AccountBalancePerExpOrIncType.TotalAmount = item.Where(x => x.EntryDate.Year == Year).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                AccountBalancePerExpOrIncTypeList.Add(AccountBalancePerExpOrIncType);
            }
            return AccountBalancePerExpOrIncTypeList;
        }

        [HttpGet("GetAccountAnnualReportPDF")]
        public BaseMessageResponse GetAccountAnnualReportPDF([FromHeader] int year = 0,[FromHeader] string AccountID=null)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountAnnualReportPDF(validation.CompanyName,year, AccountID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetAccountAndFinanceDashboard")]
        public AccountAndFinanceDashboardResponse GetAccountAndFinanceDashboard()
        {
            AccountAndFinanceDashboardResponse Response = new AccountAndFinanceDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.GetAccountAndFinanceDashboard();
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("BalanceSheetPDF")]
        public async Task<BaseMessageResponse> BalanceSheetPDF([FromHeader] AccountTreeExcelHeader header)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = await _accountAndFinance.BalanceSheetPDF(header, validation.userID, validation.CompanyName);
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }

        }

        [HttpPost("JournalEntryPDFReport")]
        public async Task<BaseMessageResponse> JournalEntryPDFReport([FromHeader] long DailyJournalEntryID = 0)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response =await _accountAndFinance.JournalEntryPDFReport(validation.CompanyName, DailyJournalEntryID);
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }

        }

        //--------------------------------------------------not serviced yet--------------------------------------------
        [HttpPost("AccountMovementReports")]
        public BaseMessageResponse AccountMovementReports([FromHeader] AccountMovementReportsHeader header)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.AccountMovementReports(header, Request);
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        /*[HttpGet("GetAccountAndFinanceInventoryStoreItemReportList")]
        public AccountsAndFinanceInventoryStoreItemResponse GetAccountAndFinanceInventoryStoreItemReportList()
        {
            AccountsAndFinanceInventoryStoreItemResponse Response = new AccountsAndFinanceInventoryStoreItemResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                var InventoryStoreItemList = new List<InventoryStoreItemForReport>();
                //decimal TotalStockBalance = 0;
                //decimal TotalStockBalanceValue = 0;
                //decimal TotalUnitCost = 0;
                int NoOfItems = 0;

                if (Response.Result)
                {
                    long InventoryStoreID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["InventoryStoreID"]) && long.TryParse(Request.Headers["InventoryStoreID"], out InventoryStoreID))
                    {
                        long.TryParse(Request.Headers["InventoryStoreID"], out InventoryStoreID);
                    }

                    int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(Request.Headers["CurrentPage"]) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 10;
                    if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }
                    decimal MinBalance = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["LowStock"]) && decimal.TryParse(Request.Headers["LowStock"], out MinBalance))
                    {
                        decimal.TryParse(Request.Headers["LowStock"], out MinBalance);
                    }

                    decimal MaxBalance = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["ExceedBalance"]) && decimal.TryParse(Request.Headers["ExceedBalance"], out MaxBalance))
                    {
                        decimal.TryParse(Request.Headers["ExceedBalance"], out MaxBalance);
                    }


                    //long POID = 0;
                    //if (!string.IsNullOrEmpty(headers["POID"]) && long.TryParse(headers["POID"], out POID))
                    //{
                    //    long.TryParse(headers["POID"], out POID);
                    //}

                    if (Response.Result)
                    {
                        // Not Grouped -----------------------------------------
                        //var InventoryStoreItemListDB = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true).ToList();
                        //if (InventoryStoreID != 0)
                        //{
                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.InventoryStoreID == InventoryStoreID).ToList();
                        //}
                        //if (headers["ItemSerial"] != null && headers["ItemSerial"] != "")
                        //{
                        //    string ItemSerial = headers["ItemSerial"];
                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.Code == ItemSerial).ToList();
                        //}

                        //if (headers["SearchKey"] != null && headers["SearchKey"] != "")
                        //{
                        //    string SearchKey = headers["SearchKey"];
                        //    var ListItemIDFilter = _Context.V_InventoryStoreItem.Where(x => x.InventoryItemName.ToLower().Contains(SearchKey.ToLower())
                        //                                                                || x.Code.ToLower().Contains(SearchKey.ToLower())
                        //                                                                || x.MarketName.ToLower().Contains(SearchKey.ToLower())
                        //                                                                || x.CommercialName.ToLower().Contains(SearchKey.ToLower())).Select(x => x.InventoryItemID).Distinct().ToList();

                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => ListItemIDFilter.Contains(x.InventoryItemID)).ToList();
                        //}

                        #region For PagedList
                        // var AllInventoryItemPriceAllList = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true).ToList();
                        var InventoryStoreItemListDB = _Context.VInventoryStoreItemPriceReports.Where(x => x.Active == true).OrderBy(a => a.InventoryItemId).AsQueryable();
                        var InventoryStoreItemWithFilterInventoryListDB = InventoryStoreItemListDB;
                        if (InventoryStoreID != 0)
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID);
                            InventoryStoreItemWithFilterInventoryListDB = InventoryStoreItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID);
                        }
                        if (!string.IsNullOrEmpty(Request.Headers["ItemSerial"]))
                        {
                            string ItemSerial = Request.Headers["ItemSerial"];
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.Code == ItemSerial);
                        }
                        if (MinBalance != 0)
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.MinBalance < MinBalance).AsQueryable();
                        }

                        if (MaxBalance != 0)
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.MaxBalance > MaxBalance).AsQueryable();
                        }
                        bool LowStock = false;
                        if (!string.IsNullOrEmpty(Request.Headers["LowStock"]) && bool.TryParse(Request.Headers["LowStock"], out LowStock))
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.FinalBalance > 0 && x.FinalBalance < x.MinBalance).AsQueryable();
                        }
                        bool ExceedBalance = false;
                        if (!string.IsNullOrEmpty(Request.Headers["ExceedBalance"]) && bool.TryParse(Request.Headers["ExceedBalance"], out ExceedBalance))
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.FinalBalance > 0 && x.FinalBalance > x.MaxBalance).AsQueryable();
                        }

                        bool IsExpDate = false;
                        if (!string.IsNullOrEmpty(Request.Headers["IsExpDate"]) && bool.TryParse(Request.Headers["IsExpDate"], out IsExpDate))
                        {
                            if (IsExpDate)
                            {
                                InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.ExpDate < DateTime.Now).AsQueryable();
                            }
                        }
                        if (!string.IsNullOrEmpty(Request.Headers["SearchKey"]))
                        {
                            string SearchKey = Request.Headers["SearchKey"];
                            SearchKey = HttpUtility.UrlDecode(SearchKey);

                            var ListItemIDFilter = _Context.VInventoryStoreItems.Where(x => x.InventoryItemName.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.Code.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.MarketName.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.CommercialName.ToLower().Contains(SearchKey.ToLower())).Select(x => x.InventoryItemId).Distinct().ToList();

                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => ListItemIDFilter.Contains(x.InventoryItemId));
                        }

                        if (!string.IsNullOrEmpty(Request.Headers["MatrialAddingOrderSerial"]))
                        {
                            string MatrialAddingOrderSerial = Request.Headers["MatrialAddingOrderSerial"];
                            var IDSInventoryItemList = _Context.VInventoryAddingOrderItems.Where(x => x.ItemSerial.Trim() == MatrialAddingOrderSerial.Trim()).Select(x => x.InventoryItemId).ToList();
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => IDSInventoryItemList.Contains(x.InventoryItemId));
                        }
                        var InventoryStoreItemPriceDistinctList = InventoryStoreItemListDB.Select(x => new InventoryStoreItemForReport
                        {
                            ID = x.InventoryItemId,
                            InventoryStoreId = x.InventoryStoreId,
                            ItemName = x.InventoryItemName,
                            InventoryStoreName = x.InventoryStoreName,
                            HoldQTY = (decimal?)( _Context.VInventoryMatrialRequestWithItems.Where(a => a.InventoryItemId == x.InventoryItemId && a.ToInventoryStoreId == x.InventoryStoreId).Select(a => (decimal?)a.ReqQuantity).Sum()??0),
                            OpenPOQTY = x.ReqQuantity > x.RecivedQuantity ? x.ReqQuantity - x.RecivedQuantity : 0,
                            ////V_PurchasePoItem_PO.Where(a => a.InventoryItemID == x.InventoryItemID && a.Status == "Open" && a.ReqQuantity != null).Sum(a => a.ReqQuantity) >=
                            //           V_InventoryAddingOrderItems.Where(a => a.InventoryItemID == x.InventoryItemID && a.RecivedQuantity != null).Sum(a => a.RecivedQuantity) ?
                            //           V_PurchasePoItem_PO.Where(a => a.InventoryItemID == x.InventoryItemID && a.Status == "Open" && a.ReqQuantity != null).Sum(a => a.ReqQuantity) -
                            //           V_InventoryAddingOrderItems.Where(a => a.InventoryItemID == x.InventoryItemID && a.RecivedQuantity != null).Sum(a => a.RecivedQuantity)
                            //           : 0,
                            Active = x.Active ?? false,
                            ItemCode = x.Code,
                            MinStock = x.MinBalance,
                            MaxStock = x.MaxBalance,
                            RequestionUOMShortName = x.RequestionUomshortName,
                            StockBalance = x.StockBalance ?? 0,// InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.Balance != null).Sum(a => a.Balance) ?? 0,
                            StockBalanceValue = 0,
                            //InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMAverageUnitPrice != null && a.CalculationType == 1).Select(a => a.SUMAverageUnitPrice).Sum() ?? 0 +
                            //                    InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMMaxUnitPrice != null && a.CalculationType == 2).Select(a => a.SUMMaxUnitPrice).Sum() ?? 0 +
                            //                    InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMLastUnitPrice != null && a.CalculationType == 3).Select(a => a.SUMLastUnitPrice).Sum() ?? 0 +
                            //                    InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMCustomeUnitPrice != null && a.CalculationType == 4).Select(a => a.SUMCustomeUnitPrice).Sum() ?? 0,
                            UnitCost = x.CustomeUnitPrice != null ? (decimal)x.CustomeUnitPrice : 0,
                        }).Distinct().AsQueryable();

                        //var IDInventoryItemList = InventoryStoreItemListDB.Select(x => new InventoryStoreItemPaging
                        //{
                        //    InventoryItemID = x.InventoryItemID,
                        //    SUMAverageUnitPrice = x.SUMAverageUnitPrice,
                        //    SUMMaxUnitPrice = x.SUMMaxUnitPrice,
                        //    SUMLastUnitPrice = x.SUMLastUnitPrice,
                        //    SUMCustomeUnitPrice = x.SUMCustomeUnitPrice,
                        //    InventoryItemName = x.InventoryItemName,
                        //    Code = x.Code,
                        //    RequestionUOMShortName = x.RequestionUOMShortName,
                        //    Balance = x.Balance,
                        //    CalculationType = x.CalculationType,
                        //    CustomeUnitPrice = x.CustomeUnitPrice
                        //}).Distinct().AsQueryable();
                        //var InventoryStoreItemPagingList = PagedList<InventoryStoreItemPaging>.Create(IDInventoryItemList, CurrentPage, NumberOfItemsPerPage);
                        //var InventoryStoreItemPagingList = PagedList<V_InventoryStoreItemPrice>.Create(InventoryStoreItemListDB, CurrentPage, NumberOfItemsPerPage);
                        InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.ID);
                        if (!string.IsNullOrEmpty(Request.Headers["SortBy"]))
                        {
                            if (Request.Headers["SortBy"] == "ItemName")
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.ItemName);
                            }
                            else if (Request.Headers["SortBy"] == "ItemCode")
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.ItemCode);
                            }
                            else if (Request.Headers["SortBy"] == "StoreName")
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.InventoryStoreName);
                            }
                        }

                        DateTime NotReleasedFrom = new DateTime(DateTime.Now.Year, 1, 1);
                        if (!string.IsNullOrEmpty(Request.Headers["NotReleasedFrom"]))
                        {
                            bool hasfrom = DateTime.TryParse(Request.Headers["NotReleasedFrom"], out NotReleasedFrom);
                        }

                        *//*Not Release duration date from and to*//*
                        var InventoryItemIDs = new List<long>();
                        if (!string.IsNullOrWhiteSpace(Request.Headers["NotReleasedFrom"]))
                        {
                            InventoryItemIDs = _Context.InventoryStoreItems.Where(x => x.OperationType.Contains("Release Order"))
                                               .GroupBy(item => item.InventoryItemId)
                                               .Select(group => group.OrderByDescending(item => item.CreationDate).FirstOrDefault()).Where(x => x.CreationDate <= NotReleasedFrom).Select(x => x.InventoryItemId)
                                               .Distinct().ToList();
                            //InventoryItemIDs =  _Context.InventoryMatrialReleaseItems.Where(x => x.InventoryMatrialRelease.CreationDate >= NotReleasedFrom && x.InventoryMatrialRelease.CreationDate <= NotReleasedTo).Select(x => x.InventoryMatrialRequestItem.InventoryItemID).ToList();
                            if (InventoryItemIDs != null)
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.Where(x => InventoryItemIDs.Contains(x.ID) && x.StockBalance > 0).AsQueryable();
                            }
                        }

                        var InventoryStoreItemPagingList = PagedList<InventoryStoreItemForReport>.Create(InventoryStoreItemPriceDistinctList, CurrentPage, NumberOfItemsPerPage);

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = CurrentPage,
                            TotalPages = InventoryStoreItemPagingList.TotalPages,
                            ItemsPerPage = NumberOfItemsPerPage,
                            TotalItems = InventoryStoreItemPagingList.TotalCount
                        };
                        #endregion



                        // New for paging Calc
                        if (InventoryStoreItemPagingList.Count > 0)
                        {
                            var ListIDSItems = InventoryStoreItemPagingList.Select(x => x.ID).ToList();
                            var ListInventoryItemsList = _Context.InventoryItems.Where(x => ListIDSItems.Contains(x.Id)).ToList();
                            var ListAddingOrderItemsList = _Context.InventoryAddingOrderItems.Where(x => ListIDSItems.Contains(x.InventoryItemId)).ToList();
                            foreach (var item in InventoryStoreItemPagingList)
                            {
                                var InventoryStoreItemListDBForCalc = InventoryStoreItemListDB.Where(a => a.InventoryItemId == item.ID && a.InventoryStoreId == item.InventoryStoreId).AsQueryable();
                                var V_InventoryItemObjDB = ListInventoryItemsList.Where(x => x.Id == item.ID).FirstOrDefault();
                                if (V_InventoryItemObjDB != null)
                                {

                                    var ItemOBJ = new InventoryStoreItemForReport();
                                    ItemOBJ.ID = item.ID;
                                    ItemOBJ.InventoryStoreName = item.InventoryStoreName;
                                    ItemOBJ.HoldQTY = item.HoldQTY;
                                    ItemOBJ.OpenPOQTY = item.OpenPOQTY;
                                    ItemOBJ.Active = item.Active;
                                    ItemOBJ.ItemCode = item.ItemCode;
                                    ItemOBJ.ItemName = item.ItemName;
                                    ItemOBJ.UnitCost = item.UnitCost;
                                    ItemOBJ.RequestionUOMShortName = item.RequestionUOMShortName;
                                    ItemOBJ.StockBalance = item.StockBalance;
                                    ItemOBJ.MinStock = item.MinStock;
                                    ItemOBJ.MaxStock = item.MaxStock;
                                    //ItemOBJ.StockBalanceValue = InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 1).Sum(a => a.SUMAverageUnitPrice) ?? 0 +
                                    //                            InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 2).Sum(a => a.SUMMaxUnitPrice) ?? 0 +
                                    //                            InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 3).Sum(a => a.SUMLastUnitPrice) ?? 0 +
                                    //                            InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 4).Sum(a => a.SUMCustomeUnitPrice) ?? 0;
                                    //InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMAverageUnitPrice != null && a.CalculationType == 1).Select(a => a.SUMAverageUnitPrice).DefaultIfEmpty(0).Sum() ?? 0 +
                                    //            InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMMaxUnitPrice != null && a.CalculationType == 2).Select(a => a.SUMMaxUnitPrice).DefaultIfEmpty(0).Sum() ?? 0 +
                                    //            InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMLastUnitPrice != null && a.CalculationType == 3).Select(a => a.SUMLastUnitPrice).DefaultIfEmpty(0).Sum() ?? 0 +
                                    //            InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMCustomeUnitPrice != null && a.CalculationType == 4).Select(a => a.SUMCustomeUnitPrice).DefaultIfEmpty(0).Sum() ?? 0;
                                    ItemOBJ.ExpDate =
                                        ListAddingOrderItemsList.Where(x => x.InventoryItemId == item.ID).Select(x => x.ExpDate?.ToShortDateString()).FirstOrDefault();
                                    //Common.GetInventoryItemExpDateFromMatrialAddingOrder(item.ID);

                                    ItemOBJ.MarketName = V_InventoryItemObjDB.MarketName ?? "";
                                    ItemOBJ.Category = V_InventoryItemObjDB.InventoryItemCategory?.Name;
                                    ItemOBJ.RUOM = V_InventoryItemObjDB.RequstionUom?.ShortName; //.RequestionUOMSHortName;
                                    ItemOBJ.CommercialName = V_InventoryItemObjDB.CommercialName ?? "";
                                    ItemOBJ.PartNO = V_InventoryItemObjDB.PartNo ?? "";
                                    ItemOBJ.Cost1 = V_InventoryItemObjDB.CostAmount1 ?? 0;
                                    ItemOBJ.Cost2 = V_InventoryItemObjDB.CostAmount2 ?? 0;
                                    ItemOBJ.Cost3 = V_InventoryItemObjDB.CostAmount3 ?? 0;
                                    ItemOBJ.ItemSerialCounter = V_InventoryItemObjDB.ItemSerialCounter != null ? V_InventoryItemObjDB.ItemSerialCounter.ToString() : "";

                                    // fill Inventory List ------------------
                                    InventoryStoreItemList.Add(ItemOBJ);
                                }
                            }
                            NoOfItems += InventoryStoreItemPagingList.Select(x => x.ID).Distinct().Count();
                        }






                        //if (InventoryStoreItemPagingList.Count > 0)
                        //{

                        //    var InventoryItemList = InventoryStoreItemPagingList.Select(x => x.InventoryItemID).Distinct().ToList();
                        //    if (headers["MatrialAddingOrderSerial"] != null && headers["MatrialAddingOrderSerial"] != "")
                        //    {
                        //        InventoryItemList = null;
                        //        string MatrialAddingOrderSerial = headers["MatrialAddingOrderSerial"];
                        //        InventoryItemList = _Context.V_InventoryAddingOrderItems.Where(x => x.ItemSerial.Trim() == MatrialAddingOrderSerial.Trim()).Select(x => x.InventoryItemID).ToList();
                        //    }
                        //    foreach (var InventoryItemID in InventoryItemList)
                        //    {

                        //        var InventoryStoreItemOBJ = new InventoryStoreItem();
                        //        decimal StockBalanceValue = 0;

                        //        int CalculationType = 0;
                        //        var ItemPErInventoryObj = InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID).FirstOrDefault();
                        //        if (ItemPErInventoryObj != null)
                        //        {
                        //            CalculationType = ItemPErInventoryObj.CalculationType != null ? (int)ItemPErInventoryObj.CalculationType : 0;
                        //            InventoryStoreItemOBJ.ID = ItemPErInventoryObj.InventoryItemID;
                        //            InventoryStoreItemOBJ.ItemName = ItemPErInventoryObj.InventoryItemName;
                        //            InventoryStoreItemOBJ.ItemCode = ItemPErInventoryObj.Code;
                        //            InventoryStoreItemOBJ.RequestionUOMShortName = ItemPErInventoryObj.RequestionUOMShortName;
                        //        }
                        //        if (CalculationType == 1)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMAverageUnitPrice != null).Sum(x => (decimal)x.SUMAverageUnitPrice);
                        //        }
                        //        else if (CalculationType == 2)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMMaxUnitPrice != null).Sum(x => (decimal)x.SUMMaxUnitPrice);
                        //        }
                        //        else if (CalculationType == 3)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMLastUnitPrice != null).Sum(x => (decimal)x.SUMLastUnitPrice);
                        //        }
                        //        else if (CalculationType == 4)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMCustomeUnitPrice != null).Sum(x => (decimal)x.SUMCustomeUnitPrice);
                        //        }
                        //        InventoryStoreItemOBJ.StockBalance = InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.Balance != null).Sum(x => (decimal)x.Balance);
                        //        InventoryStoreItemOBJ.StockBalanceValue = StockBalanceValue;  // Unit Cost *  Qty Balance
                        //        InventoryStoreItemOBJ.UnitCost = ItemPErInventoryObj.CustomeUnitPrice != null ? (decimal)ItemPErInventoryObj.CustomeUnitPrice : 0;   // Unit Cost 
                        //        InventoryStoreItemOBJ.ExpDate = Common.GetInventoryItemExpDateFromMatrialAddingOrder(InventoryItemID);

                        //        // fill Inventory List ------------------
                        //        InventoryStoreItemList.Add(InventoryStoreItemOBJ);
                        //        TotalStockBalance += InventoryStoreItemOBJ.StockBalance;
                        //        TotalStockBalanceValue += InventoryStoreItemOBJ.StockBalanceValue;
                        //    }
                        //    NoOfItems += InventoryStoreItemPagingList.Select(x => x.InventoryItemID).Distinct().Count();
                        //}


                        Response.TotalItems = Common.GetNoOFInventoryItem(_Context);
                        Response.TotalPricedItems = InventoryStoreItemWithFilterInventoryListDB.Select(x => x.InventoryItemId).Distinct().Count();
                        Response.TotalStockBalance = InventoryStoreItemListDB.Select(x => x.StockBalance).Sum() ?? 0;
                        Response.TotalStockBalanceValue =
                                                                 InventoryStoreItemListDB.Where(x => x.CalculationType == 1).Sum(a => a.SumaverageUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDB.Where(x => x.CalculationType == 2).Sum(a => a.SummaxUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDB.Where(x => x.CalculationType == 3).Sum(a => a.SumlastUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDB.Where(x => x.CalculationType == 4).Sum(a => a.SumcustomeUnitPrice) ?? 0;


                        //InventoryStoreItemListDB.Select(x => x.StockBalanceValue).Sum();

                    }
                    Response.InventoryStoreItemList = InventoryStoreItemList;
                    Response.NoOfItems = NoOfItems;


                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }*/

        [HttpGet("SalesOfferItemsReport")]
        public BaseResponseWithData<string> SalesOfferItemsReport([FromHeader] int Year, [FromHeader] int Month, [FromHeader] long SalesPersonId)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.SalesOfferItemsReport(validation.CompanyName,Year,Month,SalesPersonId);
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
        [HttpGet("AccoutingStatementOfSalesOffers")]
        public BaseResponseWithData<string> AccoutingStatementOfSalesOffers([FromHeader]int BranchId,[FromHeader] int Year)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.AccoutingStatementOfSalesOffers(validation.CompanyName,BranchId,Year);
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
        [HttpGet("RentalEquipmentsReport")]
        public BaseResponseWithData<string> RentalEquipmentsReport([FromHeader] int Year,[FromHeader] int Month)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.RentalEquipmentsReport(validation.CompanyName, Year,Month);
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        [HttpGet("Vault10Percent")]
        public BaseResponseWithData<string> Vault10Percent([FromHeader] int Year, [FromHeader] int Month)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _accountAndFinance.Vault10Percent(validation.CompanyName, Year, Month);
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        //-------------------------------------AssetsDeprecition-------------------------------------------------------

        [HttpPost("AddProductionUOM")]
        public BaseResponseWithId<long> AddProductionUOM(AddProductionUOMDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _assetDepreciationService.AddProductionUOM(dto, validation.userID);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpPost("EditProductionUOM")]
        public BaseResponseWithId<long> EditProductionUOM(EditProductionUOMDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _assetDepreciationService.EditProductionUOM(dto, validation.userID);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetProductionUOMDDL")]
        public SelectDDLResponse GetFamilyStatusDDL()
        {
            var response = new SelectDDLResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _assetDepreciationService.GetProductionUOMDDL();
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpPost("AddDepreciationType")]
        public BaseResponseWithId<long> AddDepreciationType(AddProductionUOMDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _assetDepreciationService.AddDepreciationType(dto, validation.userID);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpPost("EditDepreciationType")]
        public BaseResponseWithId<long> EditDepreciationType(EditProductionUOMDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _assetDepreciationService.EditDepreciationType(dto, validation.userID);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetDepreciationTypeDDL")]
        public SelectDDLResponse GetDepreciationTypeDDL()
        {
            var response = new SelectDDLResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _assetDepreciationService.GetDepreciationTypeDDL();
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpPost("AddAssetDepreciation")]
        public BaseResponseWithId<long> AddAssetDepreciation(AddAssetDepreciationDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _assetDepreciationService.AddAssetDepreciation(dto, validation.userID);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }
      
        [HttpPost("EditAssetDepreciation")]
        public BaseResponseWithId<long> EditAssetDepreciation(EditAssetDepreciationDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _assetDepreciationService.EditAssetDepreciation(dto, validation.userID);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetAssetDepreciation")]
        public BaseResponseWithData<List<GetAssetDepreciationDTO>> GetAssetDepreciation([FromHeader] GetAssetDepreciationFilters filters)
        {
            var response = new BaseResponseWithData<List<GetAssetDepreciationDTO>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _assetDepreciationService.GetAssetDepreciation(filters);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

    }


}
