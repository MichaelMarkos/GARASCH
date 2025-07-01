using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using NewGarasAPI.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Entities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using NewGarasAPI.Models.Supplier;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Data;
using NewGarasAPI.Models.AccountAndFinance;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Web;
using NewGaras.Infrastructure.Helper;
using NewGarasAPI.Models.User;
using NewGarasAPI.Models.Purchase;
using static iTextSharp.text.pdf.AcroFields;
using Microsoft.IdentityModel.Tokens;
using NewGaras.Domain.Models;
using ClosedXML.Excel;
using NewGaras.Infrastructure.Models.SalesOffer;
using Microsoft.Identity.Client;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace NewGaras.Domain.Services
{

    public class AccountAndFinanceService : IAccountAndFinanceService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string BaseCurrencyConverterApiAddress;
        private readonly string CurrencyConvertorAddress;
        private HearderVaidatorOutput validation;

        public HearderVaidatorOutput Validation
        {
            get
            {
                return validation;
            }
            set
            {
                validation = value;
            }
        }

        public AccountAndFinanceService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {

            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
            BaseCurrencyConverterApiAddress = "https://api.exchangerate.host/";
            CurrencyConvertorAddress = "convert?format=json";
        }
        public AccountsAndFinanceIncomeStatmentResponse GetAccountsAndFinanceIncomeStatment([FromHeader] int? year, [FromHeader] int? month, [FromHeader] int? day)
        {
            AccountsAndFinanceIncomeStatmentResponse Response = new AccountsAndFinanceIncomeStatmentResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    DateTime Today = DateTime.Now;
                    int Year = Today.Year;
                    int Month = 0;
                    int Day = 0;
                    if (year != 0 && year != null)
                    {
                        Year = (int)year;
                    }

                    if (month != null && month != 0)
                    {
                        if (year == null || year == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "You must select year first";
                            Response.Errors.Add(error);
                        }
                        Month = (int)month;
                    }

                    if (day != null && day != 0)
                    {
                        if (month == 0 || month == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "You must select Month first";
                            Response.Errors.Add(error);
                        }
                        Day = (int)day;
                    }

                    // Modified on 2023-5-10 mic
                    var ALLAccountListDB = _unitOfWork.Accounts.FindAll(x => x.AccountCategoryId == 4 || x.AccountCategoryId == 5).ToList();
                    var AccountsListDB = ALLAccountListDB.Where(x => x.ParentCategory == 0).ToList();



                    // For Calculation Profit
                    decimal TotalIncomeAmount = 0;
                    decimal TotalExpensesAmount = 0;


                    // Fill Account and Finance Income -----
                    var AccountAndFinanceIncome = new List<AccountsAndFinanceDetails>();
                    var AccountsIncomeListDB = AccountsListDB.Where(x => x.AccountCategoryId == 4).ToList();

                    if (AccountsIncomeListDB.Count > 0)
                    {
                        foreach (var account in AccountsIncomeListDB)
                        {

                            var AccountsAndFinanceObj = new AccountsAndFinanceDetails();
                            decimal Accumulative = 0;
                            if (account.Haveitem)
                            {
                                var Request = LoadChildAccountForIncomeStatment(ALLAccountListDB, (int)account.Id, Year, Month, Day);
                                AccountsAndFinanceObj.AccountsAndFinanceChildList = Request.AccountsAndFinanceDetailsList;
                                Accumulative = Request.TotalsumChildren;
                            }

                            string AccumulativeType = "";
                            if (Accumulative > 0)
                            {
                                AccumulativeType = "C";
                            }
                            else if (Accumulative < 0)
                            {
                                AccumulativeType = "D";
                            }
                            AccountsAndFinanceObj.AccountID = account.Id;
                            AccountsAndFinanceObj.AccountName = account.AccountName;
                            AccountsAndFinanceObj.HaveChild = account.Haveitem;
                            AccountsAndFinanceObj.CurrencyName = account.CurrencyId != null ? Common.GetCurrencyName((int)account.CurrencyId, _Context) : "";
                            AccountsAndFinanceObj.BalanceAmount = Math.Abs(Accumulative).ToString() + " " + AccumulativeType;


                            AccountAndFinanceIncome.Add(AccountsAndFinanceObj);

                            TotalIncomeAmount += Accumulative;
                        }
                    }

                    Response.IncomeAccountsAndFinanceList = AccountAndFinanceIncome;

                    // Loop In All Income to sum Childern to Parent
                    foreach (var item in AccountAndFinanceIncome)
                    {
                        if (!item.HaveChild)
                        {
                            var ParentSumAccumulative = AccountAndFinanceIncome.Where(x => x.ParentCategoryID == item.AccountID).Select(x => x.Accumulative).Sum();
                            AccountAndFinanceIncome.FirstOrDefault().AccountID = 5;
                        }
                    }


                    // Fill Account and Finance Expenses -----
                    var AccountAndFinanceExpenses = new List<AccountsAndFinanceDetails>();
                    var AccountsExpensesListDB = AccountsListDB.Where(x => x.AccountCategoryId == 5).ToList();

                    if (AccountsExpensesListDB.Count > 0)
                    {
                        foreach (var account in AccountsExpensesListDB)
                        {
                            var AccountsAndFinanceObj = new AccountsAndFinanceDetails();

                            decimal Accumulative = 0;
                            string AccumulativeType = "";
                            if (account.Haveitem)
                            {
                                var Request = LoadChildAccountForIncomeStatment(ALLAccountListDB, (int)account.Id, Year, Month, Day);
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

                            AccountsAndFinanceObj.AccountID = account.Id;
                            AccountsAndFinanceObj.AccountName = account.AccountName;
                            AccountsAndFinanceObj.CurrencyName = account.CurrencyId != null ? Common.GetCurrencyName((int)account.CurrencyId, _Context) : "";
                            AccountsAndFinanceObj.HaveChild = account.Haveitem;
                            AccountsAndFinanceObj.BalanceAmount = Math.Abs(Accumulative).ToString() + " " + AccumulativeType;

                            AccountAndFinanceExpenses.Add(AccountsAndFinanceObj);
                            TotalExpensesAmount += Accumulative;

                        }
                    }

                    Response.ExpensesAccountsAndFinanceList = AccountAndFinanceExpenses;
                    //Response.NetProfit = Math.Abs(TotalIncomeAmount) - Math.Abs(TotalExpensesAmount);
                    Response.NetProfit = TotalIncomeAmount + TotalExpensesAmount;

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

        public AccountsAndFinanceIncomeChildrenCalcResponse LoadChildAccount(List<Account> ALLAccountsListDB, int AccCategory, int Year, int Month, int Day)

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
                var TotalAccountOfJournalEntry = _unitOfWork.AccountOfJournalEntries.FindAll(x => x.EntryDate.Year == Year && x.Active == true &&
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
        }

        private AccountsAndFinanceIncomeChildrenCalcResponse LoadChildAccountForIncomeStatment(List<Account> ALLAccountsListDB, long parentID, int Year, int Month, int Day)
        {
            var AccountAndFinanceIncome = new List<AccountsAndFinanceDetails>();
            // modified 2023-5-10-mic
            var AccountsListDB = ALLAccountsListDB.Where(x => x.ParentCategory == parentID).ToList();
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
                var TotalAccountOfJournalEntry = _unitOfWork.AccountOfJournalEntries.FindAll(x => x.EntryDate.Year == Year && x.Active == true &&
                                                                                        (x.DtmainType == "Income" || x.DtmainType == "Expenses")
                                                                                        ).ToList();

                //var currenicesIDs = TotalAccountOfJournalEntry.Select(a => a.CurrencyId).ToList();
                var currenicesList = _unitOfWork.Currencies.GetAll();

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
                        var Request = LoadChildAccountForIncomeStatment(ALLAccountsListDB, account.Id, Year, Month, Day);
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
                    AccountsAndFinanceObj.CurrencyName = account.CurrencyId != null ? currenicesList.Where(a => a.Id == account.CurrencyId).FirstOrDefault().Name : "";
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


        public AccountsAndFinanceCashAndBanksResponse GetAccountsAndFinanceCashAndBank([FromHeader] int? year, [FromHeader] int? month, [FromHeader] int? day)
        {
            AccountsAndFinanceCashAndBanksResponse Response = new AccountsAndFinanceCashAndBanksResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    decimal TotlaAmountBanks = 0;
                    string AmountBankBalanceType = "";
                    decimal TotlaAmountTreasury = 0;
                    string AmountTreasuryType = "";
                    decimal TotlaAmountPromissory = 0;
                    string AmountPromissoryBalanceType = "";

                    decimal TotalAmountSum = 0;
                    string TotalAmountSumType = "";
                    if (Response.Result)
                    {

                        DateTime Today = DateTime.Now;
                        int Year = Today.Year;
                        int Month = 0;
                        int Day = 0;
                        if (year != 0 && year != null)
                        {
                            Year = (int)year;
                        }

                        if (month != null && month != 0)
                        {
                            if (year == null || year == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "You must select year first";
                                Response.Errors.Add(error);
                            }
                            Month = (int)month;
                        }

                        if (day != null && day != 0)
                        {
                            if (month == 0 || month == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "You must select Month first";
                                Response.Errors.Add(error);
                            }
                            Day = (int)day;
                        }

                        var AccountsListDB = _unitOfWork.Accounts.FindAll(x => x.AccountCategoryId == 1).ToList();

                        var AccountAndFinanceBanks = new List<AccountsAndFinanceDetailsAdavanced>();
                        var BankAdvanciedSettingAccountListDB = _unitOfWork.AdvanciedSettingAccounts.FindAll(x => x.Active == true && x.AdvanciedTypeId == 1).Select(x => x.AccountId).Distinct().ToList();



                        var AccountsBankListDB = AccountsListDB.Where(x => BankAdvanciedSettingAccountListDB.Contains(x.Id)).ToList();

                        if (AccountsBankListDB.Count > 0)
                        {
                            foreach (var account in AccountsBankListDB)
                            {
                                var AcumelativeListFromEntry = _unitOfWork.AccountOfJournalEntries.FindAll(x => x.EntryDate.Year == Year && x.AccountId == account.Id && x.Active == true).ToList();
                                if (Month != 0)
                                {
                                    AcumelativeListFromEntry = AcumelativeListFromEntry.Where(x => x.EntryDate.Month == Month).ToList();
                                }
                                if (Day != 0)
                                {
                                    AcumelativeListFromEntry = AcumelativeListFromEntry.Where(x => x.EntryDate.Day == Day).ToList();
                                }
                                decimal Accumulative = AcumelativeListFromEntry.Select(x => x.Amount).Sum();

                                string AccumulativeType = "";

                                if (account.AccountTypeName == "Credit")
                                {
                                    if (Accumulative > 0)
                                    {
                                        AccumulativeType = "C";
                                    }
                                    else
                                    {
                                        AccumulativeType = "D";
                                    }
                                }
                                else if (account.AccountTypeName == "Debit")
                                {
                                    if (Accumulative > 0)
                                    {
                                        AccumulativeType = "D";
                                    }
                                    else
                                    {
                                        AccumulativeType = "C";
                                    }
                                }
                                var AccountsAndFinanceObj = new AccountsAndFinanceDetailsAdavanced();
                                AccountsAndFinanceObj.AccountID = account.Id;
                                AccountsAndFinanceObj.AccountName = account.AccountName;
                                AccountsAndFinanceObj.ParentCategory = Common.GetParentCategory(account.Id, _Context);
                                AccountsAndFinanceObj.CurrencyName = account.CurrencyId != null ? Common.GetCurrencyName((int)account.CurrencyId, _Context) : "";
                                //  AccountsAndFinanceObj.BalanceLE = Math.Abs(Accumulative).ToString() + " " + AccumulativeType;


                                decimal OpeningBalance = 0;
                                string BalanceType = "";
                                string OpennigBalanceType = "";
                                if (account.Comment != null && account.Comment != "")
                                {
                                    var Comment = string.Join("/", account.Comment);
                                    var ListComment = Comment.Split('/').ToList();
                                    OpeningBalance = ListComment[0] != "" ? decimal.Parse(ListComment[0]) : 0;
                                    OpennigBalanceType = ListComment[2];
                                    if (OpennigBalanceType == "Credit")
                                    {
                                        BalanceType = "C";
                                    }
                                    else
                                    {
                                        BalanceType = "D";
                                    }
                                }


                                //  Accumulative = Math.Abs(Accumulative);
                                decimal BalanceAmount = 0;
                                if (BalanceType == AccumulativeType)
                                {
                                    BalanceAmount = Accumulative + OpeningBalance;
                                }
                                else
                                {
                                    BalanceAmount = Accumulative + (-1 * OpeningBalance);
                                }



                                string FinalBlanaceType = "";
                                if (BalanceAmount > 0)
                                {
                                    FinalBlanaceType = AccumulativeType;
                                }
                                else if (BalanceAmount < 0)
                                {
                                    if (AccumulativeType == "C")
                                    {
                                        FinalBlanaceType = "D";
                                    }
                                    else if (AccumulativeType == "D")
                                    {
                                        FinalBlanaceType = "C";
                                    }
                                }
                                AccountsAndFinanceObj.BalanceAmount = Math.Abs(BalanceAmount) + " " + FinalBlanaceType;

                                AccountAndFinanceBanks.Add(AccountsAndFinanceObj);

                                // For Calcualte Total Amount ----------------------------------------------------
                                if (AccountsAndFinanceObj.CurrencyName != "EGP")
                                {
                                    if (BalanceAmount < 0)
                                    {

                                        BalanceAmount = CurrencyConverterAsync(AccountsAndFinanceObj.CurrencyName, "EGP", Math.Abs(BalanceAmount)).Result;
                                        BalanceAmount = BalanceAmount * -1;
                                    }
                                    else
                                    {
                                        BalanceAmount = CurrencyConverterAsync(AccountsAndFinanceObj.CurrencyName, "EGP", BalanceAmount).Result;
                                    }

                                }
                                // TotlaAmountBanks += Accumulative;
                                if (AmountBankBalanceType == "") // first time
                                {
                                    TotlaAmountBanks = Math.Abs(BalanceAmount);
                                    AmountBankBalanceType = FinalBlanaceType;
                                }
                                else // Calculation 
                                {
                                    if (FinalBlanaceType != AmountBankBalanceType)
                                    {

                                        TotlaAmountBanks = Math.Abs(TotlaAmountBanks - Math.Abs(BalanceAmount));
                                        if (TotlaAmountBanks < Math.Abs(BalanceAmount))
                                        {
                                            AmountBankBalanceType = FinalBlanaceType;
                                        }
                                    }
                                    else
                                    {
                                        TotlaAmountBanks = Math.Abs(TotlaAmountBanks + Math.Abs(BalanceAmount));
                                    }
                                }
                            }
                        }

                        Response.Banks = AccountAndFinanceBanks;
                        if (TotalAmountSum == 0)
                        {
                            TotalAmountSum = TotlaAmountBanks;
                            TotalAmountSumType = AmountBankBalanceType;
                        }


                        // Fill Account and Finance Treasury --------------------------------
                        var AccountAndFinanceTreasury = new List<AccountsAndFinanceDetailsAdavanced>();

                        var TreasuryAdvanciedSettingAccountListDB = _unitOfWork.AdvanciedSettingAccounts.FindAll(x => x.Active == true && x.AdvanciedTypeId == 2).Select(x => x.AccountId).Distinct().ToList();

                        var AccountsTreasuryListDB = AccountsListDB.Where(x => TreasuryAdvanciedSettingAccountListDB.Contains(x.Id)).ToList();

                        if (AccountsTreasuryListDB.Count > 0)
                        {
                            foreach (var account in AccountsTreasuryListDB)
                            {
                                var AcumelativeListFromEntry = _unitOfWork.AccountOfJournalEntries.FindAll(x => x.EntryDate.Year == Year && x.AccountId == account.Id && x.Active == true).ToList();
                                if (Month != 0)
                                {
                                    AcumelativeListFromEntry = AcumelativeListFromEntry.Where(x => x.EntryDate.Month == Month).ToList();
                                }
                                if (Day != 0)
                                {
                                    AcumelativeListFromEntry = AcumelativeListFromEntry.Where(x => x.EntryDate.Day == Day).ToList();
                                }
                                decimal Accumulative = AcumelativeListFromEntry.Select(x => x.Amount).Sum();

                                string AccumulativeType = "";

                                if (account.AccountTypeName == "Credit")
                                {
                                    if (Accumulative > 0)
                                    {
                                        AccumulativeType = "C";
                                    }
                                    else
                                    {
                                        AccumulativeType = "D";
                                    }
                                }
                                else if (account.AccountTypeName == "Debit")
                                {
                                    if (Accumulative > 0)
                                    {
                                        AccumulativeType = "D";
                                    }
                                    else
                                    {
                                        AccumulativeType = "C";
                                    }
                                }
                                var AccountsAndFinanceObj = new AccountsAndFinanceDetailsAdavanced();
                                AccountsAndFinanceObj.AccountID = account.Id;
                                AccountsAndFinanceObj.AccountName = account.AccountName;
                                AccountsAndFinanceObj.ParentCategory = Common.GetParentCategory(account.Id, _Context);
                                AccountsAndFinanceObj.CurrencyName = account.CurrencyId != null ? Common.GetCurrencyName((int)account.CurrencyId, _Context) : "";
                                //AccountsAndFinanceObj.BalanceLE = Math.Abs(Accumulative).ToString() + " " + AccumulativeType;





                                decimal OpeningBalance = 0;
                                string BalanceType = "";
                                string OpennigBalanceType = "";
                                if (account.Comment != null && account.Comment != "")
                                {
                                    var Comment = string.Join("/", account.Comment);
                                    var ListComment = Comment.Split('/').ToList();
                                    decimal.TryParse(ListComment[0], out OpeningBalance);
                                    if (ListComment.Count() > 1)
                                    {

                                        OpennigBalanceType = ListComment[2];
                                        if (!string.IsNullOrWhiteSpace(OpennigBalanceType))
                                        {
                                            if (OpennigBalanceType == "Credit")
                                            {
                                                BalanceType = "C";
                                            }
                                            else if (OpennigBalanceType == "Debit")
                                            {
                                                BalanceType = "D";
                                            }
                                        }
                                    }
                                }


                                //Accumulative = Math.Abs(Accumulative);
                                decimal BalanceAmount = 0;
                                if (BalanceType == AccumulativeType)
                                {
                                    BalanceAmount = Accumulative + OpeningBalance;
                                }
                                else
                                {
                                    BalanceAmount = Accumulative + (-1 * OpeningBalance);
                                }



                                string FinalBlanaceType = "";
                                if (BalanceAmount > 0)
                                {
                                    FinalBlanaceType = AccumulativeType;
                                }
                                else if (BalanceAmount < 0)
                                {
                                    if (AccumulativeType == "C")
                                    {
                                        FinalBlanaceType = "D";
                                    }
                                    else if (AccumulativeType == "D")
                                    {
                                        FinalBlanaceType = "C";
                                    }
                                }


                                AccountsAndFinanceObj.BalanceAmount = Math.Abs(BalanceAmount) + " " + FinalBlanaceType;





                                AccountAndFinanceTreasury.Add(AccountsAndFinanceObj);

                                if (AccountsAndFinanceObj.CurrencyName != "EGP")
                                {
                                    if (BalanceAmount < 0)
                                    {

                                        BalanceAmount = CurrencyConverterAsync(AccountsAndFinanceObj.CurrencyName, "EGP", Math.Abs(BalanceAmount)).Result;
                                        BalanceAmount = BalanceAmount * -1;
                                    }
                                    else
                                    {
                                        BalanceAmount = CurrencyConverterAsync(AccountsAndFinanceObj.CurrencyName, "EGP", BalanceAmount).Result;
                                    }

                                }
                                // TotlaAmountTreasury += Accumulative;
                                if (AmountTreasuryType == "") // first time
                                {
                                    TotlaAmountTreasury = Math.Abs(BalanceAmount);
                                    AmountTreasuryType = FinalBlanaceType;
                                }
                                else // Calculation 
                                {
                                    if (FinalBlanaceType != AmountTreasuryType)
                                    {

                                        TotlaAmountTreasury = Math.Abs(TotlaAmountTreasury - Math.Abs(BalanceAmount));
                                        if (TotlaAmountTreasury < Math.Abs(BalanceAmount))
                                        {
                                            AmountTreasuryType = FinalBlanaceType;
                                        }
                                    }
                                    else
                                    {
                                        TotlaAmountTreasury = Math.Abs(TotlaAmountTreasury + Math.Abs(BalanceAmount));
                                    }
                                }
                            }
                        }
                        Response.Treasury = AccountAndFinanceTreasury;


                        // CAlc Sum Total ------
                        if (TotalAmountSum != 0)
                        {

                            if (AmountTreasuryType != TotalAmountSumType)
                            {

                                TotalAmountSum = Math.Abs(TotalAmountSum - Math.Abs(TotlaAmountTreasury));
                                if (TotalAmountSum < Math.Abs(TotlaAmountTreasury))
                                {
                                    TotalAmountSumType = AmountTreasuryType;
                                }
                            }
                            else
                            {
                                TotalAmountSum = Math.Abs(TotalAmountSum + Math.Abs(TotlaAmountTreasury));
                            }
                        }
                        else
                        {
                            TotalAmountSum = TotlaAmountTreasury;
                            TotalAmountSumType = AmountTreasuryType;
                        }

                        // Fill Account and Finance Promissiry ----------------------
                        var AccountAndFinancePromissory = new List<AccountsAndFinanceDetailsAdavanced>();

                        var PromissoryAdvanciedSettingAccountListDB = _unitOfWork.AdvanciedSettingAccounts.FindAll(x => x.Active == true && x.AdvanciedTypeId == 3).Select(x => x.AccountId).Distinct().ToList();

                        var AccountsPromissoryListDB = AccountsListDB.Where(x => PromissoryAdvanciedSettingAccountListDB.Contains(x.Id)).ToList();

                        if (AccountsPromissoryListDB.Count > 0)
                        {
                            foreach (var account in AccountsPromissoryListDB)
                            {
                                var AcumelativeListFromEntry = _unitOfWork.AccountOfJournalEntries.FindAll(x => x.EntryDate.Year == Year && x.AccountId == account.Id && x.Active == true).ToList();
                                if (Month != 0)
                                {
                                    AcumelativeListFromEntry = AcumelativeListFromEntry.Where(x => x.EntryDate.Month == Month).ToList();
                                }
                                if (Day != 0)
                                {
                                    AcumelativeListFromEntry = AcumelativeListFromEntry.Where(x => x.EntryDate.Day == Day).ToList();
                                }
                                decimal Accumulative = AcumelativeListFromEntry.Select(x => x.Amount).Sum();

                                string AccumulativeType = "";

                                if (account.AccountTypeName == "Credit")
                                {
                                    if (Accumulative > 0)
                                    {
                                        AccumulativeType = "C";
                                    }
                                    else
                                    {
                                        AccumulativeType = "D";
                                    }
                                }
                                else if (account.AccountTypeName == "Debit")
                                {
                                    if (Accumulative > 0)
                                    {
                                        AccumulativeType = "D";
                                    }
                                    else
                                    {
                                        AccumulativeType = "C";
                                    }
                                }
                                var AccountsAndFinanceObj = new AccountsAndFinanceDetailsAdavanced();
                                AccountsAndFinanceObj.AccountID = account.Id;
                                AccountsAndFinanceObj.AccountName = account.AccountName;
                                AccountsAndFinanceObj.ParentCategory = Common.GetParentCategory(account.Id, _Context);
                                AccountsAndFinanceObj.CurrencyName = account.CurrencyId != null ? Common.GetCurrencyName((int)account.CurrencyId, _Context) : "";
                                // AccountsAndFinanceObj.BalanceLE = Math.Abs(Accumulative).ToString() + " " + AccumulativeType;



                                decimal OpeningBalance = 0;
                                string BalanceType = "";
                                string OpennigBalanceType = "";
                                if (account.Comment != null && account.Comment != "")
                                {
                                    var Comment = string.Join("/", account.Comment);
                                    var ListComment = Comment.Split('/').ToList();
                                    OpeningBalance = ListComment[0] != "" ? decimal.Parse(ListComment[0]) : 0;
                                    OpennigBalanceType = ListComment[2];
                                    if (OpennigBalanceType == "Credit")
                                    {
                                        BalanceType = "C";
                                    }
                                    else
                                    {
                                        BalanceType = "D";
                                    }
                                }


                                //Accumulative = Math.Abs(Accumulative);
                                decimal BalanceAmount = 0;
                                if (BalanceType == AccumulativeType)
                                {
                                    BalanceAmount = Accumulative + OpeningBalance;
                                }
                                else
                                {
                                    BalanceAmount = Accumulative + (-1 * OpeningBalance);
                                }



                                string FinalBlanaceType = "";
                                if (BalanceAmount > 0)
                                {
                                    FinalBlanaceType = AccumulativeType;
                                }
                                else if (BalanceAmount < 0)
                                {
                                    if (AccumulativeType == "C")
                                    {
                                        FinalBlanaceType = "D";
                                    }
                                    else if (AccumulativeType == "D")
                                    {
                                        FinalBlanaceType = "C";
                                    }
                                }


                                AccountsAndFinanceObj.BalanceAmount = Math.Abs(BalanceAmount) + " " + FinalBlanaceType;


                                if (AccountsAndFinanceObj.CurrencyName != "EGP")
                                {
                                    if (BalanceAmount < 0)
                                    {

                                        BalanceAmount = CurrencyConverterAsync(AccountsAndFinanceObj.CurrencyName, "EGP", Math.Abs(BalanceAmount)).Result;
                                        BalanceAmount = BalanceAmount * -1;
                                    }
                                    else
                                    {
                                        BalanceAmount = CurrencyConverterAsync(AccountsAndFinanceObj.CurrencyName, "EGP", BalanceAmount).Result;
                                    }

                                }
                                AccountAndFinancePromissory.Add(AccountsAndFinanceObj);
                                //TotlaAmountPromissory += Accumulative;

                                if (AmountPromissoryBalanceType == "") // first time
                                {
                                    TotlaAmountPromissory = Math.Abs(BalanceAmount);
                                    AmountPromissoryBalanceType = FinalBlanaceType;
                                }
                                else // Calculation 
                                {
                                    if (FinalBlanaceType != AmountPromissoryBalanceType)
                                    {

                                        TotlaAmountPromissory = Math.Abs(TotlaAmountPromissory - Math.Abs(BalanceAmount));
                                        if (TotlaAmountPromissory < Math.Abs(BalanceAmount))
                                        {
                                            AmountPromissoryBalanceType = FinalBlanaceType;
                                        }
                                    }
                                    else
                                    {
                                        TotlaAmountPromissory = Math.Abs(TotlaAmountPromissory + Math.Abs(BalanceAmount));
                                    }
                                }
                            }
                        }
                        Response.Promissory = AccountAndFinancePromissory;
                        // CAlc Sum Total ------
                        if (TotalAmountSum != 0)
                        {

                            if (AmountPromissoryBalanceType != TotalAmountSumType)
                            {

                                TotalAmountSum = Math.Abs(TotalAmountSum - Math.Abs(TotlaAmountPromissory));
                                if (TotalAmountSum < Math.Abs(TotlaAmountPromissory))
                                {
                                    TotalAmountSumType = AmountPromissoryBalanceType;
                                }
                            }
                            else
                            {
                                TotalAmountSum = Math.Abs(TotalAmountSum + Math.Abs(TotlaAmountPromissory));
                            }
                        }
                        else
                        {
                            TotalAmountSum = TotlaAmountPromissory;
                            TotalAmountSumType = AmountPromissoryBalanceType;
                        }
                    }




                    Response.TotalBanksAmount = Math.Abs(TotlaAmountBanks) + " " + AmountBankBalanceType;
                    // Response.TotalTreasuryAmount = Math.Abs(TotlaAmountTreasury) + " " + TotlaAmountTreasuryType;
                    Response.TotalTreasuryAmount = Math.Abs(TotlaAmountTreasury) + " " + AmountTreasuryType;
                    Response.TotalPromissoryAmount = Math.Abs(TotlaAmountPromissory) + " " + AmountPromissoryBalanceType;
                    Response.TotalAmountSum = Math.Abs(TotalAmountSum) + " " + TotalAmountSumType;
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

        public async Task<decimal> CurrencyConverterAsync(string from, string to, decimal amount)
        {
            decimal result = 0;
            try
            {


                #region for exchangerate api with URL "https://exchangerate.host/"
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

        public AccountsDLLResponse GetAccountsList()
        {
            AccountsDLLResponse Response = new AccountsDLLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var AccountList = new List<AccountsDLL>();
                if (Response.Result)
                {
                    var AccountListDB = _unitOfWork.Accounts.FindAll(x => x.Active == true, includes: new[] { "AccountCategory" }).ToList();
                    if (AccountListDB.Count > 0)
                    {
                        foreach (var account in AccountListDB)
                        {
                            var AccountObj = new AccountsDLL();
                            AccountObj.ID = account.Id;
                            AccountObj.AccountName = account.AccountName;
                            AccountObj.CategoryName = account.AccountCategory?.AccountCategoryName;
                            AccountObj.HaveChild = account.Haveitem;
                            AccountObj.AccountTypeName = account.AccountTypeName;

                            AccountList.Add(AccountObj);
                        }
                    }
                }
                Response.AccountList = AccountList;
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

        public AccountsDLLResponse GetAccountAdvancedsList()
        {
            AccountsDLLResponse Response = new AccountsDLLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var AccountList = new List<AccountsDLL>();
                if (Response.Result)
                {
                    var AccountListDB = _unitOfWork.Accounts.FindAll(x => x.AdvanciedSettingsStatus == true).ToList();
                    if (AccountListDB.Count > 0)
                    {
                        foreach (var account in AccountListDB)
                        {
                            var AccountObj = new AccountsDLL();
                            AccountObj.ID = account.Id;
                            AccountObj.AccountName = account.AccountName;

                            AccountList.Add(AccountObj);
                        }
                    }
                }
                Response.AccountList = AccountList;
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

        public AccountsAndFinanceSpecificTreassuryResponse GetAccountAndFinanceSpecificTreasuryList([FromHeader] long? AccountID)
        {
            AccountsAndFinanceSpecificTreassuryResponse Response = new AccountsAndFinanceSpecificTreassuryResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var AccountsAndFinanceDayBalanceList = new List<AccountsAndFinanceDayBalance>();
                //cumulativeBalanceBefore And After
                decimal cumulativeBalanceBefore = 0;
                decimal cumulativeBalanceAfter = 0;
                decimal TotalSumAmount = 0;
                if (Response.Result)
                {
                    if (AccountID == 0 || AccountID == null)
                    {

                        Error error = new Error();
                        error.ErrorCode = "Err-P222";
                        error.ErrorMSG = "Invalid AccountID";
                        Response.Errors.Add(error);
                        Response.Result = false;
                    }
                    if (Response.Result)
                    {
                        var AccountOfJournyEntryLoadAllDB = _unitOfWork.AccountOfJournalEntries.FindAll(x => x.Active == true).ToList();
                        var AccountOfJournalEntryListDB = AccountOfJournyEntryLoadAllDB.Where(x => x.AccountId == AccountID).ToList();

                        var IDSEntriesForThisAccount = AccountOfJournyEntryLoadAllDB.Where(x => x.AccountId == AccountID).Select(x => x.EntryId).Distinct().ToList();

                        var AccountOfJournalEntryGroupedByListDB = AccountOfJournyEntryLoadAllDB.Where(x => x.AccountId != AccountID && IDSEntriesForThisAccount.Contains(x.EntryId)).OrderBy(x => x.EntryDate).GroupBy(x => x.EntryDate).ToList();


                        foreach (var DataGroupedByEntry in AccountOfJournalEntryGroupedByListDB)
                        {

                            var EntryDetailsPerDay = new List<AccountsAndFinanceDayBalance_Grouped>();


                            var EntryDetailsList = new List<AccountOfJournalEntryDetails>();
                            foreach (var EntryObj in DataGroupedByEntry)
                            {
                                var EntryDetails = new AccountOfJournalEntryDetails();
                                string input = EntryObj.FromOrTo;
                                string pattern = @"\d+$";
                                string replacement = "";
                                Regex rgx = new Regex(pattern);
                                string FromOrTo = rgx.Replace(input, replacement);

                                EntryDetails.AccountName = FromOrTo + " " + Common.GetAccountName(EntryObj.AccountId, _Context);
                                EntryDetails.CurrencyName = Common.GetCurrencyName(EntryObj.CurrencyId, _Context);
                                EntryDetails.Balance = EntryObj.Amount;
                                EntryDetails.EntryID = EntryObj.EntryId;
                                //EntryDetails.CreationMonth = Common.GetMonthName(EntryObj.EntryDate.Month);
                                //EntryDetails.CreationYear = EntryObj.EntryDate.TargetYear.ToString();
                                TotalSumAmount += EntryObj.Amount;
                                //TotalSumAmount += EntryObj.Amount;
                                //TotalSumAmount += EntryObj.Amount;
                                EntryDetailsList.Add(EntryDetails);
                            }



                            AccountsAndFinanceDayBalanceList.Add(new AccountsAndFinanceDayBalance()
                            {
                                // EntryName = "#" + DataGroupedByEntry.Key,
                                CreationDate = DataGroupedByEntry.Key.ToShortDateString(),
                                //item.Select(x=>x.CreatedDate.Month).ToString(),
                                EntryDetails = EntryDetailsList,
                            });


                        }



















                        if (AccountOfJournalEntryListDB.Count > 0)
                        {


                            // Cumlative before
                            var cumulativeBalanceBeforeObj = AccountOfJournalEntryListDB.FirstOrDefault();
                            if (cumulativeBalanceBeforeObj != null)
                            {
                                decimal AccBalance = cumulativeBalanceBeforeObj.AccBalance != null ? (decimal)cumulativeBalanceBeforeObj.AccBalance : 0;
                                if (AccBalance != 0)
                                {
                                    if (cumulativeBalanceBeforeObj.Debit != 0)
                                    {

                                        cumulativeBalanceBefore = AccBalance + cumulativeBalanceBeforeObj.Debit;
                                    }
                                    else if (cumulativeBalanceBeforeObj.Credit != 0)
                                    {
                                        cumulativeBalanceBefore = AccBalance - cumulativeBalanceBeforeObj.Credit;
                                    }
                                }
                            }

                            // Cumlative After
                            var cumulativeBalanceAfterObj = AccountOfJournalEntryListDB.LastOrDefault();
                            if (cumulativeBalanceAfterObj != null)
                            {
                                decimal AccBalance = cumulativeBalanceAfterObj.AccBalance != null ? (decimal)cumulativeBalanceAfterObj.AccBalance : 0;
                                if (AccBalance != 0)
                                {
                                    cumulativeBalanceAfter = AccBalance;
                                }
                            }

                        }
                    }
                    Response.DayBalanceList = AccountsAndFinanceDayBalanceList;



                    //Acumulative Balance Before
                    string AccountBalanceType = Common.GetAccountBalanceType((long)AccountID, _Context);
                    string FinalBlanaceTypeBeforAcc = "";
                    if (cumulativeBalanceBefore > 0)
                    {
                        FinalBlanaceTypeBeforAcc = AccountBalanceType;
                    }
                    else if (cumulativeBalanceBefore < 0)
                    {
                        if (AccountBalanceType == "C")
                        {
                            FinalBlanaceTypeBeforAcc = "D";
                        }
                        else if (AccountBalanceType == "D")
                        {
                            FinalBlanaceTypeBeforAcc = "C";
                        }
                    }
                    Response.CumulativeBalanceBefore = Math.Abs(cumulativeBalanceBefore) + " " + FinalBlanaceTypeBeforAcc;


                    //Acumulative Balance Before
                    string FinalBlanaceTypeAfterAcc = "";
                    if (cumulativeBalanceAfter > 0)
                    {
                        FinalBlanaceTypeAfterAcc = AccountBalanceType;
                    }
                    else if (cumulativeBalanceAfter < 0)
                    {
                        if (AccountBalanceType == "C")
                        {
                            FinalBlanaceTypeAfterAcc = "D";
                        }
                        else if (AccountBalanceType == "D")
                        {
                            FinalBlanaceTypeAfterAcc = "C";
                        }
                    }
                    Response.CumulativeBalanceAfter = Math.Abs(cumulativeBalanceAfter) + " " + FinalBlanaceTypeAfterAcc;



                    string FinalBlanaceTypeTotalAmount = "";
                    if (TotalSumAmount > 0)
                    {
                        FinalBlanaceTypeTotalAmount = AccountBalanceType;
                    }
                    else if (TotalSumAmount < 0)
                    {
                        if (AccountBalanceType == "C")
                        {
                            FinalBlanaceTypeTotalAmount = "D";
                        }
                        else if (AccountBalanceType == "D")
                        {
                            FinalBlanaceTypeTotalAmount = "C";
                        }
                    }
                    Response.TotalSumAmount = Math.Abs(TotalSumAmount) + " " + FinalBlanaceTypeTotalAmount;



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

        public AccountsAndFinanceSpecificTreassury_GroupedResponse GetAccountAndFinanceSpecificTreasuryList_GroupedBy([FromHeader] long? AccountID)
        {
            AccountsAndFinanceSpecificTreassury_GroupedResponse Response = new AccountsAndFinanceSpecificTreassury_GroupedResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                // var AccountsAndFinanceDayBalanceList = new List<AccountsAndFinanceDayBalance>();
                var BalancePerDateList = new List<BalancePerDate_Grouped>();

                //cumulativeBalanceBefore And After
                decimal cumulativeBalanceBefore = 0;
                decimal cumulativeBalanceAfter = 0;
                decimal TotalSumAmount = 0;
                if (Response.Result)
                {
                    if (AccountID == 0 || AccountID == null)
                    {

                        Error error = new Error();
                        error.ErrorCode = "Err-P222";
                        error.ErrorMSG = "Invalid AccountID";
                        Response.Errors.Add(error);
                        Response.Result = false;
                    }

                    if (Response.Result)
                    {
                        var AccountOfJournyEntryLoadAllDB = _unitOfWork.AccountOfJournalEntries.FindAll(x => x.Active == true).ToList();
                        var AccountOfJournalEntryListDB = AccountOfJournyEntryLoadAllDB.Where(x => x.AccountId == AccountID).ToList();





                        var IDSEntriesForThisAccount = AccountOfJournyEntryLoadAllDB.Where(x => x.AccountId == AccountID).Select(x => x.EntryId).Distinct().ToList();

                        var AccountOfJournalEntryGroupedByListDB = AccountOfJournyEntryLoadAllDB.Where(x => x.AccountId != AccountID && IDSEntriesForThisAccount.Contains(x.EntryId)).OrderBy(x => x.EntryDate).GroupBy(x => new { month = x.EntryDate.Month, year = x.EntryDate.Year }).ToList();


                        foreach (var DataGroupedByDate in AccountOfJournalEntryGroupedByListDB)
                        {
                            var EntryGroupByDayWithEntry = DataGroupedByDate.OrderBy(x => x.EntryDate.Day).GroupBy(x => new { Day = x.EntryDate.Day }).ToList();



                            var EntryDetailsPerDay = new List<AccountsAndFinanceDayBalance_Grouped>();
                            foreach (var Data in EntryGroupByDayWithEntry)
                            {


                                var EntryDetailsList = new List<AccountOfJournalEntryDetails>();
                                foreach (var EntryObj in Data)
                                {
                                    var EntryDetails = new AccountOfJournalEntryDetails();
                                    string input = EntryObj.FromOrTo;
                                    string pattern = @"\d+$";
                                    string replacement = "";
                                    Regex rgx = new Regex(pattern);
                                    string FromOrTo = rgx.Replace(input, replacement);

                                    EntryDetails.AccountName = FromOrTo + " " + Common.GetAccountName(EntryObj.AccountId, _Context);
                                    EntryDetails.CurrencyName = Common.GetCurrencyName(EntryObj.CurrencyId, _Context);
                                    EntryDetails.Balance = EntryObj.Amount;
                                    //EntryDetails.CreationMonth = Common.GetMonthName(EntryObj.EntryDate.Month);
                                    //EntryDetails.CreationYear = EntryObj.EntryDate.TargetYear.ToString();
                                    TotalSumAmount += EntryObj.Amount;
                                    //TotalSumAmount += EntryObj.Amount;
                                    //TotalSumAmount += EntryObj.Amount;
                                    EntryDetailsList.Add(EntryDetails);
                                }

                                EntryDetailsPerDay.Add(new AccountsAndFinanceDayBalance_Grouped
                                {
                                    DayName = Data.Select(x => x.EntryDate.ToShortDateString()).FirstOrDefault(),
                                    // EntryName = "Entry #" + Data.Key.EntryID,
                                    EntryDetails = EntryDetailsList
                                });

                            }
                            BalancePerDateList.Add(new BalancePerDate_Grouped()
                            {
                                CreationDate = Common.GetMonthName(DataGroupedByDate.Key.month) + " " + DataGroupedByDate.Key.year.ToString(),
                                //item.Select(x=>x.CreatedDate.Month).ToString(),
                                EntryDetailsPerDay = EntryDetailsPerDay,
                            });






                        }



                        if (AccountOfJournalEntryListDB.Count > 0)
                        {
                            //    foreach (var AccountJournalOBJ in AccountOfJournalEntryListDB)
                            //    {
                            //        var AccountsAndFinanceDayBalanceObj = new AccountsAndFinanceDayBalance();
                            //        AccountsAndFinanceDayBalanceObj.EntryName = "Entry #" + AccountJournalOBJ.EntryID;
                            //        AccountsAndFinanceDayBalanceObj.CreationDate = AccountJournalOBJ.EntryDate.ToShortDateString();

                            //        var EntryDetailsList = new List<AccountOfJournalEntryDetails>();
                            //        var JournalEntryDetailsListDB = AccountOfJournyEntryLoadAllDB.Where(x => x.EntryID == AccountJournalOBJ.EntryID && x.AccountID != AccountID).ToList();
                            //        if (JournalEntryDetailsListDB.Count > 0)
                            //        {
                            //            foreach (var EntryObj in JournalEntryDetailsListDB)
                            //            {
                            //                var EntryDetails = new AccountOfJournalEntryDetails();
                            //                string input = EntryObj.FromOrTo;
                            //                string pattern = @"\d+$";
                            //                string replacement = "";
                            //                Regex rgx = new Regex(pattern);
                            //                string FromOrTo = rgx.Replace(input, replacement);

                            //                EntryDetails.AccountName = FromOrTo + " " + Common.GetAccountName(EntryObj.AccountID);
                            //                EntryDetails.currencyName = Common.GetCurrencyName(EntryObj.CurrencyID);
                            //                EntryDetails.Balance = EntryObj.Amount;
                            //                //EntryDetails.CreationMonth = Common.GetMonthName(EntryObj.EntryDate.Month);
                            //                //EntryDetails.CreationYear = EntryObj.EntryDate.TargetYear.ToString();
                            //                TotalSumAmount += EntryObj.Amount;
                            //                //TotalSumAmount += EntryObj.Amount;
                            //                //TotalSumAmount += EntryObj.Amount;
                            //                EntryDetailsList.Add(EntryDetails);
                            //            }
                            //        }
                            //        AccountsAndFinanceDayBalanceObj.EntryDetails = EntryDetailsList;
                            //        AccountsAndFinanceDayBalanceList.Add(AccountsAndFinanceDayBalanceObj);
                            //    }

                            // Cumlative before
                            var cumulativeBalanceBeforeObj = AccountOfJournalEntryListDB.FirstOrDefault();
                            if (cumulativeBalanceBeforeObj != null)
                            {
                                decimal AccBalance = cumulativeBalanceBeforeObj.AccBalance != null ? (decimal)cumulativeBalanceBeforeObj.AccBalance : 0;
                                if (AccBalance != 0)
                                {
                                    if (cumulativeBalanceBeforeObj.Debit != 0)
                                    {

                                        cumulativeBalanceBefore = AccBalance + cumulativeBalanceBeforeObj.Debit;
                                    }
                                    else if (cumulativeBalanceBeforeObj.Credit != 0)
                                    {
                                        cumulativeBalanceBefore = AccBalance - cumulativeBalanceBeforeObj.Credit;
                                    }
                                }
                            }

                            // Cumlative After
                            var cumulativeBalanceAfterObj = AccountOfJournalEntryListDB.LastOrDefault();
                            if (cumulativeBalanceAfterObj != null)
                            {
                                decimal AccBalance = cumulativeBalanceAfterObj.AccBalance != null ? (decimal)cumulativeBalanceAfterObj.AccBalance : 0;
                                if (AccBalance != 0)
                                {
                                    cumulativeBalanceAfter = AccBalance;
                                }
                            }

                        }
                    }
                    Response.BalanceListPerDate = BalancePerDateList;



                    //Acumulative Balance Before
                    string AccountBalanceType = Common.GetAccountBalanceType((long)AccountID, _Context);
                    string FinalBlanaceTypeBeforAcc = "";
                    if (cumulativeBalanceBefore > 0)
                    {
                        FinalBlanaceTypeBeforAcc = AccountBalanceType;
                    }
                    else if (cumulativeBalanceBefore < 0)
                    {
                        if (AccountBalanceType == "C")
                        {
                            FinalBlanaceTypeBeforAcc = "D";
                        }
                        else if (AccountBalanceType == "D")
                        {
                            FinalBlanaceTypeBeforAcc = "C";
                        }
                    }
                    Response.CumulativeBalanceBefore = Math.Abs(cumulativeBalanceBefore) + " " + FinalBlanaceTypeBeforAcc;


                    //Acumulative Balance Before
                    string FinalBlanaceTypeAfterAcc = "";
                    if (cumulativeBalanceAfter > 0)
                    {
                        FinalBlanaceTypeAfterAcc = AccountBalanceType;
                    }
                    else if (cumulativeBalanceAfter < 0)
                    {
                        if (AccountBalanceType == "C")
                        {
                            FinalBlanaceTypeAfterAcc = "D";
                        }
                        else if (AccountBalanceType == "D")
                        {
                            FinalBlanaceTypeAfterAcc = "C";
                        }
                    }
                    Response.CumulativeBalanceAfter = Math.Abs(cumulativeBalanceAfter) + " " + FinalBlanaceTypeAfterAcc;



                    string FinalBlanaceTypeTotalAmount = "";
                    if (TotalSumAmount > 0)
                    {
                        FinalBlanaceTypeTotalAmount = AccountBalanceType;
                    }
                    else if (TotalSumAmount < 0)
                    {
                        if (AccountBalanceType == "C")
                        {
                            FinalBlanaceTypeTotalAmount = "D";
                        }
                        else if (AccountBalanceType == "D")
                        {
                            FinalBlanaceTypeTotalAmount = "C";
                        }
                    }
                    Response.TotalSumAmount = Math.Abs(TotalSumAmount) + " " + FinalBlanaceTypeTotalAmount;



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

        public AccountsAndFinanceClientReportResponse GetAccountAndFinanceClientReportList([FromHeader] long? ClientID, [FromHeader] long? ProjectID, [FromHeader] long? ProjectTypeID, [FromHeader] DateTime? ProjectCreationFrom, [FromHeader] DateTime? ProjectCreationTo)
        {
            AccountsAndFinanceClientReportResponse Response = new AccountsAndFinanceClientReportResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ClientReportDetailsList = new List<ClientReportDetails>();
                //cumulativeBalanceBefore And After
                decimal TotalSalesVolume = 0;
                decimal TotalCollected = 0;
                decimal Remain = 0;
                int TotalCountOFProject = 0;

                if (Response.Result)
                {
                    long ClientSelectedID = 0;
                    if (ClientID != 0 && ClientID != null)
                    {
                        ClientSelectedID = ClientID ?? 0;
                    }
                    long ProjectSelectedID = 0;
                    if (ProjectID != 0 && ProjectID != null)
                    {
                        ProjectSelectedID = ProjectID ?? 0;
                    }
                    long ProjectSelectedTypeID = 0;
                    if (ProjectTypeID != 0 && ProjectTypeID != null)
                    {
                        ProjectSelectedTypeID = ProjectTypeID ?? 0;
                    }

                    if (Response.Result)
                    {



                        DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
                        DateTime EndDate = DateTime.Now;




                        if (ProjectCreationFrom != null)
                        {
                            DateTime ProjectCreateFrom = ProjectCreationFrom ?? DateTime.Now;
                            //DateTime.TryParse(Request.Headers["ProjectCreationFrom"], ProjectCreateFrom)
                            /*if (!DateTime.TryParse(Request.Headers["ProjectCreationFrom"], out ProjectCreateFrom))
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err-12";
                                error.ErrorMSG = "Invalid Project Creation From";
                                Response.Errors.Add(error);
                                Response.Result = false;
                                return Response;
                            }*/
                            StartYear = ProjectCreateFrom;
                            // ProjectSalesOfferListDB = ProjectSalesOfferListDB.Where(x => x.ProjectCreationDate >= ProjectCreateFrom).ToList();
                        }

                        if (ProjectCreationTo != null)
                        {
                            DateTime ProjectCreateTo = ProjectCreationTo ?? DateTime.Now;
                            //DateTime ProjectCreationTo = DateTime.Parse(Request.Headers["ProjectCreationTo"]);
                            /*if (!DateTime.TryParse(Request.Headers["ProjectCreationTo"], out ProjectCreationTo))
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err-13";
                                error.ErrorMSG = "Invalid Project Creation To";
                                Response.Errors.Add(error);
                                Response.Result = false;
                                return Response;
                            }*/
                            EndDate = ProjectCreateTo;
                            // ProjectSalesOfferListDB = ProjectSalesOfferListDB.Where(x => x.ProjectCreationDate <= ProjectCreationTo).ToList();
                        }

                        // Not Grouped -----------------------------------------
                        List<long> IDSProject = new List<long>();
                        if (ProjectCreationTo != null)
                        {
                            IDSProject = _Context.ClientAccounts.Where(x => x.CreationDate >= StartYear && x.CreationDate < EndDate && x.ProjectId != null).Select(x => (long)x.ProjectId).Distinct().ToList();
                        }
                        var ProjectSalesOfferListDB = _unitOfWork.VProjectSalesOfferClients.FindAll(x => x.ProjectActive == true && x.ProjectCreationDate >= StartYear && x.ProjectCreationDate < DateTime.Now).ToList();

                        if (ProjectCreationTo != null)
                        {
                            if (IDSProject.Count() > 0)
                            {
                                //var ListOLDProjectColectedThisYear =  _Context.V_Project_SalesOffer_Client.Where(x => IDSProject.Contains(x.ProjectId)).ToList();
                                ProjectSalesOfferListDB = _unitOfWork.VProjectSalesOfferClients.FindAll(x => (x.ProjectActive == true && x.ProjectCreationDate >= StartYear && x.ProjectCreationDate < EndDate) || IDSProject.Contains(x.ProjectId)).ToList();
                            }
                        }



                        if (ClientSelectedID != 0)
                        {
                            ProjectSalesOfferListDB = ProjectSalesOfferListDB.Where(x => x.ClientId == ClientSelectedID).ToList();
                        }
                        if (ProjectSelectedID != 0)
                        {
                            ProjectSalesOfferListDB = ProjectSalesOfferListDB.Where(x => x.ProjectId == ProjectSelectedID).ToList();
                        }
                        if (ProjectTypeID != 0)
                        {
                            if (ProjectTypeID == 1) //New Project
                            {
                                ProjectSalesOfferListDB = ProjectSalesOfferListDB.Where(x => x.OfferType == "New Project Offer").ToList();
                            }
                            else if (ProjectTypeID == 2) // Maintenance
                            {
                                ProjectSalesOfferListDB = ProjectSalesOfferListDB.Where(x => x.OfferType == "New Maintenance Offer").ToList();
                            }
                            else if (ProjectTypeID == 3) // Rent
                            {
                                ProjectSalesOfferListDB = ProjectSalesOfferListDB.Where(x => x.OfferType == "New Rent Offer").ToList();
                            }
                            else if (ProjectTypeID == 4) // InternalProject
                            {
                                ProjectSalesOfferListDB = ProjectSalesOfferListDB.Where(x => x.OfferType == "New Internal Order").ToList();
                            }
                            else if (ProjectTypeID == 5) // Warrenty
                            {
                                ProjectSalesOfferListDB = ProjectSalesOfferListDB.Where(x => x.MaintenanceType == "Warrenty").ToList();
                            }
                        }

                        if (ProjectSalesOfferListDB.Count > 0)
                        {
                            var ClientsList = ProjectSalesOfferListDB.Select(x => x.ClientId).Distinct().ToList();
                            foreach (var ClientId in ClientsList)
                            {
                                var ClientOBJ = new ClientReportDetails();
                                ClientOBJ.ClientProjectsList = new List<ClientProjectDetails>();
                                ClientOBJ.ClientName = ClientId != null ? Common.GetClientName((long)ClientId, _Context) : "";
                                ClientOBJ.ClientID = ClientId != null ? (long)ClientId : 0;
                                decimal ClientVolume = 0;
                                decimal ClientCollected = 0;
                                decimal ClientRemain = 0;

                                var ProjectsPerClient = ProjectSalesOfferListDB.Where(x => x.ClientId == ClientId).ToList();
                                foreach (var ProjectDetailsDataOBJ in ProjectsPerClient)
                                {
                                    TotalCountOFProject++;
                                    var ClientProjectObj = new ClientProjectDetails();
                                    ClientProjectObj.ProjectName = ProjectDetailsDataOBJ.ProjectName;
                                    ClientProjectObj.ProjectID = ProjectDetailsDataOBJ.ProjectId;
                                    // Volume Per Project and sumution to supplier
                                    decimal FinalOfferPrice = ProjectDetailsDataOBJ.FinalOfferPrice != null ? (decimal)ProjectDetailsDataOBJ.FinalOfferPrice : 0;
                                    decimal ExtraCost = ProjectDetailsDataOBJ.ProjectExtraCost != null ? (decimal)ProjectDetailsDataOBJ.ProjectExtraCost : 0;
                                    ClientProjectObj.Volume = FinalOfferPrice + ExtraCost;
                                    ClientVolume += ClientProjectObj.Volume;

                                    // Collected and sumution to supplier


                                    decimal collected = 0;

                                    var clientAccounts = _unitOfWork.ClientAccounts.FindAll(x => x.Active == true && x.ProjectId == ProjectDetailsDataOBJ.ProjectId && x.ClientId == ProjectDetailsDataOBJ.ClientId
                                                                                                           && x.CreationDate >= StartYear && x.CreationDate <= DateTime.Now).ToList();



                                    if (clientAccounts.Count() > 0)
                                    {
                                        var pluss = clientAccounts.Where(x => x.AmountSign == "plus").Sum(x => x.Amount);
                                        var minus = clientAccounts.Where(x => x.AmountSign == "minus").Sum(x => x.Amount);
                                        collected = pluss - minus;
                                        //foreach (var ClientAccOBJ in clientAccounts)
                                        //{
                                        //    if (ClientAccOBJ.AmountSign == "plus")
                                        //    {
                                        //        collected = collected + ClientAccOBJ.Amount;
                                        //    }
                                        //    else if (ClientAccOBJ.AmountSign == "minus")
                                        //    {
                                        //        collected = collected - ClientAccOBJ.Amount;
                                        //    }
                                        //}
                                    }
                                    ClientProjectObj.CreationDate = ProjectDetailsDataOBJ.ProjectCreationDate.ToShortDateString();
                                    ClientProjectObj.Collected = collected;
                                    ClientProjectObj.Remain = ClientProjectObj.Volume - collected;
                                    ClientProjectObj.CollectedPercent = ClientProjectObj.Volume != 0 ? (ClientProjectObj.Collected / ClientProjectObj.Volume) * 100 : 0;

                                    ClientCollected += ClientProjectObj.Collected;
                                    ClientRemain += ClientProjectObj.Remain;


                                    // fill Project Per supplier
                                    ClientOBJ.ClientProjectsList.Add(ClientProjectObj);
                                }

                                ClientOBJ.Volume = ClientVolume;
                                ClientOBJ.Collected = ClientCollected;
                                ClientOBJ.Remain = ClientRemain;
                                ClientOBJ.CollectedPercent = ClientOBJ.Volume != 0 ? (ClientOBJ.Collected / ClientOBJ.Volume) * 100 : 0;


                                // fill supplier List ------------------
                                ClientReportDetailsList.Add(ClientOBJ);
                                TotalSalesVolume += ClientOBJ.Volume;
                                TotalCollected += ClientOBJ.Collected;
                                Remain += ClientOBJ.Remain;
                            }
                        }



                    }
                    Response.ClientList = ClientReportDetailsList;
                    Response.TotalSalesVolume = TotalSalesVolume;
                    Response.TotalCreationProjectYTD = Common.GetTotalSalesForceClientReportAmount(_Context);
                    Response.TotalCreationProjectYTDTotalCollected = Common.GetTotalProjectsCollected2021WithOutInternal(_Context);
                    Response.TotalCreationProjectYTDTotalCollectedPercent = Response.TotalCreationProjectYTD != 0 ? Math.Round((Response.TotalCreationProjectYTDTotalCollected / Response.TotalCreationProjectYTD) * 100) : 0;
                    Response.TotalCollected = TotalCollected;
                    Response.TotalCollectedPercent = TotalSalesVolume != 0 ? Math.Round((TotalCollected / TotalSalesVolume) * 100) : 0;
                    Response.TotalProjectCount = TotalCountOFProject;
                    Response.Remain = Remain;
                    var Project2021CollectedWithoutInternal = Common.GetTotalProjectsCollected2021WithOutInternal(_Context);

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

        public AccountsAndFinanceSupplierReportResponse GetAccountAndFinanceSupplierReportList(long? SupplierID, long? POID, DateTime? POCreationFrom, DateTime? POCreationTo)
        {
            AccountsAndFinanceSupplierReportResponse Response = new AccountsAndFinanceSupplierReportResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var SupplierReportDetailsList = new List<SupplierReportDetails>();
                decimal TotalSalesVolume = 0;
                decimal TotalCollected = 0;
                decimal Remain = 0;
                int TotalCountOFPO = 0;

                if (Response.Result)
                {
                    long SupplierSelectedID = 0;
                    if (SupplierID != 0 && SupplierID != null)
                    {
                        SupplierSelectedID = SupplierID ?? 0;
                    }
                    long POId = 0;
                    if (POID != 0 && POID != null)
                    {
                        POId = POID ?? 0;
                    }

                    if (Response.Result)
                    {
                        // Not Grouped -----------------------------------------
                        DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
                        List<long> PurchasePOIDs = new List<long>();

                        if (POCreationFrom==null)
                        {
                            PurchasePOIDs = _unitOfWork.SupplierAccounts.FindAll(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now && x.PurchasePoid != null).Select(x => (long)x.PurchasePoid).Distinct().ToList();
                        }
                        var SupplierPurchasePOListDB = _unitOfWork.VPurchasePos.FindAll(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now).ToList();


                        if (POCreationFrom==null)
                        {
                            SupplierPurchasePOListDB = _unitOfWork.VPurchasePos.FindAll(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now || PurchasePOIDs.Contains(x.Id)).ToList();
                            // SupplierPurchasePOListDB = SupplierPurchasePOListDB.Where(x => PurchasePOIDs.Contains(x.ID)).ToList();
                        }


                        if (POCreationFrom != null)
                        {
                            
                            SupplierPurchasePOListDB = SupplierPurchasePOListDB.Where(x => x.CreationDate >= (POCreationFrom??DateTime.Now)).ToList();
                        }

                        if (POCreationTo != null)
                        {

                            SupplierPurchasePOListDB = SupplierPurchasePOListDB.Where(x => x.CreationDate <= (POCreationTo??DateTime.Now)).ToList();
                        }







                        if (POID != 0 && POID != null)
                        {
                            SupplierPurchasePOListDB = SupplierPurchasePOListDB.Where(x => x.Id == POID).ToList();
                        }
                        if (SupplierSelectedID != 0  && SupplierSelectedID != null)
                        {
                            SupplierPurchasePOListDB = SupplierPurchasePOListDB.Where(x => x.ToSupplierId == SupplierSelectedID).ToList();
                        }

                        var SupplierGroupedBy = SupplierPurchasePOListDB.GroupBy(x => new { SupplierID = x.ToSupplierId }).ToList();
                        foreach (var POPerSupplier in SupplierGroupedBy)
                        {
                            var SupplierOBJ = new SupplierReportDetails();
                            SupplierOBJ.SupplierPOsList = new List<SupplierPODetails>();
                            SupplierOBJ.SupplierName = Common.GetSupplierName(POPerSupplier.Key.SupplierID, _Context);
                            decimal SupplierVolume = 0;
                            decimal SupplierCollected = 0;
                            decimal SupplierRemain = 0;


                            foreach (var Data in POPerSupplier)
                            {
                                TotalCountOFPO++;

                                var SupplierPOObj = new SupplierPODetails();
                                SupplierPOObj.PO = "PO_" + Data.Id;
                                SupplierPOObj.Cost = Data.TotalInvoiceCost != null ? (decimal)Data.TotalInvoiceCost : 0;
                                SupplierPOObj.Price = Data.TotalInvoicePrice != null ? (decimal)Data.TotalInvoicePrice : 0;
                                SupplierVolume += SupplierPOObj.Cost;
                                // Collected and sumution to supplier


                                decimal collected = 0;

                                var SupplierAccounts = _unitOfWork.SupplierAccounts.FindAll(x => x.Active == true && x.PurchasePoid == Data.Id && x.SupplierId == Data.ToSupplierId
                                                                                                                 && x.CreationDate >= StartYear && x.CreationDate <= DateTime.Now).ToList();
                                if (SupplierAccounts.Count() > 0)
                                {
                                    foreach (var SupplierAccOBJ in SupplierAccounts)
                                    {
                                        if (SupplierAccOBJ.AmountSign == "plus")
                                        {
                                            collected = collected + SupplierAccOBJ.Amount;
                                        }
                                        else if (SupplierAccOBJ.AmountSign == "minus")
                                        {
                                            collected = collected - SupplierAccOBJ.Amount;
                                        }
                                    }
                                }
                                SupplierPOObj.Collected = collected;
                                SupplierPOObj.Remain = SupplierPOObj.Cost - collected;
                                SupplierPOObj.PaidPercent = SupplierPOObj.Cost != 0 ? (SupplierPOObj.Collected / SupplierPOObj.Cost) * 100 : 0;

                                SupplierCollected += SupplierPOObj.Collected;
                                SupplierRemain += SupplierPOObj.Remain;


                                // fill Project Per supplier
                                SupplierOBJ.SupplierPOsList.Add(SupplierPOObj);

                            }
                            SupplierOBJ.Volume = SupplierVolume;
                            SupplierOBJ.Collected = SupplierCollected;
                            SupplierOBJ.Remain = SupplierRemain;
                            SupplierOBJ.PaidPercent = SupplierOBJ.Volume != 0 ? (SupplierOBJ.Collected / SupplierOBJ.Volume) * 100 : 0;


                            // fill supplier List ------------------
                            SupplierReportDetailsList.Add(SupplierOBJ);
                            TotalSalesVolume += SupplierOBJ.Volume;
                            TotalCollected += SupplierOBJ.Collected;
                            Remain += SupplierOBJ.Remain;

                        }










                        //if (SupplierPurchasePOListDB.Count > 0)
                        //{
                        //    if (POID != 0)
                        //    {
                        //        SupplierPurchasePOListDB = SupplierPurchasePOListDB.Where(x => x.ID == POID).ToList();
                        //    }
                        //    if (SupplierSelectedID != 0)
                        //    {
                        //        SupplierPurchasePOListDB = SupplierPurchasePOListDB.Where(x => x.ToSupplierID == SupplierSelectedID).ToList();
                        //    }
                        //    var SupplierList = SupplierPurchasePOListDB.Select(x => x.ToSupplierID).Distinct().ToList();

                        //    foreach (var SupplierID in SupplierList)
                        //    {
                        //        var SupplierOBJ = new SupplierReportDetails();
                        //        SupplierOBJ.SupplierPOsList = new List<SupplierPODetails>();
                        //        SupplierOBJ.SupplierName = Common.GetSupplierName((long)SupplierID);
                        //        decimal SupplierVolume = 0;
                        //        decimal SupplierCollected = 0;
                        //        decimal SupplierRemain = 0;

                        //        var POPerSupplier = SupplierPurchasePOListDB.Where(x => x.ToSupplierID == SupplierID).ToList();
                        //        foreach (var SupplierPerPO in POPerSupplier)
                        //        {
                        //            TotalCountOFPO++;
                        //            var SupplierPOObj = new SupplierPODetails();
                        //            SupplierPOObj.PO = "PO_" + SupplierPerPO.ID;
                        //            SupplierPOObj.Cost = SupplierPerPO.TotalInvoiceCost != null ? (decimal)SupplierPerPO.TotalInvoiceCost : 0;
                        //            SupplierPOObj.Price = SupplierPerPO.TotalInvoicePrice != null ? (decimal)SupplierPerPO.TotalInvoicePrice : 0;
                        //            SupplierVolume += SupplierPOObj.Cost;
                        //            // Collected and sumution to supplier


                        //            decimal collected = 0;

                        //            var SupplierAccounts = _Context.proc_SupplierAccountsLoadAll().Where(x => x.PurchasePOID == SupplierPerPO.ID && x.SupplierID == SupplierPerPO.ToSupplierID
                        //                                                                                             && x.CreationDate >= StartYear && x.CreationDate <= DateTime.Now).ToList();
                        //            if (SupplierAccounts.Count() > 0)
                        //            {
                        //                foreach (var SupplierAccOBJ in SupplierAccounts)
                        //                {
                        //                    if (SupplierAccOBJ.AmountSign == "plus")
                        //                    {
                        //                        collected = collected + SupplierAccOBJ.Amount;
                        //                    }
                        //                    else if (SupplierAccOBJ.AmountSign == "minus")
                        //                    {
                        //                        collected = collected - SupplierAccOBJ.Amount;
                        //                    }
                        //                }
                        //            }
                        //            SupplierPOObj.Collected = collected;
                        //            SupplierPOObj.Remain = SupplierPOObj.Cost - collected;
                        //            SupplierPOObj.PaidPercent = SupplierPOObj.Cost != 0 ? (SupplierPOObj.Collected / SupplierPOObj.Cost) * 100 : 0;

                        //            SupplierCollected += SupplierPOObj.Collected;
                        //            SupplierRemain += SupplierPOObj.Remain;


                        //            // fill Project Per supplier
                        //            SupplierOBJ.SupplierPOsList.Add(SupplierPOObj);
                        //        }

                        //        SupplierOBJ.Volume = SupplierVolume;
                        //        SupplierOBJ.Collected = SupplierCollected;
                        //        SupplierOBJ.Remain = SupplierRemain;
                        //        SupplierOBJ.PaidPercent = SupplierOBJ.Volume != 0 ? (SupplierOBJ.Collected / SupplierOBJ.Volume) * 100 : 0;


                        //        // fill supplier List ------------------
                        //        SupplierReportDetailsList.Add(SupplierOBJ);
                        //        TotalSalesVolume += SupplierOBJ.Volume;
                        //        TotalCollected += SupplierOBJ.Collected;
                        //        Remain += SupplierOBJ.Remain;
                        //    }
                        //}



                    }
                    Response.SupplierList = SupplierReportDetailsList;
                    Response.TotalSalesVolume = TotalSalesVolume;
                    Response.TotalCollected = TotalCollected;
                    Response.Remain = Remain;
                    Response.TotalPOCount = TotalCountOFPO;
                    Response.TotalCreationPOYTD = Common.GetTotalPurchasingAndSupplierReportAmount(_Context);
                    Response.TotalCollectedPercent = TotalSalesVolume != 0 ? Math.Round((TotalCollected / TotalSalesVolume) * 100) : 0;


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

        public async Task<BaseMessageResponse> AccountTreeExcel([FromHeader] AccountTreeExcelHeader header)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var GetAccountTreeExcelList = new List<TreeViewAccount>();


                if (Response.Result)
                {





                    /*bool CalcWithoutPrivate = false;
                    if (!string.IsNullOrEmpty(Request.Headers["CalcWithoutPrivate"]))
                    {
                        CalcWithoutPrivate = bool.Parse(Request.Headers["CalcWithoutPrivate"]);
                    }*/

                    //bool OrderByCreationDate = false;
                    //if (!string.IsNullOrEmpty(headers["OrderByCreationDate"]))
                    //{
                    //    OrderByCreationDate = bool.Parse(headers["OrderByCreationDate"]);
                    //}

                    DateTime FromDate = new DateTime(DateTime.Now.Year, 1, 1);
                    if (!string.IsNullOrEmpty(header.FromDate) && DateTime.TryParse(header.FromDate, out FromDate))
                    {
                        FromDate = DateTime.Parse(header.FromDate);
                    }


                    DateTime ToDate = new DateTime(DateTime.Now.Year + 1, 1, 1);
                    if (!string.IsNullOrEmpty(header.ToDate) && DateTime.TryParse(header.ToDate, out ToDate))
                    {
                        ToDate = DateTime.Parse(header.ToDate);
                    }


                    var AcccountTreeDataList = _Context.VAccounts.OrderBy(x => x.AccountNumber).ToList();
                    var dt = new System.Data.DataTable("grid");

                    //foreach (var accountiditem in AcccountTreeDataList)
                    //{

                    var categoriesId = AcccountTreeDataList.Select(a => a.AccountCategoryId);
                    var categoriesName = categoriesId.Select(a => Common.GetAccountCategoryName(a, _Context));

                    foreach (var item in AcccountTreeDataList)
                    {
                        var accountmanagementlistdb = new TreeViewAccount();
                        accountmanagementlistdb.title = item.AccountName;
                        accountmanagementlistdb.Code = item.AccountNumber;
                        accountmanagementlistdb.AdvanciedTypeName = item.AdvanciedTypeName;
                        accountmanagementlistdb.Category = categoriesName.ElementAt(AcccountTreeDataList.IndexOf(item));
                        var AccountID = new SqlParameter("AccountID", System.Data.SqlDbType.BigInt);
                        AccountID.Value = item.Id;
                        var CalcWithoutPrivateJE = new SqlParameter("CalcWithoutPrivateJE", System.Data.SqlDbType.Bit);
                        CalcWithoutPrivateJE.Value = header.CalcWithoutPrivate;
                        var OrderByCreationDate = new SqlParameter("OrderByCreationDate", System.Data.SqlDbType.Bit);
                        OrderByCreationDate.Value = true;
                        var CreationDateFrom = new SqlParameter("CreationDateFrom", System.Data.SqlDbType.DateTime);
                        CreationDateFrom.Value = new DateTime(DateTime.Now.Year, 1, 1);
                        var CreationDateTo = new SqlParameter("CreationDateTo ", System.Data.SqlDbType.DateTime);
                        CreationDateTo.Value = new DateTime(DateTime.Now.Year + 1, 1, 1);
                        object[] param = new object[] { AccountID, CalcWithoutPrivateJE, OrderByCreationDate, CreationDateFrom, CreationDateTo };
                        accountmanagementlistdb.Credit = _Context.Database.SqlQueryRaw<STP_AccountMovement_Result>("Exec STP_AccountMovement @AccountID ,@CalcWithoutPrivateJE ,@OrderByCreationDate ,@CreationDateFrom ,@CreationDateTo", param).AsEnumerable().Select(x => x.Credit).Sum();
                        accountmanagementlistdb.Debit = _Context.Database.SqlQueryRaw<STP_AccountMovement_Result>("Exec STP_AccountMovement @AccountID ,@CalcWithoutPrivateJE ,@OrderByCreationDate ,@CreationDateFrom ,@CreationDateTo", param).AsEnumerable().Select(x => x.Debit).Sum();
                        accountmanagementlistdb.Type = item.AccountTypeName;
                        accountmanagementlistdb.Accumulative = _Context.Database.SqlQueryRaw<STP_AccountMovement_Result>("Exec STP_AccountMovement @AccountID ,@CalcWithoutPrivateJE ,@OrderByCreationDate ,@CreationDateFrom ,@CreationDateTo", param).AsEnumerable().Select(x => x.Acc_Calc).LastOrDefault() ?? 0;
                        accountmanagementlistdb.Currency = Common.GetCurrencyName((int)item.CurrencyId, _Context);
                        accountmanagementlistdb.Active = item.Active;
                        accountmanagementlistdb.HasChild = item.Haveitem;
                        GetAccountTreeExcelList.Add(accountmanagementlistdb);
                    }
                    dt.Columns.AddRange(new DataColumn[11] {
                                                     new DataColumn("Account Name"),
                                                     new DataColumn("Code"),
                                                     new DataColumn("Advanced Type Name"),
                                                     new DataColumn("Category"),
                                                     new DataColumn("Type"),
                                                     new DataColumn("Debit") ,
                                                     new DataColumn("Credit"),
                                                     new DataColumn("Accumulative") ,
                                                     new DataColumn("Cu"),
                                                     new DataColumn("Active"),
                                                     new DataColumn("Have Item")

                    });
                    if (GetAccountTreeExcelList != null)
                    {
                        foreach (var item in GetAccountTreeExcelList)
                        {
                            dt.Rows.Add(
                                item.title,
                                item.Code,
                                item.AdvanciedTypeName != null ? item.AdvanciedTypeName : "N/A",
                                item.Category,
                                item.Type != null ? item.Type : "-",
                                item.Debit != null ? item.Debit : 0,
                                item.Credit,
                                item.Accumulative != null ? item.Accumulative : 0,
                                item.Currency != null ? item.Currency : "-",
                                item.Active,
                                item.HasChild

                                );
                        }
                    }
                    using (ExcelPackage packge = new ExcelPackage())
                    {
                        //Create the worksheet
                        ExcelWorksheet ws = packge.Workbook.Worksheets.Add("AccountTree");
                        ws.TabColor = System.Drawing.Color.Red;
                        ws.Columns.BestFit = true;
                        for (var i = 2; i <= 6; i++)
                        {
                            ws.Row(i).OutlineLevel = 1;
                            //worksheet.Row(i).Collapsed = true;
                        }

                        //Row Group 2
                        for (var i = 7; i <= 11; i++)
                        {
                            ws.Row(i).OutlineLevel = 1;
                            //worksheet.Row(i).Collapsed = true;
                        }


                        //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                        ws.Cells["A1"].LoadFromDataTable(dt, true);
                        //ws.Cells[1, 30].LoadFromDataTable(dt2, true);

                        //Format the header for column 1-3
                        using (ExcelRange range = ws.Cells["A1:O1"])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189)); //Set color to dark blue
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);

                        }
                        //Example how to Format Column 1 as numeric 
                        //using (ExcelRange col = ws.Cells[2, 6, 2 + dt.Rows.Count, 20])
                        //{
                        //    //col.Style.Numberformat.Format = "#,##0.00";
                        //    //col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        //    //col.Style.WrapText = true;
                        //    ws.Column(1).BestFit = true;
                        //    ws.Row(1).Diameter = 300;
                        //    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                        //}
                        var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                        string baseURL = MyConfig.GetValue<string>("AppSettings:baseURL");

                        using (var package = new ExcelPackage())
                        {

                            var CompanyName = header.CompanyName.ToLower();
                            string FullFileName = DateTime.Now.ToFileTime() + "_" + "AccountTree.xlsx";
                            string PathsTR = "Attachments/" + CompanyName + "/";
                            //String path = HttpContext.Current.Server.MapPath(PathsTR);
                            //String path = HttpContext.Current.Server.MapPath(PathsTR);
                            string path = Path.Combine(_host.WebRootPath, PathsTR);
                            string p_strPath = Path.Combine(path, FullFileName);
                            var workSheet = package.Workbook.Worksheets.Add("AccountTree");
                            ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);

                            System.IO.File.Exists(p_strPath);
                            var objFileStrm = System.IO.File.Create(p_strPath);

                            objFileStrm.Close();
                            package.Save();
                            System.IO.File.WriteAllBytes(p_strPath, package.GetAsByteArray());

                            package.Dispose();

                            Response.Message = baseURL + '/' + PathsTR + FullFileName;


                        }


                    }




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

        public async Task<DailyJournalEntryGroupingResponse> GetDailyJournalEntryDateList([FromHeader] GetDailyJournalEntryDateListHeader header, long creator)
        {
            var Response = new DailyJournalEntryGroupingResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DailyJournalEntryListByDate = new List<DailyJournalEntryGroupingByDate>();
                //int NoOfItems = 0;
                if (Response.Result)
                {

                    if (Response.Result)
                    {
                        
                        #region For PagedList
                        //var DailyJournalEntryListDB = _Context.V_DailyJournalEntry.Where(x => x.Active == true).OrderByDescending(a => a.CreationDate).AsQueryable();

                        var DailyJournalEntryListDB = _unitOfWork.DailyJournalEntries.FindAllQueryable(a => a.Active == true, includes: new[] { "CreatedByNavigation" }).OrderByDescending(a => a.CreationDate).AsQueryable();

                        // Check If Have Role To View Accounts Privae or not
                        if (!Common.CheckUserRole(validation.userID, 111,_Context))
                        {
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => x.Approval == true);
                        }


                        bool HasAutoJE = false;
                        if (!string.IsNullOrWhiteSpace(header.HasAutoJE))
                        {
                            if (!string.IsNullOrEmpty(header.HasAutoJE) && bool.TryParse(header.HasAutoJE, out HasAutoJE))
                            {
                                bool.TryParse(header.HasAutoJE, out HasAutoJE);
                                var ListEntriesIDS = (await _unitOfWork.ClientAccounts.FindAllAsync(x => x.OfferId != null)).Select(x => x.DailyAdjustingEntryId).Distinct().ToList();
                                if (HasAutoJE) // Get All Entries have Offers in Client Account Table (Automatic Journal entries)
                                {
                                    DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => ListEntriesIDS.Contains(x.Id));
                                }
                                else // Get All Entries have Offers in Client Account Table (Not Automatic Journal entries)
                                {
                                    DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => !ListEntriesIDS.Contains(x.Id));
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(header.ItemSerial))
                        {
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => x.Serial == header.ItemSerial);
                        }

                        if (!string.IsNullOrWhiteSpace(header.SearchKey))
                        {
                            string SearchKey = header.SearchKey;
                            SearchKey = HttpUtility.UrlDecode(SearchKey);

                            if ("Closed".Contains(SearchKey.ToLower()))
                            {
                                SearchKey = "true";
                            }
                            else if ("open".Contains(SearchKey.ToLower()))
                            {
                                SearchKey = "false";
                            }
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x =>
                                                                                        x.CreatedByNavigation.FirstName.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.CreatedByNavigation.LastName.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.Serial.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.DocumentNumber.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.Description.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.CreationDate.ToString().Contains(SearchKey.ToLower())
                                                                                        || x.EntryDate.ToString().Contains(SearchKey.ToLower())
                                                                                        || x.Closed.ToString().ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.TotalAmount.ToString().ToLower().Contains(SearchKey.ToLower())
                                                                                        );

                        }
                        if (header.AccountID != 0)
                        {
                            var ListDBOfEntriesID = (await _unitOfWork.VAccountOfJournalEntryWithDailies.FindAllAsync(x => x.AccountId == header.AccountID)).Select(x => x.EntryId).Distinct().ToList();
                            //if (ListDBOfEntriesID.Count() > 0)
                            //{
                            //}
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => ListDBOfEntriesID.Contains(x.Id));
                        }
                        if (header.AdvancedTypeID != 0)
                        {
                            var ListOFAcountsIDS = (await _unitOfWork.AdvanciedSettingAccounts.FindAllAsync(x => x.AdvanciedTypeId == header.AdvancedTypeID)).Select(x => x.AccountId).ToList();
                            var ListDBOfEntriesID = (await _unitOfWork.VAccountOfJournalEntryWithDailies.FindAllAsync(x => ListOFAcountsIDS.Contains(x.AccountId))).Select(x => x.EntryId).Distinct().ToList();
                            if (ListDBOfEntriesID.Count() > 0)
                            {
                                DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => ListDBOfEntriesID.Contains(x.Id));
                            }
                        }
                        if (header.branchID != 0)
                        {
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => x.BranchId == header.branchID);
                        }
                        if (header.FromDate != null)
                        {
                            if (header.SortByCreationDate)
                            {
                                DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => x.CreationDate >= header.FromDate).AsQueryable();
                            }
                            else
                            {
                                DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => x.EntryDate >= header.FromDate).AsQueryable();
                            }

                        }
                        if (header.ToDate != null)
                        {
                            if (header.SortByCreationDate)
                            {
                                DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => x.CreationDate <= header.ToDate).AsQueryable();
                            }
                            else
                            {
                                DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => x.EntryDate <= header.ToDate).AsQueryable();
                            }
                        }


                        var IDSReverseAndDeleteEntryList = new List<long>();
                        var IDSAutoEntryList = new List<long>();
                        var IDSEntryList = new List<long>();
                        //if (IsAuto)
                        //{
                        //    IDSAutoEntryList = await _Context.ClientAccounts.Where(x => x.OfferID != null && x.OfferID != 0).Select(x => x.DailyAdjustingEntryID).Distinct().ToListAsync();
                        //    IDSEntryList.AddRange(IDSAutoEntryList);
                        //}

                        if (header.IsDeletedOrReversed)
                        {
                            var JEReverseAndDeleteList = _unitOfWork.DailyJournalEntryReverses.FindAll(x => x.Active == true);
                            if (JEReverseAndDeleteList.Count() > 0)
                            {
                                IDSReverseAndDeleteEntryList = JEReverseAndDeleteList.Select(x => x.ParentDjentryId).ToList();
                                IDSReverseAndDeleteEntryList.AddRange(JEReverseAndDeleteList.Select(x => x.DjentryId).ToList());
                            }
                            IDSEntryList.AddRange(IDSReverseAndDeleteEntryList);
                        }
                        if (header.IsGeneral)
                        {
                            var RemainJEDBList = DailyJournalEntryListDB.Where(x => !IDSReverseAndDeleteEntryList.Contains(x.Id) && !IDSAutoEntryList.Contains(x.Id)).Select(x => x.Id).ToList();

                            IDSEntryList.AddRange(RemainJEDBList);
                        }
                        if (header.IsDeletedOrReversed || header.IsGeneral)
                        {
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => IDSEntryList.Contains(x.Id));

                        }


                        #endregion

                        //var test = DailyJournalEntryListDB.ToList();

                        var DailyJournalEntryListDBGrouping = DailyJournalEntryListDB.Where(x => x.Active == true).OrderByDescending(x => x.CreationDate).GroupBy(x => new { year = x.CreationDate.Year, month = x.CreationDate.Month }).ToList();
                        if (header.SortByCreationDate == false)
                        {
                            DailyJournalEntryListDBGrouping = DailyJournalEntryListDB.Where(x => x.Active == true).OrderByDescending(x => x.EntryDate).GroupBy(x => new { year = x.EntryDate.Year, month = x.EntryDate.Month }).ToList();
                        }


                        foreach (var DailyJournalEntryPerMonth in DailyJournalEntryListDBGrouping)
                        {
                            //var InfoList = new List<DailyJournalEntry>();

                            //foreach (var Data in DailyJournalEntryPerMonth)
                            //{

                            //    InfoList.Add(new DailyJournalEntry
                            //    {
                            //        ID = Data.ID,
                            //        Active = Data.Active,
                            //        Serial = Data.Serial,
                            //        DocumentNumber = Data.DocumentNumber,
                            //        Status = Data.Closed,
                            //        EntryDate = Data.EntryDate,
                            //        CreationUser = Data.CreatorFirstName + " "+ Data.CreatorLastName,
                            //        CreationDate = Data.CreationDate,
                            //        AmountTranaction = Data.TotalAmount,
                            //        Description = Data.Description,
                            //    });
                            //}
                            var ListIDSEntries = DailyJournalEntryPerMonth.Select(x => x.Id).ToList();
                            // var AccountOfJournalEntryList = _Context.V_AccountOfJournalEntryWithDaily.Where(x => ListIDSEntries.Contains(x.EntryID) && x.Active == true).ToList();
                            DateTime DateToGetList = new DateTime(DailyJournalEntryPerMonth.Key.year, DailyJournalEntryPerMonth.Key.month, 1);
                            DailyJournalEntryListByDate.Add(new DailyJournalEntryGroupingByDate()
                            {
                                DateMonth = Common.GetMonthName(DailyJournalEntryPerMonth.Key.month) + " " + DailyJournalEntryPerMonth.Key.year.ToString(),
                                DateToGetList = DateToGetList.ToShortDateString(),
                                CountOfEntry = DailyJournalEntryPerMonth.Count(),
                                TotalCreditSum = _unitOfWork.VAccountOfJournalEntryWithDailies.FindAll(x => ListIDSEntries.Contains(x.EntryId) && x.Active == true).Count() > 0 ?
                                                  (await _unitOfWork.VAccountOfJournalEntryWithDailies.FindAllAsync(x => ListIDSEntries.Contains(x.EntryId) && x.Active == true)).Sum(x => x.Credit) : 0,
                                TotalDebitSum = _unitOfWork.VAccountOfJournalEntryWithDailies.FindAll(x => ListIDSEntries.Contains(x.EntryId) && x.Active == true).Count() > 0 ?
                                                     (await _unitOfWork.VAccountOfJournalEntryWithDailies.FindAllAsync(x => ListIDSEntries.Contains(x.EntryId) && x.Active == true)).Sum(x => x.Debit) : 0

                                //item.Select(x=>x.CreatedDate.Month).ToString(),
                                //DailyJournalEntryInfoList = InfoList,
                            });
                        }

                        Response.DailyJournalEntryList = DailyJournalEntryListByDate;

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

        public async Task<DailyJournalEntryResponse> GetDailyJournalEntryPerDateList([FromHeader] GetDailyJournalEntryPerDateListHader header, long creator)

        {

            DailyJournalEntryResponse Response = new DailyJournalEntryResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DailyJournalEntryList = new List<DailyJournalEntryView>();
                if (Response.Result)
                {
                    DateTime DateToGetList = DateTime.Now;
                    if (string.IsNullOrEmpty(header.DateToGetList) || !DateTime.TryParse(header.DateToGetList, out DateToGetList))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err523";
                        error.ErrorMSG = "DateToGetList required.";
                        Response.Errors.Add(error);
                    }
                    if (Response.Result)
                    {

                        #region For PagedList
                        // var AllInventoryItemPriceAllList = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true).ToList();
                        var DailyJournalEntryListDB = _unitOfWork.DailyJournalEntries.FindAllQueryable(x => x.Active == true && x.CreationDate.Year == DateToGetList.Year && x.CreationDate.Month == DateToGetList.Month, new[] {"CreatedByNavigation", "Branch" }).OrderByDescending(a => a.Id).AsQueryable();
                        // Check If Have Role To View Accounts Privae or not
                        if (!Common.CheckUserRole(creator, 111, _Context))
                        {
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => x.Approval == true);
                        }


                        if (header.SortByCreationDate == false)
                        {
                            DailyJournalEntryListDB = _Context.DailyJournalEntries.Where(x => x.Active == true && x.EntryDate.Year == DateToGetList.Year && x.EntryDate.Month == DateToGetList.Month).OrderByDescending(a => a.EntryDate).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(header.ItemSerial))
                        {
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => x.Serial == header.ItemSerial);
                        }
                        bool HasAutoJE = false;
                        if (header.HasAutoJE != null && bool.TryParse(header.HasAutoJE, out HasAutoJE))
                        {
                            var ListEntriesIDS = _unitOfWork.ClientAccounts.FindAllAsync(x => x.OfferId != null).Result.Select(x => x.DailyAdjustingEntryId).Distinct().ToList();
                            if (HasAutoJE)
                            {
                                DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => ListEntriesIDS.Contains(x.Id));
                            }
                            else
                            {
                                DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => !ListEntriesIDS.Contains(x.Id));
                            }
                        }
                        /*bool HasAutoJE = false;
                        if (headers["HasAutoJE"] != null && headers["HasAutoJE"] != "")
                        {
                            if (!string.IsNullOrEmpty(headers["HasAutoJE"]) && bool.TryParse(headers["HasAutoJE"], out HasAutoJE))
                            {
                                bool.TryParse(headers["HasAutoJE"], out HasAutoJE);
                                var ListEntriesIDS = await _Context.ClientAccounts.Where(x => x.OfferId != null).Select(x => x.DailyAdjustingEntryId).Distinct().ToListAsync();
                                if (HasAutoJE)
                                {
                                    DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => ListEntriesIDS.Contains(x.Id));
                                }
                                else
                                {
                                    DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => !ListEntriesIDS.Contains(x.Id));
                                }
                            }
                        }*/
                        if (!string.IsNullOrEmpty(header.SearchKey))
                        {
                            var SearchKey = HttpUtility.UrlDecode(header.SearchKey);

                            if ("Closed".Contains(SearchKey.ToLower()))
                            {
                                SearchKey = "true";
                            }
                            else if ("open".Contains(SearchKey.ToLower()))
                            {
                                SearchKey = "false";
                            }
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x =>
                                                                                        x.CreatedByNavigation.FirstName.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.CreatedByNavigation.LastName.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.Serial.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.DocumentNumber.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.Description.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.CreationDate.ToString().Contains(SearchKey.ToLower())
                                                                                        || x.EntryDate.ToString().Contains(SearchKey.ToLower())
                                                                                        || x.Closed.ToString().ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.TotalAmount.ToString().ToLower().Contains(SearchKey.ToLower())
                                                                                        );

                        }
                        if (header.AccountID != 0)
                        {
                            var ListDBOfEntriesID = _unitOfWork.VAccountOfJournalEntryWithDailies.FindAllAsync(x => x.AccountId == header.AccountID).Result.Select(x => x.EntryId).Distinct().ToList();
                            //if (ListDBOfEntriesID.Count()  > 0)
                            //{
                            //}
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => ListDBOfEntriesID.Contains(x.Id));
                        }
                        if (header.AdvancedTypeID != 0)
                        {
                            var ListOFAcountsIDS = _unitOfWork.AdvanciedSettingAccounts.FindAllAsync(x => x.AdvanciedTypeId == header.AdvancedTypeID).Result.Select(x => x.AccountId).ToList();
                            var ListDBOfEntriesID = _unitOfWork.VAccountOfJournalEntryWithDailies.FindAllAsync(x => ListOFAcountsIDS.Contains(x.AccountId)).Result.Select(x => x.EntryId).Distinct().ToList();
                            if (ListDBOfEntriesID.Count() > 0)
                            {
                                DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => ListDBOfEntriesID.Contains(x.Id));
                            }
                        }


                        var IDSReverseAndDeleteEntryList = new List<long>();
                        var IDSAutoEntryList = new List<long>();
                        var IDSEntryList = new List<long>();
                        //if (IsAuto)
                        //{
                        //    IDSAutoEntryList = await _Context.ClientAccounts.Where(x => x.OfferID != null && x.OfferID != 0).Select(x => x.DailyAdjustingEntryID).Distinct().ToListAsync();
                        //    IDSEntryList.AddRange(IDSAutoEntryList);
                        //}

                        if (header.IsDeletedOrReversed)
                        {
                            var JEReverseAndDeleteList = _unitOfWork.DailyJournalEntryReverses.FindAll(x => x.Active == true);
                            if (JEReverseAndDeleteList.Count() > 0)
                            {
                                IDSReverseAndDeleteEntryList = JEReverseAndDeleteList.Select(x => x.ParentDjentryId).ToList();
                                IDSReverseAndDeleteEntryList.AddRange(JEReverseAndDeleteList.Select(x => x.DjentryId).ToList());
                            }
                            IDSEntryList.AddRange(IDSReverseAndDeleteEntryList);
                        }
                        if (header.IsGeneral)
                        {
                            var RemainJEDBList = DailyJournalEntryListDB.Where(x => !IDSReverseAndDeleteEntryList.Contains(x.Id) && !IDSAutoEntryList.Contains(x.Id)).Select(x => x.Id).ToList();

                            IDSEntryList.AddRange(RemainJEDBList);
                        }
                        if (/*IsAuto ||*/ header.IsDeletedOrReversed || header.IsGeneral)
                        {
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => IDSEntryList.Contains(x.Id));

                        }
                        var DailyJournalEntryListDBPagingList = PagedList<DailyJournalEntry>.Create(DailyJournalEntryListDB, header.CurrentPage, header.NumberOfItemsPerPage);
                        #endregion

                        if (DailyJournalEntryListDBPagingList.Count > 0)
                        {
                            foreach (var dailyTransaction in DailyJournalEntryListDBPagingList)
                            {
                                var EntryReverseList = _unitOfWork.DailyJournalEntryReverses.FindAllAsync(x => x.ParentDjentryId == dailyTransaction.Id || x.DjentryId == dailyTransaction.Id).Result.ToList();

                                var dailyTranactionObj = new DailyJournalEntryView();
                                dailyTranactionObj.ID = dailyTransaction.Id;
                                dailyTranactionObj.ParentEntryId = EntryReverseList.Where(x => x.DjentryId == dailyTransaction.Id).Select(x => x.ParentDjentryId).FirstOrDefault();
                                if (dailyTranactionObj.ParentEntryId != null && dailyTranactionObj.ParentEntryId != 0)
                                {
                                    dailyTranactionObj.ParentEntrySerial = Common.GetDailyJournalEntrySerial((long)dailyTranactionObj.ParentEntryId, _Context);
                                }
                                dailyTranactionObj.ChildEntryId = EntryReverseList.Where(x => x.ParentDjentryId == dailyTransaction.Id).Select(x => x.DjentryId).FirstOrDefault();
                                if (dailyTranactionObj.ChildEntryId != null && dailyTranactionObj.ChildEntryId != 0)
                                {
                                    dailyTranactionObj.ChildEntrySerial = Common.GetDailyJournalEntrySerial((long)dailyTranactionObj.ChildEntryId, _Context);
                                }
                                dailyTranactionObj.Active = dailyTransaction.Active;
                                dailyTranactionObj.IsPublic = dailyTransaction.Approval;
                                dailyTranactionObj.Serial = dailyTransaction.Serial;
                                dailyTranactionObj.DocumentNumber = dailyTransaction.DocumentNumber;
                                dailyTranactionObj.Status = dailyTransaction.Closed;
                                dailyTranactionObj.CreationDate = dailyTransaction.CreationDate.ToString("yyyy-MM-dd");
                                dailyTranactionObj.EntryDate = dailyTransaction.EntryDate.ToString("yyyy-MM-dd"); 

                                if (Common.GetUserPhoto(dailyTransaction.CreatedBy, _Context) != null)
                                {
                                    dailyTranactionObj.CreationUserImg = Globals.baseURL + Common.GetUserPhoto(dailyTransaction.CreatedBy, _Context);
                                }
                                dailyTranactionObj.CreationUser = Common.GetUserName(dailyTransaction.CreatedBy, _Context);
                                dailyTranactionObj.AmountTranaction = dailyTransaction.TotalAmount;
                                dailyTranactionObj.Description = dailyTransaction.Description;
                                dailyTranactionObj.AmountTranaction = dailyTransaction.TotalAmount;
                                dailyTranactionObj.BranchID = dailyTransaction.BranchId;
                                dailyTranactionObj.BranchName = dailyTransaction.Branch?.Name;

                                DailyJournalEntryList.Add(dailyTranactionObj);
                            }
                        }
                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = header.CurrentPage,
                            TotalPages = DailyJournalEntryListDBPagingList.TotalPages,
                            ItemsPerPage = header.NumberOfItemsPerPage,
                            TotalItems = DailyJournalEntryListDBPagingList.TotalCount
                        };

                        Response.DailyJournalEntryList = DailyJournalEntryList;

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

        public async Task<GetCalcDetailsResponse> GetCalcProjectDetails([FromHeader] long ProjectId = 0)

        {
            GetCalcDetailsResponse Response = new GetCalcDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {

                    var ProjectObjDB = _unitOfWork.Projects.FindAllAsync(x => x.Id == ProjectId && x.Active == true).Result.FirstOrDefault();
                    if (ProjectObjDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P212";
                        error.ErrorMSG = "Invalid Project Id";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    Response.TotalCollected = ProjectObjDB.ClientAccounts.Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                    Response.TotalAmount = ProjectObjDB.TotalCost ?? 0;
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

        public async Task<GetCalcDetailsResponse> GetCalcPODetails([FromHeader] long POId = 0)
        {
            GetCalcDetailsResponse Response = new GetCalcDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    // Check if Project exist 
                    var POObjDB = _unitOfWork.PurchasePos.FindAllAsync(x => x.Id == POId && x.Active == true, includes: new[] { "PurchasePoinvoices" }).Result.FirstOrDefault();
                    if (POObjDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P212";
                        error.ErrorMSG = "Invalid PO Id";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    Response.TotalCollected = POObjDB.SupplierAccounts.Select(x => x.Amount).Sum();
                    Response.TotalAmount = POObjDB.PurchasePoinvoices.Select(x => x.TotalInvoiceCost).Sum() ?? 0;
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

        public async Task<DailyJournalEntryDiviededResponse> GetDailyJournalEntryWithFilterList([FromHeader] GetDailyJournalEntryWithFilterListHeader header)
        {
            DailyJournalEntryDiviededResponse Response = new DailyJournalEntryDiviededResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var AutoJEList = new List<DailyJournalEntryView>();
                var ReverseAndDeleteJEList = new List<DailyJournalEntryView>();
                var GeneralJEList = new List<DailyJournalEntryView>();
                if (Response.Result)
                {
                    if (Response.Result)
                    {

                        #region For PagedList
                        // var AllInventoryItemPriceAllList = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true).ToList();
                        var DailyJournalEntryListDB = _unitOfWork.DailyJournalEntries.FindAll(x => x.Active == true).OrderByDescending(a => a.CreationDate).AsQueryable();
                        var JEReverseAndDeleteList = _unitOfWork.DailyJournalEntryReverses.FindAll(x => x.Active == true);

                        List<long> IDSEntryList = new List<long>();
                        List<long> IDSOffersList = new List<long>();
                        List<long> IDSProjectsList = new List<long>();
                        List<long> IDSClientsList = new List<long>();

                        if (header.InvoiceID != 0)
                        {
                            IDSOffersList = _unitOfWork.Invoices.FindAllAsync(x => x.Id == header.InvoiceID && x.SalesOfferId != null).Result.Select(x => (long)x.SalesOfferId).ToList();
                            IDSProjectsList = _unitOfWork.VProjectSalesOffers.FindAllAsync(x => IDSOffersList.Contains(x.SalesOfferId)).Result.Select(x => x.Id).ToList();
                            IDSClientsList = _unitOfWork.Invoices.FindAllAsync(x => x.Id == header.InvoiceID && x.ClientId != null).Result.Select(x => (long)x.ClientId).ToList();
                            // Must be The Same Project And Same Client on Client Accounts
                            IDSEntryList = _unitOfWork.ClientAccounts.FindAllAsync(x => (x.ProjectId != null ? IDSProjectsList.Contains((long)x.ProjectId) : false) && IDSClientsList.Contains(x.ClientId)).Result.Select(x => x.DailyAdjustingEntryId).ToList();
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => IDSEntryList.Contains(x.Id));
                        }
                        if (header.SalesOfferID != 0)
                        {
                            IDSProjectsList = await _Context.VProjectSalesOffers.Where(x => x.SalesOfferId == header.SalesOfferID).Select(x => x.Id).ToListAsync();
                            IDSEntryList = await _Context.ClientAccounts.Where(x => (x.ProjectId != null ? IDSProjectsList.Contains((long)x.ProjectId) : false)).Select(x => x.DailyAdjustingEntryId).ToListAsync();
                            if (JEReverseAndDeleteList.Count() > 0)
                            {
                                var IDSReverseAndDeleteChildrenEntryList = JEReverseAndDeleteList.Where(x => IDSEntryList.Contains(x.ParentDjentryId)).Select(x => x.DjentryId).ToList();
                                IDSEntryList.AddRange(IDSReverseAndDeleteChildrenEntryList);
                            }
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => IDSEntryList.Contains(x.Id));
                        }
                        if (header.ProjectID != 0)
                        {
                            IDSEntryList = await _Context.ClientAccounts.Where(x => x.ProjectId == header.ProjectID).Select(x => x.DailyAdjustingEntryId).ToListAsync();
                            DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => IDSEntryList.Contains(x.Id));
                        }
                        #endregion


                        var IDSAutoEntryList = await _Context.ClientAccounts.Where(x => x.OfferId != null && x.OfferId != 0).Select(x => x.DailyAdjustingEntryId).Distinct().ToListAsync();
                        var AutomaticSalesJEList = DailyJournalEntryListDB.Where(x => IDSAutoEntryList.Contains(x.Id)).ToList();

                        #region Automatic JE
                        if (AutomaticSalesJEList.Count() > 0)
                        {
                            foreach (var AutoJE in AutomaticSalesJEList)
                            {
                                var EntryReverseList = _Context.DailyJournalEntryReverses.Where(x => x.ParentDjentryId == AutoJE.Id || x.DjentryId == AutoJE.Id).ToList();

                                var dailyTranactionObj = new DailyJournalEntryView();
                                dailyTranactionObj.ID = AutoJE.Id;
                                dailyTranactionObj.ParentEntryId = EntryReverseList.Where(x => x.DjentryId == AutoJE.Id).Select(x => x.ParentDjentryId).FirstOrDefault();
                                if (dailyTranactionObj.ParentEntryId != null && dailyTranactionObj.ParentEntryId != 0)
                                {
                                    dailyTranactionObj.ParentEntrySerial = Common.GetDailyJournalEntrySerial((long)dailyTranactionObj.ParentEntryId, _Context);
                                }
                                dailyTranactionObj.ChildEntryId = EntryReverseList.Where(x => x.ParentDjentryId == AutoJE.Id).Select(x => x.DjentryId).FirstOrDefault();
                                if (dailyTranactionObj.ChildEntryId != null && dailyTranactionObj.ChildEntryId != 0)
                                {
                                    dailyTranactionObj.ChildEntrySerial = Common.GetDailyJournalEntrySerial((long)dailyTranactionObj.ChildEntryId, _Context);
                                }
                                dailyTranactionObj.Active = AutoJE.Active;
                                dailyTranactionObj.IsPublic = AutoJE.Approval;
                                dailyTranactionObj.Serial = AutoJE.Serial;
                                dailyTranactionObj.DocumentNumber = AutoJE.DocumentNumber;
                                dailyTranactionObj.Status = AutoJE.Closed;
                                dailyTranactionObj.CreationDate = AutoJE.CreationDate.ToString("yyyy-MM-dd");
                                dailyTranactionObj.EntryDate = AutoJE.EntryDate.ToString("yyyy-MM-dd");
                                dailyTranactionObj.CreationUser = Common.GetUserName(AutoJE.CreatedBy, _Context);
                                dailyTranactionObj.AmountTranaction = AutoJE.TotalAmount;
                                dailyTranactionObj.Description = AutoJE.Description;
                                dailyTranactionObj.BranchID = AutoJE.BranchId;
                                var AccountEntryList = new List<EntryAccount>();
                                var AccountOfJournalEntryList = _Context.VAccountOfJournalEntryWithDailies.Where(x => x.Active == true && x.EntryId == AutoJE.Id).ToList();

                                if (AccountOfJournalEntryList.Count > 0)
                                {
                                    foreach (var item in AccountOfJournalEntryList)
                                    {
                                        var AccEntryObj = new EntryAccount();
                                        AccEntryObj.AccountID = item.AccountId;
                                        AccEntryObj.AccountName = item.AccountName;
                                        AccEntryObj.Amount = item.Amount;
                                        AccEntryObj.CurrencyID = item.CurrencyId;
                                        AccEntryObj.CurrencyName = item.AccountCurrencyName;
                                        AccEntryObj.CategoryName = item.DtmainType;
                                        AccEntryObj.IncOrExTypeID = item.ExpOrIncTypeId;
                                        AccEntryObj.IncOrExTypeName = item.ExpOrIncTypeName;
                                        AccEntryObj.MethodTypeID = item.MethodId;
                                        AccEntryObj.AccountTypeName = item.AccountTypeName;
                                        AccEntryObj.SignOfAccount = item.SignOfAccount;

                                        if (item.FromOrTo.ToLower().Contains("from"))
                                        {
                                            AccEntryObj.FromAccount = true;
                                        }
                                        else if (item.FromOrTo.ToLower().Contains("to"))
                                        {
                                            AccEntryObj.FromAccount = false;
                                        }

                                        AccountEntryList.Add(AccEntryObj);

                                    }
                                }

                                dailyTranactionObj.AccountEntryList = AccountEntryList;
                                AutoJEList.Add(dailyTranactionObj);
                            }
                        }

                        #endregion



                        var IDSReverseAndDeleteEntryList = new List<long>();
                        if (JEReverseAndDeleteList.Count() > 0)
                        {
                            IDSReverseAndDeleteEntryList = JEReverseAndDeleteList.Select(x => x.ParentDjentryId).ToList();
                            IDSReverseAndDeleteEntryList.AddRange(JEReverseAndDeleteList.Select(x => x.DjentryId).ToList());
                        }
                        var ReverseAndDeleteJEDBList = DailyJournalEntryListDB.Where(x => IDSReverseAndDeleteEntryList.Contains(x.Id)).ToList();
                        #region Reverse and Delete JE
                        if (ReverseAndDeleteJEDBList.Count() > 0)
                        {
                            foreach (var RevJE in ReverseAndDeleteJEDBList)
                            {
                                var EntryReverseList = _Context.DailyJournalEntryReverses.Where(x => x.ParentDjentryId == RevJE.Id || x.DjentryId == RevJE.Id).ToList();

                                var dailyTranactionObj = new DailyJournalEntryView();
                                dailyTranactionObj.ID = RevJE.Id;
                                dailyTranactionObj.ParentEntryId = EntryReverseList.Where(x => x.DjentryId == RevJE.Id).Select(x => x.ParentDjentryId).FirstOrDefault();
                                if (dailyTranactionObj.ParentEntryId != null && dailyTranactionObj.ParentEntryId != 0)
                                {
                                    dailyTranactionObj.ParentEntrySerial = Common.GetDailyJournalEntrySerial((long)dailyTranactionObj.ParentEntryId, _Context);
                                }
                                dailyTranactionObj.ChildEntryId = EntryReverseList.Where(x => x.ParentDjentryId == RevJE.Id).Select(x => x.DjentryId).FirstOrDefault();
                                if (dailyTranactionObj.ChildEntryId != null && dailyTranactionObj.ChildEntryId != 0)
                                {
                                    dailyTranactionObj.ChildEntrySerial = Common.GetDailyJournalEntrySerial((long)dailyTranactionObj.ChildEntryId, _Context);
                                }
                                dailyTranactionObj.Active = RevJE.Active;
                                dailyTranactionObj.IsPublic = RevJE.Approval;
                                dailyTranactionObj.Serial = RevJE.Serial;
                                dailyTranactionObj.DocumentNumber = RevJE.DocumentNumber;
                                dailyTranactionObj.Status = RevJE.Closed;
                                dailyTranactionObj.CreationDate = RevJE.CreationDate.ToString("yyyy-MM-dd");
                                dailyTranactionObj.EntryDate = RevJE.EntryDate.ToString("yyyy-MM-dd");
                                dailyTranactionObj.CreationUser = Common.GetUserName(RevJE.CreatedBy, _Context);
                                dailyTranactionObj.AmountTranaction = RevJE.TotalAmount;
                                dailyTranactionObj.Description = RevJE.Description;
                                dailyTranactionObj.BranchID = RevJE.BranchId;
                                var AccountEntryList = new List<EntryAccount>();
                                var AccountOfJournalEntryList = _Context.VAccountOfJournalEntryWithDailies.Where(x => x.Active == true && x.EntryId == RevJE.Id).ToList();

                                if (AccountOfJournalEntryList.Count > 0)
                                {
                                    foreach (var item in AccountOfJournalEntryList)
                                    {
                                        var AccEntryObj = new EntryAccount();
                                        AccEntryObj.AccountID = item.AccountId;
                                        AccEntryObj.AccountName = item.AccountName;
                                        AccEntryObj.Amount = item.Amount;
                                        AccEntryObj.CurrencyID = item.CurrencyId;
                                        AccEntryObj.CurrencyName = item.AccountCurrencyName;
                                        AccEntryObj.CategoryName = item.DtmainType;
                                        AccEntryObj.IncOrExTypeID = item.ExpOrIncTypeId;
                                        AccEntryObj.IncOrExTypeName = item.ExpOrIncTypeName;
                                        AccEntryObj.MethodTypeID = item.MethodId;
                                        AccEntryObj.AccountTypeName = item.AccountTypeName;
                                        AccEntryObj.SignOfAccount = item.SignOfAccount;

                                        if (item.FromOrTo.ToLower().Contains("from"))
                                        {
                                            AccEntryObj.FromAccount = true;
                                        }
                                        else if (item.FromOrTo.ToLower().Contains("to"))
                                        {
                                            AccEntryObj.FromAccount = false;
                                        }

                                        AccountEntryList.Add(AccEntryObj);

                                    }
                                }

                                dailyTranactionObj.AccountEntryList = AccountEntryList;
                                ReverseAndDeleteJEList.Add(dailyTranactionObj);
                            }
                            // NoOfItems += InventoryStoreItemPagingList.Select(x => x.InventoryItemID).Distinct().Count();
                        }

                        #endregion




                        var RemainJEDBList = DailyJournalEntryListDB.Where(x => !IDSReverseAndDeleteEntryList.Contains(x.Id) && !IDSAutoEntryList.Contains(x.Id)).ToList();
                        #region Remain JE
                        if (RemainJEDBList.Count() > 0)
                        {
                            foreach (var RemainJE in RemainJEDBList)
                            {
                                var EntryReverseList = _Context.DailyJournalEntryReverses.Where(x => x.ParentDjentryId == RemainJE.Id || x.DjentryId == RemainJE.Id).ToList();

                                var dailyTranactionObj = new DailyJournalEntryView();
                                dailyTranactionObj.ID = RemainJE.Id;
                                dailyTranactionObj.ParentEntryId = EntryReverseList.Where(x => x.DjentryId == RemainJE.Id).Select(x => x.ParentDjentryId).FirstOrDefault();
                                if (dailyTranactionObj.ParentEntryId != null && dailyTranactionObj.ParentEntryId != 0)
                                {
                                    dailyTranactionObj.ParentEntrySerial = Common.GetDailyJournalEntrySerial((long)dailyTranactionObj.ParentEntryId, _Context);
                                }
                                dailyTranactionObj.ChildEntryId = EntryReverseList.Where(x => x.ParentDjentryId == RemainJE.Id).Select(x => x.DjentryId).FirstOrDefault();
                                if (dailyTranactionObj.ChildEntryId != null && dailyTranactionObj.ChildEntryId != 0)
                                {
                                    dailyTranactionObj.ChildEntrySerial = Common.GetDailyJournalEntrySerial((long)dailyTranactionObj.ChildEntryId, _Context);
                                }
                                dailyTranactionObj.Active = RemainJE.Active;
                                dailyTranactionObj.IsPublic = RemainJE.Approval;
                                dailyTranactionObj.Serial = RemainJE.Serial;
                                dailyTranactionObj.DocumentNumber = RemainJE.DocumentNumber;
                                dailyTranactionObj.Status = RemainJE.Closed;
                                dailyTranactionObj.CreationDate = RemainJE.CreationDate.ToString("yyyy-MM-dd");
                                dailyTranactionObj.EntryDate = RemainJE.EntryDate.ToString("yyyy-MM-dd");
                                dailyTranactionObj.CreationUser = Common.GetUserName(RemainJE.CreatedBy, _Context);
                                dailyTranactionObj.AmountTranaction = RemainJE.TotalAmount;
                                dailyTranactionObj.Description = RemainJE.Description;
                                dailyTranactionObj.BranchID = RemainJE.BranchId;





                                var AccountEntryList = new List<EntryAccount>();
                                var AccountOfJournalEntryList = _Context.VAccountOfJournalEntryWithDailies.Where(x => x.Active == true && x.EntryId == RemainJE.Id).ToList();

                                if (AccountOfJournalEntryList.Count > 0)
                                {
                                    foreach (var item in AccountOfJournalEntryList)
                                    {
                                        var AccEntryObj = new EntryAccount();
                                        AccEntryObj.AccountID = item.AccountId;
                                        AccEntryObj.AccountName = item.AccountName;
                                        AccEntryObj.Amount = item.Amount;
                                        AccEntryObj.CurrencyID = item.CurrencyId;
                                        AccEntryObj.CurrencyName = item.AccountCurrencyName;
                                        AccEntryObj.CategoryName = item.DtmainType;
                                        AccEntryObj.IncOrExTypeID = item.ExpOrIncTypeId;
                                        AccEntryObj.IncOrExTypeName = item.ExpOrIncTypeName;
                                        AccEntryObj.MethodTypeID = item.MethodId;
                                        AccEntryObj.AccountTypeName = item.AccountTypeName;
                                        AccEntryObj.SignOfAccount = item.SignOfAccount;

                                        if (item.FromOrTo.ToLower().Contains("from"))
                                        {
                                            AccEntryObj.FromAccount = true;
                                        }
                                        else if (item.FromOrTo.ToLower().Contains("to"))
                                        {
                                            AccEntryObj.FromAccount = false;
                                        }

                                        AccountEntryList.Add(AccEntryObj);

                                    }
                                }

                                dailyTranactionObj.AccountEntryList = AccountEntryList;
                                GeneralJEList.Add(dailyTranactionObj);
                            }
                            // NoOfItems += InventoryStoreItemPagingList.Select(x => x.InventoryItemID).Distinct().Count();
                        }

                        #endregion

                        //Response.PaginationHeader = new PaginationHeader
                        //{
                        //    CurrentPage = CurrentPage,
                        //    TotalPages = DailyJournalEntryListDBPagingList.TotalPages,
                        //    ItemsPerPage = NumberOfItemsPerPage,
                        //    TotalItems = DailyJournalEntryListDBPagingList.TotalCount
                        //};

                        Response.OtherJEList = GeneralJEList;
                        Response.DeleteAndReverseJEList = ReverseAndDeleteJEList;
                        Response.AutoSalesJEList = AutoJEList;
                        //Response.DailyJournalEntryList = AutoJEList; // Removed

                    }
                    Response.Count = GeneralJEList.Count() + ReverseAndDeleteJEList.Count() + AutoJEList.Count();
                    //var aa = AutoJEList.Sum(x => x.AmountTranaction) ?? 0;
                    //var b = GeneralJEList.Sum(x => x.AmountTranaction) ?? 0;
                    var DeletedSum = (ReverseAndDeleteJEList.Where(x => x.Description != null ? x.Description.Contains("(Reversing Entry) for Deleting") : false).Sum(x => x.AmountTranaction) ?? 0);
                    var ReveseSum = ((ReverseAndDeleteJEList.Where(x => x.Description != null ? x.Description.Contains("(Reversing Entry) for Reverse") : false).Sum(x => x.AmountTranaction) ?? 0));
                    Response.TotalAmount = (AutoJEList.Sum(x => x.AmountTranaction) ?? 0) + (GeneralJEList.Sum(x => x.AmountTranaction) ?? 0) -
                                           (Math.Abs(DeletedSum)) - (Math.Abs(ReveseSum));

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

        public void UpdateParentAccountAccumilative(long ParentAccountID, decimal Credit, decimal Debit, decimal Balance)
        {
            var ParentAccountDB = _unitOfWork.Accounts.FindAll(x => x.Id == ParentAccountID).FirstOrDefault();
            if (ParentAccountDB != null)
            {
                ParentAccountDB.Credit += Credit;
                ParentAccountDB.Debit += Debit;
                ParentAccountDB.Accumulative += Balance;

                _unitOfWork.Complete();

                UpdateParentAccountAccumilative(ParentAccountDB.ParentCategory ?? 0, Credit, Debit, Balance);
            }
        }

        public async Task<BaseResponseWithID> AddNewDailyJournalEntry(AddNewDailyJournalEntryRequest request, long creator)

        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.Data == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    DateTime CreationDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(request.Data.CreationDateSTR) && DateTime.TryParse(request.Data.CreationDateSTR, out CreationDate))
                    {
                        DateTime.TryParse(request.Data.CreationDateSTR, out CreationDate);
                        CreationDate = CreationDate.Add(DateTime.Now.TimeOfDay);
                    }
                    bool? ParentEntryIsPublic = null;
                    long ParentEntryId = 0;
                    DailyJournalEntry DailyJournalEntryDB = null;
                    if (request.Data.ParentEntryId != null)
                    {
                        ParentEntryId = (long)request.Data.ParentEntryId;
                        if (!Common.CheckUserRole(creator, 104, _Context))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P13";
                            error.ErrorMSG = "Not have Role to  Edit Journal Entry";
                            Response.Errors.Add(error);
                            return Response;
                        }

                        // Check if this Entry is exist
                        DailyJournalEntryDB = _unitOfWork.DailyJournalEntries.FindAllAsync(x => x.Id == ParentEntryId).Result.FirstOrDefault();
                        if (DailyJournalEntryDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P13";
                            error.ErrorMSG = "The Parent Entry selected is not exist";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        ParentEntryIsPublic = DailyJournalEntryDB.Approval;
                    }
                   int? BranchID = request.Data.BranchID;
                    if (BranchID != null)
                    {
                        var branch = _unitOfWork.Branches.FindAllAsync(a => a.Id == BranchID).Result.FirstOrDefault();
                        if (branch == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err501";
                            error.ErrorMSG = "No Branch with this ID";
                            Response.Errors.Add(error);
                        }

                    }
                    if (string.IsNullOrEmpty(request.Data.DocumentNumber))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err501";
                        error.ErrorMSG = "Document Number is required.";
                        Response.Errors.Add(error);
                    }

                    if (string.IsNullOrEmpty(request.Data.Description))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err502";
                        error.ErrorMSG = "Description is required.";
                        Response.Errors.Add(error);
                    }
                    bool IsPublic = true; // Default Public Entry
                    if (request.Data.IsPublic != null)
                    {
                        IsPublic = (bool)request.Data.IsPublic;

                        // check Role Is Public or Private
                        if (request.Data.IsPublic == false && !Common.CheckUserRole(creator, 111, _Context))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P133";
                            error.ErrorMSG = "Not have Role to select Private Entry";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    DateTime EntryDate = DateTime.Now;
                    if (string.IsNullOrEmpty(request.Data.EntryDateSTR) || !DateTime.TryParse(request.Data.EntryDateSTR, out EntryDate))
                    {
                        if (EntryDate.Date < DateTime.Now.Date)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err503";
                            error.ErrorMSG = "Entry Date Must be greater than today.";
                            Response.Errors.Add(error);
                        }
                    }
                    if (!await Common.CheckFinancialPeriodIsAvaliable(EntryDate, _Context))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P133";
                        error.ErrorMSG = "There is no financial period during Entry Date ";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (!await Common.CheckFinancialPeriodIsAvaliable(CreationDate, _Context))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P133";
                        error.ErrorMSG = "There is no financial period during Creation Date ";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    // Validate from Finanical Period
                    // Check is Current and check must be between duration




                    if (Response.Result)
                    {
                        if (request.Data.EntryAccountList == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err504";
                            error.ErrorMSG = "please insert at least one Account.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (request.Data.EntryAccountList.Count < 2)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err505";
                            error.ErrorMSG = "please insert at least one Account From and one Account To.";
                            Response.Errors.Add(error);
                            return Response;
                        }


                        var DefaultCurrencyId = _unitOfWork.Currencies.FindAllAsync(x => x.IsLocal == true).Result.Select(x => x.Id).FirstOrDefault();
                        if (DefaultCurrencyId == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err506";
                            error.ErrorMSG = "you dom't have default currency ,you must be select local Currency first ";
                            Response.Errors.Add(error);
                            return Response;
                        }


                        int Counter = 0;
                        var IDSAccount = request.Data.EntryAccountList.Select(x => x.AccountID).ToList();
                        var AddvanciedTypeListDB = _unitOfWork.AdvanciedSettingAccounts.FindAllAsync(x => IDSAccount.Contains(x.AccountId)).Result.ToList();
                        foreach (var item in request.Data.EntryAccountList)
                        {
                            Counter++;
                            if (item.AccountID > 0)
                            {

                                var CheckAccount = _unitOfWork.Accounts.FindAllAsync(x => x.Id == item.AccountID).Result.FirstOrDefault();
                                if (CheckAccount == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err506";
                                    error.ErrorMSG = "Invalid AccountID Selected item #" + Counter;
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    if (CheckAccount.Haveitem == true)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err506";
                                        error.ErrorMSG = "Invalid AccountID Selected because have children item #" + Counter;
                                        Response.Errors.Add(error);
                                    }
                                    //if (DefaultCurrencyId != CheckAccount.CurrencyID)
                                    //{
                                    //    if (item.RateToAnotherCU == null || item.RateToAnotherCU == 0)
                                    //    {
                                    //        Response.Result = false;
                                    //        Error error = new Error();
                                    //        error.ErrorCode = "Err506";
                                    //        error.ErrorMSG = "RateToAnotherCU is required because account currency is different on item #" + Counter;
                                    //        Response.Errors.Add(error);
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    item.RateToAnotherCU = 1;
                                    //}
                                    item.AccountTypeName = CheckAccount.AccountTypeName;
                                    if (item.Amount < 0)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err506";
                                        error.ErrorMSG = "Invalid Amount Selected item #" + Counter;
                                        Response.Errors.Add(error);
                                    }
                                    //if (item.FromAccount == null)
                                    //{
                                    //    Response.Result = false;
                                    //    Error error = new Error();
                                    //    error.ErrorCode = "Err507";
                                    //    error.ErrorMSG = "Invalid From Account Selected item #" + Counter;
                                    //    Response.Errors.Add(error);
                                    //}

                                    if (string.IsNullOrEmpty(item.SignOfAccount))
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err508";
                                        error.ErrorMSG = "Invalid SignOfAccount Selected item #" + Counter;
                                        Response.Errors.Add(error);
                                    }
                                    else if (item.SignOfAccount.ToLower() != "plus" && item.SignOfAccount.ToLower() != "minus")
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err508";
                                        error.ErrorMSG = "Invalid SignOfAccount Selected item #" + Counter;
                                        Response.Errors.Add(error);
                                    }

                                    // Check if send Project .. Validate is releated or with client or not
                                    if (item.ClinetID != null)
                                    {
                                        if (item.ProjectID != null)
                                        {
                                            var ProjectDB = _unitOfWork.VProjectSalesOffers.FindAll(x => x.Id == item.ProjectID).FirstOrDefault();
                                            if (ProjectDB == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err517";
                                                error.ErrorMSG = "Invalid Project ID Selected item #" + Counter;
                                                Response.Errors.Add(error);
                                            }
                                            else
                                            {
                                                if (ProjectDB.ClientId != item.ClinetID)
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err517";
                                                    error.ErrorMSG = "This Project Not releated to this Client item #" + Counter;
                                                    Response.Errors.Add(error);
                                                }

                                                if (item.OfferID != null)
                                                {
                                                    if (ProjectDB.SalesOfferId != item.OfferID)
                                                    {
                                                        Response.Result = false;
                                                        Error error = new Error();
                                                        error.ErrorCode = "Err517";
                                                        error.ErrorMSG = "This Project Not releated to this Offer item #" + Counter;
                                                        Response.Errors.Add(error);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (CheckAccount.AccountCategoryId == 4 || CheckAccount.AccountCategoryId == 5)
                                    {
                                        //if (item.IncOrExTypeID == null || item.IncOrExTypeID == 0)
                                        //{
                                        //    Response.Result = false;
                                        //    Error error = new Error();
                                        //    error.ErrorCode = "Err508";
                                        //    error.ErrorMSG = "Invalid IncOrEx Type Selected item #" + Counter;
                                        //    Response.Errors.Add(error);
                                        //}
                                        //if (string.IsNullOrEmpty(item.IncOrExTypeName))
                                        //{
                                        //    Response.Result = false;
                                        //    Error error = new Error();
                                        //    error.ErrorCode = "Err508";
                                        //    error.ErrorMSG = "Invalid IncOrEx Type Name Selected item #" + Counter;
                                        //    Response.Errors.Add(error);
                                        //}
                                    }


                                    // Validation Must send Client and Project if Advancied Type =Clients 
                                    // Must Send Suppliet and PO if Advanced Type Supplier
                                    var CheckAdvanciedTypeSetting = AddvanciedTypeListDB.Where(x => x.AccountId == item.AccountID).FirstOrDefault();
                                    if (CheckAdvanciedTypeSetting != null)
                                    {
                                        if (CheckAdvanciedTypeSetting.AdvanciedTypeId == 30)
                                        {
                                            //// Validate Must set Client ID and Project ID
                                            //if (item.ProjectID == null || item.ProjectID == 0)
                                            //{
                                            //    Response.Result = false;
                                            //    Error error = new Error();
                                            //    error.ErrorCode = "Err508";
                                            //    error.ErrorMSG = "Project is Required for this Account Selected item #" + Counter;
                                            //    Response.Errors.Add(error);
                                            //}
                                            if (item.ClinetID == null || item.ClinetID == 0)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err508";
                                                error.ErrorMSG = "Client is Required for this Account Selected item #" + Counter;
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else if (CheckAdvanciedTypeSetting.AdvanciedTypeId == 31)
                                        {
                                            // Validate Must Set Suppliet ID
                                            // Validate Must set Client ID and Project ID
                                            if (item.SupplierID == null || item.SupplierID == 0)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err508";
                                                error.ErrorMSG = "Supplier is Required for this Account Selected item #" + Counter;
                                                Response.Errors.Add(error);
                                            }

                                        }
                                    }
                                }


                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err506";
                                error.ErrorMSG = "Invalid AccountID Selected item #" + Counter;
                                Response.Errors.Add(error);
                            }
                        }
                        if (Response.Result)
                        {
                            //if (ParentEntryId != 0)
                            //{
                            //    var EntryObjDB =  _Context.V_DailyJournalEntry.Where(x => x.ID == ParentEntryId).FirstOrDefault();
                            //    if (EntryObjDB == null)
                            //    {
                            //        Response.Result = false;
                            //        Error error = new Error();
                            //        error.ErrorCode = "Err505";
                            //        error.ErrorMSG = "Invalid Parent Entry Id ";
                            //        Response.Errors.Add(error);
                            //        return Response;
                            //    }

                            //}





                            /*
                                                         decimal TotalAmountCreditAcc = Request.Data.EntryAccountList.Where(x => (x.AccountTypeName.ToLower().Trim() == "credit" && x.SignOfAccount.ToLower().Trim() == "plus")
                                                                                                || (x.AccountTypeName.ToLower().Trim() == "debit" && x.SignOfAccount.ToLower().Trim() == "minus"))
                                .Select(x => x.Amount * (x.RateToAnotherCU ?? 1 )).Sum();
                            decimal TotalAmountDebitAcc = Request.Data.EntryAccountList.Where(x => (x.AccountTypeName.ToLower().Trim() == "debit" && x.SignOfAccount.ToLower().Trim() == "plus")
                                                                                                || (x.AccountTypeName.ToLower().Trim() == "credit" && x.SignOfAccount.ToLower().Trim() == "minus"))
                                .Select(x => x.Amount * (x.RateToAnotherCU ?? 1)).Sum();
                             */
                            // Check Accounts from = Accounts To
                            decimal TotalAmountCreditAcc = request.Data.EntryAccountList.Where(x => (x.AccountTypeName.ToLower().Trim() == "credit" && x.SignOfAccount.ToLower().Trim() == "plus")
                                                                                                || (x.AccountTypeName.ToLower().Trim() == "debit" && x.SignOfAccount.ToLower().Trim() == "minus")).Select(x => x.Amount).Sum();
                            decimal TotalAmountDebitAcc = request.Data.EntryAccountList.Where(x => (x.AccountTypeName.ToLower().Trim() == "debit" && x.SignOfAccount.ToLower().Trim() == "plus")
                                                                                                || (x.AccountTypeName.ToLower().Trim() == "credit" && x.SignOfAccount.ToLower().Trim() == "minus")).Select(x => x.Amount).Sum();
                            //decimal TotalAmountToAcc = Request.Data.EntryAccountList.Where(x => x.FromAccount == false).Select(x => x.Amount).Sum();
                            if (TotalAmountCreditAcc != TotalAmountDebitAcc)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err509";
                                error.ErrorMSG = "Total Amount Account Credit must be equal Total Amount Debit";
                                Response.Errors.Add(error);
                            }
                            // Validate Cost Center 
                            if (request.Data.CostCenterStatus == true)
                            {
                                if (request.Data.CostCenter.CostCenterID == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err501";
                                    error.ErrorMSG = "CostCenterID is required.";
                                    Response.Errors.Add(error);
                                }
                            }
                            if (Response.Result)
                            {

                                long DailyJournalEntryID = 0;
                                long? ExpensesTypeID = 0;
                                string ExpensesTypeName = "";
                                string Serial = "1/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString();
                                if (request.Data.Old == true)
                                {
                                    Serial = "0/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString();
                                    CreationDate = EntryDate.AddHours(CreationDate.Hour);
                                }
                                else
                                {
                                    string OldSerial = "0/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString();
                                    var lastDailyJournalSerial = _Context.DailyJournalEntries.Where(a => a.Active == true && a.Serial != OldSerial).OrderByDescending(a => a.Id).Select(a => a.Serial).FirstOrDefault();
                                    if (lastDailyJournalSerial != null && lastDailyJournalSerial.Contains(System.DateTime.Now.Year.ToString()))
                                    {
                                        if (lastDailyJournalSerial.Split('/').Length > 0)
                                        {
                                            string strLastDailyNumber = lastDailyJournalSerial.Split('/')[0];
                                            long newNumber = long.Parse(strLastDailyNumber) + 1;
                                            Serial = newNumber + "/" + DateTime.Now.Month.ToString() + "/" + System.DateTime.Now.Year.ToString();
                                        }
                                    }
                                }


                                //long JournalEntryCount = 1;
                                //string Serial = JournalEntryCount + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString();

                                // Daily Journal Entry Insertion
                                var DailyJournal = new DailyJournalEntry()
                                {
                                    Description = request.Data.Description,
                                    Serial = Serial,
                                    Closed = false,
                                    TotalAmount = TotalAmountCreditAcc,
                                    DocumentNumber = request.Data.DocumentNumber,
                                    Approval = IsPublic,
                                    Active = true,
                                    CreatedBy = creator,
                                    CreationDate = CreationDate,
                                    ModifiedBy = creator,
                                    ModifiedDate = DateTime.Now,
                                    EntryDate = EntryDate,
                                    BranchId = BranchID
                                };
                                _unitOfWork.DailyJournalEntries.Add(DailyJournal);
                                var DailyJournalEntry = _unitOfWork.Complete();
                                /*ObjectParameter IDDailyJournalEntry = new ObjectParameter("ID", typeof(long));
                                var DailyJournalEntry = _Context.proc_DailyJournalEntryInsert(IDDailyJournalEntry,
                                                                       Request.Data.Description,
                                                                       Serial,
                                                                       false,
                                                                       TotalAmountCreditAcc,
                                                                       Request.Data.DocumentNumber,
                                                                       IsPublic,
                                                                       true,
                                                                       validation.userID,
                                                                       CreationDate,
                                                                       validation.userID,
                                                                       DateTime.Now,
                                                                       EntryDate);*/
                                if (DailyJournalEntry > 0)
                                {
                                    DailyJournalEntryID = (long)DailyJournal.Id;

                                    if (ParentEntryId != 0) // Edit 
                                    {
                                        // Update Parent DailyJournal Entry Reverse or delete
                                        var EntryReverseObjDB = new DailyJournalEntryReverse();
                                        EntryReverseObjDB.ParentDjentryId = ParentEntryId;
                                        EntryReverseObjDB.DjentryId = DailyJournalEntryID;
                                        EntryReverseObjDB.Active = true;
                                        EntryReverseObjDB.CreationDate = DateTime.Now;
                                        EntryReverseObjDB.ModificationDate = DateTime.Now;
                                        EntryReverseObjDB.CreatedBy = creator;
                                        EntryReverseObjDB.ModifiedBy = creator;
                                        var EntryReverseObj = _unitOfWork.DailyJournalEntryReverses.Add(EntryReverseObjDB);
                                        _unitOfWork.Complete();


                                        // Check If Edit Public or private about parent .. update Parent
                                        if (request.Data.IsPublic != ParentEntryIsPublic)
                                        {
                                            if (DailyJournalEntryDB != null)
                                            {
                                                DailyJournalEntryDB.Approval = IsPublic;
                                                _unitOfWork.Complete();
                                            }
                                        }
                                    }

                                    foreach (var item in request.Data.EntryAccountList)
                                    {
                                        var AccountObjDB = _unitOfWork.Accounts.FindAll(x => x.Id == item.AccountID).FirstOrDefault();

                                        if (AccountObjDB != null)
                                        {
                                            string CategoryName = AccountObjDB.AccountCategory?.AccountCategoryName;
                                            string FromOrTo = "";
                                            decimal Credit = 0;
                                            decimal Debit = 0;
                                            decimal Amount = 0;

                                            decimal CreditWithAnotherCU = 0;
                                            decimal DebitWithAnotherCU = 0;
                                            decimal AmountWithAnotherCU = 0;

                                            decimal AmountForAccumulative = 0;
                                            decimal AccBalance = AccountObjDB.Accumulative;
                                            if (item.SignOfAccount == "minus")
                                            {
                                                //AmountForAccumulative = item.Amount;
                                                FromOrTo = "From";
                                            }
                                            else if (item.SignOfAccount == "plus")
                                            {
                                                // AmountForAccumulative = -1 * item.Amount;
                                                FromOrTo = "To";
                                            }

                                            if (AccountObjDB.AccountTypeName == "Credit")
                                            {
                                                if (item.SignOfAccount == "plus")
                                                {
                                                    Credit = Math.Abs(item.Amount);
                                                    Debit = 0;
                                                    Amount = Math.Abs(item.Amount);

                                                    // For another Currency
                                                    if (item.AmountAnotherCU != null)
                                                    {
                                                        CreditWithAnotherCU = Math.Abs((decimal)item.AmountAnotherCU);
                                                        DebitWithAnotherCU = 0;
                                                        AmountWithAnotherCU = Math.Abs((decimal)item.AmountAnotherCU);
                                                    }

                                                }
                                                else if (item.SignOfAccount == "minus")
                                                {
                                                    Credit = 0;
                                                    Debit = Math.Abs(item.Amount);
                                                    Amount = -1 * Math.Abs(item.Amount);

                                                    // For another Currency
                                                    if (item.AmountAnotherCU != null)
                                                    {
                                                        CreditWithAnotherCU = 0;
                                                        DebitWithAnotherCU = Math.Abs((decimal)item.AmountAnotherCU);
                                                        AmountWithAnotherCU = -1 * Math.Abs((decimal)item.AmountAnotherCU);
                                                    }
                                                }
                                            }
                                            else if (AccountObjDB.AccountTypeName == "Debit")
                                            {
                                                if (item.SignOfAccount == "plus")
                                                {
                                                    Credit = 0;
                                                    Debit = Math.Abs(item.Amount);
                                                    Amount = Math.Abs(item.Amount);

                                                    // For another Currency
                                                    if (item.AmountAnotherCU != null)
                                                    {
                                                        CreditWithAnotherCU = 0;
                                                        DebitWithAnotherCU = Math.Abs((decimal)item.AmountAnotherCU);
                                                        AmountWithAnotherCU = Math.Abs((decimal)item.AmountAnotherCU);
                                                    }
                                                }
                                                else if (item.SignOfAccount == "minus")
                                                {
                                                    Credit = Math.Abs(item.Amount);
                                                    Debit = 0;
                                                    Amount = -1 * Math.Abs(item.Amount);



                                                    // For another Currency
                                                    if (item.AmountAnotherCU != null)
                                                    {
                                                        CreditWithAnotherCU = Math.Abs((decimal)item.AmountAnotherCU);
                                                        DebitWithAnotherCU = 0;
                                                        AmountWithAnotherCU = -1 * Math.Abs((decimal)item.AmountAnotherCU);
                                                    }
                                                }
                                            }
                                            AmountForAccumulative = Amount;
                                            AccBalance = AccBalance + AmountForAccumulative;








                                            //Account Of Journal Entry Insertion
                                            var AccOfDailyJournal = new AccountOfJournalEntry()
                                            {
                                                EntryId = DailyJournalEntryID,
                                                AccountId = item.AccountID,
                                                FromOrTo = FromOrTo,
                                                Credit = Credit,
                                                Debit = Debit,
                                                Amount = Amount,
                                                SignOfAccount = item.SignOfAccount,
                                                CurrencyId = (int)AccountObjDB.CurrencyId,
                                                MethodId = item.MethodTypeID,
                                                DtmainType = CategoryName,
                                                ExpOrIncTypeId = item.IncOrExTypeID,
                                                ExpOrIncTypeName = item.IncOrExTypeName,
                                                ExtraIdofType = 0,
                                                Active = true,
                                                AccBalance = AccBalance,
                                                EntryDate = EntryDate,
                                                CreatedBy = creator,
                                                CreationDate = CreationDate,
                                                ModifiedBy = creator,
                                                ModifiedDate = DateTime.Now
                                            };
                                            _unitOfWork.AccountOfJournalEntries.Add(AccOfDailyJournal);
                                            var ResAccountOfJE = _unitOfWork.Complete();
                                            /*ObjectParameter IDAccountOjJournal = new ObjectParameter("ID", typeof(long));
                                            var ResAccountOfJE = _Context.proc_AccountOfJournalEntryInsert(IDAccountOjJournal,
                                                                                        DailyJournalEntryID,
                                                                                        item.AccountID,
                                                                                        FromOrTo,
                                                                                        Credit,
                                                                                        Debit,
                                                                                        Amount,
                                                                                        item.SignOfAccount,
                                                                                        AccountObjDB.CurrencyID,
                                                                                        item.MethodTypeID,
                                                                                        CategoryName, // Category Name = DTMainType
                                                                                        item.IncOrExTypeID,
                                                                                        item.IncOrExTypeName,
                                                                                        0,
                                                                                        true,
                                                                                        validation.userID,
                                                                                        CreationDate,
                                                                                        validation.userID,
                                                                                        DateTime.Now,
                                                                                        AccBalance,
                                                                                        EntryDate
                                                                                        );*/


                                            // Update Accounts
                                            AccountObjDB.Credit = AccountObjDB.Credit + Credit;
                                            AccountObjDB.Debit = AccountObjDB.Debit + Debit;
                                            AccountObjDB.Accumulative = AccBalance;
                                            AccountObjDB.TranactionStatus = true;
                                            //AccountObjDB.ModifiedBy = validation.userID;
                                            //AccountObjDB.ModifiedDate = DateTime.Now;


                                            long AccountOfJEID = 0;
                                            if (ResAccountOfJE > 0)
                                            {
                                                AccountOfJEID = (long)AccOfDailyJournal.Id;



                                                // Check if Account with another Currency

                                                if (item.RateToAnotherCU != null && item.RateToAnotherCU != 0 && item.AmountAnotherCU != null && item.AmountAnotherCU != 0 && AccountObjDB.CurrencyId != null)
                                                {
                                                    var ACCLastAccountOfJournalForAnotherCU = _Context.AccountOfJournalEntryOtherCurrencies.Where(x => x.AccountId == item.AccountID).OrderByDescending(x => x.CreationDate).Select(x => x.Accumulative).FirstOrDefault();

                                                    ACCLastAccountOfJournalForAnotherCU = ACCLastAccountOfJournalForAnotherCU + AmountWithAnotherCU;
                                                    var AccountOfJEWithAnotherCurrency = new AccountOfJournalEntryOtherCurrency();
                                                    AccountOfJEWithAnotherCurrency.AccountId = item.AccountID;
                                                    AccountOfJEWithAnotherCurrency.EntryId = DailyJournalEntryID;
                                                    AccountOfJEWithAnotherCurrency.CurrencyId = AccountObjDB.CurrencyId ?? 0;
                                                    AccountOfJEWithAnotherCurrency.Credit = CreditWithAnotherCU;
                                                    AccountOfJEWithAnotherCurrency.Debit = DebitWithAnotherCU;
                                                    AccountOfJEWithAnotherCurrency.Amount = AmountWithAnotherCU;
                                                    AccountOfJEWithAnotherCurrency.Accumulative = ACCLastAccountOfJournalForAnotherCU;
                                                    AccountOfJEWithAnotherCurrency.SignOfAccount = item.SignOfAccount;
                                                    AccountOfJEWithAnotherCurrency.RateToLocalCu = (decimal)item.RateToAnotherCU;
                                                    AccountOfJEWithAnotherCurrency.Active = true;
                                                    AccountOfJEWithAnotherCurrency.CreatedBy = creator;
                                                    AccountOfJEWithAnotherCurrency.ModifiedBy = creator;
                                                    AccountOfJEWithAnotherCurrency.CreationDate = DateTime.Now;
                                                    AccountOfJEWithAnotherCurrency.ModifiedDate = DateTime.Now;

                                                    _Context.AccountOfJournalEntryOtherCurrencies.Add(AccountOfJEWithAnotherCurrency);
                                                }

                                                // Update All Parent Accounts

                                                UpdateParentAccountAccumilative(AccountObjDB.ParentCategory ?? 0, Credit, Debit, AmountForAccumulative);



                                                // add Client Account row
                                                if (/*(CategoryName == "Income" && item.IncOrExTypeID == 8) &&*/ item.ClinetID != 0 && item.ProjectID != 0 && item.ClinetID != null /*&& item.ProjectID != null*/)
                                                {
                                                    //ObjectParameter IDClientAccount = new ObjectParameter("ID", typeof(long));
                                                    //_Context.proc_ClientAccountsInsert(IDClientAccount,
                                                    //                                   DailyJournalEntryID,
                                                    //                                   item.ClinetID,
                                                    //                                   item.ProjectID,
                                                    //                                   item.AccountID,
                                                    //                                   AccountObjDB.AccountTypeName,
                                                    //                                   item.SignOfAccount,
                                                    //                                   item.Amount,
                                                    //                                   null,
                                                    //                                   true,
                                                    //                                   validation.userID,
                                                    //                                   DateTime.Now,
                                                    //                                   validation.userID,
                                                    //                                   DateTime.Now);


                                                    ClientAccount ClientAccountDB = new ClientAccount();
                                                    ClientAccountDB.DailyAdjustingEntryId = DailyJournalEntryID;
                                                    ClientAccountDB.AccountOfJeid = AccountOfJEID;
                                                    ClientAccountDB.ClientId = (long)item.ClinetID;
                                                    ClientAccountDB.ProjectId = item.ProjectID;
                                                    ClientAccountDB.AccountId = item.AccountID;
                                                    ClientAccountDB.DailyAdjustingEntryId = DailyJournalEntryID;
                                                    ClientAccountDB.AccountType = AccountObjDB.AccountTypeName;
                                                    ClientAccountDB.AmountSign = item.SignOfAccount;
                                                    ClientAccountDB.Amount = item.Amount;
                                                    ClientAccountDB.Active = true;
                                                    ClientAccountDB.CreatedBy = creator;
                                                    ClientAccountDB.ModifiedBy = creator;
                                                    ClientAccountDB.CreationDate = DateTime.Now;
                                                    ClientAccountDB.Modified = DateTime.Now;
                                                    ClientAccountDB.Description = item.Comment;
                                                    ClientAccountDB.OfferId = item.OfferID;
                                                    _unitOfWork.ClientAccounts.Add(ClientAccountDB);
                                                    _unitOfWork.Complete();

                                                    #region Closed Project when Installaton Completed & Remain = 0
                                                    #endregion
                                                }
                                                else if (/*CategoryName == "Expenses" && item.IncOrExTypeID == 2 && */item.SupplierID != 0 && item.SupplierID != null)
                                                {


                                                    long? PurchasePOID = item.PurchasePOID;
                                                    //ObjectParameter IDSupplierAccount = new ObjectParameter("ID", typeof(long));
                                                    //_Context.proc_SupplierAccountsInsert(IDSupplierAccount, DailyJournalEntryID,
                                                    //                                     item.SupplierID,
                                                    //                                     PurchasePOID,
                                                    //                                     item.AccountID,
                                                    //                                     AccountObjDB.AccountTypeName,
                                                    //                                     item.SignOfAccount,
                                                    //                                     item.Amount,
                                                    //                                     null,
                                                    //                                     true,
                                                    //                                     validation.userID,
                                                    //                                     DateTime.Now,
                                                    //                                     validation.userID,
                                                    //                                     DateTime.Now
                                                    //                                     );

                                                    SupplierAccount SupplierAccountDB = new SupplierAccount();
                                                    SupplierAccountDB.DailyAdjustingEntryId = DailyJournalEntryID;
                                                    SupplierAccountDB.SupplierId = (long)item.SupplierID;
                                                    SupplierAccountDB.AccountId = item.AccountID;
                                                    SupplierAccountDB.PurchasePoid = PurchasePOID;
                                                    SupplierAccountDB.AccountType = AccountObjDB.AccountTypeName;
                                                    SupplierAccountDB.AmountSign = item.SignOfAccount;
                                                    SupplierAccountDB.Amount = item.Amount;
                                                    SupplierAccountDB.Active = true;
                                                    SupplierAccountDB.CreatedBy = creator;
                                                    SupplierAccountDB.CreationDate = DateTime.Now;
                                                    SupplierAccountDB.ModifiedBy = creator;
                                                    SupplierAccountDB.Modified = DateTime.Now;
                                                    SupplierAccountDB.AccountOfJeid = AccountOfJEID;
                                                    SupplierAccountDB.Description = item.Comment;

                                                    _unitOfWork.SupplierAccounts.Add(SupplierAccountDB);
                                                    _unitOfWork.Complete();
                                                }

                                                if (CategoryName == "Expenses")
                                                {
                                                    // for Cost Center 
                                                    ExpensesTypeID = item.IncOrExTypeID;
                                                    ExpensesTypeName = item.IncOrExTypeName;
                                                }
                                            }
                                            _unitOfWork.Complete();
                                        }


                                    }
                                    // Cost Center
                                    if (request.Data.CostCenterStatus == true)
                                    {



                                        var CostCenterDB = _unitOfWork.DailyAdjustingEntryCostCenters.FindAll(x => x.DailyAdjustingEntryId == DailyJournalEntryID).FirstOrDefault();
                                        if (CostCenterDB != null)
                                        {
                                            CostCenterDB.CostCenterId = request.Data.CostCenter.CostCenterID;
                                            CostCenterDB.TypeId = ExpensesTypeID;
                                            CostCenterDB.Type = ExpensesTypeName;
                                            CostCenterDB.Amount = TotalAmountCreditAcc;
                                            CostCenterDB.ModifiedBy = creator;
                                            CostCenterDB.Modified = DateTime.Now;
                                        }
                                        else
                                        {
                                            CostCenterDB = new DailyAdjustingEntryCostCenter();
                                            CostCenterDB.DailyAdjustingEntryId = DailyJournalEntryID;
                                            CostCenterDB.CostCenterId = request.Data.CostCenter.CostCenterID;
                                            CostCenterDB.TypeId = ExpensesTypeID;
                                            CostCenterDB.Type = ExpensesTypeName;
                                            CostCenterDB.ProductId = 0;
                                            CostCenterDB.ProductGroupId = 0;
                                            CostCenterDB.Quantity = 0;
                                            CostCenterDB.Description = "";
                                            CostCenterDB.Amount = TotalAmountCreditAcc;
                                            CostCenterDB.EntryType = "From Adjusting Entry";
                                            CostCenterDB.Active = true;
                                            CostCenterDB.CreatedBy = creator;
                                            CostCenterDB.CreationDate = DateTime.Now;
                                            CostCenterDB.ModifiedBy = creator;
                                            CostCenterDB.Modified = DateTime.Now;
                                            var DailyEntryCostCenter = _unitOfWork.DailyAdjustingEntryCostCenters.Add(CostCenterDB);
                                        }
                                        var DailuEntryCostCenterSave = _unitOfWork.Complete();

                                        if (DailuEntryCostCenterSave > 0)
                                        {
                                            var generalCostCenters = _unitOfWork.GeneralActiveCostCenters.FindAll(x => x.Id == CostCenterDB.Id).FirstOrDefault();
                                            if (generalCostCenters != null)
                                            {
                                                generalCostCenters.CumulativeCost = generalCostCenters.CumulativeCost
                                                                                                - 0 //totalAmountOld if Edit old daily jouyrnal
                                                                                                + TotalAmountCreditAcc; // new  daily journal
                                                _unitOfWork.Complete();
                                            }
                                        }




                                    }



                                    // BeneficiaryToUser
                                    if (request.Data.BeneficiaryToUser != null)
                                    {

                                        if (request.Data.BeneficiaryToUser.BeneficiaryID != 0)
                                        {
                                            long BeneficiaryTypeID = request.Data.BeneficiaryToUser.BeneficiaryID;
                                            long AssignBeneficiaryID = request.Data.BeneficiaryToUser.AssignBeneficiaryID;
                                            string BeneficiaryTypeName = Common.GetBeneficiaryTypeName(BeneficiaryTypeID, _Context);
                                            string BeneficiaryUserName = "";

                                            if (AssignBeneficiaryID != 0)
                                            {
                                                if (BeneficiaryTypeID == 1)
                                                {
                                                    BeneficiaryUserName = Common.GetSupplierName(AssignBeneficiaryID, _Context);
                                                }
                                                else if (BeneficiaryTypeID == 2)
                                                {
                                                    BeneficiaryUserName = Common.GetClientName(BeneficiaryTypeID, _Context);
                                                }
                                                else if (BeneficiaryTypeID == 3)
                                                {
                                                    BeneficiaryUserName = Common.GetUserName(BeneficiaryTypeID, _Context);
                                                }
                                                else if (BeneficiaryTypeID == 4)
                                                {
                                                    BeneficiaryUserName = Common.GetBeneficiaryGeneralName(BeneficiaryTypeID, _Context);
                                                }
                                                else if (BeneficiaryTypeID == 5)
                                                {
                                                    BeneficiaryUserName = Common.GetUserName(BeneficiaryTypeID, _Context);
                                                }
                                                else if (BeneficiaryTypeID == 6)
                                                {
                                                    BeneficiaryUserName = Common.GetUserName(BeneficiaryTypeID, _Context);
                                                }
                                            }


                                            var BeneficiaryDB = _unitOfWork.DailyTranactionBeneficiaryToUsers.FindAll(x => x.DailyTransactionId == DailyJournalEntryID).FirstOrDefault();
                                            if (BeneficiaryDB != null)
                                            {
                                                BeneficiaryDB.BeneficiaryTypeId = BeneficiaryTypeID;
                                                BeneficiaryDB.BeneficiaryTypeName = BeneficiaryTypeName;
                                                BeneficiaryDB.BeneficiaryUserId = AssignBeneficiaryID;
                                                BeneficiaryDB.BeneficiaryUserName = BeneficiaryUserName;
                                                BeneficiaryDB.ModifiedBy = creator;
                                                BeneficiaryDB.ModifiedDate = DateTime.Now;
                                            }
                                            else
                                            {
                                                BeneficiaryDB = new DailyTranactionBeneficiaryToUser();
                                                BeneficiaryDB.DailyTransactionId = DailyJournalEntryID;
                                                BeneficiaryDB.BeneficiaryTypeId = BeneficiaryTypeID;
                                                BeneficiaryDB.BeneficiaryTypeName = BeneficiaryTypeName;
                                                BeneficiaryDB.BeneficiaryUserId = AssignBeneficiaryID;
                                                BeneficiaryDB.BeneficiaryUserName = BeneficiaryUserName;

                                                BeneficiaryDB.Description = "";
                                                BeneficiaryDB.Active = true;
                                                BeneficiaryDB.CreatedBy = creator;
                                                BeneficiaryDB.CreationDate = DateTime.Now;
                                                BeneficiaryDB.ModifiedBy = creator;
                                                BeneficiaryDB.ModifiedDate = DateTime.Now;
                                                _unitOfWork.DailyTranactionBeneficiaryToUsers.Add(BeneficiaryDB);
                                            }

                                            _unitOfWork.Complete();
                                        }

                                    }

                                }




                                // update Accounts 
                                //_Context.acc
                                // CostCenter
                                //_Context.DailyAdjustingEntryCostCenter
                                //_Context.GeneralActiveCostCenters
                                //_Context.DailyTranactionBeneficiaryToUsers
                                //_Context.DailyTranactionAttachment
                                Response.ID = DailyJournalEntryID;
                            }
                        }

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

        public void UpdateParentAccountAccumilativetest(long ParentAccountID, decimal Balance)
        {
            var ParentAccountDB = _unitOfWork.Accounts.FindAll(x => x.Id == ParentAccountID).FirstOrDefault();
            if (ParentAccountDB != null)
            {
                ParentAccountDB.Accumulative += Balance;

                _unitOfWork.Complete();

                UpdateParentAccountAccumilativetest(ParentAccountDB.ParentCategory ?? 0, Balance);
            }
        }

        public BaseResponseWithId UpdateJournalEntryWithAccountAcc()
        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var AccountList = _unitOfWork.Accounts.FindAll(x => x.Active == true).ToList();
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
                        AccountObj.Accumulative = ListAcc.OrderByDescending(a => a.CreationDate).FirstOrDefault()?.Acc_Calc ?? 0;
                        var res = _Context.SaveChanges();
                        if (AccountObj.ParentCategory != null && AccountObj.ParentCategory != 0 && res > 0)
                        {
                            UpdateParentAccountAccumilativetest(AccountObj.ParentCategory ?? 0, AccountObj.Accumulative);
                        }
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



        public async Task<GetCalcDetailsResponse> GetCalcClientCollectedDetails([FromHeader] long ClientId = 0)
        {
            GetCalcDetailsResponse Response = new GetCalcDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    //long ClientId = 0;
                    if (ClientId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Client Id";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    // Check if Project exist 
                    var ClientObjDB = _unitOfWork.Clients.FindAllAsync(x => x.Id == ClientId, includes: new[] { "SalesOffers.Projects", "ClientAccounts" }).Result.FirstOrDefault();
                    if (ClientObjDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P212";
                        error.ErrorMSG = "Invalid Client Id";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var TotalProjectAmount = ClientObjDB.SalesOffers.Sum(x => x.Projects.Where(y => y.Active == true).Select(t => t.TotalCost).Sum());
                    Response.TotalCollected = ClientObjDB.ClientAccounts.Where(x => x.Active == true).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                    Response.TotalAmount = TotalProjectAmount ?? 0;
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

        public async Task<GetCalcDetailsResponse> GetCalcSupplierCollectedDetails([FromHeader] long SupplierId = 0)
        {
            GetCalcDetailsResponse Response = new GetCalcDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (SupplierId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Supplier Id";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var SupplierObjDB = _unitOfWork.Suppliers.FindAll(x => x.Id == SupplierId).FirstOrDefault();
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

        public async Task<BaseResponseWithID> AddReverseDailyJournalEntry(AddReverseDailyJournalEntryRequest request, long creator)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    long DailyJournalEntryID = 0;
                    bool IsReverse = false;

                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.DailyJournalEntryId == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid DailyJournalEntryId";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    else
                    {
                        DailyJournalEntryID = (long)request.DailyJournalEntryId;
                    }
                    if (request.IsReverse == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid IsReverse flag";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    else
                    {
                        IsReverse = (bool)request.IsReverse;
                        if (IsReverse)
                        {
                            if (!Common.CheckUserRole(creator, 107, _Context))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P13";
                                error.ErrorMSG = "Not have Role to  Reverse  Journal Entry";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            if (!Common.CheckUserRole(creator, 106, _Context))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P13";
                                error.ErrorMSG = "Not have Role to  Delete  Journal Entry";
                                Response.Errors.Add(error);
                            }
                        }
                    }


                    if (Response.Result)
                    {
                        long? ExpensesTypeID = 0;
                        string ExpensesTypeName = "";
                        var EntryObjDB = _unitOfWork.VDailyJournalEntries.FindAll(x => x.Id == DailyJournalEntryID).FirstOrDefault();
                        if (EntryObjDB != null)
                        {


                            if (!await Common.CheckFinancialPeriodIsAvaliable(EntryObjDB.EntryDate, _Context))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P133";
                                error.ErrorMSG = "There is no financial period during Parent Entry Date ";
                                Response.Errors.Add(error);
                                return Response;
                            }

                            if (!await Common.CheckFinancialPeriodIsAvaliable(EntryObjDB.CreationDate, _Context))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P133";
                                error.ErrorMSG = "There is no financial period during Parent Creation Date ";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            string OldSerial = "0/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString();
                            var lastDailyJournalSerial = await _Context.VDailyJournalEntries.Where(a => a.Active == true && a.Serial != OldSerial).OrderByDescending(a => a.Id).Select(a => a.Serial).FirstOrDefaultAsync();
                            string Serial = "1/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Year.ToString();
                            if (lastDailyJournalSerial != null && lastDailyJournalSerial.Contains(System.DateTime.Now.Year.ToString()))
                            {
                                if (lastDailyJournalSerial.Split('/').Length > 0)
                                {
                                    string strLastDailyNumber = lastDailyJournalSerial.Split('/')[0];
                                    long newNumber = long.Parse(strLastDailyNumber) + 1;
                                    Serial = newNumber + "/" + DateTime.Now.Month.ToString() + "/" + System.DateTime.Now.Year.ToString();
                                }
                            }


                            // Account Of Daily Journal Entry
                            var AccountOfEntryListDB = _unitOfWork.VAccountOfJournalEntryWithDailies.FindAll(x => x.EntryId == DailyJournalEntryID).ToList();
                            if (AccountOfEntryListDB.Count() == 0)// For Parent Journal Entry
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "This DailyJournal not have Account List";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            decimal TotalAmountCreditAcc = AccountOfEntryListDB.Where(x => (x.AccountTypeName.ToLower() == "credit" && x.SignOfAccount.ToLower() == "plus")
                                                                                                || (x.AccountTypeName.ToLower() == "debit" && x.SignOfAccount.ToLower() == "minus")).Select(x => x.Amount).Sum();

                            // Default Delete
                            string Desc = "Automatic Journal Entry (Reversing Entry) for Deleting Entry No.#" + request.DailyJournalEntryId + "\n إدخال دفتر اليومية التلقائي (عكس الإدخال) لحذف الإدخال رقم " + request.DailyJournalEntryId;
                            if (IsReverse)
                            {
                                Desc = "Automatic Journal Entry (Reversing Entry) for Reverse  Entry No.#" + request.DailyJournalEntryId + "\n إدخال دفتر اليومية التلقائي (عكس الإدخال) لعكس  الإدخال رقم " + request.DailyJournalEntryId;
                                TotalAmountCreditAcc = TotalAmountCreditAcc * 2;
                            }


                            long NewDailyJounalEntryID = 0;
                            // Daily Journal Entry Insertion
                            var IDDailyJournalEntry = new DailyJournalEntry()
                            {
                                Description = Desc,
                                Serial = Serial,
                                Closed = false,
                                TotalAmount = TotalAmountCreditAcc,
                                DocumentNumber = EntryObjDB.DocumentNumber,
                                Approval = EntryObjDB.Approval,
                                Active = true,
                                CreatedBy = creator,
                                CreationDate = EntryObjDB.CreationDate,
                                ModifiedBy = creator,
                                ModifiedDate = DateTime.Now,
                                EntryDate = EntryObjDB.EntryDate
                            };
                            _Context.DailyJournalEntries.Add(IDDailyJournalEntry);
                            var DailyJournalEntry = _Context.SaveChanges();
                            /*                            ObjectParameter IDDailyJournalEntry = new ObjectParameter("ID", typeof(long));
                                                        var DailyJournalEntry = _Context.proc_DailyJournalEntryInsert(IDDailyJournalEntry,
                                                                                               Desc,
                                                                                               Serial,
                                                                                               false,
                                                                                               TotalAmountCreditAcc,
                                                                                               EntryObjDB.DocumentNumber,
                                                                                               EntryObjDB.Approval,
                                                                                               true,
                                                                                               validation.userID,
                                                                                               EntryObjDB.CreationDate,
                                                                                               validation.userID,
                                                                                               DateTime.Now,
                                                                                               EntryObjDB.EntryDate);*/
                            if (DailyJournalEntry > 0)
                            {
                                NewDailyJounalEntryID = (long)IDDailyJournalEntry.Id;

                                // Update Parent DailyJournal Entry Reverse or delete
                                var EntryReverseObjDB = new DailyJournalEntryReverse();
                                EntryReverseObjDB.ParentDjentryId = DailyJournalEntryID;
                                EntryReverseObjDB.DjentryId = NewDailyJounalEntryID;
                                EntryReverseObjDB.Active = true;
                                EntryReverseObjDB.CreationDate = DateTime.Now;
                                EntryReverseObjDB.ModificationDate = DateTime.Now;
                                EntryReverseObjDB.CreatedBy = creator;
                                EntryReverseObjDB.ModifiedBy = creator;
                                var EntryReverseObj = _unitOfWork.DailyJournalEntryReverses.Add(EntryReverseObjDB);
                                _unitOfWork.Complete();


                                foreach (var item in AccountOfEntryListDB)
                                {
                                    decimal AccountAmount = item.Amount;
                                    var AccountObjDB = _unitOfWork.Accounts.FindAll(x => x.Id == item.AccountId).FirstOrDefault();

                                    if (AccountObjDB != null)
                                    {

                                        string SignOfAccount = item.SignOfAccount == "plus" ? "minus" : "plus"; // Reverse Accounts on reverse and delete
                                        if (IsReverse)
                                        {
                                            AccountAmount = 2 * AccountAmount;
                                        }

                                        string FromOrTo = "";
                                        decimal Credit = 0;
                                        decimal Debit = 0;
                                        decimal Amount = 0;
                                        decimal AmountForAccumulative = 0;
                                        decimal AccBalance = AccountObjDB.Accumulative;
                                        if (SignOfAccount == "plus")
                                        {
                                            AmountForAccumulative = AccountAmount;
                                            FromOrTo = "From";
                                        }
                                        else if (SignOfAccount == "minus")
                                        {
                                            AmountForAccumulative = -1 * AccountAmount;
                                            FromOrTo = "To";
                                        }
                                        AccBalance = AccBalance + AmountForAccumulative;

                                        if (AccountObjDB.AccountTypeName == "Credit")
                                        {
                                            if (SignOfAccount == "plus")
                                            {
                                                Credit = Math.Abs(AccountAmount);
                                                Debit = 0;
                                                Amount = -1 * Math.Abs(AccountAmount);

                                            }
                                            else if (SignOfAccount == "minus")
                                            {
                                                Credit = 0;
                                                Debit = Math.Abs(AccountAmount);
                                                Amount = Math.Abs(AccountAmount);
                                            }
                                        }
                                        else if (AccountObjDB.AccountTypeName == "Debit")
                                        {
                                            if (SignOfAccount == "plus")
                                            {
                                                Credit = 0;
                                                Debit = Math.Abs(AccountAmount);
                                                Amount = Math.Abs(AccountAmount);
                                            }
                                            else if (SignOfAccount == "minus")
                                            {
                                                Credit = Math.Abs(AccountAmount);
                                                Debit = 0;
                                                Amount = -1 * Math.Abs(AccountAmount);
                                            }
                                        }

                                        //Account Of Journal Entry Insertion
                                        var IDAccountOjJournal = new AccountOfJournalEntry()
                                        {
                                            EntryId = NewDailyJounalEntryID,
                                            AccountId = item.AccountId,
                                            FromOrTo = FromOrTo,
                                            Credit = Credit,
                                            Amount = Amount,
                                            SignOfAccount = SignOfAccount,
                                            CurrencyId = (int)AccountObjDB.CurrencyId,
                                            MethodId = item.MethodId,
                                            DtmainType = item.DtmainType,
                                            ExpOrIncTypeId = item.ExpOrIncTypeId,
                                            ExpOrIncTypeName = item.ExpOrIncTypeName,
                                            ExtraIdofType = item.ExtraIdofType,
                                            Active = true,
                                            CreatedBy = creator,
                                            CreationDate = item.CreationDate,
                                            ModifiedBy = creator,
                                            ModifiedDate = DateTime.Now,
                                            AccBalance = AccBalance,
                                            EntryDate = item.EntryDate
                                        };
                                        _unitOfWork.AccountOfJournalEntries.Add(IDAccountOjJournal);
                                        var ResAccountOfJE = _unitOfWork.Complete();
                                        /*ObjectParameter IDAccountOjJournal = new ObjectParameter("ID", typeof(long));
                                        var ResAccountOfJE = _Context.proc_AccountOfJournalEntryInsert(IDAccountOjJournal,
                                                                                    NewDailyJounalEntryID,
                                                                                    item.AccountID,
                                                                                    FromOrTo,
                                                                                    Credit,
                                                                                    Debit,
                                                                                    Amount,
                                                                                    SignOfAccount,
                                                                                    AccountObjDB.CurrencyID,
                                                                                    item.MethodID,
                                                                                    item.DTMainType, // Category Name = DTMainType
                                                                                    item.ExpOrIncTypeID,
                                                                                    item.ExpOrIncTypeName,
                                                                                    item.ExtraIDOfType,
                                                                                    true,
                                                                                    validation.userID,
                                                                                    item.CreationDate,
                                                                                    validation.userID,
                                                                                    DateTime.Now,
                                                                                    AccBalance,
                                                                                    item.EntryDate
                                                                                    );*/


                                        // Update Accounts
                                        AccountObjDB.Credit = AccountObjDB.Credit + Credit;
                                        AccountObjDB.Debit = AccountObjDB.Debit + Debit;
                                        AccountObjDB.Accumulative = AccBalance;
                                        AccountObjDB.TranactionStatus = true;
                                        _unitOfWork.Complete();

                                        long AccountOfJEID = 0;
                                        if (ResAccountOfJE > 0)
                                        {
                                            AccountOfJEID = (long)IDAccountOjJournal.Id;



                                            // Update All Parent Accounts

                                            UpdateParentAccountAccumilative(AccountObjDB.ParentCategory ?? 0, Credit, Debit, AmountForAccumulative);



                                            // add Client Account row
                                            if (item.DtmainType == "Income" && item.ExpOrIncTypeId == 8 /*&& item.ClinetID != 0 && item.ProjectID != 0 && item.ClinetID != null && item.ProjectID != null*/)
                                            {
                                                var ClientAccountObj = _unitOfWork.ClientAccounts.FindAll(x => x.AccountOfJeid != null ? x.AccountOfJeid == AccountOfJEID :
                                                                                                           (x.DailyAdjustingEntryId == DailyJournalEntryID && x.AccountId == item.AccountId)).FirstOrDefault();
                                                if (ClientAccountObj != null)
                                                {
                                                    //ObjectParameter IDClientAccount = new ObjectParameter("ID", typeof(long));
                                                    //_Context.proc_ClientAccountsInsert(IDClientAccount,
                                                    //                                   NewDailyJounalEntryID,
                                                    //                                   ClientAccountObj.ClientID,
                                                    //                                   ClientAccountObj.ProjectID,
                                                    //                                   item.AccountID,
                                                    //                                   AccountObjDB.AccountTypeName,
                                                    //                                   item.SignOfAccount,
                                                    //                                   item.Amount,
                                                    //                                   null,
                                                    //                                   true,
                                                    //                                   validation.userID,
                                                    //                                   DateTime.Now,
                                                    //                                   validation.userID,
                                                    //                                   DateTime.Now);

                                                    ClientAccount ClientAccountDB = new ClientAccount();
                                                    ClientAccountDB.DailyAdjustingEntryId = DailyJournalEntryID;
                                                    ClientAccountDB.AccountOfJeid = AccountOfJEID;
                                                    ClientAccountDB.ClientId = (long)ClientAccountObj.ClientId;
                                                    ClientAccountDB.ProjectId = ClientAccountObj.ProjectId;
                                                    //ClientAccountDB.OfferID = (long)ClientAccountObj.OfferID;
                                                    ClientAccountDB.AccountId = item.AccountId;
                                                    ClientAccountDB.DailyAdjustingEntryId = DailyJournalEntryID;
                                                    ClientAccountDB.AccountType = AccountObjDB.AccountTypeName;
                                                    ClientAccountDB.AmountSign = item.SignOfAccount;
                                                    ClientAccountDB.AmountSign = item.SignOfAccount;
                                                    ClientAccountDB.Amount = item.Amount;
                                                    ClientAccountDB.Active = true;
                                                    ClientAccountDB.CreatedBy = creator;
                                                    ClientAccountDB.ModifiedBy = creator;
                                                    ClientAccountDB.CreationDate = ClientAccountObj.CreationDate; //DateTime.Now;
                                                    ClientAccountDB.Modified = DateTime.Now;
                                                    _unitOfWork.ClientAccounts.Add(ClientAccountDB);
                                                    _unitOfWork.Complete();
                                                }

                                                #region Closed Project when Installaton Completed & Remain = 0
                                                #endregion
                                            }
                                            else if (item.DtmainType == "Expenses" && item.ExpOrIncTypeId == 2 /*&& item.SupplierID != 0*/)
                                            {
                                                var SupplierAccountObj = _unitOfWork.SupplierAccounts.FindAll(x => x.AccountOfJeid != null ? x.AccountOfJeid == AccountOfJEID :
                                                                                                           (x.DailyAdjustingEntryId == DailyJournalEntryID && x.AccountId == item.AccountId)).FirstOrDefault();
                                                if (SupplierAccountObj != null)
                                                {
                                                    long? PurchasePOID = SupplierAccountObj.PurchasePoid;
                                                    //ObjectParameter IDSupplierAccount = new ObjectParameter("ID", typeof(long));
                                                    //_Context.proc_SupplierAccountsInsert(IDSupplierAccount,
                                                    //                                        NewDailyJounalEntryID,
                                                    //                                     SupplierAccountObj.SupplierID,
                                                    //                                     PurchasePOID,
                                                    //                                     item.AccountID,
                                                    //                                     AccountObjDB.AccountTypeName,
                                                    //                                     item.SignOfAccount,
                                                    //                                     item.Amount,
                                                    //                                     null,
                                                    //                                     true,
                                                    //                                     validation.userID,
                                                    //                                     DateTime.Now,
                                                    //                                     validation.userID,
                                                    //                                     DateTime.Now
                                                    //                                     );



                                                    SupplierAccount SupplierAccountDB = new SupplierAccount();
                                                    SupplierAccountDB.DailyAdjustingEntryId = DailyJournalEntryID;
                                                    SupplierAccountDB.SupplierId = (long)SupplierAccountObj.SupplierId;
                                                    SupplierAccountDB.AccountId = item.AccountId;
                                                    SupplierAccountDB.PurchasePoid = PurchasePOID;
                                                    SupplierAccountDB.AccountType = AccountObjDB.AccountTypeName;
                                                    SupplierAccountDB.AmountSign = item.SignOfAccount;
                                                    SupplierAccountDB.Amount = item.Amount;
                                                    SupplierAccountDB.Active = true;
                                                    SupplierAccountDB.CreatedBy = creator;
                                                    SupplierAccountDB.CreationDate = SupplierAccountObj.CreationDate; // DateTime.Now;
                                                    SupplierAccountDB.ModifiedBy = creator;
                                                    SupplierAccountDB.Modified = DateTime.Now;
                                                    SupplierAccountDB.AccountOfJeid = AccountOfJEID;

                                                    _unitOfWork.SupplierAccounts.Add(SupplierAccountDB);
                                                    _unitOfWork.Complete();
                                                }
                                            }


                                        }
                                        if (item.DtmainType == "Expenses")
                                        {
                                            // for Cost Center 
                                            ExpensesTypeID = item.ExpOrIncTypeId;
                                            ExpensesTypeName = item.ExpOrIncTypeName;
                                        }
                                    }
                                }
                                // Cost Center
                                // Check if Ild DailyJournal Entry Have Cost Center
                                var CostCenterOldEntryDB = _unitOfWork.DailyAdjustingEntryCostCenters.FindAllAsync(x => x.DailyAdjustingEntryId == DailyJournalEntryID).Result.FirstOrDefault();
                                if (CostCenterOldEntryDB != null   /*Request.Data.CostCenterStatus == true*/)
                                {
                                    //var CostCenterDB = _Context.DailyAdjustingEntryCostCenters.Where(x => x.DailyAdjustingEntryID == NewDailyJounalEntryID).FirstOrDefault();
                                    //if (CostCenterDB != null)
                                    //{
                                    //    CostCenterDB.CostCenterID = Request.Data.CostCenter.CostCenterID;
                                    //    CostCenterDB.TypeID = ExpensesTypeID;
                                    //    CostCenterDB.Type = ExpensesTypeName;
                                    //    CostCenterDB.Amount = TotalAmountCreditAcc;
                                    //    CostCenterDB.ModifiedBy = validation.userID;
                                    //    CostCenterDB.Modified = DateTime.Now;
                                    //}
                                    //else
                                    //{
                                    var CostCenterDB = new DailyAdjustingEntryCostCenter();
                                    CostCenterDB.DailyAdjustingEntryId = NewDailyJounalEntryID;
                                    CostCenterDB.CostCenterId = CostCenterOldEntryDB.CostCenterId;
                                    CostCenterDB.TypeId = ExpensesTypeID;
                                    CostCenterDB.Type = ExpensesTypeName;
                                    CostCenterDB.ProductId = 0;
                                    CostCenterDB.ProductGroupId = 0;
                                    CostCenterDB.Quantity = 0;
                                    CostCenterDB.Description = "";
                                    CostCenterDB.Amount = TotalAmountCreditAcc * -1;
                                    CostCenterDB.EntryType = "From Adjusting Entry";
                                    CostCenterDB.Active = true;
                                    CostCenterDB.CreatedBy = creator;
                                    CostCenterDB.CreationDate = DateTime.Now;
                                    CostCenterDB.ModifiedBy = creator;
                                    CostCenterDB.Modified = DateTime.Now;
                                    var DailyEntryCostCenter = _unitOfWork.DailyAdjustingEntryCostCenters.Add(CostCenterDB);
                                    //}
                                    var DailuEntryCostCenterSave = _unitOfWork.Complete();

                                    if (DailuEntryCostCenterSave > 0)
                                    {
                                        var generalCostCenters = _unitOfWork.GeneralActiveCostCenters.FindAllAsync(x => x.Id == CostCenterDB.Id).Result.FirstOrDefault();
                                        if (generalCostCenters != null)
                                        {
                                            generalCostCenters.CumulativeCost = generalCostCenters.CumulativeCost
                                                                                            - 0 //totalAmountOld if Edit old daily jouyrnal
                                                                                            - TotalAmountCreditAcc; // new  daily journal
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }



                                // BeneficiaryToUser
                                var BenefisaryToUser = _unitOfWork.DailyTranactionBeneficiaryToUsers.FindAllAsync(x => x.DailyTransactionId == DailyJournalEntryID).Result.FirstOrDefault();
                                if (BenefisaryToUser != null)
                                {
                                    //long BeneficiaryTypeID = Request.Data.BeneficiaryToUser.BeneficiaryID;
                                    //long AssignBeneficiaryID = Request.Data.BeneficiaryToUser.AssignBeneficiaryID;
                                    //string BeneficiaryTypeName = Common.GetBeneficiaryTypeName(BeneficiaryTypeID);
                                    //string BeneficiaryUserName = "";

                                    //if (AssignBeneficiaryID != 0)
                                    //{
                                    //    if (BeneficiaryTypeID == 1)
                                    //    {
                                    //        BeneficiaryUserName = Common.GetSupplierName(AssignBeneficiaryID);
                                    //    }
                                    //    else if (BeneficiaryTypeID == 2)
                                    //    {
                                    //        BeneficiaryUserName = Common.GetClientName(BeneficiaryTypeID);
                                    //    }
                                    //    else if (BeneficiaryTypeID == 3)
                                    //    {
                                    //        BeneficiaryUserName = Common.GetUserName(BeneficiaryTypeID);
                                    //    }
                                    //    else if (BeneficiaryTypeID == 4)
                                    //    {
                                    //        BeneficiaryUserName = Common.GetBeneficiaryGeneralName(BeneficiaryTypeID);
                                    //    }
                                    //    else if (BeneficiaryTypeID == 5)
                                    //    {
                                    //        BeneficiaryUserName = Common.GetUserName(BeneficiaryTypeID);
                                    //    }
                                    //    else if (BeneficiaryTypeID == 6)
                                    //    {
                                    //        BeneficiaryUserName = Common.GetUserName(BeneficiaryTypeID);
                                    //    }
                                    //}


                                    //var BeneficiaryDB = _Context.DailyTranactionBeneficiaryToUsers.Where(x => x.DailyTransactionID == NewDailyJounalEntryID).FirstOrDefault();
                                    //if (BeneficiaryDB != null)
                                    //{
                                    //    BeneficiaryDB.BeneficiaryTypeID = BeneficiaryTypeID;
                                    //    BeneficiaryDB.BeneficiaryTypeName = BeneficiaryTypeName;
                                    //    BeneficiaryDB.BeneficiaryUserID = AssignBeneficiaryID;
                                    //    BeneficiaryDB.BeneficiaryUserName = BeneficiaryUserName;
                                    //    BeneficiaryDB.ModifiedBy = validation.userID;
                                    //    BeneficiaryDB.ModifiedDate = DateTime.Now;
                                    //}
                                    //else
                                    //{
                                    var BeneficiaryDB = new DailyTranactionBeneficiaryToUser();
                                    BeneficiaryDB.DailyTransactionId = NewDailyJounalEntryID;
                                    BeneficiaryDB.BeneficiaryTypeId = BenefisaryToUser.BeneficiaryTypeId;
                                    BeneficiaryDB.BeneficiaryTypeName = BenefisaryToUser.BeneficiaryTypeName;
                                    BeneficiaryDB.BeneficiaryUserId = BenefisaryToUser.BeneficiaryUserId;
                                    BeneficiaryDB.BeneficiaryUserName = BenefisaryToUser.BeneficiaryUserName;

                                    BeneficiaryDB.Description = "";
                                    BeneficiaryDB.Active = true;
                                    BeneficiaryDB.CreatedBy = creator;
                                    BeneficiaryDB.CreationDate = DateTime.Now;
                                    BeneficiaryDB.ModifiedBy = creator;
                                    BeneficiaryDB.ModifiedDate = DateTime.Now;
                                    _unitOfWork.DailyTranactionBeneficiaryToUsers.Add(BeneficiaryDB);
                                    //}

                                    _unitOfWork.Complete();

                                }

                            }
                            Response.ID = NewDailyJounalEntryID;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "This Entry is not exist ";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }


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

        public async Task<BaseResponseWithID> SoftUpdateDailyJournalEntry(AddNewDailyJournalEntryRequest request, long creator)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.Data == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    bool? EntryIsPublic = null;
                    long ID = 0;
                    NewGaras.Infrastructure.Entities.DailyJournalEntry DailyJournalEntryDB = null;
                    if (request.Data.ID != 0)
                    {
                        ID = (long)request.Data.ID;
                        if (!await Common.CheckUserRoleAsync(creator, 105, _Context))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P133";
                            error.ErrorMSG = "Not have Role to Soft Edit Journal Entry";
                            Response.Errors.Add(error);
                            return Response;
                        }

                        // Check if this Entry is exist
                        DailyJournalEntryDB = _unitOfWork.DailyJournalEntries.FindAllAsync(x => x.Id == ID).Result.FirstOrDefault();
                        if (DailyJournalEntryDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P13";
                            error.ErrorMSG = "Invalid Journal Entry ID";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        EntryIsPublic = DailyJournalEntryDB.Approval;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P13";
                        error.ErrorMSG = "Invalid Journal Entry ID";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (string.IsNullOrEmpty(request.Data.DocumentNumber))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err501";
                        error.ErrorMSG = "Document Number is required.";
                        Response.Errors.Add(error);
                    }

                    if (string.IsNullOrEmpty(request.Data.Description))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err502";
                        error.ErrorMSG = "Description is required.";
                        Response.Errors.Add(error);
                    }
                    // Default Public Entry
                    if (request.Data.IsPublic != null)
                    {
                        // check Role Is Public or Private
                        if (request.Data.IsPublic == false && !await Common.CheckUserRoleAsync(creator, 111, _Context))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P133";
                            error.ErrorMSG = "Not have Role to select Private Entry";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        EntryIsPublic = (bool)request.Data.IsPublic;
                    }
                    DateTime? EntryDate = null;
                    DateTime EntryDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(request.Data.EntryDateSTR) && DateTime.TryParse(request.Data.EntryDateSTR, out EntryDateTemp))
                    {
                        if (EntryDateTemp.Date < DateTime.Now.Date)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err503";
                            error.ErrorMSG = "Entry Date Must be greater than today.";
                            Response.Errors.Add(error);
                        }
                        EntryDate = EntryDateTemp;
                    }

                    DateTime? CreationDate = null;
                    DateTime CreationDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(request.Data.CreationDateSTR) && DateTime.TryParse(request.Data.CreationDateSTR, out CreationDateTemp))
                    {
                        if (CreationDateTemp.Date < DateTime.Now.Date)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err503";
                            error.ErrorMSG = "Creation Date Must be greater than today.";
                            Response.Errors.Add(error);
                        }
                        CreationDate = CreationDateTemp;
                        // Check if have Role
                        //if (!await Common.CheckUserRoleAsync(validation.userID, 128))
                        //{
                        //    Response.Result = false;
                        //    Error error = new Error();
                        //    error.ErrorCode = "Err-P133";
                        //    error.ErrorMSG = "Not have Role to update Creation Date";
                        //    Response.Errors.Add(error);
                        //    return Response;
                        //}
                    }

                    // Check first on Entry Date and Creation Date

                    var FinancialPeriodListDB = _unitOfWork.AccountFinancialPeriods.GetAllAsync();



                    if (FinancialPeriodListDB.Result.Count() > 0)
                    {


                        var CheckDailyEntryDatePeriod = FinancialPeriodListDB.Result.Where(x => x.StartDate <= DailyJournalEntryDB.EntryDate &&
                                                                               (x.EndDate != null ? x.EndDate >= DailyJournalEntryDB.EntryDate : true)
                                                                                            && x.IsCurrent == true).FirstOrDefault();
                        if (CheckDailyEntryDatePeriod == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P133";
                            error.ErrorMSG = "This Daily entry date is out of financial period";
                            Response.Errors.Add(error);
                            return Response;
                        }


                        var CheckDailyCreationDatePeriod = FinancialPeriodListDB.Result.Where(x => x.StartDate <= DailyJournalEntryDB.CreationDate &&
                                                       (x.EndDate != null ? x.EndDate >= DailyJournalEntryDB.CreationDate : true)
                                                                    && x.IsCurrent == true).FirstOrDefault();
                        if (CheckDailyCreationDatePeriod == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P133";
                            error.ErrorMSG = "This Daily Creation date is out of financial period";
                            Response.Errors.Add(error);
                            return Response;
                        }


                        if (EntryDate != null)
                        {
                            var CheckEntryDatePeriod = FinancialPeriodListDB.Result.Where(x => x.StartDate <= (DateTime)EntryDate &&
                                                       (x.EndDate != null ? x.EndDate >= (DateTime)EntryDate : true)
                                                                    && x.IsCurrent == true).FirstOrDefault();
                            if (CheckEntryDatePeriod == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P133";
                                error.ErrorMSG = "This Daily entry date Selected is out of financial period";
                                Response.Errors.Add(error);
                                return Response;
                            }

                        }
                        if (CreationDate != null)
                        {
                            var CheckCreationDatePeriod = FinancialPeriodListDB.Result.Where(x => x.StartDate <= (DateTime)CreationDate &&
                                                       (x.EndDate != null ? x.EndDate >= (DateTime)CreationDate : true)
                                                                    && x.IsCurrent == true).FirstOrDefault();
                            if (CheckCreationDatePeriod == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P133";
                                error.ErrorMSG = "This Daily Creation date Selected is out of financial period";
                                Response.Errors.Add(error);
                                return Response;
                            }

                        }

                    }


                    if (Response.Result)
                    {


                        DailyJournalEntryDB.Description = request.Data.Description;
                        DailyJournalEntryDB.DocumentNumber = request.Data.DocumentNumber;
                        DailyJournalEntryDB.Approval = request.Data.IsPublic ?? true;
                        DailyJournalEntryDB.Active = true;
                        DailyJournalEntryDB.ModifiedDate = DateTime.Now;
                        DailyJournalEntryDB.ModifiedBy = creator;
                        if (EntryDate != null)
                        {
                            DailyJournalEntryDB.EntryDate = (DateTime)EntryDate;
                        }
                        if (CreationDate != null)
                        {
                            DailyJournalEntryDB.CreationDate = (DateTime)CreationDate;
                        }
                        var Result = _unitOfWork.Complete();
                        if (Result > 0)
                        {
                            if (CreationDate != null || EntryDate != null)
                            {
                                // Update List Of Account of Journal Entry
                                var AccountOfJournalEntryList = _unitOfWork.AccountOfJournalEntries.FindAllAsync(x => x.EntryId == DailyJournalEntryDB.Id).Result.ToList();
                                if (AccountOfJournalEntryList.Count() > 0)
                                {
                                    foreach (var AccOfEntry in AccountOfJournalEntryList)
                                    {
                                        if (CreationDate != null)
                                        {
                                            AccOfEntry.CreationDate = (DateTime)CreationDate;
                                        }
                                        if (EntryDate != null)
                                        {
                                            AccOfEntry.EntryDate = (DateTime)EntryDate;
                                        }
                                    }
                                }
                            }
                            if (CreationDate != null)
                            {
                                // Update Supplier Account 
                                var SupplierAccountList = _unitOfWork.SupplierAccounts.FindAllAsync(x => x.DailyAdjustingEntryId == DailyJournalEntryDB.Id).Result.ToList();
                                if (SupplierAccountList.Count() > 0)
                                {
                                    foreach (var SupplierAccount in SupplierAccountList)
                                    {
                                        SupplierAccount.CreationDate = (DateTime)CreationDate;
                                    }
                                }

                                // Update Client Account 
                                var ClientAccountList = _unitOfWork.ClientAccounts.FindAllAsync(x => x.DailyAdjustingEntryId == DailyJournalEntryDB.Id).Result.ToList();
                                if (ClientAccountList.Count() > 0)
                                {
                                    foreach (var ClientAccount in ClientAccountList)
                                    {
                                        ClientAccount.CreationDate = (DateTime)CreationDate;
                                    }
                                }
                            }
                            _unitOfWork.Complete();

                            // Check if have parent To Update  Private or Public 
                            var CheckHaveParentOfChildList = _unitOfWork.DailyJournalEntryReverses.FindAll(x => x.DjentryId == DailyJournalEntryDB.Id || x.ParentDjentryId == DailyJournalEntryDB.Id).ToList();
                            if (CheckHaveParentOfChildList.Count() > 0)
                            {
                                foreach (var item in CheckHaveParentOfChildList)
                                {
                                    if (item.DjentryId == DailyJournalEntryDB.Id)
                                    {
                                        // Update Parent 
                                        var DailyEntryObj = _unitOfWork.DailyJournalEntries.FindAllAsync(x => x.Id == item.ParentDjentryId).Result.FirstOrDefault();
                                        if (DailyEntryObj != null)
                                        {
                                            DailyEntryObj.Approval = DailyJournalEntryDB.Approval;
                                            _unitOfWork.Complete();
                                        }
                                    }
                                    else if (item.ParentDjentryId == DailyJournalEntryDB.Id)
                                    {
                                        // Update Child 
                                        var DailyEntryObj = _unitOfWork.DailyJournalEntries.FindAllAsync(x => x.Id == item.DjentryId).Result.FirstOrDefault();
                                        if (DailyEntryObj != null)
                                        {
                                            DailyEntryObj.Approval = DailyJournalEntryDB.Approval;
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }
                        }
                        Response.ID = DailyJournalEntryDB.Id;
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

        public async Task<BaseResponseWithId> PermanentDeleteDailyJournalEntry(AddNewDailyJournalEntryRequest request)

        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = false;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.Data == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    bool? EntryIsPublic = null;
                    long ID = 0;
                    NewGaras.Infrastructure.Entities.DailyJournalEntry DailyJournalEntryDB = null;
                    if (request.Data.ID != 0)
                    {
                        ID = (long)request.Data.ID;
                        //if (!await Common.CheckUserRoleAsync(validation.userID, 129))
                        //{
                        //    Response.Result = false;
                        //    Error error = new Error();
                        //    error.ErrorCode = "Err-P133";
                        //    error.ErrorMSG = "Not have Role to Permanent delete Journal Entry";
                        //    Response.Errors.Add(error);
                        //    return Response;
                        //}

                        // Check if this Entry is exist
                        DailyJournalEntryDB = _unitOfWork.DailyJournalEntries.FindAllAsync(x => x.Id == ID).Result.FirstOrDefault();
                        if (DailyJournalEntryDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P13";
                            error.ErrorMSG = "Invalid Journal Entry ID";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        EntryIsPublic = DailyJournalEntryDB.Approval;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P13";
                        error.ErrorMSG = "Invalid Journal Entry ID";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Response.Result)
                    {
                        // Validate if have Parent or Child before remove
                        var CheckReleationEntry = _unitOfWork.DailyJournalEntryReverses.FindAllAsync(x => x.ParentDjentryId == ID || x.DjentryId == ID).Result.FirstOrDefault();
                        if (CheckReleationEntry != null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P13";
                            error.ErrorMSG = "Can't delete this entry because already releated with another entry";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        // delete Client ACcount
                        var CheckClientAccountList = _unitOfWork.ClientAccounts.FindAllAsync(x => x.DailyAdjustingEntryId == ID).Result.ToList();
                        if (CheckClientAccountList.Count() > 0)
                        {
                            _Context.ClientAccounts.RemoveRange(CheckClientAccountList);
                            var ResToDeleteClientAccountList = _unitOfWork.Complete();
                        }
                        // delete Supplier Account 
                        var CheckSupplierAccountList = _unitOfWork.SupplierAccounts.FindAllAsync(x => x.DailyAdjustingEntryId == ID).Result.ToList();
                        if (CheckSupplierAccountList.Count() > 0)
                        {
                            _Context.SupplierAccounts.RemoveRange(CheckSupplierAccountList);
                            var ResToDeleteSupplierAccountList = _unitOfWork.Complete();
                        }
                        // delete Account of entry 
                        var AccountOfEntryList = _unitOfWork.AccountOfJournalEntries.FindAllAsync(x => x.EntryId == ID).Result.ToList();
                        if (AccountOfEntryList.Count() > 0)
                        {
                            _Context.AccountOfJournalEntries.RemoveRange(AccountOfEntryList);
                            var ResToDeleteAccountOfEntryList = _unitOfWork.Complete();
                        }


                        // Remove Entry 
                        _unitOfWork.DailyJournalEntries.Delete(DailyJournalEntryDB);
                        var Res = _unitOfWork.Complete();
                        if (Res > 0)
                        {
                            Response.Result = true;
                        }
                    }
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

        public async Task<BaseResponseWithID> UpdateEntryClientAccount(UpdateEntryClientAccountRequest request, long creator)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    long ClientAccountId = 0;
                    long EntryId = 0;
                    long AccountId = 0;
                    long ClientId = 0;
                    long ProjectId = 0;
                    string AccountType = null;
                    ClientAccount ClientAccountObjDB = null;
                    if (request.ClientAccountId != null && request.ClientAccountId != 0) // This Entry Already has ClientAccount .. and now want to edit
                    {
                        ClientAccountId = (long)request.ClientAccountId;
                        // Check if Client Account is exist
                        ClientAccountObjDB = _unitOfWork.ClientAccounts.FindAllAsync(x => x.Id == ClientAccountId).Result.FirstOrDefault();
                    }
                    // else Insert
                    if (request.EntryId == null || request.EntryId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P212";
                        error.ErrorMSG = "Invalid Entry Id";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        // Check if Entry exist 
                        var EntryObjDB = _unitOfWork.DailyJournalEntries.FindAllAsync(x => x.Id == request.EntryId).Result.FirstOrDefault();
                        if (EntryObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P212";
                            error.ErrorMSG = "Invalid Entry Id";
                            Response.Errors.Add(error);
                        }
                    }
                    if (request.AccountId == null || request.AccountId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Account Id";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        // Check if Account exist 
                        var AccountObjDB = _unitOfWork.Accounts.FindAllAsync(x => x.Id == request.AccountId).Result.FirstOrDefault();
                        if (AccountObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P212";
                            error.ErrorMSG = "Invalid Account Id";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            AccountType = AccountObjDB.AccountTypeName;
                        }
                    }

                    if (request.ClientId == null || request.ClientId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Client Id";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        // Check if Client exist 
                        var ClientObjDB = _unitOfWork.Clients.FindAllAsync(x => x.Id == request.ClientId).Result.FirstOrDefault();
                        if (ClientObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P212";
                            error.ErrorMSG = "Invalid Client Id";
                            Response.Errors.Add(error);
                        }
                    }

                    if (request.ProjectId == null || request.ProjectId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Project Id";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        // Check if Project exist 
                        var ProjectObjDB = _unitOfWork.Projects.FindAllAsync(x => x.Id == request.ProjectId).Result.FirstOrDefault();
                        if (ProjectObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P212";
                            error.ErrorMSG = "Invalid Project Id";
                            Response.Errors.Add(error);
                        }
                    }

                    if (request.Amount == null || request.Amount <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Amount";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.AmountSign) || (request.AmountSign.ToLower() != "minus" && request.AmountSign.ToLower() != "plus"))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Amount Sign";
                        Response.Errors.Add(error);
                    }
                    //Response.Result = false;
                    //Error error = new Error();
                    //error.ErrorCode = "Err502";
                    //error.ErrorMSG = "Description is required.";
                    //Response.Errors.Add(error);


                    if (Response.Result)
                    {
                        ClientId = (long)request.ClientId;
                        ProjectId = (long)request.ProjectId;
                        EntryId = (long)request.EntryId;
                        AccountId = (long)request.AccountId;

                        if (ClientAccountObjDB != null)
                        {
                            ClientAccountObjDB.DailyAdjustingEntryId = (long)request.EntryId;
                            ClientAccountObjDB.ProjectId = (long)request.ProjectId;
                            ClientAccountObjDB.ClientId = (long)request.ClientId;
                            ClientAccountObjDB.AccountId = (long)request.AccountId;
                            ClientAccountObjDB.AccountType = AccountType;
                            ClientAccountObjDB.AmountSign = request.AmountSign.ToLower();
                            ClientAccountObjDB.Amount = (decimal)request.Amount;
                            ClientAccountObjDB.Description = request.Description;
                            ClientAccountObjDB.Active = true;
                            ClientAccountObjDB.ModifiedBy = creator;
                            ClientAccountObjDB.Modified = DateTime.Now;
                        }
                        else
                        {
                            var CheckIsHaveClientAccount = _unitOfWork.ClientAccounts.FindAllAsync(x => x.DailyAdjustingEntryId == EntryId && x.AccountId == AccountId).Result.FirstOrDefault();
                            if (CheckIsHaveClientAccount == null)
                            {

                                ClientAccountObjDB = new ClientAccount();
                                ClientAccountObjDB.DailyAdjustingEntryId = (long)request.EntryId;
                                ClientAccountObjDB.ProjectId = (long)request.ProjectId;
                                ClientAccountObjDB.ClientId = (long)request.ClientId;
                                ClientAccountObjDB.AccountId = (long)request.AccountId;
                                ClientAccountObjDB.AccountType = AccountType;
                                ClientAccountObjDB.Amount = (decimal)request.Amount;
                                ClientAccountObjDB.AmountSign = request.AmountSign.ToLower();
                                ClientAccountObjDB.Description = request.Description;
                                ClientAccountObjDB.Active = true;
                                ClientAccountObjDB.CreatedBy = creator;
                                ClientAccountObjDB.ModifiedBy = creator;
                                ClientAccountObjDB.CreationDate = DateTime.Now;
                                ClientAccountObjDB.Modified = DateTime.Now;

                                _Context.ClientAccounts.Add(ClientAccountObjDB);
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P213";
                                error.ErrorMSG = "Already Entry and this account have record on client account";
                                Response.Errors.Add(error);
                                return Response;
                            }
                        }

                        var Result = _unitOfWork.Complete();
                        if (Result > 0)
                        {
                            Response.ID = ClientAccountObjDB.Id;
                            Response.Result = true;
                        }
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

        public async Task<BaseResponseWithID> UpdateEntrySupplierAccount(UpdateEntrySupplierAccountRequest request, long creator)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    long SupplierAccountId = 0;
                    long EntryId = 0;
                    long AccountId = 0;
                    long SupplierId = 0;
                    long POId = 0;
                    string AccountType = null;
                    SupplierAccount SupplierAccountObjDB = null;
                    if (request.SupplierAccountId != null && request.SupplierAccountId != 0) // This Entry Already has ClientAccount .. and now want to edit
                    {
                        SupplierAccountId = (long)request.SupplierAccountId;
                        // Check if Client Account is exist
                        SupplierAccountObjDB = await _Context.SupplierAccounts.Where(x => x.Id == SupplierAccountId).FirstOrDefaultAsync();
                    }
                    // else Insert
                    if (request.EntryId == null || request.EntryId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P212";
                        error.ErrorMSG = "Invalid Entry Id";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        // Check if Entry exist 
                        var EntryObjDB = _unitOfWork.DailyJournalEntries.FindAllAsync(x => x.Id == request.EntryId).Result.FirstOrDefault();
                        if (EntryObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P212";
                            error.ErrorMSG = "Invalid Entry Id";
                            Response.Errors.Add(error);
                        }
                    }
                    if (request.AccountId == null || request.AccountId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Account Id";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        // Check if Account exist 
                        var AccountObjDB = _unitOfWork.Accounts.FindAllAsync(x => x.Id == request.AccountId).Result.FirstOrDefault();
                        if (AccountObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P212";
                            error.ErrorMSG = "Invalid Account Id";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            AccountType = AccountObjDB.AccountTypeName;
                        }
                    }

                    if (request.SupplierId == null || request.SupplierId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Client Id";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        // Check if Client exist 
                        var SupplierObjDB = _unitOfWork.Suppliers.FindAllAsync(x => x.Id == request.SupplierId).Result.FirstOrDefault();
                        if (SupplierObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P212";
                            error.ErrorMSG = "Invalid Supplier Id";
                            Response.Errors.Add(error);
                        }
                    }

                    if (request.POId == null || request.POId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid PO Id";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        // Check if PO exist 
                        var POIDObjDB = _unitOfWork.PurchasePos.FindAllAsync(x => x.Id == request.POId).Result.FirstOrDefault();
                        if (POIDObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P212";
                            error.ErrorMSG = "Invalid PO Id";
                            Response.Errors.Add(error);
                        }
                    }

                    if (request.Amount == null || request.Amount <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Amount";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.AmountSign) || (request.AmountSign.ToLower() != "minus" && request.AmountSign.ToLower() != "plus"))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Amount Sign";
                        Response.Errors.Add(error);
                    }
                    //Response.Result = false;
                    //Error error = new Error();
                    //error.ErrorCode = "Err502";
                    //error.ErrorMSG = "Description is required.";
                    //Response.Errors.Add(error);


                    if (Response.Result)
                    {
                        SupplierId = (long)request.SupplierId;
                        POId = (long)request.POId;
                        EntryId = (long)request.EntryId;
                        AccountId = (long)request.AccountId;

                        if (SupplierAccountObjDB != null)
                        {
                            SupplierAccountObjDB.DailyAdjustingEntryId = (long)request.EntryId;
                            SupplierAccountObjDB.PurchasePoid = POId;
                            SupplierAccountObjDB.SupplierId = SupplierId;
                            SupplierAccountObjDB.AccountId = (long)request.AccountId;
                            SupplierAccountObjDB.AccountType = AccountType;
                            SupplierAccountObjDB.AmountSign = request.AmountSign.ToLower();
                            SupplierAccountObjDB.Amount = (decimal)request.Amount;
                            SupplierAccountObjDB.Description = request.Description;
                            SupplierAccountObjDB.Active = true;
                            SupplierAccountObjDB.ModifiedBy = creator;
                            SupplierAccountObjDB.Modified = DateTime.Now;
                        }
                        else
                        {
                            var CheckIsHaveSupplierAccount = _unitOfWork.SupplierAccounts.FindAllAsync(x => x.DailyAdjustingEntryId == EntryId && x.AccountId == AccountId).Result.FirstOrDefault();
                            if (CheckIsHaveSupplierAccount == null)
                            {
                                SupplierAccountObjDB = new SupplierAccount();
                                SupplierAccountObjDB.DailyAdjustingEntryId = (long)request.EntryId;
                                SupplierAccountObjDB.PurchasePoid = POId;
                                SupplierAccountObjDB.SupplierId = SupplierId;
                                SupplierAccountObjDB.AccountId = (long)request.AccountId;
                                SupplierAccountObjDB.AccountType = AccountType;
                                SupplierAccountObjDB.AmountSign = request.AmountSign.ToLower();
                                SupplierAccountObjDB.Amount = (decimal)request.Amount;
                                SupplierAccountObjDB.Description = request.Description;
                                SupplierAccountObjDB.Active = true;
                                SupplierAccountObjDB.CreatedBy = creator;
                                SupplierAccountObjDB.ModifiedBy = creator;
                                SupplierAccountObjDB.CreationDate = DateTime.Now;
                                _Context.SupplierAccounts.Add(SupplierAccountObjDB);
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P213";
                                error.ErrorMSG = "Already Entry and this account have record on Supplier account";
                                Response.Errors.Add(error);
                                return Response;
                            }

                        }

                        var Result = _unitOfWork.Complete();
                        if (Result > 0)
                        {
                            Response.ID = SupplierAccountObjDB.Id;
                            Response.Result = true;
                        }

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

        public async Task<DailyJournalEntryInfoResponse> GetDailyJournalEntryDataInfo([FromHeader] long DailyJournalEntryID = 0, [FromHeader] bool Active = true)
        {
            DailyJournalEntryInfoResponse Response = new DailyJournalEntryInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var EntryInfo = new DailyJournalEntryPrimaryInfo();

                if (Response.Result)
                {
                    if (DailyJournalEntryID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err381";
                        error.ErrorMSG = "Invalid DailyJournalEntryID";
                        Response.Errors.Add(error);
                    }
                    if (Response.Result)
                    {
                        var DailyJournalEntryObjDB = (await _unitOfWork.VDailyJournalEntries.FindAllAsync(x => x.Id == DailyJournalEntryID)).FirstOrDefault();
                        if (DailyJournalEntryObjDB != null)
                        {
                            var EntryReverseList = (await _unitOfWork.DailyJournalEntryReverses.FindAllAsync(x => x.ParentDjentryId == DailyJournalEntryObjDB.Id || x.DjentryId == DailyJournalEntryObjDB.Id)).ToList();

                            EntryInfo.ParentEntryId = EntryReverseList.Where(x => x.DjentryId == DailyJournalEntryObjDB.Id).Select(x => x.ParentDjentryId).FirstOrDefault();
                            if (EntryInfo.ParentEntryId != null && EntryInfo.ParentEntryId != 0)
                            {
                                EntryInfo.ParentEntrySerial = Common.GetDailyJournalEntrySerial((long)EntryInfo.ParentEntryId, _Context);
                            }
                            EntryInfo.ChildEntryId = EntryReverseList.Where(x => x.ParentDjentryId == DailyJournalEntryObjDB.Id).Select(x => x.DjentryId).FirstOrDefault();
                            if (EntryInfo.ChildEntryId != null && EntryInfo.ChildEntryId != 0)
                            {
                                EntryInfo.ChildEntrySerial = Common.GetDailyJournalEntrySerial((long)EntryInfo.ChildEntryId, _Context);
                            }
                            // Basic
                            EntryInfo.ID = DailyJournalEntryObjDB.Id;
                            EntryInfo.IsPublic = DailyJournalEntryObjDB.Approval;
                            EntryInfo.Active = DailyJournalEntryObjDB.Active;
                            EntryInfo.CreationDate = DailyJournalEntryObjDB.CreationDate.ToString("yyyy-MM-dd");
                            EntryInfo.EntryDate = DailyJournalEntryObjDB.EntryDate.ToString("yyyy-MM-dd");
                            EntryInfo.EntryDateSTR = DailyJournalEntryObjDB.EntryDate.ToString();
                            EntryInfo.Serial = DailyJournalEntryObjDB.Serial;
                            EntryInfo.Status = DailyJournalEntryObjDB.Closed;
                            EntryInfo.CreationUser = DailyJournalEntryObjDB.CreatorFirstName + " " + DailyJournalEntryObjDB.CreatorLastName;
                            if (Common.GetUserPhoto(DailyJournalEntryObjDB.CreatedBy, _Context) != null)
                            {
                                EntryInfo.CreationUserImg = Globals.baseURL + Common.GetUserPhoto(DailyJournalEntryObjDB.CreatedBy, _Context);
                                /*"/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(DailyJournalEntryObjDB.CreatedBy.ToString(), key)) + "&type=photo&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();*/
                            }
                            EntryInfo.Description = DailyJournalEntryObjDB.Description;
                            EntryInfo.DocumentNumber = DailyJournalEntryObjDB.DocumentNumber;
                            EntryInfo.AmountTranaction = DailyJournalEntryObjDB.TotalAmount;


                            // From - To Account Entry
                            var AccountEntryList = new List<EntryAccount>();
                            var AdjustingEntryAccountList = new List<AdjustingEntryAccount>();
                            var AccountOfJournalEntryList = _unitOfWork.VAccountOfJournalEntryWithDailies.FindAll(x => x.EntryId == DailyJournalEntryObjDB.Id && x.Active == true).ToList();
                            if (Active)
                            {
                                AccountOfJournalEntryList = AccountOfJournalEntryList.Where(x => x.Active == true).ToList();
                            }

                            if (AccountOfJournalEntryList.Count > 0)
                            {
                                var IDSAccounts = AccountOfJournalEntryList.Select(x => x.AccountId).ToList();
                                var AdvanciedTypeListDB = _unitOfWork.AdvanciedSettingAccounts.FindAll(x => IDSAccounts.Contains(x.AccountId)).ToList();
                                var AccountOfJournalEntryWithOtherCUList = _unitOfWork.AccountOfJournalEntryOtherCurrencies.FindAll(x => x.EntryId == DailyJournalEntryObjDB.Id && x.Active == true).ToList();
                                foreach (var item in AccountOfJournalEntryList)
                                {
                                    var ObjAccountOtherCurrency = AccountOfJournalEntryWithOtherCUList.Where(x => x.AccountId == item.AccountId).FirstOrDefault();
                                    var AccEntryObj = new EntryAccount();
                                    AccEntryObj.AccountID = item.AccountId;
                                    AccEntryObj.AccountName = item.AccountName;
                                    AccEntryObj.Amount = item.Amount;
                                    AccEntryObj.CurrencyID = item.CurrencyId;
                                    AccEntryObj.CurrencyName = item.AccountCurrencyName;
                                    AccEntryObj.CategoryName = item.DtmainType;
                                    AccEntryObj.IncOrExTypeID = item.ExpOrIncTypeId;
                                    AccEntryObj.IncOrExTypeName = item.ExpOrIncTypeName;
                                    AccEntryObj.MethodTypeID = item.MethodId;
                                    AccEntryObj.AccountTypeName = item.AccountTypeName;
                                    AccEntryObj.SignOfAccount = item.SignOfAccount;
                                    AccEntryObj.RateToAnotherCU = ObjAccountOtherCurrency != null ? ObjAccountOtherCurrency.RateToLocalCu : 0;
                                    AccEntryObj.AmountAnotherCU = ObjAccountOtherCurrency != null ? ObjAccountOtherCurrency.Amount : 0;
                                    AccEntryObj.MethodTypeName = Common.GetPaymentMethodType(item.MethodId, _Context);
                                    var AdvanciedTypeDB = AdvanciedTypeListDB.Where(x => x.AccountId == item.AccountId).FirstOrDefault();
                                    if (AdvanciedTypeDB != null)
                                    {
                                        AccEntryObj.AdvanciedTypeID = AdvanciedTypeDB.AdvanciedTypeId;
                                    }
                                    if (item.FromOrTo.ToLower().Contains("from"))
                                    {
                                        AccEntryObj.FromAccount = true;
                                    }
                                    else if (item.FromOrTo.ToLower().Contains("to"))
                                    {
                                        AccEntryObj.FromAccount = false;
                                    }
                                    //if (item.ExpOrIncTypeName == "From Client")
                                    //{
                                    var ClientAccountObjDB = _unitOfWork.ClientAccounts.FindAll(x => x.AccountOfJeid != null ? x.AccountOfJeid == item.Id :
                                                                                          (x.DailyAdjustingEntryId == DailyJournalEntryObjDB.Id && x.AccountId == item.AccountId), includes: new[] { "Client", "Project.SalesOffer" }).FirstOrDefault();
                                    if (ClientAccountObjDB != null)
                                    {
                                        AccEntryObj.ClientAccountId = ClientAccountObjDB.Id;
                                        AccEntryObj.ClinetID = ClientAccountObjDB.ClientId;
                                        AccEntryObj.ClinetName = ClientAccountObjDB.Client.Name;
                                        AccEntryObj.ProjectID = ClientAccountObjDB.ProjectId;
                                        AccEntryObj.Comment = ClientAccountObjDB.Description;
                                        AccEntryObj.ProjectName = ClientAccountObjDB.Project?.SalesOffer?.ProjectName;
                                    }
                                    // }
                                    //else if (item.ExpOrIncTypeName == "Purchasing(TO Supplier)")
                                    // {
                                    var SupplierAccountObjDB = _unitOfWork.SupplierAccounts.FindAll(x => x.AccountOfJeid != null ? x.AccountOfJeid == item.Id :
                                                                                              (x.DailyAdjustingEntryId == DailyJournalEntryObjDB.Id && x.AccountId == item.AccountId)).FirstOrDefault();
                                    if (SupplierAccountObjDB != null)
                                    {
                                        AccEntryObj.SupplierAccountId = SupplierAccountObjDB.Id;
                                        AccEntryObj.SupplierID = SupplierAccountObjDB.SupplierId;
                                        AccEntryObj.SupplierName = Common.GetSupplierName(SupplierAccountObjDB.SupplierId, _Context);
                                        AccEntryObj.PurchasePOID = SupplierAccountObjDB.PurchasePoid;
                                        AccEntryObj.Comment = SupplierAccountObjDB.Description;
                                        AccEntryObj.PurchasePOName = SupplierAccountObjDB.PurchasePoid != null ? "PO_" + SupplierAccountObjDB.PurchasePoid : "";
                                    }
                                    // }
                                    AccountEntryList.Add(AccEntryObj);

                                    var AdjustingEntryAccount = new AdjustingEntryAccount();
                                    AdjustingEntryAccount.Account = item.AccountName;
                                    if (item.Credit != 0)
                                    {
                                        AdjustingEntryAccount.Amount = item.Credit;
                                        AdjustingEntryAccount.Type = "Credit";
                                    }
                                    else if (item.Debit != 0)
                                    {
                                        AdjustingEntryAccount.Amount = item.Credit;
                                        AdjustingEntryAccount.Type = "Debit";
                                    }


                                }
                            }


                            var CostCenter = new CostCenter();
                            //Cost Center
                            var DailyCostCenterObjDB = _unitOfWork.DailyAdjustingEntryCostCenters.FindAll(x => x.DailyAdjustingEntryId == DailyJournalEntryObjDB.Id).FirstOrDefault();
                            if (DailyCostCenterObjDB != null)
                            {
                                EntryInfo.CostCenterStatus = true;

                                CostCenter.ProjectID = DailyCostCenterObjDB.CostCenterId;
                                CostCenter.ProjectName = Common.GetProjectName(DailyCostCenterObjDB.CostCenterId ?? 0, _Context);
                                CostCenter.CostCenterAmount = DailyCostCenterObjDB.Amount;
                            }

                            // beneficiaryToUser
                            var beneficiaryToUser = new beneficiaryToUser();
                            var beneficiaryToUserObjDB = _unitOfWork.DailyTranactionBeneficiaryToUsers.FindAll(x => x.DailyTransactionId == DailyJournalEntryObjDB.Id).FirstOrDefault();
                            if (beneficiaryToUserObjDB != null)
                            {
                                beneficiaryToUser.BeneficiaryID = beneficiaryToUserObjDB.BeneficiaryTypeId;
                                beneficiaryToUser.BeneficiaryType = beneficiaryToUserObjDB.BeneficiaryTypeName;
                                beneficiaryToUser.AssignBeneficiaryID = beneficiaryToUserObjDB.BeneficiaryUserId;
                                beneficiaryToUser.AssignBeneficiaryName = beneficiaryToUserObjDB.BeneficiaryUserName;
                            }


                            EntryInfo.EntryAccountList = AccountEntryList;
                            EntryInfo.CostCenter = CostCenter;
                            EntryInfo.BeneficiaryToUser = beneficiaryToUser;
                            Response.Data = EntryInfo;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err381";
                            error.ErrorMSG = "Invalid DailyJournalEntryID";
                            Response.Errors.Add(error);
                            return Response;
                        }

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

        public async Task<AccountsEntryDDLResponse> GetAccountsEntryList([FromHeader] long AdvancedTypeId, long creator)
        {
            AccountsEntryDDLResponse Response = new AccountsEntryDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var AccountList = new List<AccountsEntryDDL>();
                if (Response.Result)
                {
                    var ParentActiveAccounts = _unitOfWork.Accounts.FindAllAsync(x => x.Active == true, includes: new[] { "AdvanciedSettingAccounts","Currency", "AccountCategory" }).Result.ToList();
                    var IDSAccounts = ParentActiveAccounts.Select(x => x.Id).ToList();
                    //var V_Accounts_AdvanciedSettingAccount = await _Context.V_Accounts_AdvanciedSettingAccount.Where(x => IDSAccounts.Contains(x.ID) && x.Active == true).ToListAsync();
                    var AccountListDB = ParentActiveAccounts.Where(x => x.Haveitem == false).ToList();
                    if (creator != 1) // if not user system
                    {
                        AccountListDB = AccountListDB.Where(x => x.AdvanciedSettingAccounts.Any(ad => ad.KeeperId == creator || ad.KeeperId == null)).ToList();
                    }
                    if (AdvancedTypeId > 0)
                    {
                        AccountListDB = AccountListDB.Where(x => x.AdvanciedSettingAccounts.Any(ad => ad.AdvanciedTypeId == AdvancedTypeId)).ToList();
                    }
                    if (AccountListDB.Count > 0)
                    {
                        foreach (var account in AccountListDB)
                        {
                            if (CheckParentAccountIsActive(ParentActiveAccounts, account.ParentCategory ?? 0))
                            {

                                var AccountObj = new AccountsEntryDDL();
                                AccountObj.ID = account.Id;
                                AccountObj.AccountName = account.AccountName;
                                AccountObj.AccountNumber = account.AccountNumber;
                                AccountObj.HaveChild = account.Haveitem;
                                AccountObj.AccountTypeName = account.AccountTypeName;
                                AccountObj.CurrencyID = account.CurrencyId;
                                AccountObj.CategoryID = account.AccountCategoryId;
                                AccountObj.CurrencyName = account.Currency?.Name;
                                AccountObj.CategoryName = account.AccountCategory?.AccountCategoryName;
                                AccountObj.AdvancedTypeId = account.AdvanciedSettingAccounts.Where(x => x.AccountId == account.Id).Select(x => x.AdvanciedTypeId).FirstOrDefault();
                                AccountObj.AdvancedTypeName = account.AdvanciedSettingAccounts.Where(x => x.AccountId == account.Id).Select(x => x.AdvanciedType?.AdvanciedTypeName).FirstOrDefault();
                                AccountList.Add(AccountObj);
                            }
                        }
                    }
                }
                Response.AccountList = AccountList;
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

        public async Task<GetAccountEntryListByFilterResponse> GetAccountEntryListByFilter([FromHeader] long POID = 0, [FromHeader] long ProjectID = 0, [FromHeader] long OfferID = 0)
        {
            GetAccountEntryListByFilterResponse Response = new GetAccountEntryListByFilterResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (POID == 0 && ProjectID == 0 && OfferID == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-222";
                    error.ErrorMSG = "Must be select at least one filter ";
                    Response.Errors.Add(error);
                }
                if (POID != 0 && ProjectID != 0 && OfferID != 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-222";
                    error.ErrorMSG = "Must be select one filter ";
                    Response.Errors.Add(error);
                }
                var AccountOfEntryList = new List<AccountOfEntry>();
                if (Response.Result)
                {

                    var AccountOfEntryDBList = _unitOfWork.VDailyJournalEntries.FindAllQueryable(x => x.Active == true);
                    if (POID != 0)
                    {
                        var IDSEntry = _unitOfWork.SupplierAccounts.FindAllAsync(x => x.PurchasePoid == POID && x.Active == true).Result.Select(x => x.DailyAdjustingEntryId).ToList();
                        AccountOfEntryDBList = AccountOfEntryDBList.Where(x => IDSEntry.Contains(x.Id));
                    }
                    if (ProjectID != 0)
                    {
                        var IDSEntry = _unitOfWork.ClientAccounts.FindAllAsync(x => x.ProjectId == ProjectID && x.Active == true).Result.Select(x => x.DailyAdjustingEntryId).ToList();
                        AccountOfEntryDBList = AccountOfEntryDBList.Where(x => IDSEntry.Contains(x.Id));
                    }
                    if (OfferID != 0)
                    {
                        var ListOfChildrentOfferIDS = _unitOfWork.InvoiceCnandDns.FindAllAsync(x => x.ParentSalesOfferId == OfferID).Result.Select(x => x.SalesOfferId).ToList();
                        var IDSProj = _unitOfWork.Projects.FindAllAsync(x => x.SalesOfferId == OfferID || ListOfChildrentOfferIDS.Contains(x.SalesOfferId)).Result.Select(x => x.Id).ToList();
                        var IDSEntry = _unitOfWork.ClientAccounts.FindAllAsync(x => (x.ProjectId != null ? IDSProj.Contains((long)x.ProjectId) : false) && x.Active == true).Result.Select(x => x.DailyAdjustingEntryId).ToList();
                        AccountOfEntryDBList = AccountOfEntryDBList.Where(x => IDSEntry.Contains(x.Id));
                    }

                    var ListAsync = await AccountOfEntryDBList.ToListAsync();

                    if (AccountOfEntryDBList.Count() > 0)
                    {
                        foreach (var entry in ListAsync)
                        {

                            long? ResponsePOID = null;
                            long? ResponseOfferID = null;
                            long? ResponseProjectID = null;
                            decimal Amount = 0;
                            string AccountName = "";
                            var OfferIdWithProject = Common.GetOfferIDFromEntryID(entry.Id, _Context);
                            ResponseOfferID = OfferIdWithProject.Item2;
                            ResponseProjectID = OfferIdWithProject.Item1;
                            if (POID != 0)
                            {
                                var EntryObjDB = _unitOfWork.SupplierAccounts.FindAllAsync(x => x.DailyAdjustingEntryId == entry.Id && x.PurchasePoid == POID && x.Active == true).Result.FirstOrDefault();
                                if (EntryObjDB != null)
                                {
                                    Amount = EntryObjDB.Amount;
                                    AccountName = EntryObjDB.Account.AccountName;
                                    ResponsePOID = POID;
                                }
                            }
                            if (ProjectID != 0)
                            {
                                var EntryObjDB = _unitOfWork.ClientAccounts.FindAllAsync(x => x.DailyAdjustingEntryId == entry.Id && x.ProjectId == ProjectID && x.Active == true).Result.FirstOrDefault();
                                if (EntryObjDB != null)
                                {
                                    Amount = EntryObjDB.Amount;
                                    AccountName = EntryObjDB.Account?.AccountName;
                                    ResponseProjectID = ProjectID;
                                }
                            }
                            if (OfferID != 0)
                            {
                                //List<long> ListProjIDS = new L
                                var ListOfChildrentOfferIDS = _unitOfWork.InvoiceCnandDns.FindAllAsync(x => x.ParentSalesOfferId == OfferID).Result.Select(x => x.SalesOfferId).ToList();
                                var IDSProj = _unitOfWork.Projects.FindAllAsync(x => x.SalesOfferId == OfferID || ListOfChildrentOfferIDS.Contains(x.SalesOfferId)).Result.Select(x => x.Id).ToList();
                                var EntryObjDB = _unitOfWork.ClientAccounts.FindAllAsync(x => x.DailyAdjustingEntryId == entry.Id && (x.ProjectId != null ? IDSProj.Contains((long)x.ProjectId) : true) && x.Active == true).Result.FirstOrDefault();
                                if (EntryObjDB != null)
                                {
                                    Amount = EntryObjDB.Amount;
                                    AccountName = EntryObjDB.Account?.AccountName;
                                }
                            }
                            var Obj = new AccountOfEntry();
                            Obj.entryId = entry.Id;
                            Obj.POId = ResponsePOID;
                            Obj.offerId = ResponseOfferID;
                            Obj.projectId = ResponseProjectID;
                            Obj.entrySerial = entry.Serial;
                            Obj.accountName = AccountName;
                            Obj.amount = Amount;


                            AccountOfEntryList.Add(Obj);
                        }
                    }
                }
                Response.AccountOfEntryList = AccountOfEntryList;
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

        public List<long> GetParentIDSAccounts(List<VAccount> AccountList, long AccountID, List<long> IDSAcc)
        {
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

        public async Task<AccountTreeResponse> GetAccountsTree([FromHeader] bool CalcWithoutPrivate, [FromHeader] bool OrderByCreationDate , [FromHeader] long AccountID, [FromHeader] long AccountCategoryID, [FromHeader] string FromDate, [FromHeader] string DateTo)
        {

            AccountTreeResponse response = new AccountTreeResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                
                DateTime CurrentEndYear = new DateTime(DateTime.Now.Year + 1, 1, 1);
                if (!string.IsNullOrEmpty(DateTo) && DateTime.TryParse(DateTo, out CurrentEndYear))
                {
                    CurrentEndYear = DateTime.Parse(DateTo);
                }
                var StartYear = new DateTime(2000, 1, 1);
                if(!string.IsNullOrEmpty(FromDate) && DateTime.TryParse(FromDate, out StartYear))
                {
                    StartYear = DateTime.Parse(FromDate);
                }
                if (response.Result)
                {
                    var AccountTreeList = _unitOfWork.VAccounts.GetAll().OrderBy(x => x.AccountNumber).ToList();
                    if (AccountCategoryID != 0)
                    {
                        AccountTreeList = AccountTreeList.Where(x => x.AccountCategoryId == AccountCategoryID).OrderBy(x => x.AccountNumber).ToList();
                    }
                    if (AccountID != 0)
                    {
                        var IDSParentAcc = GetParentIDSAccounts(AccountTreeList, AccountID, new List<long>());

                        AccountTreeList = AccountTreeList.Where(x => IDSParentAcc.Contains(x.Id)).OrderBy(x => x.AccountNumber).ToList();
                    }
                    var ParentAccountList = AccountTreeList.Where(x => x.ParentCategory == 0).ToList();
                    //var AccountMovementList = _Context.STP_AccountMovement(CalcWithoutPrivate, OrderByCreationDate).ToList();
                    //var AccountMovementListaaa = AccountMovementList.Where(x=>x.ID == 227).ToList();
                    //-------------check if account has AssetDepreciation-----------------------------
                    var accountsIDsList = ParentAccountList.Select(a => a.Id).ToList();
                    var assetDepreciation = _unitOfWork.AssetDepreciations.FindAll(a => accountsIDsList.Contains(a.AccountId)).ToList();

                    //--------------------------------------------------------------------------------

                    
                    var TreeDtoObj = ParentAccountList.Select(c => new TreeViewAccount
                    {
                        id = c.Id.ToString(),
                        title = c.AccountName,
                        parentId = "",
                        HasChild = c.Haveitem,
                        Code = c.AccountNumber,
                        Category = c.AccountCategoryName,
                        Type = c.AccountTypeName,
                        Credit = 0, //AccountMovementList.Where(x => x.ID == c.ID).Select(x => x.Credit).LastOrDefault(), //c.Credit,
                        Debit = 0, //AccountMovementList.Where(x => x.ID == c.ID).Select(x => x.Debit).LastOrDefault(), // c.Debit,
                        Accumulative = 0, // AccountMovementList.Where(x => x.ID == c.ID).Select(x => x.Acc_Calc).LastOrDefault(), // c.Accumulative,
                        Currency = c.CurrencyName,
                        Active = c.Active,
                        AdvanciedTypeId = c.AdvanciedTypeId,
                        AdvanciedTypeName = c.AdvanciedTypeName,
                        DataLevel = c.DataLevel,
                        HasDuplicate = assetDepreciation.Where(a => a.AccountId == c.Id).FirstOrDefault() == null ? false : true
                    }).Distinct().ToList();



                    var AccountSubCategory = AccountTreeList.Where(x => x.ParentCategory != 0).ToList();



                    var modelsDto = AccountSubCategory.Select((item, index) =>
                    {
                        bool IsCredit = item.Credit != 0;
                        var AccountID = new SqlParameter("AccountID", System.Data.SqlDbType.BigInt);
                        AccountID.Value = item.Id;
                        var CalcWithoutPrivateJE = new SqlParameter("CalcWithoutPrivateJE", System.Data.SqlDbType.Bit);
                        CalcWithoutPrivateJE.Value = CalcWithoutPrivate;
                        var orderByCreationDate = new SqlParameter("OrderByCreationDate", System.Data.SqlDbType.Bit);
                        orderByCreationDate.Value = OrderByCreationDate;
                        var CreationDateFrom = new SqlParameter("CreationDateFrom", System.Data.SqlDbType.DateTime);
                        CreationDateFrom.Value = StartYear;
                        var CreationDateTo = new SqlParameter("CreationDateTo ", System.Data.SqlDbType.DateTime);
                        CreationDateTo.Value = CurrentEndYear;
                        object[] param = new object[] { AccountID, CalcWithoutPrivateJE, orderByCreationDate, CreationDateFrom, CreationDateTo };
                        var AccountMovementList = _Context.Database.SqlQueryRaw<STP_AccountMovement_Result>("Exec STP_AccountMovement @AccountID ,@CalcWithoutPrivateJE ,@OrderByCreationDate ,@CreationDateFrom ,@CreationDateTo", param).AsEnumerable().ToList();
                        return new TreeViewAccount
                        {
                            id = item.Id.ToString(),
                            title = item.AccountName,
                            parentId = item.ParentCategory.ToString(),
                            HasChild = item.Haveitem,
                            Code = item.AccountNumber,
                            Category = item.AccountCategoryName,
                            Type = item.AccountTypeName,
                            Credit = AccountMovementList.Select(x => x.Credit).Sum(), // c.Debit,
                            Debit = AccountMovementList.Select(x => x.Debit).Sum(), // c.Debit,
                            Accumulative = AccountMovementList.Select(x => x.Acc_Calc).LastOrDefault() ?? 0, // c.Accumulative,
                            Currency = item.CurrencyName,
                            Active = item.Active,
                            AdvanciedTypeId = item.AdvanciedTypeId,
                            AdvanciedTypeName = item.AdvanciedTypeName,
                            DataLevel = item.DataLevel
                        };
                    }).Distinct().ToList();




                    //var modelsDto = AccountSubCategory.Select(c => new TreeViewAccount
                    //{
                    //    id = c.ID.ToString(),
                    //    title = c.AccountName,
                    //    parentId = c.ParentCategory.ToString(),
                    //    HasChild = c.Haveitem,
                    //    Code = c.AccountNumber,
                    //    Category = c.AccountCategoryName,
                    //    Type = c.AccountTypeName,
                    //    //    AccumulativePerMonth = for (int i = 1; i <= 12; i++)
                    //    //{
                    //    //    new List<AccumulativePerMonth>().Add(new AccumulativePerMonth {
                    //    //        credit = _Context.STP_AccountMovement(c.ID, CalcWithoutPrivate, OrderByCreationDate, StartYear, CurrentEndYear).Select(x => x.Credit).Sum() 
                    //    //    });
                    //    //},
                    //    Credit = _Context.STP_AccountMovement(c.ID, CalcWithoutPrivate, OrderByCreationDate, StartYear, CurrentEndYear).Select(x => x.Credit).Sum(), // c.Debit,
                    //    //AccumulativePerMonth = _Context.STP_AccountMovement(c.ID, CalcWithoutPrivate, OrderByCreationDate, StartYear, CurrentEndYear)
                    //    //                 .GroupBy(a => months.FirstOrDefault(m => m == a.CreationDate.Month))
                    //    //                 .Select(x => new AccumulativePerMonth { month = x.Key, credit = x.Sum(y => y.Credit), Debit = x.Sum(y => y.Debit), Accumulative = x.Sum(y => y.Acc_Calc ?? 0) })
                    //    //                 .OrderBy(m => m.month).ToList(), //c.Credit,
                    //    //CreditPerMonth = months.Select(item =>  new CreditPerMonth 
                    //    //{ month = item , credit = 
                    //    //_Context.STP_AccountMovement(c.ID, CalcWithoutPrivate, OrderByCreationDate, StartYear, CurrentEndYear)
                    //    //.Where(a => a.CreationDate.Month == item).Sum(x=>x.Credit) }).ToList(),
                    //    Debit = _Context.STP_AccountMovement(c.ID, CalcWithoutPrivate, OrderByCreationDate, StartYear, CurrentEndYear).Select(x => x.Debit).Sum(), // c.Debit,
                    //    Accumulative = _Context.STP_AccountMovement(c.ID, CalcWithoutPrivate, OrderByCreationDate, StartYear, CurrentEndYear).Select(x => x.Acc_Calc).LastOrDefault() ?? 0, // c.Accumulative,
                    //    Currency = c.CurrencyName,
                    //    Active = c.Active,
                    //    AdvanciedTypeId = c.AdvanciedTypeID,
                    //    AdvanciedTypeName = c.AdvanciedTypeName,
                    //    DataLevel = c.DataLevel
                    //}).Distinct().ToList();


                    TreeDtoObj.AddRange(modelsDto);

                    var trees = Common.BuildTreeViews("", TreeDtoObj);
                    response.GetAccountTreeList = trees;
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

        public async Task<AccountMovementResponse> GetAccountsMovement([FromHeader] GetAccountsMovementHeader header)
        {
            AccountMovementResponse response = new AccountMovementResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var GetAccountMovementList = new List<AccountOfMovement>();

                if (response.Result)
                {
                    if (header.AccountID == null || header.AccountID == "")
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid AccountID";
                        response.Errors.Add(error);
                        return response;
                    }
                    DateTime FromDateTemp = new DateTime(DateTime.Now.Year - 20, 1, 1);
                    if (!string.IsNullOrEmpty(header.FromDate) && DateTime.TryParse(header.FromDate, out FromDateTemp))
                    {
                        FromDateTemp = DateTime.Parse(header.FromDate);
                    }
                    DateTime ToDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(header.DateTo) && DateTime.TryParse(header.DateTo, out ToDateTemp))
                    {
                        ToDateTemp = DateTime.Parse(header.DateTo);
                    }
                    GetAccountMovementList = Common.GetAccountMovementList_WithListAccountIds(header.AccountID, header.CalcWithoutPrivate, header.OrderByCreationDate, FromDateTemp, ToDateTemp, header.ClientId, header.SupplierId, _Context);

                    var AccumulativePerAccountList = GetAccountMovementList.GroupBy(x => x.AccountID).Select(item => new AccumulativePerAccount { AccountId = item.Key, Accumulative = item.LastOrDefault().Accumulative }).ToList();

                    response.GetAccountMovementList = GetAccountMovementList;
                    response.AccumulativePerAccountList = AccumulativePerAccountList;
                    response.DateFrom = FromDateTemp.ToShortDateString();
                    response.DateTo = ToDateTemp.ToShortDateString();
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

        public async Task<BaseResponseWithID> AddNewAccount(AddNewAccountRequest request, long creator)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    long ParentCategoryId = 0;
                    long? AccountCategoryId = null;
                    int? CurrencyId = null;
                    string AccountType = "";

                    var AccountListQuerable = _unitOfWork.Accounts.FindAllQueryable(x => x.Active == true);
                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (string.IsNullOrEmpty(request.AccountName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err511";
                        error.ErrorMSG = "Account Name is required.";
                        Response.Errors.Add(error);
                    }
                    if (request.HaveChild != true && request.HaveChild != false)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err511";
                        error.ErrorMSG = "Select Have Child is required.";
                        Response.Errors.Add(error);
                    }
                    if (request.Active != true && request.Active != false)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err511";
                        error.ErrorMSG = "Select Active is required.";
                        Response.Errors.Add(error);
                    }
                    if (request.HaveAdvancedSetting != true && request.HaveAdvancedSetting != false)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err511";
                        error.ErrorMSG = "Select Have Advanced setting is required.";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        if (request.HaveChild != false && request.HaveAdvancedSetting == true)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err511";
                            error.ErrorMSG = "Cannot Select Advanced setting because you select have child.";
                            Response.Errors.Add(error);
                        }
                    }
                    if (request.ParentCategoryId != null && request.ParentCategoryId != 0)
                    {
                        ParentCategoryId = (long)request.ParentCategoryId;
                        var CheckParentCategoryIsExist = await AccountListQuerable.Where(x => x.Id == ParentCategoryId).FirstOrDefaultAsync();
                        if (CheckParentCategoryIsExist == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err512";
                            error.ErrorMSG = "The Parent Category Id selected not exist.";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            AccountCategoryId = CheckParentCategoryIsExist.AccountCategoryId;
                            //CurrencyId = (int)CheckParentCategoryIsExist.CurrencyID;
                            AccountType = CheckParentCategoryIsExist.AccountTypeName;
                        }
                    }
                    else
                    {
                        // If Not selected Parent Category
                        if (request.AccountCategoryId != null && request.AccountCategoryId != 0)
                        {
                            AccountCategoryId = (long)request.AccountCategoryId;
                            var CheckAccountCategoryIsExist = _unitOfWork.AccountCategories.GetById((long)AccountCategoryId);
                            if (CheckAccountCategoryIsExist == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err512";
                                error.ErrorMSG = "The Account Category Id selected not exist.";
                                Response.Errors.Add(error);
                            }
                        }

                        if (string.IsNullOrEmpty(request.AccountType) || (request.AccountType != "Debit" && request.AccountType != "Credit"))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err512";
                            error.ErrorMSG = "Invalid Account Type";
                            Response.Errors.Add(error);
                        }

                    }
                    if (request.CurrencyId != null && request.CurrencyId != 0)
                    {
                        CurrencyId = (int)request.CurrencyId;
                        var CheckCurrencyIsExist = _unitOfWork.Currencies.FindAllAsync(x => x.Id == CurrencyId).Result.FirstOrDefault();
                        if (CheckCurrencyIsExist == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err512";
                            error.ErrorMSG = "The Account Category Id selected not exist.";
                            Response.Errors.Add(error);
                        }
                    }
                    // Check Validation if select Have Advanced Setting 

                    if (request.HaveAdvancedSetting == true)
                    {
                        if (request.AdvanciedTypeID == null || request.AdvanciedTypeID <= 0)
                        {

                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err511";
                            error.ErrorMSG = "Invalid Advanced Type ID.";
                            Response.Errors.Add(error);
                        }

                    }

                    if (Response.Result)
                    {
                        string newOrderStr = "";
                        int newOrder = 1;
                        int newDataLevel = 1;

                        if (ParentCategoryId != 0)
                        {
                            newOrder = await AccountListQuerable.Where(x => x.ParentCategory == ParentCategoryId).CountAsync() + 1;
                        }


                        if (newOrder < 10)
                        {
                            newOrderStr = "0" + newOrder.ToString();
                        }
                        else
                        {
                            newOrderStr = newOrder.ToString();
                        }



                        Account AccountDB = new Account();
                        AccountDB.AccountName = request.AccountName;
                        AccountDB.AccountTypeName = AccountType;
                        AccountDB.ParentCategory = ParentCategoryId;
                        AccountDB.AccountOrder = newOrder;
                        AccountDB.CurrencyId = CurrencyId;
                        AccountDB.Description = request.Description ?? request.AccountName;
                        AccountDB.Active = (bool)request.Active;
                        AccountDB.Haveitem = (bool)request.HaveChild;
                        AccountDB.Accumulative = 0;
                        AccountDB.Credit = 0;
                        AccountDB.Debit = 0;
                        AccountDB.Havetax = false;
                        AccountDB.CreationDate = DateTime.Now;
                        AccountDB.CreatedBy = creator;
                        AccountDB.ModifiedBy = creator;
                        AccountDB.ModifiedDate = DateTime.Now;
                        AccountDB.AccountCategoryId = (long)AccountCategoryId;
                        AccountDB.Comment = "";
                        AccountDB.AdvanciedSettingsStatus = request.HaveAdvancedSetting;
                        AccountDB.TranactionStatus = false;

                        if (ParentCategoryId == 0)
                        {
                            AccountDB.DataLevel = newDataLevel;
                            AccountDB.AccountNumber = newOrderStr;

                        }
                        else
                        {
                            var ParentAccountObjDB = await AccountListQuerable.Where(x => x.Id == ParentCategoryId).FirstOrDefaultAsync();
                            if (ParentAccountObjDB != null)
                            {
                                AccountDB.DataLevel = ParentAccountObjDB.DataLevel + 1;
                                AccountDB.AccountNumber = ParentAccountObjDB.AccountNumber + "-" + newOrderStr;

                            }
                        }

                        _unitOfWork.Accounts.Add(AccountDB);
                        var ResAccount = _unitOfWork.Complete();
                        if (ResAccount > 0)
                        {
                            if (request.HaveChild == false && request.HaveAdvancedSetting == true)
                            {
                                if (request.AdvancedSettingKeepersList != null && request.AdvancedSettingKeepersList.Count() > 0)
                                {
                                    foreach (var KeeperID in request.AdvancedSettingKeepersList)
                                    {
                                        // Check If Keeper ID is User Or not
                                        var KeeperName = Common.CheckUserIsExist(KeeperID, _Context);
                                        if (!string.IsNullOrEmpty(KeeperName))
                                        {
                                            var advanciedSettingAccount = new AdvanciedSettingAccount();

                                            advanciedSettingAccount.AccountId = AccountDB.Id;
                                            advanciedSettingAccount.AdvanciedTypeId = (long)request.AdvanciedTypeID;
                                            advanciedSettingAccount.Name = request.AdvancedSettingName;
                                            advanciedSettingAccount.Location = request.AdvancedSettingLocation;
                                            advanciedSettingAccount.Description = request.AdvancedSettingDescription;
                                            advanciedSettingAccount.KeeperId = KeeperID;
                                            advanciedSettingAccount.KeeperName = KeeperName;
                                            advanciedSettingAccount.CreatedBy = creator;
                                            advanciedSettingAccount.CreationDate = DateTime.Now;
                                            advanciedSettingAccount.ModifiedBy = creator;
                                            advanciedSettingAccount.ModifiedDate = DateTime.Now;
                                            advanciedSettingAccount.Active = true;


                                            _unitOfWork.AdvanciedSettingAccounts.Add(advanciedSettingAccount);
                                            _unitOfWork.Complete();

                                        }
                                    }
                                }
                                else
                                {
                                    var advanciedSettingAccount = new AdvanciedSettingAccount();

                                    advanciedSettingAccount.AccountId = AccountDB.Id;
                                    advanciedSettingAccount.AdvanciedTypeId = (long)request.AdvanciedTypeID;
                                    advanciedSettingAccount.Name = request.AdvancedSettingName;
                                    advanciedSettingAccount.Location = request.AdvancedSettingLocation;
                                    advanciedSettingAccount.Description = request.AdvancedSettingDescription;
                                    advanciedSettingAccount.KeeperId = null;
                                    advanciedSettingAccount.KeeperName = null;
                                    advanciedSettingAccount.CreatedBy = creator;
                                    advanciedSettingAccount.CreationDate = DateTime.Now;
                                    advanciedSettingAccount.ModifiedBy = creator;
                                    advanciedSettingAccount.ModifiedDate = DateTime.Now;
                                    advanciedSettingAccount.Active = true;


                                    _unitOfWork.AdvanciedSettingAccounts.Add(advanciedSettingAccount);
                                    _unitOfWork.Complete();

                                }
                            }
                        }

                        Response.ID = AccountDB.Id;
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

        public async Task<BaseResponseWithID> EditAccount(AddNewAccountRequest request, long creator)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var AccountListQuerable = _unitOfWork.Accounts.FindAllQueryable(x => x.Active == true);
                    Account AccountObjDB = null;
                    int CheckChildrenAccountsCount = 0;
                    VAccountOfJournalEntryWithDaily CheckAccountOfJEObjDB = null;

                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.ID != null && request.ID != 0)
                    {
                        AccountObjDB = await AccountListQuerable.Where(x => x.Id == request.ID).FirstOrDefaultAsync();
                        if (AccountObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err511";
                            error.ErrorMSG = "Invalid Account ID.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        else
                        {
                            CheckAccountOfJEObjDB = _unitOfWork.VAccountOfJournalEntryWithDailies.FindAll(x => x.AccountId == request.ID).FirstOrDefault();
                            CheckChildrenAccountsCount = AccountListQuerable.Where(x => x.ParentCategory == request.ID).Count();
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err511";
                        error.ErrorMSG = "Invalid ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (string.IsNullOrEmpty(request.AccountName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err511";
                        error.ErrorMSG = "Account Name is required.";
                        Response.Errors.Add(error);
                    }
                    var CheckIfHaveAdvanciedSettingListDB = _unitOfWork.AdvanciedSettingAccounts.FindAll(x => x.AccountId == AccountObjDB.Id).ToList();
                    if (request.HaveChild != null)
                    {
                        if (CheckAccountOfJEObjDB != null && ((AccountObjDB.Haveitem == false && request.HaveChild == true) || (AccountObjDB.Haveitem == true && request.HaveChild == false)))// Check  if not have JE 
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err511";
                            error.ErrorMSG = "Invalid to change  Have Child , because already have Journal entry";
                            Response.Errors.Add(error);
                        }
                        else if (AccountObjDB.Haveitem == true && request.HaveChild == false) // Check if have Children Accounts 
                        {

                            if (CheckChildrenAccountsCount > 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err511";
                                error.ErrorMSG = "Can't change HaveChild to false becasue already have Account list children";
                                Response.Errors.Add(error);
                            }
                        }
                        else if ((AccountObjDB.Haveitem == false && request.HaveChild == true) && CheckIfHaveAdvanciedSettingListDB.Count() > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err511";
                            error.ErrorMSG = "Can't change HaveChild to true becasue this Account have advanced setting ";
                            Response.Errors.Add(error);
                        }
                    }
                    if (request.Active != null)
                    {
                        if (AccountObjDB.Active == true && request.Active == false)
                        {
                            if (CheckAccountOfJEObjDB != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err511";
                                error.ErrorMSG = "Can't deactivate this Account becasue already have Journal entry";
                                Response.Errors.Add(error);
                            }
                            if (CheckChildrenAccountsCount > 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err511";
                                error.ErrorMSG = "Can't deactivate this Account becasue already have Account list children";
                                Response.Errors.Add(error);
                            }
                        }
                    }

                    if (AccountObjDB.AdvanciedSettingsStatus != true && request.HaveAdvancedSetting == true)
                    {
                        // Change and set Have Advanced setting
                        if (CheckChildrenAccountsCount > 0 || request.HaveChild == true)
                        {

                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err511";
                            error.ErrorMSG = "Cannot Select Advanced setting because you select have child.";
                            Response.Errors.Add(error);
                        }

                        if (request.AdvanciedTypeID == null || request.AdvanciedTypeID <= 0)
                        {

                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err511";
                            error.ErrorMSG = "Invalid Advanced Type ID.";
                            Response.Errors.Add(error);
                        }
                    }


                    if (Response.Result)
                    {

                        AccountObjDB.AccountName = request.AccountName;
                        AccountObjDB.Description = request.Description ?? request.AccountName;
                        if (request.Active != null)
                        {
                            AccountObjDB.Active = (bool)request.Active;
                        }
                        if (request.HaveChild != null)
                        {
                            AccountObjDB.Haveitem = (bool)request.HaveChild;
                        }
                        AccountObjDB.ModifiedBy = creator;
                        AccountObjDB.ModifiedDate = DateTime.Now;
                        AccountObjDB.AdvanciedSettingsStatus = request.HaveAdvancedSetting;
                        AccountObjDB.TranactionStatus = false;
                        AccountObjDB.CurrencyId = request.CurrencyId;


                        var ResAccount = await _Context.SaveChangesAsync();
                        //if (ResAccount > 0)
                        //{
                        if (request.HaveChild == false && request.HaveAdvancedSetting == true && request.AdvanciedTypeID != null)
                        {

                            if (request.AdvancedSettingKeepersList != null && request.AdvancedSettingKeepersList.Count() > 0)
                            {

                                // delete all old and insert again
                                _unitOfWork.AdvanciedSettingAccounts.DeleteRange(CheckIfHaveAdvanciedSettingListDB);
                                foreach (var KeeperID in request.AdvancedSettingKeepersList)
                                {
                                    // Check If Keeper ID is User Or not
                                    var KeeperName = Common.CheckUserIsExist(KeeperID, _Context);
                                    if (!string.IsNullOrEmpty(KeeperName))
                                    {
                                        var advanciedSettingAccount = new AdvanciedSettingAccount();

                                        advanciedSettingAccount.AccountId = AccountObjDB.Id;
                                        advanciedSettingAccount.AdvanciedTypeId = (long)request.AdvanciedTypeID;
                                        advanciedSettingAccount.Name = request.AdvancedSettingName;
                                        advanciedSettingAccount.Location = request.AdvancedSettingLocation;
                                        advanciedSettingAccount.Description = request.AdvancedSettingDescription;
                                        advanciedSettingAccount.KeeperId = KeeperID;
                                        advanciedSettingAccount.KeeperName = KeeperName;
                                        advanciedSettingAccount.CreatedBy = creator;
                                        advanciedSettingAccount.CreationDate = DateTime.Now;
                                        advanciedSettingAccount.ModifiedBy = creator;
                                        advanciedSettingAccount.ModifiedDate = DateTime.Now;
                                        advanciedSettingAccount.Active = true;


                                        _unitOfWork.AdvanciedSettingAccounts.Add(advanciedSettingAccount);
                                        _unitOfWork.Complete();

                                    }
                                }
                            }
                            else
                            {

                                var advanciedSettingAccount = new AdvanciedSettingAccount();
                                if (CheckIfHaveAdvanciedSettingListDB.Count() > 0)
                                {
                                    advanciedSettingAccount = CheckIfHaveAdvanciedSettingListDB.FirstOrDefault();
                                    advanciedSettingAccount.AdvanciedTypeId = (long)request.AdvanciedTypeID;
                                    advanciedSettingAccount.Name = request.AdvancedSettingName;
                                    advanciedSettingAccount.Location = request.AdvancedSettingLocation;
                                    advanciedSettingAccount.Description = request.AdvancedSettingDescription;
                                    advanciedSettingAccount.KeeperId = null;
                                    advanciedSettingAccount.KeeperName = null;
                                    advanciedSettingAccount.ModifiedBy = creator;
                                    advanciedSettingAccount.ModifiedDate = DateTime.Now;
                                    advanciedSettingAccount.Active = true;
                                }
                                else
                                {

                                    advanciedSettingAccount.AccountId = AccountObjDB.Id;
                                    advanciedSettingAccount.AdvanciedTypeId = (long)request.AdvanciedTypeID;
                                    advanciedSettingAccount.Name = request.AdvancedSettingName;
                                    advanciedSettingAccount.Location = request.AdvancedSettingLocation;
                                    advanciedSettingAccount.Description = request.AdvancedSettingDescription;
                                    advanciedSettingAccount.KeeperId = null;
                                    advanciedSettingAccount.KeeperName = null;
                                    advanciedSettingAccount.CreatedBy = creator;
                                    advanciedSettingAccount.CreationDate = DateTime.Now;
                                    advanciedSettingAccount.ModifiedBy = creator;
                                    advanciedSettingAccount.ModifiedDate = DateTime.Now;
                                    advanciedSettingAccount.Active = true;


                                    _unitOfWork.AdvanciedSettingAccounts.Add(advanciedSettingAccount);
                                }

                                _unitOfWork.Complete();
                            }
                        }
                        else
                        {
                            // delete all old and insert again
                            _unitOfWork.AdvanciedSettingAccounts.DeleteRange(CheckIfHaveAdvanciedSettingListDB);
                            _unitOfWork.Complete();
                        }
                        //}

                        Response.ID = AccountObjDB.Id;
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

        public async Task<AccountInfoResponse> GetAccountInfo([FromHeader] long AccountID = 0)
        {
            AccountInfoResponse Response = new AccountInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (AccountID == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "Invalid AccountID";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (Response.Result)
                {
                    var AccountDB = _unitOfWork.Accounts.FindAllAsync(x => x.Id == AccountID).Result.FirstOrDefault();

                    if (AccountDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This AccountID is not exist";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    AccountInfoModel AccountInfoModelObj = new AccountInfoModel();
                    AccountInfoModelObj.ID = AccountDB.Id;
                    AccountInfoModelObj.AccountName = AccountDB.AccountName;
                    AccountInfoModelObj.AccountNumber = AccountDB.AccountNumber;
                    AccountInfoModelObj.DataLevel = AccountDB.DataLevel;
                    AccountInfoModelObj.AccountOrder = AccountDB.AccountOrder;
                    AccountInfoModelObj.AccountCategoryId = AccountDB.AccountCategoryId;
                    AccountInfoModelObj.AccountCategoryName = AccountDB.AccountCategory?.AccountCategoryName;
                    AccountInfoModelObj.AccountType = AccountDB.AccountTypeName;
                    AccountInfoModelObj.Active = AccountDB.Active;
                    AccountInfoModelObj.CurrencyId = AccountDB.CurrencyId;
                    AccountInfoModelObj.CurrencyName = AccountDB.Currency?.Name;
                    AccountInfoModelObj.Description = AccountDB.Description;
                    AccountInfoModelObj.HaveChild = AccountDB.Haveitem;
                    AccountInfoModelObj.ParentCategoryId = AccountDB.ParentCategory;
                    if (AccountDB.ParentCategory != null)
                    {
                        AccountInfoModelObj.ParentCategoryName = Common.GetAccountName((long)AccountDB.ParentCategory, _Context);
                    }
                    AccountInfoModelObj.HaveAdvancedSetting = AccountDB.AdvanciedSettingsStatus;

                    var AdvancedSettingListDB = _unitOfWork.AdvanciedSettingAccounts.FindAllAsync(x => x.AccountId == AccountDB.Id).Result.ToList();
                    var AdvancedSettingObjDB = AdvancedSettingListDB.FirstOrDefault();
                    if (AdvancedSettingObjDB != null)
                    {
                        AccountInfoModelObj.AdvanciedTypeID = AdvancedSettingObjDB.AdvanciedTypeId;
                        AccountInfoModelObj.AdvancedSettingDescription = AdvancedSettingObjDB.Description;
                        AccountInfoModelObj.AdvancedSettingLocation = AdvancedSettingObjDB.Location;
                        AccountInfoModelObj.AdvancedSettingName = AdvancedSettingObjDB.Name;
                    }
                    AccountInfoModelObj.AdvancedSettingKeepersList = AdvancedSettingListDB.Where(x => x.KeeperId != null).Select(x => x.KeeperId ?? 0).ToList();


                    Response.AccountInfoModel = AccountInfoModelObj;
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

        public BaseResponseWithId DeleteAccount([FromHeader] long AccountId = 0)
        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (AccountId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Please provide an Account Id";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (AccountId != 0)
                    {
                        var account = _unitOfWork.Accounts.GetById(AccountId);
                        if (account != null)
                        {
                            if (!account.Haveitem)
                            {
                                var transactions = _unitOfWork.AccountOfJournalEntries.FindAllQueryable(a => a.AccountId == AccountId);
                                if (transactions.Count() > 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err101";
                                    error.ErrorMSG = "Can't Delete Account due to found transactions";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                                else
                                {
                                    var parent = _unitOfWork.Accounts.FindAll(a => a.Id == account.ParentCategory && a.Haveitem).FirstOrDefault();
                                    if (parent != null)
                                    {
                                        var advancedTypes = _unitOfWork.AdvanciedSettingAccounts.FindAllQueryable(a => a.AccountId == AccountId);
                                        foreach (var type in advancedTypes)
                                        {
                                            _unitOfWork.AdvanciedSettingAccounts.Delete(type);
                                        }



                                        _unitOfWork.Accounts.Delete(account);
                                        _unitOfWork.Complete();

                                        decimal sumBalanceOfParent = 0;
                                        decimal sumCreditOfParent = 0;
                                        decimal sumDebitOfParent = 0;
                                        var childs = _unitOfWork.Accounts.FindAll(a => a.ParentCategory == parent.Id).ToList();
                                        foreach (var child in childs)
                                        {
                                            sumBalanceOfParent += child.Accumulative;
                                            sumCreditOfParent += child.Credit;
                                            sumDebitOfParent += child.Debit;
                                        }
                                        parent.Accumulative = sumBalanceOfParent;
                                        parent.Credit = sumCreditOfParent;
                                        parent.Debit = sumDebitOfParent;
                                        _unitOfWork.Complete();
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err101";
                                        error.ErrorMSG = "Account Doesn't Have A parent";
                                        Response.Errors.Add(error);
                                        return Response;
                                    }


                                }

                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err101";
                                error.ErrorMSG = "This Account is A parent";
                                Response.Errors.Add(error);
                                return Response;
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "Account is not found";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
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

        public async Task<BaseResponseWithId> AddAndEditFinancialPeriod(FinancialPeriodAccountRequest request, long creator)
        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    DateTime? StartDate = null;
                    DateTime StartDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(request.StartDate) && DateTime.TryParse(request.StartDate, out StartDateTemp))
                    {
                        StartDateTemp = DateTime.Parse(request.StartDate);
                        StartDate = StartDateTemp;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err511";
                        error.ErrorMSG = "Start Date is required.";
                        Response.Errors.Add(error);
                    }

                    DateTime? EndDate = null;
                    DateTime EndDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(request.EndDate) && DateTime.TryParse(request.EndDate, out EndDateTemp))
                    {
                        EndDateTemp = DateTime.Parse(request.EndDate);
                        EndDate = EndDateTemp;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err511";
                        error.ErrorMSG = "End Date is required.";
                        Response.Errors.Add(error);
                    }

                    if (request.IsCurrent != null)
                    {
                        // check if There another Current Financial Period
                        if (request.IsCurrent == true)
                        {
                            var CheckFinancialPeriodDB = _unitOfWork.AccountFinancialPeriods.FindAllAsync(x => x.IsCurrent == true &&
                                                                ((request.ID != null && request.ID != 0) ? x.Id != request.ID : true)
                                                                ).Result.FirstOrDefault();
                            if (CheckFinancialPeriodDB != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err511";
                                error.ErrorMSG = "can't Select This Period is current , because there is another period is current .";
                                Response.Errors.Add(error);
                            }
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err511";
                        error.ErrorMSG = "IsCurrent is required.";
                        Response.Errors.Add(error);
                    }
                    if (Response.Result)
                    {
                        if (request.ID == null || request.ID == 0)
                        {

                            var FinancialPeriodObjDB = new AccountFinancialPeriod();
                            FinancialPeriodObjDB.StartDate = (DateTime)StartDate;
                            FinancialPeriodObjDB.EndDate = (DateTime)EndDate;
                            FinancialPeriodObjDB.Description = request.Description;
                            FinancialPeriodObjDB.IsCurrent = (bool)request.IsCurrent;
                            FinancialPeriodObjDB.IsClosed = false;
                            FinancialPeriodObjDB.CreatedBy = creator;
                            FinancialPeriodObjDB.CreationDate = DateTime.Now;
                            FinancialPeriodObjDB.ModifiedBy = creator;
                            FinancialPeriodObjDB.ModificationDate = DateTime.Now;

                            _unitOfWork.AccountFinancialPeriods.Add(FinancialPeriodObjDB);
                        }
                        else
                        {
                            // Check ID is valid or not before update
                            var FinancialPeriodObjDB = _unitOfWork.AccountFinancialPeriods.FindAllAsync(x => x.Id == request.ID).Result.FirstOrDefault();
                            if (FinancialPeriodObjDB != null)
                            {
                                FinancialPeriodObjDB.EndDate = (DateTime)EndDate;
                                FinancialPeriodObjDB.Description = request.Description;
                                FinancialPeriodObjDB.IsCurrent = (bool)request.IsCurrent;
                                if (request.IsClosed != null)
                                {
                                    FinancialPeriodObjDB.IsClosed = (bool)request.IsClosed;
                                }
                                if (FinancialPeriodObjDB.IsCurrent == true && FinancialPeriodObjDB.IsClosed == true)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err511";
                                    error.ErrorMSG = "Can't select This Period current and closed .";
                                    Response.Errors.Add(error);
                                    return Response;

                                }
                                FinancialPeriodObjDB.ModifiedBy = creator;
                                FinancialPeriodObjDB.ModificationDate = DateTime.Now;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err511";
                                error.ErrorMSG = "Invalid ID.";
                                Response.Errors.Add(error);
                            }
                        }

                        _unitOfWork.Complete();
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

        public async Task<FinancialPeriodAccountResponse> GetFinancialPeriodList()
        {
            FinancialPeriodAccountResponse Response = new FinancialPeriodAccountResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var FinancialPeriodList = new List<FinancialPeriod>();
                if (Response.Result)
                {
                    var FinancialPeriodListDB = _unitOfWork.AccountFinancialPeriods.GetAllAsync();
                    if (FinancialPeriodListDB.Result.Count() > 0)
                    {
                        foreach (var item in FinancialPeriodListDB.Result)
                        {
                            var FinancialPeriodObj = new FinancialPeriod();
                            FinancialPeriodObj.ID = item.Id;
                            FinancialPeriodObj.StartDate = item.StartDate.ToShortDateString();
                            FinancialPeriodObj.EndDate = item.EndDate != null ? ((DateTime)item.EndDate).ToShortDateString() : null;
                            FinancialPeriodObj.IsCurrent = item.IsCurrent;
                            FinancialPeriodObj.IsClosed = item.IsClosed;
                            FinancialPeriodObj.Description = item.Description;

                            FinancialPeriodList.Add(FinancialPeriodObj);

                        }
                    }
                }
                Response.FinancialPeriodList = FinancialPeriodList;
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

        public SelectDDLResponse GetMethodTypeList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ItemList = new List<SelectDDL>();

                if (Response.Result)
                {

                    var ListDB = _unitOfWork.PurchasePaymentMethods.GetAll();
                    foreach (var item in ListDB)
                    {
                        var Obj = new SelectDDL();
                        Obj.ID = item.Id;
                        Obj.Name = item.Name;
                        ItemList.Add(Obj);
                    }

                    Response.DDLList = ItemList;
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

        public SelectDDLResponse GetIncOrExpTypeList([FromHeader] long AccountID = 0)
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
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
                    var AccountObjDB = _unitOfWork.Accounts.GetById(AccountID);
                    if (AccountObjDB != null)
                    {
                        if (AccountObjDB.AccountCategoryId == 4)
                        {
                            var IncomeListDB = _unitOfWork.IncomeTypes.FindAll(x => x.Active == true).ToList();
                            if (IncomeListDB.Count > 0)
                            {
                                foreach (var item in IncomeListDB)
                                {
                                    var Obj = new SelectDDL();
                                    Obj.ID = item.Id;
                                    Obj.Name = item.IncomeTypeName;
                                    ItemList.Add(Obj);

                                }
                            }
                        }
                        else if (AccountObjDB.AccountCategoryId == 5)
                        {
                            var ExpensesListDB = _Context.ExpensisTypes.Where(x => x.Active == true).ToList();
                            if (ExpensesListDB.Count > 0)
                            {
                                foreach (var item in ExpensesListDB)
                                {
                                    var Obj = new SelectDDL();
                                    Obj.ID = item.Id;
                                    Obj.Name = item.ExpensisTypeName;
                                    ItemList.Add(Obj);

                                }
                            }
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err506";
                        error.ErrorMSG = "Invalid AccountID";
                        Response.Errors.Add(error);
                    }


                    Response.DDLList = ItemList;
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

        public SelectDDLResponse GetBeneficiaryList()

        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ItemList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.DailyTranactionBeneficiaryToTypes.GetAll();
                    foreach (var item in ListDB)
                    {
                        var Obj = new SelectDDL();
                        Obj.ID = item.Id;
                        Obj.Name = item.BeneficiaryName;
                        ItemList.Add(Obj);
                    }

                    Response.DDLList = ItemList;
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

        public SelectDDLResponse GetCostCenterList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ItemList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.GeneralActiveCostCenters.GetAll();
                    foreach (var item in ListDB)
                    {
                        var Obj = new SelectDDL();
                        Obj.ID = item.Id;
                        Obj.Name = item.CostCenterName;
                        ItemList.Add(Obj);
                    }

                    Response.DDLList = ItemList;
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

        public SelectAdvancedTypeDDLResponse GetAdvancedTypeList()
        {
            SelectAdvancedTypeDDLResponse Response = new SelectAdvancedTypeDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ItemList = new List<SelectAdvancedTypeDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.AdvanciedTypes.FindAll(x => x.Active == true).ToList();
                    var ListIDSCategoriesList = ListDB.Select(x => x.AccountCategoryId).ToList();
                    var CategoryList = _unitOfWork.AccountCategories.FindAll(x => ListIDSCategoriesList.Contains(x.Id)).ToList();
                    foreach (var item in ListDB)
                    {
                        var Obj = new SelectAdvancedTypeDDL();
                        Obj.ID = item.Id;
                        Obj.Name = item.AdvanciedTypeName;
                        Obj.AccountCategoryId = item.AccountCategoryId;
                        Obj.AccountCategoryName = CategoryList.Where(x => x.Id == item.AccountCategoryId).Select(x => x.AccountCategoryName).FirstOrDefault();

                        ItemList.Add(Obj);
                    }

                    Response.DDLList = ItemList;
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

        public GetAdvanciedTypeResponse GetAdvancedTypeAccountSettingsList()
        {
            GetAdvanciedTypeResponse Response = new GetAdvanciedTypeResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();


            try
            {
                var AccountCategorylist = new List<GetAccountCategory>();
                if (Response.Result)
                {
                    AccountCategorylist = _unitOfWork.AccountCategories.FindAll(x => x.Active).Select(a => new GetAccountCategory()
                    {
                        Name = a.AccountCategoryName,
                        Id = a.Id,
                        AdvanciedTypeLList = _unitOfWork.AdvanciedTypes.FindAll(x => x.Active && x.AccountCategoryId == a.Id).Select(b => new GetAdvanciedType()
                        {
                            ID = b.Id,
                            Name = b.AdvanciedTypeName,
                            SettingAccounts = _unitOfWork.AdvanciedSettingAccounts.FindAll(x => x.Active && x.AdvanciedTypeId == b.Id, includes: new[] { "Account" }).Select(c => new GetAdvanciedSettingAccount() { ID = c.Id, AccountID = c.AccountId, AccountName = c.Account?.AccountName, }).ToList()
                        }).ToList()
                    }).ToList();
                    Response.AccountCategory = AccountCategorylist;
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

        public DailyJournalEntryResponse GetDailyJournalEntryList([FromHeader] long InventoryStoreID = 0, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10, [FromHeader] string ItemSerial = null, [FromHeader] string SearchKey = null)
        {
            DailyJournalEntryResponse Response = new DailyJournalEntryResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DailyJournalEntryList = new List<DailyJournalEntryView>();
                if (Response.Result)
                {
                    #region For PagedList
                    // var AllInventoryItemPriceAllList = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true).ToList();
                    var DailyJournalEntryListDB = _unitOfWork.VDailyJournalEntries.FindAllQueryable(x => x.Active == true).OrderByDescending(a => a.CreationDate).AsQueryable();
                    if (ItemSerial != null)
                    {
                        //string ItemSerial = headers["ItemSerial"];
                        DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x => x.Serial == ItemSerial);
                    }

                    if (SearchKey != null)
                    {
                        //string SearchKey = headers["SearchKey"];
                        SearchKey = HttpUtility.UrlDecode(SearchKey);

                        if ("Closed".Contains(SearchKey.ToLower()))
                        {
                            SearchKey = "true";
                        }
                        else if ("open".Contains(SearchKey.ToLower()))
                        {
                            SearchKey = "false";
                        }
                        DailyJournalEntryListDB = DailyJournalEntryListDB.Where(x =>
                                                                                    x.CreatorFirstName.ToLower().Contains(SearchKey.ToLower())
                                                                                    || x.CreatorLastName.ToLower().Contains(SearchKey.ToLower())
                                                                                    || x.Serial.ToLower().Contains(SearchKey.ToLower())
                                                                                    || x.DocumentNumber.ToLower().Contains(SearchKey.ToLower())
                                                                                    || x.Description.ToLower().Contains(SearchKey.ToLower())
                                                                                    || x.CreationDate.ToString().Contains(SearchKey.ToLower())
                                                                                    || x.EntryDate.ToString().Contains(SearchKey.ToLower())
                                                                                    || x.Closed.ToString().ToLower().Contains(SearchKey.ToLower())
                                                                                    || x.TotalAmount.ToString().ToLower().Contains(SearchKey.ToLower())
                                                                                    );

                    }
                    var DailyJournalEntryListDBPagingList = PagedList<VDailyJournalEntry>.Create(DailyJournalEntryListDB, CurrentPage, NumberOfItemsPerPage);
                    #endregion

                    if (DailyJournalEntryListDBPagingList.Count > 0)
                    {
                        foreach (var dailyTransaction in DailyJournalEntryListDBPagingList)
                        {
                            var dailyTranactionObj = new DailyJournalEntryView();
                            dailyTranactionObj.ID = dailyTransaction.Id;
                            dailyTranactionObj.Active = dailyTransaction.Active;
                            dailyTranactionObj.Serial = dailyTransaction.Serial;
                            dailyTranactionObj.DocumentNumber = dailyTransaction.DocumentNumber;
                            dailyTranactionObj.Status = dailyTransaction.Closed;
                            dailyTranactionObj.CreationDate = dailyTransaction.CreationDate.ToString("yyyy-MM-dd");
                            dailyTranactionObj.EntryDate = dailyTransaction.EntryDate.ToString("yyyy-MM-dd");
                            dailyTranactionObj.CreationUser = Common.GetUserName(dailyTransaction.CreatedBy, _Context);
                            dailyTranactionObj.AmountTranaction = dailyTransaction.TotalAmount;
                            dailyTranactionObj.Description = dailyTransaction.Description;
                            dailyTranactionObj.AmountTranaction = dailyTransaction.TotalAmount;
                            DailyJournalEntryList.Add(dailyTranactionObj);
                        }
                    }
                    Response.DailyJournalEntryList = DailyJournalEntryList;
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

        public BaseMessageResponse GetAccountsAndFinanceIncomeStatmentPDF(string companyname, [FromHeader] string Year, [FromHeader] string Month, [FromHeader] string Day)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    DateTime Today = DateTime.Now;
                    int year = Today.Year;
                    int month = 0;
                    int day = 0;
                    if (!string.IsNullOrEmpty(Year))
                    {
                        int.TryParse(Year, out year);
                    }

                    if (!string.IsNullOrEmpty(Month))
                    {
                        if (string.IsNullOrEmpty(Year))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "You must select year first";
                            Response.Errors.Add(error);
                        }
                        int.TryParse(Month, out month);
                    }

                    if (!string.IsNullOrEmpty(Day))
                    {
                        if (string.IsNullOrEmpty(Month))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "You must select Month first";
                            Response.Errors.Add(error);
                        }
                        int.TryParse(Day, out day);
                    }

                    // var IncomeAccountsAndFinanceList = new List<IncomeAccountsAndFinanceList>();
                    var ALLAccountListDB = _unitOfWork.Accounts.FindAll(x => x.AccountCategoryId == 4 || x.AccountCategoryId == 5).ToList();

                    // Fill Account and Finance Income -----
                    var AccountsAndFinanceList = new List<AccountsAndFinanceDetails>();
                    var request = LoadChildAccount(ALLAccountListDB, 4, year, month, day);
                    AccountsAndFinanceList = request.AccountsAndFinanceDetailsList;


                    var AccountAndFinanceExpensesList = new List<AccountsAndFinanceDetails>();
                    var RequestExpenses = LoadChildAccount(ALLAccountListDB, 5, year, month, day);
                    AccountAndFinanceExpensesList = RequestExpenses.AccountsAndFinanceDetailsList;




                    //Start PDF Service


                    MemoryStream ms = new MemoryStream();

                    //Size of page

                    iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate());
                    iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.A4);

                    document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                    PdfWriter pw = PdfWriter.GetInstance(document, ms);

                    document.SetMargins(5, 5, 30, 30);

                    //Call the footer Function

                    pw.PageEvent = new HeaderFooter2();

                    pw.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    document.Open();

                    //Handle fonts and Sizes +  Attachments images logos 

                    //   iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.LETTER);



                    //document.SetMargins(0, 0, 20, 20);
                    BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font font = new Font(bf, 7, Font.NORMAL);
                    Font fontExpenses = new Font(bf, 7, Font.NORMAL, BaseColor.WHITE);

                    String path = Path.Combine(_host.WebRootPath, "/Attachments");





                    //Adding paragraph for report generated by  
                    Paragraph prgGeneratedBY = new Paragraph();
                    BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                    prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                    document.Add(prgGeneratedBY);





                    var dt2 = new System.Data.DataTable("grid");


                    dt2.Columns.AddRange(new DataColumn[15] {
                                                      new DataColumn("المتوسط") ,
                                                      new DataColumn("الاجمالى"),
                                                      new DataColumn("ديسمبر"),
                                                      new DataColumn("نوفمبر"),
                                                      new DataColumn("اكتوبر") ,
                                                      new DataColumn("سبتمبر"),
                                                      new DataColumn("اغسطس"),
                                                      new DataColumn("يوليو") ,
                                                      new DataColumn("يونيو"),
                                                      new DataColumn("مايو"),
                                                      new DataColumn("ابريل"),
                                                      new DataColumn("مارس") ,
                                                      new DataColumn("فبراير"),
                                                      new DataColumn("يناير"),
                                                      new DataColumn("البيان / الشهر")
                    });



                    decimal AccumulativeMonth1 = 0;
                    decimal AccumulativeMonth2 = 0;
                    decimal AccumulativeMonth3 = 0;
                    decimal AccumulativeMonth4 = 0;
                    decimal AccumulativeMonth5 = 0;
                    decimal AccumulativeMonth6 = 0;
                    decimal AccumulativeMonth7 = 0;
                    decimal AccumulativeMonth8 = 0;
                    decimal AccumulativeMonth9 = 0;
                    decimal AccumulativeMonth10 = 0;
                    decimal AccumulativeMonth11 = 0;
                    decimal AccumulativeMonth12 = 0;
                    decimal AccumulativeTotal = 0;
                    decimal AccumulativeAverageTotal = 0;




                    string AccumulativeMonth1String = "";
                    string AccumulativeMonth2String = "";
                    string AccumulativeMonth3String = "";
                    string AccumulativeMonth4String = "";
                    string AccumulativeMonth5String = "";
                    string AccumulativeMonth6String = "";
                    string AccumulativeMonth7String = "";
                    string AccumulativeMonth8String = "";
                    string AccumulativeMonth9String = "";
                    string AccumulativeMonth10String = "";
                    string AccumulativeMonth11String = "";
                    string AccumulativeMonth12String = "";
                    string AccumulativeTotalString = "";
                    string AccumulativeAverageTotalString = "";








                    //StartIncome

                    int NumberMonthDivided = 12;
                    if (DateTime.Now.Year == year)
                    {
                        NumberMonthDivided = DateTime.Now.Month; // With Current Month calc
                    }

                    if (AccountsAndFinanceList != null)
                    {
                        foreach (var Account in AccountsAndFinanceList)
                        {



                            // Mic 2023-5-10
                            decimal TotalAccumulative = Account.Accumulative; //.BalancePerMonthList.Sum(x => x.Accumulative);
                            decimal AverageAccumulative = TotalAccumulative / NumberMonthDivided;

                            dt2.Rows.Add(


                                            String.Format("{0:n}", Math.Abs(Decimal.Round(AverageAccumulative, 2))),
                                            String.Format("{0:n}", Math.Abs(Decimal.Round(TotalAccumulative, 2))),

                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[11].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[10].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[9].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[8].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[7].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[6].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[5].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[4].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[3].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[2].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[1].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[0].Accumulative, 2))) : "0",
                                            Account.AccountName
                                        );
                            if (Account.BalancePerMonthList != null && Account.BalancePerMonthList.Count() > 0)
                            {
                                AccumulativeMonth1 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[0].Accumulative, 2));
                                AccumulativeMonth2 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[1].Accumulative, 2));
                                AccumulativeMonth3 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[2].Accumulative, 2));
                                AccumulativeMonth4 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[3].Accumulative, 2));
                                AccumulativeMonth5 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[4].Accumulative, 2));
                                AccumulativeMonth6 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[5].Accumulative, 2));
                                AccumulativeMonth7 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[6].Accumulative, 2));
                                AccumulativeMonth8 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[7].Accumulative, 2));
                                AccumulativeMonth9 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[8].Accumulative, 2));
                                AccumulativeMonth10 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[9].Accumulative, 2));
                                AccumulativeMonth11 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[10].Accumulative, 2));
                                AccumulativeMonth12 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[11].Accumulative, 2));
                                AccumulativeTotal += Math.Abs(Decimal.Round(TotalAccumulative, 2));
                                AccumulativeAverageTotal += Math.Abs(Decimal.Round(AverageAccumulative, 2));
                            }



                        }
                        AccumulativeMonth1String += String.Format("{0:n}", AccumulativeMonth1);
                        AccumulativeMonth2String += String.Format("{0:n}", AccumulativeMonth2);
                        AccumulativeMonth3String += String.Format("{0:n}", AccumulativeMonth3);
                        AccumulativeMonth4String += String.Format("{0:n}", AccumulativeMonth4);
                        AccumulativeMonth5String += String.Format("{0:n}", AccumulativeMonth5);
                        AccumulativeMonth6String += String.Format("{0:n}", AccumulativeMonth6);
                        AccumulativeMonth7String += String.Format("{0:n}", AccumulativeMonth7);
                        AccumulativeMonth8String += String.Format("{0:n}", AccumulativeMonth8);
                        AccumulativeMonth9String += String.Format("{0:n}", AccumulativeMonth9);
                        AccumulativeMonth10String += String.Format("{0:n}", AccumulativeMonth10);
                        AccumulativeMonth11String += String.Format("{0:n}", AccumulativeMonth11);
                        AccumulativeMonth12String += String.Format("{0:n}", AccumulativeMonth12);
                        AccumulativeTotalString += String.Format("{0:n}", AccumulativeTotal);
                        AccumulativeAverageTotalString += String.Format("{0:n}", AccumulativeAverageTotal);



                    }









                    PdfPTable tableHeading = new PdfPTable(3);

                    tableHeading.WidthPercentage = 100;

                    tableHeading.SetTotalWidth(new float[] { 33, 33, 33 });


                    PdfPCell cellPurchasePOInvoiceTypeIDName = new PdfPCell();
                    string cellPurchasePOInvoiceTypeIDNametext = " ";
                    cellPurchasePOInvoiceTypeIDName.Phrase = new Phrase("", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOInvoiceTypeIDName.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellPurchasePOInvoiceTypeIDName.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellPurchasePOInvoiceTypeIDName.BorderColor = BaseColor.WHITE;
                    cellPurchasePOInvoiceTypeIDName.PaddingBottom = 15;
                    cellPurchasePOInvoiceTypeIDName.PaddingTop = 15;
                    //  cellPurchasePOInvoiceTypeIDName.BackgroundColor = new BaseColor(4, 189, 189);

                    PdfPCell cellPurchasePOIDNumber = new PdfPCell();
                    string cellPurchasePOIDNumbertext = "تقرير الايرادات و المصروفات الشهرى لشركة مارينا للسقالات و الروافع لعام " + Year;
                    cellPurchasePOIDNumber.Phrase = new Phrase(cellPurchasePOIDNumbertext, fontExpenses);
                    iTextSharp.text.Font arabicFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOIDNumber.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellPurchasePOIDNumber.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellPurchasePOIDNumber.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    cellPurchasePOIDNumber.BorderColor = BaseColor.BLACK;
                    cellPurchasePOIDNumber.PaddingBottom = 8;
                    cellPurchasePOIDNumber.PaddingTop = 8;
                    cellPurchasePOIDNumber.BackgroundColor = new BaseColor(69, 168, 255);



                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);
                    tableHeading.AddCell(cellPurchasePOIDNumber);
                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);


                    tableHeading.KeepTogether = true;

                    tableHeading.SpacingAfter = 20;







                    PdfPTable table4 = new PdfPTable(4);

                    table4.WidthPercentage = 100;

                    table4.SetTotalWidth(new float[] { 55, 400, 100, 98 });


                    //Adding PdfPTable


                    PdfPTable table = new PdfPTable(dt2.Columns.Count);


                    //table Width
                    table.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    table.SetTotalWidth(new float[] { 95, 95, 95, 95, 95, 95, 95, 95, 95, 95, 95, 95, 95, 95, 60 });
                    table.PaddingTop = 20;

                    for (int i = 0; i < dt2.Columns.Count; i++)
                    {
                        string cellText = HttpUtility.HtmlDecode(dt2.Columns[i].ColumnName);
                        PdfPCell cell = new PdfPCell();
                        cell.Phrase = new Phrase(cellText, font);
                        iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                        cell.PaddingBottom = 5;
                        table.AddCell(cell);


                    }


                    //writing table Data
                    for (int i = 0; i < dt2.Rows.Count; i++)
                    {
                        for (int j = 0; j < dt2.Columns.Count; j++)
                        {
                            //table.AddCell(dt.Rows[i][j].ToString());
                            PdfPCell cell = new PdfPCell();

                            if (j == 14)
                            {
                                cell.BackgroundColor = new BaseColor(196, 196, 196);
                            }
                            cell.ArabicOptions = 1;
                            if (j <= 5)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.Padding = 8;
                            }
                            else if (j >= 9)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }
                            else
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }

                            if (cell.ArabicOptions == 1)
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                cell.Padding = 8;

                            }
                            else
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                cell.Padding = 8;

                            }

                            cell.Phrase = new Phrase(1, dt2.Rows[i][j].ToString(), font);
                            //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                            table.AddCell(cell);

                        }

                    }




                    PdfPTable tableExpenses = new PdfPTable(4);

                    tableExpenses.WidthPercentage = 100;

                    tableExpenses.SetTotalWidth(new float[] { 80, 30, 30, 17 });


                    PdfPCell CellExpenses1 = new PdfPCell();
                    string CellExpenses1Text = "";
                    CellExpenses1.Phrase = new Phrase(CellExpenses1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontNi = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellExpenses1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpenses1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpenses1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellExpenses2 = new PdfPCell();
                    string CellExpenses2text2 = "";
                    CellExpenses2.Phrase = new Phrase(CellExpenses2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellExpenses2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellExpenses2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpenses2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellExpenses3 = new PdfPCell();
                    string CellExpenses3text = " ";
                    CellExpenses3.Phrase = new Phrase(CellExpenses3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#0007EE"))));
                    iTextSharp.text.Font arabicFontWss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellExpenses3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpenses3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpenses3.BorderColor = BaseColor.WHITE;





                    PdfPCell CellExpenses4 = new PdfPCell();
                    string CellExpenses4text = "المصروفات";
                    CellExpenses4.Phrase = new Phrase(CellExpenses4text, fontExpenses);
                    CellExpenses4.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellExpenses4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpenses4.BorderColor = BaseColor.BLACK;
                    CellExpenses4.BackgroundColor = new BaseColor(145, 1, 1);
                    CellExpenses4.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    CellExpenses4.PaddingBottom = 6;
                    CellExpenses4.PaddingTop = 6;





                    tableExpenses.AddCell(CellExpenses1);
                    tableExpenses.AddCell(CellExpenses2);
                    tableExpenses.AddCell(CellExpenses3);
                    tableExpenses.AddCell(CellExpenses4);



                    PdfPTable tableIncomes = new PdfPTable(4);

                    tableIncomes.WidthPercentage = 100;

                    tableIncomes.SetTotalWidth(new float[] { 80, 30, 30, 17 });


                    PdfPCell CellIncomes1 = new PdfPCell();
                    string CellIncomes1Text = "";
                    CellIncomes1.Phrase = new Phrase(CellIncomes1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomes1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomes1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomes1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellIncomes2 = new PdfPCell();
                    string CellIncomes2text2 = "";
                    CellIncomes2.Phrase = new Phrase(CellIncomes2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellIncomes2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellIncomes2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomes2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellIncomes3 = new PdfPCell();
                    string CellIncomes3text = " ";
                    CellIncomes3.Phrase = new Phrase(CellIncomes3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#0007EE"))));
                    CellIncomes3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomes3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomes3.BorderColor = BaseColor.WHITE;





                    PdfPCell CellIncomes4 = new PdfPCell();
                    string CellIncomes4text = "الايرادات";
                    CellIncomes4.Phrase = new Phrase(CellIncomes4text, fontExpenses);
                    CellIncomes4.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomes4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomes4.BorderColor = BaseColor.BLACK;
                    CellIncomes4.BackgroundColor = new BaseColor(148, 139, 82);
                    CellIncomes4.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    CellIncomes4.PaddingBottom = 6;
                    CellIncomes4.PaddingTop = 6;




                    // Total Incomes By Month



                    PdfPTable tableIncomeTotal = new PdfPTable(15);


                    //table Width
                    tableIncomeTotal.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    tableIncomeTotal.SetTotalWidth(new float[] { 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 60 });




                    PdfPCell CellIncomesTotal1 = new PdfPCell();
                    string CellIncomesTotal1Text = AccumulativeMonth1String;
                    CellIncomesTotal1.Phrase = new Phrase(CellIncomesTotal1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal1.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal1.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotal2 = new PdfPCell();
                    string CellIncomesTotal2text2 = AccumulativeMonth2String;
                    CellIncomesTotal2.Phrase = new Phrase(CellIncomesTotal2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellIncomesTotal2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellIncomesTotal2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal2.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal2.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotal3 = new PdfPCell();
                    string CellIncomesTotal3text = AccumulativeMonth3String;
                    CellIncomesTotal3.Phrase = new Phrase(CellIncomesTotal3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal3.BorderColor = BaseColor.BLACK;

                    CellIncomesTotal3.BackgroundColor = new BaseColor(196, 196, 196);



                    PdfPCell CellIncomesTotal4 = new PdfPCell();
                    string CellIncomesTotal4text = AccumulativeMonth4String;
                    CellIncomesTotal4.Phrase = new Phrase(CellIncomesTotal4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal4.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomesTotal4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal4.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal4.PaddingBottom = 6;
                    CellIncomesTotal4.PaddingTop = 6;
                    CellIncomesTotal4.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotal5 = new PdfPCell();
                    string CellIncomesTotal5Text = AccumulativeMonth5String;
                    CellIncomesTotal5.Phrase = new Phrase(CellIncomesTotal5Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal5.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal5.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal5.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal5.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellIncomesTotal6 = new PdfPCell();
                    string CellIncomesTotal6text6 = AccumulativeMonth6String;
                    CellIncomesTotal6.Phrase = new Phrase(CellIncomesTotal6text6, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellIncomesTotal6.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellIncomesTotal6.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal6.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal6.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotal7 = new PdfPCell();
                    string CellIncomesTotal7text = AccumulativeMonth7String;
                    CellIncomesTotal7.Phrase = new Phrase(CellIncomesTotal7text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal7.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal7.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal7.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal7.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellIncomesTotal8 = new PdfPCell();
                    string CellIncomesTotal8text = AccumulativeMonth8String;
                    CellIncomesTotal8.Phrase = new Phrase(CellIncomesTotal8text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal8.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomesTotal8.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal8.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal8.PaddingBottom = 6;
                    CellIncomesTotal8.PaddingTop = 6;
                    CellIncomesTotal8.BackgroundColor = new BaseColor(196, 196, 196);






                    PdfPCell CellIncomesTotal9 = new PdfPCell();
                    string CellIncomesTotal9Text = AccumulativeMonth9String;
                    CellIncomesTotal9.Phrase = new Phrase(CellIncomesTotal9Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal9.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal9.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal9.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal9.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellIncomesTotal10 = new PdfPCell();
                    string CellIncomesTotal10text10 = AccumulativeMonth10String;
                    CellIncomesTotal10.Phrase = new Phrase(CellIncomesTotal10text10, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellIncomesTotal10.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellIncomesTotal10.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal10.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal10.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotal11 = new PdfPCell();
                    string CellIncomesTotal11text = AccumulativeMonth11String;
                    CellIncomesTotal11.Phrase = new Phrase(CellIncomesTotal11text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal11.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal11.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal11.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal11.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellIncomesTotal12 = new PdfPCell();
                    string CellIncomesTotal12text = AccumulativeMonth12String;
                    CellIncomesTotal12.Phrase = new Phrase(CellIncomesTotal12text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal12.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomesTotal12.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal12.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal12.PaddingBottom = 6;
                    CellIncomesTotal12.PaddingTop = 6;
                    CellIncomesTotal12.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotalSum = new PdfPCell();
                    string CellIncomesTotalSumtext = AccumulativeTotalString;
                    CellIncomesTotalSum.Phrase = new Phrase(CellIncomesTotalSumtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotalSum.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomesTotalSum.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotalSum.BorderColor = BaseColor.BLACK;
                    CellIncomesTotalSum.PaddingBottom = 6;
                    CellIncomesTotalSum.PaddingTop = 6;
                    CellIncomesTotalSum.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotalAverage = new PdfPCell();
                    string CellIncomesTotalAveragetext = AccumulativeAverageTotalString;
                    CellIncomesTotalAverage.Phrase = new Phrase(CellIncomesTotalAveragetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotalAverage.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomesTotalAverage.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotalAverage.BorderColor = BaseColor.BLACK;
                    CellIncomesTotalAverage.PaddingBottom = 6;
                    CellIncomesTotalAverage.PaddingTop = 6;
                    CellIncomesTotalAverage.BackgroundColor = new BaseColor(196, 196, 196);






                    PdfPCell CellTotalIncomeString = new PdfPCell();
                    string CellTotalIncomeStringtext = "اجمالى الايرادات";
                    CellTotalIncomeString.Phrase = new Phrase(CellTotalIncomeStringtext, font);
                    CellTotalIncomeString.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellTotalIncomeString.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTotalIncomeString.BorderColor = BaseColor.BLACK;
                    CellTotalIncomeString.PaddingBottom = 6;
                    CellTotalIncomeString.PaddingTop = 6;
                    CellTotalIncomeString.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    CellTotalIncomeString.BackgroundColor = new BaseColor(196, 196, 196);



                    tableIncomeTotal.AddCell(CellIncomesTotalAverage);
                    tableIncomeTotal.AddCell(CellIncomesTotalSum);
                    tableIncomeTotal.AddCell(CellIncomesTotal12);
                    tableIncomeTotal.AddCell(CellIncomesTotal11);
                    tableIncomeTotal.AddCell(CellIncomesTotal10);
                    tableIncomeTotal.AddCell(CellIncomesTotal9);
                    tableIncomeTotal.AddCell(CellIncomesTotal8);
                    tableIncomeTotal.AddCell(CellIncomesTotal7);
                    tableIncomeTotal.AddCell(CellIncomesTotal6);
                    tableIncomeTotal.AddCell(CellIncomesTotal5);
                    tableIncomeTotal.AddCell(CellIncomesTotal4);
                    tableIncomeTotal.AddCell(CellIncomesTotal3);
                    tableIncomeTotal.AddCell(CellIncomesTotal2);
                    tableIncomeTotal.AddCell(CellIncomesTotal1);

                    tableIncomeTotal.AddCell(CellTotalIncomeString);







                    PdfPTable tableWin = new PdfPTable(4);

                    tableWin.WidthPercentage = 100;

                    tableWin.SetTotalWidth(new float[] { 80, 30, 30, 17 });


                    PdfPCell CellWin1 = new PdfPCell();
                    string CellWin1Text = "";
                    CellWin1.Phrase = new Phrase(CellWin1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellWin1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellWin1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellWin1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellWin2 = new PdfPCell();
                    string CellWin2text2 = "";
                    CellWin2.Phrase = new Phrase(CellWin2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellWin2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellWin2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellWin2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellWin3 = new PdfPCell();
                    string CellWin3text = " ";
                    CellWin3.Phrase = new Phrase(CellWin3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#0007EE"))));
                    CellWin3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellWin3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellWin3.BorderColor = BaseColor.WHITE;





                    PdfPCell CellWin4 = new PdfPCell();
                    string CellWin4text = "صافى الربح";
                    CellWin4.Phrase = new Phrase(CellWin4text, font);
                    CellWin4.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellWin4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellWin4.BorderColor = BaseColor.BLACK;
                    CellWin4.BackgroundColor = new BaseColor(162, 211, 254);
                    CellWin4.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    CellWin4.PaddingBottom = 6;
                    CellWin4.PaddingTop = 6;





                    tableWin.AddCell(CellWin1);
                    tableWin.AddCell(CellWin2);
                    tableWin.AddCell(CellWin3);
                    tableWin.AddCell(CellWin4);











                    var dtExpenses = new System.Data.DataTable("grid");


                    dtExpenses.Columns.AddRange(new DataColumn[15] {
                                                      new DataColumn("المتوسط") ,
                                                      new DataColumn("الاجمالى"),
                                                      new DataColumn("ديسمبر"),
                                                      new DataColumn("نوفمبر"),
                                                      new DataColumn("اكتوبر") ,
                                                      new DataColumn("سبتمبر"),
                                                      new DataColumn("اغسطس"),
                                                      new DataColumn("يوليو") ,
                                                      new DataColumn("يونيو"),
                                                      new DataColumn("مايو"),
                                                      new DataColumn("ابريل"),
                                                      new DataColumn("مارس") ,
                                                      new DataColumn("فبراير"),
                                                      new DataColumn("يناير"),
                                                      new DataColumn("البيان / الشهر")
                    });









                    decimal AccumulativeExpensesMonth1 = 0;
                    decimal AccumulativeExpensesMonth2 = 0;
                    decimal AccumulativeExpensesMonth3 = 0;
                    decimal AccumulativeExpensesMonth4 = 0;
                    decimal AccumulativeExpensesMonth5 = 0;
                    decimal AccumulativeExpensesMonth6 = 0;
                    decimal AccumulativeExpensesMonth7 = 0;
                    decimal AccumulativeExpensesMonth8 = 0;
                    decimal AccumulativeExpensesMonth9 = 0;
                    decimal AccumulativeExpensesMonth10 = 0;
                    decimal AccumulativeExpensesMonth11 = 0;
                    decimal AccumulativeExpensesMonth12 = 0;
                    decimal AccumulativeExpensesTotal = 0;
                    decimal AccumulativeExpensesAverageTotal = 0;

                    string AccumulativeExpensesMonth1String = "";
                    string AccumulativeExpensesMonth2String = "";
                    string AccumulativeExpensesMonth3String = "";
                    string AccumulativeExpensesMonth4String = "";
                    string AccumulativeExpensesMonth5String = "";
                    string AccumulativeExpensesMonth6String = "";
                    string AccumulativeExpensesMonth7String = "";
                    string AccumulativeExpensesMonth8String = "";
                    string AccumulativeExpensesMonth9String = "";
                    string AccumulativeExpensesMonth10String = "";
                    string AccumulativeExpensesMonth11String = "";
                    string AccumulativeExpensesMonth12String = "";
                    string AccumulativeExpensesTotalString = "";
                    string AccumulativeExpensesAverageTotalString = "";



                    //Start Expenses



                    if (AccountAndFinanceExpensesList != null)
                    {
                        foreach (var ExpensesAccounts in AccountAndFinanceExpensesList)
                        {

                            decimal TotalAccumulative = ExpensesAccounts.BalancePerMonthList.Sum(x => x.Accumulative);
                            decimal AverageAccumulative = TotalAccumulative / NumberMonthDivided;


                            dtExpenses.Rows.Add(


                String.Format("{0:n}", Math.Abs(Decimal.Round(AverageAccumulative, 2))),
                String.Format("{0:n}", Math.Abs(Decimal.Round(TotalAccumulative, 2))),
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[11].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[10].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[9].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[8].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[7].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[6].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[5].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[4].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[3].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[2].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[1].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0 ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[0].Accumulative, 2))) : "0",
                ExpensesAccounts.AccountName
                                      );
                            if (ExpensesAccounts.BalancePerMonthList != null && ExpensesAccounts.BalancePerMonthList.Count() > 0)
                            {
                                AccumulativeExpensesMonth1 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[0].Accumulative, 2));
                                AccumulativeExpensesMonth2String += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[1].Accumulative, 2));
                                AccumulativeExpensesMonth3 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[2].Accumulative, 2));
                                AccumulativeExpensesMonth4 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[3].Accumulative, 2));
                                AccumulativeExpensesMonth5 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[4].Accumulative, 2));
                                AccumulativeExpensesMonth6 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[5].Accumulative, 2));
                                AccumulativeExpensesMonth7 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[6].Accumulative, 2));
                                AccumulativeExpensesMonth8 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[7].Accumulative, 2));
                                AccumulativeExpensesMonth9 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[8].Accumulative, 2));
                                AccumulativeExpensesMonth10 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[9].Accumulative, 2));
                                AccumulativeExpensesMonth11 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[10].Accumulative, 2));
                                AccumulativeExpensesMonth12 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[11].Accumulative, 2));
                                AccumulativeExpensesTotal += Math.Abs(Decimal.Round(TotalAccumulative, 2));
                                AccumulativeExpensesAverageTotal += Math.Abs(Decimal.Round(AverageAccumulative, 2));

                            }





                        }


                        AccumulativeExpensesMonth1String = String.Format("{0:n}", AccumulativeExpensesMonth1);
                        AccumulativeExpensesMonth2String = String.Format("{0:n}", AccumulativeExpensesMonth2);
                        AccumulativeExpensesMonth3String = String.Format("{0:n}", AccumulativeExpensesMonth3);
                        AccumulativeExpensesMonth4String = String.Format("{0:n}", AccumulativeExpensesMonth4);
                        AccumulativeExpensesMonth5String = String.Format("{0:n}", AccumulativeExpensesMonth5);
                        AccumulativeExpensesMonth6String = String.Format("{0:n}", AccumulativeExpensesMonth6);
                        AccumulativeExpensesMonth7String = String.Format("{0:n}", AccumulativeExpensesMonth7);
                        AccumulativeExpensesMonth8String = String.Format("{0:n}", AccumulativeExpensesMonth8);
                        AccumulativeExpensesMonth9String = String.Format("{0:n}", AccumulativeExpensesMonth9);
                        AccumulativeExpensesMonth10String = String.Format("{0:n}", AccumulativeExpensesMonth10);
                        AccumulativeExpensesMonth11String = String.Format("{0:n}", AccumulativeExpensesMonth11);
                        AccumulativeExpensesMonth12String = String.Format("{0:n}", AccumulativeExpensesMonth12);
                        AccumulativeExpensesTotalString = String.Format("{0:n}", AccumulativeExpensesTotal);
                        AccumulativeExpensesAverageTotalString = String.Format("{0:n}", AccumulativeExpensesAverageTotal);



                    }









                    PdfPTable MaintableExpenses = new PdfPTable(dtExpenses.Columns.Count);


                    //table Width
                    MaintableExpenses.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    MaintableExpenses.SetTotalWidth(new float[] { 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 60 });
                    MaintableExpenses.PaddingTop = 20;

                    for (int i = 0; i < dtExpenses.Columns.Count; i++)
                    {
                        string cellText = HttpUtility.HtmlDecode(dtExpenses.Columns[i].ColumnName);
                        PdfPCell cell = new PdfPCell();
                        cell.Phrase = new Phrase(cellText, font);
                        iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                        cell.PaddingBottom = 5;
                        MaintableExpenses.AddCell(cell);


                    }


                    //writing table Data
                    for (int i = 0; i < dtExpenses.Rows.Count; i++)
                    {
                        for (int j = 0; j < dtExpenses.Columns.Count; j++)
                        {
                            //table.AddCell(dt.Rows[i][j].ToString());
                            PdfPCell cell = new PdfPCell();

                            if (j == 14)
                            {
                                cell.BackgroundColor = new BaseColor(196, 196, 196);
                            }
                            cell.ArabicOptions = 1;
                            if (j <= 5)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.Padding = 8;
                            }
                            else if (j >= 9)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }
                            else
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }

                            if (cell.ArabicOptions == 1)
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                cell.Padding = 8;

                            }
                            else
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                cell.Padding = 8;

                            }

                            cell.Phrase = new Phrase(1, dtExpenses.Rows[i][j].ToString(), font);
                            //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                            MaintableExpenses.AddCell(cell);

                        }

                    }


                    // Total Incomes By Month



                    PdfPTable tableExpensesTotal = new PdfPTable(15);


                    //table Width
                    tableExpensesTotal.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    tableExpensesTotal.SetTotalWidth(new float[] { 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 60 });




                    PdfPCell CellExpensesTotal1 = new PdfPCell();
                    string CellExpensesTotal1Text = AccumulativeExpensesMonth1String;
                    CellExpensesTotal1.Phrase = new Phrase(CellExpensesTotal1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal1.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal1.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellExpensesTotal2 = new PdfPCell();
                    string CellExpensesTotal2text2 = AccumulativeExpensesMonth2String;
                    CellExpensesTotal2.Phrase = new Phrase(CellExpensesTotal2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellExpensesTotal2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellExpensesTotal2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal2.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal2.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellExpensesTotal3 = new PdfPCell();
                    string CellExpensesTotal3text = AccumulativeExpensesMonth3String;
                    CellExpensesTotal3.Phrase = new Phrase(CellExpensesTotal3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal3.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal3.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellExpensesTotal4 = new PdfPCell();
                    string CellExpensesTotal4text = AccumulativeExpensesMonth4String;
                    CellExpensesTotal4.Phrase = new Phrase(CellExpensesTotal4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal4.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellExpensesTotal4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal4.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal4.PaddingBottom = 6;
                    CellExpensesTotal4.PaddingTop = 6;
                    CellExpensesTotal4.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellExpensesTotal5 = new PdfPCell();
                    string CellExpensesTotal5Text = AccumulativeExpensesMonth5String;
                    CellExpensesTotal5.Phrase = new Phrase(CellExpensesTotal5Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal5.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal5.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal5.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal5.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellExpensesTotal6 = new PdfPCell();
                    string CellExpensesTotal6text6 = AccumulativeExpensesMonth6String;
                    CellExpensesTotal6.Phrase = new Phrase(CellExpensesTotal6text6, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellExpensesTotal6.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellExpensesTotal6.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal6.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal6.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellExpensesTotal7 = new PdfPCell();
                    string CellExpensesTotal7text = AccumulativeExpensesMonth7String;
                    CellExpensesTotal7.Phrase = new Phrase(CellExpensesTotal7text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal7.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal7.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal7.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal7.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellExpensesTotal8 = new PdfPCell();
                    string CellExpensesTotal8text = AccumulativeExpensesMonth8String;
                    CellExpensesTotal8.Phrase = new Phrase(CellExpensesTotal8text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal8.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellExpensesTotal8.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal8.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal8.PaddingBottom = 6;
                    CellExpensesTotal8.PaddingTop = 6;
                    CellExpensesTotal8.BackgroundColor = new BaseColor(196, 196, 196);






                    PdfPCell CellExpensesTotal9 = new PdfPCell();
                    string CellExpensesTotal9Text = AccumulativeExpensesMonth9String;
                    CellExpensesTotal9.Phrase = new Phrase(CellExpensesTotal9Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal9.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal9.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal9.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal9.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellExpensesTotal10 = new PdfPCell();
                    string CellExpensesTotal10text10 = AccumulativeExpensesMonth10String;
                    CellExpensesTotal10.Phrase = new Phrase(CellExpensesTotal10text10, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellExpensesTotal10.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellExpensesTotal10.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal10.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal10.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellExpensesTotal11 = new PdfPCell();
                    string CellExpensesTotal11text = AccumulativeExpensesMonth11String;
                    CellExpensesTotal11.Phrase = new Phrase(CellExpensesTotal11text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal11.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal11.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal11.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal11.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellExpensesTotal12 = new PdfPCell();
                    string CellExpensesTotal12text = AccumulativeExpensesMonth12String;
                    CellExpensesTotal12.Phrase = new Phrase(CellExpensesTotal12text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal12.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal12.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal12.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal12.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellExpensesTotalSum = new PdfPCell();
                    string CellExpensesTotalSumtext = AccumulativeExpensesTotalString;
                    CellExpensesTotalSum.Phrase = new Phrase(CellExpensesTotalSumtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotalSum.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotalSum.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotalSum.BorderColor = BaseColor.BLACK;
                    CellExpensesTotalSum.BackgroundColor = new BaseColor(196, 196, 196);



                    PdfPCell CellExpensesAverageTotal = new PdfPCell();
                    string CellExpensesAverageTotaltext = AccumulativeExpensesAverageTotalString;
                    CellExpensesAverageTotal.Phrase = new Phrase(CellExpensesAverageTotaltext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesAverageTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesAverageTotal.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesAverageTotal.BorderColor = BaseColor.BLACK;
                    CellExpensesAverageTotal.BackgroundColor = new BaseColor(196, 196, 196);







                    PdfPCell CellTotalExpensesString = new PdfPCell();
                    string CellTotalExpensesStringtext = "اجمالى المصروفات";
                    CellTotalExpensesString.Phrase = new Phrase(CellTotalExpensesStringtext, font);
                    CellTotalExpensesString.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellTotalExpensesString.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTotalExpensesString.BorderColor = BaseColor.BLACK;
                    CellTotalExpensesString.PaddingBottom = 6;
                    CellTotalExpensesString.PaddingTop = 6;
                    CellTotalExpensesString.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    CellTotalExpensesString.BackgroundColor = new BaseColor(196, 196, 196);


                    tableExpensesTotal.AddCell(CellExpensesAverageTotal);
                    tableExpensesTotal.AddCell(CellExpensesTotalSum);
                    tableExpensesTotal.AddCell(CellExpensesTotal12);
                    tableExpensesTotal.AddCell(CellExpensesTotal11);
                    tableExpensesTotal.AddCell(CellExpensesTotal10);
                    tableExpensesTotal.AddCell(CellExpensesTotal9);
                    tableExpensesTotal.AddCell(CellExpensesTotal8);
                    tableExpensesTotal.AddCell(CellExpensesTotal7);
                    tableExpensesTotal.AddCell(CellExpensesTotal6);
                    tableExpensesTotal.AddCell(CellExpensesTotal5);
                    tableExpensesTotal.AddCell(CellExpensesTotal4);
                    tableExpensesTotal.AddCell(CellExpensesTotal3);
                    tableExpensesTotal.AddCell(CellExpensesTotal2);
                    tableExpensesTotal.AddCell(CellExpensesTotal1);

                    tableExpensesTotal.AddCell(CellTotalExpensesString);






                    decimal DifferenceWinMonth1 = Decimal.Round(AccumulativeMonth1 - AccumulativeExpensesMonth1);
                    decimal DifferenceWinMonth2 = Decimal.Round(AccumulativeMonth2 - AccumulativeExpensesMonth2);
                    decimal DifferenceWinMonth3 = Decimal.Round(AccumulativeMonth3 - AccumulativeExpensesMonth3);
                    decimal DifferenceWinMonth4 = Decimal.Round(AccumulativeMonth4 - AccumulativeExpensesMonth4);
                    decimal DifferenceWinMonth5 = Decimal.Round(AccumulativeMonth5 - AccumulativeExpensesMonth5);
                    decimal DifferenceWinMonth6 = Decimal.Round(AccumulativeMonth6 - AccumulativeExpensesMonth6);
                    decimal DifferenceWinMonth7 = Decimal.Round(AccumulativeMonth7 - AccumulativeExpensesMonth7);
                    decimal DifferenceWinMonth8 = Decimal.Round(AccumulativeMonth8 - AccumulativeExpensesMonth8);
                    decimal DifferenceWinMonth9 = Decimal.Round(AccumulativeMonth9 - AccumulativeExpensesMonth9);
                    decimal DifferenceWinMonth10 = Decimal.Round(AccumulativeMonth10 - AccumulativeExpensesMonth10);
                    decimal DifferenceWinMonth11 = Decimal.Round(AccumulativeMonth11 - AccumulativeExpensesMonth11);
                    decimal DifferenceWinMonth12 = Decimal.Round(AccumulativeMonth12 - AccumulativeExpensesMonth12);
                    decimal DifferenceWinMonthTotal = Decimal.Round(AccumulativeTotal - AccumulativeExpensesTotal);
                    decimal DifferenceWinMonthAverageTotal = Decimal.Round(AccumulativeAverageTotal - AccumulativeExpensesAverageTotal);



















                    // Total Win Table



                    PdfPTable tableDifferenceWinTable = new PdfPTable(15);


                    //table Width
                    tableDifferenceWinTable.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    tableDifferenceWinTable.SetTotalWidth(new float[] { 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 60 });


                    PdfPCell CellDiffWin1 = new PdfPCell();
                    string CellDiffWin1Text = String.Format("{0:n}", DifferenceWinMonth1);
                    CellDiffWin1.Phrase = new Phrase(CellDiffWin1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWin1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellDiffWin1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin1.BorderColor = BaseColor.BLACK;
                    CellDiffWin1.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellDiffWin2 = new PdfPCell();
                    string CellDiffWin2text2 = String.Format("{0:n}", DifferenceWinMonth2);
                    CellDiffWin2.Phrase = new Phrase(CellDiffWin2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellDiffWin2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellDiffWin2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin2.BorderColor = BaseColor.BLACK;
                    CellDiffWin2.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellDiffWin3 = new PdfPCell();
                    string CellDiffWin3text = String.Format("{0:n}", DifferenceWinMonth3);
                    CellDiffWin3.Phrase = new Phrase(CellDiffWin3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWin3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellDiffWin3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin3.BorderColor = BaseColor.BLACK;
                    CellDiffWin3.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellDiffWin4 = new PdfPCell();
                    string CellDiffWin4text = String.Format("{0:n}", DifferenceWinMonth4);
                    CellDiffWin4.Phrase = new Phrase(CellDiffWin4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWin4.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellDiffWin4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin4.BorderColor = BaseColor.BLACK;
                    CellDiffWin4.PaddingBottom = 6;
                    CellDiffWin4.PaddingTop = 6;
                    CellDiffWin4.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellDiffWin5 = new PdfPCell();
                    string CellDiffWin5Text = String.Format("{0:n}", DifferenceWinMonth5);
                    CellDiffWin5.Phrase = new Phrase(CellDiffWin5Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWin5.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellDiffWin5.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin5.BorderColor = BaseColor.BLACK;
                    CellDiffWin5.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellDiffWin6 = new PdfPCell();
                    string CellDiffWin6text6 = String.Format("{0:n}", DifferenceWinMonth6);
                    CellDiffWin6.Phrase = new Phrase(CellDiffWin6text6, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellDiffWin6.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellDiffWin6.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin6.BorderColor = BaseColor.BLACK;
                    CellDiffWin6.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellDiffWin7 = new PdfPCell();
                    string CellDiffWin7text = String.Format("{0:n}", DifferenceWinMonth7);
                    CellDiffWin7.Phrase = new Phrase(CellDiffWin7text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWin7.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellDiffWin7.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin7.BorderColor = BaseColor.BLACK;
                    CellDiffWin7.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellDiffWin8 = new PdfPCell();
                    string CellDiffWin8text = String.Format("{0:n}", DifferenceWinMonth8);
                    CellDiffWin8.Phrase = new Phrase(CellDiffWin8text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWin8.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellDiffWin8.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin8.BorderColor = BaseColor.BLACK;
                    CellDiffWin8.PaddingBottom = 6;
                    CellDiffWin8.PaddingTop = 6;
                    CellDiffWin8.BackgroundColor = new BaseColor(196, 196, 196);






                    PdfPCell CellDiffWin9 = new PdfPCell();
                    string CellDiffWin9Text = String.Format("{0:n}", DifferenceWinMonth9);
                    CellDiffWin9.Phrase = new Phrase(CellDiffWin9Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWin9.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellDiffWin9.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin9.BorderColor = BaseColor.BLACK;
                    CellDiffWin9.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellDiffWin10 = new PdfPCell();
                    string CellDiffWin10text10 = String.Format("{0:n}", DifferenceWinMonth10);
                    CellDiffWin10.Phrase = new Phrase(CellDiffWin10text10, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellDiffWin10.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellDiffWin10.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin10.BorderColor = BaseColor.BLACK;
                    CellDiffWin10.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellDiffWin11 = new PdfPCell();
                    string CellDiffWin11text = String.Format("{0:n}", DifferenceWinMonth11);
                    CellDiffWin11.Phrase = new Phrase(CellDiffWin11text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWin11.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellDiffWin11.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin11.BorderColor = BaseColor.BLACK;
                    CellDiffWin11.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellDiffWin12 = new PdfPCell();
                    string CellDiffWin12text = String.Format("{0:n}", DifferenceWinMonth12);
                    CellDiffWin12.Phrase = new Phrase(CellDiffWin12text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWin12.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellDiffWin12.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin12.BorderColor = BaseColor.BLACK;
                    CellDiffWin12.PaddingBottom = 6;
                    CellDiffWin12.PaddingTop = 6;
                    CellDiffWin12.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellDiffWin12Sum = new PdfPCell();
                    string CellDiffWin12Sumtext = String.Format("{0:n}", DifferenceWinMonthTotal);
                    CellDiffWin12Sum.Phrase = new Phrase(CellDiffWin12Sumtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWin12Sum.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellDiffWin12Sum.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWin12Sum.BorderColor = BaseColor.BLACK;
                    CellDiffWin12Sum.PaddingBottom = 6;
                    CellDiffWin12Sum.PaddingTop = 6;
                    CellDiffWin12Sum.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellDiffWinTotalAverage = new PdfPCell();
                    string CellDiffWinTotalAveragetext = String.Format("{0:n}", DifferenceWinMonthAverageTotal);
                    CellDiffWinTotalAverage.Phrase = new Phrase(CellDiffWinTotalAveragetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellDiffWinTotalAverage.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellDiffWinTotalAverage.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWinTotalAverage.BorderColor = BaseColor.BLACK;
                    CellDiffWinTotalAverage.PaddingBottom = 6;
                    CellDiffWinTotalAverage.PaddingTop = 6;
                    CellDiffWinTotalAverage.BackgroundColor = new BaseColor(196, 196, 196);







                    PdfPCell CellDiffWinString = new PdfPCell();
                    string CellDiffWinStringtext = "(صافى الربح (الفرق بين الايرادات و المصروفات";
                    CellDiffWinString.Phrase = new Phrase(CellDiffWinStringtext, font);
                    CellDiffWinString.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellDiffWinString.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDiffWinString.BorderColor = BaseColor.BLACK;
                    CellDiffWinString.PaddingBottom = 6;
                    CellDiffWinString.PaddingTop = 6;
                    CellDiffWinString.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    CellDiffWinString.BackgroundColor = new BaseColor(196, 196, 196);



                    tableDifferenceWinTable.AddCell(CellDiffWinTotalAverage);
                    tableDifferenceWinTable.AddCell(CellDiffWin12Sum);
                    tableDifferenceWinTable.AddCell(CellDiffWin12);
                    tableDifferenceWinTable.AddCell(CellDiffWin11);
                    tableDifferenceWinTable.AddCell(CellDiffWin10);
                    tableDifferenceWinTable.AddCell(CellDiffWin9);
                    tableDifferenceWinTable.AddCell(CellDiffWin8);
                    tableDifferenceWinTable.AddCell(CellDiffWin7);
                    tableDifferenceWinTable.AddCell(CellDiffWin6);
                    tableDifferenceWinTable.AddCell(CellDiffWin5);
                    tableDifferenceWinTable.AddCell(CellDiffWin4);
                    tableDifferenceWinTable.AddCell(CellDiffWin3);
                    tableDifferenceWinTable.AddCell(CellDiffWin2);
                    tableDifferenceWinTable.AddCell(CellDiffWin1);

                    tableDifferenceWinTable.AddCell(CellDiffWinString);





                    tableIncomes.AddCell(CellIncomes1);
                    tableIncomes.AddCell(CellIncomes2);
                    tableIncomes.AddCell(CellIncomes3);
                    tableIncomes.AddCell(CellIncomes4);

                    document.Add(tableHeading);
                    document.Add(table4);
                    document.Add(tableIncomes);
                    document.Add(table);
                    document.Add(tableIncomeTotal);
                    document.Add(tableExpenses);
                    tableExpenses.SpacingBefore = 10;
                    document.Add(MaintableExpenses);
                    document.Add(tableExpensesTotal);
                    document.Add(tableWin);
                    document.Add(tableDifferenceWinTable);

                    // table.SpacingAfter = 15;
                    //  document.Add(tablTotals);
                    //  tablTotals.SpacingAfter = 200;


                    document.Close();
                    byte[] result = ms.ToArray();
                    ms = new MemoryStream();
                    ms.Write(result, 0, result.Length);
                    ms.Position = 0;


                    var CompanyName = companyname.ToLower();

                    string FullFileName = DateTime.Now.ToFileTime() + "_" + "AccountAndFinanceIncome.pdf";
                    string PathsTR = "Attachments/" + CompanyName + "/";
                    string Filepath = Path.Combine(_host.WebRootPath, PathsTR);
                    string p_strPath = Path.Combine(Filepath, FullFileName);
                    if (!Directory.Exists(Filepath))
                    {
                        Directory.CreateDirectory(Filepath);
                    }

                    System.IO.File.WriteAllBytes(p_strPath, result);



                    Response.Message = Globals.baseURL + '/' + PathsTR + FullFileName;
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

        public List<AccountBalancePerExpOrIncType> AccountBalancePerExpOrIncType(List<long> AccountIdList, int Year, int CategoryType)
        {
            var AccountBalancePerExpOrIncTypeList = new List<AccountBalancePerExpOrIncType>();
            var ListAccountOfJournalList = new List<BalancePerMonth>();

            var ListOfAccountJournalEntryList = _unitOfWork.AccountOfJournalEntries
            .FindAll(x => x.Active == true && AccountIdList.Contains(x.AccountId) && x.EntryDate.Year == Year && x.Account.AccountCategoryId == CategoryType).ToList().GroupBy(x => x.ExpOrIncTypeName).OrderByDescending(x => x.Key);
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

        public BaseMessageResponse GetAccountAnnualReportPDF(string companyname, [FromHeader] int year = 0, [FromHeader] string AccountID = null)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    List<long> ListOfAccounts = new List<long>();
                    if (string.IsNullOrWhiteSpace(AccountID))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Must be select at least one Account";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    else
                    {
                        string ListOfAccountsSTR = AccountID;
                        ListOfAccounts = ListOfAccountsSTR.Split(',').Select(x => x.Trim()).Select(x => long.Parse(x)).ToList();
                    }
                    if (ListOfAccounts.Count() <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Must be select at least one Account";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    // Get List Of Accounts Name 
                    List<string> AccountsNameList = new List<string>();
                    var AccountsListDB = _unitOfWork.Accounts.FindAll(x => ListOfAccounts.Contains(x.Id)).Select(x => x.AccountName).ToList();
                    string AccountsNameSTRList = string.Join(",", AccountsListDB);
                    //if (!string.IsNullOrEmpty(headers["AccountID"]) && List<long>.pas(headers["AccountID"], out listOfIds))
                    //{
                    //    long.TryParse(headers["AccountID"], out AccountID);
                    //}else
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = "Account ID is Mandatory";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}
                    DateTime Today = DateTime.Now;
                    int Year = Today.Year;
                    if (year != 0)
                    {
                        Year = year;
                    }



                    var AccountBalancePerIncomesList = new List<AccountBalancePerExpOrIncType>();
                    AccountBalancePerIncomesList = AccountBalancePerExpOrIncType(ListOfAccounts, Year, 4);


                    var AccountBalancePerExpensesList = new List<AccountBalancePerExpOrIncType>();
                    AccountBalancePerExpensesList = AccountBalancePerExpOrIncType(ListOfAccounts, Year, 5);



                    //Start PDF Service


                    MemoryStream ms = new MemoryStream();

                    //Size of page

                    iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate());
                    iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.A4);

                    document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                    PdfWriter pw = PdfWriter.GetInstance(document, ms);

                    document.SetMargins(5, 5, 30, 30);

                    //Call the footer Function

                    pw.PageEvent = new HeaderFooter2();

                    pw.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    document.Open();

                    //Handle fonts and Sizes +  Attachments images logos 

                    //   iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.LETTER);



                    //document.SetMargins(0, 0, 20, 20);
                    BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font font = new Font(bf, 8, Font.NORMAL);
                    Font fontExpenses = new Font(bf, 8, Font.NORMAL, BaseColor.WHITE);

                    String path = Path.Combine(_host.WebRootPath, "/Attachments");





                    //Adding paragraph for report generated by  
                    Paragraph prgGeneratedBY = new Paragraph();
                    BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                    prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                    document.Add(prgGeneratedBY);





                    var dt2 = new System.Data.DataTable("grid");


                    dt2.Columns.AddRange(new DataColumn[15] {
                                                      new DataColumn("المتوسط") ,
                                                      new DataColumn("الاجمالى"),
                                                      new DataColumn("ديسمبر"),
                                                      new DataColumn("نوفمبر"),
                                                      new DataColumn("اكتوبر") ,
                                                      new DataColumn("سبتمبر"),
                                                      new DataColumn("اغسطس"),
                                                      new DataColumn("يوليو") ,
                                                      new DataColumn("يونيو"),
                                                      new DataColumn("مايو"),
                                                      new DataColumn("ابريل"),
                                                      new DataColumn("مارس") ,
                                                      new DataColumn("فبراير"),
                                                      new DataColumn("يناير"),
                                                      new DataColumn("البيان / الشهر")
                    });




                    decimal AccumulativeMonth1 = 0;
                    decimal AccumulativeMonth2 = 0;
                    decimal AccumulativeMonth3 = 0;
                    decimal AccumulativeMonth4 = 0;
                    decimal AccumulativeMonth5 = 0;
                    decimal AccumulativeMonth6 = 0;
                    decimal AccumulativeMonth7 = 0;
                    decimal AccumulativeMonth8 = 0;
                    decimal AccumulativeMonth9 = 0;
                    decimal AccumulativeMonth10 = 0;
                    decimal AccumulativeMonth11 = 0;
                    decimal AccumulativeMonth12 = 0;
                    decimal AccumulativeTotal = 0;
                    decimal AccumulativeAverageTotal = 0;




                    string AccumulativeMonth1String = "";
                    string AccumulativeMonth2String = "";
                    string AccumulativeMonth3String = "";
                    string AccumulativeMonth4String = "";
                    string AccumulativeMonth5String = "";
                    string AccumulativeMonth6String = "";
                    string AccumulativeMonth7String = "";
                    string AccumulativeMonth8String = "";
                    string AccumulativeMonth9String = "";
                    string AccumulativeMonth10String = "";
                    string AccumulativeMonth11String = "";
                    string AccumulativeMonth12String = "";
                    string AccumulativeTotalString = "";
                    string AccumulativeAverageTotalString = "";








                    //StartIncome

                    int NumberMonthDivided = 12;
                    if (DateTime.Now.Year == Year)
                    {
                        NumberMonthDivided = DateTime.Now.Month; // With Current Month calc
                    }

                    if (AccountBalancePerIncomesList != null)
                    {
                        foreach (var Account in AccountBalancePerIncomesList)
                        {



                            // Mic 2023-5-10
                            decimal TotalAccumulative = Account.TotalAmount; //.BalancePerMonthList.Sum(x => x.Accumulative);
                            decimal AverageAccumulative = TotalAccumulative / NumberMonthDivided;

                            dt2.Rows.Add(


                                            String.Format("{0:n}", Math.Abs(Decimal.Round(AverageAccumulative, 2))),
                                            String.Format("{0:n}", Math.Abs(Decimal.Round(TotalAccumulative, 2))),

                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[11].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[10].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[9].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[8].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[7].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[6].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[5].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[4].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[3].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[2].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[1].Accumulative, 2))) : "0",
                                            Account.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(Account.BalancePerMonthList[0].Accumulative, 2))) : "0",
                                            Account.ExpOrIncTypeName ?? "أخرى"
                                        );

                            AccumulativeMonth1 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[0].Accumulative, 2));
                            AccumulativeMonth2 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[1].Accumulative, 2));
                            AccumulativeMonth3 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[2].Accumulative, 2));
                            AccumulativeMonth4 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[3].Accumulative, 2));
                            AccumulativeMonth5 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[4].Accumulative, 2));
                            AccumulativeMonth6 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[5].Accumulative, 2));
                            AccumulativeMonth7 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[6].Accumulative, 2));
                            AccumulativeMonth8 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[7].Accumulative, 2));
                            AccumulativeMonth9 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[8].Accumulative, 2));
                            AccumulativeMonth10 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[9].Accumulative, 2));
                            AccumulativeMonth11 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[10].Accumulative, 2));
                            AccumulativeMonth12 += Math.Abs(Decimal.Round(Account.BalancePerMonthList[11].Accumulative, 2));
                            AccumulativeTotal += Math.Abs(Decimal.Round(TotalAccumulative, 2));
                            AccumulativeAverageTotal += Math.Abs(Decimal.Round(AverageAccumulative, 2));




                        }
                        AccumulativeMonth1String += String.Format("{0:n}", AccumulativeMonth1);
                        AccumulativeMonth2String += String.Format("{0:n}", AccumulativeMonth2);
                        AccumulativeMonth3String += String.Format("{0:n}", AccumulativeMonth3);
                        AccumulativeMonth4String += String.Format("{0:n}", AccumulativeMonth4);
                        AccumulativeMonth5String += String.Format("{0:n}", AccumulativeMonth5);
                        AccumulativeMonth6String += String.Format("{0:n}", AccumulativeMonth6);
                        AccumulativeMonth7String += String.Format("{0:n}", AccumulativeMonth7);
                        AccumulativeMonth8String += String.Format("{0:n}", AccumulativeMonth8);
                        AccumulativeMonth9String += String.Format("{0:n}", AccumulativeMonth9);
                        AccumulativeMonth10String += String.Format("{0:n}", AccumulativeMonth10);
                        AccumulativeMonth11String += String.Format("{0:n}", AccumulativeMonth11);
                        AccumulativeMonth12String += String.Format("{0:n}", AccumulativeMonth12);
                        AccumulativeTotalString += String.Format("{0:n}", AccumulativeTotal);
                        AccumulativeAverageTotalString += String.Format("{0:n}", AccumulativeAverageTotal);



                    }









                    PdfPTable tableHeading = new PdfPTable(3);

                    tableHeading.WidthPercentage = 100;

                    tableHeading.SetTotalWidth(new float[] { 33, 33, 33 });


                    PdfPCell cellPurchasePOInvoiceTypeIDName = new PdfPCell();
                    string cellPurchasePOInvoiceTypeIDNametext = " ";
                    cellPurchasePOInvoiceTypeIDName.Phrase = new Phrase("", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOInvoiceTypeIDName.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellPurchasePOInvoiceTypeIDName.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellPurchasePOInvoiceTypeIDName.BorderColor = BaseColor.WHITE;
                    cellPurchasePOInvoiceTypeIDName.PaddingBottom = 15;
                    cellPurchasePOInvoiceTypeIDName.PaddingTop = 15;
                    //  cellPurchasePOInvoiceTypeIDName.BackgroundColor = new BaseColor(4, 189, 189);

                    PdfPCell cellPurchasePOIDNumber = new PdfPCell();
                    string cellPurchasePOIDNumbertext = "تقرير حركة الحسابات السنويه " + Year;
                    cellPurchasePOIDNumbertext += "  للحسابات  :" + AccountsNameSTRList;
                    cellPurchasePOIDNumber.Phrase = new Phrase(cellPurchasePOIDNumbertext, fontExpenses);
                    iTextSharp.text.Font arabicFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOIDNumber.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellPurchasePOIDNumber.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellPurchasePOIDNumber.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    cellPurchasePOIDNumber.BorderColor = BaseColor.BLACK;
                    cellPurchasePOIDNumber.PaddingBottom = 8;
                    cellPurchasePOIDNumber.PaddingTop = 8;
                    cellPurchasePOIDNumber.BackgroundColor = new BaseColor(69, 168, 255);



                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);
                    tableHeading.AddCell(cellPurchasePOIDNumber);
                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);


                    tableHeading.KeepTogether = true;

                    tableHeading.SpacingAfter = 20;







                    PdfPTable table4 = new PdfPTable(4);

                    table4.WidthPercentage = 100;

                    table4.SetTotalWidth(new float[] { 55, 400, 100, 98 });


                    //Adding PdfPTable


                    PdfPTable table = new PdfPTable(dt2.Columns.Count);


                    //table Width
                    table.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    table.SetTotalWidth(new float[] { 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 60 });
                    table.PaddingTop = 20;

                    for (int i = 0; i < dt2.Columns.Count; i++)
                    {
                        string cellText = HttpUtility.HtmlDecode(dt2.Columns[i].ColumnName);
                        PdfPCell cell = new PdfPCell();
                        cell.Phrase = new Phrase(cellText, font);
                        iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                        cell.PaddingBottom = 5;
                        table.AddCell(cell);


                    }


                    //writing table Data
                    for (int i = 0; i < dt2.Rows.Count; i++)
                    {
                        for (int j = 0; j < dt2.Columns.Count; j++)
                        {
                            //table.AddCell(dt.Rows[i][j].ToString());
                            PdfPCell cell = new PdfPCell();

                            if (j == 14)
                            {
                                cell.BackgroundColor = new BaseColor(196, 196, 196);
                            }
                            cell.ArabicOptions = 1;
                            if (j <= 5)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.Padding = 8;
                            }
                            else if (j >= 9)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }
                            else
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }

                            if (cell.ArabicOptions == 1)
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                cell.Padding = 8;

                            }
                            else
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                cell.Padding = 8;

                            }

                            cell.Phrase = new Phrase(1, dt2.Rows[i][j].ToString(), font);
                            //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                            table.AddCell(cell);

                        }

                    }






                    #region

                    #endregion


                    PdfPTable tableExpenses = new PdfPTable(4);

                    tableExpenses.WidthPercentage = 100;

                    tableExpenses.SetTotalWidth(new float[] { 80, 30, 30, 17 });


                    PdfPCell CellExpenses1 = new PdfPCell();
                    string CellExpenses1Text = "";
                    CellExpenses1.Phrase = new Phrase(CellExpenses1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontNi = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellExpenses1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpenses1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpenses1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellExpenses2 = new PdfPCell();
                    string CellExpenses2text2 = "";
                    CellExpenses2.Phrase = new Phrase(CellExpenses2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellExpenses2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellExpenses2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpenses2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellExpenses3 = new PdfPCell();
                    string CellExpenses3text = " ";
                    CellExpenses3.Phrase = new Phrase(CellExpenses3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#0007EE"))));
                    iTextSharp.text.Font arabicFontWss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellExpenses3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpenses3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpenses3.BorderColor = BaseColor.WHITE;





                    PdfPCell CellExpenses4 = new PdfPCell();
                    string CellExpenses4text = "المصروفات";
                    CellExpenses4.Phrase = new Phrase(CellExpenses4text, fontExpenses);
                    CellExpenses4.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellExpenses4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpenses4.BorderColor = BaseColor.BLACK;
                    CellExpenses4.BackgroundColor = new BaseColor(145, 1, 1);
                    CellExpenses4.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    CellExpenses4.PaddingBottom = 6;
                    CellExpenses4.PaddingTop = 6;





                    tableExpenses.AddCell(CellExpenses1);
                    tableExpenses.AddCell(CellExpenses2);
                    tableExpenses.AddCell(CellExpenses3);
                    tableExpenses.AddCell(CellExpenses4);



                    PdfPTable tableIncomes = new PdfPTable(4);

                    tableIncomes.WidthPercentage = 100;

                    tableIncomes.SetTotalWidth(new float[] { 80, 30, 30, 17 });


                    PdfPCell CellIncomes1 = new PdfPCell();
                    string CellIncomes1Text = "";
                    CellIncomes1.Phrase = new Phrase(CellIncomes1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomes1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomes1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomes1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellIncomes2 = new PdfPCell();
                    string CellIncomes2text2 = "";
                    CellIncomes2.Phrase = new Phrase(CellIncomes2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellIncomes2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellIncomes2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomes2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellIncomes3 = new PdfPCell();
                    string CellIncomes3text = " ";
                    CellIncomes3.Phrase = new Phrase(CellIncomes3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#0007EE"))));
                    CellIncomes3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomes3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomes3.BorderColor = BaseColor.WHITE;





                    PdfPCell CellIncomes4 = new PdfPCell();
                    string CellIncomes4text = "الايرادات";
                    CellIncomes4.Phrase = new Phrase(CellIncomes4text, fontExpenses);
                    CellIncomes4.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomes4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomes4.BorderColor = BaseColor.BLACK;
                    CellIncomes4.BackgroundColor = new BaseColor(148, 139, 82);
                    CellIncomes4.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    CellIncomes4.PaddingBottom = 6;
                    CellIncomes4.PaddingTop = 6;




                    // Total Incomes By Month



                    PdfPTable tableIncomeTotal = new PdfPTable(15);


                    //table Width
                    tableIncomeTotal.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    tableIncomeTotal.SetTotalWidth(new float[] { 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 60 });




                    PdfPCell CellIncomesTotal1 = new PdfPCell();
                    string CellIncomesTotal1Text = AccumulativeMonth1String;
                    CellIncomesTotal1.Phrase = new Phrase(CellIncomesTotal1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal1.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal1.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotal2 = new PdfPCell();
                    string CellIncomesTotal2text2 = AccumulativeMonth2String;
                    CellIncomesTotal2.Phrase = new Phrase(CellIncomesTotal2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellIncomesTotal2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellIncomesTotal2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal2.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal2.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotal3 = new PdfPCell();
                    string CellIncomesTotal3text = AccumulativeMonth3String;
                    CellIncomesTotal3.Phrase = new Phrase(CellIncomesTotal3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal3.BorderColor = BaseColor.BLACK;

                    CellIncomesTotal3.BackgroundColor = new BaseColor(196, 196, 196);



                    PdfPCell CellIncomesTotal4 = new PdfPCell();
                    string CellIncomesTotal4text = AccumulativeMonth4String;
                    CellIncomesTotal4.Phrase = new Phrase(CellIncomesTotal4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal4.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomesTotal4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal4.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal4.PaddingBottom = 6;
                    CellIncomesTotal4.PaddingTop = 6;
                    CellIncomesTotal4.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotal5 = new PdfPCell();
                    string CellIncomesTotal5Text = AccumulativeMonth5String;
                    CellIncomesTotal5.Phrase = new Phrase(CellIncomesTotal5Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal5.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal5.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal5.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal5.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellIncomesTotal6 = new PdfPCell();
                    string CellIncomesTotal6text6 = AccumulativeMonth6String;
                    CellIncomesTotal6.Phrase = new Phrase(CellIncomesTotal6text6, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellIncomesTotal6.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellIncomesTotal6.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal6.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal6.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotal7 = new PdfPCell();
                    string CellIncomesTotal7text = AccumulativeMonth7String;
                    CellIncomesTotal7.Phrase = new Phrase(CellIncomesTotal7text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal7.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal7.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal7.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal7.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellIncomesTotal8 = new PdfPCell();
                    string CellIncomesTotal8text = AccumulativeMonth8String;
                    CellIncomesTotal8.Phrase = new Phrase(CellIncomesTotal8text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal8.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomesTotal8.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal8.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal8.PaddingBottom = 6;
                    CellIncomesTotal8.PaddingTop = 6;
                    CellIncomesTotal8.BackgroundColor = new BaseColor(196, 196, 196);






                    PdfPCell CellIncomesTotal9 = new PdfPCell();
                    string CellIncomesTotal9Text = AccumulativeMonth9String;
                    CellIncomesTotal9.Phrase = new Phrase(CellIncomesTotal9Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal9.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal9.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal9.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal9.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellIncomesTotal10 = new PdfPCell();
                    string CellIncomesTotal10text10 = AccumulativeMonth10String;
                    CellIncomesTotal10.Phrase = new Phrase(CellIncomesTotal10text10, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellIncomesTotal10.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellIncomesTotal10.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal10.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal10.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotal11 = new PdfPCell();
                    string CellIncomesTotal11text = AccumulativeMonth11String;
                    CellIncomesTotal11.Phrase = new Phrase(CellIncomesTotal11text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal11.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellIncomesTotal11.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal11.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal11.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellIncomesTotal12 = new PdfPCell();
                    string CellIncomesTotal12text = AccumulativeMonth12String;
                    CellIncomesTotal12.Phrase = new Phrase(CellIncomesTotal12text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotal12.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomesTotal12.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotal12.BorderColor = BaseColor.BLACK;
                    CellIncomesTotal12.PaddingBottom = 6;
                    CellIncomesTotal12.PaddingTop = 6;
                    CellIncomesTotal12.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotalSum = new PdfPCell();
                    string CellIncomesTotalSumtext = AccumulativeTotalString;
                    CellIncomesTotalSum.Phrase = new Phrase(CellIncomesTotalSumtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotalSum.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomesTotalSum.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotalSum.BorderColor = BaseColor.BLACK;
                    CellIncomesTotalSum.PaddingBottom = 6;
                    CellIncomesTotalSum.PaddingTop = 6;
                    CellIncomesTotalSum.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellIncomesTotalAverage = new PdfPCell();
                    string CellIncomesTotalAveragetext = AccumulativeAverageTotalString;
                    CellIncomesTotalAverage.Phrase = new Phrase(CellIncomesTotalAveragetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellIncomesTotalAverage.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellIncomesTotalAverage.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellIncomesTotalAverage.BorderColor = BaseColor.BLACK;
                    CellIncomesTotalAverage.PaddingBottom = 6;
                    CellIncomesTotalAverage.PaddingTop = 6;
                    CellIncomesTotalAverage.BackgroundColor = new BaseColor(196, 196, 196);






                    PdfPCell CellTotalIncomeString = new PdfPCell();
                    string CellTotalIncomeStringtext = "اجمالى الايرادات";
                    CellTotalIncomeString.Phrase = new Phrase(CellTotalIncomeStringtext, font);
                    CellTotalIncomeString.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellTotalIncomeString.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTotalIncomeString.BorderColor = BaseColor.BLACK;
                    CellTotalIncomeString.PaddingBottom = 6;
                    CellTotalIncomeString.PaddingTop = 6;
                    CellTotalIncomeString.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    CellTotalIncomeString.BackgroundColor = new BaseColor(196, 196, 196);



                    tableIncomeTotal.AddCell(CellIncomesTotalAverage);
                    tableIncomeTotal.AddCell(CellIncomesTotalSum);
                    tableIncomeTotal.AddCell(CellIncomesTotal12);
                    tableIncomeTotal.AddCell(CellIncomesTotal11);
                    tableIncomeTotal.AddCell(CellIncomesTotal10);
                    tableIncomeTotal.AddCell(CellIncomesTotal9);
                    tableIncomeTotal.AddCell(CellIncomesTotal8);
                    tableIncomeTotal.AddCell(CellIncomesTotal7);
                    tableIncomeTotal.AddCell(CellIncomesTotal6);
                    tableIncomeTotal.AddCell(CellIncomesTotal5);
                    tableIncomeTotal.AddCell(CellIncomesTotal4);
                    tableIncomeTotal.AddCell(CellIncomesTotal3);
                    tableIncomeTotal.AddCell(CellIncomesTotal2);
                    tableIncomeTotal.AddCell(CellIncomesTotal1);

                    tableIncomeTotal.AddCell(CellTotalIncomeString);




                    var dtExpenses = new System.Data.DataTable("grid");


                    dtExpenses.Columns.AddRange(new DataColumn[15] {
                                                      new DataColumn("المتوسط") ,
                                                      new DataColumn("الاجمالى"),
                                                      new DataColumn("ديسمبر"),
                                                      new DataColumn("نوفمبر"),
                                                      new DataColumn("اكتوبر") ,
                                                      new DataColumn("سبتمبر"),
                                                      new DataColumn("اغسطس"),
                                                      new DataColumn("يوليو") ,
                                                      new DataColumn("يونيو"),
                                                      new DataColumn("مايو"),
                                                      new DataColumn("ابريل"),
                                                      new DataColumn("مارس") ,
                                                      new DataColumn("فبراير"),
                                                      new DataColumn("يناير"),
                                                      new DataColumn("البيان / الشهر")
                    });









                    decimal AccumulativeExpensesMonth1 = 0;
                    decimal AccumulativeExpensesMonth2 = 0;
                    decimal AccumulativeExpensesMonth3 = 0;
                    decimal AccumulativeExpensesMonth4 = 0;
                    decimal AccumulativeExpensesMonth5 = 0;
                    decimal AccumulativeExpensesMonth6 = 0;
                    decimal AccumulativeExpensesMonth7 = 0;
                    decimal AccumulativeExpensesMonth8 = 0;
                    decimal AccumulativeExpensesMonth9 = 0;
                    decimal AccumulativeExpensesMonth10 = 0;
                    decimal AccumulativeExpensesMonth11 = 0;
                    decimal AccumulativeExpensesMonth12 = 0;
                    decimal AccumulativeExpensesTotal = 0;
                    decimal AccumulativeExpensesAverageTotal = 0;

                    string AccumulativeExpensesMonth1String = "";
                    string AccumulativeExpensesMonth2String = "";
                    string AccumulativeExpensesMonth3String = "";
                    string AccumulativeExpensesMonth4String = "";
                    string AccumulativeExpensesMonth5String = "";
                    string AccumulativeExpensesMonth6String = "";
                    string AccumulativeExpensesMonth7String = "";
                    string AccumulativeExpensesMonth8String = "";
                    string AccumulativeExpensesMonth9String = "";
                    string AccumulativeExpensesMonth10String = "";
                    string AccumulativeExpensesMonth11String = "";
                    string AccumulativeExpensesMonth12String = "";
                    string AccumulativeExpensesTotalString = "";
                    string AccumulativeExpensesAverageTotalString = "";



                    //Start Expenses



                    if (AccountBalancePerExpensesList != null)
                    {
                        foreach (var ExpensesAccounts in AccountBalancePerExpensesList)
                        {

                            decimal TotalAccumulative = ExpensesAccounts.BalancePerMonthList.Sum(x => x.Accumulative);
                            decimal AverageAccumulative = TotalAccumulative / NumberMonthDivided;


                            dtExpenses.Rows.Add(


                String.Format("{0:n}", Math.Abs(Decimal.Round(AverageAccumulative, 2))),
                String.Format("{0:n}", Math.Abs(Decimal.Round(TotalAccumulative, 2))),
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[11].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[10].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[9].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[8].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[7].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[6].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[5].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[4].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[3].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[2].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[1].Accumulative, 2))) : "0",
                ExpensesAccounts.BalancePerMonthList != null ? String.Format("{0:n}", Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[0].Accumulative, 2))) : "0",
                ExpensesAccounts.ExpOrIncTypeName ?? "أخرى"
                                      );

                            AccumulativeExpensesMonth1 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[0].Accumulative, 2));
                            AccumulativeExpensesMonth2String += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[1].Accumulative, 2));
                            AccumulativeExpensesMonth3 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[2].Accumulative, 2));
                            AccumulativeExpensesMonth4 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[3].Accumulative, 2));
                            AccumulativeExpensesMonth5 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[4].Accumulative, 2));
                            AccumulativeExpensesMonth6 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[5].Accumulative, 2));
                            AccumulativeExpensesMonth7 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[6].Accumulative, 2));
                            AccumulativeExpensesMonth8 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[7].Accumulative, 2));
                            AccumulativeExpensesMonth9 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[8].Accumulative, 2));
                            AccumulativeExpensesMonth10 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[9].Accumulative, 2));
                            AccumulativeExpensesMonth11 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[10].Accumulative, 2));
                            AccumulativeExpensesMonth12 += Math.Abs(Decimal.Round(ExpensesAccounts.BalancePerMonthList[11].Accumulative, 2));
                            AccumulativeExpensesTotal += Math.Abs(Decimal.Round(TotalAccumulative, 2));
                            AccumulativeExpensesAverageTotal += Math.Abs(Decimal.Round(AverageAccumulative, 2));







                        }


                        AccumulativeExpensesMonth1String = String.Format("{0:n}", AccumulativeExpensesMonth1);
                        AccumulativeExpensesMonth2String = String.Format("{0:n}", AccumulativeExpensesMonth2);
                        AccumulativeExpensesMonth3String = String.Format("{0:n}", AccumulativeExpensesMonth3);
                        AccumulativeExpensesMonth4String = String.Format("{0:n}", AccumulativeExpensesMonth4);
                        AccumulativeExpensesMonth5String = String.Format("{0:n}", AccumulativeExpensesMonth5);
                        AccumulativeExpensesMonth6String = String.Format("{0:n}", AccumulativeExpensesMonth6);
                        AccumulativeExpensesMonth7String = String.Format("{0:n}", AccumulativeExpensesMonth7);
                        AccumulativeExpensesMonth8String = String.Format("{0:n}", AccumulativeExpensesMonth8);
                        AccumulativeExpensesMonth9String = String.Format("{0:n}", AccumulativeExpensesMonth9);
                        AccumulativeExpensesMonth10String = String.Format("{0:n}", AccumulativeExpensesMonth10);
                        AccumulativeExpensesMonth11String = String.Format("{0:n}", AccumulativeExpensesMonth11);
                        AccumulativeExpensesMonth12String = String.Format("{0:n}", AccumulativeExpensesMonth12);
                        AccumulativeExpensesTotalString = String.Format("{0:n}", AccumulativeExpensesTotal);
                        AccumulativeExpensesAverageTotalString = String.Format("{0:n}", AccumulativeExpensesAverageTotal);



                    }









                    PdfPTable MaintableExpenses = new PdfPTable(dtExpenses.Columns.Count);


                    //table Width
                    MaintableExpenses.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    MaintableExpenses.SetTotalWidth(new float[] { 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 60 });
                    MaintableExpenses.PaddingTop = 20;

                    for (int i = 0; i < dtExpenses.Columns.Count; i++)
                    {
                        string cellText = HttpUtility.HtmlDecode(dtExpenses.Columns[i].ColumnName);
                        PdfPCell cell = new PdfPCell();
                        cell.Phrase = new Phrase(cellText, font);
                        iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                        cell.PaddingBottom = 5;
                        MaintableExpenses.AddCell(cell);


                    }


                    //writing table Data
                    for (int i = 0; i < dtExpenses.Rows.Count; i++)
                    {
                        for (int j = 0; j < dtExpenses.Columns.Count; j++)
                        {
                            //table.AddCell(dt.Rows[i][j].ToString());
                            PdfPCell cell = new PdfPCell();

                            if (j == 14)
                            {
                                cell.BackgroundColor = new BaseColor(196, 196, 196);
                            }
                            cell.ArabicOptions = 1;
                            if (j <= 5)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.Padding = 8;
                            }
                            else if (j >= 9)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }
                            else
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }

                            if (cell.ArabicOptions == 1)
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                cell.Padding = 8;

                            }
                            else
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                cell.Padding = 8;

                            }

                            cell.Phrase = new Phrase(1, dtExpenses.Rows[i][j].ToString(), font);
                            //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                            MaintableExpenses.AddCell(cell);

                        }

                    }


                    // Total Incomes By Month



                    PdfPTable tableExpensesTotal = new PdfPTable(15);


                    //table Width
                    tableExpensesTotal.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    tableExpensesTotal.SetTotalWidth(new float[] { 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 60 });




                    PdfPCell CellExpensesTotal1 = new PdfPCell();
                    string CellExpensesTotal1Text = AccumulativeExpensesMonth1String;
                    CellExpensesTotal1.Phrase = new Phrase(CellExpensesTotal1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal1.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal1.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellExpensesTotal2 = new PdfPCell();
                    string CellExpensesTotal2text2 = AccumulativeExpensesMonth2String;
                    CellExpensesTotal2.Phrase = new Phrase(CellExpensesTotal2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellExpensesTotal2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellExpensesTotal2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal2.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal2.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellExpensesTotal3 = new PdfPCell();
                    string CellExpensesTotal3text = AccumulativeExpensesMonth3String;
                    CellExpensesTotal3.Phrase = new Phrase(CellExpensesTotal3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal3.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal3.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellExpensesTotal4 = new PdfPCell();
                    string CellExpensesTotal4text = AccumulativeExpensesMonth4String;
                    CellExpensesTotal4.Phrase = new Phrase(CellExpensesTotal4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal4.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellExpensesTotal4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal4.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal4.PaddingBottom = 6;
                    CellExpensesTotal4.PaddingTop = 6;
                    CellExpensesTotal4.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellExpensesTotal5 = new PdfPCell();
                    string CellExpensesTotal5Text = AccumulativeExpensesMonth5String;
                    CellExpensesTotal5.Phrase = new Phrase(CellExpensesTotal5Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal5.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal5.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal5.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal5.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellExpensesTotal6 = new PdfPCell();
                    string CellExpensesTotal6text6 = AccumulativeExpensesMonth6String;
                    CellExpensesTotal6.Phrase = new Phrase(CellExpensesTotal6text6, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellExpensesTotal6.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellExpensesTotal6.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal6.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal6.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellExpensesTotal7 = new PdfPCell();
                    string CellExpensesTotal7text = AccumulativeExpensesMonth7String;
                    CellExpensesTotal7.Phrase = new Phrase(CellExpensesTotal7text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal7.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal7.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal7.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal7.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellExpensesTotal8 = new PdfPCell();
                    string CellExpensesTotal8text = AccumulativeExpensesMonth8String;
                    CellExpensesTotal8.Phrase = new Phrase(CellExpensesTotal8text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal8.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellExpensesTotal8.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal8.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal8.PaddingBottom = 6;
                    CellExpensesTotal8.PaddingTop = 6;
                    CellExpensesTotal8.BackgroundColor = new BaseColor(196, 196, 196);






                    PdfPCell CellExpensesTotal9 = new PdfPCell();
                    string CellExpensesTotal9Text = AccumulativeExpensesMonth9String;
                    CellExpensesTotal9.Phrase = new Phrase(CellExpensesTotal9Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal9.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal9.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal9.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal9.BackgroundColor = new BaseColor(196, 196, 196);

                    PdfPCell CellExpensesTotal10 = new PdfPCell();
                    string CellExpensesTotal10text10 = AccumulativeExpensesMonth10String;
                    CellExpensesTotal10.Phrase = new Phrase(CellExpensesTotal10text10, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellExpensesTotal10.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellExpensesTotal10.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal10.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal10.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellExpensesTotal11 = new PdfPCell();
                    string CellExpensesTotal11text = AccumulativeExpensesMonth11String;
                    CellExpensesTotal11.Phrase = new Phrase(CellExpensesTotal11text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal11.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal11.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal11.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal11.BackgroundColor = new BaseColor(196, 196, 196);


                    PdfPCell CellExpensesTotal12 = new PdfPCell();
                    string CellExpensesTotal12text = AccumulativeExpensesMonth12String;
                    CellExpensesTotal12.Phrase = new Phrase(CellExpensesTotal12text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotal12.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotal12.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotal12.BorderColor = BaseColor.BLACK;
                    CellExpensesTotal12.BackgroundColor = new BaseColor(196, 196, 196);




                    PdfPCell CellExpensesTotalSum = new PdfPCell();
                    string CellExpensesTotalSumtext = AccumulativeExpensesTotalString;
                    CellExpensesTotalSum.Phrase = new Phrase(CellExpensesTotalSumtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesTotalSum.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesTotalSum.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesTotalSum.BorderColor = BaseColor.BLACK;
                    CellExpensesTotalSum.BackgroundColor = new BaseColor(196, 196, 196);



                    PdfPCell CellExpensesAverageTotal = new PdfPCell();
                    string CellExpensesAverageTotaltext = AccumulativeExpensesAverageTotalString;
                    CellExpensesAverageTotal.Phrase = new Phrase(CellExpensesAverageTotaltext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellExpensesAverageTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellExpensesAverageTotal.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellExpensesAverageTotal.BorderColor = BaseColor.BLACK;
                    CellExpensesAverageTotal.BackgroundColor = new BaseColor(196, 196, 196);







                    PdfPCell CellTotalExpensesString = new PdfPCell();
                    string CellTotalExpensesStringtext = "اجمالى المصروفات";
                    CellTotalExpensesString.Phrase = new Phrase(CellTotalExpensesStringtext, font);
                    CellTotalExpensesString.HorizontalAlignment = Element.ALIGN_CENTER;
                    CellTotalExpensesString.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTotalExpensesString.BorderColor = BaseColor.BLACK;
                    CellTotalExpensesString.PaddingBottom = 6;
                    CellTotalExpensesString.PaddingTop = 6;
                    CellTotalExpensesString.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    CellTotalExpensesString.BackgroundColor = new BaseColor(196, 196, 196);


                    tableExpensesTotal.AddCell(CellExpensesAverageTotal);
                    tableExpensesTotal.AddCell(CellExpensesTotalSum);
                    tableExpensesTotal.AddCell(CellExpensesTotal12);
                    tableExpensesTotal.AddCell(CellExpensesTotal11);
                    tableExpensesTotal.AddCell(CellExpensesTotal10);
                    tableExpensesTotal.AddCell(CellExpensesTotal9);
                    tableExpensesTotal.AddCell(CellExpensesTotal8);
                    tableExpensesTotal.AddCell(CellExpensesTotal7);
                    tableExpensesTotal.AddCell(CellExpensesTotal6);
                    tableExpensesTotal.AddCell(CellExpensesTotal5);
                    tableExpensesTotal.AddCell(CellExpensesTotal4);
                    tableExpensesTotal.AddCell(CellExpensesTotal3);
                    tableExpensesTotal.AddCell(CellExpensesTotal2);
                    tableExpensesTotal.AddCell(CellExpensesTotal1);

                    tableExpensesTotal.AddCell(CellTotalExpensesString);









                    tableIncomes.AddCell(CellIncomes1);
                    tableIncomes.AddCell(CellIncomes2);
                    tableIncomes.AddCell(CellIncomes3);
                    tableIncomes.AddCell(CellIncomes4);

                    document.Add(tableHeading);
                    document.Add(table4);
                    document.Add(tableIncomes);
                    document.Add(table);
                    document.Add(tableIncomeTotal);
                    document.Add(tableExpenses);
                    tableExpenses.SpacingBefore = 10;
                    document.Add(MaintableExpenses);
                    document.Add(tableExpensesTotal);

                    // table.SpacingAfter = 15;
                    //  document.Add(tablTotals);
                    //  tablTotals.SpacingAfter = 200;


                    document.Close();
                    byte[] result = ms.ToArray();
                    ms = new MemoryStream();
                    ms.Write(result, 0, result.Length);
                    ms.Position = 0;


                    var CompanyName = companyname.ToLower();

                    string FullFileName = DateTime.Now.ToFileTime() + "_" + "AccountAndFinanceIncome.pdf";
                    string PathsTR = "Attachments/" + CompanyName + "/";
                    String Filepath = Path.Combine(_host.WebRootPath, PathsTR);
                    string p_strPath = Path.Combine(Filepath, FullFileName);

                    System.IO.File.WriteAllBytes(p_strPath, result);

                    Response.Message = Globals.baseURL + '/' + PathsTR + FullFileName;
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

        public async Task<BaseMessageResponse> JournalEntryPDFReport(string companyname, [FromHeader] long DailyJournalEntryID = 0)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    //long DailyJournalEntryID = 0;
                    if (DailyJournalEntryID == 0)

                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err169";
                        error.ErrorMSG = "Invalid Daily JournalEntry ID";
                        Response.Errors.Add(error);
                        return Response;
                    }



                    var dt = new System.Data.DataTable("grid");


                    dt.Columns.AddRange(new DataColumn[5] { new DataColumn("No"),
                                 new DataColumn("From / To Account"),
                                 new DataColumn("Credit"),
                                 new DataColumn("Debit") ,
                                 new DataColumn("Other"),

                    });
                    var Counter = 1;






                    MemoryStream ms = new MemoryStream();

                    //Size of page
                    iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate());


                    PdfWriter pw = PdfWriter.GetInstance(document, ms);

                    //Call the footer Function

                    pw.PageEvent = new HeaderFooter();

                    document.Open();


                    //Handle fonts and Sizes +  Attachments images logos 

                    iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.A4);



                    BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font font = new Font(bf, 9, Font.NORMAL);

                    String path = Path.Combine(_host.WebRootPath, "Attachments/");

                    if (companyname.ToString() == "marinaplt")
                    {
                        string PDFp_strPath2 = Path.Combine(path, "logoMarina.png");

                        if (System.IO.File.Exists(PDFp_strPath2))
                        {
                            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath2);
                            jpg.SetAbsolutePosition(60f, 550f);
                            //document.Add(logo);
                            document.Add(jpg);
                        }
                        iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                        Paragraph prgHeading = new Paragraph();
                        prgHeading.Alignment = Element.ALIGN_RIGHT;
                        prgHeading.SpacingBefore = -10;
                        prgHeading.SpacingAfter = 20;


                        Chunk cc = new Chunk("Journal Entry Report".ToUpper() + " ", fntHead);
                        cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                        prgHeading.Add(cc);

                        document.Add(prgHeading);

                    }
                    else if (companyname.ToString() == "piaroma")
                    {
                        string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                        if (System.IO.File.Exists(Piaroma_p_strPath))
                        {

                            Image logo = Image.GetInstance(Piaroma_p_strPath);
                            logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                            logo.ScaleAbsolute(300f, 300f);

                            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                            //document.Add(logo);
                            document.Add(jpg);
                        }
                        iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                        Paragraph prgHeading = new Paragraph();
                        prgHeading.Alignment = Element.ALIGN_RIGHT;
                        prgHeading.SpacingBefore = -10;
                        prgHeading.SpacingAfter = 20;

                        Chunk cc = new Chunk("Journal Entry Report".ToUpper() + " ", fntHead);
                        cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                        prgHeading.Add(cc);

                        document.Add(prgHeading);
                    }
                    else if (companyname.ToString() == "Garastest")
                    {
                        string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                        Image logo = Image.GetInstance(Piaroma_p_strPath);
                        logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                        logo.ScaleAbsolute(300f, 300f);

                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                        //document.Add(logo);
                        document.Add(jpg);
                        iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                        Paragraph prgHeading = new Paragraph();
                        prgHeading.Alignment = Element.ALIGN_RIGHT;
                        prgHeading.SpacingBefore = -10;
                        prgHeading.SpacingAfter = 20;

                        Chunk cc = new Chunk("Journal Entry Report".ToUpper() + " ", fntHead);
                        cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                        prgHeading.Add(cc);

                        document.Add(prgHeading);
                    }



                    var JournalEntryList =await GetDailyJournalEntryDataInfo(DailyJournalEntryID, true);

                    if (JournalEntryList.Data?.EntryAccountList != null)
                    {
                        foreach (var item in JournalEntryList.Data.EntryAccountList)
                        {

                            dt.Rows.Add(
                                Counter.ToString(),
                                item.AccountName != null ? item.AccountName + " " + "(" + item.CategoryName + " " + "/" + " " + item.AccountTypeName + ")" : "-",
                                item.AccountTypeName == "Credit" && item.SignOfAccount == "plus" || item.AccountTypeName == "Debit" && item.SignOfAccount == "minus" ? Decimal.Round(decimal.Parse(string.Format("{0:0.00}", item.Amount)), 1) : 0,
                                item.AccountTypeName == "Debit" && item.SignOfAccount == "plus" || item.AccountTypeName == "Credit" && item.SignOfAccount == "minus" ? Decimal.Round(decimal.Parse(string.Format("{0:0.00}", item.Amount)), 1) : 0,
                                item.SupplierName != null ? item.SupplierName + item.PurchasePOName : item.ClinetName != null ? item.ClinetName + item.ProjectName : "N/A"

                                );
                            Counter++;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "This Journal Entry Is Empty";
                        Response.Errors.Add(error);
                    }






                    //Adding paragraph for report generated by  
                    Paragraph prgGeneratedBY = new Paragraph();
                    BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                    prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                    document.Add(prgGeneratedBY);




                    PdfPTable tableHeading = new PdfPTable(4);

                    tableHeading.WidthPercentage = 100;

                    tableHeading.SetTotalWidth(new float[] { 150, 400, 80, 80 });


                    PdfPCell cell3 = new PdfPCell();
                    string cell3text = "";
                    string PDFp_strPath = Path.Combine(path, "logoMarina.png");
                    if (System.IO.File.Exists(PDFp_strPath))
                    {
                        cell3.AddElement(iTextSharp.text.Image.GetInstance(PDFp_strPath));
                    }
                    cell3.Phrase = new Phrase("", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    cell3.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell3.BorderColor = new BaseColor(4, 189, 189);
                    cell3.PaddingBottom = 15;
                    cell3.PaddingTop = 15;
                    cell3.BackgroundColor = new BaseColor(4, 189, 189);

                    PdfPCell cell4 = new PdfPCell();
                    string cell4text = "Journal Entry Number " + DailyJournalEntryID;
                    cell4.Phrase = new Phrase(cell4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    cell4.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell4.BorderColor = new BaseColor(4, 189, 189);
                    cell4.PaddingBottom = 15;
                    cell4.PaddingTop = 15;
                    cell4.BackgroundColor = new BaseColor(4, 189, 189);



                    tableHeading.AddCell(cell3);
                    tableHeading.AddCell(cell4);
                    tableHeading.AddCell(cell3);
                    tableHeading.AddCell(cell3);

                    tableHeading.KeepTogether = true;



                    PdfPTable table2 = new PdfPTable(4);

                    table2.WidthPercentage = 100;

                    table2.SetTotalWidth(new float[] { 50, 200, 150, 100 });


                    PdfPCell cellRefDocument = new PdfPCell();
                    string cellRefDocumenttext = "Ref. Doc: ";
                    cellRefDocument.Phrase = new Phrase(cellRefDocumenttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    cellRefDocument.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellRefDocument.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellRefDocument.BorderColor = BaseColor.WHITE;
                    cellRefDocument.PaddingTop = 5;


                    PdfPCell cellDocumentNumber = new PdfPCell();
                    string cellDocumentNumbertext = JournalEntryList.Data?.DocumentNumber != null ? JournalEntryList.Data?.DocumentNumber : "N/A";
                    cellDocumentNumber.Phrase = new Phrase(cellDocumentNumbertext, font);


                    cellDocumentNumber.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    cellDocumentNumber.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cellDocumentNumber.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellDocumentNumber.BorderColor = BaseColor.WHITE;
                    cellDocumentNumber.PaddingTop = 5;


                    PdfPCell cellDescription = new PdfPCell();
                    string cellDescriptiontext = "Description";
                    cellDescription.Phrase = new Phrase(cellDescriptiontext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont6 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    cellDescription.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cellDescription.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellDescription.BorderColor = BaseColor.WHITE;
                    cellDescription.PaddingTop = 5;


                    PdfPCell cellDescriptionJournal = new PdfPCell();
                    string cellDescriptionJournaltext = JournalEntryList.Data?.Description;
                    cellDescriptionJournal.Phrase = new Phrase(cellDescriptionJournaltext, font);
                    cellDescriptionJournal.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellDescriptionJournal.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellDescriptionJournal.BorderColor = BaseColor.WHITE;
                    cellDescriptionJournal.PaddingTop = 5;
                    cellDescriptionJournal.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    table2.AddCell(cellRefDocument);
                    table2.AddCell(cellDocumentNumber);
                    table2.AddCell(cellDescription);
                    table2.AddCell(cellDescriptionJournal);


                    PdfPCell CellEntryDate = new PdfPCell();
                    string CellEntryDatetext = "Entry Date: ";
                    CellEntryDate.Phrase = new Phrase(CellEntryDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontD = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellEntryDate.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellEntryDate.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellEntryDate.BorderColor = BaseColor.WHITE;
                    CellEntryDate.PaddingTop = 5;


                    PdfPCell CellEntryDateData = new PdfPCell();
                    string CellEntryDateDatatext = JournalEntryList.Data?.EntryDate.ToString();


                    CellEntryDateData.Phrase = new Phrase(CellEntryDateDatatext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont4s = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellEntryDateData.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellEntryDateData.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellEntryDateData.BorderColor = BaseColor.WHITE;
                    CellEntryDateData.PaddingTop = 5;
                    CellEntryDateData.Bottom = 5;


                    PdfPCell CellCreationDate = new PdfPCell();
                    string CellCreationDatetext = "Creation Date";
                    CellCreationDate.Phrase = new Phrase(CellCreationDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontt = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellCreationDate.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellCreationDate.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellCreationDate.BorderColor = BaseColor.WHITE;
                    CellCreationDate.PaddingTop = 5;
                    CellCreationDate.Bottom = 5;


                    PdfPCell CellCreationDateData = new PdfPCell();
                    string CellCreationDateDataText = JournalEntryList.Data?.CreationDate.ToString();

                    CellCreationDateData.Phrase = new Phrase(CellCreationDateDataText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont78 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellCreationDateData.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellCreationDateData.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellCreationDateData.BorderColor = BaseColor.WHITE;
                    CellCreationDateData.PaddingTop = 5;
                    CellCreationDateData.Bottom = 5;

                    table2.AddCell(CellEntryDate);
                    table2.AddCell(CellEntryDateData);
                    table2.AddCell(CellCreationDate);
                    table2.AddCell(CellCreationDateData);



                    table2.SpacingAfter = 20;


                    PdfPTable table4 = new PdfPTable(4);

                    table4.WidthPercentage = 100;

                    table4.SetTotalWidth(new float[] { 55, 100, 200, 400 });


                    //Adding PdfPTable


                    PdfPTable table = new PdfPTable(dt.Columns.Count);







                    //table Width
                    table.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    table.SetTotalWidth(new float[] { 4, 50, 25, 20, 24 });
                    table.SpacingAfter = 20;

                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        string cellText = HttpUtility.HtmlDecode(dt.Columns[i].ColumnName);
                        PdfPCell cell = new PdfPCell();
                        cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                        iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));

                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;





                        cell.PaddingBottom = 5;
                        table.AddCell(cell);


                    }


                    //writing table Data
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            //table.AddCell(dt.Rows[i][j].ToString());
                            PdfPCell cell = new PdfPCell();
                            cell.ArabicOptions = 1;
                            if (j <= 2)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.Padding = 8;
                            }
                            cell.ArabicOptions = 1;
                            if (j == 3)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }
                            if (j == 4)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }
                            if (j == 5)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.Padding = 8;
                            }
                            if (j == 7)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }


                            if (cell.ArabicOptions == 1)
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                cell.Padding = 8;

                            }
                            else
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                cell.Padding = 8;

                            }

                            cell.Phrase = new Phrase(1, dt.Rows[i][j].ToString(), font);

                            table.AddCell(cell);

                        }

                    }







                    PdfPTable table3 = new PdfPTable(4);

                    table3.WidthPercentage = 100;

                    table3.SetTotalWidth(new float[] { 55, 100, 200, 400 });


                    PdfPCell CellDirector = new PdfPCell();
                    string CellDirectorText = "";
                    CellDirector.Phrase = new Phrase(CellDirectorText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontNi = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellDirector.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDirector.BorderColor = BaseColor.WHITE;


                    PdfPCell CellDirector2 = new PdfPCell();
                    string CellDirectortext2 = "";
                    CellDirector2.Phrase = new Phrase(CellDirectortext2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellDirector2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDirector2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellDirector3 = new PdfPCell();
                    string CellDirector3text = "Purchasing Director";
                    CellDirector3.Phrase = new Phrase(CellDirector3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontWss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellDirector3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDirector3.BorderColor = BaseColor.WHITE;



                    PdfPCell CellDirector4 = new PdfPCell();
                    string CellDirector4text = "";
                    CellDirector4.Phrase = new Phrase(CellDirector4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontVs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector4.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellDirector4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDirector4.BorderColor = BaseColor.WHITE;





                    PdfPTable table5 = new PdfPTable(1);

                    table5.WidthPercentage = 100;










                    PdfPTable FooterPart = new PdfPTable(1);

                    FooterPart.WidthPercentage = 100;










                    document.Add(tableHeading);
                    document.Add(table2);
                    document.Add(table4);
                    document.Add(table);

                    document.Add(table5);

                    document.NewPage();


                    //Start PDF Service

                    document.Close();
                    byte[] result = ms.ToArray();
                    ms = new MemoryStream();
                    ms.Write(result, 0, result.Length);
                    ms.Position = 0;


                    var CompanyName = companyname.ToString().ToLower();

                    string FullFileName = DateTime.Now.ToFileTime() + "_" + "JournalEntryReport.pdf";
                    string PathsTR = "Attachments/" + CompanyName + "/";
                    String Filepath = Path.Combine(_host.WebRootPath, PathsTR);
                    string p_strPath = Path.Combine(Filepath, FullFileName);

                    System.IO.File.WriteAllBytes(p_strPath, result);

                    Response.Message = Globals.baseURL + '/' + PathsTR + FullFileName;
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

        public BaseMessageResponse AccountMovementReports([FromHeader] AccountMovementReportsHeader header, HttpRequest Request)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var GetAccountMouvementReportList = new List<AccountOfMovement>();
                if (Response.Result)
                {
                    string AccountsListString = "";
                    List<long> AccountsIdsList = new List<long>();
                    if (!string.IsNullOrEmpty(header.AccountsList))
                    {
                        AccountsListString = header.AccountsList.ToString();
                        AccountsIdsList = AccountsListString.Split(',').Select(s => long.Parse(s.Trim())).ToList();
                    }
                    DateTime FromDate = new DateTime(DateTime.Now.Year, 1, 1);
                    if (!string.IsNullOrEmpty(header.FromDate) && DateTime.TryParse(header.FromDate, out FromDate))
                    {
                        FromDate = DateTime.Parse(header.FromDate);
                    }
                    DateTime ToDate = new DateTime(DateTime.Now.Year + 1, 1, 1);
                    if (!string.IsNullOrEmpty(header.ToDate) && DateTime.TryParse(header.ToDate, out ToDate))
                    {
                        ToDate = DateTime.Parse(header.ToDate);
                    }
                    var dt = new System.Data.DataTable("grid");
                    GetAccountMouvementReportList = Common.GetAccountMovementList_WithListAccountIds(AccountsListString, header.CalcWithoutPrivate, header.OrderByCreationDate, FromDate, ToDate, 0, 0, _Context);
                    dt.Columns.AddRange(new DataColumn[15] { new DataColumn("Creation Date / Entry Date"),
                                                     new DataColumn("From or To"),
                                                     new DataColumn("Account Name"),
                                                     new DataColumn("Related Account"),
                                                     new DataColumn("Account Code") ,
                                                     new DataColumn("Account Category"),
                                                     //new DataColumn("Account Type"),
                                                     new DataColumn("Cu.") ,
                                                     new DataColumn("Credit"),
                                                     new DataColumn("Debit"),
                                                     new DataColumn("AccBalance"),
                                                     new DataColumn("Journal Entry #"),
                                                     new DataColumn("Description"),
                                                     new DataColumn("Ref. Doc#"),
                                                     new DataColumn("Created By"),
                                                     new DataColumn("Method"),


                    });
                    if (GetAccountMouvementReportList != null)
                    {
                        foreach (var item in GetAccountMouvementReportList)
                        {
                            if (item.FromOrTo.ToLower().Contains("from"))
                            {
                                item.FromOrTo = "To";
                            }
                            else
                            {
                                item.FromOrTo = "From";
                            }
                            dt.Rows.Add(
                               "C.D " + item.CreationDate + "\n" + " " + "E.D " + item.EntryDate,
                                item.FromOrTo,
                                item.AccountName,
                                item.ReleatedAccount != null ? item.ReleatedAccount : "-",
                                item.AccountCode != null ? item.AccountCode : "-",
                                item.AccountCategory + " " + "(" + item.AccountType + ")",
                                //item.AccountType != null ? item.AccountType : "-",
                                item.Currency != null ? item.Currency : "-",
                                item.Credit != null ? item.Credit : 0,
                                item.Debit != null ? item.Debit : 0,
                                item.Accumulative != null ? item.Accumulative : 0,
                                //Test,
                                item.EntryDate != null ? item.EntryDate : "-",
                                item.Description != null ? item.Description : "-",
                                item.Document != null ? item.Document : "-",
                                item.CreatedBy != null ? item.CreatedBy : "-",
                                item.MethodName != null ? item.MethodName : "-"


                                );
                        }
                    }
                    if (header.FileExtension != null && header.FileExtension == "PDF")
                    {
                        MemoryStream ms = new MemoryStream();

                        //Size of page
                        iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate());


                        PdfWriter pw = PdfWriter.GetInstance(document, ms);

                        //Call the footer Function

                        pw.PageEvent = new HeaderFooter3(Request, _Context);

                        document.Open();

                        //Handle fonts and Sizes +  Attachments images logos 

                        iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.A4);

                        document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                        //document.SetMargins(-80, -80, 10, 10);
                        BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        Font font = new Font(bf, 8, Font.NORMAL);

                        String path = Path.Combine(_host.WebRootPath, "/Attachments");

                         if (Request.Headers["CompanyName"].ToString().ToLower() == "piaroma" || Request.Headers["CompanyName"].ToString().ToLower() == "garastest")
                        {

                            string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                            if (System.IO.File.Exists(Piaroma_p_strPath))
                            {
                                Image logo = Image.GetInstance(Piaroma_p_strPath);
                                logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                                logo.ScaleAbsolute(300f, 300f);

                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                document.Add(logo);
                                document.Add(jpg);
                            }
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 15;
                            Chunk cc = new Chunk("Account Movement Report" + " ", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);
                            prgHeading.Add(cc);

                            document.Add(prgHeading);


                        }

                        //Adding paragraph for report generated by  
                        Paragraph prgGeneratedBY = new Paragraph();
                        BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                        prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                        document.Add(prgGeneratedBY);

                        //Adding a line  
                        Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.WHITE, Element.ALIGN_CENTER, 1)));
                        document.Add(p);

                        PdfPTable table2 = new PdfPTable(4);

                        table2.WidthPercentage = 100;

                        table2.SetTotalWidth(new float[] { 80, 380, 164, 52 });


                        PdfPCell cellA = new PdfPCell();
                        string cellAtext = "For Account Name: ";
                        cellA.Phrase = new Phrase(cellAtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                        iTextSharp.text.Font arabicFont5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cellA.HorizontalAlignment = Element.ALIGN_LEFT;
                        cellA.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cellA.BorderColor = BaseColor.WHITE;
                        cellA.PaddingTop = 15;

                        string AccountNameListSelected = GetAccountMouvementReportList.Select(x => x.AccountName).Distinct().ToList().Aggregate((x, y) => x + "," + y);

                        PdfPCell cellB = new PdfPCell();
                        string cellBtext = AccountNameListSelected;
                        cellB.Phrase = new Phrase(cellBtext, font);
                        iTextSharp.text.Font arabicFont4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        cellB.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                        cellB.ArabicOptions = 1;
                        //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cellB.HorizontalAlignment = Element.ALIGN_LEFT;
                        cellB.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cellB.BorderColor = BaseColor.WHITE;
                        cellB.PaddingTop = 15;


                        PdfPCell cellC = new PdfPCell();
                        string cellCtext = "Sorted By:";
                        cellC.Phrase = new Phrase(cellCtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                        iTextSharp.text.Font arabicFont6 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cellC.HorizontalAlignment = Element.ALIGN_RIGHT;
                        cellC.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cellC.BorderColor = BaseColor.WHITE;
                        cellC.PaddingTop = 15;


                        PdfPCell cellD = new PdfPCell();
                        if (header.OrderByCreationDate != true)
                        {
                            string cellDtext = "Entry Date";
                            cellD.Phrase = new Phrase(cellDtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                        }
                        else
                        {
                            string cellDtext = "Creation Date ";
                            cellD.Phrase = new Phrase(cellDtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                        }
                        iTextSharp.text.Font arabicFont7 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cellD.HorizontalAlignment = Element.ALIGN_RIGHT;
                        cellD.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cellD.BorderColor = BaseColor.WHITE;
                        cellD.PaddingTop = 15;

                        table2.AddCell(cellA);
                        table2.AddCell(cellB);
                        table2.AddCell(cellC);
                        table2.AddCell(cellD);
                        //Adding PdfPTable


                        PdfPTable table = new PdfPTable(dt.Columns.Count);

                        //table Width
                        table.WidthPercentage = 100;

                        //Define Sizes of Cloumns

                        table.SetTotalWidth(new float[] { 40, 20, 36, 30, 40, 18, 24, 22, 32, 23, 80, 25, 25, 20, 20 });
                        table.PaddingTop = 20;

                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            string cellText = HttpUtility.HtmlDecode(dt.Columns[i].ColumnName);
                            PdfPCell cell = new PdfPCell();
                            cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cell.PaddingBottom = 5;
                            table.AddCell(cell);

                        }


                        //writing table Data
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                //table.AddCell(dt.Rows[i][j].ToString());
                                PdfPCell cell = new PdfPCell();
                                cell.ArabicOptions = 1;
                                if (j <= 2)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cell.Padding = 5;
                                }
                                cell.ArabicOptions = 1;
                                if (j == 3)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cell.Padding = 5;
                                }
                                if (j == 4)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cell.Padding = 5;
                                }
                                if (j == 5)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cell.Padding = 5;
                                }
                                if (j == 6)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    cell.Padding = 5;
                                }
                                if (j == 7)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    cell.Padding = 5;
                                }
                                if (j == 8)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    cell.Padding = 5;
                                }
                                if (j == 9)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    cell.Padding = 5;
                                }
                                //else if (j >= 9)
                                //{
                                //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                //    cell.Padding = 5;
                                //}
                                //else
                                //{
                                //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                //    cell.Padding = 5;
                                //}

                                if (cell.ArabicOptions == 1)
                                {
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                    cell.Padding = 5;

                                }
                                else
                                {
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                    cell.Padding = 5;

                                }

                                cell.Phrase = new Phrase(1, dt.Rows[i][j].ToString(), font);
                                //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                                table.AddCell(cell);

                            }
                        }

                        table2.SpacingAfter = 15;
                        //document.Add(tableHeading);
                        document.Add(table2);
                        document.Add(table);
                        document.Close();
                        byte[] result = ms.ToArray();
                        ms = new MemoryStream();
                        ms.Write(result, 0, result.Length);
                        ms.Position = 0;


                        var CompanyName = Request.Headers["CompanyName"].ToString().ToLower();

                        string FullFileName = DateTime.Now.ToFileTime() + "_" + "Account_Managment.pdf";
                        string PathsTR = "Attachments/" + CompanyName + "/";
                        String Filepath = Path.Combine(_host.WebRootPath, PathsTR);
                        string p_strPath = Path.Combine(Filepath, FullFileName);

                        System.IO.File.WriteAllBytes(p_strPath, result);

                        Response.Message = Globals.baseURL + '/' + PathsTR + FullFileName;
                    }
                    else
                    {
                        using (ExcelPackage packge = new ExcelPackage())
                        {
                            var CompanyName = Request.Headers["CompanyName"].ToString().ToLower();
                            //Create the worksheet
                            ExcelWorksheet ws = packge.Workbook.Worksheets.Add("Account Mouvement Report ");
                            ws.TabColor = System.Drawing.Color.Red;
                            ws.Columns.BestFit = true;
                            //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                            ws.Cells["A1"].LoadFromDataTable(dt, true);
                            //ws.Cells[1, 30].LoadFromDataTable(dt2, true);

                            //Format the header for column 1-3
                            using (ExcelRange range = ws.Cells["A1:O1"])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189)); //Set color to dark blue
                                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                            }
                            string FromDateSTR = FromDate.Year + "-" + FromDate.Month + "-" + FromDate.Day;
                            string ToDateSTR = ToDate.Year + "-" + ToDate.Month + "-" + ToDate.Day;

                            using (var package = new ExcelPackage())
                            {

                                string text = "test";
                                string FullFileName = DateTime.Now.ToFileTime() + "_" + "Account_Mouvement_Report_of_" + CompanyName + "_for_" + text.Replace(" ", string.Empty) + "_From_" + FromDateSTR + "_To_" + ToDateSTR + ".xlsx";
                                string PathsTR = "Attachments/" + CompanyName + "/";
                                String path = Path.Combine(_host.WebRootPath, PathsTR);
                                string p_strPath = Path.Combine(path, FullFileName);
                                var workSheet = package.Workbook.Worksheets.Add("sheet");
                                ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);

                                System.IO.File.Exists(p_strPath);
                                FileStream objFileStrm = System.IO.File.Create(p_strPath);

                                objFileStrm.Close();
                                package.Save();
                                System.IO.File.WriteAllBytes(p_strPath, package.GetAsByteArray());
                                package.Dispose();

                                Response.Message = Globals.baseURL + '/' + PathsTR + FullFileName;
                            }
                        }

                    }


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

        public ExcelWorksheet MergeCells(ExcelWorksheet worksheet, string from)
        {
            if (from == "all")
            {
                string searchValue = worksheet.Cells[4, 24].Value == null || worksheet.Cells[4, 24].Value.ToString() == "" ? null : worksheet.Cells[4, 24].Value.ToString();
                var first = 4;
                var end = 4;
                for (int currentRow = 5; currentRow <= worksheet.Dimension.End.Row; currentRow++)
                {
                    if (worksheet.Cells[currentRow, 24].Value != null && worksheet.Cells[currentRow, 24].Value.ToString() != searchValue)
                    {
                        searchValue = worksheet.Cells[currentRow, 24].Value == null || worksheet.Cells[currentRow, 24].Value.ToString() == "" ? null : worksheet.Cells[currentRow, 24].Value.ToString();
                        worksheet.Cells[first, 3, end, 3].Merge = true;
                        worksheet.Cells[first, 4, end, 4].Merge = true;
                        //worksheet.Cells[first, 5, end, 5].Merge = true;
                        worksheet.Cells[first, 15, end, 15].Merge = true;
                        worksheet.Cells[first, 16, end, 16].Merge = true;
                        worksheet.Cells[first, 17, end, 17].Merge = true;
                        worksheet.Cells[first, 18, end, 18].Merge = true;
                        worksheet.Cells[first, 19, end, 19].Merge = true;
                        //worksheet.Cells[first, 19, end, 19].Merge = true;
                        worksheet.Cells[first, 21, end, 21].Merge = true;
                        worksheet.Cells[first, 22, end, 22].Merge = true;
                        worksheet.Cells[first, 23, end, 23].Merge = true;
                        first = currentRow;
                        end = currentRow;
                    }
                    else
                    {
                        end++;
                    }
                }
            }
            else if (from == "rent")
            {
                string searchValue = worksheet.Cells[4, 27].Value == null || worksheet.Cells[4, 27].Value.ToString() == "" ? null : worksheet.Cells[4, 27].Value.ToString();
                var first = 4;
                var end = 4;

                for (int currentRow = 5; currentRow <= worksheet.Dimension.End.Row; currentRow++)
                {
                    if (currentRow == 46)
                    {
                        searchValue = "";
                    }
                    if (worksheet.Cells[currentRow, 27].Value != null && worksheet.Cells[currentRow, 27].Value.ToString() != searchValue)
                    {
                        if (worksheet.Cells[first, 27].Value == null || string.IsNullOrEmpty(worksheet.Cells[first, 27].Value.ToString()))
                        {
                            first = currentRow;
                            end = first;
                        }
                        else
                        {
                            searchValue = worksheet.Cells[currentRow, 27].Value == null || worksheet.Cells[currentRow, 27].Value.ToString() == "" ? null : worksheet.Cells[currentRow, 27].Value.ToString();
                            worksheet.Cells[first, 15, end, 15].Merge = true;
                            worksheet.Cells[first, 16, end, 16].Merge = true;
                            worksheet.Cells[first, 17, end, 17].Merge = true;
                            worksheet.Cells[first, 18, end, 18].Merge = true;
                            worksheet.Cells[first, 19, end, 19].Merge = true;
                            worksheet.Cells[first, 20, end, 20].Merge = true;
                            worksheet.Cells[first, 21, end, 21].Merge = true;
                            worksheet.Cells[first, 25, end, 25].Merge = true;
                            worksheet.Cells[first, 26, end, 26].Merge = true;
                            first = currentRow;
                            end = currentRow;
                        }
                    }
                    else
                    {
                        if (worksheet.Cells[currentRow, 27].Value == null || string.IsNullOrEmpty(worksheet.Cells[currentRow, 27].Value.ToString()))
                        {
                            first++;
                            end = first;
                        }
                        else
                        {
                            end++;
                        }
                    }
                }
            }
            return worksheet;
        }

        public BaseResponseWithData<string> SalesOfferItemsReport(string CompanyName, int Year, int Month, long SalesPersonId)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Month < 1 || Month > 12)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "month value should be between 1 and 12";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (Year < DateTime.MinValue.Year || Year > DateTime.Now.Year)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Year Value not right";
                    Response.Errors.Add(error);
                    return Response;
                }
                User? salesperson = null;
                if (SalesPersonId != 0)
                {
                    salesperson = _unitOfWork.Users.GetById(SalesPersonId);
                    if (salesperson == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = "salesperson not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                var datetime = new DateTime(Year, Month, 1);
                var MonthName = datetime.ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("ar"));

                var products = _unitOfWork.SalesOfferProducts.FindAllQueryable(a => ((DateTime)a.Offer.ClientApprovalDate).Year == Year && ((DateTime)a.Offer.ClientApprovalDate).Month == Month, includes: new[] { "Offer.Client", "Offer.SalesPerson", "Product", "InventoryItemCategory", "Offer.Invoices", "SalesOfferProductTaxes", "Offer.Projects.ClientAccounts.AccountOfJe.Account.AdvanciedSettingAccounts", "InventoryItem" });
                if (salesperson != null)
                {
                    products = products.Where(a => a.Offer.SalesPersonId == SalesPersonId);
                }
                var items = products.OrderBy(a => a.Offer.OfferSerial).ToList();
                string fileInfo = _host.WebRootPath + @$"\Attachments\{CompanyName}\Templates\SalesOffersItemsTemplate.xlsx";

                if (!File.Exists(fileInfo))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Template not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                ExcelPackage package = new ExcelPackage(new FileInfo(fileInfo));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                worksheet.Cells["A1"].Value = $"مبيعات - شهر {MonthName} {datetime.Year}";
                worksheet.Cells["Q3"].Value = $"{MonthName}";
                /*if (salesperson != null)
                {
                    worksheet.Cells["A4"].Value = $" مبيعات مارينا للسقالات والروافع -  {salesperson.FirstName + ' ' + salesperson.LastName}";
                }
                else
                {
                    worksheet.Cells["A4"].Value = $"مبيعات مارينا للسقالات والروافع";
                }*/
                worksheet.DefaultRowHeight = 56;
                int rowIndex = 4;
                int counter = 1;
                foreach (var item in items)
                {
                    worksheet.Cells[rowIndex, 1].Value = counter;
                    worksheet.Cells[rowIndex, 2].Value = item.Offer.OfferSerial == null ? "" : item.Offer.OfferSerial.ToString();
                    worksheet.Cells[rowIndex, 3].Value = item.Offer.Client.Name ?? "";
                    worksheet.Cells[rowIndex, 4].Value = (item.Offer.SalesPerson.FirstName + " " + item.Offer.SalesPerson.LastName) ?? "";
                    worksheet.Cells[rowIndex, 5].Value = (item.InventoryItemCategory?.Name ?? "") + " / " + (item.InventoryItem?.Name ?? "");
                    worksheet.Cells[rowIndex, 6].Value = item.Quantity;
                    worksheet.Cells[rowIndex, 7].Value = item.Offer?.ProjectStartDate != null ? ((DateTime)item.Offer?.ProjectStartDate).ToShortDateString() : "";
                    worksheet.Cells[rowIndex, 8].Value = string.Join(@"\n", item.Offer?.Invoices.Where(a => a.Serial != null).Select(a => a.Serial)).ToString();
                    worksheet.Cells[rowIndex, 9].Value = item.Offer?.Invoices.FirstOrDefault()?.InvoiceDate != null ? ((DateTime)item.Offer?.Invoices.FirstOrDefault()?.InvoiceDate).ToShortDateString() : "";
                    worksheet.Cells[rowIndex, 9].Value = item.Offer?.Invoices.FirstOrDefault()?.InvoiceDate != null ? ((DateTime)item.Offer?.Invoices.FirstOrDefault()?.InvoiceDate).ToShortDateString() : "";
                    worksheet.Cells[rowIndex, 10].Value = item.FinalPrice;
                    worksheet.Cells[rowIndex, 11].Value = item.DiscountValue ?? 0;
                    worksheet.Cells[rowIndex, 12].Value = item.FinalPrice - (item.DiscountValue) ?? 0;
                    worksheet.Cells[rowIndex, 13].Value = item.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum();
                    worksheet.Cells[rowIndex, 24].Value = item.Offer.Id.ToString();
                    worksheet.Cells[rowIndex, 14].Value = (item.FinalPrice - item.DiscountValue ?? 0) + item.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum();
                    worksheet.Cells[rowIndex, 15].Value = items.Where(a => a.OfferId == item.OfferId).Sum(a => (a.FinalPrice - a.DiscountValue ?? 0) + a.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum());
                    worksheet.Cells[rowIndex, 16].Value =
                        item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Month == Month && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount) -
                        item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Month == Month && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount);

                    worksheet.Cells[rowIndex, 17].Value = item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Month == Month && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount);

                    var projectId = item.Offer.Projects.Select(a => a.Id).FirstOrDefault();
                    decimal TreasuaryAmount = 0;
                    decimal PermissoryAmount = 0;
                    var ClientAccountList = _unitOfWork.ClientAccounts.FindAll(x => x.Project.Id == projectId).ToList();
                    var AccountsIDSList = ClientAccountList.Select(x => x.AccountId).ToList();
                    var EntriesIDSList = ClientAccountList.Select(x => x.DailyAdjustingEntryId).ToList();

                    if (AccountsIDSList.Count > 0 && EntriesIDSList.Count > 0)
                    {
                        var AccountOfJonalEntriesList = _unitOfWork.AccountOfJournalEntries
                            .FindAll(x => EntriesIDSList.Contains(x.EntryId) && !x.Account.AdvanciedSettingAccounts.Where(a => a.AdvanciedTypeId == 30).Any() && x.CreationDate.Month == Month && x.CreationDate.Year == DateTime.Now.Year, includes: new[] { "Account.AdvanciedSettingAccounts" }).ToList();

                        TreasuaryAmount = AccountOfJonalEntriesList.Where(x => x.SignOfAccount == "plus" && x.Account != null &&
                            x.Account.AdvanciedSettingAccounts.ToList().Where(x => x.AdvanciedTypeId == 2).Any()).Select(x => x.Amount).DefaultIfEmpty(0).Sum();



                        PermissoryAmount = AccountOfJonalEntriesList.Where(x => x.SignOfAccount == "plus" && x.Account != null && x.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 3).Any()).Select(x => x.Amount).DefaultIfEmpty(0).Sum();


                    }
                    worksheet.Cells[rowIndex, 18].Value = PermissoryAmount;
                    worksheet.Cells[rowIndex, 19].Value = TreasuaryAmount;

                    decimal taxpercentage = decimal.Parse(worksheet.Cells[rowIndex, 12].Value.ToString()) != 0 ? ((decimal.Parse(worksheet.Cells[rowIndex, 13].Value.ToString()) / decimal.Parse(worksheet.Cells[rowIndex, 12].Value.ToString())) * 100) : 0;
                    worksheet.Cells[rowIndex, 20].Value = decimal.Parse(worksheet.Cells[rowIndex, 17].Value.ToString()) / (1 + taxpercentage);


                    worksheet.Cells[rowIndex, 21].Value = item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.CreationDate.Month == Month && a.CreationDate.Year == DateTime.Now.Year && a.AmountSign == "plus").Sum(a => a.Amount);

                    worksheet.Cells[rowIndex, 22].Value = (decimal.Parse(worksheet.Cells[rowIndex, 15].Value.ToString()) - decimal.Parse(worksheet.Cells[rowIndex, 21].Value.ToString()));

                    worksheet.Cells[rowIndex, 23].Value = decimal.Parse(worksheet.Cells[rowIndex, 15].Value.ToString()) != 0 ? ((decimal.Parse(worksheet.Cells[rowIndex, 21].Value.ToString()) / decimal.Parse(worksheet.Cells[rowIndex, 15].Value.ToString())) * 100) : 0;


                    worksheet.Row(rowIndex).CustomHeight = false;
                    worksheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    counter++;
                    rowIndex++;
                }
                worksheet.Column(1).AutoFit();
                worksheet.Column(2).AutoFit();
                worksheet.Column(3).Style.WrapText = true;
                worksheet.Column(3).Width *= 2;
                worksheet.Column(4).AutoFit();
                worksheet.Column(5).AutoFit();
                worksheet.Column(6).AutoFit();
                worksheet.Column(7).AutoFit();
                worksheet.Column(8).AutoFit();
                worksheet.Cells[6, 10, worksheet.Dimension.End.Row, 22].Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(9).AutoFit();
                worksheet.Column(11).AutoFit();
                worksheet.Column(12).AutoFit();
                worksheet.Column(13).AutoFit();
                worksheet.Column(15).AutoFit();
                worksheet.Column(16).AutoFit();
                worksheet.Column(17).AutoFit();
                worksheet.Column(18).AutoFit();
                worksheet.Column(20).AutoFit();
                worksheet.Column(23).Style.Numberformat.Format = "#0\\.00%";
                worksheet.Column(21).AutoFit();
                worksheet.Column(22).AutoFit();
                worksheet.Column(23).AutoFit();
                worksheet.Column(24).Hidden = true;
                worksheet = MergeCells(worksheet, "all");
                worksheet.View.FreezePanes(4, 1);
                var newpath = $"Attachments\\{CompanyName}\\OfferProductsSheets";
                var savedPath = Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var excelPath = savedPath + $"\\OfferProductsSheet.xlsx";
                package.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                package.Dispose();
                Response.Data = Globals.baseURL + '\\' + newpath + $"\\OfferProductsSheet.xlsx";

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




        public BaseResponseWithData<string> AccoutingStatementOfSalesOffers(string CompanyName, int BranchId, int Year)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Year < DateTime.MinValue.Year || Year > DateTime.Now.Year)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Year Value not right";
                    Response.Errors.Add(error);
                    return Response;
                }

                Branch? branch = null;
                if (BranchId != 0)
                {
                    branch = _unitOfWork.Branches.GetById(BranchId);
                    if (branch == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "Branch not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                var offers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Status == "Closed" && a.OfferType == "New Rent Offer" && ((DateTime)a.ClientApprovalDate).Year == Year, includes: new[] { "SalesOfferProducts.InventoryItem.InventoryItemCategory" });
                if (branch != null)
                {
                    offers = offers.Where(a => a.BranchId == branch.Id).AsQueryable();
                }

                var offerslist = offers.ToList();

                var productCategories = offerslist.SelectMany(a => a.SalesOfferProducts.Select(a => a.InventoryItem.InventoryItemCategory)).ToList();
                string fileInfo = _host.WebRootPath + @$"\Attachments\{CompanyName}\Templates\AccountantStatementTemplate.xlsx";

                if (!File.Exists(fileInfo))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Template not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                ExcelPackage package = new ExcelPackage(new FileInfo(fileInfo));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                worksheet.Cells["B2"].Value = $"كشف تفصيلى لتعاقدات ومتحصلات ايجار معدات شركة مارينا للسقالات والروافع لعام {Year}";

                var groups = productCategories.Where(a => a != null).Distinct().ToList();
                int column = 7;
                int column2 = 9;
                foreach (var group in groups)
                {
                    worksheet.InsertColumn(column, 1);
                    worksheet.Column(column).Style.Font.Size = 14;
                    worksheet.Column(column).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells[3, column].Value = group.Name;
                    worksheet.Cells[3, column].Style.Font.Bold = true;
                    worksheet.Cells[3, column, 4, column].Merge = true;
                    worksheet.Cells[3, column, 4, column].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    worksheet.Cells[3, column, 4, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[3, column, 4, column].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(198, 224, 180));
                    worksheet.Cells[29, column].Value = group.Name;
                    worksheet.Cells[29, column].Style.Font.Bold = true;
                    worksheet.Cells[29, column].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    worksheet.Cells[29, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[29, column].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(198, 224, 180));
                    //worksheet.InsertColumn(column2, 1);
                    worksheet.Column(column + 2 + groups.IndexOf(group)).Style.Font.Size = 14;
                    worksheet.Column(column + 2 + groups.IndexOf(group)).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells[3, column + 2 + groups.IndexOf(group)].Value = group.Name;
                    worksheet.Cells[3, column + 2 + groups.IndexOf(group)].Style.Font.Bold = true;
                    worksheet.Cells[3, column + 2 + groups.IndexOf(group), 4, column + 2 + groups.IndexOf(group)].Merge = true;
                    worksheet.Cells[3, column + 2 + groups.IndexOf(group), 4, column + 2 + groups.IndexOf(group)].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    worksheet.Cells[3, column + 2 + groups.IndexOf(group), 4, column + 2 + groups.IndexOf(group)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[3, column + 2 + groups.IndexOf(group), 4, column + 2 + groups.IndexOf(group)].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(198, 224, 180));

                    worksheet.Cells[29, column + 2 + groups.IndexOf(group)].Value = group.Name;
                    worksheet.Cells[29, column + 2 + groups.IndexOf(group)].Style.Font.Bold = true;
                    worksheet.Cells[29, column + 2 + groups.IndexOf(group)].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    worksheet.Cells[29, column + 2 + groups.IndexOf(group)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[29, column + 2 + groups.IndexOf(group)].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(198, 224, 180));
                    worksheet.Column(column + 2 + groups.IndexOf(group)).AutoFit();

                    column++;
                }

                var offersJan = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 1).ToList();
                worksheet.Cells[5, 3].Value = offersJan.Sum(a => a.OfferAmount);
                worksheet.Cells[5, 4].Value = offersJan.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[5, 5].Value = $"Jan - {Year}";

                var offersFeb = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 2).ToList();
                worksheet.Cells[7, 3].Value = offersFeb.Sum(a => a.OfferAmount);
                worksheet.Cells[7, 4].Value = offersFeb.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[7, 5].Value = $"Feb - {Year}";

                var offersMar = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 3).ToList();
                worksheet.Cells[9, 3].Value = offersMar.Sum(a => a.OfferAmount);
                worksheet.Cells[9, 4].Value = offersMar.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[9, 5].Value = $"Mar - {Year}";

                var offersApr = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 4).ToList();
                worksheet.Cells[11, 3].Value = offersApr.Sum(a => a.OfferAmount);
                worksheet.Cells[11, 4].Value = offersApr.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[11, 5].Value = $"Apr - {Year}";

                var offersMay = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 5).ToList();
                worksheet.Cells[13, 3].Value = offersMay.Sum(a => a.OfferAmount);
                worksheet.Cells[13, 4].Value = offersMay.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[13, 5].Value = $"May - {Year}";

                var offersJun = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 6).ToList();
                worksheet.Cells[15, 3].Value = offersJun.Sum(a => a.OfferAmount);
                worksheet.Cells[15, 4].Value = offersJun.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[15, 5].Value = $"Jun - {Year}";

                var offersJul = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 7).ToList();
                worksheet.Cells[17, 3].Value = offersJul.Sum(a => a.OfferAmount);
                worksheet.Cells[17, 4].Value = offersJul.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[17, 5].Value = $"Jul - {Year}";

                var offersAug = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 8).ToList();
                worksheet.Cells[19, 3].Value = offersAug.Sum(a => a.OfferAmount);
                worksheet.Cells[19, 4].Value = offersAug.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[19, 5].Value = $"Aug - {Year}";

                var offersSep = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 9).ToList();
                worksheet.Cells[21, 3].Value = offersSep.Sum(a => a.OfferAmount);
                worksheet.Cells[21, 4].Value = offersSep.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[21, 5].Value = $"Sep - {Year}";

                var offersOct = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 10).ToList();
                worksheet.Cells[23, 3].Value = offersOct.Sum(a => a.OfferAmount);
                worksheet.Cells[23, 4].Value = offersOct.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[23, 5].Value = $"Oct - {Year}";

                var offersNov = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 11).ToList();
                worksheet.Cells[25, 3].Value = offersNov.Sum(a => a.OfferAmount);
                worksheet.Cells[25, 4].Value = offersNov.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[25, 5].Value = $"Nov - {Year}";

                var offersDec = offerslist.Where(a => ((DateTime)a.ClientApprovalDate).Month == 12).ToList();
                worksheet.Cells[27, 3].Value = offersDec.Sum(a => a.OfferAmount);
                worksheet.Cells[27, 4].Value = offersDec.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                worksheet.Cells[27, 5].Value = $"Dec - {Year}";

                worksheet.Row(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Row(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Row(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Row(29).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Column(3).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Column(3).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(4).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Column(4).Style.Numberformat.Format = "#,##0.00";

                column = 7;
                column2 = column + groups.Count() + 1;

                foreach (var group in groups)
                {
                    var forMonth1 = offersJan.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[5, column].Value = forMonth1.Sum(a => a.OfferAmount);
                    worksheet.Cells[6, column].Value = forMonth1.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[5, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[6, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth2 = offersFeb.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[7, column].Value = forMonth2.Sum(a => a.OfferAmount);
                    worksheet.Cells[8, column].Value = forMonth2.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[7, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[8, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth3 = offersMar.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[9, column].Value = forMonth3.Sum(a => a.OfferAmount);
                    worksheet.Cells[10, column].Value = forMonth3.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[9, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[10, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth4 = offersApr.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[11, column].Value = forMonth4.Sum(a => a.OfferAmount);
                    worksheet.Cells[12, column].Value = forMonth4.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[11, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[12, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth5 = offersMay.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[13, column].Value = forMonth5.Sum(a => a.OfferAmount);
                    worksheet.Cells[14, column].Value = forMonth5.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[13, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[14, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth6 = offersJun.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[15, column].Value = forMonth6.Sum(a => a.OfferAmount);
                    worksheet.Cells[16, column].Value = forMonth6.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[15, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[16, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth7 = offersJul.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[17, column].Value = forMonth7.Sum(a => a.OfferAmount);
                    worksheet.Cells[18, column].Value = forMonth7.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[17, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[18, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth8 = offersAug.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[19, column].Value = forMonth8.Sum(a => a.OfferAmount);
                    worksheet.Cells[20, column].Value = forMonth8.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[19, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[20, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth9 = offersSep.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[21, column].Value = forMonth9.Sum(a => a.OfferAmount);
                    worksheet.Cells[22, column].Value = forMonth9.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[21, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[22, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth10 = offersOct.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[23, column].Value = forMonth10.Sum(a => a.OfferAmount);
                    worksheet.Cells[24, column].Value = forMonth10.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[23, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[24, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth11 = offersNov.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[25, column].Value = forMonth11.Sum(a => a.OfferAmount);
                    worksheet.Cells[26, column].Value = forMonth11.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[25, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[26, column].Style.Numberformat.Format = "#,##0.00";

                    var forMonth12 = offersDec.Where(a => a.SalesOfferProducts.Where(b => b.InventoryItemCategoryId == group.Id).Any());
                    worksheet.Cells[27, column].Value = forMonth12.Sum(a => a.OfferAmount);
                    worksheet.Cells[28, column].Value = forMonth12.Select(a => a.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount)).Sum();
                    worksheet.Cells[27, column].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[28, column].Style.Numberformat.Format = "#,##0.00";







                    worksheet.Cells[5, column2].Value = forMonth1.Count();
                    worksheet.Cells[5, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[5, column2, 6, column2].Merge = true;
                    worksheet.Cells[7, column2].Value = forMonth2.Count();
                    worksheet.Cells[7, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[7, column2, 8, column2].Merge = true;
                    worksheet.Cells[9, column2].Value = forMonth3.Count();
                    worksheet.Cells[9, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[9, column2, 10, column2].Merge = true;
                    worksheet.Cells[11, column2].Value = forMonth4.Count();
                    worksheet.Cells[11, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[11, column2, 12, column2].Merge = true;
                    worksheet.Cells[13, column2].Value = forMonth5.Count();
                    worksheet.Cells[13, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[13, column2, 14, column2].Merge = true;
                    worksheet.Cells[15, column2].Value = forMonth6.Count();
                    worksheet.Cells[15, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[15, column2, 16, column2].Merge = true;
                    worksheet.Cells[17, column2].Value = forMonth7.Count();
                    worksheet.Cells[17, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[17, column2, 18, column2].Merge = true;
                    worksheet.Cells[19, column2].Value = forMonth8.Count();
                    worksheet.Cells[19, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[19, column2, 20, column2].Merge = true;
                    worksheet.Cells[21, column2].Value = forMonth9.Count();
                    worksheet.Cells[21, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[21, column2, 22, column2].Merge = true;
                    worksheet.Cells[23, column2].Value = forMonth10.Count();
                    worksheet.Cells[23, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[23, column2, 24, column2].Merge = true;
                    worksheet.Cells[25, column2].Value = forMonth11.Count();
                    worksheet.Cells[25, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[25, column2, 26, column2].Merge = true;
                    worksheet.Cells[27, column2].Value = forMonth12.Count();
                    worksheet.Cells[27, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[27, column2, 28, column2].Merge = true;

                    worksheet.Column(column).AutoFit();
                    worksheet.Column(column).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Column(column).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Column(column2).AutoFit();
                    worksheet.Column(column2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Column(column2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    decimal oddSum = 0;
                    decimal evenSum = 0;
                    decimal countof = 0;
                    for (int row = 5; row < 29; row++)
                    {
                        if (row % 2 != 0) // Check if the row is odd
                        {
                            oddSum += decimal.Parse(worksheet.Cells[row, column].Value.ToString());
                            countof += decimal.Parse(worksheet.Cells[row, column2].Value.ToString());
                        }
                        else
                        {
                            evenSum += decimal.Parse(worksheet.Cells[row, column].Value.ToString());
                        }

                    }
                    worksheet.Cells[30, column].Value = oddSum;
                    worksheet.Cells[31, column].Value = oddSum / 12;
                    worksheet.Cells[32, column].Value = evenSum;
                    worksheet.Cells[33, column].Value = evenSum / 12;
                    worksheet.Cells[30, column2].Value = countof;
                    worksheet.Cells[30, column2].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[30, column2, 33, column2].Merge = true;

                    column++;
                    column2++;
                }

                var range = worksheet.Cells[5, 3, 30, 5];
                var values = range.Value as object[,];
                double sum = values
                .Cast<object>()
                .Select(value =>
                {
                    if (value != null && double.TryParse(value.ToString(), out double numValue))
                    {
                        return numValue;
                    }
                    return 0.0;
                }).Sum();

                worksheet.Cells[30, 3].Value = sum;
                worksheet.Cells[31, 3].Value = sum / 12;

                var range3 = worksheet.Cells[5, 3, 30, 5];
                var values3 = range3.Value as object[,];

                double sum3 = values3
                .Cast<object>()
                .Select(value =>
                {
                    if (value != null && double.TryParse(value.ToString(), out double numValue))
                    {
                        return numValue;
                    }
                    return 0.0;
                }).Sum();

                worksheet.Cells[30, 4].Value = sum3;
                worksheet.Cells[31, 4].Value = sum3 / 12;
                if (groups.Count > 0)
                {
                    var end = 7 + groups.Count();
                    var end2 = end + groups.Count();
                    worksheet.Cells[2, 2, 2, end].Merge = true;
                    worksheet.Cells[2, 2, 2, end].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    worksheet.Column(end + 2).AutoFit();
                    worksheet.Cells[2, end + 1, 2, end2].Merge = true;
                    worksheet.Cells[2, end + 1, 2, end2].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    worksheet.Cells[2, end + 1, 2, end2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    for (int i = 5; i < 34; i++)
                    {
                        if (i != 29)
                        {
                            var range2 = worksheet.Cells[i, 7, i, end];
                            var values2 = range2.Value as object[,];
                            double sum2 = values2
                            .Cast<object>()
                            .Select(value =>
                            {
                                if (value != null && double.TryParse(value.ToString(), out double numValue))
                                {
                                    return numValue;
                                }
                                return 0.0;
                            }).Sum();

                            worksheet.Cells[i, end].Value = sum2;
                            worksheet.Cells[i, end].Style.Numberformat.Format = "#,##0.00";
                            worksheet.Column(end).AutoFit();
                        }
                    }

                    for (int i = 0; i < groups.Count(); i++)
                    {
                        worksheet.Cells[34, 7 + i].Value = decimal.Parse(worksheet.Cells[33, 7 + i].Value.ToString()) / (decimal.Parse(worksheet.Cells[33, end].Value.ToString()) != 0 ? decimal.Parse(worksheet.Cells[33, end].Value.ToString()) : 1);
                    }
                }


                var newpath = $"Attachments\\{CompanyName}\\AccountantStatements";
                var savedPath = Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var excelPath = savedPath + $"\\AccountantStatement.xlsx";
                package.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                package.Dispose();
                Response.Data = Globals.baseURL + '\\' + newpath + $"\\AccountantStatement.xlsx";

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

        public BaseResponseWithData<string> RentalEquipmentsReport(string CompanyName, int Year, int Month)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Month < 1 || Month > 12)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "month value should be between 1 and 12";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (Year < 1 || Year > DateTime.Now.Year)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Year Value not right";
                    Response.Errors.Add(error);
                    return Response;
                }
                var datetime = new DateTime(Year, Month, 1);
                var MonthName = datetime.ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("ar"));

                var products = _unitOfWork.SalesOfferProducts.FindAll(a => ((DateTime)a.Offer.ClientApprovalDate).Year == DateTime.Now.Year && ((DateTime)a.Offer.ClientApprovalDate).Month == Month && a.Offer.OfferType == "New Rent Offer" && a.Offer.Status == "Closed", includes: new[] { "Offer.Client", "Offer.SalesPerson", "Product", "InventoryItem", "InventoryItemCategory", "Offer.Invoices", "SalesOfferProductTaxes", "Offer.Projects.ClientAccounts.AccountOfJe.Account.AdvanciedSettingAccounts" });

                var items = products.OrderBy(a => a.Offer.OfferSerial).ToList();

                string fileInfo = _host.WebRootPath + @$"\Attachments\{CompanyName}\Templates\RentalEquipmentsReportTemplate.xlsx";

                if (!File.Exists(fileInfo))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Template not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                ExcelPackage package = new ExcelPackage(new FileInfo(fileInfo));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                worksheet.Cells["A1"].Value = $"إيجار شهــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــر {MonthName} {Year} ";
                worksheet.Cells["P2"].Value = $"دفعة {MonthName}";

                var counter = 1;

                var Lists = new List<List<SalesOfferProduct>>() { };

                var returnedRemain = items.Where(a => a.ConfirmReceivingQuantity != null && a.Quantity == a.ConfirmReceivingQuantity && (items.Where(b => b.OfferId == a.OfferId).Sum(a => (a.ItemPrice - a.DiscountValue ?? 0) + a.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum()) - a.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.CreationDate.Month == Month && a.CreationDate.Year == DateTime.Now.Year).Sum(a => a.Amount)) > 0).ToList();

                var notReturned = items.Where(a => a.ConfirmReceivingQuantity == null || a.ConfirmReceivingQuantity == 0).ToList();

                var final = items.Where(a => a.ConfirmReceivingQuantity != null && a.Quantity == a.ConfirmReceivingQuantity && items.Where(b => b.OfferId == a.OfferId).Sum(a => (a.ItemPrice - a.DiscountValue ?? 0) + a.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum()) == a.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.CreationDate.Month == Month && a.CreationDate.Year == DateTime.Now.Year).Sum(a => a.Amount)).ToList();

                var halfReturned = items.Where(a => a.ConfirmReceivingQuantity != null && a.Quantity > a.ConfirmReceivingQuantity).ToList();

                foreach (var item in halfReturned)
                {
                    var x = item.ConfirmReceivingQuantity;
                    var q = item.Quantity;
                    item.Quantity = q - x;
                    notReturned.Add(item);
                    item.Quantity = x;
                    final.Add(item);
                }
                Lists.Add(returnedRemain);
                Lists.Add(notReturned);
                Lists.Add(final);

                var typeNames = new List<string> { "Type1", "Type2", "Type3" };

                foreach (var list in Lists)
                {
                    var key = typeNames[Lists.IndexOf(list)];
                    foreach (var item in list)
                    {
                        var namedRange = package.Workbook.Names[key];
                        var Row = namedRange.Start.Row;
                        worksheet.InsertRow(Row, 1);
                        worksheet.Cells[Row, 1].Value = counter.ToString();
                        worksheet.Cells[Row, 2].Value = item.Offer.Client.Name ?? "";
                        worksheet.Cells[Row, 3].Value = item.InventoryItem.Name ?? "";
                        worksheet.Cells[Row, 4].Value = item.InventoryItemCategory?.Name ?? "";
                        worksheet.Cells[Row, 5].Value = item.Quantity ?? (double)0;
                        worksheet.Cells[Row, 6].Value = item.Offer?.ProjectStartDate != null ? ((DateTime)item.Offer?.ProjectStartDate).ToShortDateString() : "";
                        worksheet.Cells[Row, 7].Value = item.Offer?.ProjectEndDate != null ? ((DateTime)item.Offer?.ProjectEndDate).ToShortDateString() : "";
                        worksheet.Cells[Row, 8].Value = item.Offer?.ProjectEndDate != null && item.Offer?.ProjectStartDate != null ? ((DateTime)item.Offer?.ProjectEndDate - (DateTime)item.Offer?.ProjectStartDate).Days + 1 : 0;
                        worksheet.Cells[Row, 9].Value = item.FinalPrice ?? (decimal)0;
                        worksheet.Cells[Row, 10].Value = (item.FinalPrice ?? (decimal)0) * ((decimal)(item.Quantity ?? (double)0)) * decimal.Parse(worksheet.Cells[Row, 8].Value.ToString());
                        worksheet.Cells[Row, 11].Value = item.ItemPrice - item.DiscountValue ?? 0;
                        worksheet.Cells[Row, 12].Value = item.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum();
                        worksheet.Cells[Row, 13].Value = (item.ItemPrice - item.DiscountValue ?? 0) + item.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum();
                        worksheet.Cells[Row, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[Row, 14].Style.Fill.BackgroundColor.SetColor(1, 82, 82, 82);
                        worksheet.Cells[Row, 15].Value =
                            item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount) -
                            item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Month == Month && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount);
                        worksheet.Cells[Row, 16].Value = item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Month == Month && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount);
                        var projectId = item.Offer.Projects.Select(a => a.Id).FirstOrDefault();
                        decimal TreasuaryAmount = 0;
                        decimal PermissoryAmount = 0;

                        var ClientAccountList = _unitOfWork.ClientAccounts.FindAll(x => x.Project.Id == projectId).ToList();
                        var AccountsIDSList = ClientAccountList.Select(x => x.AccountId).ToList();
                        var EntriesIDSList = ClientAccountList.Select(x => x.DailyAdjustingEntryId).ToList();

                        if (AccountsIDSList.Count > 0 && EntriesIDSList.Count > 0)
                        {
                            var AccountOfJonalEntriesList = _unitOfWork.AccountOfJournalEntries
                                .FindAll(x => EntriesIDSList.Contains(x.EntryId) && !AccountsIDSList.Contains(x.AccountId) && x.CreationDate.Month == Month && x.CreationDate.Year == DateTime.Now.Year, includes: new[] { "Account.AdvanciedSettingAccounts" }).ToList();

                            TreasuaryAmount = AccountOfJonalEntriesList.Where(x => x.SignOfAccount == "plus" && x.Account != null &&
                                x.Account.AdvanciedSettingAccounts.ToList().Where(x => x.AdvanciedTypeId == 2).Any()).Select(x => x.Amount).DefaultIfEmpty(0).Sum();



                            PermissoryAmount = AccountOfJonalEntriesList.Where(x => x.SignOfAccount == "plus" && x.Account != null && x.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 3).Any()).Select(x => x.Amount).DefaultIfEmpty(0).Sum();


                        }
                        worksheet.Cells[Row, 17].Value = PermissoryAmount;
                        worksheet.Cells[Row, 18].Value = TreasuaryAmount;
                        worksheet.Cells[Row, 19].Value = item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.CreationDate.Month == Month && a.CreationDate.Year == DateTime.Now.Year).Sum(a => a.Amount);
                        worksheet.Cells[Row, 20].Value = item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.CreationDate.Month == Month && a.CreationDate.Year == DateTime.Now.Year).Sum(a => a.Amount) - item.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum();
                        worksheet.Cells[Row, 21].Value = items.Where(a => a.OfferId == item.OfferId).Sum(a => (a.ItemPrice - a.DiscountValue ?? 0) + a.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum()) - decimal.Parse(worksheet.Cells[Row, 19].Value.ToString());
                        worksheet.Cells[Row, 25].Value = (item.Offer.SalesPerson.FirstName + " " + item.Offer.SalesPerson.LastName) ?? "";
                        worksheet.Cells[Row, 27].Value = item.Offer.Id.ToString();

                        worksheet.Row(Row).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Row(Row).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        counter++;
                    }
                }
                var headers = new List<string> { "Header", "Type1", "Type2", "Type3" };

                for (int i = 0; i < headers.Count - 1; i++)
                {
                    var header1 = package.Workbook.Names[headers[i]];
                    var header2 = package.Workbook.Names[headers[i + 1]];

                    var range1 = worksheet.Cells[header1.Start.Row + 1, 19, header2.Start.Row - 1, 19];

                    var values1 = range1.Value as object[,];
                    double sum1 = values1?
                    .Cast<object>()
                    .Select(value =>
                    {
                        if (value != null && double.TryParse(value.ToString(), out double numValue))
                        {
                            return numValue;
                        }
                        return 0.0;
                    }).Sum() ?? 0;
                    worksheet.Cells[header2.Start.Row, 19].Value = sum1;

                    var range2 = worksheet.Cells[header1.Start.Row + 1, 20, header2.Start.Row - 1, 20];
                    var values2 = range2.Value as object[,];
                    double sum2 = values2?
                    .Cast<object>()
                    .Select(value =>
                    {
                        if (value != null && double.TryParse(value.ToString(), out double numValue))
                        {
                            return numValue;
                        }
                        return 0.0;
                    }).Sum() ?? 0;
                    worksheet.Cells[header2.Start.Row, 20].Value = sum2;

                    var range3 = worksheet.Cells[header1.Start.Row + 1, 21, header2.Start.Row - 1, 21];
                    var values3 = range3.Value as object[,];
                    double sum3 = values3?
                    .Cast<object>()
                    .Select(value =>
                    {
                        if (value != null && double.TryParse(value.ToString(), out double numValue))
                        {
                            return numValue;
                        }
                        return 0.0;
                    }).Sum() ?? 0;
                    worksheet.Cells[header2.Start.Row, 21].Value = sum3;

                    var range4 = worksheet.Cells[header1.Start.Row + 1, 22, header2.Start.Row - 1, 22];
                    var values4 = range4.Value as object[,];
                    double sum4 = values4?
                    .Cast<object>()
                    .Select(value =>
                    {
                        if (value != null && double.TryParse(value.ToString(), out double numValue))
                        {
                            return numValue;
                        }
                        return 0.0;
                    }).Sum() ?? 0;
                    worksheet.Cells[header2.Start.Row, 22].Value = sum4;

                    var range5 = worksheet.Cells[header1.Start.Row + 1, 23, header2.Start.Row - 1, 23];
                    var values5 = range5.Value as object[,];
                    double sum5 = values5?
                    .Cast<object>()
                    .Select(value =>
                    {
                        if (value != null && double.TryParse(value.ToString(), out double numValue))
                        {
                            return numValue;
                        }
                        return 0.0;
                    }).Sum() ?? 0;
                    worksheet.Cells[header2.Start.Row, 23].Value = sum5;

                    var range6 = worksheet.Cells[header1.Start.Row + 1, 24, header2.Start.Row - 1, 24];
                    var values6 = range6.Value as object[,];
                    double sum6 = values6?
                    .Cast<object>()
                    .Select(value =>
                    {
                        if (value != null && double.TryParse(value.ToString(), out double numValue))
                        {
                            return numValue;
                        }
                        return 0.0;
                    }).Sum() ?? 0;
                    worksheet.Cells[header2.Start.Row, 24].Value = sum6;
                }






                worksheet.Column(2).AutoFit();
                worksheet.Column(3).AutoFit();
                worksheet.Column(4).AutoFit();
                worksheet.Column(5).AutoFit();
                worksheet.Column(6).AutoFit();
                worksheet.Column(7).AutoFit();
                worksheet.Column(8).AutoFit();
                worksheet.Column(9).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(9).AutoFit();
                worksheet.Column(10).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(10).AutoFit();
                worksheet.Column(11).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(11).AutoFit();
                worksheet.Column(12).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(12).AutoFit();
                worksheet.Column(13).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(13).AutoFit();
                worksheet.Column(15).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(15).AutoFit();
                worksheet.Column(16).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(16).AutoFit();
                worksheet.Column(17).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(17).AutoFit();
                worksheet.Column(18).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(18).AutoFit();
                worksheet.Column(19).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(19).AutoFit();
                worksheet.Column(20).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(20).AutoFit();
                worksheet.Column(21).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(21).AutoFit();
                worksheet.Column(25).AutoFit();
                worksheet.Column(27).Hidden = true;
                worksheet = MergeCells(worksheet, "rent");
                worksheet.View.FreezePanes(4, 1);
                var newpath = $"Attachments\\{CompanyName}\\RentalEquipmentsReports";
                var savedPath = Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var excelPath = savedPath + $"\\RentalEquipmentsReport.xlsx";
                package.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                package.Dispose();
                Response.Data = Globals.baseURL + '\\' + newpath + $"\\RentalEquipmentsReport.xlsx";

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

        public BaseResponseWithData<string> Vault10Percent(string CompanyName, int Year, int Month)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Month < 1 || Month > 12)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "month value should be between 1 and 12";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (Year < 1 || Year > DateTime.Now.Year)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Year Value not right";
                    Response.Errors.Add(error);
                    return Response;
                }
                var datetime = new DateTime(Year, Month, 1);
                var MonthName = datetime.ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("ar"));

                var Offers = _unitOfWork.SalesOffers.FindAll(a => ((DateTime)a.ClientApprovalDate).Year == DateTime.Now.Year && ((DateTime)a.ClientApprovalDate).Month == Month, includes: new[] { "Client", "SalesPerson", "Invoices", "Projects.ClientAccounts.AccountOfJe.Account.AdvanciedSettingAccounts", "SalesOfferProducts.SalesOfferProductTaxes" });

                var items = Offers.OrderBy(a => a.OfferSerial).ToList();

                string fileInfo = _host.WebRootPath + @$"\Attachments\{CompanyName}\Templates\Vault10PercentTempelate.xlsx";
                if (!File.Exists(fileInfo))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Template not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                ExcelPackage package = new ExcelPackage(new FileInfo(fileInfo));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                worksheet.Cells["A1"].Value = $"{Year} - {MonthName}";
                int rowIndex = 7;
                int counter = 1;

                foreach (var item in items)
                {
                    worksheet.Cells[rowIndex, 1].Value = counter;
                    worksheet.Cells[rowIndex, 2].Value = item.OfferSerial.ToString();
                    worksheet.Cells[rowIndex, 3].Value = item.ProjectName.ToString();
                    worksheet.Cells[rowIndex, 4].Value = item.Client.Name.ToString();
                    worksheet.Cells[rowIndex, 5].Value = item.SalesPerson.FirstName.ToString() + " " + item.SalesPerson.LastName.ToString();
                    var finalPrice = item.FinalOfferPrice;
                    var sumOfTax = item.SalesOfferProducts.SelectMany(a => a.SalesOfferProductTaxes).Sum(a => a.Value);
                    worksheet.Cells[rowIndex, 6].Value = finalPrice;
                    worksheet.Cells[rowIndex, 7].Value = finalPrice - sumOfTax;

                    worksheet.Cells[rowIndex, 10].Value = item.ProjectStartDate != null ? ((DateTime)item.ProjectStartDate).ToShortDateString() : "";

                    var projectId = item.Projects.Select(a => a.Id).FirstOrDefault();
                    decimal TreasuaryAmount = 0;
                    decimal PermissoryAmount = 0;

                    var ClientAccountList = _unitOfWork.ClientAccounts.FindAll(x => x.Project.Id == projectId).ToList();
                    var AccountsIDSList = ClientAccountList.Select(x => x.AccountId).ToList();
                    var EntriesIDSList = ClientAccountList.Select(x => x.DailyAdjustingEntryId).ToList();

                    if (AccountsIDSList.Count > 0 && EntriesIDSList.Count > 0)
                    {
                        var AccountOfJonalEntriesList = _unitOfWork.AccountOfJournalEntries
                            .FindAll(x => EntriesIDSList.Contains(x.EntryId) && !AccountsIDSList.Contains(x.AccountId) && x.CreationDate.Month == Month && x.CreationDate.Year == DateTime.Now.Year, includes: new[] { "Account.AdvanciedSettingAccounts" }).ToList();

                        TreasuaryAmount = AccountOfJonalEntriesList.Where(x => x.SignOfAccount == "plus" && x.Account != null &&
                            x.Account.AdvanciedSettingAccounts.ToList().Where(x => x.AdvanciedTypeId == 2).Any()).Select(x => x.Amount).DefaultIfEmpty(0).Sum();



                        PermissoryAmount = AccountOfJonalEntriesList.Where(x => x.SignOfAccount == "plus" && x.Account != null && x.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 3).Any()).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                    }
                    worksheet.Cells[rowIndex, 8].Value = TreasuaryAmount;
                    worksheet.Cells[rowIndex, 9].Value = PermissoryAmount;

                    rowIndex++;
                    counter++;

                }

                var offersOfYear = _unitOfWork.SalesOffers.FindAll(a => ((DateTime)a.ClientApprovalDate).Year == DateTime.Now.Year, includes: new[] { "Client", "SalesPerson", "Invoices", "Projects.ClientAccounts.AccountOfJe.Account.AdvanciedSettingAccounts", "SalesOfferProducts.SalesOfferProductTaxes" });

                var sumOfTotalWithoutTax = offersOfYear.Sum(a => a.FinalOfferPrice - a.SalesOfferProducts.SelectMany(a => a.SalesOfferProductTaxes).Sum(a => a.Value));


                decimal treasuryTotalAmount = 0;
                decimal treasuryMonthlyAmount = 0;

                /*foreach(var item in offersOfYear)
                {
                   
                    var projectId = item.Projects.Select(a => a.Id).FirstOrDefault();

                    var ClientAccountList = _unitOfWork.ClientAccounts.FindAll(x => x.Project.Id == projectId && x.Account.AdvanciedSettingAccounts.ToList().Where(x => x.AdvanciedTypeId == 35).Any()).ToList();
                    if (ClientAccountList.Count>0)
                    {
                        var test = 1;
                    }
                    var AccountsIDSList = ClientAccountList.Select(x => x.AccountId).ToList();
                    var EntriesIDSList = ClientAccountList.Select(x => x.DailyAdjustingEntryId).ToList();

                    if (AccountsIDSList.Count > 0 && EntriesIDSList.Count > 0)
                    {
                        var AccountOfJonalEntriesList = _unitOfWork.AccountOfJournalEntries
                            .FindAll(x => EntriesIDSList.Contains(x.EntryId) && !AccountsIDSList.Contains(x.AccountId) && x.CreationDate.Year == DateTime.Now.Year, includes: new[] { "Account.AdvanciedSettingAccounts" }).ToList();

                        treasuryTotalAmount += AccountOfJonalEntriesList.Where(x => x.SignOfAccount == "plus" && x.Account != null &&
                            x.Account.AdvanciedSettingAccounts.ToList().Where(x => x.AdvanciedTypeId == 3 || x.AdvanciedTypeId ==2).Any()).Select(x => x.Amount).DefaultIfEmpty(0).Sum();



                        treasuryMonthlyAmount += AccountOfJonalEntriesList.Where(x => x.CreationDate.Month==Month && x.SignOfAccount == "plus" && x.Account != null && x.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 35).Any()).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                    }
                }*/
                worksheet.Cells["N7"].Value = $"إجمالى حصة الصندوق ({Year})";
                worksheet.Cells["N7"].Value = $"إجمالى حصة الصندوق ({MonthName} {Year})";



                var vaultYear = _unitOfWork.AccountOfJournalEntries.FindAll(a => a.Account.AdvanciedSettingAccounts.FirstOrDefault().AdvanciedTypeId == 35 && a.EntryDate.Year == Year).ToList();

                var EntryIdYear = vaultYear.Select(a => a.EntryId).ToList();
                var AccOfJEYear = _unitOfWork.AccountOfJournalEntries.FindAll(a => (a.Account.AdvanciedSettingAccounts.FirstOrDefault().AdvanciedTypeId == 2 || a.Account.AdvanciedSettingAccounts.FirstOrDefault().AdvanciedTypeId == 3) && a.EntryDate.Year == Year && EntryIdYear.Contains(a.EntryId) && a.SignOfAccount == "minus", includes: new[] { "Account" }).GroupBy(a => a.Account.AccountName).ToList();

                var vaultMonth = vaultYear.Where(a => a.EntryDate.Month == Month).ToList();
                var EntryIdMonth = vaultMonth.Select(a => a.EntryId).ToList();
                var AccOfJEMonth = _unitOfWork.AccountOfJournalEntries.FindAll(a => (a.Account.AdvanciedSettingAccounts.FirstOrDefault().AdvanciedTypeId == 2 || a.Account.AdvanciedSettingAccounts.FirstOrDefault().AdvanciedTypeId == 3) && a.EntryDate.Year == Year && a.EntryDate.Month == Month && EntryIdYear.Contains(a.EntryId) && a.SignOfAccount == "minus", includes: new[] { "Account" }).GroupBy(a => a.Account.AccountName).ToList();

                treasuryTotalAmount = vaultYear.Where(a => a.SignOfAccount == "plus").Select(a => a.Amount).Sum();
                worksheet.Cells["Q6"].Value = sumOfTotalWithoutTax ?? 0 * (decimal)0.1;
                worksheet.Cells["Q6"].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells["P9"].Value = treasuryTotalAmount;
                worksheet.Cells["P9"].Style.Numberformat.Format = "#,##0.00";

                treasuryMonthlyAmount = vaultMonth.Where(a => a.SignOfAccount == "plus").Select(a => a.Amount).Sum();
                worksheet.Cells["R9"].Value = treasuryMonthlyAmount;
                worksheet.Cells["R9"].Style.Numberformat.Format = "#,##0.00";

                var start = 10;
                foreach (var account in AccOfJEYear)
                {
                    worksheet.Cells["N" + start].Value = $"من خزينة {account.Key} فى {Year}";
                    worksheet.Cells["P" + start].Value = Math.Abs(account.Sum(a => a.Amount));
                    worksheet.Cells["P" + start].Style.Numberformat.Format = "#,##0.00";

                    worksheet.Cells["Q" + start].Value = $"خصم الشهر الحالى:";
                    worksheet.Cells["Q" + start].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["Q" + start].Style.Font.UnderLine = true;
                    worksheet.Cells["R" + start].Value = Math.Abs(account.Where(a => a.EntryDate.Month == Month).Sum(a => a.Amount));
                    worksheet.Cells["R" + start].Style.Font.Size = 12;
                    worksheet.Cells[$"R{start}:S{start}"].Merge = true;
                    worksheet.Cells["R" + start].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["R" + start].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    start++;
                }
                start++;
                worksheet.Cells["N" + start].Value = "رصيد الصندوق اخر الشهر السابق:";
                worksheet.Cells["P" + start].Value = vaultYear.Where(a => a.EntryDate < datetime).Sum(a => a.Amount);
                worksheet.Cells["P" + start].Style.Numberformat.Format = "#,##0.00";
                start++;
                worksheet.Cells["N" + start].Value = "رصيد الصندوق الشهر الحالى:";
                worksheet.Cells["P" + start].Value = vaultYear.Where(a => a.EntryDate <= DateTime.Now).Sum(a => a.Amount);
                worksheet.Cells["P" + start].Style.Numberformat.Format = "#,##0.00";
                start++;
                start++;
                worksheet.Cells["N" + start].Value = "متبقى ك رصيد للصندوق لم يتم تحويله من شهر التقرير";
                worksheet.Cells["N" + start].Style.Font.Bold = true;
                worksheet.Cells["N" + start].Style.Font.Size = 16;
                worksheet.Cells[$"Q{start}:S{start}"].Merge = true;
                worksheet.Cells[$"Q{start}:S{start}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                worksheet.Cells[$"Q{start}:S{start}"].Formula = "=K4-R9";
                worksheet.Cells[$"Q{start}:S{start}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"Q{start}:S{start}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                start++;

                worksheet.Cells["N" + start].Value = $"متبقى ك رصيد للصندوق لم يتم تحويله من {Year}";
                worksheet.Cells["N" + start].Style.Font.Bold = true;
                worksheet.Cells["N" + start].Style.Font.Size = 16;
                worksheet.Cells[$"Q{start}:S{start}"].Merge = true;
                worksheet.Cells[$"Q{start}:S{start}"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                worksheet.Cells[$"Q{start}:S{start}"].Formula = "=Q6-P9";
                worksheet.Cells[$"Q{start}:S{start}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"Q{start}:S{start}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Column(1).AutoFit();
                worksheet.Column(2).AutoFit();
                worksheet.Column(3).AutoFit();
                worksheet.Column(4).AutoFit();
                worksheet.Column(5).AutoFit();
                worksheet.Column(6).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(6).AutoFit();
                worksheet.Column(6).Width *= 1.5;
                worksheet.Column(7).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(7).AutoFit();
                worksheet.Column(8).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(8).AutoFit();
                worksheet.Column(9).Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(9).AutoFit();
                worksheet.Column(9).Width = worksheet.Column(9).Width <= 28.43 ? 29 : worksheet.Column(9).Width;
                worksheet.Column(10).Style.Numberformat.Format = "yyyy/mm/dd";
                worksheet.Column(10).AutoFit();
                worksheet.Column(10).Width *= 1.5;


                worksheet.Cells[5, 6, 6, 6].Merge = true;
                worksheet.Cells[5, 10, 6, 10].Merge = true;
                worksheet.Cells[5, 2, 6, 2].Merge = true;
                worksheet.Cells[5, 3, 6, 3].Merge = true;
                worksheet.Cells[5, 4, 6, 4].Merge = true;
                worksheet.Cells[5, 5, 6, 5].Merge = true;
                worksheet.Cells[5, 7, 6, 7].Merge = true;
                worksheet.Cells[5, 11, 6, 11].Merge = true;
                worksheet.Cells[5, 8, 5, 9].Merge = true;
                worksheet.Cells[3, 8, 3, 9].Merge = true;
                worksheet.Cells[1, 1, 1, 11].Merge = true;

                var newpath = $"Attachments\\{CompanyName}\\Vault10PercentReports";
                var savedPath = Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var excelPath = savedPath + $"\\Vault10PercentReport.xlsx";
                package.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                package.Dispose();
                Response.Data = Globals.baseURL + '\\' + newpath + $"\\Vault10PercentReport.xlsx";

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

        public AccountAndFinanceDashboardResponse GetAccountAndFinanceDashboard()
        {
            AccountAndFinanceDashboardResponse Response = new AccountAndFinanceDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var AccAndFinanceDashboardInfoObj = new AccountAndFinanceDashboardInfo();
                if (Response.Result)
                {
                    //var ResponseAccountAndFinanceIncomeStatment = GetAccountsAndFinanceIncomeStatment();
                    //AccAndFinanceDashboardInfoObj.IncomeStatment = ResponseAccountAndFinanceIncomeStatment.NetProfit;
                    AccAndFinanceDashboardInfoObj.IncomeStatment = Common.GetIncomeStatmentNetProfit(_Context);
                    // API Cash And Bank
                    var ResponseAccountAndFinanceCashAndBank = GetAccountsAndFinanceCashAndBank(null,null,null);
                    AccAndFinanceDashboardInfoObj.CashAndBank = ResponseAccountAndFinanceCashAndBank.TotalAmountSum;
                    // API Inventory
                    //var ResponseInventory = GetAccountAndFinanceInventoryStoreItemReportList();
                    //AccAndFinanceDashboardInfoObj.Inventory = ResponseInventory.TotalStockBalanceValue;
                    AccAndFinanceDashboardInfoObj.Inventory = Common.GetTotalAmountInventoryItem(0, _Context);//0 =>  All Items

                    // API supplier Report Sales Force
                    // var ResponseClientReport = GetAccountAndFinanceClientReportList();
                    // AccAndFinanceDashboardInfoObj.SalesForce = ResponseClientReport.TotalSalesVolume;
                    // AccAndFinanceDashboardInfoObj.SalesForceCollectedPercent = ResponseClientReport.TotalCollectedPercent;
                    AccAndFinanceDashboardInfoObj.TotalFinalOfferPrice = Common.GetTotalSalesOfferProjectFinalPriceAmount(_Context);
                    AccAndFinanceDashboardInfoObj.TotalProjectExtraCost = Common.GetTotalSalesOfferProjectExtraCostsAmount(_Context);
                    AccAndFinanceDashboardInfoObj.TotalFinalOfferPriceWithInternalType = Common.GetTotalSalesOfferProjectAmountForOffterTypeInternal(_Context);
                    AccAndFinanceDashboardInfoObj.SalesForce = AccAndFinanceDashboardInfoObj.TotalFinalOfferPrice + AccAndFinanceDashboardInfoObj.TotalProjectExtraCost;

                    //Common.GetTotalSalesForceClientReportAmount();
                    AccAndFinanceDashboardInfoObj.SalesForceCollectedPercent = Common.GetSalesForceCollectedPercent(AccAndFinanceDashboardInfoObj.SalesForce, _Context);

                    // API supplier Report Sales Force
                    // var ResponseSupplierReport = GetAccountAndFinanceSupplierReportList();
                    // AccAndFinanceDashboardInfoObj.Purchasing = ResponseSupplierReport.TotalSalesVolume;
                    // AccAndFinanceDashboardInfoObj.PurchasingPaidPercent = ResponseSupplierReport.TotalCollectedPercent;
                    AccAndFinanceDashboardInfoObj.Purchasing = Common.GetTotalPurchasingAndSupplierReportAmount(_Context);
                    AccAndFinanceDashboardInfoObj.PurchasingPaidPercent = Common.GetPurchasingCollectedPercent(AccAndFinanceDashboardInfoObj.Purchasing, _Context);


                    Response.Data = AccAndFinanceDashboardInfoObj;
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




    }
}

