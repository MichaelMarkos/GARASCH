using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net;
using NewGaras.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Models.User;
using System.Web;
using NewGarasAPI.Models.Project.UsedInResponses;
using NewGaras.Infrastructure.Helper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Client;
using NewGarasAPI.Helper;
using NewGaras.Infrastructure.Models.CRM;
using NewGaras.Infrastructure.Models.SalesOffer.UsedInResponses;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGaras.Infrastructure.Models.CRM.Filters;
using System.Linq.Expressions;
using NewGaras.Infrastructure.Models.CRM.UsedInResponse;
using DocumentFormat.OpenXml.Spreadsheet;

namespace NewGaras.Domain.Services
{
    public class CrmService : ICrmService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        static readonly string key = "SalesGarasPass";
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
        public CrmService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {

            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;

        }
        public async Task<MyClientsCRMDashboardResponse> GetMyClientsDetailsCRM(GetMyClientsDetailsCRMFilters filters)
        {
            MyClientsCRMDashboardResponse Response = new MyClientsCRMDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);

                    if (filters.Year > 0)
                    {
                        if (filters.Month > 0)
                        {
                            StartDate = new DateTime(filters.Year, filters.Month, 1);

                            if (filters.Month != 12)
                            {
                                EndDate = new DateTime(filters.Year, filters.Month + 1, 1);
                            }
                            else
                            {
                                EndDate = new DateTime(filters.Year + 1, 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(filters.Year, 1, 1);
                            EndDate = new DateTime(filters.Year + 1, 1, 1);
                        }
                    }
                    else
                    {
                        if (filters.Month > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }
                    var StartDateLastYear = StartDate.AddYears(-1);
                    var EndDateLastYear = EndDate.AddYears(-1);

                    var StartDateThisYear = new DateTime(DateTime.Now.Year, 1, 1);
                    var EndDateThisYear = new DateTime(DateTime.Now.Year + 1, 1, 1);
                    var DateAfterTenDays = DateTime.Now.AddDays(10);

                    var sp_salesPersonId = new SqlParameter("SalesPersonId", System.Data.SqlDbType.BigInt);
                    var sp_branchId = new SqlParameter("BranchId", System.Data.SqlDbType.Int);

                    sp_salesPersonId.Value = filters.SalesPersonId!=0?filters.SalesPersonId:DBNull.Value;

                    sp_branchId.Value = filters.BranchId!=0?filters.BranchId:DBNull.Value;

                    var sp_startdate = new SqlParameter("StartDate", System.Data.SqlDbType.DateTime);
                    sp_startdate.Value = StartDate;
                    var sp_enddate = new SqlParameter("EndDate", System.Data.SqlDbType.DateTime);
                    sp_enddate.Value = EndDate;
                    var sp_WithProjectExtraModifications = new SqlParameter("WithProjectExtraModifications", System.Data.SqlDbType.Bit);
                    sp_WithProjectExtraModifications.Value = filters.WithProjectExtraModifications;
                    object[] param = new object[] { sp_salesPersonId, sp_branchId, sp_startdate, sp_enddate, sp_WithProjectExtraModifications };

                    var ClientsDealsCRM = _Context.Database.SqlQueryRaw<STP_ClientsCRM_Result>("Exec STP_ClientsCRM @SalesPersonId ,@BranchId ,@StartDate ,@EndDate ,@WithProjectExtraModifications", param).AsEnumerable().ToList();

                    /* var ClientsDealsCRM = _Context.STPClientsCRM(sp_salesPersonId, sp_branchId, StartDate, EndDate, filters.WithProjectExtraModifications).ToList();*/
                    Response.OldClientsSupportedByList = ClientsDealsCRM.Select(sb => new ClientsStatistics
                    {
                        DealsCount = sb.OldClientsDeals,
                        TotalDealsExtraCostPrice = sb.OldClientsProjectExtraModifications,
                        SupportedBy = sb.SupportedBy,
                        ClientsCount = sb.OldClientsCount,
                        ClientsRFQCount = sb.OldClientsRFQCount,
                        DealedClientsCount = sb.OldDealedClients,
                        TotalDealsPrice = sb.TotalDealsPriceOldClients
                    }).ToList();

                    var OldClientsCount = ClientsDealsCRM.Sum(a => a.OldClientsCount);
                    var OldClientsRFQCount = ClientsDealsCRM.Sum(a => a.OldClientsRFQCount);
                    Response.OldClientsCount = OldClientsCount;
                    Response.OldClientsRFQCount = OldClientsRFQCount;

                    var ExpiredOldClientsQuery = _unitOfWork.VClientUseers.FindAllQueryable(a => a.CreationDate < StartDateThisYear && a.ClientExpireDate < DateTime.Now && a.NeedApproval == 0).AsQueryable();
                    //filter the old client which are the clients created only in the last year

                    var WillExpiredOldClientsQuery = _unitOfWork.VClientUseers.FindAllQueryable(a => a.CreationDate < StartDateThisYear && a.ClientExpireDate < DateAfterTenDays && a.ClientExpireDate > DateTime.Now && a.NeedApproval == 0).AsQueryable();
                    if (filters.BranchId != 0)
                    {
                        ExpiredOldClientsQuery = ExpiredOldClientsQuery.Where(a => a.BranchId == filters.BranchId);
                        WillExpiredOldClientsQuery = WillExpiredOldClientsQuery.Where(a => a.BranchId == filters.BranchId);
                    }

                    if (filters.SalesPersonId != 0)
                    {
                        ExpiredOldClientsQuery = ExpiredOldClientsQuery.Where(a => a.SalesPersonId == filters.SalesPersonId);
                        WillExpiredOldClientsQuery = WillExpiredOldClientsQuery.Where(a => a.SalesPersonId == filters.SalesPersonId);
                    }

                    Response.OldExpiredClients = await ExpiredOldClientsQuery.CountAsync();
                    Response.OldWillExpiredClients = await WillExpiredOldClientsQuery.CountAsync();

                    var ClientsDealsCRMLastYear = _Context.Database.SqlQueryRaw<STP_ClientsCRM_Result>("Exec STP_ClientsCRM @SalesPersonId ,@BranchId ,@StartDate ,@EndDate ,@WithProjectExtraModifications", param).AsEnumerable().ToList();
                    var OldClientsTotalEngagingRateLastYear = ClientsDealsCRMLastYear.Sum(a => a.TotalDealsPriceOldClients);

                    EngagingRate NewEngaging = new EngagingRate
                    {
                        ClientsCount = ClientsDealsCRM.Sum(a => a.OldClientsCount),
                        DealedClientsCount = ClientsDealsCRM.Sum(a => a.OldDealedClients),
                        ClientsRFQCount = ClientsDealsCRM.Sum(a => a.OldClientsRFQCount),
                        DealsCount = ClientsDealsCRM.Sum(a => a.OldClientsDeals),
                        TotalEngagingRate = ClientsDealsCRM.Sum(a => a.TotalDealsPriceOldClients),
                        TotalEngagingRateLastYear = OldClientsTotalEngagingRateLastYear,
                        ClientsCountLastYear = ClientsDealsCRMLastYear.Sum(a => a.OldClientsCount),
                        DealsCountLastYear = ClientsDealsCRMLastYear.Sum(a => a.OldClientsDeals),
                        TotalDealsExtraCosts = ClientsDealsCRM.Sum(a => a.OldClientsProjectExtraModifications)
                    };
                    //Closing Rate (Old supplier)
                    NewEngaging.ClosingRatePercentage = OldClientsRFQCount != 0 ? (String.Format("{0:0.0}", (decimal)NewEngaging.DealedClientsCount / (decimal)OldClientsRFQCount * 100) + "%") : "0%";

                    //Engaging Rate = Sum of Dealed Old Clients This Period / Total Sum Of Old Clients This Period (Not Only Has RFQ) 
                    NewEngaging.EngagingRatePercentage = OldClientsCount != 0 ? (String.Format("{0:0.0}", (decimal)NewEngaging.DealedClientsCount / (decimal)OldClientsCount * 100) + "%") : "0%";

                    if (NewEngaging.TotalEngagingRate > OldClientsTotalEngagingRateLastYear)
                        NewEngaging.EngagingRateState = "Up";
                    else
                        NewEngaging.EngagingRateState = "Down";
                    Response.EngagingRate = NewEngaging;

                    Response.NewClientsSupportedByList = ClientsDealsCRM.Select(sb => new ClientsStatistics
                    {
                        DealsCount = sb.NewClientsDeals,
                        TotalDealsExtraCostPrice = sb.NewClientsProjectExtraModifications,
                        SupportedBy = sb.SupportedBy,
                        ClientsCount = sb.NewClientsCount,
                        ClientsRFQCount = sb.NewClientsRFQCount,
                        DealedClientsCount = sb.NewDealedClients,
                        TotalDealsPrice = sb.TotalDealsPriceNewClients,
                        ClientsCountLastYear = sb.NewClientsCountLastYear,
                        TotalDealsPriceLastYear = sb.TotalDealsPriceNewClientsLastYear,
                        ClientsState = sb.NewClientsCountLastYear >= sb.NewClientsCount ? "Down" : "Up",
                        DealsState = sb.TotalDealsPriceNewClientsLastYear >= sb.TotalDealsPriceNewClients ? "Down" : "Up",
                    }).ToList();

                    var NewClientsCount = ClientsDealsCRM.Sum(a => a.NewClientsCount);
                    var NewClientsRFQCount = ClientsDealsCRM.Sum(a => a.NewClientsRFQCount);
                    Response.NewClientsCount = NewClientsCount;
                    Response.NewClientsRFQCount = NewClientsRFQCount;
                    var NewClientsCountLastYear = ClientsDealsCRM.Sum(a => a.NewClientsCountLastYear);
                    Response.NewClientsCountLastYear = NewClientsCountLastYear;

                    var ExpiredNewClientsQuery = _unitOfWork.VClientUseers.FindAllQueryable(a => a.CreationDate >= StartDateThisYear && a.CreationDate < EndDateThisYear && a.ClientExpireDate < DateTime.Now && a.NeedApproval == 0).AsQueryable();
                    var WillExpiredNewClientsQuery = _unitOfWork.VClientUseers.FindAllQueryable(a => a.CreationDate >= StartDateThisYear && a.CreationDate < EndDateThisYear && a.ClientExpireDate < DateAfterTenDays && a.ClientExpireDate > DateTime.Now && a.NeedApproval == 0).AsQueryable();
                    if (filters.BranchId != 0)
                    {
                        ExpiredNewClientsQuery = ExpiredNewClientsQuery.Where(a => a.BranchId == filters.BranchId);
                        WillExpiredNewClientsQuery = WillExpiredNewClientsQuery.Where(a => a.BranchId == filters.BranchId);
                    }

                    if (filters.SalesPersonId != 0)
                    {
                        ExpiredNewClientsQuery = ExpiredNewClientsQuery.Where(a => a.SalesPersonId == filters.SalesPersonId);
                        WillExpiredNewClientsQuery = WillExpiredNewClientsQuery.Where(a => a.SalesPersonId == filters.SalesPersonId);
                    }
                    Response.NewExpiredClients = await ExpiredNewClientsQuery.CountAsync();
                    Response.NewWillExpiredClients = await WillExpiredNewClientsQuery.CountAsync();

                    var NewClientsLastYearCount = ClientsDealsCRMLastYear.Sum(a => a.NewClientsCount);

                    var NewClientsTotalAcquisitionRateLastYear = ClientsDealsCRMLastYear.Sum(a => a.TotalDealsPriceNewClients);

                    AcquisitionRate NewAcquisition = new AcquisitionRate
                    {
                        DealedClientsCount = ClientsDealsCRM.Sum(a => a.NewDealedClients),
                        ClientsCount = ClientsDealsCRM.Sum(a => a.NewClientsCount),
                        ClientsRFQCount = ClientsDealsCRM.Sum(a => a.NewClientsRFQCount),
                        DealsCount = ClientsDealsCRM.Sum(a => a.NewClientsDeals),
                        TotalAcquisitionRate = ClientsDealsCRM.Sum(a => a.TotalDealsPriceNewClients),
                        TotalAcquisitionRateLastYear = NewClientsTotalAcquisitionRateLastYear,
                        ClientsCountLastYear = ClientsDealsCRMLastYear.Sum(a => a.NewClientsCount),
                        DealsCountLastYear = ClientsDealsCRMLastYear.Sum(a => a.NewClientsDeals),
                        TotalDealsExtraCosts = ClientsDealsCRM.Sum(a => a.NewClientsProjectExtraModifications)
                    };

                    //Closing Rate (New supplier)
                    NewAcquisition.ClosingRatePercentage = NewClientsRFQCount != 0 ? (String.Format("{0:0.0}", (decimal)NewAcquisition.DealedClientsCount / (decimal)NewClientsRFQCount * 100) + "%") : "0%";

                    //Aquesition Rate = Sum of Dealed New Clients This Period / Total Sum Of New Clients This Period (Not Only Has RFQ) 
                    NewAcquisition.AcquisitionRatePercentage = NewClientsCount != 0 ? (String.Format("{0:0.0}", (decimal)NewAcquisition.DealedClientsCount / (decimal)NewClientsCount * 100) + "%") : "0%";

                    if (NewAcquisition.TotalAcquisitionRate > NewClientsTotalAcquisitionRateLastYear)
                        NewAcquisition.AcquisitionRateState = "Up";
                    else
                        NewAcquisition.AcquisitionRateState = "Down";
                    Response.AcquisitionRate = NewAcquisition;

                    if (NewClientsCount > NewClientsLastYearCount)
                        Response.NewClientsState = "Up";
                    else
                        Response.NewClientsState = "Down";

                    var SalesTarget = new AchievedTarget();

                    var salesTargetDb = (await _unitOfWork.SalesTargets.FindAllAsync(a => a.Year != null && a.Year == StartDate.Year)).FirstOrDefault();
                    var salesTargetLastYearDb = (await _unitOfWork.SalesTargets.FindAllAsync(a => a.Year == (StartDate.Year - 1))).FirstOrDefault();

                    if (salesTargetDb != null)
                    {
                        SalesTarget.TargetAmount = salesTargetDb.Target;
                        SalesTarget.Currency = (await _unitOfWork.Currencies.FindAllAsync(a => a.Id == salesTargetDb.CurrencyId)).Select(a => a.Name).FirstOrDefault();
                    }

                    SalesTarget.Achieved = NewAcquisition.TotalAcquisitionRate + NewEngaging.TotalEngagingRate;
                    SalesTarget.TargetExtraCostAmount = NewAcquisition.TotalDealsExtraCosts + NewEngaging.TotalDealsExtraCosts;

                    decimal TargetPercentage = 0;
                    if (SalesTarget.TargetAmount > 0)
                    {
                        TargetPercentage = SalesTarget.Achieved / SalesTarget.TargetAmount * 100;
                        SalesTarget.AchievedPercentage = String.Format("{0:0.0}", TargetPercentage) + "%";
                    }
                    else
                        SalesTarget.AchievedPercentage = "0%";

                    var AchievedTargetLastYear = NewClientsTotalAcquisitionRateLastYear + OldClientsTotalEngagingRateLastYear;
                    SalesTarget.AchievedLastYear = AchievedTargetLastYear;
                    SalesTarget.TargetExtraCostsAmountLastYear = ClientsDealsCRMLastYear.Sum(a => a.NewClientsProjectExtraModifications) + ClientsDealsCRMLastYear.Sum(a => a.OldClientsProjectExtraModifications); ;

                    decimal TargetPercentageLastYear = 0;
                    if (salesTargetLastYearDb != null)
                    {
                        SalesTarget.TargetAmountLastYear = salesTargetLastYearDb.Target;
                        if (salesTargetLastYearDb.Target > 0)
                        {
                            TargetPercentageLastYear = AchievedTargetLastYear / salesTargetLastYearDb.Target * 100;
                            SalesTarget.AchievedPercentageLastYear = TargetPercentageLastYear + "%";
                        }
                    }
                    if (TargetPercentage > TargetPercentageLastYear)
                    {
                        SalesTarget.AchievedState = "Up";
                    }
                    else
                    {
                        SalesTarget.AchievedState = "Down";
                    }

                    Response.AchievedTarget = SalesTarget;


                    Response.TotalClientsCount = NewClientsCount + OldClientsCount;
                    Response.TotalClientsRFQCount = NewClientsRFQCount + OldClientsRFQCount;
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


        public async Task<SalesPersonsClientsDetailsResponse> GetSalesPersonsClientsDetails(GetMyClientsDetailsCRMFilters filters)
        {
            SalesPersonsClientsDetailsResponse Response = new SalesPersonsClientsDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                var EndDate = new DateTime((DateTime.Now.Year + 1), 1, 1);


                if (filters.Year > 0)
                {
                    if (filters.Month > 0)
                    {
                        StartDate = new DateTime(filters.Year, filters.Month, 1);

                        if (filters.Month != 12)
                        {
                            EndDate = new DateTime(filters.Year, (filters.Month + 1), 1);
                        }
                        else
                        {
                            EndDate = new DateTime((filters.Year + 1), 1, 1);
                        }
                    }
                    else
                    {
                        StartDate = new DateTime(filters.Year, 1, 1);
                        EndDate = new DateTime((filters.Year + 1), 1, 1);
                    }
                }
                else
                {
                    if (filters.Month > 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "ErrCRM1";
                        error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                        Response.Errors.Add(error);

                        return Response;
                    }
                }
                var StartDateLastYear = StartDate.AddYears(-1);
                var EndDateLastYear = EndDate.AddYears(-1);

                List<SalesPersonClientsStatistics> salesPersonsClientsDetailsList = new List<SalesPersonClientsStatistics>();

                var SalesPersons = (await _unitOfWork.VGroupUsers.FindAllAsync(a => a.Id == 4)).ToList();

                var Sp_BranchId = new SqlParameter("BranchId", System.Data.SqlDbType.Int);
                if (filters.BranchId != 0)
                {
                    Sp_BranchId.Value = filters.BranchId;
                }
                var spUserID = new SqlParameter("SalesPersonId", System.Data.SqlDbType.BigInt);
                var sp_startdate = new SqlParameter("StartDate", System.Data.SqlDbType.DateTime);
                sp_startdate.Value = StartDate;
                var sp_enddate = new SqlParameter("EndDate", System.Data.SqlDbType.DateTime);
                sp_enddate.Value = EndDate;
                var sp_WithProjectExtraModifications = new SqlParameter("WithProjectExtraModifications", System.Data.SqlDbType.Bit);
                sp_WithProjectExtraModifications.Value = filters.WithProjectExtraModifications;
                foreach (var SP in SalesPersons)
                {
                    spUserID.Value = SP.UserId;
                    object[] param = new object[] { spUserID, Sp_BranchId, sp_startdate, sp_enddate, sp_WithProjectExtraModifications };
                    List<STP_ClientsCRM_Result> ClientsCRM = _Context.Database.SqlQueryRaw<STP_ClientsCRM_Result>("Exec STP_ClientsCRM @SalesPersonId ,@BranchId ,@StartDate ,@EndDate ,@WithProjectExtraModifications", param).AsEnumerable().ToList();

                    List<ClientsStatistics> OldClients = ClientsCRM.Select(sb => new ClientsStatistics
                    {
                        DealsCount = sb.OldClientsDeals,
                        TotalDealsExtraCostPrice = sb.OldClientsProjectExtraModifications,
                        SupportedBy = sb.SupportedBy,
                        ClientsCount = sb.OldClientsCount,
                        DealedClientsCount = sb.OldDealedClients,
                        TotalDealsPrice = sb.TotalDealsPriceOldClients
                    }).ToList();

                    List<ClientsStatistics> NewClients = ClientsCRM.Select(sb => new ClientsStatistics
                    {
                        DealsCount = sb.NewClientsDeals,
                        TotalDealsExtraCostPrice = sb.NewClientsProjectExtraModifications,
                        SupportedBy = sb.SupportedBy,
                        ClientsCount = sb.NewClientsCount,
                        DealedClientsCount = sb.NewDealedClients,
                        TotalDealsPrice = sb.TotalDealsPriceNewClients,
                        ClientsCountLastYear = sb.NewClientsCountLastYear,
                        TotalDealsPriceLastYear = sb.TotalDealsPriceNewClientsLastYear,
                        ClientsState = sb.NewClientsCountLastYear >= sb.NewClientsCount ? "Down" : "Up",
                        DealsState = sb.TotalDealsPriceNewClientsLastYear >= sb.TotalDealsPriceNewClients ? "Down" : "Up",
                    }).ToList();

                    SalesPersonClientsStatistics salesPersonClientsStatistics = new SalesPersonClientsStatistics()
                    {
                        SalesPersonId = SP.UserId,
                        SalesPersonName = SP.UserName,
                        OldClientsStatisticsList = OldClients,
                        NewClientsStatisticsList = NewClients
                    };

                    salesPersonsClientsDetailsList.Add(salesPersonClientsStatistics);
                }

                Response.SalesPersonsClientsDetailsList = new List<SalesPersonClientsStatistics>();

                Response.SalesPersonsClientsDetailsList = salesPersonsClientsDetailsList;


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


        public async Task<SalesPersonClientsListResponse> SalesPersonClientsList([FromHeader] long SalesPersonId, [FromHeader] string SupportedBy, [FromHeader] int Month, [FromHeader] int Year)
        {
            SalesPersonClientsListResponse Response = new SalesPersonClientsListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (SalesPersonId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "ErrCRM1";
                        error.ErrorMSG = "SalesPersonId Is Mandatory";
                        Response.Errors.Add(error);

                        return Response;
                    }
                    var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var EndDate = new DateTime((DateTime.Now.Year + 1), 1, 1);

                    if (Year > 0)
                    {
                        if (Month > 0)
                        {
                            StartDate = new DateTime(Year, Month, 1);

                            if (Month != 12)
                            {
                                EndDate = new DateTime(Year, (Month + 1), 1);
                            }
                            else
                            {
                                EndDate = new DateTime((Year + 1), 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(Year, 1, 1);
                            EndDate = new DateTime((Year + 1), 1, 1);
                        }
                    }
                    else
                    {
                        if (Month > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }
                    var StartDateLastYear = StartDate.AddYears(-1);
                    var EndDateLastYear = EndDate.AddYears(-1);

                    Response.SalesPersonClientsList = (await _unitOfWork.Clients.FindAllAsync(a => a.NeedApproval == 0 && a.SalesOffers.Any(x => x.SalesPersonId == SalesPersonId) && a.CreationDate >= StartDate && a.CreationDate < EndDate && (SupportedBy == null || SupportedBy == "" || (SupportedBy != "Other" ? a.SupportedBy == SupportedBy : a.SupportedBy == null || a.SupportedBy == "Other")))).Select(clnt => new SelectDDL { ID = clnt.Id, Name = clnt.Name }).ToList();

                    Response.ClientsCount = Response.SalesPersonClientsList.Count();
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


        public async Task<SalesPersonsProductResponse> GetProductsSalesPersons([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] long ProductId)
        {
            SalesPersonsProductResponse Response = new SalesPersonsProductResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var EndDate = new DateTime((DateTime.Now.Year + 1), 1, 1);
                    if (Year > 0)
                    {
                        if (Month > 0)
                        {
                            StartDate = new DateTime(Year, Month, 1);

                            if (Month != 12)
                            {
                                EndDate = new DateTime(Year, Month + 1, 1);
                            }
                            else
                            {
                                EndDate = new DateTime(Year + 1, 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(Year, 1, 1);
                            EndDate = new DateTime(Year + 1, 1, 1);
                        }
                    }
                    else
                    {
                        if (Month > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }

                    var SellingProductsDbQuery = _unitOfWork.VSalesOfferProductSalesOffers.FindAllQueryable(a => a.Status == "Closed" && a.Active == true).AsQueryable();

                    SellingProductsDbQuery = SellingProductsDbQuery.Where(a => a.CreationDate >= StartDate && a.CreationDate < EndDate);

                    if (BranchId != 0)
                    {
                        SellingProductsDbQuery = SellingProductsDbQuery.Where(a => a.BranchId == BranchId);
                    }

                    if (ProductId != 0)
                    {
                        SellingProductsDbQuery = SellingProductsDbQuery.Where(a => a.InventoryItemId == ProductId);
                    }
                    var test = SellingProductsDbQuery.ToList();
                    var SellingProductsDbList = (await SellingProductsDbQuery.Where(a => a.OfferType != "Sales Return").ToListAsync()).GroupBy(a => new { a.SalesPersonId }).ToList();
                    var ReturnSellingProductsDbList = await SellingProductsDbQuery.Where(a => a.OfferType == "Sales Return").ToListAsync();
                    var SalesPersonProductsList = new List<SalesPersonProducts>();

                    foreach (var SalesPersonSellingProducts in SellingProductsDbList)
                    {
                        if (SalesPersonSellingProducts != null)
                        {
                            var salesPerson = _unitOfWork.Users.GetById((long)SalesPersonSellingProducts.Key.SalesPersonId);

                            SalesPersonProducts SPP = new SalesPersonProducts()
                            {
                                SalesPersonId = SalesPersonSellingProducts.Key.SalesPersonId ?? 0,
                                SalesPersonName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName
                            };


                            var SellingProductsGrouped = SalesPersonSellingProducts.GroupBy(a => new { a.InventoryItemId, a.Name }).ToList();
                            var SellingProductsList = new List<SellingProductsCRM>();
                            foreach (var SellingProduct in SellingProductsGrouped)
                            {
                                if (SellingProduct != null)
                                {
                                    var TotalReturnPrice = ReturnSellingProductsDbList.Where(a => a.InventoryItemId == SellingProduct.Key.InventoryItemId).Sum(a => a.ItemPrice * (decimal)a.Quantity);
                                    var TotalReturnCount = ReturnSellingProductsDbList.Where(a => a.InventoryItemId == SellingProduct.Key.InventoryItemId).Count();
                                    var SellingProductObj = new SellingProductsCRM()
                                    {
                                        ProductId = SellingProduct.Key.InventoryItemId ?? 0,
                                        ProductName = SellingProduct.Key.Name != null ? SellingProduct.Key.Name.Trim() : "",
                                        SoldCount = SellingProduct.Count() - TotalReturnCount,
                                        TotalSoldPrice = SellingProduct.Sum(a => a.ItemPrice * (decimal)a.Quantity) - (TotalReturnPrice ?? 0) ?? 0
                                    };

                                    SellingProductsList.Add(SellingProductObj);
                                }

                            }

                            SPP.SalesPersonProductsList = SellingProductsList;

                            SalesPersonProductsList.Add(SPP);
                        }


                    }

                    Response.SalesPersonsProductsList = SalesPersonProductsList;

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


        public async Task<MyOffersCRMDashboardResponse> GetMyOffersDetailsCRM([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] long SalesPersonId)
        {
            MyOffersCRMDashboardResponse Response = new MyOffersCRMDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var OffersDbQuery = _unitOfWork.VSalesOfferClients.FindAllQueryable(a => true);

                    var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var EndDate = new DateTime((DateTime.Now.Year + 1), 1, 1);
                    if (Year > 0)
                    {
                        if (Month > 0)
                        {
                            StartDate = new DateTime(Year, Month, 1);

                            if (Month != 12)
                            {
                                EndDate = new DateTime(Year, (Month + 1), 1);
                            }
                            else
                            {
                                EndDate = new DateTime((Year + 1), 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(Year, 1, 1);
                            EndDate = new DateTime((Year + 1), 1, 1);
                        }
                    }
                    else
                    {
                        if (Month > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }

                    OffersDbQuery = OffersDbQuery.Where(a => a.CreationDate >= StartDate && a.CreationDate < EndDate);

                    if (BranchId != 0)
                    {
                        OffersDbQuery = OffersDbQuery.Where(a => a.SalesPersonBranchId == BranchId);
                    }

                    if (SalesPersonId != 0)
                    {
                        OffersDbQuery = OffersDbQuery.Where(a => a.SalesPersonId == SalesPersonId);
                    }

                    var OffersDbList = await OffersDbQuery.ToListAsync();

                    var Today = DateTime.Now.Date;
                    Response.TotalOffersCount = OffersDbList.Where(a => a.Status == "Recieved" || a.Status == "ClientApproval" || a.Status == "Pricing").Count();
                    Response.TotalOffersPrice = OffersDbList.Where(a => a.Status == "Recieved" || a.Status == "ClientApproval").Sum(a => a.FinalOfferPrice) ?? 0;

                    Response.UnderPricingOffersCount = OffersDbList.Where(a => a.Status == "Pricing").Count();
                    Response.UnderPricingDelayCount = OffersDbList.Where(a => a.Status == "Pricing" && a.EndDate < DateOnly.FromDateTime(Today)).Count();

                    Response.SendingOffersCount = OffersDbList.Where(a => a.Status == "Recieved").Count();
                    Response.SendingOffersPrice = OffersDbList.Where(a => a.Status == "Recieved").Sum(a => a.FinalOfferPrice) ?? 0;

                    Response.WaitingApprovalCount = OffersDbList.Where(a => a.Status == "ClientApproval").Count();
                    Response.WaitingApprovalPrice = OffersDbList.Where(a => a.Status == "ClientApproval").Sum(a => a.FinalOfferPrice) ?? 0;
                    Response.ApprovalDelayCount = OffersDbList.Where(a => a.Status == "ClientApproval" && a.ReminderDate < Today).Count();
                    Response.ApprovalWillExpireCount = OffersDbList.Where(
                        a => a.Status == "ClientApproval" && a.OfferExpirationDate > Today.AddDays(-((double)(a.OfferExpirationPeriod ?? 0) * 7 * 20) / 100) && a.OfferExpirationDate < Today).Count();
                    Response.ApprovalExpiredCount = OffersDbList.Where(a => a.Status == "ClientApproval" && a.OfferExpirationDate >= Today).Count();

                    Response.ClosedOffersCount = OffersDbList.Where(a => a.Status == "Closed").Count();
                    var ClosedOfferPrice = OffersDbList.Where(a => a.Status == "Closed" && a.OfferType != "Sales Return").Sum(a => a.FinalOfferPrice) ?? 0;
                    var ReturnClosedPrice = OffersDbList.Where(a => a.Status == "Closed" && a.OfferType == "Sales Return").Sum(a => a.FinalOfferPrice) ?? 0;
                    Response.ClosedOffersPrice = ClosedOfferPrice - ReturnClosedPrice;

                    Response.RejectedOffersCount = OffersDbList.Where(a => a.Status == "Rejected").Count();
                    Response.RejectedOffersPrice = OffersDbList.Where(a => a.Status == "Rejected").Sum(a => a.FinalOfferPrice) ?? 0;
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

        public async Task<SalesPersonsOffersDetailsResponse> GetSalesPersonOffersDetails([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] long SalesPersonId)
        {
            SalesPersonsOffersDetailsResponse Response = new SalesPersonsOffersDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    var OffersDbQuery = _unitOfWork.VSalesOfferClients.FindAllQueryable(a => true);

                    var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var EndDate = new DateTime((DateTime.Now.Year + 1), 1, 1);
                    if (Year > 0)
                    {
                        if (Month > 0)
                        {
                            StartDate = new DateTime(Year, Month, 1);

                            if (Month != 12)
                            {
                                EndDate = new DateTime(Year, (Month + 1), 1);
                            }
                            else
                            {
                                EndDate = new DateTime((Year + 1), 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(Year, 1, 1);
                            EndDate = new DateTime((Year + 1), 1, 1);
                        }
                    }
                    else
                    {
                        if (Month > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }

                    OffersDbQuery = OffersDbQuery.Where(a => a.CreationDate >= StartDate && a.CreationDate < EndDate);

                    if (BranchId != 0)
                    {
                        OffersDbQuery = OffersDbQuery.Where(a => a.SalesPersonBranchId == BranchId);
                    }

                    if (SalesPersonId != 0)
                    {
                        OffersDbQuery = OffersDbQuery.Where(a => a.SalesPersonId == SalesPersonId);
                    }
                    var GroupedOffers = (await OffersDbQuery.ToListAsync()).GroupBy(a => a.SalesPersonId).ToList();

                    var SalesPerosnsOffersDetailsList = new List<SalesPersonOffersDetails>();

                    var Today = DateTime.Now.Date;
                    foreach (var offerGrp in GroupedOffers)
                    {
                        if (offerGrp != null)
                        {
                            var salesPersonName = "";
                            var salesPerson = _unitOfWork.Users.GetById(offerGrp.Key);
                            if (salesPerson != null)
                            {
                                salesPersonName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName;
                            }
                            var ClosedOfferPrice = offerGrp.Where(a => a.Status == "Closed" && a.OfferType != "Sales Return").Sum(a => a.FinalOfferPrice) ?? 0;
                            var ReturnClosedPrice = offerGrp.Where(a => a.Status == "Closed" && a.OfferType == "Sales Return").Sum(a => a.FinalOfferPrice) ?? 0;
                            var SalesPersonOffersDetailsObj = new SalesPersonOffersDetails
                            {
                                SalesPersonId = offerGrp.Key,
                                SalesPersonName = salesPersonName,

                                TotalOffersCount = offerGrp.Where(a => a.Status == "Recieved" || a.Status == "ClientApproval" || a.Status == "Pricing").Count(),
                                TotalOffersPrice = offerGrp.Where(a => a.Status == "Recieved" || a.Status == "ClientApproval").Sum(a => a.FinalOfferPrice) ?? 0,

                                UnderPricingOffersCount = offerGrp.Where(a => a.Status == "Pricing").Count(),
                                UnderPricingDelayCount = offerGrp.Where(a => a.Status == "Pricing" && a.EndDate < DateOnly.FromDateTime(Today)).Count(),

                                SendingOffersCount = offerGrp.Where(a => a.Status == "Recieved").Count(),
                                SendingOffersPrice = offerGrp.Where(a => a.Status == "Recieved").Sum(a => a.FinalOfferPrice) ?? 0,

                                WaitingApprovalCount = offerGrp.Where(a => a.Status == "ClientApproval").Count(),
                                WaitingApprovalPrice = offerGrp.Where(a => a.Status == "ClientApproval").Sum(a => a.FinalOfferPrice) ?? 0,
                                ApprovalDelayCount = offerGrp.Where(a => a.Status == "ClientApproval" && a.ReminderDate < Today).Count(),
                                ApprovalWillExpireCount = offerGrp.Where(
                                a => a.Status == "ClientApproval" && a.OfferExpirationDate > Today.AddDays(-((double)(a.OfferExpirationPeriod ?? 0) * 7 * 20) / 100) && a.OfferExpirationDate < Today).Count(),
                                ApprovalExpiredCount = offerGrp.Where(a => a.Status == "ClientApproval" && a.OfferExpirationDate >= Today).Count(),

                                ClosedOffersCount = offerGrp.Where(a => a.Status == "Closed").Count(),

                                RejectedOffersCount = offerGrp.Where(a => a.Status == "Rejected").Count(),
                                RejectedOffersPrice = offerGrp.Where(a => a.Status == "Rejected").Sum(a => a.FinalOfferPrice) ?? 0

                            };
                            SalesPersonOffersDetailsObj.ClosedOffersPrice = ClosedOfferPrice - ReturnClosedPrice;

                            SalesPerosnsOffersDetailsList.Add(SalesPersonOffersDetailsObj);
                        }
                    }

                    Response.SalesPersonsOffersDetailsList = SalesPerosnsOffersDetailsList;
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


        public async Task<MyReportsCRMDashboardResponse> GetMyReportsDetailsCRM(GetMyReportsDetailsCRMFilters filters)
        {
            MyReportsCRMDashboardResponse Response = new MyReportsCRMDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (!string.IsNullOrEmpty(filters.ClientStatus))
                    {
                        if (filters.ClientStatus.ToLower() != "new" && filters.ClientStatus.ToLower() != "old")
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "ClientStatus must be New Or Old";
                            Response.Errors.Add(error);
                        }
                    }

                    var reportsDBQuery = _unitOfWork.VCrmreports.FindAllQueryable(a => true);
                    var DailyReportsDBQuery = _unitOfWork.VDailyReportReportLineThroughApis.FindAllQueryable(a => a.Status != "Not Filled").AsQueryable();

                    if (filters.SalesPersonId != 0)
                    {
                        reportsDBQuery = reportsDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId);
                        DailyReportsDBQuery = DailyReportsDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId);
                    }

                    if (filters.ReportCreator != 0)
                    {
                        reportsDBQuery = reportsDBQuery.Where(a => a.CrmuserId == filters.ReportCreator);
                        DailyReportsDBQuery = DailyReportsDBQuery.Where(a => a.UserId == filters.ReportCreator);
                    }

                    if (filters.BranchId != 0)
                    {
                        reportsDBQuery = reportsDBQuery.Where(a => a.BranchId == filters.BranchId);
                        DailyReportsDBQuery = DailyReportsDBQuery.Where(a => a.BranchId == filters.BranchId);
                    }

                    var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);
                    if (filters.Year > 0)
                    {
                        if (filters.Month > 0)
                        {
                            StartDate = new DateTime(filters.Year, filters.Month, 1);

                            if (filters.Month != 12)
                            {
                                EndDate = new DateTime(filters.Year, (filters.Month + 1), 1);
                            }
                            else
                            {
                                EndDate = new DateTime((filters.Year + 1), 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(filters.Year, 1, 1);
                            EndDate = new DateTime((filters.Year + 1), 1, 1);
                        }

                    }
                    else
                    {
                        if (filters.Month > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }
                    reportsDBQuery = reportsDBQuery.Where(a => a.ReportDate >= StartDate && a.ReportDate < EndDate);
                    DailyReportsDBQuery = DailyReportsDBQuery.Where(a => a.ReprotDate >= StartDate && a.ReprotDate < EndDate);

                    var ClientIds = new List<long>();

                    if (filters.ClientId != 0)
                    {
                        ClientIds.Add(filters.ClientId);
                    }

                    if (filters.ClientStatus?.ToLower() == "old")
                    {
                        var YearBegining = StartDate.AddMonths(-(StartDate.Month - 1));
                        var OldClientIds = _unitOfWork.Clients.FindAll(a => a.CreationDate < YearBegining && a.NeedApproval == 0).Select(a => a.Id).ToList();
                        ClientIds.AddRange(OldClientIds);
                    }
                    else if (filters.ClientStatus?.ToLower() == "new")
                    {
                        var NewClientIds = _unitOfWork.Clients.FindAll(a => a.CreationDate >= StartDate && a.CreationDate < EndDate && a.NeedApproval == 0).Select(a => a.Id).ToList();
                        ClientIds.AddRange(NewClientIds);
                    }

                    var reportsList = await reportsDBQuery.ToListAsync();
                    var dailyReportsList = await DailyReportsDBQuery.ToListAsync();

                    if (ClientIds != null && ClientIds.Any())
                    {
                        reportsList = reportsList.Where(a => ClientIds.Contains(a.ClientId)).ToList();
                        dailyReportsList = dailyReportsList.Where(a => ClientIds.Contains(a.ClientId ?? 0)).ToList();
                    }

                    Response.TotalCRMRecievedPhoneCount = reportsList.Where(a => a.RecievedName == "Phonedd").Count();
                    Response.TotalCRMRecievedWhatsappCount = reportsList.Where(a => a.RecievedName == "Whatsapp").Count();
                    Response.TotalCRMRecievedEmailCount = reportsList.Where(a => a.RecievedName == "Email").Count();
                    Response.TotalCRMRecievedOtherCount = reportsList.Where(a => a.RecievedName == "Other").Count();

                    Response.TotalCRMRecievedMeetingsCount = reportsList.Where(a => a.RecievedName == "Meeting").Count();

                    Response.TotalCRMSentPhoneCount = reportsList.Where(a => a.ContactName == "Phone1").Count();
                    Response.TotalCRMSentWhatsappCount = reportsList.Where(a => a.ContactName == "Whatsapp").Count();
                    Response.TotalCRMSentEmailCount = reportsList.Where(a => a.ContactName == "Email").Count();
                    Response.TotalCRMSentOtherCount = reportsList.Where(a => a.RecievedName == "Other").Count();

                    Response.TotalCRMSentMeetingsCount = reportsList.Where(a => a.ContactName == "Meeting").Count();

                    Response.TotalCRMReportsCount = reportsList.Count();


                    Response.TotalSalesPhoneCount = dailyReportsList.Where(a => a.Name == "Phone").Count();
                    Response.TotalSalesWhatsappCount = dailyReportsList.Where(a => a.Name == "Whatsapp").Count();
                    Response.TotalSalesEmailCount = dailyReportsList.Where(a => a.Name == "Email").Count();
                    Response.TotalSalesOtherCount = dailyReportsList.Where(a => a.Name == "Other").Count();

                    Response.TotalSalesMeetingsCount = dailyReportsList.Where(a => a.Name == "Visit" || a.Name == "Visit Of Client").Count();

                    var TotalSalesReportsCount = dailyReportsList.Count();
                    Response.TotalSalesReportsCount = TotalSalesReportsCount;

                    double ReviewAvg = 0;
                    if (TotalSalesReportsCount > 0)
                    {
                        ReviewAvg = dailyReportsList.Sum(a => a.Review ?? 0) / TotalSalesReportsCount;
                    }
                    Response.SalesReportsReviewAvg = String.Format("{0:0.0}", ReviewAvg) + "%";

                    var CRMReasonsGroups = reportsList.GroupBy(a => a.ReasonName).ToList();
                    var CRMReasonsList = new List<ReportReason>();
                    foreach (var reasonGroup in CRMReasonsGroups)
                    {
                        var CRMReasonObj = new ReportReason();
                        if (reasonGroup.Key != null && reasonGroup.Key != "")
                        {
                            CRMReasonObj.ReasonName = reasonGroup.Key;
                        }
                        else
                        {
                            CRMReasonObj.ReasonName = "Other";
                        }
                        CRMReasonObj.ReasonCount = reasonGroup.Count();

                        CRMReasonsList.Add(CRMReasonObj);
                    }
                    CRMReasonsList = CRMReasonsList.OrderByDescending(a => a.ReasonCount).ToList();

                    Response.CrmReportReasons = CRMReasonsList;

                    var SalesReasonsGroups = dailyReportsList.GroupBy(a => a.ReasonTypeName).ToList();
                    var SalesReasonsList = new List<ReportReason>();
                    foreach (var reasonGroup in SalesReasonsGroups)
                    {
                        var SalesReasonObj = new ReportReason();
                        if (reasonGroup.Key != null && reasonGroup.Key != "")
                        {
                            SalesReasonObj.ReasonName = reasonGroup.Key;
                        }
                        else
                        {
                            SalesReasonObj.ReasonName = "Other";
                        }
                        SalesReasonObj.ReasonCount = reasonGroup.Count();

                        SalesReasonsList.Add(SalesReasonObj);
                    }
                    SalesReasonsList = SalesReasonsList.OrderByDescending(a => a.ReasonCount).ToList();

                    Response.SalesReportReasons = SalesReasonsList;

                    var TotalReasonsList = new List<ReportReason>();
                    TotalReasonsList.AddRange(CRMReasonsList);
                    TotalReasonsList.AddRange(SalesReasonsList);

                    var ListOfReasonsTypes = TotalReasonsList.Select(a => a.ReasonName).Distinct().ToList();

                    var MergedReasonsList = new List<ReportReason>();
                    foreach (var item in ListOfReasonsTypes)
                    {
                        var TotalReasonSum = TotalReasonsList.Where(a => a.ReasonName == item).Sum(a => a.ReasonCount);
                        MergedReasonsList.Add(new ReportReason()
                        {
                            ReasonName = item,
                            ReasonCount = TotalReasonSum
                        });
                    }

                    Response.TotalReportReasons = MergedReasonsList;

                    var CRMClientsIDs = reportsList.Where(a => a.CustomerSatisfaction != null).Select(a => a.ClientId).Distinct().ToList();
                    var CRMClientsCount = CRMClientsIDs.Count();

                    var SalesClientsIDs = dailyReportsList.Where(a => a.CustomerSatisfaction != null).Select(a => a.ClientId).Distinct().ToList();
                    var SalesClientsCount = SalesClientsIDs.Count();
                    var totalIds = new List<long>();
                    if (CRMClientsIDs != null)
                        totalIds.AddRange(CRMClientsIDs);
                    if (SalesClientsIDs != null)
                    {
                        foreach (var id in SalesClientsIDs)
                        {
                            totalIds.Add((long)id);
                        }
                    }

                    if (!string.IsNullOrEmpty(filters.ClientStatus))
                    {
                        totalIds = totalIds.Intersect(ClientIds).ToList();
                    }



                    decimal ClientsTotalCRMAvg = 0;
                    decimal ClientsTotalSalesAvg = 0;
                    decimal ClientsTotalAvg = 0;
                    foreach (var clientId in totalIds)
                    {
                        var CRMSatSum = reportsList.Where(a => a.CustomerSatisfaction != null && a.ClientId == clientId).Select(a => a.CustomerSatisfaction).Sum() ?? 0;
                        var CRMSatCount = reportsList.Where(a => a.CustomerSatisfaction != null && a.ClientId == clientId).Select(a => a.CustomerSatisfaction).Count();

                        var SalesSatSum = dailyReportsList.Where(a => a.CustomerSatisfaction != null && a.ClientId == clientId).Select(a => a.CustomerSatisfaction).Sum() ?? 0;
                        var SalesSatCount = dailyReportsList.Where(a => a.CustomerSatisfaction != null && a.ClientId == clientId).Select(a => a.CustomerSatisfaction).Count();


                        var CRMAvg = CRMSatCount == 0 ? 0 : (CRMSatSum / CRMSatCount);
                        ClientsTotalCRMAvg += CRMAvg;

                        var SalesAvg = SalesSatCount == 0 ? 0 : (SalesSatSum / SalesSatCount);
                        ClientsTotalSalesAvg += SalesAvg;

                        decimal clientAvg = (CRMSatSum + SalesSatSum) / (CRMSatCount + SalesSatCount);
                        ClientsTotalAvg += clientAvg;
                    }

                    var TotalCRMAvg = CRMClientsCount != 0 ? (ClientsTotalCRMAvg / CRMClientsCount) : 0;
                    Response.CrmCustomerSatisfactionPercentage = String.Format("{0:0.0}", TotalCRMAvg) + "%";

                    var TotalSalesAvg = SalesClientsCount != 0 ? (ClientsTotalSalesAvg / SalesClientsCount) : 0;
                    Response.SalesCustomerSatisfactionPercentage = String.Format("{0:0.0}", TotalSalesAvg) + "%";

                    var TotalAvg = totalIds.Count() != 0 ? (ClientsTotalAvg / totalIds.Count()) : 0;
                    Response.TotalCustomerSatisfactionPercentage = String.Format("{0:0.0}", TotalAvg) + "%";
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


        public ClientsSalesAndCrmReportsDetailsResponse SalesAndCRMReportsDetails(SalesAndCRMReportsFilters filters)
        {
            ClientsSalesAndCrmReportsDetailsResponse Response = new ClientsSalesAndCrmReportsDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var StartDate = DateTime.Today;
                    if (filters.StartDate != null)
                    {
                        StartDate = ((DateTime)filters.StartDate).Date;
                        Response.FilteredStartDate = StartDate.ToShortDateString();
                    }
                    if (!string.IsNullOrEmpty(filters.ClientStatus))
                    {
                        if (filters.ClientStatus.ToLower() != "new" && filters.ClientStatus.ToLower() != "old")
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "ClientStatus must be New Or Old";
                            Response.Errors.Add(error);
                        }
                    }

                    var EndDate = StartDate.AddDays(-filters.BeforeDays);
                    if (filters.BeforeDays == 0)
                    {
                        if (filters.EndDate != null)
                        {

                            EndDate = ((DateTime)filters.EndDate).Date;
                            Response.FilteredEndDate = EndDate.ToShortDateString();
                        }
                        else
                        {
                            EndDate = StartDate.AddDays(-20);
                        }
                    }

                    if (filters.SalesPersonId != 0)
                    {
                        Response.FilteredSalesPersonId = filters.SalesPersonId;
                    }

                    if (filters.ReportCreator != 0)
                    {
                        Response.FilteredReportCreator = filters.ReportCreator;
                    }

                    if (filters.BranchId != 0)
                    {
                        Response.FilteredBranchId = filters.BranchId;
                    }

                    if (filters.ClientId != 0)
                    {
                        Response.FilteredClientId = filters.ClientId;
                        Response.FilteredClientName = _unitOfWork.Clients.GetById(filters.ClientId)?.Name ?? "";
                    }

                    var CrmAndSalesByDayList = new List<SalesAndCRMByDay>();

                    var CrmReportsDbQuery = _unitOfWork.Crmreports.FindAllQueryable(a => a.ReportDate.Date <= StartDate.Date && a.ReportDate >= EndDate.Date, includes: new[] { "CrmcontactType", "CrmreportReason", "CreatedByNavigation", "Client.SalesPerson", "CrmrecievedType" }).AsQueryable();
                    var SalesReportsDbQuery = _unitOfWork.VDailyReportReportLineThroughApis.FindAllQueryable(a => ((DateTime)a.ReprotDate).Date <= StartDate.Date && ((DateTime)a.ReprotDate).Date >= EndDate.Date && a.Status != "Not Filled").AsQueryable();
                    var db = SalesReportsDbQuery.ToList();
                    if (filters.BranchId != 0)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.BranchId == filters.BranchId);
                        SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.BranchId == filters.BranchId);
                    }
                    if (filters.SalesPersonId != 0)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.Client.SalesPersonId == filters.SalesPersonId);
                        SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.SalesPersonId == filters.SalesPersonId);
                    }
                    var SalesName = "";
                    var salesPerson = _unitOfWork.Users.GetById(filters.SalesPersonId);
                    if (salesPerson != null)
                    {
                        SalesName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName;
                    }
                    if (filters.ReportCreator != 0)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.CrmuserId == filters.ReportCreator);
                        SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.UserId == filters.ReportCreator);
                    }
                    if (filters.ReminderDate != DateTime.MinValue)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => ((DateTime)a.ReminderDate).Date == filters.ReminderDate.Date);
                        SalesReportsDbQuery = SalesReportsDbQuery.Where(a => ((DateTime)a.ReminderDate) == filters.ReminderDate);
                    }

                    var db2 = SalesReportsDbQuery.ToList();
                    if (!string.IsNullOrEmpty(filters.ClientStatus))
                    {
                        var totalIds = new List<long>();
                        if (filters.ClientStatus.ToLower() == "old")
                        {
                            var YearBegining = EndDate.AddMonths(-(EndDate.Month - 1));
                            totalIds = _unitOfWork.Clients.FindAll(a => totalIds.Contains(a.Id) && a.CreationDate < YearBegining && a.NeedApproval == 0).Select(a => a.Id).ToList();
                        }
                        else if (filters.ClientStatus.ToLower() == "new")
                        {
                            totalIds = _unitOfWork.Clients.FindAll(a => totalIds.Contains(a.Id) && a.CreationDate >= EndDate && a.CreationDate < StartDate && a.NeedApproval == 0).Select(a => a.Id).ToList();
                        }
                        if (totalIds != null && totalIds.Count > 0)
                        {
                            CrmReportsDbQuery = CrmReportsDbQuery.Where(a => totalIds.Contains(a.ClientId));

                            SalesReportsDbQuery = SalesReportsDbQuery.Where(a => totalIds.Contains(a.ClientId ?? 0));
                        }
                    }

                    if (filters.ClientId != 0)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.ClientId == filters.ClientId);

                        SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.ClientId == filters.ClientId);
                    }

                    if (!string.IsNullOrEmpty(filters.ThroughName))
                    {
                        if (filters.ThroughName.ToLower() == "other")
                        {
                            CrmReportsDbQuery = CrmReportsDbQuery.Where(a => !string.IsNullOrEmpty(a.OtherContactName));
                            SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.Name.ToLower().Contains("other"));
                        }
                        else
                        {
                            CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.CrmcontactType.Name.ToLower().Contains(filters.ThroughName.ToLower()));
                            SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.Name.ToLower().Contains(filters.ThroughName.ToLower()));
                        }
                    }





                    // List from db here

                    var AllCrmReportsDbList = CrmReportsDbQuery.Where(a => a.ReportDate.Date <= StartDate.Date && a.ReportDate.Date >= EndDate.Date).ToList();
                    var AllSalesReportsDbList = SalesReportsDbQuery.Where(a => ((DateTime)a.ReprotDate).Date <= StartDate.Date && ((DateTime)a.ReprotDate).Date >= EndDate.Date && a.Status != "Not Filled").ToList();
                    /*                        SalesReportsDbQuery.Where(a => DbFunctions.TruncateTime(a.CreationDate) <= StartDate.Date && DbFunctions.TruncateTime(a.CreationDate) >= EndDate.Date ).ToList();
                    */
                    var IDSClientsCRMList = AllCrmReportsDbList.Select(x => x.ClientId).ToList();
                    var IDSClientsSalesList = AllSalesReportsDbList.Select(x => x.ClientId ?? 0).ToList();
                    var ClientsIds = new List<long>();
                    ClientsIds = IDSClientsSalesList.Union(IDSClientsCRMList).Distinct().ToList();
                    var clientListdb = _unitOfWork.Clients.FindAll(a => ClientsIds.Contains(a.Id), includes: new[] { "ClientAddresses" }).ToList();



                    var RelatedToInventoryItemCRMList = AllCrmReportsDbList.Select(x => x.RelatedToInventoryItemId).ToList();
                    var RelatedToInventoryItemSalesList = AllSalesReportsDbList.Select(x => x.RelatedToInventoryItemId).ToList();
                    var RelatedToInventoryItemIds = new List<long?>();
                    RelatedToInventoryItemIds = RelatedToInventoryItemCRMList.Union(RelatedToInventoryItemSalesList).Distinct().ToList();


                    var InventoryItemsList = _unitOfWork.InventoryItems.FindAll(a => RelatedToInventoryItemIds.Contains(a.Id)).ToList();

                    var RelatedToSalesOfferCRMList = AllCrmReportsDbList.Select(x => x.RelatedToSalesOfferId).ToList();
                    var RelatedToSalesOfferSalesList = AllSalesReportsDbList.Select(x => x.RelatedToSalesOfferId).ToList();
                    var RelatedToSalesOfferIds = new List<long?>();
                    RelatedToSalesOfferIds = RelatedToSalesOfferCRMList.Union(RelatedToSalesOfferSalesList).Distinct().ToList();

                    var SalesOffersList = _unitOfWork.SalesOffers.FindAll(a => RelatedToSalesOfferIds.Contains(a.Id)).Distinct().ToList();

                    //var IDSCRMList = AllCrmReportsDbList.Select(x => x.ID).ToList();
                    //var IDSSalesList = AllSalesReportsDbList.Select(x => x.ID).ToList();
                    //var CRMAndSalesIds = new List<long>();
                    //CRMAndSalesIds = IDSCRMList.Union(IDSSalesList).Distinct().ToList();

                    //var DailyReportExpensesList = _Context.DailyReportExpenses.Where(a => a.DailyReportLineID != null ?CRMAndSalesIds.Contains((long)a.DailyReportLineID) :false).ToList();
                    var DailyReportExpensesList = _unitOfWork.DailyReportExpenses.GetAll();


                    for (DateTime day = StartDate; day >= EndDate; day = day.AddDays(-1))
                    {
                        var PreviousDay = day.AddDays(-1);
                        var NextDay = day.AddDays(1);

                        var CrmReportsDbFilter = AllCrmReportsDbList.Where(a => a.ReportDate.Date < NextDay && a.ReportDate.Date > PreviousDay).ToList();
                        var SalesReportsDbFilter = AllSalesReportsDbList.Where(a => a.ReprotDate < NextDay.Date && a.ReprotDate > PreviousDay.Date && a.Status != "Not Filled").ToList();

                        SalesAndCRMByDay CrmAndSalesByDay = new SalesAndCRMByDay();

                        //CrmReportsDbList = CrmReportsDbList.OrderByDescending(a => a.CreationDate);
                        //SalesReportsDbQuery = SalesReportsDbQuery.OrderByDescending(a => a.CreationDate);

                        var CrmReportsListDB = CrmReportsDbFilter.OrderByDescending(a => a.CreationDate).ToList();
                        var SalesReportsListDB = SalesReportsDbFilter.OrderByDescending(a => a.CreationDate).ToList();

                        if (CrmReportsListDB.Count > 0 || SalesReportsListDB.Count > 0)
                        {
                            CrmAndSalesByDay.DayDate = day.ToShortDateString();

                            if (CrmReportsListDB.Count > 0)
                            {
                                List<CrmSalesClientReport> CrmReportsListResponse = new List<CrmSalesClientReport>();
                                foreach (var report in CrmReportsListDB)
                                {
                                    string ClientCurrentStatus = null;
                                    string ClientCalssification = null;
                                    string ClientAddress = null;
                                    int? ClientClassificationId = null;
                                    if (report.ClientId != 0)
                                    {
                                        var client = clientListdb.Where(a => a.Id == report.ClientId).FirstOrDefault();
                                        if (client != null)
                                        {
                                            ClientAddress = client.ClientAddresses?.FirstOrDefault()?.Address;
                                            if (client.ClientClassification != null)
                                            {
                                                ClientCalssification = client.ClientClassification.Name;
                                                ClientClassificationId = client.ClientClassification.Id;

                                            }
                                            if (client.NeedApproval != null)
                                            {
                                                switch (client.NeedApproval)
                                                {
                                                    case 0:
                                                        ClientCurrentStatus = "Approved";
                                                        break;
                                                    case 1:
                                                        ClientCurrentStatus = "Waiting";
                                                        break;
                                                    case 2:
                                                        ClientCurrentStatus = "Rejected";
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    var reportVM = new CrmSalesClientReport
                                    {
                                        ID = report.Id,
                                        ClientID = report.ClientId,
                                        ClientName = report.Client?.Name,
                                        ClientCalssification = ClientCalssification,
                                        ClientClassificationId = ClientClassificationId,
                                        ClientStatus = filters.ClientStatus??"",
                                        ClientAddress = ClientAddress,
                                        ContactPersonMobile = report.ClientContactPerson?.Mobile,
                                        ContactPersonName = report.ClientContactPerson?.Name,
                                        IsNew = report.IsNew,
                                        Reason = report.CrmreportReason?.Name,
                                        ReportDate = report.ReportDate.ToShortDateString(),
                                        ReminderDate = report.ReminderDate != null ? report.ReminderDate?.ToShortDateString() : null,
                                        CRMUserId = report.CrmuserId,
                                        CRMUserName = report.CreatedByNavigation.FirstName + " " + report.CreatedByNavigation.LastName,
                                        Comment = report.Comment,
                                        SalesName = report.Client?.SalesPerson?.FirstName + " " + report.Client?.SalesPerson?.LastName,
                                        CreationDate = report.CreationDate.ToShortDateString()
                                    };
                                    if (report.CrmcontactTypeId != null && report.CrmcontactTypeId != 0)
                                    {
                                        reportVM.CRMContactMethod = "Contacted By";
                                        reportVM.CRMContactName = report.CrmcontactType.Name;
                                        reportVM.ContactType = "Sent";

                                        if (report.OtherContactName != "")
                                            reportVM.CRMContactName += " - " + report.OtherContactName;
                                    }
                                    else
                                    {
                                        reportVM.CRMContactMethod = "Recieved By";
                                        reportVM.CRMContactName = report.CrmrecievedType?.Name;
                                        reportVM.ContactType = "Recieved";

                                        if (report.OtherRecievedName != "")
                                            reportVM.CRMContactName += " - " + report.OtherRecievedName;
                                    }
                                    if (report.RelatedToInventoryItemId != null)
                                    {
                                        var productDb = InventoryItemsList.Where(a => a.Id == report.RelatedToInventoryItemId).FirstOrDefault();
                                        if (productDb != null)
                                        {
                                            reportVM.RelatedToInventoryItemId = report.RelatedToInventoryItemId;
                                            reportVM.RelatedToInventoryItemName = productDb.Name;
                                        }
                                    }
                                    else if (report.RelatedToSalesOfferId != null)
                                    {
                                        var offerDb = SalesOffersList.Where(a => a.Id == report.RelatedToSalesOfferId).FirstOrDefault();


                                        if (offerDb.Status.ToLower() == "closed")
                                        {
                                            var projectDb = offerDb.Projects.FirstOrDefault();
                                            if (projectDb != null)
                                            {
                                                reportVM.RelatedToProjectId = projectDb.Id;
                                                reportVM.RelatedToProjectSerial = projectDb.ProjectSerial;
                                            }
                                            reportVM.RelatedToProjectName = offerDb.ProjectName;

                                            if (report.RelatedToSalesOfferProductId != null)
                                            {
                                                var productDb = offerDb.SalesOfferProducts.Where(a => a.Id == report.RelatedToSalesOfferProductId).FirstOrDefault();
                                                if (productDb != null)
                                                {
                                                    reportVM.RelatedToProjectProductId = report.RelatedToSalesOfferProductId;
                                                    reportVM.RelatedToProjectProductName = productDb.InventoryItem?.Name;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            reportVM.RelatedToSalesOfferId = report.RelatedToSalesOfferId;
                                            reportVM.RelatedToSalesOfferSerial = offerDb.OfferSerial;
                                            reportVM.RelatedToSalesOfferName = offerDb.ProjectName;

                                            if (report.RelatedToSalesOfferProductId != null)
                                            {
                                                var productDb = offerDb.SalesOfferProducts.Where(a => a.Id == report.RelatedToSalesOfferProductId).FirstOrDefault();
                                                if (productDb != null)
                                                {
                                                    reportVM.RelatedToSalesOfferProductId = report.RelatedToSalesOfferProductId;
                                                    reportVM.RelatedToSalesOfferProductName = productDb.InventoryItem?.Name;
                                                }
                                            }
                                        }
                                    }
                                    CrmReportsListResponse.Add(reportVM);
                                }
                                CrmAndSalesByDay.CRMReportsList = CrmReportsListResponse;
                            }

                            if (SalesReportsListDB.Count > 0)
                            {
                                List<CrmSalesClientReport> SalesReportsListResponse = new List<CrmSalesClientReport>();
                                var salespersons = _unitOfWork.Users.FindAll(a => SalesReportsListDB.Select(x => x.SalesPersonId).Contains(a.Id));
                                foreach (var report in SalesReportsListDB)
                                {
                                    string ClientCurrentStatus = null;
                                    string ClientCalssification = null;
                                    int? ClientCalssificationId = null;
                                    var client = clientListdb.Where(a => a.Id == report.ClientId).FirstOrDefault();
                                    if (client != null)
                                    {
                                        if (client.ClientClassification != null)
                                        {
                                            ClientCalssification = client.ClientClassification.Name;
                                            ClientCalssificationId = client.ClientClassification.Id;
                                        }
                                        if (client.NeedApproval != null)
                                        {
                                            switch (client.NeedApproval)
                                            {
                                                case 0:
                                                    ClientCurrentStatus = "Approved";
                                                    break;
                                                case 1:
                                                    ClientCurrentStatus = "Waiting";
                                                    break;
                                                case 2:
                                                    ClientCurrentStatus = "Rejected";
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }



                                    var reportVM = new CrmSalesClientReport
                                    {
                                        ID = report.Id,
                                        ClientID = report.ClientId ?? 0,
                                        ClientName = report.ClientName,
                                        ClientStatus = filters.ClientStatus,
                                        ClientCalssification = ClientCalssification,
                                        ClientClassificationId = ClientCalssificationId,
                                        ClientAddress = client?.ClientAddresses?.Count > 0 ? client.ClientAddresses.FirstOrDefault().Address : null,
                                        ContactPersonMobile = report.ContactPersonMobile,
                                        ContactPersonName = report.ContactPerson,
                                        IsNew = report.New ?? false,
                                        NewClientName = report.NewClientName,
                                        NewClientAddress = report.NewClientAddress,
                                        NewClientTele = report.NewClientTel,
                                        Location = report.Location,
                                        FromTime = report.FromTime ?? 0,
                                        ToTime = report.ToTime ?? 0,
                                        ReportDate = report.ReprotDate?.ToShortDateString(),
                                        ThroughID = report.DailyReportThroughId,
                                        ThroughName = report.Name,
                                        SalesName = salespersons.Where(a => a.Id == report.SalesPersonId).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault(),
                                        Comment = report.Reason,
                                        Reason = report.ReasonTypeName,
                                        SalesReportCreator = report.FirstName + " " + report.LastName,
                                        Longitude = report.Longitude,
                                        Latitude = report.Latitude,
                                        ReminderDate = report.ReminderDate != null ? report.ReminderDate?.ToShortDateString() : null,
                                        IsReviewed = report.Reviewed,
                                        CreationDate = report.CreationDate.ToShortDateString()
                                    };
                                    var expensesDb = DailyReportExpensesList.Where(a => a.DailyReportLineId == report.Id).ToList();
                                    if (expensesDb != null && expensesDb.Any())
                                    {
                                        reportVM.SalesReportLineExpenses = expensesDb.Select(expense => new SalesReportLineExpense
                                        {
                                            Amount = expense.Amount ?? 0,
                                            CurrencyId = expense.CurrencyId,
                                            CurrencyName = expense.Currency.Name,
                                            DailyReportLineID = report.Id,
                                            Id = expense.Id,
                                            Type = expense.Type
                                        }).ToList();
                                    }
                                    if (report.RelatedToInventoryItemId != null)
                                    {
                                        var productDb = InventoryItemsList.Where(a => a.Id == report.RelatedToInventoryItemId).FirstOrDefault();
                                        if (productDb != null)
                                        {
                                            reportVM.RelatedToInventoryItemId = report.RelatedToInventoryItemId;
                                            reportVM.RelatedToInventoryItemName = productDb.Name;
                                        }
                                    }
                                    else if (report.RelatedToSalesOfferId != null)
                                    {
                                        var offerDb = SalesOffersList.Where(a => a.Id == report.RelatedToSalesOfferId).FirstOrDefault();


                                        if (offerDb.Status.ToLower() == "closed")
                                        {
                                            var projectDb = offerDb.Projects.FirstOrDefault();
                                            if (projectDb != null)
                                            {

                                                reportVM.RelatedToProjectId = projectDb.Id;
                                                reportVM.RelatedToProjectSerial = projectDb.ProjectSerial;
                                            }
                                            reportVM.RelatedToProjectName = offerDb.ProjectName;

                                            if (report.RelatedToSalesOfferProductId != null)
                                            {
                                                var productDb = offerDb.SalesOfferProducts.Where(a => a.Id == report.RelatedToSalesOfferProductId).FirstOrDefault();
                                                if (productDb != null)
                                                {
                                                    reportVM.RelatedToProjectProductId = report.RelatedToSalesOfferProductId;
                                                    reportVM.RelatedToProjectProductName = productDb.InventoryItem?.Name;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            reportVM.RelatedToSalesOfferId = report.RelatedToSalesOfferId;
                                            reportVM.RelatedToSalesOfferSerial = offerDb.OfferSerial;
                                            reportVM.RelatedToSalesOfferName = offerDb.ProjectName;

                                            if (report.RelatedToSalesOfferProductId != null)
                                            {
                                                var productDb = offerDb.SalesOfferProducts.Where(a => a.Id == report.RelatedToSalesOfferProductId).FirstOrDefault();
                                                if (productDb != null)
                                                {
                                                    reportVM.RelatedToSalesOfferProductId = report.RelatedToSalesOfferProductId;
                                                    reportVM.RelatedToSalesOfferProductName = productDb.InventoryItem?.Name;
                                                }
                                            }
                                        }
                                    }
                                    SalesReportsListResponse.Add(reportVM);
                                }
                                CrmAndSalesByDay.SalesReportsList = SalesReportsListResponse;
                            }

                            CrmAndSalesByDayList.Add(CrmAndSalesByDay);
                        }
                    }
                    Response.CrmAndSalesByDayReports = CrmAndSalesByDayList;
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public ClientsCrmReportsDetailsResponse CRMReportsDetails(CRMReportsDetailsFilters filters)
        {
            ClientsCrmReportsDetailsResponse Response = new ClientsCrmReportsDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (filters.SalesPersonId != 0)
                    {
                        Response.FilteredSalesPersonId = filters.SalesPersonId;
                    }
                    if (filters.ReportCreator != 0)
                    {
                        Response.FilteredReportCreator = filters.ReportCreator;
                    }
                    if (!string.IsNullOrEmpty(filters.ContactType))
                    {
                        if (filters.ContactType.ToLower() != "recieved" && filters.ContactType.ToLower() != "send")
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "ContactType must be recieved Or send";
                            Response.Errors.Add(error);
                        }
                    }

                    if (filters.BranchId != 0)
                    {
                        Response.FilteredBranchId = filters.BranchId;
                    }

                    if (filters.ClientId != 0)
                    {
                        Response.FilteredClientId = filters.ClientId;
                        Response.FilteredClientName = _unitOfWork.Clients.GetById(filters.ClientId)?.Name;
                    }

                    var CrmReportsDbQuery = _unitOfWork.Crmreports.FindAllQueryable(a => true, includes: new[] { "Client.SalesPerson", "CrmcontactType", "CrmreportReason", "CreatedByNavigation", "CrmrecievedType" });

                    if (filters.StartDate == DateTime.MinValue)
                    {
                        filters.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    }
                    if (filters.EndDate == DateTime.MinValue)
                    {
                        filters.EndDate = new DateTime(filters.StartDate.Year + 1, 1, 1);
                    }
                    CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.ReportDate.Date >= filters.StartDate.Date && a.ReportDate.Date <= filters.EndDate.Date);

                    if (filters.CRMId != 0)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.Id == filters.CRMId);
                    }

                    if (filters.BranchId != 0)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.BranchId == filters.BranchId);
                    }
                    if (filters.ReportCreator > 0)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(c => c.CrmuserId == filters.ReportCreator).AsQueryable();
                    }
                    var SalesName = "";
                    if (filters.SalesPersonId != 0)
                    {
                        var salesPerson = _unitOfWork.Users.GetById(filters.SalesPersonId);
                        if (salesPerson != null)
                        {
                            SalesName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName;
                        }
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.Client.SalesPersonId == filters.SalesPersonId).AsQueryable();
                    }
                    if (filters.ClientId != 0)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.ClientId == filters.ClientId);
                    }
                    if (!string.IsNullOrEmpty(filters.ThroughName))
                    {
                        if (filters.ThroughName.ToLower() == "other")
                        {
                            CrmReportsDbQuery = CrmReportsDbQuery.Where(a => !string.IsNullOrEmpty(a.OtherContactName));
                        }
                        else
                        {
                            CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.CrmcontactType.Name.ToLower().Contains(filters.ThroughName.ToLower()));
                        }
                    }
                    if (filters.CRMUserId != 0)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.CrmuserId == filters.CRMUserId);
                    }

                    if (!string.IsNullOrEmpty(filters.ContactType))
                    {
                        if (filters.ContactType == "send")
                        {
                            CrmReportsDbQuery = CrmReportsDbQuery.Where(a => (a.CrmcontactTypeId != null || a.CrmcontactTypeId != 0) && (a.CrmcontactTypeId == null || a.CrmcontactTypeId == 0)).AsQueryable();
                        }
                        else
                        {
                            CrmReportsDbQuery = CrmReportsDbQuery.Where(a => (a.CrmcontactTypeId == null || a.CrmcontactTypeId == 0) && (a.CrmcontactTypeId != null || a.CrmcontactTypeId != 0)).AsQueryable();
                        }
                    }
                    if (filters.ReminderDate != DateTime.MinValue)
                    {
                        CrmReportsDbQuery = CrmReportsDbQuery.Where(a => a.ReminderDate == filters.ReminderDate);
                    }

                    CrmReportsDbQuery = CrmReportsDbQuery.OrderByDescending(a => a.CreationDate);
                    var CrmReportsListDB = PagedList<Crmreport>.Create(CrmReportsDbQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);

                    List<CrmSalesClientReport> CrmReportsListResponse = new List<CrmSalesClientReport>();
                    foreach (var report in CrmReportsListDB)
                    {

                        var ClientDb = _unitOfWork.Clients.FindAll(a => a.Id == report.ClientId, includes: new[] { "ClientClassification", "ClientAddresses" }).FirstOrDefault();
                        string ClientCurrentStatus = null;
                        if (ClientDb.NeedApproval != null)
                        {
                            switch (ClientDb.NeedApproval)
                            {
                                case 0:
                                    ClientCurrentStatus = "Approved";
                                    break;
                                case 1:
                                    ClientCurrentStatus = "Waiting";
                                    break;
                                case 2:
                                    ClientCurrentStatus = "Rejected";
                                    break;
                                default:
                                    break;
                            }
                        }

                        var reportVM = new CrmSalesClientReport
                        {
                            ID = report.Id,
                            ClientID = report.ClientId,
                            ClientName = report.Client?.Name,
                            ClientCalssification = ClientDb.ClientClassification?.Name,
                            ClientClassificationId = ClientDb.ClientClassification?.Id,
                            ClientStatus = ClientCurrentStatus,
                            ClientAddress = ClientDb.ClientAddresses.Count > 0 ? ClientDb?.ClientAddresses.FirstOrDefault().Address : null,
                            ContactPersonMobile = report.ClientContactPerson?.Mobile,
                            ContactPersonName = report.ClientContactPerson?.Name,
                            IsNew = report.IsNew,
                            Reason = report.CrmreportReason?.Name,
                            ReportDate = report.ReportDate.ToShortDateString(),
                            CRMUserId = report.CrmuserId,
                            CRMUserName = report.CreatedByNavigation?.FirstName + " " + report.CreatedByNavigation?.LastName,
                            Comment = report.Comment,
                            SalesName = report.Client?.SalesPerson.FirstName,
                            LineId = report.DailyReportLineId,
                            Hint = report.Hint,
                            CreationDate = report.CreationDate.ToShortDateString(),
                            CRMReportReasonName = report.CrmreportReason?.Name,
                            CRMReportReasonID = report.CrmreportReasonId
                        };
                        if (report.CrmcontactTypeId != null && report.CrmcontactTypeId != 0)
                        {
                            reportVM.CRMContactMethod = "Contacted By";
                            reportVM.CRMContactName = report.CrmcontactType?.Name;
                            reportVM.ContactType = "Sent";

                            if (report.OtherContactName != "")
                                reportVM.CRMContactName += " - " + report.OtherContactName;
                        }
                        else
                        {
                            reportVM.CRMContactMethod = "Recieved By";
                            reportVM.CRMContactName = report.CrmrecievedType?.Name;
                            reportVM.ContactType = "Recieved";

                            if (report.OtherRecievedName != "")
                                reportVM.CRMContactName += " - " + report.OtherRecievedName;
                        }
                        if (report.RelatedToInventoryItemId != null)
                        {
                            var productDb = _unitOfWork.InventoryItems.FindAll(a => a.Id == report.RelatedToInventoryItemId).FirstOrDefault();
                            if (productDb != null)
                            {
                                reportVM.RelatedToInventoryItemId = report.RelatedToInventoryItemId;
                                reportVM.RelatedToInventoryItemName = productDb.Name;
                            }
                        }
                        else if (report.RelatedToSalesOfferId != null)
                        {
                            var offerDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == report.RelatedToSalesOfferId).FirstOrDefault();


                            if (offerDb.Status.ToLower() == "closed")
                            {
                                var projectDb = offerDb.Projects.FirstOrDefault();
                                if (projectDb != null)
                                {
                                    reportVM.RelatedToProjectId = projectDb.Id;
                                    reportVM.RelatedToProjectSerial = projectDb.ProjectSerial;
                                }
                                reportVM.RelatedToProjectName = offerDb.ProjectName;

                                if (report.RelatedToSalesOfferProductId != null)
                                {
                                    var productDb = offerDb.SalesOfferProducts.Where(a => a.Id == report.RelatedToSalesOfferProductId).FirstOrDefault();
                                    if (productDb != null)
                                    {
                                        reportVM.RelatedToProjectProductId = report.RelatedToSalesOfferProductId;
                                        reportVM.RelatedToProjectProductName = productDb.InventoryItem?.Name;
                                    }
                                }
                            }
                            else
                            {
                                reportVM.RelatedToSalesOfferId = report.RelatedToSalesOfferId;
                                reportVM.RelatedToSalesOfferSerial = offerDb.OfferSerial;
                                reportVM.RelatedToSalesOfferName = offerDb.ProjectName;

                                if (report.RelatedToSalesOfferProductId != null)
                                {
                                    var productDb = offerDb.SalesOfferProducts.Where(a => a.Id == report.RelatedToSalesOfferProductId).FirstOrDefault();
                                    if (productDb != null)
                                    {
                                        reportVM.RelatedToSalesOfferProductId = report.RelatedToSalesOfferProductId;
                                        reportVM.RelatedToSalesOfferProductName = productDb.InventoryItem?.Name;
                                    }
                                }
                            }
                        }
                        CrmReportsListResponse.Add(reportVM);
                    }
                    Response.CrmReports = CrmReportsListResponse;

                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = CrmReportsListDB.CurrentPage,
                        ItemsPerPage = CrmReportsListDB.PageSize,
                        TotalItems = CrmReportsListDB.TotalCount,
                        TotalPages = CrmReportsListDB.TotalPages
                    };
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public BaseResponseWithData<List<SelectDDL>> GetCrmContactTypesDDL()
        {
            BaseResponseWithData<List<SelectDDL>> Response = new BaseResponseWithData<List<SelectDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    Response.Data = _unitOfWork.CrmcontactTypes.GetAll().Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
        public BaseResponseWithData<List<SelectDDL>> GetCrmRecievedTypesDDL()
        {
            BaseResponseWithData<List<SelectDDL>> Response = new BaseResponseWithData<List<SelectDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    Response.Data = _unitOfWork.CrmrecievedTypes.GetAll().Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
        public BaseResponseWithData<List<SelectDDL>> GetDailyReportThroughDDL()
        {
            BaseResponseWithData<List<SelectDDL>> Response = new BaseResponseWithData<List<SelectDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    Response.Data = _unitOfWork.DailyReportThroughs.GetAll().Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<SelectDDL>> GeCrmReportReasonsDDL()
        {
            BaseResponseWithData<List<SelectDDL>> Response = new BaseResponseWithData<List<SelectDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    Response.Data = _unitOfWork.CrmreportReasons.GetAll().Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();

                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public SalesAndCRMAddNewReportScreenData GetSalesAndCRMAddNewReportScreenData()
        {
            SalesAndCRMAddNewReportScreenData Response = new SalesAndCRMAddNewReportScreenData();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    Response.ClientsDDL = _unitOfWork.Clients.GetAll().Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();

                    Response.ContactedByDDL = _unitOfWork.CrmcontactTypes.GetAll().Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();

                    Response.RecievedByDDL = _unitOfWork.CrmrecievedTypes.GetAll().Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();

                    Response.ThroughDDL = _unitOfWork.DailyReportThroughs.GetAll().Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();

                    Response.ReasonsDDL = _unitOfWork.CrmreportReasons.GetAll().Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> AddNewCRMReport(AddNewCRMReport Request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long? ReportId = null;
                    if (Request.Id != null)
                    {
                        ReportId = Request.Id;
                    }

                    string CrmUserIdString = null;
                    if (Request.CrmUserId != null)
                    {
                        CrmUserIdString = Request.CrmUserId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "CrmUserId Is Mandatory";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    long CRMUserId = long.Parse(Encrypt_Decrypt.Decrypt(CrmUserIdString, key));

                    int BranchId = 0;
                    if (Request.BranchId != 0)
                    {
                        BranchId = Request.BranchId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Branch Id Is Mandatory";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    DateTime ReportDate = DateTime.Now.Date;
                    if (string.IsNullOrEmpty(Request.ReportDate) || !DateTime.TryParse(Request.ReportDate, out ReportDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err23";
                        error.ErrorMSG = "Invalid Report Date";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    long ClientId = 0;
                    if (Request.ClientId != 0)
                    {
                        ClientId = Request.ClientId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Client Id Is Mandatory";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long ClientContactPersonId = 0;
                    if (Request.ClientContactPersonId != 0)
                    {
                        ClientContactPersonId = Request.ClientContactPersonId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Client ContactPerson Id Is Mandatory";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    int CrmReportReasonId = 0;
                    if (Request.CrmReportReasonId != null)
                    {
                        CrmReportReasonId = (int)Request.CrmReportReasonId;
                    }
                    string Comment = null;
                    if (Request.Comment != null)
                    {
                        Comment = Request.Comment;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Comment Is Mandatory";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    string CreatedByString = null;
                    if (Request.CreatedBy != null)
                    {
                        CreatedByString = Request.CreatedBy;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Created By Is Mandatory";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    long CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));


                    if (Response.Result)
                    {
                        DateTime ReminderDateTemb = DateTime.Now;
                        DateTime? ReminderDate = null;
                        if (!string.IsNullOrEmpty(Request.ReminderDate) && DateTime.TryParse(Request.ReminderDate, out ReminderDateTemb))
                        {
                            ReminderDateTemb = DateTime.Parse(Request.ReminderDate);
                            ReminderDate = ReminderDateTemb;
                        }
                        if (ReportId != null && ReportId != 0)
                        {
                            var CRMReportDb = _unitOfWork.Crmreports.FindAll(a => a.Id == ReportId).FirstOrDefault();

                            if (CRMReportDb != null)
                            {
                                //Update
                                CRMReportDb.CrmuserId = CRMUserId;
                                CRMReportDb.BranchId = BranchId;
                                CRMReportDb.ReportDate = CRMReportDb.ReportDate;
                                CRMReportDb.ClientId = ClientId;
                                CRMReportDb.ClientContactPersonId = ClientContactPersonId;
                                CRMReportDb.IsNew = Request.IsNew;
                                CRMReportDb.CrmcontactTypeId = Request.CrmContactTypeId;
                                CRMReportDb.OtherContactName = Request.OtherContactName;
                                CRMReportDb.CrmrecievedTypeId = Request.CrmRecievedTypeId;
                                CRMReportDb.OtherRecievedName = Request.OtherRecievedName;
                                if (Request.CrmReportReasonId != null)
                                {
                                    CRMReportDb.CrmreportReasonId = Request.CrmReportReasonId;
                                }
                                CRMReportDb.Comment = Comment;
                                CRMReportDb.ModifiedBy = Request.ModifiedBy;
                                CRMReportDb.ModifiedDate = DateTime.Now;
                                CRMReportDb.CustomerSatisfaction = Request.CustomerSatisfaction;
                                CRMReportDb.DailyReportId = Request.DailyReportId;
                                CRMReportDb.DailyReportLineId = Request.DailyReportLineId;
                                CRMReportDb.ReminderDate = ReminderDate;
                                CRMReportDb.RelatedToInventoryItemId = Request.RelatedToInventoryItemId;
                                CRMReportDb.RelatedToSalesOfferProductId = Request.RelatedToSalesOfferProductId;
                                CRMReportDb.RelatedToSalesOfferId = Request.RelatedToSalesOfferId;
                                CRMReportDb.Hint = Request.Hint;
                                CRMReportDb.ReminderIsClosed = Request.ReminderIsClosed;
                                _unitOfWork.Complete();
                                Response.ID = ReportId ?? 0;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Report Doesn't Exist!";
                                Response.Errors.Add(error);
                            }


                        }
                        else
                        {
                            //Add
                            var InsertedReport = new Crmreport()
                            {
                                CrmuserId = CRMUserId,
                                BranchId = BranchId,
                                ReportDate = DateTime.Now.Date,
                                ClientId = ClientId,
                                ClientContactPersonId = ClientContactPersonId,
                                IsNew = Request.IsNew,
                                CrmcontactTypeId = Request.CrmContactTypeId,
                                OtherContactName = Request.OtherContactName,
                                CrmrecievedTypeId = Request.CrmRecievedTypeId,
                                OtherRecievedName = Request.OtherRecievedName,
                                Comment = Comment,
                                CreatedBy = creator,
                                CrmreportReasonId = Request.CrmReportReasonId,
                                CreationDate = DateTime.Now,
                                CustomerSatisfaction = Request.CustomerSatisfaction,
                                DailyReportId = Request.DailyReportId,
                                DailyReportLineId = Request.DailyReportLineId,
                                ReminderDate = ReminderDate, //DateTime.TryParse(Request.ReminderDate, out ReminderDate) != null ? ReminderDate : DateTime.Now,
                                RelatedToInventoryItemId = Request.RelatedToInventoryItemId,
                                RelatedToSalesOfferProductId = Request.RelatedToSalesOfferProductId,
                                RelatedToSalesOfferId = Request.RelatedToSalesOfferId,
                                Hint = Request.Hint,
                                ReminderIsClosed = Request.ReminderIsClosed

                            };

                            _unitOfWork.Crmreports.Add(InsertedReport);
                            _unitOfWork.Complete();
                            Response.ID = InsertedReport.Id;

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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public BaseResponseWithId<long> AddNewSalesReport(AddNewSalesReport Request, long creator, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    DateTime ReportDate = DateTime.Now.Date;
                    string ModifiedByString = null;
                    long? ModifiedBy = 0;
                    long? ReportId = null;
                    if (Request.Id != null)
                    {
                        ReportId = Request.Id;

                        if (Request.ModifiedBy != null)
                        {
                            ModifiedByString = Request.ModifiedBy;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Modified By Id Is Mandatory";
                            Response.Errors.Add(error);
                        }
                        ModifiedBy = long.Parse(Encrypt_Decrypt.Decrypt(ModifiedByString, key));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(Request.ReportDate) || !DateTime.TryParse(Request.ReportDate, out ReportDate))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err23";
                            error.ErrorMSG = "Invalid Report Date";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    long UserId = 0;
                    if (!string.IsNullOrEmpty(Request.UserId))
                    {
                        UserId = long.Parse(Encrypt_Decrypt.Decrypt(Request.UserId, key));
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "User Id Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (Response.Result)
                    {
                        DateTime ReviewDateTemb = DateTime.Now;
                        DateTime? ReviewDate = null;
                        if (Request.Status != null && Request.Status == "Completed")
                        {
                            ReviewDateTemb = DateTime.Now;
                            ReviewDate = ReviewDateTemb;
                        }

                        if (ReportId != null && ReportId != 0)
                        {

                            var DailyReportDb = _unitOfWork.DailyReports.FindAll(a => a.Id == ReportId).FirstOrDefault();
                            //Update
                            if (DailyReportDb != null)
                            {


                                DailyReportDb.ModifiedBy = ModifiedBy;
                                DailyReportDb.ModifiedDate = DateTime.Now;
                                DailyReportDb.Status = Request.Status;
                                DailyReportDb.Note = Request.Note;
                                DailyReportDb.ReviewComment = Request.ReviewComment;
                                DailyReportDb.ReviewedBy = Request.ReviewedBy;
                                DailyReportDb.Reviewed = Request.IsReviewed;
                                DailyReportDb.Review = Request.Review;
                                DailyReportDb.ReviewDate = ReviewDate;

                                _unitOfWork.Complete();
                                Response.ID = ReportId ?? 0;

                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Daily Report Doesn't Exist!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            //Add
                            bool hasReport = false;
                            DateTime lastReportDate = DateTime.Now.AddDays(-1).Date;

                            var OldReport = _unitOfWork.DailyReports.FindAll(a => a.UserId == UserId).OrderByDescending(a => a.ReprotDate).FirstOrDefault();
                            if (OldReport != null)
                            {
                                hasReport = true;
                                lastReportDate = OldReport.ReprotDate.Date;
                            }

                            bool createreport = false;
                            if (hasReport)
                            {
                                if (DateTime.Compare(lastReportDate.Date, DateTime.Now.Date) != 0)
                                {
                                    createreport = true;
                                }
                            }
                            else
                            {
                                createreport = true;
                            }

                            if (createreport)
                            {
                                var InsertedReport = new DailyReport()
                                {
                                    UserId = UserId,
                                    ReprotDate = ReportDate,
                                    CreationDate = DateTime.Now,
                                    Status = Request.Status,
                                    Note = Request.Note,
                                    ReviewComment = Request.ReviewComment,
                                    ReviewedBy = Request.ReviewedBy,
                                    ReviewDate = ReviewDate,
                                    Reviewed = Request.IsReviewed,
                                    Review = Request.Review
                                };

                                _unitOfWork.DailyReports.Add(InsertedReport);
                                _unitOfWork.Complete();

                                Response.ID = InsertedReport.Id;
                                ReportId = InsertedReport.Id;

                                Response.Result = true;

                            }
                            else
                            {
                                ReportId = OldReport.Id;
                            }

                        }

                        if (Request.SalesReportExpenses != null && Request.SalesReportExpenses.Any())
                        {
                            foreach (var expense in Request.SalesReportExpenses)
                            {

                                if (expense.Id != null && expense.Id != 0)
                                {
                                    var DailyReportExpenseDb = _unitOfWork.DailyReportExpenses.FindAll(a => a.Id == expense.Id).FirstOrDefault();

                                    if (DailyReportExpenseDb != null)
                                    {
                                        //Update

                                        DailyReportExpenseDb.Amount = expense.Amount;
                                        DailyReportExpenseDb.CurrencyId = expense.CurrencyId;
                                        DailyReportExpenseDb.Type = expense.Type;

                                        _unitOfWork.Complete();
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "This Report Line Doesn't Exist!";
                                        Response.Errors.Add(error);
                                    }


                                }
                                else
                                {
                                    //Add
                                    var InsertedReportExpense = new DailyReportExpense()
                                    {
                                        Active = true,
                                        Amount = expense.Amount,
                                        CreatedBy = creator,
                                        CreationDate = DateTime.Now,
                                        CurrencyId = expense.CurrencyId,
                                        DailyReportId = ReportId,
                                        Type = expense.Type
                                    };

                                    _unitOfWork.DailyReportExpenses.Add(InsertedReportExpense);
                                    _unitOfWork.Complete();

                                }
                            }
                        }

                        if (Request.ReportLinesList.Count > 0)
                        {
                            var counter = 0;
                            foreach (var line in Request.ReportLinesList)
                            {
                                counter++;

                                int DailyReportLineThroughId = 0;
                                if (line.DailyReportThroughId != 0)
                                {
                                    DailyReportLineThroughId = line.DailyReportThroughId;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Line: " + counter + ", " + "Message: Daily Report Through Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                            }

                            if (Response.Result)
                            {
                                var ClientsIdsRequest = Request.ReportLinesList?.Select(x => x.ClientId).ToList();
                                var ClientsDBList = _unitOfWork.Clients.FindAll(x => ClientsIdsRequest.Contains(x.Id)).ToList();
                                foreach (var line in Request.ReportLinesList)
                                {

                                    long lineId = 0;
                                    DateTime ReminderDateTemb = DateTime.Now;
                                    DateTime? ReminderDate = null;
                                    if (!string.IsNullOrEmpty(line.ReminderDate) && DateTime.TryParse(line.ReminderDate, out ReminderDateTemb))
                                    {
                                        ReminderDateTemb = DateTime.Parse(line.ReminderDate);
                                        ReminderDate = ReminderDateTemb;
                                    }

                                    if (line.Id != null && line.Id != 0)
                                    {
                                        var DailyReportLineDb = _unitOfWork.DailyReportLines.FindAll(a => a.Id == line.Id).FirstOrDefault();

                                        if (DailyReportLineDb != null)
                                        {
                                            //Update

                                            DailyReportLineDb.ClientId = line.ClientId;
                                            DailyReportLineDb.DailyReportThroughId = line.DailyReportThroughId;
                                            DailyReportLineDb.FromTime = line.FromTime;
                                            DailyReportLineDb.ToTime = line.ToTime;
                                            DailyReportLineDb.Location = line.Location;
                                            DailyReportLineDb.Reason = line.Reason;
                                            DailyReportLineDb.New = line.IsNew;
                                            DailyReportLineDb.NewClientAddress = line.NewClientAddress;
                                            DailyReportLineDb.NewClientTel = line.NewClientTel;
                                            DailyReportLineDb.Reviewed = line.IsReviewed;
                                            DailyReportLineDb.ReviewedBy = line.ReviewedBy;
                                            DailyReportLineDb.ReviewDate = DateTime.Now;
                                            DailyReportLineDb.NewClientName = line.NewClientName;
                                            DailyReportLineDb.ContactPerson = line.ContactPerson;
                                            DailyReportLineDb.ReasonTypeId = line.ReasonTypeId;
                                            DailyReportLineDb.CustomerSatisfaction = line.CustomerSatisfaction;
                                            DailyReportLineDb.PickLocation = line.PickLocation;
                                            if (ReminderDate != null)
                                            {
                                                DailyReportLineDb.ReminderDate = ReminderDate;
                                            }
                                            DailyReportLineDb.RelatedToInventoryItemId = line.RelatedToInventoryItemId;
                                            DailyReportLineDb.RelatedToSalesOfferProductId = line.RelatedToSalesOfferProductId;
                                            DailyReportLineDb.Longitude = line.Longitude;
                                            DailyReportLineDb.Latitude = line.Latitude;
                                            DailyReportLineDb.RelatedToSalesOfferId = line.RelatedToSalesOfferId;
                                            DailyReportLineDb.Hint = line.Hint;
                                            DailyReportLineDb.ReminderIsClosed = line.RemindIsClosed;

                                            _unitOfWork.Complete();
                                            lineId = (long)line.Id;

                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This Report Line Doesn't Exist!";
                                            Response.Errors.Add(error);
                                        }


                                    }
                                    else
                                    {
                                        //Add
                                        var InsertedReportLine = new DailyReportLine()
                                        {
                                            DailyReportId = (long)ReportId,
                                            ClientId = line.ClientId,
                                            DailyReportThroughId = line.DailyReportThroughId,
                                            FromTime = line.FromTime,
                                            ToTime = line.ToTime,
                                            Location = line.Location,
                                            Reason = line.Reason,
                                            CreationDate = DateTime.Now,
                                            New = line.IsNew,
                                            NewClientAddress = line.NewClientAddress,
                                            NewClientTel = line.NewClientTel,
                                            Reviewed = line.IsReviewed,
                                            ReviewedBy = line.ReviewedBy,
                                            ReviewDate = DateTime.Now,
                                            NewClientName = line.NewClientName,
                                            ContactPerson = line.ContactPerson,
                                            ContactPersonMobile = line.ContactPersonMobile,
                                            ReasonTypeId = line.ReasonTypeId,
                                            CustomerSatisfaction = line.CustomerSatisfaction,
                                            PickLocation = line.PickLocation,
                                            ReminderDate = ReminderDate,
                                            RelatedToInventoryItemId = line.RelatedToInventoryItemId,
                                            RelatedToSalesOfferProductId = line.RelatedToSalesOfferProductId,
                                            RelatedToSalesOfferId = line.RelatedToSalesOfferId,
                                            Longitude = line.Longitude,
                                            Latitude = line.Latitude,
                                            Hint = line.Hint,
                                            ReminderIsClosed = line.RemindIsClosed

                                        };

                                        _unitOfWork.DailyReportLines.Add(InsertedReportLine);
                                        _unitOfWork.Complete();
                                        lineId = InsertedReportLine.Id;
                                    }


                                    if (line.ClientId > 0) // Update last report date in client in case created new report
                                    {
                                        var ClientDB = ClientsDBList.Where(x => x.Id == line.ClientId).FirstOrDefault();
                                        if (ClientDB != null)
                                        {
                                            ClientDB.LastReportDate = DateTime.Now;
                                            _unitOfWork.Complete();
                                        }
                                    }

                                    if (line.SalesReportLineExpenses != null && line.SalesReportLineExpenses.Any())
                                    {
                                        foreach (var expense in line.SalesReportLineExpenses)
                                        {

                                            if (expense.Id != null && expense.Id != 0)
                                            {
                                                var DailyReportLineExpenseDb = _unitOfWork.DailyReportExpenses.FindAll(a => a.Id == expense.Id).FirstOrDefault();

                                                if (DailyReportLineExpenseDb != null)
                                                {
                                                    //Updated

                                                    DailyReportLineExpenseDb.Amount = expense.Amount;
                                                    DailyReportLineExpenseDb.CurrencyId = expense.CurrencyId;
                                                    DailyReportLineExpenseDb.Type = expense.Type;
                                                    DailyReportLineExpenseDb.Comment = expense.Comment;

                                                    if (expense.AttachmentObj != null)
                                                    {

                                                        if (expense.AttachmentObj.Active == false) // delete without repleace
                                                        {
                                                            var AttachmentPath = Path.Combine(_host.WebRootPath, DailyReportLineExpenseDb.AttachmentPath);

                                                            if (File.Exists(AttachmentPath))
                                                            {
                                                                File.Delete(AttachmentPath);
                                                                DailyReportLineExpenseDb.AttachmentPath = null;

                                                            }
                                                        } // Update with delete old  or insert new

                                                        if (expense.AttachmentObj.File != null)
                                                        {
                                                            if (DailyReportLineExpenseDb.AttachmentPath != null)
                                                            {
                                                                var AttachmentPath = Path.Combine(_host.WebRootPath, DailyReportLineExpenseDb.AttachmentPath);

                                                                if (File.Exists(AttachmentPath))
                                                                {
                                                                    File.Delete(AttachmentPath);
                                                                    DailyReportLineExpenseDb.AttachmentPath = null;
                                                                }
                                                            } // Insert Only
                                                            var fileExtension = expense.AttachmentObj.File.FileName.Split('.').Last();
                                                            var virtualPath = $"/Attachments/" + CompanyName + "/SalesReport/ReportExpenses/" + DailyReportLineExpenseDb.Id + "/";
                                                            var FileName = System.IO.Path.GetFileNameWithoutExtension(expense.AttachmentObj.File.FileName);
                                                            var FilePath = Common.SaveFileIFF(virtualPath, expense.AttachmentObj.File, FileName, fileExtension, _host);
                                                            DailyReportLineExpenseDb.AttachmentPath = FilePath;

                                                        }

                                                    }

                                                    _unitOfWork.Complete();
                                                }
                                                else
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err25";
                                                    error.ErrorMSG = "This Report Line Expense Doesn't Exist!";
                                                    Response.Errors.Add(error);
                                                }


                                            }
                                            else
                                            {

                                                //Add
                                                var InsertedReportLineExpense = new DailyReportExpense()
                                                {
                                                    Active = true,
                                                    Amount = expense.Amount,
                                                    CreatedBy = creator,
                                                    CreationDate = DateTime.Now,
                                                    CurrencyId = expense.CurrencyId,
                                                    DailyReportLineId = lineId,
                                                    Type = expense.Type,
                                                    Comment = expense.Comment
                                                };

                                                _unitOfWork.DailyReportExpenses.Add(InsertedReportLineExpense);
                                                _unitOfWork.Complete();

                                                if (expense.AttachmentObj != null)
                                                {
                                                    if (expense.AttachmentObj.File != null)
                                                    {
                                                        var fileExtension = expense.AttachmentObj.File.FileName.Split('.').Last();
                                                        var virtualPath = $"/Attachments/" + CompanyName + "/SalesReport/ReportExpenses/" + InsertedReportLineExpense.Id + "/";
                                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(expense.AttachmentObj.File.FileName);
                                                        var FilePath = Common.SaveFileIFF(virtualPath, expense.AttachmentObj.File, FileName, fileExtension, _host);
                                                        InsertedReportLineExpense.AttachmentPath = FilePath;
                                                        _unitOfWork.Complete();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public GetSalesReportsListResponse GetSalesReportList(GetSalesReportFilters filters)
        {
            GetSalesReportsListResponse Response = new GetSalesReportsListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (filters.SalesPersonId != 0)
                    {
                        var salesPersonDb = _unitOfWork.Users.GetById(filters.SalesPersonId);
                        if (salesPersonDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This SalesPerson Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    int BranchId = 0;
                    if (filters.BranchId != 0)
                    {
                        var branchDb = _unitOfWork.Branches.GetById(BranchId);
                        if (branchDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Branch Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    var StartDate = DateTime.Now;
                    var StartDateFiltered = false;
                    var EndDate = DateTime.Now;
                    var EndDateFiltered = false;

                    if (filters.CreationFrom != null)
                    {
                        StartDate = (DateTime)filters.CreationFrom;
                        StartDateFiltered = true;
                    }

                    if (filters.CreationTo != null)
                    {
                        EndDate = (DateTime)filters.CreationTo;
                        EndDateFiltered = true;
                    }

                    string StatusListFilter = null;
                    List<string> StatusList = new List<string>();
                    if (filters.Status != null)
                    {
                        StatusListFilter = filters.Status;
                        StatusList = StatusListFilter.Split(',').ToList();
                    }


                    if (Response.Result)
                    {
                        var DailyReportsQuery = _unitOfWork.DailyReports.FindAllQueryable(a => true, includes: new[] { "DailyReportLines.Crmreports", "ReviewedByNavigation", "User" });

                        if (filters.SalesPersonId != 0)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.UserId == filters.SalesPersonId);
                        }

                        if (BranchId != 0)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.User.BranchId == BranchId);
                        }

                        if (StartDateFiltered)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.ReprotDate >= StartDate);
                        }

                        if (EndDateFiltered)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.ReprotDate <= EndDate);
                        }

                        if (filters.IsReviewed != null)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.Reviewed == filters.IsReviewed);
                        }

                        if (filters.NotApproved != null)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.DailyReportLines.Any(b => b.Client.NeedApproval != 0));
                        }

                        if (filters.HaveCRM != null)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.DailyReportLines.Any() && a.DailyReportLines.Any(c => c.Crmreports.Any()));
                        }
                        if (StatusListFilter != null && StatusList.Count() > 0)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(x => StatusList.Contains(x.Status));
                        }
                        DailyReportsQuery = DailyReportsQuery.OrderByDescending(a => a.CreationDate);
                        var DailyReportsPagedList = PagedList<DailyReport>.Create(DailyReportsQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);
                        var ReportsIds = DailyReportsPagedList.Select(b => b.Id).ToList();

                        var DailyReports = DailyReportsPagedList.Select((dailyReport, index) =>
                        {
                            var LinesCountValue = (int)dailyReport.DailyReportLines?.Count();
                            var CrmCountValue = (int)dailyReport.DailyReportLines?.Where(a => a.Crmreports != null && a.Crmreports.Any()).Select(a => a.Crmreports.Select(b => b.DailyReportLineId)).Distinct().Count();
                            var NotApprovedClients = dailyReport.DailyReportLines?.Where(a => a.DailyReportId == dailyReport.Id).Where(a => a.Client != null && a.Client.NeedApproval != 0).Select(a => a.ClientId).Distinct().Count();
                            var WaitingApprovalClients = dailyReport.DailyReportLines?.Where(a => a.DailyReportId == dailyReport.Id).Where(a => a.Client != null && a.Client.NeedApproval == 1).Select(a => a.ClientId).Distinct().Count();
                            return new SalesReport
                            {
                                Id = dailyReport.Id,
                                IsReviewed = dailyReport.Reviewed,
                                Note = dailyReport.Note,
                                ReportDate = dailyReport.ReprotDate.ToShortDateString(),
                                Review = dailyReport.Review,
                                ReviewComment = dailyReport.ReviewComment,
                                ReviewDate = dailyReport.ReviewDate?.ToShortDateString(),
                                ReviewedBy = dailyReport.ReviewedBy,
                                ReviewedByName = dailyReport.ReviewedBy != null ? dailyReport.ReviewedByNavigation.FirstName + " " + dailyReport.ReviewedByNavigation.FirstName : null,
                                Status = dailyReport.Status,
                                UserId = dailyReport.UserId,
                                UserName = dailyReport.User.FirstName + " " + dailyReport.User.LastName,
                                LinesCount = LinesCountValue,
                                CrmCount = CrmCountValue,
                                CRMReportPercent = LinesCountValue > 0 ? string.Format("{0:0.0}", (CrmCountValue / LinesCountValue) * 100) + "%" : "0%",
                                ClientsCount = (int)dailyReport.DailyReportLines?.Where(a => a.DailyReportId == dailyReport.Id).Select(a => a.ClientId).Distinct().Count(),
                                NotApprovedClientsCount = (int)NotApprovedClients,
                                WaitingApprovalClientsCount = (int)WaitingApprovalClients
                            };
                        }).ToList();

                        Response.SalesReportsList = DailyReports;


                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = DailyReportsPagedList.CurrentPage,
                            TotalPages = DailyReportsPagedList.TotalPages,
                            ItemsPerPage = DailyReportsPagedList.PageSize,
                            TotalItems = DailyReportsPagedList.TotalCount
                        };
                    }

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public GetSalesReportsListStatisticsResponse GetSalesReportListStatistics(GetSalesReportFilters filters)
        {
            GetSalesReportsListStatisticsResponse Response = new GetSalesReportsListStatisticsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (filters.SalesPersonId != 0)
                    {
                        var salesPersonDb = _unitOfWork.Users.GetById(filters.SalesPersonId);
                        if (salesPersonDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This SalesPerson Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    int BranchId = 0;
                    if (filters.BranchId != 0)
                    {
                        var branchDb = _unitOfWork.Branches.GetById(BranchId);
                        if (branchDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Branch Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    var StartDate = DateTime.Now;
                    var StartDateFiltered = false;
                    var EndDate = DateTime.Now;
                    var EndDateFiltered = false;
                    if (filters.CreationFrom != null)
                    {
                        StartDate = (DateTime)filters.CreationFrom;
                        StartDateFiltered = true;
                    }

                    if (filters.CreationTo != null)
                    {
                        EndDate = (DateTime)filters.CreationTo;
                        EndDateFiltered = true;
                    }



                    string StatusListFilter = null;
                    List<string> StatusList = new List<string>();
                    if (filters.Status != null)
                    {
                        StatusListFilter = filters.Status;
                        StatusList = StatusListFilter.Split(',').ToList();
                    }

                    if (Response.Result)
                    {
                        var DailyReportsQuery = _unitOfWork.DailyReports.FindAllQueryable(a => true, includes: new[] { "DailyReportLines.Crmreports", "ReviewedByNavigation", "User" });

                        if (filters.SalesPersonId != 0)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.UserId == filters.SalesPersonId);
                        }

                        if (BranchId != 0)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.User.BranchId == BranchId);
                        }

                        if (StartDateFiltered)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.ReprotDate >= StartDate);
                        }

                        if (EndDateFiltered)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.ReprotDate <= EndDate);
                        }

                        if (filters.IsReviewed != null)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.Reviewed == filters.IsReviewed);
                        }
                        if (filters.HaveCRM != null)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(a => a.DailyReportLines.Any() && a.DailyReportLines.Any(c => c.Crmreports.Any()));
                        }

                        if (StatusListFilter != null && StatusList.Count() > 0)
                        {
                            DailyReportsQuery = DailyReportsQuery.Where(x => StatusList.Contains(x.Status));
                        }

                        var DailyReportsList = DailyReportsQuery.ToList();

                        var ReportsCount = DailyReportsList.Count();
                        double ReviewAvg = 0;
                        if (ReportsCount > 0)
                        {
                            ReviewAvg = DailyReportsList.Sum(a => a.Review ?? 0) / ReportsCount;
                        }
                        Response.ReviewAvg = String.Format("{0:0.0}", ReviewAvg) + "%";

                        var WorkingDays = DailyReportsList.Select(a => a.ReprotDate.DayOfYear).Distinct().Count();
                        Response.WorkingDays = WorkingDays;

                        var CreatedReportCount = DailyReportsList.Where(a => a.Status != "Not Filled").Count();
                        Response.CreatedReport = CreatedReportCount;

                        var CreatedReportSalesPersonsCount = DailyReportsList.Select(a => a.UserId).Distinct().Count();
                        Response.ForSalesPersonsCount = CreatedReportSalesPersonsCount;
                    }

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public GetSalesReportLinesListResponse GetSalesReportLinesList([FromHeader] long SalesReportId)
        {
            GetSalesReportLinesListResponse Response = new GetSalesReportLinesListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (SalesReportId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "ErrCRM1";
                        error.ErrorMSG = "SalesReportId Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        var SalesReportsListDB = _unitOfWork.DailyReportLines.FindAll(a => a.DailyReportId == SalesReportId, includes: new[] { "Client.ClientClassification", "ReasonType", "DailyReportThrough", "DailyReport" }).ToList();

                        var ClientsIds = SalesReportsListDB.Select(a => a.ClientId).ToList();
                        var clientsExpirationDates = _unitOfWork.VClientUseers.FindAll(a => ClientsIds.Contains(a.Id)).ToList();

                        List<SalesReportLine> SalesReportLinesList = new List<SalesReportLine>();
                        var users = _unitOfWork.Users.FindAll(a => SalesReportsListDB.Select(x => x.ReviewedBy).Contains(a.Id));
                        foreach (var reportLine in SalesReportsListDB)
                        {
                            var ClientStatus = "";
                            if (reportLine.Client != null && reportLine.Client.NeedApproval != null)
                            {
                                switch (reportLine.Client.NeedApproval)
                                {
                                    case 0:
                                        ClientStatus = "Approved";
                                        break;
                                    case 1:
                                        ClientStatus = "Waiting";
                                        break;
                                    case 2:
                                        ClientStatus = "Rejected";
                                        break;
                                    default:
                                        break;
                                }
                            }

                            var reportVM = new SalesReportLine
                            {
                                Id = reportLine.Id,
                                ClientId = reportLine.ClientId ?? 0,
                                ClientName = reportLine.Client?.Name,
                                ClientMobile = reportLine.Client?.ClientMobiles?.FirstOrDefault()?.Mobile,
                                ClientPhone = reportLine.Client?.ClientPhones?.FirstOrDefault()?.Phone,
                                ClientEmail = reportLine.Client?.Email,
                                ClientExpirationDate = clientsExpirationDates.Where(a => a.Id == (reportLine.ClientId ?? 0)).Select(a => a.ClientExpireDate).FirstOrDefault()?.ToShortDateString(),
                                ClientStatus = ClientStatus,
                                ClientClassification = reportLine.Client?.ClientClassification?.Name,
                                ContactPersonMobile = reportLine.ContactPersonMobile,
                                ContactPerson = reportLine.ContactPerson,
                                IsNew = reportLine.New ?? false,
                                NewClientName = reportLine.NewClientName,
                                NewClientAddress = reportLine.NewClientAddress,
                                NewClientTel = reportLine.NewClientTel,
                                Location = reportLine.Location,
                                FromTime = reportLine.FromTime != null ? (float)reportLine.FromTime : 0,
                                ToTime = reportLine.ToTime != null ? (float)reportLine.ToTime : 0,
                                ReportDate = reportLine.DailyReport?.ReprotDate.ToShortDateString(),
                                DailyReportThroughId = reportLine.DailyReportThroughId,
                                DailyReportThroughName = reportLine.DailyReportThrough?.Name,
                                ReasonTypeId = reportLine.ReasonTypeId,
                                ReasonTypeName = reportLine.ReasonType?.Name,
                                Reason = reportLine.Reason,
                                CustomerSatisfaction = reportLine.CustomerSatisfaction,
                                IsReviewed = (bool)reportLine.Reviewed,
                                PickLocation = reportLine.PickLocation,
                                ReviewedBy = reportLine.ReviewedBy,
                                Comment = reportLine.Reason,
                                ReviewedByName = reportLine.ReviewedBy != null ? users.Where(a => a.Id == reportLine.ReviewedBy).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault() : null,
                                Longitude = reportLine.Longitude,
                                Latitude = reportLine.Latitude,
                                ReminderDate = reportLine.ReminderDate?.ToShortDateString(),
                                CRMLinesIds = reportLine.Crmreports != null && reportLine.Crmreports.Any() ? reportLine.Crmreports.Select(a => a.Id).ToList() : null,
                                Hint = reportLine.Hint,
                                CreationDate = reportLine.CreationDate.ToShortDateString()
                            };
                            var expensesDb = _Context.DailyReportExpenses.Where(a => a.DailyReportLineId == reportLine.Id).ToList();
                            if (expensesDb != null && expensesDb.Any())
                            {
                                reportVM.SalesReportLineExpenses = expensesDb.Select(expense => new SalesReportLineExpense
                                {
                                    Amount = expense.Amount ?? 0,
                                    CurrencyId = expense.CurrencyId,
                                    CurrencyName = expense.Currency.Name,
                                    DailyReportLineID = reportLine.Id,
                                    Id = expense.Id,
                                    Type = expense.Type,
                                    Comment = expense.Comment,
                                    FilePath = expense.AttachmentPath != null ? Globals.baseURL + expense.AttachmentPath : null,
                                }).ToList();
                            }
                            if (reportLine.RelatedToInventoryItemId != null)
                            {
                                var productDb = _Context.InventoryItems.Where(a => a.Id == reportLine.RelatedToInventoryItemId).FirstOrDefault();
                                if (productDb != null)
                                {
                                    reportVM.RelatedToInventoryItemId = reportLine.RelatedToInventoryItemId;
                                    reportVM.RelatedToInventoryItemName = productDb.Name;
                                }
                            }
                            else if (reportLine.RelatedToSalesOfferId != null)
                            {
                                var offerDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == reportLine.RelatedToSalesOfferId, includes: new[] { "SalesOfferProducts.InventoryItem" }).FirstOrDefault();

                                reportVM.RelatedToSalesOfferId = reportLine.RelatedToSalesOfferId;
                                if (offerDb.Status.ToLower() == "closed")
                                {
                                    var projectDb = offerDb.Projects.FirstOrDefault();
                                    reportVM.RelatedToProjectId = projectDb.Id;
                                    reportVM.RelatedToProjectSerial = projectDb.ProjectSerial;
                                    reportVM.RelatedToProjectName = offerDb.ProjectName;

                                    if (reportLine.RelatedToSalesOfferProductId != null)
                                    {
                                        var productDb = offerDb.SalesOfferProducts.Where(a => a.Id == reportLine.RelatedToSalesOfferProductId).FirstOrDefault();
                                        if (productDb != null)
                                        {
                                            reportVM.RelatedToProjectProductId = reportLine.RelatedToSalesOfferProductId;
                                            reportVM.RelatedToProjectProductName = productDb.InventoryItem?.Name;
                                        }
                                    }
                                }
                                else
                                {
                                    reportVM.RelatedToSalesOfferId = reportLine.RelatedToSalesOfferId;
                                    reportVM.RelatedToSalesOfferSerial = offerDb.OfferSerial;
                                    reportVM.RelatedToSalesOfferName = offerDb.ProjectName;

                                    if (reportLine.RelatedToSalesOfferProductId != null)
                                    {
                                        var productDb = offerDb.SalesOfferProducts.Where(a => a.Id == reportLine.RelatedToSalesOfferProductId).FirstOrDefault();
                                        if (productDb != null)
                                        {
                                            reportVM.RelatedToSalesOfferProductId = reportLine.RelatedToSalesOfferProductId;
                                            reportVM.RelatedToSalesOfferProductName = productDb.InventoryItem?.Name;
                                        }
                                    }
                                }
                            }
                            SalesReportLinesList.Add(reportVM);
                        }

                        Response.SalesReportLinesList = SalesReportLinesList;
                    }

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<MapSaleReportLinesResponse> GetMapSalesReportLinesList([FromHeader] long SalesReportId, [FromHeader] long SalesPersonId, [FromHeader] int BranchId, [FromHeader] DateTime? CreationFrom, [FromHeader] DateTime? CreationTo)
        {
            MapSaleReportLinesResponse Response = new MapSaleReportLinesResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (SalesReportId != 0)
                    {
                        var salesReportDb = _unitOfWork.DailyReports.GetById(SalesReportId);
                        if (salesReportDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This SalesReport Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    if (SalesPersonId != 0)
                    {
                        var salesPersonDb = _unitOfWork.Users.GetById(SalesPersonId);
                        if (salesPersonDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This SalesPerson Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    if (BranchId != 0)
                    {
                        var branchDb = _unitOfWork.Branches.GetById(BranchId);
                        if (branchDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Branch Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    var StartDate = DateTime.Now.Date;
                    var StartDateFiltered = false;
                    var EndDate = DateTime.Now.Date;
                    var EndDateFiltered = false;
                    if (CreationFrom != null)
                    {
                        StartDate = ((DateTime)CreationFrom).Date;
                        StartDateFiltered = true;
                    }

                    if (CreationTo != null)
                    {
                        EndDate = ((DateTime)CreationTo).Date;
                        EndDateFiltered = true;
                    }


                    if (Response.Result)
                    {
                        var SalesReportsListQuery = _unitOfWork.DailyReportLines.FindAllQueryable(a => true, includes: new[] { "DailyReport.User" });

                        if (SalesPersonId != 0)
                        {
                            SalesReportsListQuery = SalesReportsListQuery.Where(a => a.DailyReport.UserId == SalesPersonId);
                        }

                        if (SalesReportId != 0)
                        {
                            SalesReportsListQuery = SalesReportsListQuery.Where(a => a.DailyReportId == SalesReportId);
                        }

                        if (BranchId != 0)
                        {
                            SalesReportsListQuery = SalesReportsListQuery.Where(a => a.DailyReport.User.BranchId == BranchId);
                        }

                        if (StartDateFiltered)
                        {
                            SalesReportsListQuery = SalesReportsListQuery.Where(a => a.DailyReport.ReprotDate >= StartDate.Date);
                        }

                        if (EndDateFiltered)
                        {
                            SalesReportsListQuery = SalesReportsListQuery.Where(a => a.DailyReport.ReprotDate <= EndDate.Date);
                        }
                        var SalesReportsListDB = await SalesReportsListQuery.OrderByDescending(a => a.DailyReport.ReprotDate).ToListAsync();

                        if (SalesReportsListDB != null && SalesReportsListDB.Any())
                        {
                            List<MapSalesReportLine> MapSalesReportLinesList = SalesReportsListDB.Select(reportLine => new MapSalesReportLine
                            {
                                Latitude = reportLine.Latitude?.ToString(),
                                Longiude = reportLine.Longitude?.ToString(),
                                LineId = reportLine.Id,
                                ReportId = reportLine.DailyReportId,
                                LocationName = reportLine.Location,
                                ReportDate = reportLine.DailyReport?.ReprotDate.ToShortDateString(),
                                SalesPersonId = reportLine.DailyReport.UserId,
                                SalesPersonName = reportLine.DailyReport?.User?.FirstName + " " + reportLine.DailyReport?.User?.LastName,
                                SalesPersonImage = reportLine.DailyReport?.User.PhotoUrl != null ? Globals.baseURL + reportLine.DailyReport?.User.PhotoUrl : null

                            }).ToList();

                            Response.MapSalesReportLines = MapSalesReportLinesList;
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
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> EditSalesReportExpenses(SalesReportLineExpense Request, long creator, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request != null)
                    {
                        if (Request.Id != null && Request.Id != 0)
                        {
                            var expense = _unitOfWork.DailyReportExpenses.GetById((long)Request.Id);
                            if (expense != null)
                            {
                                if (Request.DailyReportLineID != 0) { expense.DailyReportLineId = Request.DailyReportLineID; }
                                expense.Amount = Request.Amount;
                                expense.Type = Request.Type;
                                expense.CurrencyId = Request.CurrencyId;
                                expense.Comment = Request.Comment;
                                expense.ModifiedBy = creator;
                                expense.Modified = DateTime.Now;
                                if (Request.AttachmentObj != null)
                                {
                                    if (expense.AttachmentPath != null)
                                    {
                                        var AttachmentPath = Path.Combine(_host.WebRootPath, expense.AttachmentPath);

                                        if (File.Exists(AttachmentPath))
                                        {
                                            File.Delete(AttachmentPath);
                                            expense.AttachmentPath = null;
                                        }
                                    }
                                    // Update with delete old  or insert new
                                    if (Request.AttachmentObj.File != null)
                                    {
                                        if (expense.AttachmentPath != null)
                                        {
                                            var AttachmentPath = Path.Combine(_host.WebRootPath, expense.AttachmentPath);

                                            if (File.Exists(AttachmentPath))
                                            {
                                                File.Delete(AttachmentPath);
                                                expense.AttachmentPath = null;
                                            }
                                        } // Insert Only
                                        var fileExtension = Request.AttachmentObj.File.FileName.Split('.').Last();
                                        var virtualPath = $"/Attachments/" + CompanyName + "/SalesReport/ReportExpenses/" + expense.Id + "/";
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(Request.AttachmentObj.File.FileName);
                                        var FilePath = Common.SaveFileIFF(virtualPath, Request.AttachmentObj.File, FileName, fileExtension, _host);
                                        expense.AttachmentPath = FilePath;

                                    }


                                }
                                _unitOfWork.Complete();
                                Response.ID = expense.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                Response.Errors.Add(error);
                                return Response;
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public BaseResponse DeleteSalesReportExpenses([FromHeader] long SalesReportExpenseId)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                if (Response.Result)
                {

                    if (SalesReportExpenseId > 0)
                    {
                        var SalesReportExpense = _unitOfWork.DailyReportExpenses.GetById(SalesReportExpenseId);
                        if (SalesReportExpense != null)
                        {
                            _unitOfWork.DailyReportExpenses.Delete(SalesReportExpense);
                            _unitOfWork.Complete();
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err132";
                            error.ErrorMSG = "This Sales Report Expense is not found";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err122";
                        error.ErrorMSG = "Please Provide the Sales Report Expense Id";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponse DeleteSalesReportLines([FromHeader] long SalesReportlineId)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (SalesReportlineId > 0)
                    {
                        var SalesReportline = _unitOfWork.DailyReportLines.GetById(SalesReportlineId);
                        if (SalesReportline != null)
                        {
                            _unitOfWork.DailyReportLines.Delete(SalesReportline);
                            _unitOfWork.Complete();
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err103";
                            error.ErrorMSG = "This Sales Report Line is not found";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err109";
                        error.ErrorMSG = "Please Provide the Sales Report Line Id";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<GetCRMReportReasonsResponse> GetCRMReportReasonsList()
        {
            GetCRMReportReasonsResponse response = new GetCRMReportReasonsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var CRMReportReasonsResponseList = new List<GetCRMReportReasonsResponseVM>();
                if (response.Result)
                {
                    if (response.Result)
                    {
                        var CRMReportReasonsDB = await _unitOfWork.CrmreportReasons.GetAllAsync();


                        if (CRMReportReasonsDB != null && CRMReportReasonsDB.Count() > 0)
                        {

                            foreach (var CRMReportReasonsDBOBJ in CRMReportReasonsDB)
                            {
                                var CRMReportReasonsOBJ = new GetCRMReportReasonsResponseVM();

                                CRMReportReasonsOBJ.ID = CRMReportReasonsDBOBJ.Id;

                                CRMReportReasonsOBJ.Name = CRMReportReasonsDBOBJ.Name;


                                CRMReportReasonsOBJ.Active = CRMReportReasonsDBOBJ.Active;




                                CRMReportReasonsResponseList.Add(CRMReportReasonsOBJ);
                            }



                        }

                    }

                }
                response.CRMReportReasonsList = CRMReportReasonsResponseList;
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

        public BaseResponseWithId<long> AddEditCRMReportReasons(GetCRMReportReasonsResponseVM Request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (string.IsNullOrEmpty(Request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "CRM Report Reason Name Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (Response.Result)
                    {
                        if (Request.ID != 0)
                        {
                            var CRMReportReasonsDb = _unitOfWork.CrmreportReasons.GetById(Request.ID);




                            if (CRMReportReasonsDb != null)
                            {
                                // Update
                                CRMReportReasonsDb.Name = Request.Name;
                                CRMReportReasonsDb.Active = Request.Active;
                                CRMReportReasonsDb.ModifiedDate = DateTime.Now;
                                CRMReportReasonsDb.ModifiedBy = validation.userID;
                                _unitOfWork.CrmreportReasons.Update(CRMReportReasonsDb);
                                var CRMReportReasonsUpdate = _unitOfWork.Complete();
                                if (CRMReportReasonsUpdate > 0)
                                {
                                    Response.Result = true;

                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this CRM Report Reason!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This CRM Report Reason Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }


                        else
                        {





                            // Insert
                            var reason = new CrmreportReason()
                            {
                                Name = Request.Name,
                                Active = Request.Active,
                                CreatedBy = validation.userID,
                                CreationDate = DateTime.Now,
                                ModifiedBy = validation.userID,
                                ModifiedDate = DateTime.Now
                            };
                            _unitOfWork.CrmreportReasons.Add(reason);
                            var CRMReportReasonInsert = _unitOfWork.Complete();


                            if (CRMReportReasonInsert > 0)
                            {
                                Response.ID = reason.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this CRM Report Reason!!";
                                Response.Errors.Add(error);
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

        public GetReportStatiscsGroupbyDateResponse GetCRMReportLineStatisticsPerDate(GetCRMReportLineStatisticsPerDateFilter filters)
        {
            GetReportStatiscsGroupbyDateResponse Response = new GetReportStatiscsGroupbyDateResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //int year = 0;
                //if (!string.IsNullOrEmpty(headers["Year"]) && int.TryParse(headers["Year"], out year))
                //{
                //    year = int.Parse(headers["Year"]);
                //}
                //long ReportCreator = 0;
                //if (!string.IsNullOrEmpty(headers["ReportCreator"]) && long.TryParse(headers["ReportCreator"], out ReportCreator))
                //{
                //    ReportCreator = long.Parse(headers["ReportCreator"]);
                //}
                //long SalesPersonId = 0;
                //if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
                //{
                //    SalesPersonId = long.Parse(headers["SalesPersonId"]);
                //}
                //long ClientId = 0;
                //if (!string.IsNullOrEmpty(headers["ClientId"]) && long.TryParse(headers["ClientId"], out ClientId))
                //{
                //    ClientId = long.Parse(headers["ClientId"]);
                //}
                //long BranchId = 0;
                //if (!string.IsNullOrEmpty(headers["BranchId"]) && long.TryParse(headers["BranchId"], out BranchId))
                //{
                //    BranchId = long.Parse(headers["BranchId"]);
                //}
                //string ThroughName = null;
                //if (!string.IsNullOrEmpty(headers["ThroughName"]))
                //{
                //    ThroughName = headers["ThroughName"];
                //}
                string ContactType = "";
                if (!string.IsNullOrEmpty(filters.ContactType))
                {
                    ContactType = filters.ContactType;
                    if (ContactType.ToLower() != "recieved" && ContactType.ToLower() != "send")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "ContactType must be recieved Or send";
                        Response.Errors.Add(error);
                    }
                }
                if (Response.Result)
                {
                    List<ReportStatisctsPerDate> CRMReportLinesListPerDate = new List<ReportStatisctsPerDate>();
                    var CRMReportsList = _unitOfWork.Crmreports.FindAllQueryable(a => true);
                    if (filters.Year > 0 && filters.Year != null)
                    {
                        CRMReportsList = CRMReportsList.Where(a => a.ReportDate.Year == filters.Year).AsQueryable();
                    }
                    if (filters.ReportCreator > 0 && filters.ReportCreator != null)
                    {
                        CRMReportsList = CRMReportsList.Where(a => a.CrmuserId == filters.ReportCreator).AsQueryable();
                    }
                    if (filters.SalesPersonId > 0 && filters.SalesPersonId != null)
                    {
                        CRMReportsList = CRMReportsList.Where(a => a.Client.SalesPersonId == filters.SalesPersonId).AsQueryable();
                    }
                    if (filters.ClientId > 0 && filters.ClientId != null)
                    {
                        CRMReportsList = CRMReportsList.Where(a => a.ClientId == filters.ClientId).AsQueryable();
                    }
                    if (filters.BranchId > 0 && filters.BranchId != null)
                    {
                        CRMReportsList = CRMReportsList.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.ThroughName))
                    {
                        if (filters.ThroughName.Trim().ToLower() == "other")
                        {
                            CRMReportsList = CRMReportsList.Where(a => !string.IsNullOrEmpty(a.OtherContactName)).AsQueryable();
                        }
                        else
                        {
                            if (ContactType == "send")
                            {
                                CRMReportsList = CRMReportsList.Where(a => a.CrmcontactType.Name.Trim().ToLower() == filters.ThroughName.Trim().ToLower());

                            }
                            else
                            {
                                CRMReportsList = CRMReportsList.Where(a => a.CrmrecievedType.Name.Trim().ToLower() == filters.ThroughName.Trim().ToLower());
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(ContactType))
                    {
                        if (ContactType == "send")
                        {
                            CRMReportsList = CRMReportsList.Where(a => (a.CrmcontactTypeId != null || a.CrmcontactTypeId != 0) && (a.CrmrecievedTypeId == null || a.CrmrecievedTypeId == 0)).AsQueryable();
                        }
                        else
                        {
                            CRMReportsList = CRMReportsList.Where(a => (a.CrmcontactTypeId == null || a.CrmcontactTypeId == 0) && (a.CrmrecievedTypeId != null || a.CrmrecievedTypeId != 0)).AsQueryable();
                        }
                    }


                    var CRMReportsListDB = CRMReportsList.GroupBy(x => new { x.ReportDate.Year }).ToList();
                    var CRMReportsListPerYeatDB = CRMReportsListDB.Select((item, index) =>
                    {
                        int CountOfItemsPerYear = item.Count();
                        var CRMReportsListPerMonthDB = item.GroupBy(x => new { x.ReportDate.Year, x.ReportDate.Month }).ToList();


                        var CRMReportsPerMonths = CRMReportsListPerMonthDB.Select((itemPerMonth, indexPerMonth) =>
                        {
                            int CountOfItemsPerMonth = itemPerMonth.Count();
                            var CRMReportsListPerDayDB = itemPerMonth.GroupBy(x => new { x.ReportDate.Year, x.ReportDate.Month, x.ReportDate.Day }).ToList();


                            var CRMReportsPerDay = CRMReportsListPerDayDB.Select((itemPerDay, indexPerDay) =>
                            {
                                int CountOfItemsPerDay = itemPerDay.Count();

                                return new ReportStatisctsPerDate  // Per Day
                                {
                                    Count = CountOfItemsPerDay,
                                    DatePerType = itemPerDay.Key.Day,
                                    CreationDate = new DateTime(itemPerDay.Key.Year, itemPerDay.Key.Month, itemPerDay.Key.Day).ToShortDateString()
                                };
                            }).OrderBy(a => a.DatePerType).ToList();

                            return new ReportStatisctsPerDate   // Per Month
                            {
                                ReportLinesPerDateList = CRMReportsPerDay,
                                Count = CountOfItemsPerMonth,
                                DatePerType = itemPerMonth.Key.Month
                            };
                        }).OrderBy(a => a.DatePerType).ToList();

                        return new ReportStatisctsPerDate   // Per Year
                        {

                            ReportLinesPerDateList = CRMReportsPerMonths,
                            Count = CountOfItemsPerYear,
                            DatePerType = item.Key.Year
                        };
                    }).OrderBy(a => a.DatePerType).ToList();


                    Response.Data = CRMReportsListPerYeatDB;
                }


                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public GetSalesAndCRMReportStatiscsGroupbyDateResponse GetSalesAndCRMReportLineStatisticsPerDate(GetSalesAndCRMReportLineStatisticsPerDateFilters filters)
        {
            GetSalesAndCRMReportStatiscsGroupbyDateResponse Response = new GetSalesAndCRMReportStatiscsGroupbyDateResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                string GroupBy = "Year"; //default
                //int Year = 0;
                if (filters.Year != null)
                {
                    GroupBy = "Month";
                }

                int Month = 0;
                if (filters.Month != null)
                {
                    //int.TryParse(headers["Month"], out Month);
                    if (filters.Year == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err170";
                        error.ErrorMSG = "you must be select year";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    GroupBy = "Day";
                }

                #region old headers
                //long ReportCreator = 0;
                //if (filters..)
                //{
                //    ReportCreator = long.Parse(headers["ReportCreator"]);
                //}
                //long SalesPersonId = 0;
                //if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
                //{
                //    SalesPersonId = long.Parse(headers["SalesPersonId"]);
                //}
                //long ClientId = 0;
                //if (!string.IsNullOrEmpty(headers["ClientId"]) && long.TryParse(headers["ClientId"], out ClientId))
                //{
                //    ClientId = long.Parse(headers["ClientId"]);
                //}
                //long BranchId = 0;
                //if (!string.IsNullOrEmpty(headers["BranchId"]) && long.TryParse(headers["BranchId"], out BranchId))
                //{
                //    BranchId = long.Parse(headers["BranchId"]);
                //}
                //bool? isReviewed = null;
                //if (!string.IsNullOrEmpty(headers["isReviewed"]) && bool.Parse(headers["isReviewed"]) != null)
                //{
                //    isReviewed = bool.Parse(headers["isReviewed"]);
                //}
                //string ThroughName = null;
                //if (!string.IsNullOrEmpty(headers["ThroughName"]))
                //{
                //    ThroughName = headers["ThroughName"];
                //}
                #endregion

                if (Response.Result)
                {
                    List<ReportSalesAndCRMStatisctsPerDate> SalesReportLinesListPerDate = new List<ReportSalesAndCRMStatisctsPerDate>();

                    //Expression<Func<List<IGrouping<GroupbyDate, CRMReport>>, bool>> CRMReportGroupedWhereClause;
                    Expression<Func<VDailyReportReportLineThroughApi, GroupbyDate>> DailyReportGroupClause;
                    Expression<Func<Infrastructure.Entities.Crmreport, GroupbyDate>> CRMReportGroupClause;
                    switch (GroupBy)
                    {
                        case "Month":
                            DailyReportGroupClause = a => new GroupbyDate { Year = ((DateTime)a.ReprotDate).Year, Month = ((DateTime)a.ReprotDate).Month };
                            CRMReportGroupClause = a => new GroupbyDate { Year = a.ReportDate.Year, Month = a.ReportDate.Month };
                            break;
                        case "Day":
                            DailyReportGroupClause = a => new GroupbyDate { Year = ((DateTime)a.ReprotDate).Year, Month = ((DateTime)a.ReprotDate).Month, Day = ((DateTime)a.ReprotDate).Day };
                            CRMReportGroupClause = a => new GroupbyDate { Year = a.ReportDate.Year, Month = a.ReportDate.Month, Day = a.ReportDate.Day };
                            break;
                        default: // Year
                            DailyReportGroupClause = a => new GroupbyDate { Year = ((DateTime)a.ReprotDate).Year };
                            CRMReportGroupClause = a => new GroupbyDate { Year = a.ReportDate.Year };
                            break;
                    }

                    var SalesReportsListDB = _unitOfWork.VDailyReportReportLineThroughApis.FindAllQueryable(a => true);
                    var CRMReportsListDB = _unitOfWork.Crmreports.FindAllQueryable(a => true);
                    if (filters.Year != 0 && filters.Year != null)
                    {
                        SalesReportsListDB = SalesReportsListDB.Where(x => ((DateTime)x.ReprotDate).Year == filters.Year && x.Status != "Not Filled").AsQueryable();
                        CRMReportsListDB = CRMReportsListDB.Where(x => x.ReportDate.Year == filters.Year).AsQueryable();
                    }
                    if (Month != 0)
                    {
                        SalesReportsListDB = SalesReportsListDB.Where(x => ((DateTime)x.ReprotDate).Year == filters.Year && ((DateTime)x.ReprotDate).Month == Month).AsQueryable();
                        CRMReportsListDB = CRMReportsListDB.Where(x => x.ReportDate.Year == filters.Year && x.ReportDate.Month == Month).AsQueryable();
                    }
                    if (filters.ReportCreator > 0 && filters.ReportCreator != null)
                    {
                        SalesReportsListDB = SalesReportsListDB.Where(x => x.UserId == filters.ReportCreator).AsQueryable();
                        CRMReportsListDB = CRMReportsListDB.Where(x => x.CrmuserId == filters.ReportCreator).AsQueryable();
                    }
                    if (filters.SalesPersonId > 0 && filters.SalesPersonId != null)
                    {
                        SalesReportsListDB = SalesReportsListDB.Where(x => x.SalesPersonId == filters.SalesPersonId).AsQueryable();
                        CRMReportsListDB = CRMReportsListDB.Where(x => x.Client.SalesPersonId == filters.SalesPersonId).AsQueryable();
                    }
                    if (filters.ClientId > 0 && filters.ClientId != null)
                    {
                        SalesReportsListDB = SalesReportsListDB.Where(x => x.ClientId == filters.ClientId).AsQueryable();
                        CRMReportsListDB = CRMReportsListDB.Where(x => x.ClientId == filters.ClientId).AsQueryable();
                    }
                    if (filters.BranchId > 0 && filters.BranchId != null)
                    {
                        SalesReportsListDB = SalesReportsListDB.Where(x => x.BranchId == filters.BranchId).AsQueryable();
                        CRMReportsListDB = CRMReportsListDB.Where(x => x.BranchId == filters.BranchId).AsQueryable();
                    }
                    if (filters.isReviewed != null)
                    {
                        SalesReportsListDB = SalesReportsListDB.Where(x => x.Reviewed == filters.isReviewed).AsQueryable();
                    }
                    if (filters.ThroughName != null)
                    {
                        if (filters.ThroughName.Trim().ToLower() == "other")
                        {
                            SalesReportsListDB = SalesReportsListDB.Where(a => a.Name.Trim().ToLower() == "other").AsQueryable();
                            CRMReportsListDB = CRMReportsListDB.Where(a => !string.IsNullOrEmpty(a.OtherContactName)).AsQueryable();
                        }
                        else
                        {
                            SalesReportsListDB = SalesReportsListDB.Where(a => a.Name.Trim().ToLower() == filters.ThroughName.Trim().ToLower()).AsQueryable();

                            CRMReportsListDB = CRMReportsListDB.Where(a => a.CrmcontactType.Name.Trim().ToLower() == filters.ThroughName.Trim().ToLower());
                        }
                    }

                    var SalesReportsGrouped = SalesReportsListDB.GroupBy(DailyReportGroupClause).ToList();   //the error is  here
                    var CRMReportsGrouped = CRMReportsListDB.GroupBy(CRMReportGroupClause).ToList();

                    //var CountOfCRMItemsPerYearaa = CRMReportsListDB.Where(x=>x.Key.Year == 2019).Select(x=>x.Count());
                    var ListPerYeatDB = SalesReportsGrouped.Select((item, index) =>
                    {

                        int DatePerType = 0;
                        string CreationDate = null;
                        Expression<Func<IGrouping<GroupbyDate, Infrastructure.Entities.Crmreport>, bool>> CRMReportGroupedWhereClause;
                        switch (GroupBy)
                        {
                            case "Month":
                                CRMReportGroupedWhereClause = a => a.Key.Year == item.Key.Year && a.Key.Month == item.Key.Month;
                                DatePerType = item.Key.Month;
                                break;
                            case "Day":
                                CRMReportGroupedWhereClause = a => a.Key.Year == item.Key.Year && a.Key.Month == item.Key.Month && a.Key.Day == item.Key.Day;
                                DatePerType = item.Key.Day;
                                CreationDate = new DateTime(item.Key.Year, item.Key.Month, item.Key.Day).ToShortDateString();
                                break;
                            default: // Year
                                CRMReportGroupedWhereClause = a => a.Key.Year == item.Key.Year;
                                DatePerType = item.Key.Year;
                                break;
                        }


                        int CountOfSalesItemsPerDate = item.Count();
                        int CountOfCRMItemsPerDate = CRMReportsGrouped.AsQueryable().Where(CRMReportGroupedWhereClause).Select(x => x.Count()).FirstOrDefault();
                        //var SalesReportsListPerMonthDB = item.GroupBy(x => new { x.CreationDate.Year, x.CreationDate.Month }).ToList();
                        //var CRMReportsListPerMonthDB = item.GroupBy(x => new { x.CreationDate.Year, x.CreationDate.Month }).ToList();


                        return new ReportSalesAndCRMStatisctsPerDate   // Per Year
                        {
                            CountOfCRM = CountOfCRMItemsPerDate,
                            CountOfSales = CountOfSalesItemsPerDate,
                            DatePerType = DatePerType,
                            CreationDate = CreationDate,
                        };
                    }).ToList();


                    Response.Data = ListPerYeatDB;
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
        } // not working yet 

        public BaseResponse CRMReportEditReminderStatus([FromHeader] long CrmReportId, [FromHeader] bool Status)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (CrmReportId != 0)
                    {
                        var CrmReport = _unitOfWork.Crmreports.GetById(CrmReportId);
                        if (CrmReport != null)
                        {
                            CrmReport.ReminderIsClosed = Status;
                            CrmReport.ModifiedBy = validation.userID;
                            CrmReport.ModifiedDate = DateTime.Now;
                            _unitOfWork.Crmreports.Update(CrmReport);
                            _unitOfWork.Complete();
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "CRM Report is not found";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "CRM Report Id Should be Provided";
                        Response.Errors.Add(error);
                        return Response;
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



    }
}
