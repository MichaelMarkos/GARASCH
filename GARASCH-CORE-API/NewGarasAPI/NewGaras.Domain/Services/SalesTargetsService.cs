using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Models.SalesTargets;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses;

namespace NewGaras.Domain.Services
{
    public class SalesTargetsService : ISalesTargetsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private GarasTestContext _Context;
        static readonly string key = "SalesGarasPass";

        public SalesTargetsService(IUnitOfWork unitOfWork, IWebHostEnvironment host, GarasTestContext context)
        {
            _unitOfWork = unitOfWork;
            _host = host;
            _Context = context;
           
        }

        public async Task<BaseResponseWithID> AddEditSalesTarget(AddSalesTarget Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
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
                    var TargetId = 0;
                    SalesTarget targetDb = new SalesTarget();
                    if (Request.Id != null)
                    {
                        targetDb = await _unitOfWork.SalesTargets.FindAsync(a => a.Id == Request.Id);
                        if (targetDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Target Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    if (Request.CurrencyId != 0)
                    {
                        var currencyDb = await _unitOfWork.Currencies.FindAsync(a => a.Id == Request.CurrencyId);
                        if (currencyDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Currency Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Currency Id Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    DateTime FromDate = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.FromDate) || !DateTime.TryParse(Request.FromDate, out FromDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err33";
                        error.ErrorMSG = "Invalid FromDate Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    DateTime ToDate = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.ToDate) || !DateTime.TryParse(Request.ToDate, out ToDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err33";
                        error.ErrorMSG = "Invalid ToDate Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {

                        if (Request.Id != null && Request.Id != 0)
                        {
                            if (targetDb.CanEdit)
                            {
                                if (Request.Active != null && !(bool)Request.Active)
                                {
                                    targetDb.Active = false;
                                }
                                else
                                {
                                    targetDb.ToDate = ToDate;
                                    targetDb.ModifiedDate = DateTime.Now;
                                    targetDb.CanEdit = Request.CanEdit;
                                    targetDb.CurrencyId = Request.CurrencyId;
                                    targetDb.FromDate = FromDate;
                                    targetDb.Target = Request.Target;
                                    targetDb.Year = Request.Year;
                                    targetDb.Modified = UserID;

                                    _unitOfWork.Complete();

                                    Response.ID = (int)Request.Id;
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Target Is Not Editable!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            var InsertedTarget = new SalesTarget()
                            {
                                CreationDate = DateTime.Now,
                                CreatedBy = UserID,
                                ToDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                CanEdit = Request.CanEdit,
                                CurrencyId = Request.CurrencyId,
                                FromDate = DateTime.Now,
                                Target = Request.Target,
                                Year = Request.Year,
                                Modified = UserID,
                                Active = true
                            };

                            _unitOfWork.SalesTargets.Add(InsertedTarget);
                            _unitOfWork.Complete();
                            Response.ID = InsertedTarget.Id;
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

        public async Task<BaseResponseWithID> AddEditSalesBranchTarget(AddSalesBranchTargetResponse Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
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

                    SalesTarget targetDb = new SalesTarget();
                    if (Request.TargetId != 0)
                    {
                        targetDb = await _unitOfWork.SalesTargets.FindAsync(a => a.Id == Request.TargetId);
                        if (targetDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Target Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "TargetId Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (Request.SalesBranchTargets != null && Request.SalesBranchTargets.Any())
                    {
                        foreach (var item in Request.SalesBranchTargets)
                        {
                            if (item.BranchID != 0)
                            {
                                var branchDb = await _unitOfWork.Branches.FindAsync(a => a.Id == item.BranchID);
                                if (branchDb == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Branch Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Branch Id Is Mandatory";
                                Response.Errors.Add(error);
                            }
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Branches Target List Is Empty!!";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        foreach (var item in Request.SalesBranchTargets)
                        {
                            if (item.ID != null && item.ID != 0)
                            {
                                var targetBranchDb = await _unitOfWork.SalesBranchTargets.FindAsync(a => a.Id == item.ID);
                                if (targetBranchDb != null)
                                {
                                    targetBranchDb.CurrencyId = item.CurrencyID;
                                    targetBranchDb.ModifiedBy = UserID;
                                    targetBranchDb.Percentage = (double)item.Percentage;
                                    targetBranchDb.Amount = item.Amount;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Sales Branch Target With Id (" + item.ID + ") Is Not Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                var InsertedBranchTarget = new SalesBranchTarget()
                                {
                                    Active = true,
                                    Amount = item.Amount,
                                    CurrencyId = item.CurrencyID,
                                    Percentage = (double)item.Percentage,
                                    TargetId = Request.TargetId,
                                    BranchId = item.BranchID,
                                    CreatedBy = UserID,
                                    CreationDate = DateTime.Now
                                };

                                _unitOfWork.SalesBranchTargets.Add(InsertedBranchTarget);
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithID> AddEditSalesBranchUserTarget(AddSalesBranchUserTargetResponse Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
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

                    SalesTarget targetDb = new SalesTarget();
                    if (Request.TargetId != 0)
                    {
                        targetDb = await _unitOfWork.SalesTargets.FindAsync(a => a.Id == Request.TargetId);
                        if (targetDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Target Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "TargetId Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    Branch BranchDb = new Branch();
                    if (Request.BranchId != 0)
                    {
                        BranchDb = await _unitOfWork.Branches.FindAsync(a => a.Id == Request.BranchId);
                        if (BranchDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Branch Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "BranchId Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (Request.SalesBranchUserTargets != null && Request.SalesBranchUserTargets.Any())
                    {
                        foreach (var item in Request.SalesBranchUserTargets)
                        {
                            if (item.UserId != 0)
                            {
                                var userDb = await _unitOfWork.Users.FindAsync(a => a.Id == item.UserId);
                                if (userDb == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This User Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "User Id Is Mandatory";
                                Response.Errors.Add(error);
                            }
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Branch Users Target List Is Empty!!";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        foreach (var item in Request.SalesBranchUserTargets)
                        {
                            if (item.ID != null && item.ID != 0)
                            {
                                var targetBranchUserDb = await _unitOfWork.SalesBranchUserTargets.FindAsync(a => a.Id == item.ID);
                                if (targetBranchUserDb != null)
                                {
                                    targetBranchUserDb.CurrencyId = item.CurrencyID;
                                    targetBranchUserDb.ModifiedBy = UserID;
                                    targetBranchUserDb.Percentage = (double)item.Percentage;
                                    targetBranchUserDb.Amount = item.Amount;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Sales Branch Target With Id (" + item.ID + ") Is Not Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                var InsertedBranchUserTarget = new SalesBranchUserTarget()
                                {
                                    Active = true,
                                    Amount = item.Amount,
                                    CurrencyId = item.CurrencyID,
                                    Percentage = (double)item.Percentage,
                                    TargetId = Request.TargetId,
                                    BranchId = Request.BranchId,
                                    UserId = item.UserId,
                                    CreatedBy = UserID,
                                    CreationDate = DateTime.Now
                                };

                                _unitOfWork.SalesBranchUserTargets.Add(InsertedBranchUserTarget);
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithID> AddEditSalesBranchProductTarget(AddSalesBranchProductTargetResponse Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
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

                    SalesTarget targetDb = new SalesTarget();
                    if (Request.TargetId != 0)
                    {
                        targetDb = await _unitOfWork.SalesTargets.FindAsync(a => a.Id == Request.TargetId);
                        if (targetDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Target Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "TargetId Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    Branch BranchDb = new Branch();
                    if (Request.BranchId != 0)
                    {
                        BranchDb = await _unitOfWork.Branches.FindAsync(a => a.Id == Request.BranchId);
                        if (BranchDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Branch Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "BranchId Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (Request.SalesBranchProductTargets != null && Request.SalesBranchProductTargets.Any())
                    {
                        foreach (var item in Request.SalesBranchProductTargets)
                        {
                            if (item.ProductId != 0)
                            {
                                var ProductDb = await _unitOfWork.InventoryItems.FindAsync(a => a.Id == item.ProductId);
                                if (ProductDb == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Product Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Product Id Is Mandatory";
                                Response.Errors.Add(error);
                            }
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Branch Products Target List Is Empty!!";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        foreach (var item in Request.SalesBranchProductTargets)
                        {
                            if (item.ID != null && item.ID != 0)
                            {
                                var targetBranchProductDb = await _unitOfWork.SalesBranchProductTargets.FindAsync(a => a.Id == item.ID);
                                if (targetBranchProductDb != null)
                                {
                                    targetBranchProductDb.CurrencyId = item.CurrencyID;
                                    targetBranchProductDb.ModifiedBy = UserID;
                                    targetBranchProductDb.Percentage = (double)item.Percentage;
                                    targetBranchProductDb.Amount = item.Amount;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Sales Branch Target With Id (" + item.ID + ") Is Not Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                var InsertedBranchProductTarget = new SalesBranchProductTarget()
                                {
                                    Active = true,
                                    Amount = item.Amount,
                                    CurrencyId = item.CurrencyID,
                                    Percentage = (double)item.Percentage,
                                    TargetId = Request.TargetId,
                                    BranchId = Request.BranchId,
                                    ProductId = item.ProductId,
                                    CreatedBy = UserID,
                                    CreationDate = DateTime.Now
                                };

                                _Context.SalesBranchProductTargets.Add(InsertedBranchProductTarget);
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<GetSalesTargetListResponse> GetLastSalesTargetList(int? filterYear)
        {
            GetSalesTargetListResponse Response = new GetSalesTargetListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                int Year = DateTime.Now.Year;
                if (filterYear != null || filterYear != 0)
                {
                    Year = filterYear ?? 0;
                }

                var startDate = new DateTime(Year, 1, 1);
                var endDate = new DateTime(Year + 1, 1, 1);

                if (Response.Result)
                {
                    List<TargetDetails> targetDetailsList = new List<TargetDetails>();

                    for (int i = 0; i < 5; i++)
                    {
                        var TargetThisYear = await _unitOfWork.SalesTargets.FindAsync(a => a.Year == startDate.Year);
                        var DealsDb = await _unitOfWork.Projects.FindAllAsync(a => a.SalesOffer.ClientApprovalDate >= startDate && a.SalesOffer.ClientApprovalDate < endDate && a.Active == true && a.SalesOffer.OfferType != "Sales Return", new[] { "SalesOffer" });

                        var DealsAmount = DealsDb.Sum(a => a.SalesOffer?.FinalOfferPrice) ?? 0;
                        var ReturnAmount = _unitOfWork.SalesOffers.FindAll(a => a.ClientApprovalDate >= startDate && a.ClientApprovalDate < endDate && a.OfferType == "Sales Return").Sum(a => a.FinalOfferPrice) ?? 0;
                        var AchievedTarget = DealsAmount - ReturnAmount;
                        decimal AchievedPercentage = 0;
                        if (TargetThisYear != null && TargetThisYear.Target != 0)
                        {
                            AchievedPercentage = AchievedTarget / TargetThisYear.Target * 100;
                        }

                        TargetDetails targetDetails = new TargetDetails
                        {
                            AchievedTargetAmount = AchievedTarget,
                            AchievedTargetPercentage = String.Format("{0:0.0}", AchievedPercentage) + "%",
                            TargetAmount = TargetThisYear?.Target ?? 0,
                            Year = startDate.Year
                        };

                        targetDetailsList.Add(targetDetails);

                        startDate = startDate.AddYears(-1);
                        endDate = endDate.AddYears(-1);
                    }

                    Response.SalesTargets = targetDetailsList;
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

        public async Task<GetSalesBranchTargetResponse> GetSalesBranchTargetList(int TargetId)
        {
            GetSalesBranchTargetResponse Response = new GetSalesBranchTargetResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                
                if (TargetId != 0)
                {
                    
                    var TargetDb = await _unitOfWork.SalesTargets.FindAsync(a => a.Id == TargetId);
                    if (TargetDb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This Target Doesn't Exist!!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "Invalid TargetId";
                    Response.Errors.Add(error);
                    return Response;
                }



                if (Response.Result)
                {
                    List<ViewSalesBranchTarget> BranchestargetList = new List<ViewSalesBranchTarget>();

                    var BranchesDb = await _unitOfWork.SalesBranchTargets.FindAllAsync(a => a.TargetId == TargetId, includes: new[] { "Currency", "Branch" });
                    if (BranchesDb != null && BranchesDb.Any())
                    {
                        Response.SalesBranchTargets = BranchesDb.Select(item => new ViewSalesBranchTarget
                        {
                            Amount = item.Amount,
                            BranchID = item.BranchId,
                            BranchName = item.Branch.Name,
                            CurrencyID = item.CurrencyId,
                            CurrencyName = item.Currency.Name,
                            ID = item.Id,
                            Percentage = (decimal)item.Percentage,
                            TargetID = item.TargetId
                        }).ToList();
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

        public async Task<GetSalesBranchUserTargetResponse> GetSalesBranchUserTargetList(int TargetId, int? BranchId)
        {
            GetSalesBranchUserTargetResponse Response = new GetSalesBranchUserTargetResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                
                if (TargetId != 0)
                {
                    var TargetDb = await _unitOfWork.SalesTargets.FindAsync(a => a.Id == TargetId);
                    if (TargetDb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This Target Doesn't Exist!!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "Invalid TargetId";
                    Response.Errors.Add(error);
                    return Response;
                }

                
                if (BranchId != null)
                {
                    
                    var BranchDb = await _unitOfWork.Branches.FindAsync(a => a.Id == BranchId);
                    if (BranchDb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This Branch Doesn't Exist!!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }

                if (Response.Result)
                {
                    List<ViewSalesBranchUserTarget> BranchesUserTargetList = new List<ViewSalesBranchUserTarget>();

                    var BranchUsersDb =  _unitOfWork.SalesBranchUserTargets.FindAll(a => a.TargetId == TargetId && (BranchId == 0 || a.BranchId == BranchId)).GroupBy(x => new { x.BranchId, x.Branch.Name });

                    foreach (var item in BranchUsersDb)
                    {
                        ViewSalesBranchUserTarget BranchUserTargetObj = new ViewSalesBranchUserTarget()
                        {
                            BranchID = item.Key.BranchId,
                            BranchName = item.Key.Name
                        };

                        if (item != null && item.Any())
                        {
                            BranchUserTargetObj.SalesBranchUserTargetList = item.Select(a => new SalesBranchUserTargetObj
                            {
                                Amount = a.Amount,
                                CurrencyID = a.CurrencyId,
                                CurrencyName = a.Currency.Name,
                                ID = a.Id,
                                Percentage = (decimal)a.Percentage,
                                UserId = a.UserId,
                                UserName = a.User.FirstName + " " + a.User.MiddleName + " " + a.User.LastName
                            }).ToList();
                        }

                        BranchesUserTargetList.Add(BranchUserTargetObj);
                    }

                    Response.SalesBranchUserTargets = BranchesUserTargetList;
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

        public async Task<GetSalesBranchProductTargetResponse> GetSalesBranchProductTargetList(int TargetId, int? BranchId)
        {
            GetSalesBranchProductTargetResponse Response = new GetSalesBranchProductTargetResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (TargetId != 0)
                {
                    var TargetDb = await _unitOfWork.SalesTargets.FindAsync(a => a.Id == TargetId);
                    if (TargetDb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This Target Doesn't Exist!!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "Invalid TargetId";
                    Response.Errors.Add(error);
                    return Response;
                }


                if (BranchId != null)
                {

                    var BranchDb = await _unitOfWork.Branches.FindAsync(a => a.Id == BranchId);
                    if (BranchDb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This Branch Doesn't Exist!!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }

                if (Response.Result)
                {
                    List<ViewSalesBranchProductTarget> BranchesProductTargetList = new List<ViewSalesBranchProductTarget>();

                    var BranchProductsDb =  _unitOfWork.SalesBranchProductTargets.FindAll(a => a.TargetId == TargetId && (BranchId == 0 || a.BranchId == BranchId), new[] { "Product" }).GroupBy(x => new { x.BranchId, x.Branch.Name });

                    foreach (var item in BranchProductsDb)
                    {
                        ViewSalesBranchProductTarget BranchProductTargetObj = new ViewSalesBranchProductTarget()
                        {
                            BranchID = item.Key.BranchId,
                            BranchName = item.Key.Name
                        };

                        if (item != null && item.Any())
                        {
                            BranchProductTargetObj.SalesBranchProductTargetList = item.Select(a => new SalesBranchProductTargetObj
                            {
                                Amount = a.Amount,
                                CurrencyID = a.CurrencyId,
                                CurrencyName = a.Currency.Name,
                                ID = a.Id,
                                Percentage = (decimal)a.Percentage,
                                ProductId = a.ProductId,
                                ProductName = a.Product.Name
                            }).ToList();
                        }

                        BranchesProductTargetList.Add(BranchProductTargetObj);
                    }

                    Response.SalesBranchProductTargets = BranchesProductTargetList;
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

        public async Task<TopSellingAndProfitProductsResponse> GetTopSellingAndProfitProductsList(int? TargetYear, int? StartYear)
        {
            TopSellingAndProfitProductsResponse Response = new TopSellingAndProfitProductsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                int startYear = DateTime.Now.Year;
                int endYear = DateTime.Now.Year;

                if (TargetYear != null)
                {
                    //int.TryParse(headers["TargetYear"], out startYear);
                    StartYear = TargetYear ?? 0;
                    endYear = startYear + 1;
                }
                else if (StartYear != null)
                {
                    //int.TryParse(headers["StartYear"], out endYear);
                    endYear = startYear;
                    startYear = endYear - 5;
                }

                var startDate = new DateTime(startYear, 1, 1);
                var endDate = new DateTime(endYear, 1, 1);

                if (Response.Result)
                {
                    var SellingProductsDbQuery = _unitOfWork.VSalesOfferProductSalesOffers.FindAllQueryable(a => a.Status == "Closed" && a.Active == true).AsQueryable();

                    SellingProductsDbQuery = SellingProductsDbQuery.Where(a => a.CreationDate >= startDate && a.CreationDate < endDate);
                    var SellingProductsDbData = SellingProductsDbQuery.ToList();
                    var SellingProductsDbList =  SellingProductsDbData.Where(a => a.OfferType != "Sales Return").GroupBy(a => new { a.InventoryItemId, a.Name }).ToList();
                    var ProductsIds = SellingProductsDbList.Select(a => a.Key.InventoryItemId).Distinct().ToList();
                    var ReturnSellingProductsDbList = await SellingProductsDbQuery.Where(a => a.OfferType == "Sales Return").ToListAsync();
                    var InventoryItemsDb = await _unitOfWork.InventoryItems.FindAllAsync(a => ProductsIds.Contains(a.Id));
                    var SellingProductsList = new List<TopSellingProfitProcuct>();
                    var ProfitProductsList = new List<TopSellingProfitProcuct>();
                    foreach (var SellingProduct in SellingProductsDbList)
                    {
                        if (SellingProduct != null)
                        {
                            var TotalReturnPrice = ReturnSellingProductsDbList.Where(a => a.InventoryItemId == SellingProduct.Key.InventoryItemId).Sum(a => a.ItemPrice * (decimal)a.Quantity);
                            var TotalReturnCount = ReturnSellingProductsDbList.Where(a => a.InventoryItemId == SellingProduct.Key.InventoryItemId).Count();
                            var SellingProductObj = new TopSellingProfitProcuct()
                            {
                                Name = SellingProduct.Key.Name != null ? SellingProduct.Key.Name.Trim() : "",
                                Quantity = SellingProduct.Count() - TotalReturnCount,
                                Value = SellingProduct.Sum(a => a.ItemPrice * (decimal)a.Quantity) - (TotalReturnPrice ?? 0) ?? 0
                            };
                            var ProfitProductObj = new TopSellingProfitProcuct()
                            {
                                Name = SellingProduct.Key.Name != null ? SellingProduct.Key.Name.Trim() : "",
                                Quantity = SellingProduct.Count() - TotalReturnCount,
                                Value = (SellingProduct.Sum(a => a.ItemPrice * (decimal)a.Quantity) - (TotalReturnPrice ?? 0) ?? 0) - InventoryItemsDb.Where(a => a.Id == SellingProduct.Key.InventoryItemId).Select(a => a.AverageUnitPrice).FirstOrDefault()
                            };

                            SellingProductsList.Add(SellingProductObj);
                            ProfitProductsList.Add(ProfitProductObj);
                        }
                    }
                    Response.TopSellingProducts = SellingProductsList.OrderByDescending(a => a.Value).ToList();
                    Response.TopProfitProducts = SellingProductsList.OrderByDescending(a => a.Value).ToList();
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

        public async Task<GetSalesTargetDDLResponse> GetTargetList()
        {
            GetSalesTargetDDLResponse Response = new GetSalesTargetDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var listDb = await _unitOfWork.SalesTargets.FindAllAsync(x => x.Active == true);
                    var listVM = new List<TargetDetailsDDL>();
                    foreach (var item in listDb)
                    {
                        var DLLObj = new TargetDetailsDDL();
                        DLLObj.ID = item.Id;
                        DLLObj.Name = item.Year.ToString();
                        DLLObj.TargetAmount = item.Target;

                        listVM.Add(DLLObj);
                    }
                    Response.SalesTargets = listVM;
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
