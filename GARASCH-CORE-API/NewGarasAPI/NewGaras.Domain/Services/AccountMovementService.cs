using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGarasAPI.Models.AccountAndFinance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class AccountMovementService : IAccountMovementService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IProjectService _projectService;
        public static DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
        public static DateTime CurrentEndYear = new DateTime(DateTime.Now.Year + 1, 1, 1);

        public AccountMovementService(GarasTestContext context, ITenantService tenantService, IProjectService projectService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _projectService = projectService;
        }

        public List<AccountOfMovement> GetAccountMovementList_WithListAccountIds(string AccountIdSTRr, bool CalcWithoutPrivate, bool OrderByCreationDatee, DateTime? DateFrom, DateTime? DateTo, long ClientIdd, long SupplierIdd,long BranchIdd)
        {
            DateTime CreationDateFromm = StartYear;
            DateTime CreationDateToo = CurrentEndYear;

            if (DateFrom != null)
            {
                CreationDateFromm = (DateTime)DateFrom;
            }
            if (DateTo != null)
            {
                CreationDateToo = (DateTime)DateTo;
            }

            DateTime AccountBegine = new DateTime(2000, 1, 1);
            // Default From and To Current Year
            var AccountOfMovementList = new List<AccountOfMovement>();
            //var AccountsMovementListDB = _Context.STP_AccountMovement(AccountId, CalcWithoutPrivate, OrderByCreationDate, AccountBegine, CreationDateTo).ToList();
            //var IDSSTR = String.Join(",", AccountIds.Select(x => x.ToString()).ToArray());
            List<long> AccountIds = AccountIdSTRr.Split(',').Select(long.Parse).ToList();
            var AccountIDSTR = new SqlParameter("AccountIDSTR", System.Data.SqlDbType.NVarChar);
            AccountIDSTR.Value = AccountIdSTRr;
            var CalcWithoutPrivateJE = new SqlParameter("CalcWithoutPrivateJE", System.Data.SqlDbType.Bit);
            CalcWithoutPrivateJE.Value = CalcWithoutPrivate;
            var OrderByCreationDate = new SqlParameter("OrderByCreationDate", System.Data.SqlDbType.Bit);
            OrderByCreationDate.Value = OrderByCreationDatee;
            var CreationDateFrom = new SqlParameter("CreationDateFrom", System.Data.SqlDbType.DateTime);
            CreationDateFrom.Value = CreationDateFromm;
            var CreationDateTo = new SqlParameter("CreationDateTo ", System.Data.SqlDbType.DateTime);
            CreationDateTo.Value = CreationDateToo;
            var ClientId = new SqlParameter("ClientId ", System.Data.SqlDbType.BigInt);
            ClientId.Value = ClientIdd;
            var SupplierId = new SqlParameter("SupplierId ", System.Data.SqlDbType.BigInt);
            SupplierId.Value = SupplierIdd;
            var BranchId = new SqlParameter("BranchID ", System.Data.SqlDbType.Int);
            BranchId.Value = BranchIdd;
            object[] param = new object[] { AccountIDSTR, CalcWithoutPrivateJE, OrderByCreationDate, CreationDateFrom, CreationDateTo, ClientId, SupplierId,BranchId };
            var AccountsMovementListDB = _Context.Database.SqlQueryRaw<STP_AccountMovement_WithMultiAccountsWithOtherCU_Result>("Exec STP_AccountMovement_WithMultiAccountsWithOtherCU @AccountIDSTR ,@CalcWithoutPrivateJE ,@OrderByCreationDate ,@CreationDateFrom ,@CreationDateTo, @ClientId, @SupplierId, @BranchID", param).AsEnumerable().ToList();



            var test = AccountsMovementListDB.Where(a => a.DocumentNumber == "13-8").ToList();
            var test2 = test.Sum(a => a.Acc_Calc);
            if (OrderByCreationDatee)
            {
                AccountsMovementListDB = AccountsMovementListDB.Where(x => x.CreationDate >= CreationDateFromm && x.CreationDate <= CreationDateToo).ToList();
            }
            else
            {
                AccountsMovementListDB = AccountsMovementListDB.Where(x => x.EntryDate >= CreationDateFromm && x.EntryDate <= CreationDateToo).ToList();
            }
            if (AccountsMovementListDB.Count() > 0)
            {
                var IDSDailyJournalEntry = AccountsMovementListDB.Where(x => AccountIds.Contains(x.ID)).Select(x => x.EntryID).ToList();

                var AccountOfJournalEntryList = _Context.VAccountOfJournalEntryWithDailies.Where(a => IDSDailyJournalEntry.Contains(a.EntryId) && a.Active == true).ToList();
                if (AccountsMovementListDB.Count() > 0)
                {



                    var IDSAccountOfJournalEntry = AccountsMovementListDB.Where(x => x.aofjeID != null).Select(x => x.aofjeID).Distinct().ToList();
                    var SupplierAccountsList = _Context.SupplierAccounts.Include(a=>a.Supplier).Where(x => IDSAccountOfJournalEntry.Contains((long)x.AccountOfJeid)).ToList();
                    var ClientAccountsList = _Context.ClientAccounts.Include(a=>a.Client).Where(x => IDSAccountOfJournalEntry.Contains((long)x.AccountOfJeid)).ToList();
                    var AccountOfMovementListdb = AccountsMovementListDB.Select((item, index) =>
                    {
                        bool IsCredit = item.Credit != 0;
                        var CountOfReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryId == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).Count();
                        string ReleatedAccount = "";
                        if (CountOfReleatedAccount == 1)
                        {
                            ReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryId == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).FirstOrDefault().AccountName;
                        }
                        else
                        {
                            ReleatedAccount = CountOfReleatedAccount + " Accounts";
                        }

                        long? SupplierID = null;
                        string SupplierName = null;
                        long? POID = null;
                        string AccountDescription = null;
                        long? ClientID = null;
                        long? ProjectID = null;
                        string ProjectName = null;
                        string ClientName = null;

                        var SupplierAccountDb = SupplierAccountsList.Where(x => x.AccountOfJeid == item.aofjeID).FirstOrDefault();
                        if (SupplierAccountDb != null)
                        {
                            SupplierID = SupplierAccountDb.SupplierId;
                            SupplierName = SupplierAccountDb.Supplier != null ? SupplierAccountDb.Supplier.Name : "";
                            POID = SupplierAccountDb.PurchasePoid;
                            AccountDescription = SupplierAccountDb.Description;
                        }
                        var ClientAccountDb = ClientAccountsList.Where(x => x.AccountOfJeid == item.aofjeID).FirstOrDefault();
                        if (ClientAccountDb != null)
                        {
                            ClientID = ClientAccountDb.ClientId;
                            ClientName = ClientAccountDb.Client != null ? ClientAccountDb.Client.Name : "";
                            ProjectID = ClientAccountDb.ProjectId;
                            AccountDescription = ClientAccountDb.Description;
                            ProjectName = ClientAccountDb.ProjectId != null ? _projectService.GetProjectName((long)ClientAccountDb.ProjectId) : "";
                        }
                        return new AccountOfMovement
                        {

                            AccountID = item.ID,
                            CreationDate = item.CreationDate.ToShortDateString(),
                            EntryDate = item.EntryDate.ToShortDateString(),
                            AccountName = item.AccountName,
                            Serial = item.Serial,
                            AccountCode = item.AccountNumber,
                            AccountCategory = item.AccountCategoryName,
                            AccountType = item.AccountTypeName,
                            Currency = item.CurrencyName,
                            Credit = item.Credit,
                            Debit = item.Debit,
                            Accumulative = item.Acc_Calc,
                            DailyJournalId = item.EntryID,
                            Description = item.Description,
                            Document = item.DocumentNumber,
                            CreatedBy = item.DailyCreatedFirstName + " " + item.DailyCreatedLastName,
                            MethodName = item.MethodName,
                            FromOrTo = item.FromOrTo,
                            AccountEntryComment = item.EntryAccountComment,
                            ReleatedAccount = ReleatedAccount,
                            ReleatedAccountList = AccountOfJournalEntryList.Where(a => a.EntryId == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).Select(a => a.AccountName).ToList(),
                            SupplierID = SupplierID,
                            SupplierName = SupplierName,
                            POID = POID,
                            AccountDescription = AccountDescription,
                            ClientID = ClientID,
                            ClientName = ClientName,
                            ProjectID = ProjectID,
                            ProjectName = ProjectName,
                            RateToLocalCU = item.RateToLocalCU,
                            CreditOtherCU = item.CreditOtherCU,
                            DebitOtherCU = item.DebitOtherCU,
                            ACCOtherCU = item.ACCOtherCU,
                            AmountOtherCU = item.AmountOtherCU,
                        };
                    }).ToList();







                    //foreach (var item in AccountsMovementListDB)
                    //{
                    //    var AccountOfMovement = new AccountOfMovement();
                    //    AccountOfMovement.AccountID = item.ID;
                    //    AccountOfMovement.CreationDate = item.CreationDate.ToShortDateString();
                    //    AccountOfMovement.EntryDate = item.EntryDate.ToShortDateString();
                    //    AccountOfMovement.AccountName = item.AccountName;
                    //    AccountOfMovement.Serial = item.Serial;
                    //    AccountOfMovement.AccountCode = item.AccountNumber;
                    //    AccountOfMovement.AccountCategory = item.AccountCategoryName;
                    //    AccountOfMovement.AccountType = item.AccountTypeName;
                    //    AccountOfMovement.Currency = item.CurrencyName;
                    //    AccountOfMovement.Credit = item.Credit;
                    //    AccountOfMovement.Debit = item.Debit;
                    //    AccountOfMovement.Accumulative = item.Acc_Calc;
                    //    AccountOfMovement.DailyJournalId = item.EntryID;
                    //    AccountOfMovement.Description = item.Description;
                    //    AccountOfMovement.Document = item.DocumentNumber;
                    //    AccountOfMovement.CreatedBy = item.DailyCreatedFirstName + " " + item.DailyCreatedLastName;
                    //    AccountOfMovement.MethodName = item.MethodName;
                    //    AccountOfMovement.FromOrTo = item.FromOrTo;
                    //    AccountOfMovement.AccountEntryComment = item.EntryAccountComment;


                    //    bool IsCredit = item.Credit != 0;
                    //    //var CountOfReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryID == item.EntryID && a.FromOrTo != item.FromOrTo).Count() ;
                    //    var CountOfReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryID == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).Count();
                    //    if (CountOfReleatedAccount == 1)
                    //    {
                    //        AccountOfMovement.ReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryID == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).FirstOrDefault().AccountName;
                    //    }
                    //    else
                    //    {
                    //        AccountOfMovement.ReleatedAccount = CountOfReleatedAccount + " Accounts";
                    //    }
                    //    //AccountOfMovement.ReleatedAccountList = AccountOfJournalEntryList.Where(a => a.EntryID == item.EntryID && a.FromOrTo != item.FromOrTo).Select(a => a.AccountName).ToList();
                    //    AccountOfMovement.ReleatedAccountList = AccountOfJournalEntryList.Where(a => a.EntryID == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).Select(a => a.AccountName).ToList();
                    //    var SupplierAccountDb = _Context.SupplierAccounts.Where(x => x.AccountOfJEId == item.aofjeID).FirstOrDefault();
                    //    if (SupplierAccountDb != null)
                    //    {
                    //        AccountOfMovement.SupplierID = SupplierAccountDb.SupplierID;
                    //        AccountOfMovement.SupplierName = SupplierAccountDb.Supplier != null ? SupplierAccountDb.Supplier.Name : "";
                    //        AccountOfMovement.POID = SupplierAccountDb.PurchasePOID;
                    //        AccountOfMovement.AccountDescription = SupplierAccountDb.Description;
                    //    }
                    //    var ClientAccountDb = _Context.ClientAccounts.Where(x => x.AccountOfJEId == item.aofjeID).FirstOrDefault();
                    //    if (ClientAccountDb != null)
                    //    {
                    //        AccountOfMovement.ClientID = ClientAccountDb.ClientID;
                    //        AccountOfMovement.ClientName = ClientAccountDb.Client != null ? ClientAccountDb.Client.Name : "";
                    //        AccountOfMovement.ProjectID = ClientAccountDb.ProjectID;
                    //        AccountOfMovement.AccountDescription = ClientAccountDb.Description;
                    //        AccountOfMovement.ProjectName = ClientAccountDb.ProjectID != null ? Common.GetProjectName((long)ClientAccountDb.ProjectID) : "";
                    //    }
                    //    AccountOfMovementList.Add(AccountOfMovement);
                    //}

                    AccountOfMovementList = AccountOfMovementListdb;
                }

            }
            return AccountOfMovementList;
        }
    }
}
