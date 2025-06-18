using AutoMapper;
using Azure;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Models.Admin;
using NewGaras.Infrastructure.Models.Admin.Responses;
using NewGaras.Infrastructure.Models.Client;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Admin;
using NewGarasAPI.Models.Admin.Responses;
using NewGarasAPI.Models.Admin.UsedInAdminResponses;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Data;
using NewGarasAPI.Models.Project.Headers;

namespace NewGaras.Domain.Services
{
    public class AdminService : IAdminService
    {
        private GarasTestContext _context;
        private readonly ITenantService _tenantService;
        private readonly IHrUserService _hrUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly ILogService _logService;
        private readonly IProjectService _projectService;
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

        public AdminService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, ITenantService tenantService, IHrUserService hrUserService, ILogService logService,IProjectService projectService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _tenantService = tenantService;
            _context = new GarasTestContext(_tenantService);
            _hrUserService = hrUserService;
            _logService = logService;
            _projectService = projectService;
        }

        public GetCurrencyResponse GetCurrencyList(string CompanyName = "")
        {
            GetCurrencyResponse response = new GetCurrencyResponse();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {
                //var CompanyName = Request.Headers["CompanyName"].ToString().ToLower();
                var CurrencyResponseList = new List<CurrencyData>();

                if (response.Result)
                {

                    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    var CurrencyDB = _unitOfWork.Currencies.GetAll();
                    if (CompanyName == "marinapltq")
                    {
                        CurrencyDB = CurrencyDB.Where(x => x.Id == 5).ToList();
                    }

                    if (CurrencyDB != null && CurrencyDB.Count() > 0)
                    {

                        foreach (var CurrencyDBOBJ in CurrencyDB)
                        {
                            var CurrencyDBOBJResponse = new CurrencyData();

                            CurrencyDBOBJResponse.ID = CurrencyDBOBJ.Id;

                            CurrencyDBOBJResponse.CurrencyName = CurrencyDBOBJ.Name;

                            CurrencyDBOBJResponse.ShortCurrencyName = CurrencyDBOBJ.ShortName;

                            CurrencyDBOBJResponse.Active = CurrencyDBOBJ.Active;

                            CurrencyDBOBJResponse.IsLocal = CurrencyDBOBJ.IsLocal;


                            CurrencyResponseList.Add(CurrencyDBOBJResponse);
                        }



                    }

                }

                response.CurrencyList = CurrencyResponseList;
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

        public async Task<BaseResponseWithID> AddNewCurrency(CurrencyData request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.CurrencyName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Currency Name Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.ShortCurrencyName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err27";
                        error.ErrorMSG = "Short Currency Name Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        //var loginuserName = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var CurrencyDb = await _unitOfWork.Currencies.FindAsync(x => x.Id == request.ID);
                            var CurrencyCheck = await _unitOfWork.Currencies.FindAsync(x => x.IsLocal == true);

                            if (request.IsLocal == true && CurrencyCheck != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "There are already a Local Currency!!";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                if (CurrencyDb != null)
                                {
                                    // Update


                                    if (CurrencyDb != null)
                                    {
                                        CurrencyDb.Name = request.CurrencyName;
                                        CurrencyDb.ShortName = request.ShortCurrencyName;
                                        CurrencyDb.Active = request.Active;
                                        CurrencyDb.IsLocal = request.IsLocal;
                                        CurrencyDb.ModifiedBy = UserID.ToString();
                                        CurrencyDb.Modified = DateTime.Now;
                                        _unitOfWork.Complete();
                                    }



                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Faild To Update this Currency!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Currency Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }

                        }
                        else
                        {

                            var CurrencyCheck = await _unitOfWork.Currencies.FindAsync(x => x.IsLocal == true);

                            if (request.IsLocal == true && CurrencyCheck != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "There are already a Local Currency!!";
                                Response.Errors.Add(error);
                            }


                            else
                            {
                                // Insert

                                var CurrencyDb = new Currency();


                                CurrencyDb.Name = request.CurrencyName;
                                CurrencyDb.ShortName = request.ShortCurrencyName;
                                CurrencyDb.Active = request.Active;
                                CurrencyDb.IsLocal = request.IsLocal;
                                CurrencyDb.ModifiedBy = UserID.ToString();
                                CurrencyDb.Modified = DateTime.Now;
                                CurrencyDb.CreatedBy = UserID.ToString();
                                CurrencyDb.CreationDate = DateTime.Now;
                                _unitOfWork.Currencies.Add(CurrencyDb);
                                var Res = _unitOfWork.Complete();



                                if (Res > 0)
                                {
                                    Response.ID = CurrencyDb.Id;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Insert this Currency!!";
                                    Response.Errors.Add(error);
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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithID> AddEditTeamIndex(TeamData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Team Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {

                            var TeamDB = await _unitOfWork.Teams.FindAsync(x => x.Id == request.ID);

                            if (TeamDB != null)
                            {
                                // Update

                                if (TeamDB != null)
                                {
                                    TeamDB.Name = request.Name;
                                    TeamDB.Description = request.Description;
                                    TeamDB.DepartmentId = request.DepartmentID;
                                    TeamDB.Active = request.Active;
                                    TeamDB.ModifiedBy = UserId;
                                    TeamDB.ModifiedDate = DateTime.Now;
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Team!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Team Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            var Res = 0;

                            var TeamDB = new Team();

                            var DepartmentIDCheck = await _unitOfWork.Departments.FindAsync(x => x.Id == request.DepartmentID);
                            if (DepartmentIDCheck != null)
                            {

                                TeamDB.Name = request.Name;
                                TeamDB.Description = request.Description;
                                TeamDB.DepartmentId = request.DepartmentID;
                                TeamDB.Active = request.Active;
                                TeamDB.CreatedBy = UserId;
                                TeamDB.CreatedDate = DateTime.Now;
                                TeamDB.ModifiedBy = UserId;
                                TeamDB.ModifiedDate = DateTime.Now;
                                _unitOfWork.Teams.Add(TeamDB);
                                Res = _unitOfWork.Complete();
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Department ID  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }




                            if (Res > 0)
                            {

                                Response.ID = TeamDB.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Team!!";
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

        public async Task<GetExpensisTypeResponse> GetExpensisType()
        {
            GetExpensisTypeResponse response = new GetExpensisTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetExpensisTypeResponseList = new List<ExpensisTypeData>();
                if (response.Result)
                {

                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetExpensisTypeDB = await _unitOfWork.ExpensisTypes.FindAllAsync(a => a.Active == true, new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                        if (GetExpensisTypeDB != null && GetExpensisTypeDB.Count() > 0)
                        {

                            foreach (var GetExpensisTypeOBJ in GetExpensisTypeDB)
                            {
                                var ExpensisTypeDBResponse = new ExpensisTypeData();

                                ExpensisTypeDBResponse.ID = (int)GetExpensisTypeOBJ.Id;

                                ExpensisTypeDBResponse.ExpensisTypeName = GetExpensisTypeOBJ.ExpensisTypeName;

                                ExpensisTypeDBResponse.Description = GetExpensisTypeOBJ.Description;

                                ExpensisTypeDBResponse.Active = GetExpensisTypeOBJ.Active;

                                ExpensisTypeDBResponse.CreatedBy = GetExpensisTypeOBJ.CreatedByNavigation.FirstName + " " + GetExpensisTypeOBJ?.CreatedByNavigation?.MiddleName + " " + GetExpensisTypeOBJ?.CreatedByNavigation.LastName;
                                ExpensisTypeDBResponse.ModifiedBy = GetExpensisTypeOBJ.ModifiedByNavigation.FirstName + " " + GetExpensisTypeOBJ?.ModifiedByNavigation?.MiddleName + " " + GetExpensisTypeOBJ?.ModifiedByNavigation.LastName;
                                GetExpensisTypeResponseList.Add(ExpensisTypeDBResponse);
                            }



                        }

                    }

                }
                response.ExpensisTypeList = GetExpensisTypeResponseList;
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

        public async Task<BaseResponseWithID> AddEditExpensisType(ExpensisTypeData request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;


            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.ExpensisTypeName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Expensis Type Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var ExpensisTypeDB = await _unitOfWork.ExpensisTypes.FindAsync(x => x.Id == request.ID);
                            if (ExpensisTypeDB != null)
                            {
                                // Update





                                if (ExpensisTypeDB != null)
                                {
                                    ExpensisTypeDB.ExpensisTypeName = request.ExpensisTypeName;
                                    ExpensisTypeDB.Description = request.Description;
                                    ExpensisTypeDB.Active = request.Active;
                                    ExpensisTypeDB.ModifiedBy = UserID;
                                    ExpensisTypeDB.ModifiedDate = DateTime.Now;
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Expensis Type!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Expensis Type Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert

                            var ExpensisTypeDB = new ExpensisType();


                            ExpensisTypeDB.ExpensisTypeName = request.ExpensisTypeName;
                            ExpensisTypeDB.Description = request.Description;
                            ExpensisTypeDB.CreatedBy = UserID;
                            ExpensisTypeDB.CreationDate = DateTime.Now;
                            ExpensisTypeDB.ModifiedBy = UserID;
                            ExpensisTypeDB.ModifiedDate = DateTime.Now;
                            _unitOfWork.ExpensisTypes.Add(ExpensisTypeDB);
                            var Res = _unitOfWork.Complete();

                            if (Res > 0)
                            {

                                Response.ID = ExpensisTypeDB.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Expensis Type!!";
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

        public async Task<GetIncomeTypeResponse> GetIncomeType()
        {
            GetIncomeTypeResponse response = new GetIncomeTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                var GetIncomeTypeResponseList = new List<IncomeTypeData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetIncomeTypeDB = await _unitOfWork.IncomeTypes.FindAllAsync(a => a.Active == true, new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                        if (GetIncomeTypeDB != null && GetIncomeTypeDB.Count() > 0)
                        {

                            foreach (var GetIncomeTypeOBJ in GetIncomeTypeDB)
                            {
                                var IncomeTypeDBResponse = new IncomeTypeData();

                                IncomeTypeDBResponse.ID = (int)GetIncomeTypeOBJ.Id;

                                IncomeTypeDBResponse.IncomeTypeName = GetIncomeTypeOBJ.IncomeTypeName;

                                IncomeTypeDBResponse.Description = GetIncomeTypeOBJ.Description;

                                IncomeTypeDBResponse.Active = GetIncomeTypeOBJ.Active;

                                IncomeTypeDBResponse.CreatedBy = GetIncomeTypeOBJ.CreatedByNavigation.FirstName + " " + GetIncomeTypeOBJ?.CreatedByNavigation?.MiddleName + " " + GetIncomeTypeOBJ?.CreatedByNavigation?.LastName;
                                IncomeTypeDBResponse.ModifiedBy = GetIncomeTypeOBJ.ModifiedByNavigation.FirstName + " " + GetIncomeTypeOBJ?.ModifiedByNavigation?.MiddleName + " " + GetIncomeTypeOBJ?.ModifiedByNavigation?.LastName;

                                GetIncomeTypeResponseList.Add(IncomeTypeDBResponse);
                            }



                        }

                    }

                }
                response.IncomeTypeList = GetIncomeTypeResponseList;
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

        public async Task<BaseResponseWithID> AddEditIncomeType(IncomeTypeData request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.IncomeTypeName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Income Type Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var IncomeTypeDB = await _unitOfWork.IncomeTypes.FindAsync(x => x.Id == request.ID);
                            if (IncomeTypeDB != null)
                            {
                                // Update

                                if (IncomeTypeDB != null)
                                {
                                    IncomeTypeDB.IncomeTypeName = request.IncomeTypeName;
                                    IncomeTypeDB.Description = request.Description;
                                    IncomeTypeDB.Active = request.Active;
                                    IncomeTypeDB.ModifiedBy = UserID;
                                    IncomeTypeDB.ModifiedDate = DateTime.Now;
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Income Type!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Income Type Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            var IncomeTypeDB = new IncomeType();


                            IncomeTypeDB.IncomeTypeName = request.IncomeTypeName;
                            IncomeTypeDB.Description = request.Description;
                            IncomeTypeDB.Active = request.Active;
                            IncomeTypeDB.CreatedBy = UserID;
                            IncomeTypeDB.CreationDate = DateTime.Now;
                            _unitOfWork.IncomeTypes.Add(IncomeTypeDB);
                            var Res = _unitOfWork.Complete();


                            if (Res > 0)
                            {
                                Response.ID = IncomeTypeDB.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Income Type!!";
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

        public async Task<GetShippingMethodResponse> GetShippingMethod()
        {
            GetShippingMethodResponse response = new GetShippingMethodResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetShippingMethodResponseList = new List<ShippingMethodData>();
                if (response.Result)
                {
                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var ShippingMethodDB = await _unitOfWork.ShippingMethods.GetAllAsync();

                        if (ShippingMethodDB != null && ShippingMethodDB.Count() > 0)
                        {

                            foreach (var ShippingMethodDBOBJ in ShippingMethodDB)
                            {
                                var ShippingMethodResponse = new ShippingMethodData();

                                ShippingMethodResponse.ID = (int)ShippingMethodDBOBJ.Id;

                                ShippingMethodResponse.Name = ShippingMethodDBOBJ.Name;

                                ShippingMethodResponse.Active = ShippingMethodDBOBJ.Active;


                                GetShippingMethodResponseList.Add(ShippingMethodResponse);
                            }
                        }

                    }

                }
                response.ShippingMethodList = GetShippingMethodResponseList;
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

        public async Task<BaseResponseWithID> AddEditShippingMethod(ShippingMethodData request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Cost Type Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var ShippingMehodDB = await _unitOfWork.ShippingMethods.FindAsync(x => x.Id == request.ID);
                            if (ShippingMehodDB != null)
                            {
                                // Update




                                if (ShippingMehodDB != null)
                                {
                                    ShippingMehodDB.Name = request.Name;
                                    ShippingMehodDB.Active = request.Active;
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Shipping Method!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Shipping Method Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert


                            var ShippingMethodDB = new ShippingMethod();


                            ShippingMethodDB.Name = request.Name;
                            ShippingMethodDB.Name = request.Name;
                            ShippingMethodDB.Active = request.Active;
                            _unitOfWork.ShippingMethods.Add(ShippingMethodDB);
                            var Res = _unitOfWork.Complete();

                            if (Res > 0)
                            {
                                Response.ID = ShippingMethodDB.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Shipping Method !!";
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


        public async Task<GetCRMContactTypeResponse> GetCRMContactType()
        {
            GetCRMContactTypeResponse response = new GetCRMContactTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {
                var GetCRMContactTypeResponseList = new List<CRMContactTypeData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetCRMContactTypeDB = await _unitOfWork.CrmContactTypes.FindAllAsync(a => a.Active == true, new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                        if (GetCRMContactTypeDB != null)
                        {

                            foreach (var GetCRMContactTypeDBOBJ in GetCRMContactTypeDB)
                            {
                                var GetCRMContactTypeResponse = new CRMContactTypeData();

                                GetCRMContactTypeResponse.ID = (int)GetCRMContactTypeDBOBJ.Id;

                                GetCRMContactTypeResponse.Name = GetCRMContactTypeDBOBJ.Name;

                                GetCRMContactTypeResponse.Active = GetCRMContactTypeDBOBJ.Active;

                                GetCRMContactTypeResponse.CreatedByID = GetCRMContactTypeDBOBJ.CreatedByNavigation.Id;

                                GetCRMContactTypeResponse.CreatedBy = GetCRMContactTypeDBOBJ.CreatedByNavigation.FirstName + " " + GetCRMContactTypeDBOBJ.CreatedByNavigation?.MiddleName + " " + GetCRMContactTypeDBOBJ.CreatedByNavigation?.LastName;

                                GetCRMContactTypeResponse.ModifiedByID = GetCRMContactTypeDBOBJ?.ModifiedByNavigation?.Id;

                                if (GetCRMContactTypeDBOBJ.ModifiedBy != null) GetCRMContactTypeResponse.ModifiedBy = GetCRMContactTypeDBOBJ?.ModifiedByNavigation?.FirstName + " " + GetCRMContactTypeDBOBJ.ModifiedByNavigation?.MiddleName + " " + GetCRMContactTypeDBOBJ.ModifiedByNavigation?.LastName;

                                GetCRMContactTypeResponseList.Add(GetCRMContactTypeResponse);
                            }



                        }

                    }

                }
                response.CRMContactTypeList = GetCRMContactTypeResponseList;
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

        public BaseResponseWithID AddEditCRMContactType(CRMContactTypeData request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            /*                            var CRMContactTypeDB = _Context.proc_CRMContactTypeLoadByPrimaryKey(Request.ID).FirstOrDefault();
                            */
                            var CRMContactTypeDB = _unitOfWork.CrmContactTypes.Find(a => a.Id == request.ID);

                            if (CRMContactTypeDB != null)
                            {
                                // Update

                                CRMContactTypeDB.Name = request.Name;
                                CRMContactTypeDB.Active = request.Active;
                                CRMContactTypeDB.ModifiedDate = DateTime.Now;
                                CRMContactTypeDB.ModifiedBy = UserID;
                                /*var CRMContactTypeUpdate = _Context.proc_CRMContactTypeUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                   Request.Active,
                                                                                                   CRMContactTypeDB.CreationDate,
                                                                                                   CRMContactTypeDB.CreatedBy,
                                                                                                   //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                                                                                                   DateTime.Now,
                                                                                                     validation.userID
                                                                                                    );*/
                                var CRMContactTypeUpdate = _unitOfWork.Complete();
                                if (CRMContactTypeUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this CRM Contact Type!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This CRM Contact Type Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            var crmInsert = new CrmcontactType()
                            {
                                Name = request.Name,
                                Active = request.Active,
                                CreationDate = DateTime.Now,
                                CreatedBy = UserID,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = UserID,
                            };

                            /*ObjectParameter CRMContactTypeID = new ObjectParameter("ID", typeof(int));
                            var CRMContactTypeInsert = _Context.proc_CRMContactTypeInsert(CRMContactTypeID,
                                                                                            Request.Name,
                                                                                            Request.Active,
                                                                                            DateTime.Now,
                                                                                            validation.userID,
                                                                                            null,
                                                                                            null
                                                                                           );*/

                            _unitOfWork.CrmContactTypes.Add(crmInsert);
                            var CRMContactTypeInsert = _unitOfWork.Complete();

                            if (CRMContactTypeInsert > 0)
                            {
                                var CRMContactTypeInsertID = long.Parse(crmInsert.Id.ToString());
                                Response.ID = CRMContactTypeInsertID;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this CRM Contact Type!!";
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

        public async Task<GetCRMRecievedTypeResponse> GetCRMRecievedType()
        {
            GetCRMRecievedTypeResponse response = new GetCRMRecievedTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetCRMRecievedTypeList = new List<CRMRecievedTypeData>();
                if (response.Result)
                {

                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetCRMRecievedTypeDB = await _unitOfWork.CrmRecievedTypes.FindAllAsync((x => (true)), new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                        if (GetCRMRecievedTypeDB != null && GetCRMRecievedTypeDB.Count() > 0)
                        {

                            foreach (var GetCRMRecievedTypeOBJ in GetCRMRecievedTypeDB)
                            {
                                var GetCRMRecievedTypeResponse = new CRMRecievedTypeData();

                                GetCRMRecievedTypeResponse.ID = (int)GetCRMRecievedTypeOBJ.Id;

                                GetCRMRecievedTypeResponse.Name = GetCRMRecievedTypeOBJ.Name;

                                GetCRMRecievedTypeResponse.Active = GetCRMRecievedTypeOBJ.Active;

                                GetCRMRecievedTypeResponse.CreatedById = GetCRMRecievedTypeOBJ.CreatedByNavigation.Id;

                                GetCRMRecievedTypeResponse.CreatedBy = GetCRMRecievedTypeOBJ.CreatedByNavigation.FirstName + " " + GetCRMRecievedTypeOBJ.CreatedByNavigation?.MiddleName + " " + GetCRMRecievedTypeOBJ.CreatedByNavigation?.LastName;

                                if (GetCRMRecievedTypeOBJ.ModifiedByNavigation != null) GetCRMRecievedTypeResponse.ModifiedBy = GetCRMRecievedTypeOBJ.ModifiedByNavigation.FirstName + " " + GetCRMRecievedTypeOBJ.ModifiedByNavigation?.MiddleName + " " + GetCRMRecievedTypeOBJ.ModifiedByNavigation?.LastName;


                                GetCRMRecievedTypeList.Add(GetCRMRecievedTypeResponse);
                            }



                        }

                    }

                }
                response.CRMRecievedTypeList = GetCRMRecievedTypeList;
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

        public BaseResponseWithID AddEditCRMRecievedType(CRMRecievedTypeData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {


                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var CRMRecievedTypeDB = _unitOfWork.CrmRecievedTypes.GetById(request.ID ?? 0);
                            if (CRMRecievedTypeDB != null)
                            {
                                // Update
                                /*var CRMRecievedTypeUpdate = _Context.proc_CRMRecievedTypeUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                   Request.Active,
                                                                                                   CRMRecievedTypeDB.CreationDate,
                                                                                                   CRMRecievedTypeDB.CreatedBy,
                                                                                                   //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                                                                                                   DateTime.Now,
                                                                                                     validation.userID
                                                                                                    );*/
                                CRMRecievedTypeDB.Name = request.Name;
                                CRMRecievedTypeDB.Active = request.Active;
                                CRMRecievedTypeDB.ModifiedDate = DateTime.Now;
                                CRMRecievedTypeDB.ModifiedBy = UserId;

                                var CRMRecievedTypeUpdate = _unitOfWork.Complete();

                                if (CRMRecievedTypeUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this CRM Reecieved Type!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This CRM Reecieved Type Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter CRMReecievedTypeID = new ObjectParameter("ID", typeof(int));
                            var CRMReecievedTypeInsert = _Context.proc_CRMRecievedTypeInsert(CRMReecievedTypeID,
                                                                                            Request.Name,
                                                                                            Request.Active,
                                                                                            DateTime.Now,
                                                                                            validation.userID,
                                                                                            null,
                                                                                            null
                                                                                           );*/
                            var CrmRecieved = new CrmrecievedType()
                            {
                                Name = request.Name,
                                Active = request.Active,
                                CreationDate = DateTime.Now,
                                CreatedBy = UserId,
                                ModifiedBy = UserId,
                                ModifiedDate = DateTime.Now,
                            };

                            _unitOfWork.CrmRecievedTypes.Add(CrmRecieved);

                            var CRMReecievedTypeInsert = _unitOfWork.Complete();

                            if (CRMReecievedTypeInsert > 0)
                            {
                                var CRMReecievedTypeInsertID = long.Parse(CrmRecieved.Id.ToString());
                                Response.ID = CRMReecievedTypeInsertID;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this CRM Recieved Type!!";
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

        public async Task<GetDailyReportThroughResponse> GetDailyReportThrough()
        {
            GetDailyReportThroughResponse response = new GetDailyReportThroughResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetDailyReportThroughDataResponseList = new List<DailyReportThroughData>();
                if (response.Result)
                {

                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetDailyReportThroughDB = await _unitOfWork.DailyReportThroughs.FindAllAsync(a => (true), new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                        if (GetDailyReportThroughDB != null && GetDailyReportThroughDB.Count() > 0)
                        {

                            foreach (var GetDailyReportThroughOBJ in GetDailyReportThroughDB)
                            {
                                var DailyReportThroughDataResponse = new DailyReportThroughData();

                                DailyReportThroughDataResponse.ID = (int)GetDailyReportThroughOBJ.Id;

                                DailyReportThroughDataResponse.Name = GetDailyReportThroughOBJ.Name;

                                DailyReportThroughDataResponse.Description = GetDailyReportThroughOBJ.Description;

                                DailyReportThroughDataResponse.Active = GetDailyReportThroughOBJ.Active;

                                DailyReportThroughDataResponse.CreatedById = GetDailyReportThroughOBJ.CreatedBy;

                                DailyReportThroughDataResponse.CreatedBy = GetDailyReportThroughOBJ.CreatedByNavigation.FirstName + " " + GetDailyReportThroughOBJ.CreatedByNavigation?.MiddleName + " " + GetDailyReportThroughOBJ.CreatedByNavigation?.LastName;

                                if (GetDailyReportThroughOBJ.ModifiedBy != null) DailyReportThroughDataResponse.ModifiedBy = GetDailyReportThroughOBJ.ModifiedByNavigation.FirstName + " " + GetDailyReportThroughOBJ.ModifiedByNavigation?.MiddleName + " " + GetDailyReportThroughOBJ.ModifiedByNavigation?.LastName;

                                DailyReportThroughDataResponse.ModifiedById = GetDailyReportThroughOBJ?.ModifiedByNavigation?.Id;


                                GetDailyReportThroughDataResponseList.Add(DailyReportThroughDataResponse);
                            }



                        }

                    }

                }
                response.DailyReportThroughList = GetDailyReportThroughDataResponseList;
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

        public BaseResponseWithID AddEditDailyReportThrough(DailyReportThroughData request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {

                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Daily Report Through Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var DailyReportThroughDB = _unitOfWork.DailyReportThroughs.GetById(request.ID ?? 0);
                            if (DailyReportThroughDB != null)
                            {
                                // Update
                                /*var DailyReportThroughUpdate = _Context.proc_DailyReportThroughUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                   Request.Description,
                                                                                                   DailyReportThroughDB.CreatedBy,
                                                                                                   DailyReportThroughDB.CreationDate,
                                                                                                   validation.userID,
                                                                                                   //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                                                                                                   DateTime.Now,
                                                                                                    Request.Active
                                                                                                    );*/
                                DailyReportThroughDB.Name = request.Name;
                                DailyReportThroughDB.Description = request.Description;
                                DailyReportThroughDB.ModifedDate = DateTime.Now;
                                DailyReportThroughDB.ModifiedBy = UserID;
                                DailyReportThroughDB.Active = request.Active;

                                var DailyReportThroughUpdate = _unitOfWork.Complete();

                                if (DailyReportThroughUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Daily Reprt Through !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Daily Reprt Through  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /* ObjectParameter DailyReportThroughID = new ObjectParameter("ID", typeof(long));
                             var DailyReportThroughInsert = _Context.proc_DailyReportThroughInsert(DailyReportThroughID,
                                                                                             Request.Name,
                                                                                             Request.Description,

                                                                                             validation.userID,
                                                                                             DateTime.Now,
                                                                                             null,
                                                                                             null,
                                                                                                 Request.Active
                                                                                            );*/

                            var DailyReport = new DailyReportThrough()
                            {
                                Name = request.Name,
                                Description = request.Description,
                                CreatedBy = UserID,
                                CreationDate = DateTime.Now,
                                ModifedDate = DateTime.Now,
                                ModifiedBy = UserID,
                                Active = request.Active
                            };

                            _unitOfWork.DailyReportThroughs.Add(DailyReport);

                            var DailyReportThroughInsert = _unitOfWork.Complete();

                            if (DailyReportThroughInsert > 0)
                            {
                                var DailyReportThroughIDInsert = long.Parse(DailyReport.Id.ToString());
                                Response.ID = DailyReportThroughIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Daily Reprt Through!!";
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

        public async Task<GetInventoryUOMResponse> GetInventoryUOM()
        {
            GetInventoryUOMResponse response = new GetInventoryUOMResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetInventoryUOMResponseList = new List<InventoryUOMData>();
                if (response.Result)
                {
                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetInventoryUOMDB = await _unitOfWork.InventoryUoms.FindAllAsync(a => (true), new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                        if (GetInventoryUOMDB != null && GetInventoryUOMDB.Count() > 0)
                        {

                            foreach (var GetInventoryUOMDBJ in GetInventoryUOMDB)
                            {
                                var InventoryUOMResponse = new InventoryUOMData();

                                InventoryUOMResponse.ID = (int)GetInventoryUOMDBJ.Id;

                                InventoryUOMResponse.Name = GetInventoryUOMDBJ.Name.Trim();

                                InventoryUOMResponse.ShortName = GetInventoryUOMDBJ.ShortName.Trim();

                                InventoryUOMResponse.Active = GetInventoryUOMDBJ.Active;

                                InventoryUOMResponse.CreatedByID = GetInventoryUOMDBJ.CreatedByNavigation.Id;

                                InventoryUOMResponse.CreatedBy = GetInventoryUOMDBJ.CreatedByNavigation.FirstName + " " + GetInventoryUOMDBJ.CreatedByNavigation?.MiddleName + " " + GetInventoryUOMDBJ.CreatedByNavigation?.LastName;

                                if (GetInventoryUOMDBJ.ModifiedByNavigation != null) InventoryUOMResponse.ModifiedBy = GetInventoryUOMDBJ.ModifiedByNavigation.FirstName + " " + GetInventoryUOMDBJ.ModifiedByNavigation?.MiddleName + " " + GetInventoryUOMDBJ.ModifiedByNavigation?.LastName;

                                if (GetInventoryUOMDBJ.ModifiedByNavigation != null) InventoryUOMResponse.ModifiedByID = GetInventoryUOMDBJ?.ModifiedByNavigation.Id;

                                GetInventoryUOMResponseList.Add(InventoryUOMResponse);
                            }



                        }

                    }

                }
                response.InventoryUOMDList = GetInventoryUOMResponseList;
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

        public BaseResponseWithID AddEditInventoryUOM(InventoryUOMData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Inventory UOM Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var InventoryUOMDB = _unitOfWork.InventoryUoms.GetById(request.ID ?? 0);
                            if (InventoryUOMDB != null)
                            {
                                // Update
                                /*var InventoryUOMUpdate = _Context.proc_InventoryUOMUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                   Request.ShortName,
                                                                                                   Request.Active,
                                                                                                   InventoryUOMDB.CreatedBy,
                                                                                                   InventoryUOMDB.CreationDate,
                                                                                                   validation.userID,
                                                                                                   //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                                                                                                   DateTime.Now
                                                                                                    );*/
                                InventoryUOMDB.Name = request.Name;
                                InventoryUOMDB.ShortName = request.ShortName;
                                InventoryUOMDB.Active = request.Active;
                                InventoryUOMDB.ModifiedBy = UserId;
                                InventoryUOMDB.ModifiedDate = DateTime.Now;

                                var InventoryUOMUpdate = _unitOfWork.Complete();

                                if (InventoryUOMUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this InventoryUOM  !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This InventoryUOM Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter InventoryUOMID = new ObjectParameter("ID", typeof(int));
                            var InventoryUOMInsert = _Context.proc_InventoryUOMInsert(InventoryUOMID,
                                                                                            Request.Name,
                                                                                            Request.ShortName,
                                                                                            Request.Active,
                                                                                            validation.userID,
                                                                                            DateTime.Now,
                                                                                            null,
                                                                                            null
                                                                                           );*/

                            var InventoryUom = new InventoryUom()
                            {
                                Name = request.Name,
                                ShortName = request.ShortName,
                                Active = request.Active,
                                CreatedBy = UserId,
                                ModifiedBy = UserId,
                                CreationDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                            };
                            _unitOfWork.InventoryUoms.Add(InventoryUom);

                            var InventoryUOMInsert = _unitOfWork.Complete();

                            if (InventoryUOMInsert > 0)
                            {
                                var InventoryUOMIDInsert = long.Parse(InventoryUom.Id.ToString());
                                Response.ID = InventoryUOMIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this InventoryUOM!!";
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

        public async Task<GetDeliveryAndShippingMethodResponse> GetDeliveryAndShippingMethod()
        {
            GetDeliveryAndShippingMethodResponse response = new GetDeliveryAndShippingMethodResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var DeliveryAndShippingMethodDataList = new List<DeliveryAndShippingMethodData>();
                if (response.Result)
                {
                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetDeliveryAndShippingMethodDB = await _unitOfWork.DeliveryAndShippingMethods.GetAllAsync();


                        if (GetDeliveryAndShippingMethodDB != null && GetDeliveryAndShippingMethodDB.Count() > 0)
                        {

                            foreach (var GetDeliveryAndShippingMethodDBBJ in GetDeliveryAndShippingMethodDB)
                            {
                                var GetDeliveryAndShippingMethodResponse = new DeliveryAndShippingMethodData();

                                GetDeliveryAndShippingMethodResponse.ID = (int)GetDeliveryAndShippingMethodDBBJ.Id;

                                GetDeliveryAndShippingMethodResponse.Name = GetDeliveryAndShippingMethodDBBJ.Name;

                                GetDeliveryAndShippingMethodResponse.Active = GetDeliveryAndShippingMethodDBBJ.Active;

                                GetDeliveryAndShippingMethodResponse.CreatefById = GetDeliveryAndShippingMethodDBBJ.CreatedBy;


                                DeliveryAndShippingMethodDataList.Add(GetDeliveryAndShippingMethodResponse);
                            }



                        }

                    }

                }
                response.DeliveryAndShippingMethodList = DeliveryAndShippingMethodDataList;
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

        public BaseResponseWithID AddEditDeliveryAndShippingMethod(DeliveryAndShippingMethodData request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {


                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Delivery and Shipping Method Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var DeliveryAndShippingMethodDB = _unitOfWork.DeliveryAndShippingMethods.GetById(request.ID ?? 0);
                            if (DeliveryAndShippingMethodDB != null)
                            {
                                // Update
                                /*var DeliveryAndShippingMethodUpdate = _Context.proc_DeliveryAndShippingMethodUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                    Request.Active,
                                                                                                   DeliveryAndShippingMethodDB.CreatedBy,
                                                                                                   DeliveryAndShippingMethodDB.CreationDate,
                                                                                                   validation.userID,
                                                                                                   //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                                                                                                   DateTime.Now
                                                                                                    );*/
                                DeliveryAndShippingMethodDB.Name = request.Name;
                                DeliveryAndShippingMethodDB.Active = request.Active;
                                DeliveryAndShippingMethodDB.ModifiedBy = UserID;
                                DeliveryAndShippingMethodDB.ModifiedDate = DateTime.Now;

                                var DeliveryAndShippingMethodUpdate = _unitOfWork.Complete();

                                if (DeliveryAndShippingMethodUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Delivery And Shipping Method  !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Delivery And Shipping Method Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter DeliveryAndShippingMethodID = new ObjectParameter("ID", typeof(int));
                            var DeliveryAndShippingMethodInsert = _Context.proc_DeliveryAndShippingMethodInsert(DeliveryAndShippingMethodID,
                                                                                            Request.Name,
                                                                                            Request.Active,
                                                                                            validation.userID,
                                                                                            DateTime.Now,
                                                                                            null,
                                                                                            null
                                                                                           );*/
                            var Delivery = new DeliveryAndShippingMethod()
                            {
                                Name = request.Name,
                                Active = request.Active,
                                CreatedBy = UserID,
                                CreationDate = DateTime.Now,
                                ModifiedBy = UserID,
                                ModifiedDate = DateTime.Now,
                            };
                            _unitOfWork.DeliveryAndShippingMethods.Add(Delivery);
                            var DeliveryAndShippingMethodInsert = _unitOfWork.Complete();
                            if (DeliveryAndShippingMethodInsert > 0)
                            {
                                var DeliveryAndShippingMethodIDInsert = long.Parse(Delivery.Id.ToString());
                                Response.ID = DeliveryAndShippingMethodIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Delivery And Shipping Method!!";
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

        public async Task<GetSalesExtraCostTypeResponse> GetSalesExtraCostType()
        {
            GetSalesExtraCostTypeResponse response = new GetSalesExtraCostTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                var SalesExtraCostTypeList = new List<SalesExtraCostTypeData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var SalesExtraCostTypeDB = await _unitOfWork.SalesExtraCostTypes.GetAllAsync();


                        if (SalesExtraCostTypeDB != null && SalesExtraCostTypeDB.Count() > 0)
                        {

                            foreach (var SalesExtraCostTypeDBBJ in SalesExtraCostTypeDB)
                            {
                                var SalesExtraCostTypeResponse = new SalesExtraCostTypeData();

                                SalesExtraCostTypeResponse.ID = (int)SalesExtraCostTypeDBBJ.Id;

                                SalesExtraCostTypeResponse.Name = SalesExtraCostTypeDBBJ.Name;

                                SalesExtraCostTypeResponse.Active = SalesExtraCostTypeDBBJ.Active;




                                SalesExtraCostTypeList.Add(SalesExtraCostTypeResponse);
                            }



                        }

                    }

                }
                response.SalesExtraCostTypeList = SalesExtraCostTypeList;
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

        public BaseResponseWithID AddEditSalesExtraCostType(SalesExtraCostTypeData request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var SalesExtraCostTypeDB = _unitOfWork.SalesExtraCostTypes.GetById((long)request.ID);
                            if (SalesExtraCostTypeDB != null)
                            {
                                // Update
                                /*var SalesExtraCostTypeUpdate = _Context.proc_SalesExtraCostTypeUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                    Request.Active,
                                                                                                   SalesExtraCostTypeDB.CreationDate
                                                                                                    );*/
                                SalesExtraCostTypeDB.Name = request.Name;
                                SalesExtraCostTypeDB.Active = request.Active;

                                var SalesExtraCostTypeUpdate = _unitOfWork.Complete();
                                if (SalesExtraCostTypeUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Sales Extra Cost Type  !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Sales Extra Cost Type Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter SalesExtraCostTypeID = new ObjectParameter("ID", typeof(int));
                            var SalesExtraCostTypeInsert = _Context.proc_SalesExtraCostTypeInsert(SalesExtraCostTypeID,
                                                                                            Request.Name,
                                                                                            Request.Active,

                                                                                            DateTime.Now

                                                                                           );*/

                            var SalesCost = new SalesExtraCostType()
                            {
                                Name = request.Name,
                                Active = request.Active,
                                CreationDate = DateTime.Now,
                            };

                            _unitOfWork.SalesExtraCostTypes.Add(SalesCost);
                            var SalesExtraCostTypeInsert = _unitOfWork.Complete();

                            if (SalesExtraCostTypeInsert > 0)
                            {
                                var SalesExtraCostTypeIDInsert = long.Parse(SalesCost.Id.ToString());
                                Response.ID = SalesExtraCostTypeIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Sales Extra Cost Type!!";
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

        public async Task<GetDailyTransactionToGeneralTypeResponse> GetDailyTransactionToGeneralType()
        {
            GetDailyTransactionToGeneralTypeResponse response = new GetDailyTransactionToGeneralTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                var DailyTransactionToGeneralTypeList = new List<DailyTransactionToGeneralTypeData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var DailyTransactionToGeneralTypeDB = await _unitOfWork.DailyTranactionBeneficiaryToGeneralTypes.FindAllAsync(a => (true), new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                        if (DailyTransactionToGeneralTypeDB != null && DailyTransactionToGeneralTypeDB.Count() > 0)
                        {

                            foreach (var DailyTransactionToGeneralTypeDBBJ in DailyTransactionToGeneralTypeDB)
                            {
                                var DailyTransactionToGeneralTypeResponse = new DailyTransactionToGeneralTypeData();

                                DailyTransactionToGeneralTypeResponse.ID = (int)DailyTransactionToGeneralTypeDBBJ.Id;

                                DailyTransactionToGeneralTypeResponse.Name = DailyTransactionToGeneralTypeDBBJ.Name;

                                DailyTransactionToGeneralTypeResponse.Description = DailyTransactionToGeneralTypeDBBJ.Description;

                                DailyTransactionToGeneralTypeResponse.Location = DailyTransactionToGeneralTypeDBBJ.Location;

                                DailyTransactionToGeneralTypeResponse.Active = DailyTransactionToGeneralTypeDBBJ.Active;

                                DailyTransactionToGeneralTypeResponse.CreatedBy = DailyTransactionToGeneralTypeDBBJ.CreatedByNavigation.FirstName + " " + DailyTransactionToGeneralTypeDBBJ.CreatedByNavigation?.MiddleName + " " + DailyTransactionToGeneralTypeDBBJ.CreatedByNavigation?.LastName;

                                DailyTransactionToGeneralTypeResponse.CreatedById = DailyTransactionToGeneralTypeDBBJ.CreatedBy;

                                if (DailyTransactionToGeneralTypeDBBJ.ModifiedBy != null) DailyTransactionToGeneralTypeResponse.ModifiedBy = DailyTransactionToGeneralTypeDBBJ.ModifiedByNavigation.FirstName + " " + DailyTransactionToGeneralTypeDBBJ.ModifiedByNavigation?.MiddleName + " " + DailyTransactionToGeneralTypeDBBJ.ModifiedByNavigation?.LastName;

                                DailyTransactionToGeneralTypeResponse.ModifiedById = DailyTransactionToGeneralTypeDBBJ?.ModifiedByNavigation?.Id;



                                DailyTransactionToGeneralTypeList.Add(DailyTransactionToGeneralTypeResponse);
                            }


                        }

                    }

                }
                response.DailyTransactionToGeneralTypeList = DailyTransactionToGeneralTypeList;
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

        public BaseResponseWithID AddEditDailyTransactionToGeneralType(DailyTransactionToGeneralTypeData request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {

                            var DailyTransactionToGeneralTypeDB = _unitOfWork.DailyTranactionBeneficiaryToGeneralTypes.GetById((long)request.ID);
                            if (DailyTransactionToGeneralTypeDB != null)
                            {
                                // Update
                                /*var DailyTransactionToGeneralTypeUpdate = _Context.proc_DailyTranactionBeneficiaryToGeneralTypeUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                   Request.Location,
                                                                                                   Request.Description,
                                                                                                    Request.Active,
                                                                                                   DailyTransactionToGeneralTypeDB.CreatedBy,
                                                                                                   DailyTransactionToGeneralTypeDB.CreationDate,
                                                                                                    validation.userID,
                                                                                                   //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                                                                                                   DateTime.Now

                                                                                                    );*/
                                DailyTransactionToGeneralTypeDB.Name = request.Name;
                                DailyTransactionToGeneralTypeDB.Location = request.Location;
                                DailyTransactionToGeneralTypeDB.Description = request.Description;
                                DailyTransactionToGeneralTypeDB.Active = request.Active;
                                DailyTransactionToGeneralTypeDB.ModifiedDate = DateTime.Now;
                                DailyTransactionToGeneralTypeDB.ModifiedBy = UserID;

                                var DailyTransactionToGeneralTypeUpdate = _unitOfWork.Complete();

                                if (DailyTransactionToGeneralTypeUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Daily Transaction To General Type !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "ThisDaily Transaction To General Type Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter DailyTransactionToGeneralTypeID = new ObjectParameter("ID", typeof(int));
                            var DailyTransactionToGeneralTypeInsert = _Context.proc_DailyTranactionBeneficiaryToGeneralTypeInsert(DailyTransactionToGeneralTypeID,
                                                                                            Request.Name,
                                                                                            Request.Location,
                                                                                            Request.Description,
                                                                                            Request.Active,
                                                                                            validation.userID,
                                                                                            DateTime.Now,
                                                                                            null,
                                                                                            null
                                                                                           );*/

                            var DailyTransaction = new DailyTranactionBeneficiaryToGeneralType()
                            {
                                Name = request.Name,
                                Location = request.Location,
                                Description = request.Description,
                                Active = request.Active,
                                CreationDate = DateTime.Now,
                                CreatedBy = UserID,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = UserID
                            };
                            _unitOfWork.DailyTranactionBeneficiaryToGeneralTypes.Add(DailyTransaction);
                            var DailyTransactionToGeneralTypeInsert = _unitOfWork.Complete();
                            if (DailyTransactionToGeneralTypeInsert > 0)
                            {
                                var DailyTransactionToGeneralTypeIDInsert = long.Parse(DailyTransaction.Id.ToString());
                                Response.ID = DailyTransactionToGeneralTypeIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Daily Transaction To General Type!!";
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

        public BaseResponseWithID AddEditDailyTranactionBeneficiaryToType(DailyTranactionBeneficiaryToTypeData request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.BeneficiaryName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var DailyTranactionBeneficiaryToTypeDB = _unitOfWork.DailyTranactionBeneficiaryToTypes.GetById((long)request.ID);
                            if (DailyTranactionBeneficiaryToTypeDB != null)
                            {
                                // Update
                                /*var DailyTranactionBeneficiaryToTypeUpdate = _Context.proc_DailyTranactionBeneficiaryToTypeUpdate(Request.ID,
                                                                                                   Request.BeneficiaryName,
                                                                                                   Request.Description,
                                                                                                    Request.Active,
                                                                                                   DailyTranactionBeneficiaryToTypeDB.CreatedBy,
                                                                                                   DailyTranactionBeneficiaryToTypeDB.CreationDate,
                                                                                                    validation.userID,
                                                                                                   //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                                                                                                   DateTime.Now

                                                                                                    );*/
                                DailyTranactionBeneficiaryToTypeDB.BeneficiaryName = request.BeneficiaryName;
                                DailyTranactionBeneficiaryToTypeDB.Description = request.Description;
                                DailyTranactionBeneficiaryToTypeDB.Active = request.Active;
                                DailyTranactionBeneficiaryToTypeDB.ModifiedBy = UserID;
                                DailyTranactionBeneficiaryToTypeDB.ModifiedDate = DateTime.Now;
                                var DailyTranactionBeneficiaryToTypeUpdate = _unitOfWork.Complete();
                                if (DailyTranactionBeneficiaryToTypeUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Daily Tranaction Beneficiary To Type !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Daily Tranaction Beneficiary To Type Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        else
                        {
                            // Insert
                            /*      ObjectParameter DailyTranactionBeneficiaryToTypeID = new ObjectParameter("ID", typeof(int));
                                  var DailyTranactionBeneficiaryToTypeInsert = _Context.proc_DailyTranactionBeneficiaryToTypeInsert(DailyTranactionBeneficiaryToTypeID,
                                                                                                  Request.BeneficiaryName,
                                                                                                  Request.Description,
                                                                                                  Request.Active,
                                                                                                  validation.userID,
                                                                                                  DateTime.Now,
                                                                                                  null,
                                                                                                  null
                                                                                                 );*/

                            var DailyTransaction = new DailyTranactionBeneficiaryToType()
                            {
                                BeneficiaryName = request.BeneficiaryName,
                                Description = request.Description,
                                Active = request.Active,
                                CreatedBy = UserID,
                                ModifiedBy = UserID,
                                CreationDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                            };
                            _unitOfWork.DailyTranactionBeneficiaryToTypes.Add(DailyTransaction);
                            var DailyTranactionBeneficiaryToTypeInsert = _unitOfWork.Complete();
                            if (DailyTranactionBeneficiaryToTypeInsert > 0)
                            {
                                var DailyTransactionToTypeIDInsert = long.Parse(DailyTransaction.Id.ToString());
                                Response.ID = DailyTransactionToTypeIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Daily Transaction To Type!!";
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

        public async Task<GetPurchasePOInvoiceTaxIncludedTypeResponse> GetPurchasePOInvoiceTaxIncludedType()
        {
            GetPurchasePOInvoiceTaxIncludedTypeResponse response = new GetPurchasePOInvoiceTaxIncludedTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                var PurchasePOInvoiceTaxIncludedTypeResponse = new List<PurchasePOInvoiceTaxIncludedTypeData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var PurchasePOInvoiceTaxIncludedTypeDB = await _unitOfWork.PurchasePoinvoiceTaxIncludedTypes.GetAllAsync();


                        if (PurchasePOInvoiceTaxIncludedTypeDB != null && PurchasePOInvoiceTaxIncludedTypeDB.Count() > 0)
                        {

                            foreach (var PurchasePOInvoiceTaxIncludedTypeOBJ in PurchasePOInvoiceTaxIncludedTypeDB)
                            {
                                var PurchasePOInvoiceTaxIncludedTypeResponseList = new PurchasePOInvoiceTaxIncludedTypeData();

                                PurchasePOInvoiceTaxIncludedTypeResponseList.ID = (int)PurchasePOInvoiceTaxIncludedTypeOBJ.Id;

                                PurchasePOInvoiceTaxIncludedTypeResponseList.Name = PurchasePOInvoiceTaxIncludedTypeOBJ.Name.Trim();

                                PurchasePOInvoiceTaxIncludedTypeResponseList.Active = PurchasePOInvoiceTaxIncludedTypeOBJ.Active;




                                PurchasePOInvoiceTaxIncludedTypeResponse.Add(PurchasePOInvoiceTaxIncludedTypeResponseList);
                            }



                        }

                    }

                }
                response.PurchasePOInvoiceTaxIncludedTypeList = PurchasePOInvoiceTaxIncludedTypeResponse;
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

        public async Task<GetDailyTranactionBeneficiaryToTypeResponse> GetDailyTranactionBeneficiaryToType()
        {
            GetDailyTranactionBeneficiaryToTypeResponse response = new GetDailyTranactionBeneficiaryToTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {
                var DailyTranactionBeneficiaryToTypeList = new List<DailyTranactionBeneficiaryToTypeData>();
                if (response.Result)
                {

                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var DailyTranactionBeneficiaryToTypeDB = await _unitOfWork.DailyTranactionBeneficiaryToTypes.FindAllAsync(a => (true), new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                        if (DailyTranactionBeneficiaryToTypeDB != null && DailyTranactionBeneficiaryToTypeDB.Count() > 0)
                        {

                            foreach (var DailyTranactionBeneficiaryToTypeOBJ in DailyTranactionBeneficiaryToTypeDB)
                            {
                                var DailyTranactionBeneficiaryToTypeResponse = new DailyTranactionBeneficiaryToTypeData();

                                DailyTranactionBeneficiaryToTypeResponse.ID = (int)DailyTranactionBeneficiaryToTypeOBJ.Id;

                                DailyTranactionBeneficiaryToTypeResponse.BeneficiaryName = DailyTranactionBeneficiaryToTypeOBJ.BeneficiaryName;

                                DailyTranactionBeneficiaryToTypeResponse.Description = DailyTranactionBeneficiaryToTypeOBJ.Description;

                                DailyTranactionBeneficiaryToTypeResponse.Active = DailyTranactionBeneficiaryToTypeOBJ.Active;

                                DailyTranactionBeneficiaryToTypeResponse.CreatedBy = DailyTranactionBeneficiaryToTypeOBJ.CreatedByNavigation.FirstName + " " + DailyTranactionBeneficiaryToTypeOBJ.CreatedByNavigation?.MiddleName + " " + DailyTranactionBeneficiaryToTypeOBJ.CreatedByNavigation?.LastName;

                                DailyTranactionBeneficiaryToTypeResponse.CreatedById = DailyTranactionBeneficiaryToTypeOBJ.CreatedBy;

                                if (DailyTranactionBeneficiaryToTypeOBJ.ModifiedBy != null) DailyTranactionBeneficiaryToTypeResponse.ModifiedBy = DailyTranactionBeneficiaryToTypeOBJ.ModifiedByNavigation.FirstName + " " + DailyTranactionBeneficiaryToTypeOBJ.ModifiedByNavigation?.MiddleName + " " + DailyTranactionBeneficiaryToTypeOBJ.ModifiedByNavigation?.LastName;

                                DailyTranactionBeneficiaryToTypeResponse.ModifiedById = DailyTranactionBeneficiaryToTypeOBJ?.ModifiedByNavigation?.Id;


                                DailyTranactionBeneficiaryToTypeList.Add(DailyTranactionBeneficiaryToTypeResponse);
                            }

                        }

                    }

                }
                response.DailyTransactionToTypeList = DailyTranactionBeneficiaryToTypeList;
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

        public BaseResponseWithID AddEditPurchasePOInvoiceTaxIncludedType(PurchasePOInvoiceTaxIncludedTypeData request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var PurchasePOInvoiceTaxIncludedTypeDB = _unitOfWork.PurchasePoinvoiceTaxIncludedTypes.GetById((long)request.ID);
                            if (PurchasePOInvoiceTaxIncludedTypeDB != null)
                            {
                                // Update
                                /*var PurchasePOInvoiceTaxIncludedTypeDBUpdate = _Context.proc_PurchasePOInvoiceTaxIncludedTypeUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                    Request.Active


                                                                                                    );*/
                                PurchasePOInvoiceTaxIncludedTypeDB.Name = request.Name;
                                PurchasePOInvoiceTaxIncludedTypeDB.Active = request.Active;
                                var PurchasePOInvoiceTaxIncludedTypeDBUpdate = _unitOfWork.Complete();
                                if (PurchasePOInvoiceTaxIncludedTypeDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Purchase PO Invoice Tax Included Type !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Purchase PO Invoice Tax Included Type Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter PurchasePOInvoiceTaxIncludedTypeID = new ObjectParameter("ID", typeof(int));
                            var PurchasePOInvoiceTaxIncludedTypeInsert = _Context.proc_PurchasePOInvoiceTaxIncludedTypeInsert(PurchasePOInvoiceTaxIncludedTypeID,
                                                                                            Request.Name,
                                                                                            Request.Active



                                                                                           );*/
                            var Purchase = new PurchasePoinvoiceTaxIncludedType()
                            {
                                Name = request.Name,
                                Active = request.Active,
                            };
                            _unitOfWork.PurchasePoinvoiceTaxIncludedTypes.Add(Purchase);
                            var PurchasePOInvoiceTaxIncludedTypeInsert = _unitOfWork.Complete();
                            if (PurchasePOInvoiceTaxIncludedTypeInsert > 0)
                            {
                                var PurchasePOInvoiceTaxIncludedTypeIDInsert = long.Parse(Purchase.Id.ToString());
                                Response.ID = PurchasePOInvoiceTaxIncludedTypeIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Purchase PO Invoice Tax Included Type!!";
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

        public async Task<GetPurchasePOInvoiceNotIncludeTaxTypeResponse> GetPurchasePOInvoiceNotIncludeTaxType()
        {
            GetPurchasePOInvoiceNotIncludeTaxTypeResponse response = new GetPurchasePOInvoiceNotIncludeTaxTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var PurchasePOInvoiceNotIncludeTaxTypeResponseList = new List<PurchasePOInvoiceNotIncludeTaxTypeData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var PurchasePOInvoiceNotIncludeTaxTypeDB = await _unitOfWork.PurchasePoinvoiceNotIncludedTaxTypes.GetAllAsync();


                        if (PurchasePOInvoiceNotIncludeTaxTypeDB != null && PurchasePOInvoiceNotIncludeTaxTypeDB.Count() > 0)
                        {

                            foreach (var PurchasePOInvoiceNotIncludeTaxTypeOBJ in PurchasePOInvoiceNotIncludeTaxTypeDB)
                            {
                                var PurchasePOInvoiceTaxIncludedTypeResponse = new PurchasePOInvoiceNotIncludeTaxTypeData();

                                PurchasePOInvoiceTaxIncludedTypeResponse.ID = (int)PurchasePOInvoiceNotIncludeTaxTypeOBJ.Id;

                                PurchasePOInvoiceTaxIncludedTypeResponse.Name = PurchasePOInvoiceNotIncludeTaxTypeOBJ.Name.Trim();

                                PurchasePOInvoiceTaxIncludedTypeResponse.Active = PurchasePOInvoiceNotIncludeTaxTypeOBJ.Active;




                                PurchasePOInvoiceNotIncludeTaxTypeResponseList.Add(PurchasePOInvoiceTaxIncludedTypeResponse);
                            }



                        }

                    }

                }
                response.PurchasePOInvoiceNotIncludeTaxTypeList = PurchasePOInvoiceNotIncludeTaxTypeResponseList;
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

        public BaseResponseWithID AddEditPurchasePOInvoiceNotIncludeTaxType(PurchasePOInvoiceNotIncludeTaxTypeData request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var PurchasePOInvoiceNotIncludeTaxTypeDB = _unitOfWork.PurchasePoinvoiceNotIncludedTaxTypes.GetById((long)request.ID);
                            if (PurchasePOInvoiceNotIncludeTaxTypeDB != null)
                            {
                                // Update
                                /* var PurchasePOInvoiceNotIncludeTaxTypeDBUpdate = _Context.proc_PurchasePOInvoiceNotIncludedTaxTypeUpdate(Request.ID,
                                                                                                    Request.Name,
                                                                                                     Request.Active
                                                                                                     );*/
                                PurchasePOInvoiceNotIncludeTaxTypeDB.Name = request.Name;
                                PurchasePOInvoiceNotIncludeTaxTypeDB.Active = request.Active;
                                var PurchasePOInvoiceNotIncludeTaxTypeDBUpdate = _unitOfWork.Complete();

                                if (PurchasePOInvoiceNotIncludeTaxTypeDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Purchase PO Invoice Not Include Tax Type !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Purchase PO Invoice Not Include Tax Type Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /* ObjectParameter PurchasePOInvoiceNotIncludeTaxTypeID = new ObjectParameter("ID", typeof(int));
                             var PurchasePOInvoiceNotIncludeTaxTypeInsert = _Context.proc_PurchasePOInvoiceNotIncludedTaxTypeInsert(PurchasePOInvoiceNotIncludeTaxTypeID,
                                                                                             Request.Name,
                                                                                             Request.Active
                                                                                            );*/

                            var purchase = new PurchasePoinvoiceNotIncludedTaxType()
                            {
                                Name = request.Name,
                                Active = request.Active,
                            };
                            _unitOfWork.PurchasePoinvoiceNotIncludedTaxTypes.Add(purchase);
                            var PurchasePOInvoiceNotIncludeTaxTypeInsert = _unitOfWork.Complete();
                            if (PurchasePOInvoiceNotIncludeTaxTypeInsert > 0)
                            {
                                var PurchasePOInvoiceNotIncludeTaxTypeIDInsert = long.Parse(purchase.Id.ToString());
                                Response.ID = PurchasePOInvoiceNotIncludeTaxTypeIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Purchase PO Invoice Not Include Tax Type!!";
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

        public async Task<GetPurchasePOInvoiceExtraFeesTypeResponse> GetPurchasePOInvoiceExtraFeesType()
        {
            GetPurchasePOInvoiceExtraFeesTypeResponse response = new GetPurchasePOInvoiceExtraFeesTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetPurchasePOInvoiceExtraFeesTypeResponseList = new List<PurchasePOInvoiceExtraFeesTypeData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetPurchasePOInvoiceExtraFeesTypeDB = await _unitOfWork.PurchasePoinvoiceExtraFeesTypes.GetAllAsync();


                        if (GetPurchasePOInvoiceExtraFeesTypeDB != null && GetPurchasePOInvoiceExtraFeesTypeDB.Count() > 0)
                        {

                            foreach (var GetPurchasePOInvoiceExtraFeesTypeOBJ in GetPurchasePOInvoiceExtraFeesTypeDB)
                            {
                                var GetPurchasePOInvoiceExtraFeesTypeResponse = new PurchasePOInvoiceExtraFeesTypeData();

                                GetPurchasePOInvoiceExtraFeesTypeResponse.ID = (int)GetPurchasePOInvoiceExtraFeesTypeOBJ.Id;

                                GetPurchasePOInvoiceExtraFeesTypeResponse.Name = GetPurchasePOInvoiceExtraFeesTypeOBJ.Name.Trim();

                                GetPurchasePOInvoiceExtraFeesTypeResponse.Active = GetPurchasePOInvoiceExtraFeesTypeOBJ.Active;




                                GetPurchasePOInvoiceExtraFeesTypeResponseList.Add(GetPurchasePOInvoiceExtraFeesTypeResponse);
                            }



                        }

                    }

                }
                response.PurchasePOInvoiceExtraFeesTypeList = GetPurchasePOInvoiceExtraFeesTypeResponseList;
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

        public BaseResponseWithID AddEditPurchasePOInvoiceExtraFeesType(PurchasePOInvoiceExtraFeesTypeData request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var PurchasePOInvoiceExtraFeesTypeDB = _unitOfWork.PurchasePoinvoiceExtraFeesTypes.GetById((long)request.ID);
                            if (PurchasePOInvoiceExtraFeesTypeDB != null)
                            {
                                // Update
                                /*var AddEditPurchasePOInvoiceExtraFeesTypeDBUpdate = _Context.proc_PurchasePOInvoiceExtraFeesTypeUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                    Request.Active
                                                                                                    );*/
                                PurchasePOInvoiceExtraFeesTypeDB.Name = request.Name;
                                PurchasePOInvoiceExtraFeesTypeDB.Active = request.Active;
                                var AddEditPurchasePOInvoiceExtraFeesTypeDBUpdate = _unitOfWork.Complete();
                                if (AddEditPurchasePOInvoiceExtraFeesTypeDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Purchase PO Invoice Extra Fees Type !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Purchase PO Invoice Extra Fees Type  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter PurchasePOInvoiceExtraFeesTypeID = new ObjectParameter("ID", typeof(int));
                            var PurchasePOInvoiceExtraFeesTypeInsert = _Context.proc_PurchasePOInvoiceExtraFeesTypeInsert(PurchasePOInvoiceExtraFeesTypeID,
                                                                                            Request.Name,
                                                                                            Request.Active
                                                                                           );*/

                            var purchase = new PurchasePoinvoiceExtraFeesType()
                            {
                                Name = request.Name,
                                Active = request.Active,
                            };
                            _unitOfWork.PurchasePoinvoiceExtraFeesTypes.Add(purchase);

                            var PurchasePOInvoiceExtraFeesTypeInsert = _unitOfWork.Complete();
                            if (PurchasePOInvoiceExtraFeesTypeInsert > 0)
                            {
                                var PurchasePOInvoiceExtraFeesTypeIDInsert = long.Parse(purchase.Id.ToString());
                                Response.ID = PurchasePOInvoiceExtraFeesTypeIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Purchase PO Invoice Extra Fees Type !!";
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

        public async Task<GetPurchasePOInvoiceDeductionTypeResponse> GetPurchasePOInvoiceDeductionType()
        {
            GetPurchasePOInvoiceDeductionTypeResponse response = new GetPurchasePOInvoiceDeductionTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetPurchasePOInvoiceDeductionTypeResponseList = new List<PurchasePOInvoiceDeductionTypeData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetPurchasePOInvoiceDeductionTypeDB = await _unitOfWork.PurchasePoinvoiceDeductionTypes.GetAllAsync();


                        if (GetPurchasePOInvoiceDeductionTypeDB != null && GetPurchasePOInvoiceDeductionTypeDB.Count() > 0)
                        {

                            foreach (var GetPurchasePOInvoiceDeductionTypeOBJ in GetPurchasePOInvoiceDeductionTypeDB)
                            {
                                var GetPurchasePOInvoiceDeductionTypeResponse = new PurchasePOInvoiceDeductionTypeData();

                                GetPurchasePOInvoiceDeductionTypeResponse.ID = (int)GetPurchasePOInvoiceDeductionTypeOBJ.Id;

                                GetPurchasePOInvoiceDeductionTypeResponse.Name = GetPurchasePOInvoiceDeductionTypeOBJ.Name.Trim();

                                GetPurchasePOInvoiceDeductionTypeResponse.Active = GetPurchasePOInvoiceDeductionTypeOBJ.Active;

                                GetPurchasePOInvoiceDeductionTypeResponseList.Add(GetPurchasePOInvoiceDeductionTypeResponse);
                            }



                        }

                    }

                }
                response.PurchasePOInvoiceDeductionTypeList = GetPurchasePOInvoiceDeductionTypeResponseList;
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

        public BaseResponseWithID AddEditPurchasePOInvoiceDeductionType(PurchasePOInvoiceDeductionTypeData request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var PurchasePOInvoiceDeductionTypeDB = _unitOfWork.PurchasePoinvoiceDeductionTypes.GetById((long)request.ID);
                            if (PurchasePOInvoiceDeductionTypeDB != null)
                            {
                                // Update
                                /* var PurchasePOInvoiceDeductionTypeDBUpdate = _Context.proc_PurchasePOInvoiceDeductionTypeUpdate(Request.ID,
                                                                                                    Request.Name,
                                                                                                     Request.Active
                                                                                                     );*/
                                PurchasePOInvoiceDeductionTypeDB.Name = request.Name;
                                PurchasePOInvoiceDeductionTypeDB.Active = request.Active;
                                var PurchasePOInvoiceDeductionTypeDBUpdate = _unitOfWork.Complete();
                                if (PurchasePOInvoiceDeductionTypeDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Purchase PO Invoice Deduction Type !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Purchase PO Invoice Deduction Type  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter PurchasePOInvoiceDeductionTypeID = new ObjectParameter("ID", typeof(int));
                            var PurchasePOInvoiceDeductionTypeInsert = _Context.proc_PurchasePOInvoiceDeductionTypeInsert(PurchasePOInvoiceDeductionTypeID,
                                                                                            Request.Name,
                                                                                            Request.Active
                                                                                           );*/

                            var purchase = new PurchasePoinvoiceDeductionType()
                            {
                                Name = request.Name,
                                Active = request.Active,
                            };
                            _unitOfWork.PurchasePoinvoiceDeductionTypes.Add(purchase);
                            var PurchasePOInvoiceDeductionTypeInsert = _unitOfWork.Complete();
                            if (PurchasePOInvoiceDeductionTypeInsert > 0)
                            {
                                var PurchasePOInvoiceDeductionTypeIDInsert = long.Parse(purchase.Id.ToString());
                                Response.ID = PurchasePOInvoiceDeductionTypeIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Purchase PO Invoice Deduction Type!!";
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

        public async Task<GetPurchasePaymentMethodResponse> GetPurchasePaymentMethod()
        {
            GetPurchasePaymentMethodResponse response = new GetPurchasePaymentMethodResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var GetPurchasePaymentMethodResponseList = new List<PurchasePaymentMethodData>();
                if (response.Result)
                {

                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetPurchasePaymentMethodDataDB = await _unitOfWork.PurchasePaymentMethods.GetAllAsync();


                        if (GetPurchasePaymentMethodDataDB != null && GetPurchasePaymentMethodDataDB.Count() > 0)
                        {

                            foreach (var GetPurchasePaymentMethodDataOBJ in GetPurchasePaymentMethodDataDB)
                            {
                                var GetPurchasePaymentMethodDataResponse = new PurchasePaymentMethodData();

                                GetPurchasePaymentMethodDataResponse.ID = (int)GetPurchasePaymentMethodDataOBJ.Id;

                                GetPurchasePaymentMethodDataResponse.Name = GetPurchasePaymentMethodDataOBJ.Name.Trim();

                                GetPurchasePaymentMethodResponseList.Add(GetPurchasePaymentMethodDataResponse);
                            }



                        }

                    }

                }
                response.PurchasePaymentMethodList = GetPurchasePaymentMethodResponseList;
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

        public BaseResponseWithID AddEditPurchasePaymentMethode(PurchasePaymentMethodData request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var PurchasePaymentMethodeDB = _unitOfWork.PurchasePaymentMethods.GetById((long)request.ID);
                            if (PurchasePaymentMethodeDB != null)
                            {
                                // Update
                                /*var PurchasePaymentMethodeDBUpdate = _Context.proc_PurchasePaymentMethodUpdate(Request.ID,
                                                                                                   Request.Name

                                                                                                    );*/
                                PurchasePaymentMethodeDB.Name = request.Name;
                                var PurchasePaymentMethodeDBUpdate = _unitOfWork.Complete();
                                if (PurchasePaymentMethodeDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Purchase Payment Method !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Purchase PO Invoice Payment Method  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter PurchasePaymentMethodeID = new ObjectParameter("ID", typeof(int));
                            var PurchasePaymentMethodeInsert = _Context.proc_PurchasePaymentMethodInsert(PurchasePaymentMethodeID,
                                                                                            Request.Name
                                                                                           );*/

                            var purchase = new PurchasePaymentMethod()
                            {
                                Name = request.Name,
                            };
                            _unitOfWork.PurchasePaymentMethods.Add(purchase);
                            var PurchasePaymentMethodeInsert = _unitOfWork.Complete();
                            if (PurchasePaymentMethodeInsert > 0)
                            {
                                var PurchasePaymentMethodeIDInsert = long.Parse(purchase.Id.ToString());
                                Response.ID = PurchasePaymentMethodeIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Purchase PO Invoice  Payment Method !!";
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

        public async Task<GetSpecialityForClientResponse> GetSpecialityForClient()
        {
            GetSpecialityForClientResponse response = new GetSpecialityForClientResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetClientSpecialityResponseList = new List<SpecialityForClientDataList>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetClientSpecialityDB = await _unitOfWork.Specialities.FindAllAsync(a => (true), new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                        if (GetClientSpecialityDB != null && GetClientSpecialityDB.Count() > 0)
                        {

                            foreach (var GetClientSpecialityOBJ in GetClientSpecialityDB)
                            {
                                var GetClientSpecialityResponse = new SpecialityForClientDataList();

                                GetClientSpecialityResponse.ID = (int)GetClientSpecialityOBJ.Id;

                                GetClientSpecialityResponse.Name = GetClientSpecialityOBJ.Name;

                                GetClientSpecialityResponse.Active = GetClientSpecialityOBJ.Active;

                                GetClientSpecialityResponse.CreatedBy = GetClientSpecialityOBJ.CreatedByNavigation.FirstName + " " + GetClientSpecialityOBJ.CreatedByNavigation?.MiddleName + " " + GetClientSpecialityOBJ.CreatedByNavigation?.LastName;

                                GetClientSpecialityResponse.CreatedById = GetClientSpecialityOBJ.CreatedBy;

                                if (GetClientSpecialityOBJ.ModifiedBy != null) GetClientSpecialityResponse.ModifiedBy = GetClientSpecialityOBJ.ModifiedByNavigation.FirstName + " " + GetClientSpecialityOBJ.ModifiedByNavigation?.MiddleName + " " + GetClientSpecialityOBJ.ModifiedByNavigation?.LastName;

                                GetClientSpecialityResponse.ModifiedById = GetClientSpecialityOBJ?.ModifiedByNavigation?.Id;




                                GetClientSpecialityResponseList.Add(GetClientSpecialityResponse);
                            }



                        }

                    }

                }
                response.SpecialityforClientList = GetClientSpecialityResponseList;
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

        public BaseResponseWithID AddEditSpecialityforClient(SpecialityForClientDataList request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var ClientSpecialityDB = _unitOfWork.Specialities.GetById((int)request.ID);
                            if (ClientSpecialityDB != null)
                            {
                                // Update
                                /*var ClientSpecialityDBUpdate = _Context.proc_SpecialityUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                   Request.Active,
                                                                                                   ClientSpecialityDB.CreationDate,
                                                                                                   validation.userID,
                                                                                                   DateTime.Now,
                                                                                                   ClientSpecialityDB.CreatedBy
                                                                                                    );*/
                                ClientSpecialityDB.Name = request.Name;
                                ClientSpecialityDB.Active = request.Active;
                                ClientSpecialityDB.ModifiedDate = DateTime.Now;
                                ClientSpecialityDB.ModifiedBy = UserId;
                                var ClientSpecialityDBUpdate = _unitOfWork.Complete();
                                if (ClientSpecialityDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Speciality !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Speciality  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter ClientSpecialityID = new ObjectParameter("ID", typeof(int));
                            var ClientSpecialityInsert = _Context.proc_SpecialityInsert(ClientSpecialityID,
                                                                                            Request.Name,
                                                                                            Request.Active,
                                                                                            DateTime.Now,
                                                                                            null,
                                                                                            null,
                                                                                            validation.userID
                                                                                           );*/

                            var speciality = new Speciality()
                            {
                                Name = request.Name,
                                Active = request.Active,
                                CreationDate = DateTime.Now,
                                CreatedBy = UserId,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = UserId,
                            };
                            _unitOfWork.Specialities.Add(speciality);
                            var ClientSpecialityInsert = _unitOfWork.Complete();


                            if (ClientSpecialityInsert > 0)
                            {
                                var ClientSpecialityIDInsert = long.Parse(speciality.Id.ToString());
                                Response.ID = ClientSpecialityIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Client Speciality !!";
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

        public async Task<GetSpecialitySupplierResponse> GetSpecialitySupplier()
        {
            GetSpecialitySupplierResponse response = new GetSpecialitySupplierResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetSpecialitySupplierList = new List<SpecialitySupplierResponseDataList>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetSpecialitySupplierDB = await _unitOfWork.SpecialitySuppliers.FindAllAsync(a => (true), new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                        if (GetSpecialitySupplierDB != null && GetSpecialitySupplierDB.Count() > 0)
                        {

                            foreach (var GetSpecialitySupplierOBJ in GetSpecialitySupplierDB)
                            {
                                var GetSpecialitySupplierResponse = new SpecialitySupplierResponseDataList();

                                GetSpecialitySupplierResponse.ID = (int)GetSpecialitySupplierOBJ.Id;

                                GetSpecialitySupplierResponse.Name = GetSpecialitySupplierOBJ.Name;

                                GetSpecialitySupplierResponse.Active = GetSpecialitySupplierOBJ.Active;


                                GetSpecialitySupplierResponse.CreatedBy = GetSpecialitySupplierOBJ.CreatedByNavigation.FirstName + " " + GetSpecialitySupplierOBJ.CreatedByNavigation?.MiddleName + " " + GetSpecialitySupplierOBJ.CreatedByNavigation?.LastName;

                                GetSpecialitySupplierResponse.CreatedById = GetSpecialitySupplierOBJ.CreatedBy;

                                if (GetSpecialitySupplierOBJ.ModifiedBy != null) GetSpecialitySupplierResponse.ModifiedBy = GetSpecialitySupplierOBJ.ModifiedByNavigation.FirstName + " " + GetSpecialitySupplierOBJ.ModifiedByNavigation?.MiddleName + " " + GetSpecialitySupplierOBJ.ModifiedByNavigation?.LastName;

                                GetSpecialitySupplierResponse.ModifiedById = GetSpecialitySupplierOBJ?.ModifiedByNavigation?.Id;




                                GetSpecialitySupplierList.Add(GetSpecialitySupplierResponse);
                            }



                        }

                    }

                }
                response.SpecialitySupplierResponseList = GetSpecialitySupplierList;
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

        public BaseResponseWithID AddEditSpecialitySupplier(SpecialitySupplierResponseDataList request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var SpecialitySupplierDB = _unitOfWork.SpecialitySuppliers.Find(a => a.Id == (int)request.ID, new[] { "CreatedByNavigation", "ModifiedByNavigation" });
                            if (SpecialitySupplierDB != null)
                            {
                                // Update
                                /*var SpecialitySupplierDBUpdate = _Context.proc_SpecialitySupplierUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                   Request.Active,
                                                                                                   SpecialitySupplierDB.CreationDate,
                                                                                                   validation.userID,
                                                                                                   DateTime.Now,
                                                                                                   SpecialitySupplierDB.CreatedBy
                                                                                                    );*/
                                SpecialitySupplierDB.Name = request.Name;
                                SpecialitySupplierDB.Active = request.Active;
                                SpecialitySupplierDB.ModifiedBy = UserId;
                                SpecialitySupplierDB.ModifiedDate = DateTime.Now;
                                var SpecialitySupplierDBUpdate = _unitOfWork.Complete();
                                if (SpecialitySupplierDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Speciality Supplier !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Speciality Supplier  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter SpecialitySupplierID = new ObjectParameter("ID", typeof(int));
                            var SpecialitySupplierInsert = _Context.proc_SpecialitySupplierInsert(SpecialitySupplierID,
                                                                                            Request.Name,
                                                                                            Request.Active,
                                                                                            DateTime.Now,
                                                                                            null,
                                                                                            null,
                                                                                            validation.userID
                                                                                           );*/
                            var speciality = new SpecialitySupplier()
                            {
                                Name = request.Name,
                                Active = request.Active,
                                ModifiedBy = UserId,
                                ModifiedDate = DateTime.Now,
                                CreationDate = DateTime.Now,
                                CreatedBy = UserId,
                            };
                            _unitOfWork.SpecialitySuppliers.Add(speciality);
                            var SpecialitySupplierInsert = _unitOfWork.Complete();
                            if (SpecialitySupplierInsert > 0)
                            {
                                var SpecialitySupplierIDInsert = long.Parse(speciality.Id.ToString());
                                Response.ID = SpecialitySupplierIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Speciality Supplier !!";
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

        public GetDepartmentResponse GetDepartment([FromHeader] string DepartmentName, [FromHeader] int? BranchId)
        {
            GetDepartmentResponse response = new GetDepartmentResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetDepartmentList = new List<DepartmentData>();
                if (response.Result)
                {
                    if (response.Result)
                    {
                        var GetDepartmentDB = _unitOfWork.Departments.FindAllQueryable(a => true, new[] { "CreatedByNavigation", "ModifiedByNavigation", "Branch" });

                        if (DepartmentName != null)
                        {

                            var name = HttpUtility.UrlDecode(DepartmentName);
                            GetDepartmentDB = GetDepartmentDB.Where(a => a.Name == name);
                        }
                        if (BranchId != null)
                        {
                            GetDepartmentDB = GetDepartmentDB.Where(a => a.BranchId == BranchId);
                        }

                        var list = GetDepartmentDB.ToList();
                        if (GetDepartmentDB != null)
                        {

                            foreach (var GetDepartmentDBOBJ in list)
                            {
                                var GetDepartmentResponse = new DepartmentData();

                                GetDepartmentResponse.ID = (int)GetDepartmentDBOBJ.Id;

                                GetDepartmentResponse.Name = GetDepartmentDBOBJ.Name;

                                GetDepartmentResponse.Description = GetDepartmentDBOBJ.Description;

                                GetDepartmentResponse.Active = GetDepartmentDBOBJ.Active;

                                GetDepartmentResponse.BranchID = GetDepartmentDBOBJ.Branch.Id;

                                GetDepartmentResponse.BranchName = GetDepartmentDBOBJ.Branch.Name;


                                GetDepartmentResponse.CreatedBy = GetDepartmentDBOBJ.CreatedByNavigation.FirstName + " " + GetDepartmentDBOBJ.CreatedByNavigation?.MiddleName + " " + GetDepartmentDBOBJ.CreatedByNavigation?.LastName;

                                GetDepartmentResponse.CreatedById = GetDepartmentDBOBJ.CreatedBy;

                                if (GetDepartmentDBOBJ.ModifiedBy != null)
                                {
                                    GetDepartmentResponse.ModifiedBy = GetDepartmentDBOBJ.ModifiedByNavigation.FirstName + " " + GetDepartmentDBOBJ.ModifiedByNavigation?.MiddleName + " " + GetDepartmentDBOBJ.ModifiedByNavigation?.LastName;

                                    GetDepartmentResponse.ModifiedById = GetDepartmentDBOBJ?.ModifiedByNavigation?.Id ?? 0;
                                }


                                GetDepartmentList.Add(GetDepartmentResponse);
                            }



                        }

                    }

                }
                response.DepartmentResponseList = GetDepartmentList;
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

        public BaseResponseWithID AddEditDepartment(DepartmentData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    long BranchID = 0;

                    if (request.BranchID != 0)
                    {

                        BranchID = (long)request.BranchID;
                        var DepartmentIDDB = _unitOfWork.Branches.Find(a => a.Id == BranchID);

                        if (DepartmentIDDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Branch Doesn't Exist";
                            Response.Errors.Add(error);
                        }

                    }

                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Branch Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var DepartmentDB = _unitOfWork.Departments.Find(a => a.Id == (int)request.ID);
                            if (DepartmentDB != null)
                            {
                                // Update
                                /*var DepartmentDBUpdate = _Context.proc_DepartmentUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                   Request.Description,
                                                                                                   Request.Active,
                                                                                                   DepartmentDB.CreatedBy,
                                                                                                   DepartmentDB.CreationDate,
                                                                                                   validation.userID,
                                                                                                   DateTime.Now,
                                                                                                   Request.BranchID
                                                                                                    );*/
                                DepartmentDB.Name = request.Name;
                                DepartmentDB.Description = request.Description;
                                DepartmentDB.Active = request.Active;
                                DepartmentDB.BranchId = request.BranchID;
                                DepartmentDB.ModifiedDate = DateTime.Now;
                                DepartmentDB.ModifiedBy = UserId;
                                var DepartmentDBUpdate = _unitOfWork.Complete();
                                if (DepartmentDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Department !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Department  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter DepartmentID = new ObjectParameter("ID", typeof(int));
                            var DepartmentInsert = _Context.proc_DepartmentInsert(DepartmentID,
                                                                                            Request.Name,
                                                                                            Request.Description,
                                                                                            Request.Active,
                                                                                            validation.userID,
                                                                                            DateTime.Now,
                                                                                            null,
                                                                                            null,
                                                                                            Request.BranchID
                                                                                           );*/
                            var dep = new Department()
                            {
                                Name = request.Name,
                                Description = request.Description,
                                Active = request.Active,
                                BranchId = request.BranchID,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = UserId,
                                CreatedBy = UserId,
                                CreationDate = DateTime.Now,
                            };
                            _unitOfWork.Departments.Add(dep);
                            var DepartmentInsert = _unitOfWork.Complete();

                            if (DepartmentInsert > 0)
                            {
                                var DepartmentIDInsert = long.Parse(dep.Id.ToString());
                                Response.ID = DepartmentIDInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Department !!";
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
        public GetBranchesResponse GetBranches()
        {
            GetBranchesResponse response = new GetBranchesResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetBranchesList = new List<BranchData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetBranchesDB = _unitOfWork.Branches.FindAll(a => (true), new[] { "CreatedByNavigation", "ModifiedByNavigation", "Governorate", "Country" });


                        if (GetBranchesDB != null && GetBranchesDB.Count() > 0)
                        {

                            foreach (var GetBranchDBOBJ in GetBranchesDB)
                            {
                                var GetBranchResponse = new BranchData();

                                GetBranchResponse.ID = (int)GetBranchDBOBJ.Id;

                                GetBranchResponse.Name = GetBranchDBOBJ.Name;

                                GetBranchResponse.Name = GetBranchDBOBJ.Name;

                                GetBranchResponse.Address = GetBranchDBOBJ.Address;

                                GetBranchResponse.Email = GetBranchDBOBJ.Email;

                                GetBranchResponse.Fax = GetBranchDBOBJ.Fax;

                                GetBranchResponse.Tel = GetBranchDBOBJ.Telephone;

                                GetBranchResponse.Description = GetBranchDBOBJ.Description;

                                GetBranchResponse.Active = GetBranchDBOBJ.Active;

                                GetBranchResponse.CountryID = GetBranchDBOBJ.CountryId;

                                GetBranchResponse.CountryName = GetBranchDBOBJ.Country?.Name;

                                GetBranchResponse.GovernorateID = GetBranchDBOBJ.GovernorateId;

                                GetBranchResponse.GovernorateName = GetBranchDBOBJ.Governorate.Name;

                                GetBranchResponse.CreatedById = GetBranchDBOBJ.CreatedBy;

                                GetBranchResponse.CreatedBy = GetBranchDBOBJ.CreatedByNavigation.FirstName + " " + GetBranchDBOBJ.CreatedByNavigation?.MiddleName + " " + GetBranchDBOBJ.CreatedByNavigation?.LastName;

                                if (GetBranchDBOBJ.ModifiedBy != null)
                                {
                                    GetBranchResponse.ModifiedById = GetBranchDBOBJ.ModifiedBy;

                                    GetBranchResponse.ModifiedBy = GetBranchDBOBJ.ModifiedByNavigation.FirstName + " " + GetBranchDBOBJ.ModifiedByNavigation?.MiddleName + " " + GetBranchDBOBJ.ModifiedByNavigation?.LastName;
                                }

                                GetBranchesList.Add(GetBranchResponse);
                            }



                        }

                    }

                }
                response.BranchResponseList = GetBranchesList;
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
        public BaseResponseWithID AddEditBranches(BranchData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.Address))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err26";
                        error.ErrorMSG = " Address Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    long CountryID = 0;

                    if (request.CountryID != 0)
                    {

                        CountryID = (long)request.CountryID;
                        var CountryIDDB = _unitOfWork.Countries.Find(a => a.Id == CountryID);

                        if (CountryIDDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Country Doesn't Exist";
                            Response.Errors.Add(error);
                        }

                    }

                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Country Is Mandatory";
                        Response.Errors.Add(error);
                    }



                    long GovernorateID = 0;

                    if (request.GovernorateID != 0)
                    {

                        GovernorateID = (long)request.GovernorateID;
                        var GovernorateIDDB = _unitOfWork.Governorates.Find(a => a.Id == GovernorateID);

                        if (GovernorateIDDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err27";
                            error.ErrorMSG = "Governorate Doesn't Exist";
                            Response.Errors.Add(error);
                        }

                    }

                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err28";
                        error.ErrorMSG = "Governorate Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var BranchDB = _unitOfWork.Branches.Find(a => a.Id == (int)request.ID);
                            if (BranchDB != null)
                            {
                                // Update
                                /*var BranchDBUpdate = _Context.proc_BranchUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                   Request.Description,
                                                                                                   Request.Address,
                                                                                                   Request.Tel,
                                                                                                   Request.Fax,
                                                                                                   Request.Email,
                                                                                                   Request.Active,
                                                                                                   BranchDB.CreatedBy,
                                                                                                   BranchDB.CreationDate,
                                                                                                   validation.userID,
                                                                                                   DateTime.Now,
                                                                                                   Request.CountryID,
                                                                                                   Request.GovernorateID
                                                                                                    );*/
                                BranchDB.Name = request.Name;
                                BranchDB.Description = request.Description;
                                BranchDB.Address = request.Address;
                                BranchDB.Telephone = request.Tel;
                                BranchDB.Fax = request.Fax;
                                BranchDB.Email = request.Email;
                                BranchDB.Active = request.Active;
                                BranchDB.CountryId = request.CountryID;
                                BranchDB.GovernorateId = request.GovernorateID;
                                BranchDB.ModifiedDate = DateTime.Now;
                                BranchDB.ModifiedBy = UserId;
                                var BranchDBUpdate = _unitOfWork.Complete();
                                if (BranchDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Branch !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Branch  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter BranchID = new ObjectParameter("ID", typeof(int));
                            var BranchIDInsert = _Context.proc_BranchInsert(BranchID,
                                                                                            Request.Name,
                                                                                            Request.Description,
                                                                                            Request.Address,
                                                                                            Request.Tel,
                                                                                            Request.Fax,
                                                                                            Request.Email,
                                                                                            Request.Active,
                                                                                            validation.userID,
                                                                                            DateTime.Now,
                                                                                            null,
                                                                                            null,
                                                                                            Request.CountryID,
                                                                                            Request.GovernorateID
                                                                                           );*/
                            var branch = new Branch()
                            {
                                Name = request.Name,
                                Description = request.Description,
                                Address = request.Address,
                                Telephone = request.Tel,
                                Fax = request.Fax,
                                Email = request.Email,
                                Active = request.Active,
                                CountryId = request.CountryID,
                                GovernorateId = request.GovernorateID,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = UserId,
                                CreationDate = DateTime.Now,
                                CreatedBy = UserId,
                            };
                            _unitOfWork.Branches.Add(branch);
                            var BranchIDInsert = _unitOfWork.Complete();

                            if (BranchIDInsert > 0)
                            {
                                var BranchInsert = long.Parse(branch.Id.ToString());
                                Response.ID = BranchInsert;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Branch !!";
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
        public async Task<GetTermsAndConditionsResponse> GetTermsAndConditions()
        {
            GetTermsAndConditionsResponse response = new GetTermsAndConditionsResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetTermsAndConditionsList = new List<TermsAndConditionsData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetTermsAndConditionsDB = await _unitOfWork.TermsAndConditions.FindAllAsync(a => (true), new[] { "CreatedByNavigation", "ModifiedByNavigation", "TermsCategory" });


                        if (GetTermsAndConditionsDB != null && GetTermsAndConditionsDB.Count() > 0)
                        {

                            foreach (var GetTermsAndConditionOBJ in GetTermsAndConditionsDB)
                            {
                                var GetTermsAndConditionResponse = new TermsAndConditionsData();

                                GetTermsAndConditionResponse.ID = (int)GetTermsAndConditionOBJ.Id;

                                GetTermsAndConditionResponse.Name = GetTermsAndConditionOBJ.Name;

                                GetTermsAndConditionResponse.Description = GetTermsAndConditionOBJ.Description;

                                GetTermsAndConditionResponse.Active = GetTermsAndConditionOBJ.Active;

                                GetTermsAndConditionResponse.TermsCategoryID = GetTermsAndConditionOBJ.TermsCategoryId;

                                GetTermsAndConditionResponse.TermsAndConditionsName = GetTermsAndConditionOBJ.TermsCategory.Name;

                                GetTermsAndConditionResponse.CreatedById = GetTermsAndConditionOBJ.CreatedBy;

                                GetTermsAndConditionResponse.CreatedBy = GetTermsAndConditionOBJ.CreatedByNavigation.FirstName + " " + GetTermsAndConditionOBJ.CreatedByNavigation?.MiddleName + " " + GetTermsAndConditionOBJ.CreatedByNavigation?.LastName;

                                if (GetTermsAndConditionOBJ.ModifiedBy != null)
                                {
                                    GetTermsAndConditionResponse.ModifiedById = GetTermsAndConditionOBJ.ModifiedBy;

                                    GetTermsAndConditionResponse.ModifiedBy = GetTermsAndConditionOBJ.ModifiedByNavigation.FirstName + " " + GetTermsAndConditionOBJ.ModifiedByNavigation?.MiddleName + " " + GetTermsAndConditionOBJ.ModifiedByNavigation?.LastName;
                                }

                                GetTermsAndConditionsList.Add(GetTermsAndConditionResponse);
                            }



                        }

                    }

                }
                response.TermsAndConditionsList = GetTermsAndConditionsList;
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
        public BaseResponseWithID AddEditTermsAndConditions(TermsAndConditionsData request, long userId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.Description))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err26";
                        error.ErrorMSG = " Description Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    var TermsCategory = _unitOfWork.TermsAndConditionsCategories.FindAll(a => a.Id == request.TermsCategoryID).FirstOrDefault();
                    if (TermsCategory == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err26";
                        error.ErrorMSG = " No TermsCategory with This ID";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        var TermsCategoryID = _unitOfWork.TermsAndConditionsCategories.GetById(request.TermsCategoryID)?.Id;
                        if (TermsCategoryID != 0)
                        {
                            if (request.ID != null && request.ID != 0)
                            {




                                var TermsAndConditionsDB = _unitOfWork.TermsAndConditions.Find(a => a.Id == (long)request.ID);
                                if (TermsAndConditionsDB != null)
                                {
                                    // Update
                                    /*var TermsAndConditionsUpdate = _Context.proc_TermsAndConditionsUpdate(Request.ID,
                                                                                                       Request.TermsCategoryID,
                                                                                                       Request.Name,
                                                                                                       Request.Description,
                                                                                                       Request.Active,
                                                                                                       TermsAndConditionsDB.CreationDate,
                                                                                                       TermsAndConditionsDB.CreatedBy,
                                                                                                       DateTime.Now,
                                                                                                       validation.userID
                                                                                                        );*/
                                    TermsAndConditionsDB.TermsCategoryId = request.TermsCategoryID;
                                    TermsAndConditionsDB.Name = request.Name;
                                    TermsAndConditionsDB.Description = request.Description;
                                    TermsAndConditionsDB.Active = request.Active;
                                    TermsAndConditionsDB.ModificationDate = DateTime.Now;
                                    TermsAndConditionsDB.ModifiedBy = userId;
                                    var TermsAndConditionsUpdate = _unitOfWork.Complete();
                                    if (TermsAndConditionsUpdate > 0)
                                    {
                                        Response.Result = true;
                                        Response.ID = request.ID ?? 0;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Failed To Update this Terms and Conditions !!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Terms and Conditions  Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                // Insert
                                /*ObjectParameter TermsAndConditionsID = new ObjectParameter("ID", typeof(int));
                                var TermsAndConditionsIDInsert = _Context.proc_TermsAndConditionsInsert(TermsAndConditionsID,
                                                                                                Request.TermsCategoryID,
                                                                                                Request.Name,
                                                                                                Request.Description,
                                                                                                Request.Active,
                                                                                                DateTime.Now,
                                                                                                validation.userID,
                                                                                                null,
                                                                                                null
                                                                                               );*/
                                var terms = new TermsAndCondition()
                                {
                                    TermsCategoryId = request.TermsCategoryID,
                                    Name = request.Name,
                                    Description = request.Description,
                                    Active = request.Active,
                                    ModificationDate = DateTime.Now,
                                    ModifiedBy = userId,
                                    CreationDate = DateTime.Now,
                                    CreatedBy = userId,
                                };
                                _unitOfWork.TermsAndConditions.Add(terms);
                                var TermsAndConditionsIDInsert = _unitOfWork.Complete();
                                if (TermsAndConditionsIDInsert > 0)
                                {
                                    var TermsAndConditionsInsert = long.Parse(terms.Id.ToString());
                                    Response.ID = TermsAndConditionsInsert;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Insert this Terms and Conditions !!";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Failed To Insert a valid Terms and Conditions Category !!";
                            Response.Errors.Add(error);
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
        public async Task<SelectDDLResponse> GetAreasList(int GovernorateId)
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var DDListDB = new List<SelectDDL>();
                if (response.Result)
                {

                    var Areas = await _unitOfWork.Areas.GetAllAsync();
                    if (GovernorateId != 0)
                    {
                        Areas = Areas.Where(x => x.GovernorateId == GovernorateId).ToList();
                    }
                    DDListDB = Areas.Select(c => new SelectDDL
                    {
                        ID = c.Id,
                        Name = c.Name
                    }).ToList();


                }

                response.DDLList = DDListDB;

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
        public BaseResponseWithID AddEditArea(AreaData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Area Name Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    //if (string.IsNullOrEmpty(Request.GovenorateID))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = "Governorate ID Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}

                    if (Response.Result)
                    {
                        int GovenorateRequestID = 0;




                        int AreaID = 0;

                        if (!string.IsNullOrEmpty(request.ID))
                        {



                            try
                            {
                                AreaID = int.Parse(request.ID.Split('-')[1]);
                            }
                            catch (Exception ex)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Please Insert A Valid Area ID ";
                                Response.Errors.Add(error);
                                return Response;

                            }




                        }
                        try
                        {
                            GovenorateRequestID = int.Parse(request.GovenorateID.Split('-')[1]);
                        }
                        catch (Exception)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Please Insert A Valid Governorate ID ";
                            Response.Errors.Add(error);
                            return Response;

                        }
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);


                        var AreaIDCheck = _unitOfWork.Areas.FindAll(x => x.Id == AreaID).FirstOrDefault();
                        var GovenorateIDCheck = _unitOfWork.Governorates.FindAll(x => x.Id == GovenorateRequestID).FirstOrDefault();



                        if (AreaID != 0)
                        {

                            if (AreaIDCheck != null)
                            {
                                if (GovenorateIDCheck != null)
                                {

                                    var AreaDB = _unitOfWork.Areas.GetById((long)AreaID);
                                    if (AreaDB != null)
                                    {
                                        // Update
                                        /*var AreaDBUpdate = _Context.proc_AreaUpdate(AreaID,
                                                                                                           Request.Name,
                                                                                                           Request.Description,
                                                                                                           GovenorateRequestID,
                                                                                                           AreaDB.CreationDate,
                                                                                                           AreaDB.CreatedBy,
                                                                                                           DateTime.Now,
                                                                                                           validation.userID
                                                                                                            );*/
                                        AreaDB.Name = request.Name;
                                        AreaDB.Description = request.Description;
                                        AreaDB.GovernorateId = GovenorateRequestID;
                                        AreaDB.ModifiedDate = DateTime.Now;
                                        AreaDB.ModifiedBy = UserId;
                                        var AreaDBUpdate = _unitOfWork.Complete();
                                        if (AreaDBUpdate > 0)
                                        {
                                            Response.Result = true;
                                            Response.ID = (long)AreaID;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Faild To Update this Area!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "This Area Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Governorate Doesn't Exist!!";
                                    Response.Errors.Add(error);

                                }

                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Area Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }

                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter AreaInsertedID = new ObjectParameter("ID", typeof(int));
                            var AreaInserted = _Context.proc_AreaInsert(AreaInsertedID,
                                                                                           Request.Name,
                                                                                           Request.Description,
                                                                                           GovenorateRequestID,
                                                                                            DateTime.Now,
                                                                                            validation.userID,
                                                                                            null,
                                                                                            null
                                                                                           );*/
                            var area = new Area()
                            {
                                Name = request.Name,
                                Description = request.Description,
                                GovernorateId = GovenorateRequestID,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = UserId,
                                CreatedBy = UserId,
                                CreationDate = DateTime.Now,
                            };
                            _unitOfWork.Areas.Add(area);
                            var AreaInserted = _unitOfWork.Complete();

                            if (AreaInserted > 0)
                            {
                                var AreaaID = long.Parse(area.Id.ToString());
                                Response.ID = AreaaID;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Area !!";
                                Response.Errors.Add(error);
                            }
                        }




                    }
                    ;


                    //var CountryID = int.Parse(Request.ID.Split('-')[1]);






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
        public BaseResponseWithID AddEditCountry(CountryData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Country Name Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        int countryID = 0;

                        if (!string.IsNullOrEmpty(request.ID))
                        {



                            try
                            {
                                countryID = int.Parse(request.ID.Split('-')[1]);
                            }
                            catch (Exception ex)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Please Insert A Valid Country ID ";
                                Response.Errors.Add(error);
                                return Response;

                            }

                        }

                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (countryID != 0)
                        {
                            var CountryDB = _unitOfWork.Countries.GetById(countryID);
                            if (CountryDB != null)
                            {
                                // Update
                                /*var CountryDBUpdate = _Context.proc_CountryUpdate(countryID,
                                                                                                   Request.Name,
                                                                                                   Request.Active,
                                                                                                   CountryDB.CreationDate,
                                                                                                   validation.userID,
                                                                                                   DateTime.Now,
                                                                                                   CountryDB.CreatedBy,
                                                                                                    null
                                                                                                    );*/
                                CountryDB.Name = request.Name;
                                CountryDB.Active = request.Active;
                                CountryDB.ModifiedDate = DateTime.Now;
                                CountryDB.ModifiedBy = UserId;
                                var CountryDBUpdate = _unitOfWork.Complete();
                                if (CountryDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = (long)countryID;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Country!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Country Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            /*ObjectParameter CountryInsertedID = new ObjectParameter("ID", typeof(int));
                            var CountryInserted = _Context.proc_CountryInsert(CountryInsertedID,
                                                                                            Request.Name,
                                                                                            Request.Active,
                                                                                            DateTime.Now,
                                                                                            null,
                                                                                            null,
                                                                                            validation.userID,
                                                                                            null
                                                                                           );*/
                            var country = new Country()
                            {
                                Name = request.Name,
                                Active = request.Active,
                                CreationDate = DateTime.Now,
                                CreatedBy = UserId,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = UserId,
                            };
                            _unitOfWork.Countries.Add(country);
                            var CountryInserted = _unitOfWork.Complete();

                            if (CountryInserted > 0)
                            {
                                var CountryID = long.Parse(country.Id.ToString());
                                Response.ID = CountryID;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Country!!";
                                Response.Errors.Add(error);
                            }
                        }




                    }
                    ;


                    //var CountryID = int.Parse(Request.ID.Split('-')[1]);






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
        public BaseResponseWithID AddEditGovernorate(GovernorateData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Governorate Name Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.CountryRequestedID))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Country ID is Mandatory";
                        Response.Errors.Add(error);
                    }



                    int CountryRequestID = 0;




                    int governorateID = 0;

                    if (!string.IsNullOrEmpty(request.CountryRequestedID))
                    {
                        try
                        {
                            CountryRequestID = int.Parse(request.CountryRequestedID.Split('-')[1]);
                        }
                        catch (Exception)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Please Insert A Valid Country ID ";
                            Response.Errors.Add(error);
                            return Response;

                        }
                    }

                    if (!string.IsNullOrEmpty(request.ID))
                    {



                        try
                        {
                            governorateID = int.Parse(request.ID.Split('-')[1]);
                        }
                        catch (Exception ex)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Please Insert A Valid Governorate ID ";
                            Response.Errors.Add(error);
                            return Response;

                        }




                    }

                    //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                    if (governorateID != 0)
                    {
                        var GovernorateDB = _unitOfWork.Governorates.GetById(governorateID);
                        if (GovernorateDB != null)
                        {
                            // Update
                            /*var GovernorateDBUpdate = _Context.proc_GovernorateUpdate(governorateID,
                                                                                               Request.Name,
                                                                                               Request.Active,
                                                                                               GovernorateDB.CreationDate,
                                                                                               validation.userID,
                                                                                               DateTime.Now,
                                                                                               CountryRequestID,
                                                                                               GovernorateDB.CreatedBy,
                                                                                                null
                                                                                                ); ;*/
                            GovernorateDB.Name = request.Name;
                            GovernorateDB.Active = request.Active;
                            GovernorateDB.CountryId = CountryRequestID;
                            GovernorateDB.ModifiedBy = UserId;
                            GovernorateDB.Modified = DateTime.Now;
                            var GovernorateDBUpdate = _unitOfWork.Complete();
                            if (GovernorateDBUpdate > 0)
                            {
                                Response.Result = true;
                                Response.ID = (long)governorateID;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Update this Country!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Country Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        // Insert
                        /*ObjectParameter GovernorateInsertedID = new ObjectParameter("ID", typeof(int));
                        var GovernorateInserted = _Context.proc_GovernorateInsert(GovernorateInsertedID,
                                                                                        Request.Name,
                                                                                        Request.Active,
                                                                                        DateTime.Now,
                                                                                        null,
                                                                                        null,
                                                                                        CountryRequestID,
                                                                                        validation.userID,
                                                                                        null
                                                                                       );*/
                        var governate = new Governorate()
                        {
                            Name = request.Name,
                            Active = request.Active,
                            CountryId = CountryRequestID,
                            ModifiedBy = UserId,
                            Modified = DateTime.Now,
                            CreatedBy = UserId,
                            CreationDate = DateTime.Now,
                        };
                        _unitOfWork.Governorates.Add(governate);
                        var GovernorateInserted = _unitOfWork.Complete();

                        if (GovernorateInserted > 0)
                        {
                            var GovernorateID = long.Parse(governate.Id.ToString());
                            Response.ID = GovernorateID;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Insert this Governorate!!";
                            Response.Errors.Add(error);
                        }
                    }







                    //var CountryID = int.Parse(Request.ID.Split('-')[1]);






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
        public async Task<GetCountryGovernorateAreaResponse> GetCountryGovernorateArea(bool allData = true)
        {
            GetCountryGovernorateAreaResponse response = new GetCountryGovernorateAreaResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetCountryGovernorateAreaResponseList = new List<TreeViewCountr>();
                /*bool allData = true;
                if (!string.IsNullOrEmpty(headers["allData"]) && bool.TryParse(headers["allData"], out allData))
                {
                    bool.TryParse(headers["allData"], out allData);
                }*/




                if (response.Result)
                {
                    var ClientAddressList = await _unitOfWork.ClientAddresses.FindAllAsync(x => x.Active == true);

                    var Countries = await _unitOfWork.Countries.GetAllAsync();

                    var TreeDtoObj = Countries.Select(c => new TreeViewCountr
                    {
                        id = "Country-" + c.Id.ToString(),
                        title = c.Name,
                        parentId = "",
                        CountOfClient = ClientAddressList.Where(x => x.CountryId == c.Id).Select(x => x.ClientId).Distinct().Count()
                    }).ToList();

                    var Govenorates = await _unitOfWork.Governorates.GetAllAsync();
                    var GovernorateDto = Govenorates.Select(c => new TreeViewCountr
                    {
                        id = "Governorate-" + c.Id.ToString(),
                        title = c.Name,
                        parentId = "Country-" + c.CountryId.ToString(),
                        CountOfClient = ClientAddressList.Where(x => x.GovernorateId == c.Id).Select(x => x.ClientId).Distinct().Count()
                    }).ToList();

                    TreeDtoObj.AddRange(GovernorateDto);


                    var Areas = await _unitOfWork.Areas.GetAllAsync();
                    var AreasDto = Areas.Select(c => new TreeViewCountr
                    {
                        id = "Area-" + c.Id.ToString(),
                        title = c.Name,
                        parentId = "Governorate-" + c.GovernorateId.ToString()
                        ,
                        CountOfClient = ClientAddressList.Where(x => x.AreaId == c.Id).Select(x => x.ClientId).Distinct().Count()
                    }).ToList();

                    TreeDtoObj.AddRange(AreasDto);
                    if (!allData)
                    {
                        TreeDtoObj = TreeDtoObj.Where(x => x.CountOfClient > 0).ToList();

                    }
                    var trees = Common.BuildTreeViews("", TreeDtoObj);
                    response.GetCountryGovernorateAreaResponseList = trees;
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
        public async Task<GetRoleResponse> GetRole()
        {
            GetRoleResponse response = new GetRoleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetRoleList = new List<RoleData>();

                if (response.Result)
                {

                    var GetRoleDB = await _unitOfWork.Roles.FindAllAsync(x => x.Active == true, new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                    if (GetRoleDB != null && GetRoleDB.Count() > 0)
                    {

                        foreach (var GetRoleOBJ in GetRoleDB)
                        {
                            var GetRoleResponse = new RoleData();

                            GetRoleResponse.ID = GetRoleOBJ.Id;

                            GetRoleResponse.Name = GetRoleOBJ.Name;

                            GetRoleResponse.Description = GetRoleOBJ.Description;

                            GetRoleResponse.Active = GetRoleOBJ.Active;

                            GetRoleResponse.ModifiedById = GetRoleOBJ.ModifiedBy;

                            GetRoleResponse.CreatedById = GetRoleOBJ.CreatedBy;

                            GetRoleResponse.CreatedBy = GetRoleOBJ.CreatedByNavigation.FirstName + " " + GetRoleOBJ.CreatedByNavigation?.MiddleName + " " + GetRoleOBJ.CreatedByNavigation?.LastName;

                            if (GetRoleOBJ.ModifiedBy != null)
                            {
                                GetRoleResponse.ModifiedById = GetRoleOBJ.ModifiedBy;

                                GetRoleResponse.ModifiedBy = GetRoleOBJ.ModifiedByNavigation.FirstName + " " + GetRoleOBJ.ModifiedByNavigation?.MiddleName + " " + GetRoleOBJ.ModifiedByNavigation?.LastName;
                            }
                            GetRoleList.OrderBy(x => x.Name);
                            GetRoleList.Add(GetRoleResponse);


                        }

                    }

                }


                response.RoleDataList = GetRoleList;
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
        public BaseResponseWithID AddGroupRole(AddGroupRoleData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    long GroupAssignedID = 0;

                    if (Response.Result)
                    {


                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);




                        /*ObjectParameter GroupID = new ObjectParameter("ID", typeof(long));
                        var GroupInserted = _Context.proc_GroupInsert(GroupID,
                                                                                           Request.Name,
                                                                                           Request.Description,
                                                                                           Request.Active,
                                                                                           validation.userID,
                                                                                           DateTime.Now,
                                                                                           validation.userID,
                                                                                           DateTime.Now
                                                                                       );*/
                        var Group = new Group()
                        {
                            Name = request.Name,
                            Description = request.Description,
                            Active = (bool)request.Active,
                            CreationDate = DateTime.Now,
                            CreatedBy = UserId,
                            ModifiedDate = DateTime.Now,
                            ModifiedBy = UserId
                        };
                        _unitOfWork.Groups.Add(Group);
                        var GroupInserted = _unitOfWork.Complete();

                        if (GroupInserted > 0)
                        {
                            GroupAssignedID = Group.Id;


                            if (request.UserID != null)
                            {
                                if (request.UserID.Count() > 0)
                                {
                                    foreach (var GroupUserIDOBJ in request.UserID)
                                    {
                                        /*ObjectParameter GroupUserID = new ObjectParameter("ID", typeof(long));
                                        var GroupUserInserted = _Context.proc_Group_UserInsert(GroupUserID,
                                                                                                           GroupAssignedID,
                                                                                                           GroupUserIDOBJ,
                                                                                                           validation.userID,
                                                                                                           DateTime.Now,
                                                                                                           true
                                                                                                       );*/
                                        var GroupUser = new GroupUser()
                                        {
                                            GroupId = GroupAssignedID,
                                            UserId = GroupUserIDOBJ,
                                            CreationDate = DateTime.Now,
                                            CreatedBy = UserId,
                                            Active = true,
                                        };
                                        _unitOfWork.GroupUsers.Add(GroupUser);
                                        var GroupUserInserted = _unitOfWork.Complete();
                                        if (GroupUserInserted > 0)
                                        {
                                            var GroupRoleInsertedID = GroupUser.Id;

                                            if (request.RoleID != null)
                                            {
                                                if (request.RoleID.Count() > 0)
                                                {
                                                    foreach (var RoleIDOBJ in request.RoleID)
                                                    {
                                                        /*  ObjectParameter GroupRoleID = new ObjectParameter("ID", typeof(long));
                                                          var GroupRoleInserted = _Context.proc_GroupRoleInsert(GroupRoleID,
                                                                                                                             RoleIDOBJ,
                                                                                                                             GroupAssignedID,
                                                                                                                             validation.userID,
                                                                                                                             DateTime.Now

                                                                                                                         );
  */
                                                        var GroupRole = new GroupRole()
                                                        {
                                                            RoleId = RoleIDOBJ,
                                                            GroupId = GroupAssignedID,
                                                            CreatedBy = UserId,
                                                            CreationDate = DateTime.Now,
                                                        };
                                                        _unitOfWork.GroupRoles.Add(GroupRole);
                                                        var GroupRoleInserted = _unitOfWork.Complete();


                                                        if (GroupRoleInserted > 0)
                                                        {
                                                            var GroupRoleInsertion = GroupRole.Id;

                                                        }

                                                        else
                                                        {
                                                            Response.Result = false;
                                                            Error error = new Error();
                                                            error.ErrorCode = "Err25";
                                                            error.ErrorMSG = "Faild To Insert this Group Role !!";
                                                            Response.Errors.Add(error);
                                                        }
                                                    }
                                                }
                                            }
                                            var GroupDb = _unitOfWork.Groups.FindAll(a => a.Id == request.GroupID, includes: new[] { "GroupRoles", "GroupUsers" }).FirstOrDefault();

                                            var roles = _unitOfWork.UserRoles.FindAll(a => GroupDb.GroupUsers.Select(b => b.UserId).Contains(a.UserId)).ToList();

                                            foreach (var user in GroupDb.GroupUsers)
                                            {
                                                foreach (var a in GroupDb.GroupRoles)
                                                {
                                                    if (roles.Where(x => x.UserId == user.UserId && x.RoleId == a.RoleId).FirstOrDefault() == null)
                                                    {

                                                        _unitOfWork.UserRoles.Add(new UserRole() { RoleId = a.RoleId, UserId = user.UserId, CreatedBy = UserId, CreationDate = DateTime.Now });
                                                        _unitOfWork.Complete();

                                                        var message = $"Add User Role\nadd Role Id {a.RoleId} To User Id {user.UserId}";

                                                        _logService.AddLogError("AddGroupRole", message, validation.userID, validation.CompanyName);
                                                    }
                                                }
                                                ;
                                            }


                                        }

                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Faild To Insert this Group Role !!";
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
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Insert this Group !!";
                            Response.Errors.Add(error);
                        }




                        //     // Insert


                        //}
                    }
                    ;




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
        public BaseResponseWithID EditGroupRole(EditGroupData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (request.GroupID == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Group ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    int counter = 0;
                    int GroupRolecounter = 0;

                    var GroupUserDB = _unitOfWork.GroupUsers.FindAll(x => x.GroupId == request.GroupID).ToList();
                    var GroupRoleDB = _unitOfWork.GroupRoles.FindAll(x => x.GroupId == request.GroupID).ToList();

                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.GroupID != null && request.GroupID != 0)
                        {
                            var GroupDB = _unitOfWork.Groups.GetById(request.GroupID ?? 0);
                            if (GroupDB != null)
                            {
                                GroupDB.Name = request.Name;
                                GroupDB.Description = request.Description;
                                GroupDB.Active = (bool)request.Active;
                                GroupDB.ModifiedBy = UserId;
                                GroupDB.ModifiedDate = DateTime.Now;
                                var GroupUpdate = _unitOfWork.Complete();
                                if (GroupUpdate > 0)
                                {
                                    Response.Result = true;

                                    if (request.UserID != null)
                                    {

                                        if (GroupUserDB.Count() < request.UserID.Count())
                                        {
                                            foreach (var GroupUser in request.UserID)
                                            {

                                                if (!GroupUserDB.Select(x => x.UserId).ToList().Contains(GroupUser))
                                                {

                                                    /*ObjectParameter GroupUserID = new ObjectParameter("ID", typeof(long));
                                                    var GroupUserInsertion = _Context.proc_Group_UserInsert(GroupUserID,
                                                       Request.GroupID,
                                                       (int)GroupUser,
                                                       validation.userID,
                                                       DateTime.Now.Date,
                                                       true
                                                        );*/
                                                    var Groupuser = new GroupUser()
                                                    {
                                                        GroupId = (long)request.GroupID,
                                                        UserId = (int)GroupUser,
                                                        //HrUserId = GroupUser,
                                                        CreatedBy = UserId,
                                                        CreationDate = DateTime.Now,
                                                    };
                                                    _unitOfWork.GroupUsers.Add(Groupuser);
                                                    _unitOfWork.Complete();
                                                }
                                            }


                                        }
                                        GroupUserDB = _unitOfWork.GroupUsers.FindAll(x => x.GroupId == request.GroupID).ToList();
                                        if (GroupUserDB.Count() > request.UserID.Count())
                                        {


                                            var GroupUserListDB = _unitOfWork.GroupUsers.FindAll(x => x.GroupId == request.GroupID).ToList();


                                            if (request.UserID != null)
                                            {
                                                if (request.UserID.Count() >= 0)
                                                {

                                                    var UsersIDSForDelete = GroupUserDB.Where(x => x.UserId != null).Select(x => (long)x.UserId).ToList().Except(request.UserID.ToList());

                                                    if (UsersIDSForDelete.Count() > 0)
                                                    {

                                                        foreach (var deletedUserId in UsersIDSForDelete)
                                                        {
                                                            var deletedId = GroupUserListDB.Where(x => x.UserId == deletedUserId).FirstOrDefault();
                                                            _unitOfWork.GroupUsers.Delete(deletedId);
                                                        }
                                                    }

                                                }
                                            }





                                        }
                                        GroupUserDB = _unitOfWork.GroupUsers.FindAll(x => x.GroupId == request.GroupID).ToList();
                                        if (GroupUserDB.Count() == request.UserID.Count())
                                        {
                                            foreach (var GroupUser in request.UserID)
                                            {
                                                var ID = GroupUserDB[counter].Id;

                                                /*var GroupUserUpdate = _Context.proc_Group_UserUpdate(ID,
                                                     Request.GroupID,
                                                     (int)GroupUser,
                                                     validation.userID,
                                                     DateTime.Now.Date,
                                                     true


                                              );*/
                                                GroupUserDB[counter].GroupId = (long)request.GroupID;
                                                GroupUserDB[counter].UserId = GroupUser;
                                                GroupUserDB[counter].Active = true;
                                                _unitOfWork.Complete();
                                                counter++;
                                            }
                                        }
                                        ;
                                    }



                                    counter = 0;

                                    if (request.RoleID != null)
                                    {
                                        //Group Role Count < Requested Role ID
                                        if (GroupRoleDB.Count() < request.RoleID.Count())
                                        {
                                            foreach (var GroupRole in request.RoleID)
                                            {

                                                if (!GroupRoleDB.Select(x => x.RoleId).ToList().Contains(GroupRole))
                                                {

                                                    /*ObjectParameter GroupRoleID = new ObjectParameter("ID", typeof(long));
                                                    var GroupRoleInsertion = _Context.proc_GroupRoleInsert(GroupRoleID,
                                                       (int)GroupRole,
                                                       Request.GroupID,
                                                       validation.userID,
                                                       DateTime.Now.Date

                                                        );*/
                                                    var Grouperole = new GroupRole()
                                                    {
                                                        RoleId = (int)GroupRole,
                                                        GroupId = (long)request.GroupID,
                                                        CreatedBy = UserId,
                                                        CreationDate = DateTime.Now
                                                    };
                                                    _unitOfWork.GroupRoles.Add(Grouperole); _unitOfWork.Complete();
                                                }
                                            }
                                        }
                                        //END

                                        //Group Role Count > Requested Role ID
                                        GroupRoleDB = _unitOfWork.GroupRoles.FindAll(x => x.GroupId == (long)request.GroupID).ToList();
                                        if (GroupRoleDB.Count() > request.RoleID.Count())
                                        {


                                            var GroupRoleListDB = _unitOfWork.GroupRoles.FindAll(x => x.GroupId == request.GroupID).ToList();


                                            if (request.RoleID != null)
                                            {
                                                if (request.RoleID.Count() > 0)
                                                {
                                                    //var GroupUserList = Request.EditGroupUser.Where(x => x.GroupID != null).Select(x => x.GroupID).ToList();

                                                    //var DeleteIds = GroupUserListDB.Where(db => GroupUserList.All(nw => nw != db)).ToArray();

                                                    var RolesIDSForDelete = GroupRoleDB.Select(x => x.RoleId).ToList().Except(request.RoleID.ToList());

                                                    if (RolesIDSForDelete.Count() > 0)
                                                    {

                                                        foreach (var deletedRoleId in RolesIDSForDelete)
                                                        {
                                                            var deletedId = GroupRoleListDB.Where(x => x.RoleId == deletedRoleId).FirstOrDefault();
                                                            _unitOfWork.GroupRoles.Delete(deletedId);
                                                            _unitOfWork.Complete();
                                                        }
                                                    }

                                                }
                                            }



                                        }
                                        //End


                                        //Group Role Count == Requested Role ID
                                        GroupRoleDB = _unitOfWork.GroupRoles.FindAll(x => x.GroupId == (long)request.GroupID).ToList();
                                        if (GroupRoleDB.Count() == request.RoleID.Count())
                                        {
                                            foreach (var GroupRole in request.RoleID)
                                            {
                                                var RoleID = GroupRoleDB[counter].Id;

                                                /*var GroupRoleUpdate = _Context.proc_GroupRoleUpdate(RoleID,
                                                      (int)GroupRole,
                                                     Request.GroupID,
                                                     validation.userID,
                                                     DateTime.Now.Date

                                              );*/
                                                GroupRoleDB[counter].RoleId = (int)GroupRole;
                                                GroupRoleDB[counter].GroupId = (long)request.GroupID;
                                                _unitOfWork.Complete();
                                                counter++;
                                            }
                                        }
                                        ;
                                        //End


                                    }
                                    var GroupDb = _unitOfWork.Groups.FindAll(a => a.Id == request.GroupID, includes: new[] { "GroupRoles", "GroupUsers" }).FirstOrDefault();

                                    var roles = _unitOfWork.UserRoles.FindAll(a => GroupDb.GroupUsers.Select(b => b.UserId).Contains(a.UserId)).ToList();

                                    foreach (var user in GroupDb.GroupUsers)
                                    {
                                        if (GroupDb.GroupRoles.Count > 0)
                                        {
                                            foreach (var a in GroupDb.GroupRoles)
                                            {
                                                if (roles.Where(x => x.UserId == user.UserId && x.RoleId == a.RoleId).FirstOrDefault() == null)
                                                {

                                                    _unitOfWork.UserRoles.Add(new UserRole() { RoleId = a.RoleId, UserId = user.UserId, CreatedBy = UserId, CreationDate = DateTime.Now });
                                                    _unitOfWork.Complete();
                                                    var message = $"Add User Role\nadd Role Id {a.RoleId} To User Id {user.UserId}";

                                                    _logService.AddLogError("EditGroupRole", message, validation.userID, validation.CompanyName);
                                                }
                                            }
                                            ;
                                        }
                                    }

                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Group Role!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Group Role Doesn't Exist!!";
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
        public BaseResponseWithID AddEditGroup(GroupData request, long userID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {


                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);


                        if (request.ID != 0 && request.ID != null)
                        {
                            var GroupDB = _unitOfWork.Groups.GetById((long)request.ID);
                            if (GroupDB != null)
                            {
                                // Update
                                /*var GroupDBUpdate = _Context.proc_GroupUpdate(Request.ID,
                                                                                                   Request.Name,
                                                                                                   Request.Description,
                                                                                                   Request.Active,
                                                                                                   GroupDB.CreatedBy,
                                                                                                   GroupDB.CreationDate,
                                                                                                   validation.userID,
                                                                                                   DateTime.Now

                                                                                                    );*/
                                GroupDB.Name = request.Name;
                                GroupDB.Description = request.Description;
                                GroupDB.Active = request.Active;
                                GroupDB.ModifiedBy = userID;
                                GroupDB.ModifiedDate = DateTime.Now;
                                var GroupDBUpdate = _unitOfWork.Complete();
                                if (GroupDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = (long)request.ID;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Group!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Group Doesn't Exist!!";
                                Response.Errors.Add(error);


                            }
                        }
                        else
                        {


                            // Insert
                            /*ObjectParameter GroupID = new ObjectParameter("ID", typeof(int));
                            var GroupInserted = _Context.proc_GroupInsert(GroupID,
                                                                                               Request.Name,
                                                                                               Request.Description,
                                                                                               Request.Active,
                                                                                               validation.userID,
                                                                                               DateTime.Now,
                                                                                             validation.userID,
                                                                                               DateTime.Now

                                                                                           );*/
                            var group = new Group()
                            {
                                Name = request.Name,
                                Description = request.Description,
                                Active = request.Active,
                                CreatedBy = userID,
                                CreationDate = DateTime.Now,
                                ModifiedBy = userID,
                                ModifiedDate = DateTime.Now,
                            };
                            _unitOfWork.Groups.Add(group);
                            var GroupInserted = _unitOfWork.Complete();
                            if (GroupInserted > 0)
                            {
                                Response.ID = (long)group.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Group !!";
                                Response.Errors.Add(error);
                            }







                        }
                    }
                    ;




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
        public async Task<GetGroupRoleResponse> GetGroupDetails(long GroupID = 0)
        {
            GetGroupRoleResponse response = new GetGroupRoleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetGroupList = new GroupRoleData();
                var GetGroupUserList = new List<ViewGroupUser>();
                var GetGroupRoleList = new List<ViewGroupRole>();

                /*long GroupID = 0;
                if (!string.IsNullOrEmpty(headers["GroupID"]) && long.TryParse(headers["GroupID"], out GroupID))
                {
                    long.TryParse(headers["GroupID"], out GroupID);
                }*/

                if (response.Result)
                {

                    var GetGroupDB = await _unitOfWork.Groups.FindAsync(x => x.Id == GroupID);
                    var GetGroupUserDB = await _unitOfWork.GroupUsers.FindAllAsync(x => x.GroupId == GroupID, new[] { "User" });
                    var GetGroupRoleDB = await _unitOfWork.GroupRoles.FindAllAsync(x => x.GroupId == GroupID, new[] { "Role" });



                    if (GetGroupDB != null)
                    {


                        var GetGroupResponse = new GroupRoleData();


                        //GetGroupRoleResponse.ID  = GetGroupRoleOBJ.ID;

                        GetGroupResponse.GroupID = (int)GetGroupDB.Id;
                        GetGroupResponse.GroupName = GetGroupDB.Name;
                        GetGroupResponse.Description = GetGroupDB.Description;
                        GetGroupResponse.Active = GetGroupDB.Active;


                        //GetGroupRoleResponse.GroupID  = (int)GetGroupRoleOBJ.GroupID;

                        //GetGroupRoleResponse.Name  = GetGroupRoleOBJ.FirstName + GetGroupRoleOBJ.LastName;

                        //GetGroupRoleResponse.RoleName  = Common.GetRoleName(GetGroupRoleOBJ.RoleID);

                        //GetGroupRoleResponse.GroupName  = Common.GetGroupName(GetGroupRoleOBJ.GroupID);

                        //GetGroupRoleResponse.UserID  = (int)GetGroupRoleOBJ.UserID;



                        foreach (var GroupUserOBJ in GetGroupUserDB)
                        {
                            var GroupUserResponse = new ViewGroupUser();

                            GroupUserResponse.ID = (int)GroupUserOBJ.Id;

                            GroupUserResponse.GroupID = (int)GroupUserOBJ.GroupId;

                            GroupUserResponse.UserID = (int)GroupUserOBJ.UserId;

                            GroupUserResponse.UserName = GroupUserOBJ.User.FirstName + " " + GroupUserOBJ.User?.MiddleName + " " + GroupUserOBJ.User.LastName;





                            GetGroupUserList.Add(GroupUserResponse);
                        }
                        GetGroupResponse.GroupUser = GetGroupUserList;





                        foreach (var GroupRoleOBJ in GetGroupRoleDB)
                        {
                            var GroupRoleResponse = new ViewGroupRole();

                            GroupRoleResponse.ID = (int)GroupRoleOBJ.Id;

                            GroupRoleResponse.GroupID = (int)GroupRoleOBJ.GroupId;

                            GroupRoleResponse.RoleID = (int)GroupRoleOBJ.RoleId;

                            GroupRoleResponse.RoleName = GroupRoleOBJ.Role.Name;

                            GetGroupRoleList.Add(GroupRoleResponse);
                        }
                        GetGroupResponse.GroupRole = GetGroupRoleList;

                        response.GroupRoleObj = GetGroupResponse;
                    }

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
        public GetGenderResponse GetGender()
        {
            GetGenderResponse response = new GetGenderResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetGenderResponseList = new List<GenderData>();




                if (response.Result)
                {

                    var GetGenderDB = _unitOfWork.Genders.GetAll();


                    if (GetGenderDB != null && GetGenderDB.Count() > 0)
                    {

                        foreach (var GetGenderOBJ in GetGenderDB)
                        {
                            var GetGenderResponse = new GenderData();

                            GetGenderResponse.Id = (int)GetGenderOBJ.Id;

                            GetGenderResponse.Name = GetGenderOBJ.Name;




                            GetGenderResponseList.Add(GetGenderResponse);
                        }



                    }

                }


                response.GenderList = GetGenderResponseList;
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
        public async Task<GetImportantDateResponse> GetImportantDateList(int ImpDateId = 0)
        {
            GetImportantDateResponse response = new GetImportantDateResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string baseURL = MyConfig.GetValue<string>("AppSettings:baseURL");



            try
            {

                /*                int ImpDateId = 0;
                                if (!string.IsNullOrEmpty(headers["ImpDateId"]) && int.TryParse(headers["ImpDateId"], out ImpDateId))
                                {
                                    int.TryParse(headers["ImpDateId"], out ImpDateId);
                                }*/
                var ImportantDateModelList = new List<ImportantDateModel>();
                if (response.Result)
                {

                    var ImpDateListDB = await _unitOfWork.ImportantDates.GetAllAsync();
                    if (ImpDateId != 0)
                    {
                        ImpDateListDB = ImpDateListDB.Where(x => x.Id == ImpDateId).ToList();
                    }
                    ImportantDateModelList = ImpDateListDB.Select(item => new ImportantDateModel
                    {
                        ID = item.Id,
                        ReminderDate = item.ReminderDate.ToShortDateString(),
                        Comment = item.Comment,
                        Active = item.Active,
                        Status = item.Status,
                        Type = item.Type,
                        FilePath = baseURL + item.FilePath
                    }).ToList();
                }
                response.ImportantDateList = ImportantDateModelList;
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

        public async Task<BaseResponseWithID> AddImportantDate(AddImportantDateRequest request, long UserId, string CompanyName)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    DateTime ReminderDate = DateTime.Now;
                    if (string.IsNullOrEmpty(request.ReminderDate) || !DateTime.TryParse(request.ReminderDate, out ReminderDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err888";
                        error.ErrorMSG = "Invalid Reminder Date";
                        Response.Errors.Add(error);
                    }
                    if (request.Status != "Open" && request.Status != "Closed")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err889";
                        error.ErrorMSG = "Invalid Status (Open Or Closed)";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {

                        if (request.ID == 0)
                        {

                            var ImpDate = new ImportantDate();
                            ImpDate.Active = true;
                            ImpDate.Status = request.Status.Trim();
                            ImpDate.ReminderDate = ReminderDate;
                            ImpDate.Comment = request.Comment;
                            ImpDate.Type = request.Type;
                            ImpDate.CreatedBy = UserId;
                            ImpDate.FileName = request.FileName;
                            ImpDate.Fileextention = request.FileExtension;
                            _unitOfWork.ImportantDates.Add(ImpDate);
                            var Res = _unitOfWork.Complete();
                            if (Res > 0)
                            {
                                //var CompanyName = Request.Headers["CompanyName"].ToString().ToLower();
                                if (!string.IsNullOrWhiteSpace(request.FileName) && !string.IsNullOrWhiteSpace(request.FileContent) && !string.IsNullOrWhiteSpace(request.FileExtension))
                                {
                                    ImpDate.FilePath = await Common.SaveFileAsync("/Attachments/" + CompanyName + "/ImportantDate/" + ImpDate.Id + "/", request.FileContent, request.FileName, request.FileExtension, _host);
                                }
                            }


                            request.ID = ImpDate.Id;
                        }
                        else
                        {
                            var ImpDateObjDb = await _unitOfWork.ImportantDates.FindAsync(x => x.Id == request.ID);
                            if (ImpDateObjDb != null)
                            {
                                ImpDateObjDb.Active = true;
                                ImpDateObjDb.Status = request.Status.Trim();
                                ImpDateObjDb.ReminderDate = ReminderDate;
                                ImpDateObjDb.Comment = request.Comment;
                                ImpDateObjDb.Type = request.Type;
                                ImpDateObjDb.Fileextention = request.FileExtension;
                                ImpDateObjDb.FileName = request.FileName;
                                if (!string.IsNullOrWhiteSpace(request.FileContent) && !string.IsNullOrWhiteSpace(request.FileName) && !string.IsNullOrWhiteSpace(request.FileExtension))
                                {
                                    if (Directory.Exists(ImpDateObjDb.FilePath))
                                    {
                                        Directory.Delete(ImpDateObjDb.FilePath);
                                    }
                                    //var CompanyName = Request.Headers["CompanyName"].ToString().ToLower();
                                    ImpDateObjDb.FilePath = await Common.SaveFileAsync("Attachments/" + CompanyName + "/ImportantDate/" + ImpDateObjDb.Id + "/", request.FileContent, request.FileName, request.FileExtension, _host);
                                }

                            }
                        }
                        _unitOfWork.Complete();

                        Response.ID = request.ID;
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

        public GetRoleModuleResponse GetRoleModule(long ModuleID = 0)
        {
            GetRoleModuleResponse response = new GetRoleModuleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetRoleModuleList = new List<RoleModuleData>();


                /*long ModuleID = 0;
                if (!string.IsNullOrEmpty(headers["ModuleID"]) && long.TryParse(headers["ModuleID"], out ModuleID))
                {
                    long.TryParse(headers["ModuleID"], out ModuleID);
                }*/

                if (response.Result)
                {

                    var GetRoleModuleDB = _unitOfWork.RoleModules.FindAll(a => (true), new[] { "Role" });
                    if (ModuleID != 0)
                    {
                        GetRoleModuleDB = _unitOfWork.RoleModules.FindAll(x => x.ModuleId == ModuleID);
                    }

                    if (GetRoleModuleDB != null)
                    {

                        foreach (var GetRoleModuleOBJ in GetRoleModuleDB)
                        {
                            var GetRoleModuleResponse = new RoleModuleData();


                            GetRoleModuleResponse.ID = (int)GetRoleModuleOBJ.Id;
                            GetRoleModuleResponse.RoleID = GetRoleModuleOBJ.RoleId;

                            GetRoleModuleResponse.ModuleID = (int)GetRoleModuleOBJ.ModuleId;

                            //GetRoleModuleResponse.Name = GetRoleModuleOBJ.FirstName + GetRoleModuleOBJ.LastName;

                            GetRoleModuleResponse.RoleName = GetRoleModuleOBJ.Role.Name;

                            GetRoleModuleResponse.ModuleName = _unitOfWork.Modules.GetById(GetRoleModuleOBJ.ModuleId)?.Name;

                            //GetRoleModuleResponse.Active = GetRoleModuleOBJ.A;



                            GetRoleModuleList.Add(GetRoleModuleResponse);
                        }



                    }

                }


                response.RoleModuleList = GetRoleModuleList;
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

        public BaseResponseWithID AddEditRoleModule(RoleModuleData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Response.Result)
                    {


                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);


                        if (request.ID != 0 && request.ID != null)
                        {




                            var RoleModuleDB = _unitOfWork.RoleModules.GetById(request.ID);
                            if (RoleModuleDB != null)
                            {
                                // Update
                                /*var RoleModuleDBUpdate = _Context.proc_RoleModuleUpdate(Request.ID,
                                                                                                   Request.RoleID,
                                                                                                   Request.ModuleID,
                                                                                                   RoleModuleDB.CreatedBy,
                                                                                                   RoleModuleDB.CreationDate
                                                                                                    );*/
                                RoleModuleDB.RoleId = request.RoleID;
                                RoleModuleDB.ModuleId = request.ModuleID;
                                var RoleModuleDBUpdate = _unitOfWork.Complete();
                                if (RoleModuleDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = (long)request.ID;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Role Module!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Role Module Doesn't Exist!!";
                                Response.Errors.Add(error);


                            }
                        }
                        else
                        {


                            // Insert
                            /*ObjectParameter RoleModuleID = new ObjectParameter("ID", typeof(int));
                            var RoleModuleInserted = _Context.proc_RoleModuleInsert(RoleModuleID,
                                                                                               Request.RoleID,
                                                                                               Request.ModuleID,
                                                                                               validation.userID,
                                                                                               DateTime.Now

                                                                                           );*/

                            var roleModule = new RoleModule()
                            {
                                RoleId = request.RoleID,
                                ModuleId = request.ModuleID,
                                CreatedBy = UserId,
                                CreationDate = DateTime.Now,
                            };
                            _unitOfWork.RoleModules.Add(roleModule);

                            var RoleModuleInserted = _unitOfWork.Complete();

                            if (RoleModuleInserted > 0)
                            {
                                var RoleModuleInsertedID = (long)roleModule.Id;

                            }

                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Role Module !!";
                                Response.Errors.Add(error);
                            }


                        }
                    }
                    ;




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

        public async Task<GetModuleResponse> GetModule()
        {
            GetModuleResponse response = new GetModuleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetModuleList = new List<ModuleData>();




                if (response.Result)
                {

                    var GetModuleDB = await _unitOfWork.Modules.GetAllAsync();

                    if (GetModuleDB != null)
                    {

                        foreach (var GetModuleOBJ in GetModuleDB)
                        {
                            var GetModuleResponse = new ModuleData();



                            GetModuleResponse.ID = (int)GetModuleOBJ.Id;

                            GetModuleResponse.Name = GetModuleOBJ.Name;
                            GetModuleResponse.Description = GetModuleOBJ.Description;
                            GetModuleResponse.Active = GetModuleOBJ.Active;





                            GetModuleList.Add(GetModuleResponse);
                        }



                    }

                }


                response.ModuleList = GetModuleList;
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

        public async Task<GetTaxResponse> GetTax()
        {
            GetTaxResponse response = new GetTaxResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetTaxResponseList = new List<TaxData>();
                if (response.Result)
                {

                    var GetTaxDB = await _unitOfWork.Taxes.FindAllAsync(a => (true), new[] { "CreatedByNavigation", "ModifiedByNavigation" });


                    if (GetTaxDB != null && GetTaxDB.Count() > 0)
                    {

                        foreach (var GetTaxOBJ in GetTaxDB)
                        {
                            var GetTaxResponse = new TaxData();

                            GetTaxResponse.ID = (int)GetTaxOBJ.Id;

                            GetTaxResponse.TaxName = GetTaxOBJ.TaxName;

                            GetTaxResponse.TaxType = GetTaxOBJ.TaxType;

                            GetTaxResponse.TaxPercentage = GetTaxOBJ.TaxPercentage;

                            GetTaxResponse.TaxCode = GetTaxOBJ.TaxCode;

                            GetTaxResponse.IsPercentage = GetTaxOBJ.IsPercentage;

                            GetTaxResponse.Description = GetTaxOBJ.Description;

                            GetTaxResponse.Active = GetTaxOBJ.Active;

                            GetTaxResponse.CreatedById = GetTaxOBJ.CreatedBy;

                            GetTaxResponse.CreatedBy = GetTaxOBJ.CreatedByNavigation.FirstName + " " + GetTaxOBJ.CreatedByNavigation?.MiddleName + " " + GetTaxOBJ.CreatedByNavigation?.LastName;

                            if (GetTaxOBJ.ModifiedBy != null)
                            {
                                GetTaxResponse.ModifiedById = GetTaxOBJ.ModifiedBy;

                                GetTaxResponse.ModifiedBy = GetTaxOBJ.ModifiedByNavigation.FirstName + " " + GetTaxOBJ.ModifiedByNavigation?.MiddleName + " " + GetTaxOBJ.ModifiedByNavigation?.LastName;
                            }

                            GetTaxResponseList.Add(GetTaxResponse);
                        }



                    }

                }


                response.TaxList = GetTaxResponseList;
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

        public BaseResponseWithID AddEditTax(TaxData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (request.TaxName == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert Tax name.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Response.Result)
                    {


                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);


                        if (request.ID != 0 && request.ID != null)
                        {
                            var TaxDB = _unitOfWork.Taxes.GetById((long)request.ID);
                            if (TaxDB != null)
                            {
                                // Update
                                /*var TaxDBpdate = _Context.proc_TaxUpdate(Request.ID,
                                                                                                   Request.TaxName,
                                                                                                   Request.TaxCode,
                                                                                                   Request.TaxType,
                                                                                                   Request.TaxPercentage,
                                                                                                   Request.Description,
                                                                                                   Request.Active,
                                                                                                   DateTime.Now,
                                                                                                   validation.userID,
                                                                                                    DateTime.Now,
                                                                                                   validation.userID,
                                                                                                    Request.IsPercentage
                                                                                                    );*/
                                TaxDB.TaxName = request.TaxName;
                                TaxDB.TaxCode = request.TaxCode;
                                TaxDB.TaxType = request.TaxType;
                                TaxDB.TaxPercentage = request.TaxPercentage;
                                TaxDB.Description = request.Description;
                                TaxDB.Active = request.Active;
                                TaxDB.ModifiedBy = UserId;
                                TaxDB.ModifiedDate = DateTime.Now;
                                var TaxDBpdate = _unitOfWork.Complete();
                                if (TaxDBpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = (long)request.ID;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Tax!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Tax Doesn't Exist!!";
                                Response.Errors.Add(error);


                            }
                        }
                        else
                        {


                            // Insert
                            /*ObjectParameter TaxID = new ObjectParameter("ID", typeof(int));
                            var TaxInserted = _Context.proc_TaxInsert(TaxID,
                                                                                                  Request.TaxName,
                                                                                                   Request.TaxCode,
                                                                                                   Request.TaxType,
                                                                                                   Request.TaxPercentage,
                                                                                                   Request.Description,
                                                                                                   Request.Active,
                                                                                                   DateTime.Now,
                                                                                                   validation.userID,
                                                                                                    DateTime.Now,
                                                                                                   validation.userID,
                                                                                                    Request.IsPercentage

                                                                                           );*/

                            var Tax = new Tax()
                            {
                                TaxName = request.TaxName,
                                TaxCode = request.TaxCode,
                                TaxType = request.TaxType,
                                TaxPercentage = request.TaxPercentage,
                                Description = request.Description,
                                Active = request.Active,
                                CreatedBy = UserId,
                                CreationDate = DateTime.Now,
                                ModifiedBy = UserId,
                                ModifiedDate = DateTime.Now
                            };
                            _unitOfWork.Taxes.Add(Tax);
                            var TaxInserted = _unitOfWork.Complete();


                            if (TaxInserted > 0)
                            {
                                var EInvoiceCompanyActivityInsertedID = (long)Tax.Id;

                            }

                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Tax !!";
                                Response.Errors.Add(error);
                            }


                        }
                    }
                    ;




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

        public GetDBTablesNameResponse GetDBTablesName()
        {
            GetDBTablesNameResponse response = new GetDBTablesNameResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetDBTablesNameDataList = new List<GetDBTablesNameData>();




                if (response.Result)
                {
                    var GetDBTablesNameDB = _context.Database.SqlQueryRaw<string>("Exec Myproc_GetDBTablesName").AsEnumerable().ToList();
                    response.GetDBTablesNameList = GetDBTablesNameDB;
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

        public GetTablesCloumnsResponse GetTablesCloumns()
        {
            GetTablesCloumnsResponse response = new GetTablesCloumnsResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetDBTablesNameDataList = new List<GetDBTablesNameData>();




                if (response.Result)
                {

                    var GetDBTablesNameDB = _context.Database.SqlQueryRaw<string>("Exec Myproc_GetDBTablesName").AsEnumerable().ToList();

                    var GetTablesList = new List<GetDBTablesColumnsData>();

                    foreach (var item in GetDBTablesNameDB)
                    {
                        var GetDBTablesCloumnsResponse = new GetDBTablesColumnsData();

                        GetDBTablesCloumnsResponse.TableName = item;
                        var TableName = new SqlParameter("TableName", System.Data.SqlDbType.NVarChar);
                        TableName.Value = item;


                        object[] param = new object[] { TableName };

                        var GetDBTablesCoulmnsNames = _context.Database.SqlQueryRaw<string>("Exec proc_GetDBTablesColumnsName @TableName", param).AsEnumerable().ToList();

                        GetDBTablesCloumnsResponse.ColumnsList = GetDBTablesCoulmnsNames;


                        GetTablesList.Add(GetDBTablesCloumnsResponse);

                    }
                    response.GetDBColumnsNames = GetTablesList;
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

        public GetUserListDDLResponse GetUserListDDL(int GroupId, long projectId)
        {
            GetUserListDDLResponse response = new GetUserListDDLResponse()
            {
                result = true,
                errors = new List<Error>()
            };

            try
            {
                if (response.result)
                {
                    var dataQuery = _unitOfWork.GroupUsers.FindAll(a => a.GroupId == GroupId, new[] { "User" });
                    //.Include(b => b.User)
                    //.ToList();

                    var defualtUserQuery = _unitOfWork.Projects.FindAll(a => a.Id == projectId, new[] { "SalesOffer" });
                    //.Include(b => b.SalesOffer).ToList();

                    var defualtUser = defualtUserQuery.FirstOrDefault();
                    var defualtUserName = dataQuery.Where(a => a.UserId == defualtUser.Id)
                                                    .Select(b => new
                                                    {
                                                        b.User.FirstName,
                                                        b.User.MiddleName,
                                                        b.User.LastName,
                                                    });

                    if (dataQuery != null)
                    {
                        var projectManagers = new List<MiniUserDDL>();

                        if (defualtUser != null)
                        {
                            projectManagers.Add(new MiniUserDDL
                            {
                                Id = defualtUser.Id,
                                Name = defualtUserName.Select(a => a.FirstName).FirstOrDefault() +
                                       defualtUserName.Select(a => a.MiddleName).FirstOrDefault() +
                                       defualtUserName.Select(a => a.LastName).FirstOrDefault()
                            });
                        }

                        foreach (var manager in dataQuery)
                        {
                            projectManagers.Add(new MiniUserDDL
                            {
                                Id = manager.User.Id,
                                Name = manager.User.FirstName + " " + manager.User.MiddleName + " " + manager.User.LastName,
                            });
                        }
                        response.UsersList = projectManagers;
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                response.result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.errors.Add(error);

                return response;
            }
        }

        public BaseResponseWithID AssignPManagerToProject(long projectId, long pManagerId)
        {
            BaseResponseWithID response = new BaseResponseWithID()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    var project = _unitOfWork.Projects.Find(a => a.Id == projectId);
                    if (project != null)
                    {
                        var user = _unitOfWork.Users.Find(a => a.Id == pManagerId);
                        if (pManagerId != 0)
                        {
                            if (user != null)
                            {
                                project.ProjectManagerId = pManagerId;
                                _unitOfWork.Complete();
                                response.ID = pManagerId;
                                //Send notification to the assigned PM
                                var offer = _unitOfWork.SalesOffers.Find(a => a.Id == project.SalesOfferId);
                                if (offer != null)
                                {
                                    NewGaras.Infrastructure.Entities.Notification notification = new NewGaras.Infrastructure.Entities.Notification();
                                    //notification.AddNew();
                                    notification.Title = "New Project Assigned (" + offer.ProjectName + ")";
                                    notification.Description = offer.ProjectName + " is now assigned to you.";
                                    notification.Date = DateTime.Now;
                                    notification.New = true;
                                    notification.Url = "~/project/details/" + project.Id;
                                    notification.UserId = pManagerId;
                                    _unitOfWork.Notifications.Add(notification);
                                    _unitOfWork.Complete();

                                }
                            }
                        }

                    }
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        public async Task<UserDDLResponse> GetUserList(int BranchId = 0, int RoleId = 0, long GroupId = 0, bool WithTeam = false)
        {
            UserDDLResponse Response = new UserDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                /*int BranchId = 0;
                if (!string.IsNullOrEmpty(headers["BranchId"]) && int.TryParse(headers["BranchId"], out BranchId))
                {
                    int.TryParse(headers["BranchId"], out BranchId);
                }
                int RoleId = 0;
                if (!string.IsNullOrEmpty(headers["RoleId"]) && int.TryParse(headers["RoleId"], out RoleId))
                {
                    int.TryParse(headers["RoleId"], out RoleId);
                }
                long GroupId = 0;
                if (!string.IsNullOrEmpty(headers["GroupId"]) && long.TryParse(headers["GroupId"], out GroupId))
                {
                    long.TryParse(headers["GroupId"], out GroupId);
                }*/

                var DDLList = new List<UserDDL>();
                if (Response.Result)
                {
                    List<long> UsersIds = new List<long>();

                    var ListDBQuery = _unitOfWork.Users.FindAllQueryable(x => x.Active == true).AsQueryable();
                    if (BranchId != 0)
                    {
                        ListDBQuery = ListDBQuery.Where(a => a.BranchId == BranchId);
                    }
                    if (RoleId != 0)
                    {
                        UsersIds.AddRange(_unitOfWork.UserRoles.FindAll(a => a.RoleId == RoleId).Select(a => (long)a.UserId));

                        /*foreach (var roleUser in RoleUsers)
                        {
                            UsersIds.Add(roleUser);
                        }*/
                    }
                    if (GroupId != 0)
                    {
                        UsersIds.AddRange(_unitOfWork.GroupUsers.FindAll(a => a.GroupId == GroupId).Select(a => (long)a.UserId));
                        /*foreach (var grpUser in GroupUsers)
                        {
                            UsersIds.Add(grpUser);
                        }*/
                    }

                    if (UsersIds.Count() > 0)
                    {
                        ListDBQuery = ListDBQuery.Where(a => UsersIds.Contains(a.Id)).Distinct();
                    }

                    var ListDB = ListDBQuery.ToList();
                    //if (headers["SearchKey"] != null && headers["SearchKey"] != "")
                    //{

                    //    string SearchKey = headers["SearchKey"];

                    //    var ListIDSMobileClient = _Context.proc_ClientMobileLoadAll().Where(x => x.Active == true && x.Mobile.ToLower().Contains(SearchKey.ToLower())).Select(x => x.ClientID).Distinct().ToList();
                    //    var ListIDSContactPersonClient = _Context.proc_ClientContactPersonLoadAll().Where(x => x.Active == true && (x.Mobile.ToLower().Contains(SearchKey.ToLower())
                    //                                                                                                      || x.Email != null ? x.Email.ToLower().Contains(SearchKey.ToLower()) : false)
                    //                                                                                                      ).Select(x => x.ClientID).Distinct().ToList();
                    //    ListDB = ListDB.Where(x => x.Name.ToLower().Contains(SearchKey.ToLower())
                    //                            || (x.Email != null ? x.Email.ToLower().Contains(SearchKey.ToLower()) : false)
                    //                            || ListIDSMobileClient.Contains(x.ID) || ListIDSContactPersonClient.Contains(x.ID)
                    //                            ).ToList();
                    //}
                    var HrUserList = new List<HrUser>();
                    if (WithTeam)
                    {
                        var ListUserIdsList = ListDB.Select(x => x.Id).ToList();
                        HrUserList = _unitOfWork.HrUsers.FindAll(x => ListUserIdsList.Contains(x.Id), new[] { "Team" }).ToList();
                    }
                    if (ListDB.Count > 0)
                    {
                        foreach (var user in ListDB)
                        {
                            var DLLObj = new UserDDL();
                            DLLObj.ID = user.Id;
                            DLLObj.Email = user.Email.Trim(); ;
                            DLLObj.BranchId = user.BranchId;
                            DLLObj.Department = user.Department?.Name; // != null ? Common.GetDepartmentName((int)user.DepartmentID) : "";
                            DLLObj.JobTitleName = user.JobTitle?.Name; // != null ? Common.GetJobTitleName((int)user.JobTitleID) : "";
                            DLLObj.BranchName = user.Branch?.Name; // != null ? Common.GetBranchName((int)user.BranchID) : "";
                            DLLObj.Name = user.FirstName.Trim() + " " + user.MiddleName.Trim() + " " + user.LastName.Trim();
                            if (HrUserList.Count() > 0)
                            {
                                var TeamObj = HrUserList.Where(x => x.UserId == user.Id).Select(x => x.BelongToChurch).FirstOrDefault();
                                if (TeamObj != null)
                                {
                                    DLLObj.TeamId = TeamObj.Id;
                                    DLLObj.Name = TeamObj.ChurchName;
                                }
                            }
                            if (user.PhotoUrl != null)
                            {
                                DLLObj.Image = Globals.baseURL + user.PhotoUrl;
                                //+ "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(user.Id.ToString(), key)) + "&type=photo&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();
                            }

                            // Fill User role List 
                            var RoleList = new List<Roles>();
                            //var RoleListDB = await _Context.VUserRoles.Where(x => x.UserId == user.Id).ToListAsync();
                            var RoleListDB = await _unitOfWork.UserRoles.FindAllAsync(x => x.UserId == user.Id, new[] { "Role", "User" });

                            foreach (var UserRoleOBJ in RoleListDB)
                            {
                                RoleList.Add(new Roles
                                {
                                    RoleID = UserRoleOBJ.RoleId,
                                    RoleName = UserRoleOBJ.Role.Name // Common.GetRoleName(UserRoleOBJ.RoleID)
                                });
                            }

                            // Fill User Group List 
                            var GroupList = new List<GroupRoles>();
                            var GroupListDB = await _unitOfWork.GroupUsers.FindAllAsync(x => x.UserId == user.Id && x.Active == true, new[] { "Group" });
                            foreach (var UserGroupOBJ in GroupListDB)
                            {
                                GroupList.Add(new GroupRoles
                                {
                                    GroupID = UserGroupOBJ.GroupId,
                                    GroupName = UserGroupOBJ.Group.Name
                                }); ;
                            }
                            DLLObj.RoleList = RoleList;
                            DLLObj.GroupList = GroupList;
                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList.Distinct().ToList();
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

        public async Task<GetGroupResponse> GetGroup()
        {
            GetGroupResponse response = new GetGroupResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetGroupList = new List<GroupData>();




                if (response.Result)
                {

                    var GetGroupDB = await _unitOfWork.Groups.GetAllAsync();


                    if (GetGroupDB != null && GetGroupDB.Count() > 0)
                    {

                        foreach (var GetGroupOBJ in GetGroupDB)
                        {
                            var GetGroupResponse = new GroupData();

                            GetGroupResponse.ID = (int)GetGroupOBJ.Id;

                            GetGroupResponse.Name = GetGroupOBJ.Name;

                            GetGroupResponse.Description = GetGroupOBJ.Description;

                            GetGroupResponse.Active = GetGroupOBJ.Active;


                            GetGroupResponse.ModifiedBy = GetGroupOBJ.ModifiedBy.ToString();





                            GetGroupResponse.CreatedBy = GetGroupOBJ.CreatedBy.ToString();


                            var GetGroupUserDB = await _unitOfWork.GroupUsers.FindAllAsync(x => x.GroupId == GetGroupOBJ.Id);
                            GetGroupUserDB = GetGroupUserDB.Distinct();
                            var GetGroupRoleDB = _unitOfWork.GroupRoles.FindAll(x => x.GroupId == GetGroupOBJ.Id).Select(x => x.RoleId).ToList();
                            var GetGroupUserCount = GetGroupUserDB.Count();
                            var GetGroupRoleCount = GetGroupRoleDB.Count();

                            GetGroupResponse.GroupUserCount = GetGroupUserCount;
                            GetGroupResponse.GroupRoleCount = GetGroupRoleCount;

                            GetGroupResponse.RoleIDs = GetGroupRoleDB;
                            var GetGroupUserList = new List<GroupUserList>();
                            foreach (var GroupUserOBJ in GetGroupUserDB)
                            {
                                var GroupUserResponse = new GroupUserList();

                                //GroupUserResponse.ID = (int)GroupUserOBJ.ID;

                                //GroupUserResponse.GroupID = (int)GroupUserOBJ.GroupID;

                                GroupUserResponse.UserID = GroupUserOBJ.UserId != null ? (int)GroupUserOBJ.UserId : 0;

                                GroupUserResponse.UserName = GroupUserOBJ.UserId != null ? _unitOfWork.Users.FindAll(x => x.Id == (long)GroupUserOBJ.UserId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault() : "";





                                GetGroupUserList.Add(GroupUserResponse);
                            }
                            GetGroupResponse.GroupUser = GetGroupUserList;








                            GetGroupList.OrderBy(x => x.Active);

                            GetGroupList.Add(GetGroupResponse);

                        }
                        response.GroupDataList = GetGroupList;



                    }

                }


                response.GroupDataList = GetGroupList;
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

        public async Task<BaseResponseWithID> EditEmployeeGroup(EmployeeGroupData request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    //long UserID = 0;
                    //if (!string.IsNullOrEmpty(Request.Headers["UserID"]) && long.TryParse(Request.Headers["UserID"], out UserID))
                    //{
                    //    long.TryParse(Request.Headers["UserID"], out UserID);
                    //}

                    if (Response.Result)
                    {
                        var user = _unitOfWork.HrUsers.GetById((long)request.UserID);
                        if (user != null)
                        {
                            var ListUserGroupVM = request.userGroupData;
                            var ListUserGroupDB = await _unitOfWork.GroupUsers.FindAllAsync(x => x.HrUserId == request.UserID || x.UserId == user.UserId);
                            if (ListUserGroupVM != null && ListUserGroupVM.Count() > 0)
                            {
                                var IDSListUserGroupDB = ListUserGroupDB.Select(x => x.Id).ToList();
                                var IDSListUserGroupVM = new List<long>();



                                foreach (var item in ListUserGroupVM)
                                {
                                    //if (item.ID != 0) // Edit
                                    //{
                                    //    var UpdateKeeperObjDB = ListUserGroupDB.Where(x => x.Id == item.ID).FirstOrDefault();
                                    //    UpdateKeeperObjDB.GroupId = item.GroupID;
                                    //    _Context.SaveChanges();
                                    //}
                                    //else //Insert
                                    //{
                                    var CheckIfExistBefore = ListUserGroupDB.Where(x => (x.HrUserId == request.UserID || x.UserId == user.UserId) && x.GroupId == item.GroupID).FirstOrDefault();
                                    if (CheckIfExistBefore == null)
                                    {
                                        var hrUser = _unitOfWork.HrUsers.GetById((long)request.UserID);
                                        var InsertUserGroupsObjDB = new GroupUser();
                                        InsertUserGroupsObjDB.HrUserId = request.UserID;
                                        InsertUserGroupsObjDB.GroupId = item.GroupID;


                                        InsertUserGroupsObjDB.CreatedBy = UserId;

                                        InsertUserGroupsObjDB.CreationDate = DateTime.Now;
                                        InsertUserGroupsObjDB.Active = true;
                                        if (hrUser != null && hrUser.IsUser)
                                        {
                                            InsertUserGroupsObjDB.UserId = hrUser.UserId;
                                        }
                                        _unitOfWork.GroupUsers.Add(InsertUserGroupsObjDB);
                                        var Res = _unitOfWork.Complete();
                                        if (Res > 0)
                                        {
                                            IDSListUserGroupDB.Add(InsertUserGroupsObjDB.Id);
                                            IDSListUserGroupVM.Add(InsertUserGroupsObjDB.Id);
                                        }
                                    }
                                    else
                                    {
                                        IDSListUserGroupVM.Add(CheckIfExistBefore.Id);
                                    }
                                    //}
                                }


                                //int Counter = ListKeeperVM.Count() >= ListKeeperDB.Count() ? ListKeeperVM.Count() : ListKeeperDB.Count();

                                //for (int Count = 0; Count < ListKeeperVM.Count(); Count++)
                                //{
                                //    if (ListKeeperVM[Count].ID != 0) // Edit
                                //    {
                                //        var UpdateKeeperObjDB = ListKeeperDB.Where(x => x.ID == ListKeeperVM[Count].ID).FirstOrDefault();
                                //        UpdateKeeperObjDB.UserID = ListKeeperVM[Count].UserID;
                                //        _Context.SaveChanges();
                                //    }
                                //    else //Insert
                                //    {
                                //        var CheckIfExistBefore = ListKeeperDB.Where(x => x.UserID == ListKeeperVM[Count].UserID).FirstOrDefault();
                                //        if (CheckIfExistBefore == null)
                                //        {
                                //            var InsertKeeperObjDB =new InventoryStoreKeeper();
                                //            InsertKeeperObjDB.InventoryStoreID = Request.ID;
                                //            InsertKeeperObjDB.UserID = ListKeeperVM[Count].UserID;
                                //            InsertKeeperObjDB.Active = ListKeeperVM[Count].Active;
                                //            InsertKeeperObjDB.CreatedBy = validation.userID;
                                //            InsertKeeperObjDB.ModifiedBy = validation.userID;
                                //            InsertKeeperObjDB.CreationDate = DateTime.Now;
                                //            InsertKeeperObjDB.ModifiedDate = DateTime.Now;
                                //            _Context.InventoryStoreKeepers.Add(InsertKeeperObjDB);
                                //           var Res =  _Context.SaveChanges();
                                //            if (Res > 0)
                                //            {
                                //                IDSListKeebersDB.Add(InsertKeeperObjDB.ID);
                                //                IDSListKeebersVM.Add(InsertKeeperObjDB.ID);
                                //            }
                                //        }
                                //    }
                                //}

                                var IDSListToRemove = IDSListUserGroupDB.Except(IDSListUserGroupVM).ToList();

                                var DeletedUserGroupListDB = ListUserGroupDB.Where(x => IDSListToRemove.Contains(x.Id)).ToList();
                                _unitOfWork.GroupUsers.DeleteRange(DeletedUserGroupListDB);
                                _unitOfWork.Complete();


                            }
                            else // List is empty must be deleted
                            {
                                // delete list from DB
                                _unitOfWork.GroupUsers.DeleteRange(ListUserGroupDB);
                                _unitOfWork.Complete();
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "User is not Found !!";
                            Response.Errors.Add(error);
                        }




                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Faild To Insert this User Group !!";
                        Response.Errors.Add(error);
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

        public async Task<BaseResponseWithID> EditEmployeeGroupNew(EmployeeGroupData request, long UserId)
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
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    //long UserID = 0;
                    //if (!string.IsNullOrEmpty(Request.Headers["UserID"]) && long.TryParse(Request.Headers["UserID"], out UserID))
                    //{
                    //    long.TryParse(Request.Headers["UserID"], out UserID);
                    //}

                    if (Response.Result)
                    {

                        var ListUserGroupVM = request.userGroupData;
                        var ListUserGroupDB = await _unitOfWork.GroupUsers.FindAllAsync(x => x.UserId == request.UserID);
                        if (ListUserGroupVM != null && ListUserGroupVM.Count() > 0)
                        {
                            var IDSListUserGroupDB = ListUserGroupDB.Select(x => x.Id).ToList();
                            var IDSListUserGroupVM = new List<long>();



                            foreach (var item in ListUserGroupVM)
                            {
                                //if (item.ID != 0) // Edit
                                //{
                                //    var UpdateKeeperObjDB = ListUserGroupDB.Where(x => x.ID == item.ID).FirstOrDefault();
                                //    UpdateKeeperObjDB.GroupID = item.GroupID;
                                //    _Context.SaveChanges();
                                //}
                                //else //Insert
                                //{
                                var CheckIfExistBefore = ListUserGroupDB.Where(x => x.UserId == request.UserID && x.GroupId == item.GroupID).FirstOrDefault();
                                if (CheckIfExistBefore == null)
                                {
                                    var InsertUserGroupsObjDB = new GroupUser();
                                    InsertUserGroupsObjDB.UserId = request.UserID;
                                    InsertUserGroupsObjDB.GroupId = item.GroupID;


                                    InsertUserGroupsObjDB.CreatedBy = UserId;

                                    InsertUserGroupsObjDB.CreationDate = DateTime.Now;
                                    InsertUserGroupsObjDB.Active = true;

                                    _unitOfWork.GroupUsers.Add(InsertUserGroupsObjDB);
                                    var Res = _unitOfWork.Complete();
                                    if (Res > 0)
                                    {
                                        IDSListUserGroupDB.Add(InsertUserGroupsObjDB.Id);
                                        IDSListUserGroupVM.Add(InsertUserGroupsObjDB.Id);
                                        var grouproles = _unitOfWork.GroupRoles.FindAll(a => a.GroupId == item.GroupID).ToList();
                                        foreach (var role in grouproles)
                                        {
                                            var check = _unitOfWork.UserRoles.Find(a => a.UserId == request.UserID && a.RoleId == role.RoleId);
                                            if (check == null)
                                            {
                                                var userRole = new UserRole()
                                                {
                                                    RoleId = role.RoleId,
                                                    UserId = request.UserID,
                                                    CreatedBy = UserId,
                                                    CreationDate = DateTime.Now
                                                };
                                                _unitOfWork.UserRoles.Add(userRole);
                                                _unitOfWork.Complete();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    IDSListUserGroupVM.Add(CheckIfExistBefore.Id);
                                }
                                //}
                            }


                            //int Counter = ListKeeperVM.Count() >= ListKeeperDB.Count() ? ListKeeperVM.Count() : ListKeeperDB.Count();

                            //for (int Count = 0; Count < ListKeeperVM.Count(); Count++)
                            //{
                            //    if (ListKeeperVM[Count].ID != 0) // Edit
                            //    {
                            //        var UpdateKeeperObjDB = ListKeeperDB.Where(x => x.ID == ListKeeperVM[Count].ID).FirstOrDefault();
                            //        UpdateKeeperObjDB.UserID = ListKeeperVM[Count].UserID;
                            //        _Context.SaveChanges();
                            //    }
                            //    else //Insert
                            //    {
                            //        var CheckIfExistBefore = ListKeeperDB.Where(x => x.UserID == ListKeeperVM[Count].UserID).FirstOrDefault();
                            //        if (CheckIfExistBefore == null)
                            //        {
                            //            var InsertKeeperObjDB =new InventoryStoreKeeper();
                            //            InsertKeeperObjDB.InventoryStoreID = Request.ID;
                            //            InsertKeeperObjDB.UserID = ListKeeperVM[Count].UserID;
                            //            InsertKeeperObjDB.Active = ListKeeperVM[Count].Active;
                            //            InsertKeeperObjDB.CreatedBy = validation.userID;
                            //            InsertKeeperObjDB.ModifiedBy = validation.userID;
                            //            InsertKeeperObjDB.CreationDate = DateTime.Now;
                            //            InsertKeeperObjDB.ModifiedDate = DateTime.Now;
                            //            _Context.InventoryStoreKeepers.Add(InsertKeeperObjDB);
                            //           var Res =  _Context.SaveChanges();
                            //            if (Res > 0)
                            //            {
                            //                IDSListKeebersDB.Add(InsertKeeperObjDB.ID);
                            //                IDSListKeebersVM.Add(InsertKeeperObjDB.ID);
                            //            }
                            //        }
                            //    }
                            //}

                            var IDSListToRemove = IDSListUserGroupDB.Except(IDSListUserGroupVM).ToList();

                            var DeletedUserGroupListDB = ListUserGroupDB.Where(x => IDSListToRemove.Contains(x.Id)).ToList();
                            foreach (var group in DeletedUserGroupListDB)
                            {
                                var roles = _unitOfWork.GroupRoles.FindAll(a => a.GroupId == group.GroupId).ToList();
                                foreach (var role in roles)
                                {
                                    var userRoles = _unitOfWork.UserRoles.FindAll(a => a.RoleId == role.Id && a.UserId == request.UserID);
                                    _unitOfWork.UserRoles.DeleteRange(userRoles);
                                    _unitOfWork.Complete();
                                }
                            }
                            _unitOfWork.GroupUsers.DeleteRange(DeletedUserGroupListDB);
                            _unitOfWork.Complete();


                        }
                        else // List is empty must be deleted
                        {
                            // delete list from DB
                            _unitOfWork.GroupUsers.DeleteRange(ListUserGroupDB);
                            _unitOfWork.Complete();
                        }





                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Faild To Insert this User Group !!";
                        Response.Errors.Add(error);
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

        public async Task<BaseResponseWithData<List<SelectDDL>>> GetTeamList(int? DepartmentId)
        {
            var response = new BaseResponseWithData<List<SelectDDL>>();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var List = new List<SelectDDL>();

                if (DepartmentId == 0 || DepartmentId == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "Invalid Department Id.";
                    response.Errors.Add(error);
                    return response;
                }
                var DepartmentDB = _unitOfWork.Departments.Find(x => x.Id == DepartmentId);
                if (DepartmentDB == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "Invalid Department";
                    response.Errors.Add(error);
                    return response;
                }
                if (response.Result)
                {
                    var TeamDBList = _unitOfWork.Teams.FindAll(x => x.DepartmentId == DepartmentId).ToList();
                    List = TeamDBList.Select(item => new SelectDDL { ID = item.Id, Name = item.Name }).ToList();
                }


                response.Data = List;
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


        public GetEmployeeListResponse GetEmployeeList(GetEmployeeListFilters filters)
        {
            GetEmployeeListResponse response = new GetEmployeeListResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetEmployeeResponseList = new List<EmployeeBasicInfo>();

                UserLogin login = new UserLogin();

                if (response.Result)
                {
                    if (!string.IsNullOrEmpty(filters.SearchKey))
                    {
                        filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);
                    }

                    if (response.Result)
                    {
                        var GetEmployeeInfoDB = _unitOfWork.Users.FindAllQueryable(a => true);
                        if (validation.userID != 1)
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.Id != 1);
                        }
                        if (!string.IsNullOrEmpty(filters.SearchKey))
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.FirstName.Contains(filters.SearchKey) ||
                                                                             x.LastName.Contains(filters.SearchKey) ||
                                                                             x.MiddleName.Contains(filters.SearchKey) ||
                                                                             x.Email.Contains(filters.SearchKey) ||
                                                                             x.Mobile == filters.SearchKey
                                                                             ).AsQueryable();
                        }
                        if (filters.isExpired)
                        {
                            var listCount = _unitOfWork.HremployeeAttachments.FindAll(x => x.EmployeeUserId == x.Id && x.Active && x.ExpiredDate < filters.expiredIn).Count();
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(a => listCount > 0).AsQueryable();
                        }
                        if (filters.DepartmentID != 0)
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.DepartmentId == filters.DepartmentID).AsQueryable();
                        }
                        if (filters.BranchID != 0)
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.BranchId == filters.BranchID).AsQueryable();
                        }
                        if (filters.UserID != 0)
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.Id == filters.UserID).AsQueryable();
                        }
                        var EmployeePagingList = PagedList<User>.Create(GetEmployeeInfoDB.OrderBy(x => x.FirstName), filters.CurrentPage, filters.NumberOfItemsPerPage);

                        response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = EmployeePagingList.CurrentPage,
                            TotalPages = EmployeePagingList.TotalPages,
                            ItemsPerPage = EmployeePagingList.PageSize,
                            TotalItems = EmployeePagingList.TotalCount
                        };



                        if (EmployeePagingList != null)
                        {

                            foreach (var GetEmployeeOBJ in EmployeePagingList)
                            {

                                var GetEmployeeResponse = new EmployeeBasicInfo();

                                GetEmployeeResponse.ID = (int)GetEmployeeOBJ.Id;

                                GetEmployeeResponse.EmployeeName = GetEmployeeOBJ.FirstName + " " + GetEmployeeOBJ.MiddleName + " " + GetEmployeeOBJ.LastName;
                                GetEmployeeResponse.DepartmentID = GetEmployeeOBJ.DepartmentId;
                                GetEmployeeResponse.DepartmentName = GetEmployeeOBJ.Department?.Name;
                                GetEmployeeResponse.JobTitleID = GetEmployeeOBJ.JobTitleId;
                                GetEmployeeResponse.JobTitleName = GetEmployeeOBJ.JobTitle?.Name;
                                GetEmployeeResponse.Email = GetEmployeeOBJ.Email;
                                GetEmployeeResponse.Mobile = GetEmployeeOBJ.Mobile;
                                GetEmployeeResponse.Status = GetEmployeeOBJ.Active;
                                GetEmployeeResponse.BranchName = GetEmployeeOBJ.Branch?.Name;


                                GetEmployeeResponse.Photo = GetEmployeeOBJ.PhotoUrl == null ? null : Globals.baseURL + "/" + GetEmployeeOBJ.PhotoUrl;

                                GetEmployeeResponseList.Add(GetEmployeeResponse);
                            }



                        }

                    }


                    response.EmployeeInfoList = GetEmployeeResponseList;

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

        public async Task<BaseResponseWithMessage<string>> TopSellingProductExcel(GetMyProjectsDetailsCRMHeaders headers)
        {
            //BaseMessageResponse Response = new BaseMessageResponse();
            BaseResponseWithMessage<string> Response = new BaseResponseWithMessage<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                

                var GetMyProjectsList = await _projectService.GetMyProjectsDetailsCRM(headers);





                var dt = new System.Data.DataTable("Grid");

                dt.Columns.AddRange(new DataColumn[4] {
                                                     //new DataColumn("ID"),
                                                     new DataColumn("Category Name"),
                                                     new DataColumn("Product Name"),
                                                     new DataColumn("Quantity"),
                                                     new DataColumn("Value")




                    });



                if (GetMyProjectsList != null)
                {

                    foreach (var CategoryName in GetMyProjectsList.TopSellingProductsCategoryGrouped)
                    {

                        foreach (var ProductsDetails in CategoryName.TopSellingProductsList)
                        {
                            dt.Rows.Add(
                            //item != null ? item.ID : 0

                            //item.Id != null ? item.Id : 0,
                            CategoryName.CategoryName != null ? CategoryName.CategoryName : "N/A",
                            ProductsDetails.ProductName != null ? ProductsDetails.ProductName : "N/A",
                            ProductsDetails.SoldCount,
                            ProductsDetails.TotalSoldPrice
                            );

                        }
                        ;


                    }

                }


                var dtProduct = new System.Data.DataTable("Grid");

                dtProduct.Columns.AddRange(new DataColumn[3] {
                                                     //new DataColumn("ID"),
                                                 
                                                     new DataColumn("Product Name"),
                                                     new DataColumn("Quantity"),
                                                     new DataColumn("Value")




                    });



                if (GetMyProjectsList != null)
                {



                    foreach (var ProductsDetails in GetMyProjectsList.TopSellingProducts)
                    {
                        dtProduct.Rows.Add(


                            ProductsDetails.ProductName != null ? ProductsDetails.ProductName : "N/A",
                            ProductsDetails.SoldCount,
                            ProductsDetails.TotalSoldPrice
                            );

                    }
                    ;




                }



                var dtProductSum = new System.Data.DataTable("Grid");

                dtProductSum.Columns.AddRange(new DataColumn[3] {
                                                     //new DataColumn("ID"),
                                                 
                                                     new DataColumn("Category"),
                                                     new DataColumn("Quantity"),
                                                     new DataColumn("Value")




                    });



                if (GetMyProjectsList != null)
                {



                    foreach (var ProductsSum in GetMyProjectsList.TopSellingProductsCategoryGrouped)
                    {
                        dtProductSum.Rows.Add(


                            ProductsSum.CategoryName,
                            ProductsSum.TotalDealsCount,
                            ProductsSum.TotalDealsPrice
                            );

                    }
                    ;




                }






                //Second List to pass it to PDF



                //if (FileExtension != null && FileExtension == "xml")
                //{
                using (ExcelPackage packge = new ExcelPackage())
                {
                    //Create the worksheet
                    ExcelWorksheet ws = packge.Workbook.Worksheets.Add("SalesOffer");
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






                    using (var package = new ExcelPackage())
                    {





                        var CompanyName = validation.CompanyName.ToString().ToLower();

                        string FullFileName = DateTime.Now.ToFileTime() + "_" + "ProjectDetails.xlsx";
                        string PathsTR = "/Attachments/" + CompanyName + "/";
                        String path = Path.Combine(_host.WebRootPath, PathsTR);
                            /*HttpContext.Current.Server.MapPath(PathsTR);*/
                        string p_strPath = Path.Combine(path, FullFileName);
                        var workSheet = package.Workbook.Worksheets.Add("Project Details Grouped by Category");
                        ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);
                        var workSheetByProduct = package.Workbook.Worksheets.Add("Project Details Product");
                        ExcelRangeBase excelRangeBase2 = workSheetByProduct.Cells.LoadFromDataTable(dtProduct, true);
                        var workSheetProductSum = package.Workbook.Worksheets.Add("Project Product Sum");
                        ExcelRangeBase excelRangeBase3 = workSheetProductSum.Cells.LoadFromDataTable(dtProductSum, true);

                        File.Exists(p_strPath);
                        FileStream objFileStrm = File.Create(p_strPath);

                        objFileStrm.Close();
                        package.Save();
                        File.WriteAllBytes(p_strPath, package.GetAsByteArray());
                        package.Dispose();

                        Response.Message = Globals.baseURL+"/" + PathsTR + FullFileName;


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

        //------------------------------------Dashboard-----------------------------------------

        public decimal GetNetProfitIncomeStatment()
        {
            decimal Amount = 0;
            var data = _unitOfWork.Accounts.FindAll(x => x.ParentCategory == 0 && (x.AccountCategoryId == 4 || x.AccountCategoryId == 5));
            Amount = data.Select(x => x.Accumulative).Sum();
            #region Old Calc ...
            //var AccountsListDB = _Context.proc_AccountsLoadAll().Where(x => x.ParentCategory == 0 && (x.AccountCategoryID == 4 || x.AccountCategoryID == 5)).ToList();
            //var AcumelativeIncomeEntrySum = _Context.proc_AccountsLoadAll().Where(x => x.ParentCategory == 0 && x.AccountCategoryID == 4).Select(x => x.Accumulative).Sum();
            //var AcumelativeIncomeEntrySum = _Context.proc_AccountOfJournalEntryLoadAll().Where(x => IDAccountsIncomeDB.Contains(x.AccountID) && x.Active == true).Select(x => x.Amount).Sum();

            //var AcumelativeExpensesEntrySum = _Context.proc_AccountsLoadAll().Where(x => x.ParentCategory == 0 && x.AccountCategoryID == 5).Select(x => x.Accumulative).Sum();
            // var AcumelativeExpensesEntrySum = _Context.proc_AccountOfJournalEntryLoadAll().Where(x => IDAccountsExpensesDB.Contains(x.AccountID) && x.Active == true).Select(x => x.Amount).Sum();

            //if (AcumelativeIncomeEntrySum > AcumelativeExpensesEntrySum)
            //{
            //}
            //Amount = AcumelativeIncomeEntrySum + AcumelativeExpensesEntrySum;
            #endregion
            return Amount;
        }

        public decimal GetTotalAmountInventoryItem(long InventoryStoreID)
        {
            decimal totalValue = 0;
            var InventoryStoreItemQuerableDB = _unitOfWork.VInventoryStoreItemPrices.FindAllQueryable(x => x.Active == true && x.StoreActive == true).AsQueryable();
            if (InventoryStoreID != 0)
            {
                InventoryStoreItemQuerableDB = InventoryStoreItemQuerableDB.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
            }
            //var InventoryStoreItemListDB = InventoryStoreItemQuerableDB.ToList();
            totalValue = InventoryStoreItemQuerableDB.Select(x => x.SumaverageUnitPrice != null && x.SumaverageUnitPrice != 0 ? x.SumaverageUnitPrice : x.SumcustomeUnitPrice != null ? x.SumcustomeUnitPrice : 0).Sum() ?? 0;
            //var totalValuea = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true && x.StoreActive == true).Select(x => (x.SUMAverageUnitPrice != null && x.SUMAverageUnitPrice != 0) ? x.SUMAverageUnitPrice : (x.SUMCustomeUnitPrice != null ? x.SUMCustomeUnitPrice : 0)).Sum() ?? 0;



            //var SUMAverageUnitPriceQuerable = InventoryStoreItemQuerableDB.Where(x => x.CalculationType == 1 && x.SUMAverageUnitPrice != null).Select(x => (decimal)x.SUMAverageUnitPrice).Sum();
            //var SUMAverageUnitPrice = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true && x.StoreActive == true && x.CalculationType == 1 && x.SUMAverageUnitPrice != null).Sum(x => (decimal)x.SUMAverageUnitPrice);


            //   //var SUMMaxUnitPrice = InventoryStoreItemQuerableDB.Where(x => x.CalculationType == 2 && x.SUMMaxUnitPrice != null)?.Select(x => (decimal)x.SUMMaxUnitPrice)?.Sum()??0;

            //   var SUMLastUnitPrice = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true && x.StoreActive == true && x.CalculationType == 3 && x.SUMLastUnitPrice != null).Sum(x => (decimal)x.SUMLastUnitPrice);
            //   var SUMLastUnitPriceQuerable = InventoryStoreItemQuerableDB.Where(x => x.CalculationType == 3 && x.SUMLastUnitPrice != null).Select(x => (decimal)x.SUMLastUnitPrice).Sum();

            //   var SUMCustomeUnitPrice = InventoryStoreItemQuerableDB.Where(x => x.CalculationType == 4 && x.SUMCustomeUnitPrice != null).Select(x => (decimal)x.SUMCustomeUnitPrice).Sum();
            //   totalValue = SUMAverageUnitPrice + 0 + SUMLastUnitPrice + SUMCustomeUnitPrice;

            //foreach (var Store in InventoryStoreItemListDB)
            //{
            //    if (Store.CalculationType == 1)
            //    {
            //        if (Store.SUMAverageUnitPrice != null)
            //        {
            //            totalValue = totalValue + (decimal)Store.SUMAverageUnitPrice;
            //        }
            //    }
            //    else if (Store.CalculationType == 2)
            //    {
            //        if (Store.SUMMaxUnitPrice != null)
            //        {
            //            totalValue = totalValue + (decimal)Store.SUMMaxUnitPrice;
            //        }
            //    }
            //    else if (Store.CalculationType == 3)
            //    {
            //        if (Store.SUMLastUnitPrice != null)
            //        {
            //            totalValue = totalValue + (decimal)Store.SUMLastUnitPrice;
            //        }
            //    }
            //    else if (Store.CalculationType == 4)
            //    {
            //        if (Store.SUMCustomeUnitPrice != null)
            //        {
            //            totalValue = totalValue + (decimal)Store.SUMCustomeUnitPrice;
            //        }
            //    }
            //}


            //foreach (var Store in InventoryItemsListDB)
            //{
            //    if (Store.CalculationType == 1)
            //    {
            //        totalValue = totalValue + (decimal)Store.SUMAverageUnitPrice;
            //    }
            //    else if (Store.CalculationType == 2)
            //    {
            //        totalValue = totalValue + (decimal)Store.SUMMaxUnitPrice;
            //    }
            //    else if (Store.CalculationType == 3)
            //    {
            //        totalValue = totalValue + (decimal)Store.SUMLastUnitPrice;
            //    }
            //    else if (Store.CalculationType == 4)
            //    {
            //        totalValue = totalValue + (decimal)Store.SUMCustomeUnitPrice;
            //    }
            //}
            return totalValue;
        }

        public decimal GetTotalSalesForceClientReportAmount()
        {
            decimal totalValue = 0;
            DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
            var ProjectSalesOfferListDB = _unitOfWork.VProjectSalesOfferClients.FindAll(a => a.OfferType != "New Internal Order" && a.SalesOfferStatus == "Closed" && a.ProjectCreationDate >= StartYear && a.ProjectCreationDate < DateTime.Now && a.ProjectActive == true).ToList();
            //var ProjectSalesOfferListDB = _Context.V_Project_SalesOffer.Where(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now).ToList();
            decimal FinalOfferPrice = ProjectSalesOfferListDB.Where(x => x.FinalOfferPrice != null).Select(x => (decimal)x.FinalOfferPrice).Sum();
            decimal ExtraCost = ProjectSalesOfferListDB.Where(x => x.ProjectExtraCost != null).Select(x => (decimal)x.ProjectExtraCost).Sum();
            totalValue = FinalOfferPrice + ExtraCost;
            return totalValue;
        }

        public decimal GetTotalPurchasingAndSupplierReportAmount()
        {
            decimal totalValue = 0;
            DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
            var SupplierPurchasePOListDB = _unitOfWork.VPurchasePos.FindAll(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now).ToList();
            totalValue = SupplierPurchasePOListDB.Where(x => x.TotalInvoiceCost != null).Select(x => (decimal)x.TotalInvoiceCost).Sum();
            return totalValue;
        }

        public long GetTotalCountOfProjects(string ProjectsStatus)
        {
            long TotalProjectsCount = 0;

            Expression<Func<VProjectSalesOffer, bool>> whereClause;

            switch (ProjectsStatus.ToLower())
            {
                case "open":
                    whereClause = a => a.Closed == false && a.Active == true;
                    break;
                case "closed":
                    whereClause = a => a.Closed == true && a.Active == true;
                    break;
                case "deactivated":
                    whereClause = a => a.Active == false;
                    break;
                default:
                    whereClause = a => a.Active == true;
                    break;
            }

            var projects = new List<VProjectSalesOffer>();

            TotalProjectsCount = _unitOfWork.VProjectSalesOffers.FindAll(whereClause).Count();

            return TotalProjectsCount;
        }

        public decimal GetTotalCostOfProjects(string ProjectsStatus)
        {
            decimal TotalProjectsCost = 0;

            Expression<Func<VProjectSalesOffer, bool>> whereClause;

            switch (ProjectsStatus.ToLower())
            {
                case "open":
                    whereClause = a => a.Closed == false && a.Active == true;
                    break;
                case "closed":
                    whereClause = a => a.Closed == true && a.Active == true;
                    break;
                case "deactivated":
                    whereClause = a => a.Active == false;
                    break;
                default:
                    whereClause = a => a.Active == true;
                    break;
            }

            var projects = new List<VProjectSalesOffer>();

            projects = _unitOfWork.VProjectSalesOffers.FindAll(whereClause).ToList();
            TotalProjectsCost = projects.Sum(a => a.ExtraCost ?? 0 + a.FinalOfferPrice ?? 0);


            return TotalProjectsCost;
        }

        public decimal GetTotalCollectedCostOfProjects(string ProjectsStatus)
        {
            decimal TotalProjectsCollectedCost = 0;

            Expression<Func<VProjectSalesOffer, bool>> whereClause;

            switch (ProjectsStatus.ToLower())
            {
                case "open":
                    whereClause = a => a.Closed == false && a.Active == true;
                    break;
                case "closed":
                    whereClause = a => a.Closed == true && a.Active == true;
                    break;
                case "deactivated":
                    whereClause = a => a.Active == false;
                    break;
                default:
                    whereClause = a => a.Active == true;
                    break;
            }

            var projects = new List<VProjectSalesOffer>();

            projects = _unitOfWork.VProjectSalesOffers.FindAll(whereClause).ToList();

            var test = _unitOfWork.VProjectSalesOffers.FindAll(whereClause).Select(a => a.Id).ToList();


            var clientAccounts = _unitOfWork.ClientAccounts.FindAll(x => test.Contains(x.ProjectId ?? 0));

            decimal ProjectCollected = 0;

            foreach (var clientAccount in clientAccounts)
            {
                if (clientAccount.AmountSign == "plus")
                {
                    ProjectCollected = ProjectCollected + clientAccount.Amount;
                }
                else if (clientAccount.AmountSign == "minus")
                {
                    ProjectCollected = ProjectCollected - clientAccount.Amount;
                }
            }

            TotalProjectsCollectedCost += ProjectCollected;

            return TotalProjectsCollectedCost;
        }

        public DashboardResponse GetDashboard()
        {
            DashboardResponse Response = new DashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DashboardInfoObj = new DashboardInfo();
                if (Response.Result)
                {
                    // Call Constructor Common Class 
                    // Common._Context = _Context;

                    //var ResponseAccountAndFinanceIncomeStatment = GetAccountsAndFinanceIncomeStatment();
                    //DashboardInfoObj.AccountsAndFinance = ResponseAccountAndFinanceIncomeStatment.NetProfit;
                    DashboardInfoObj.AccountsAndFinance = GetNetProfitIncomeStatment();
                    // API Inventory
                    //var ResponseInventory = GetAccountAndFinanceInventoryStoreItemReportList();
                    // DashboardInfoObj.InventoryAndStores = ResponseInventory.TotalStockBalanceValue;
                    DashboardInfoObj.InventoryAndStores = GetTotalAmountInventoryItem(0); //0 =>  All Items
                                                                                          // API supplier Report Sales Force
                                                                                          // var ResponseClientReport = GetAccountAndFinanceClientReportList();
                                                                                          // DashboardInfoObj.SalesForceAndClients = ResponseClientReport.TotalSalesVolume;
                                                                                          //DashboardInfoObj.TotalFinalOfferPrice = Common.GetTotalSalesOfferProjectFinalPriceAmount();
                                                                                          //DashboardInfoObj.TotalProjectExtraCost = Common.GetTotalSalesOfferProjectExtraCostsAmount();
                                                                                          //DashboardInfoObj.TotalFinalOfferPriceWithInternalType = Common.GetTotalSalesOfferProjectAmountForOffterTypeInternal();
                                                                                          //DashboardInfoObj.SalesForceAndClients = DashboardInfoObj.TotalFinalOfferPrice + DashboardInfoObj.TotalProjectExtraCost;
                    DashboardInfoObj.SalesForceAndClients = GetTotalSalesForceClientReportAmount();

                    // API supplier Report Sales Force
                    // var ResponseSupplierReport = GetAccountAndFinanceSupplierReportList();
                    // DashboardInfoObj.PurchasingAndSuppliers = ResponseSupplierReport.TotalSalesVolume;
                    DashboardInfoObj.PurchasingAndSuppliers = GetTotalPurchasingAndSupplierReportAmount();

                    // Project Detailes
                    DashboardInfoObj.CountOFOpenProject = GetTotalCountOfProjects("open");
                    DashboardInfoObj.CountOFClosedProject = GetTotalCountOfProjects("closed");
                    var TotalAmountProject = GetTotalCostOfProjects("AllProject"); // Default all
                    var TotalCollectionProject = GetTotalCollectedCostOfProjects("AllProject"); // Default all
                    DashboardInfoObj.TotalCollectionActiveProjects = TotalCollectionProject;
                    DashboardInfoObj.PercentCollectionActiveProjects = TotalAmountProject != 0 ? (TotalCollectionProject / TotalAmountProject) * 100 : 0;


                    Response.Data = DashboardInfoObj;
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

        public BaseResponseWithId<long> DeleteGroupRole([FromHeader] long GroupId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                var Group = _unitOfWork.Groups.GetById(GroupId);
                if (Group == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Group not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
                Response.ID = Group.Id;
                var GroupUsers = _unitOfWork.GroupUsers.FindAll(a => a.GroupId == Group.Id).ToList();
                _unitOfWork.GroupUsers.DeleteRange(GroupUsers);
                _unitOfWork.Complete();

                var GroupRoles = _unitOfWork.GroupRoles.FindAll(a => a.GroupId == Group.Id).ToList();
                _unitOfWork.GroupRoles.DeleteRange(GroupRoles);
                _unitOfWork.Complete();

                _unitOfWork.Groups.Delete(Group);
                _unitOfWork.Complete();

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


        public GetJobTitleResponse GetJobTitle()
        {
            GetJobTitleResponse response = new GetJobTitleResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var JobTitleResponseList = new List<JobTitleData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var JobTitleDB = _unitOfWork.JobTitles.GetAll();


                        if (JobTitleDB != null && JobTitleDB.Count() > 0)
                        {

                            foreach (var JobTitleDBOBJ in JobTitleDB)
                            {
                                var JobTitleDBResponse = new JobTitleData();

                                JobTitleDBResponse.ID = JobTitleDBOBJ.Id;

                                JobTitleDBResponse.Name = JobTitleDBOBJ.Name;

                                JobTitleDBResponse.Description = JobTitleDBOBJ.Description;

                                JobTitleDBResponse.Active = JobTitleDBOBJ.Active;

                                JobTitleResponseList.Add(JobTitleDBResponse);
                            }

                        }

                    }

                }
                response.JobTitleList = JobTitleResponseList;
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

        public GetAddEmployeeScreenDataResponse GetAddEmployeeScreenData()
        {
            GetAddEmployeeScreenDataResponse response = new GetAddEmployeeScreenDataResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                var BranchList = GetBranches().BranchResponseList;
                var GenderList = GetGender().GenderList;
                var DepartmentList = GetDepartment(null, null).DepartmentResponseList;
                var JobTitleList = GetJobTitle().JobTitleList;






                response.BranchList = BranchList;
                response.GenderList = GenderList;
                response.DepartmentList = DepartmentList;
                response.JobTitelList = JobTitleList;



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

        public BaseResponseWithId<long> AddEditBundleModule(BundleModuleData Request, long creator)
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




                    if (Response.Result)
                    {
                        if (Request.ID != 0 && Request.ID != null)
                        {




                            var BundleModuleDB = _unitOfWork.BundleModules.GetById(Request.ID);
                            var BundleHasChildCount = _unitOfWork.BundleModules.FindAll(x => x.ParentId == Request.ID).Count();
                            if (BundleModuleDB != null)
                            {
                                // Update
                                if (BundleHasChildCount > 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Bundle Has Child , please edit the Relation";
                                    Response.Errors.Add(error);
                                }
                                else
                                {

                                    BundleModuleDB.BundleOrModuleName = Request.BundleOrModuleName;
                                    BundleModuleDB.ParentId = Request.ParentID;
                                    BundleModuleDB.Active = Request.Active;
                                    BundleModuleDB.ModifiedDate = DateTime.Now;
                                    BundleModuleDB.ModifiedBy = creator;

                                    _unitOfWork.BundleModules.Update(BundleModuleDB);

                                    var BundleModuleUpdate = _unitOfWork.Complete();
                                    if (BundleModuleUpdate > 0)
                                    {
                                        Response.Result = true;
                                        Response.ID = (long)Request.ID;
                                    }
                                    if (Request.ParentID == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Faild To Update !!";
                                        Response.Errors.Add(error);
                                    }

                                }

                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Module Doesn't Exist!!";
                                Response.Errors.Add(error);


                            }
                        }
                        else
                        {


                            // Insert
                            var bundle = new BundleModule()
                            {
                                BundleOrModuleName = Request.BundleOrModuleName,
                                ParentId = Request.ParentID,
                                Active = Request.Active,
                                CreatedBy = creator,
                                CreationDate = DateTime.Now,
                                ModifiedBy = creator,
                                ModifiedDate = DateTime.Now
                            };
                            _unitOfWork.BundleModules.Add(bundle);

                            var BundleModuleInserted = _unitOfWork.Complete();





                            if (BundleModuleInserted > 0)
                            {
                                Response.ID = bundle.Id;

                            }
                            else
                            {
                                if (Request.ParentID == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Insert this Bundle !!";
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Insert this Module !!";
                                    Response.Errors.Add(error);
                                }
                            }



                        }
                    }
                    ;




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


        public async Task<GetTermsCategoryResponse> GetTermsCategoryDDL()
        {
            GetTermsCategoryResponse response = new GetTermsCategoryResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var TermsAndCategoryDDl = new List<TermsCategoryData>();
                if (response.Result)
                {
                    if (response.Result)
                    {
                        var TermsAndCategoryDDlDB = await _unitOfWork.TermsAndConditionsCategories.GetAllAsync();
                        if (TermsAndCategoryDDlDB != null && TermsAndCategoryDDlDB.Count() > 0)
                        {

                            foreach (var TermsAndCategoryDDlOBJ in TermsAndCategoryDDlDB)
                            {
                                var GetTermsCategoryDDLResponse = new TermsCategoryData();

                                GetTermsCategoryDDLResponse.ID = TermsAndCategoryDDlOBJ.Id;

                                GetTermsCategoryDDLResponse.Name = TermsAndCategoryDDlOBJ.Name;

                                GetTermsCategoryDDLResponse.Active = TermsAndCategoryDDlOBJ.Active;

                                TermsAndCategoryDDl.Add(GetTermsCategoryDDLResponse);
                            }
                        }
                    }
                }
                response.TermsCategoryDDLList = TermsAndCategoryDDl;
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

        public BaseResponseWithId<int> AddEditTermsCategory(CategoryofTermsandConditionsData Request, long creator)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
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
                        error.ErrorMSG = "Category Name Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (Response.Result)
                    {
                        if (Request.ID != null && Request.ID != 0)
                        {
                            var CategoryofTermsandConditionsDB = _unitOfWork.TermsAndConditionsCategories.GetById((int)Request.ID);
                            if (CategoryofTermsandConditionsDB != null)
                            {
                                // Update

                                CategoryofTermsandConditionsDB.Name = Request.Name;
                                CategoryofTermsandConditionsDB.Active = Request.Active;
                                CategoryofTermsandConditionsDB.ModifiedDate = DateTime.Now;
                                CategoryofTermsandConditionsDB.ModifiedBy = creator;

                                _unitOfWork.TermsAndConditionsCategories.Update(CategoryofTermsandConditionsDB);
                                var CategoryofTermsandConditionsUpdate = _unitOfWork.Complete();


                                if (CategoryofTermsandConditionsUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = Request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Terms Category!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Terms Category Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert

                            var terms = new TermsAndConditionsCategory()
                            {
                                Name = Request.Name,
                                Active = Request.Active,
                                CreatedBy = creator,
                                ModifiedBy = creator,
                                CreationDate = DateTime.Now,
                                ModifiedDate = DateTime.Now
                            };


                            _unitOfWork.TermsAndConditionsCategories.Add(terms);
                            var TermsCategoryInsert = _unitOfWork.Complete();
                            if (TermsCategoryInsert > 0)
                            {
                                var TermsCategoryInsertID = (int)terms.Id;
                                Response.ID = TermsCategoryInsertID;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Terms Category !!";
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

        public async Task<GetSupportedByResponse> GetSupportedByList()
        {
            GetSupportedByResponse Response = new GetSupportedByResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    var SupportedByListDB = await _unitOfWork.SupportedBies.GetAllAsync();
                    var SupportedByList = new List<SelectDDL>();
                    foreach (var sb in SupportedByListDB)
                    {
                        var DLLObj = new SelectDDL();
                        DLLObj.ID = sb.Id;
                        DLLObj.Name = sb.Name;
                        SupportedByList.Add(DLLObj);
                    }
                    Response.SupportedByList = SupportedByList;
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

        public async Task<BaseResponseWithId<long>> AddEditDeleteSupportedBy(AddEditDeleteSupportedByRequest Request, long creator)
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
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (string.IsNullOrEmpty(Request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Name Is Mandatory";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    else
                    {
                        long supportedById = 0;
                        if (Request.Id != null && Request.Id != 0)
                        {
                            var SupportedByDb = await _unitOfWork.SupportedBies.GetByIdAsync((long)Request.Id);
                            if (SupportedByDb != null)
                            {
                                supportedById = (long)Request.Id;

                                if (Request.Active != null && Request.Active == false)
                                {
                                    //Delete
                                    _unitOfWork.SupportedBies.Delete(SupportedByDb);
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    //Edit
                                    SupportedByDb.Name = Request.Name;
                                    SupportedByDb.ModifiedBy = creator;
                                    SupportedByDb.ModificationDate = DateTime.Now;

                                    _unitOfWork.SupportedBies.Update(SupportedByDb);
                                    _unitOfWork.Complete();
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "This Item Not Exist!!";
                                Response.Errors.Add(error);
                                return Response;
                            }
                        }
                        else
                        {
                            //Insert
                            SupportedBy supportedBy = new SupportedBy
                            {
                                Active = true,
                                CreatedBy = creator,
                                CreationDate = DateTime.Now,
                                Name = Request.Name
                            };
                            _unitOfWork.SupportedBies.Add(supportedBy);
                            _unitOfWork.Complete();
                            supportedById = supportedBy.Id;
                        }

                        int success = _unitOfWork.Complete();
                        if (success > 0)
                        {
                            Response.Result = true;
                            Response.ID = supportedById;
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

        public BaseResponseWithId<long> DeleteArea([FromHeader] long AreaId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (AreaId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Area Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var area = _unitOfWork.Areas.GetById(AreaId);
                if (area == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Area not fpund";
                    Response.Errors.Add(error);
                    return Response;
                }
                Response.ID = area.Id;
                _unitOfWork.Areas.Delete(area);
                _unitOfWork.Complete();
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

        public BaseResponseWithId<long> UserSoftDelete([FromHeader] long UserId, [FromHeader] bool Active)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (UserId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "User Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var User = _unitOfWork.Users.GetById(UserId);
                if (User == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "User not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                User.Active = Active;
                User.Modified = DateTime.Now;
                User.ModifiedBy = validation.userID;
                Response.ID = User.Id;
                _unitOfWork.Users.Update(User);
                _unitOfWork.Complete();
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

        public BaseResponseWithId<long> AddEditRole(RoleData request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
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
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        var modifiedUser = Common.GetUserName(validation.userID, _context);
                        if (request.ID != 0 && request.ID != null)
                        {
                            var RoleDB = _unitOfWork.Roles.GetById(request.ID);
                            if (RoleDB != null)
                            {
                                RoleDB.Name = request.Name;
                                RoleDB.Description = request.Description;
                                RoleDB.Active = request.Active;
                                RoleDB.ModifiedBy = validation.userID;
                                RoleDB.ModifiedDate = DateTime.Now;
                                _unitOfWork.Roles.Update(RoleDB);
                                var RoleDBUpdate = _unitOfWork.Complete();
                                if (RoleDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = (long)request.ID;
                                    if (RoleDB.Active == false)
                                    {
                                        var userRoles = _unitOfWork.UserRoles.FindAll(a => a.RoleId == RoleDB.Id).ToList();
                                        var groupRoles = _unitOfWork.GroupRoles.FindAll(a => a.RoleId == RoleDB.Id).ToList();
                                        _unitOfWork.UserRoles.DeleteRange(userRoles);
                                        _unitOfWork.GroupRoles.DeleteRange(groupRoles);
                                        _unitOfWork.Complete();
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Role!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Role Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            var RoleID = _unitOfWork.Roles.GetMaxInt(x => x.Id);

                            Role RoleInserted = new Role
                            {
                                Id = (RoleID + 1),
                                Name = request.Name,
                                Description = request.Description,
                                CreatedBy = validation.userID,
                                CreationDate = DateTime.Now,
                                Active = request.Active
                            };

                            _unitOfWork.Roles.Add(RoleInserted);
                            _unitOfWork.Complete();
                            if (request.RoleModuleIDs != null)
                            {
                                if (request.RoleModuleIDs.Count() > 0)
                                {
                                    foreach (var RoleModule in request.RoleModuleIDs)
                                    {
                                        var roleModule = new RoleModule()
                                        {
                                            RoleId = (int)Response.ID,
                                            ModuleId = RoleModule,
                                            CreatedBy = validation.userID,
                                            CreationDate = DateTime.Now,
                                        };
                                        _unitOfWork.RoleModules.Add(roleModule);
                                        _unitOfWork.Complete();

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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithId<int>> AddInventoryStore(AddInventoryStoreData Request)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryKeeperDataList = new List<InventoryKeeperData>();
                var InventoryLocationDataList = new List<InventoryLocationData>();
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

                    int StoreID = 0;
                    long UserID = validation.userID;

                    if (Response.Result)
                    {

                        var InventoryStore = new InventoryStore()
                        {
                            Name = Request.StoreName,
                            Active = Request.Active,
                            Location = Request.Location,
                            Tel = Request.Tel,
                            CreatedBy = UserID,
                            CreationDate = DateTime.Now,
                            ModifiedBy = UserID,
                            ModifiedDate = DateTime.Now
                        };

                        _unitOfWork.InventoryStores.Add(InventoryStore);

                        // Insert
                        var InventoryStoreInsertion = _unitOfWork.Complete();



                        if (InventoryStoreInsertion > 0)
                        {
                            StoreID = (int)InventoryStore.Id;

                            Response.ID = StoreID;

                            if (Request.AddInventoryStoreKeeperData != null)
                            {
                                if (Request.AddInventoryStoreKeeperData.Count() > 0)
                                {
                                    foreach (var InventorySotreKeeper in Request.AddInventoryStoreKeeperData)
                                    {

                                        var InventorySotreKeeperInsertion = _unitOfWork.InventoryStoreKeepers.Add(new InventoryStoreKeeper()
                                        {
                                            InventoryStoreId = StoreID,
                                            UserId = InventorySotreKeeper.UserID,
                                            Active = InventorySotreKeeper.Active,
                                            CreatedBy = validation.userID,
                                            CreationDate = DateTime.Now.Date,
                                            ModifiedBy = validation.userID,
                                            ModifiedDate = DateTime.Now.Date
                                        });

                                        _unitOfWork.Complete();

                                    }
                                }
                            }


                            if (Request.AddInventoryStoreLocationData != null)
                            {
                                if (Request.AddInventoryStoreLocationData.Count() > 0)
                                {
                                    foreach (var InventorySotreLocation in Request.AddInventoryStoreLocationData)
                                    {

                                        var InventorySotreLocationInsertion = _unitOfWork.InventoryStoreLocations.Add(new InventoryStoreLocation()
                                        {
                                            InventoryStoreId = StoreID,
                                            Location = InventorySotreLocation.Location,
                                            Active = InventorySotreLocation.Active,
                                            CreatedBy = validation.userID,
                                            CreationDate = DateTime.Now.Date,
                                            ModifiedBy = validation.userID,
                                            ModifiedDate = DateTime.Now.Date
                                        });

                                        _unitOfWork.Complete();
                                    }
                                }
                            }





                        }

                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Insert this Inventory Store !!";
                            Response.Errors.Add(error);
                        }






                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Faild To Insert this Store !!";
                        Response.Errors.Add(error);
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

        public async Task<GoogleMapsRespone> MapPlaceDetails([FromHeader] string Url)
        {
            GoogleMapsRespone Response = new GoogleMapsRespone();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (string.IsNullOrEmpty(Url) || string.IsNullOrWhiteSpace(Url))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err76";
                    error.ErrorMSG = "Url Is Mandatory";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (Response.Result)
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(@"" + Url);
                        client.DefaultRequestHeaders.Accept.Clear();

                        var ApiRequest = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri(client.BaseAddress.ToString()),
                        };
                        var response = client.SendAsync(ApiRequest).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var ResponseJsonString = response.Content.ReadAsStringAsync().Result;
                            Response.ResponseBody = ResponseJsonString;
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
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<GetCostTypeResponse> GetCostType()
        {
            GetCostTypeResponse response = new GetCostTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetCostTypeResponseList = new List<CostTypeData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetCostTypeDB = await _unitOfWork.CostTypes.GetAllAsync();


                        if (GetCostTypeDB != null && GetCostTypeDB.Count() > 0)
                        {

                            foreach (var GetCostTypeDBOBJ in GetCostTypeDB)
                            {
                                var CostTypeResponse = new CostTypeData();

                                CostTypeResponse.ID = (int)GetCostTypeDBOBJ.Id;

                                CostTypeResponse.Name = GetCostTypeDBOBJ.Name;





                                GetCostTypeResponseList.Add(CostTypeResponse);
                            }



                        }

                    }

                }
                response.CostTypeList = GetCostTypeResponseList;
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

        public async Task<BaseResponseWithID> AddEditCostType(CostTypeData Request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
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
                        error.ErrorMSG = "Cost Type Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        var user = _unitOfWork.Users.GetById(validation.userID);
                        var modifiedUser = user?.FirstName + " " + user?.LastName;

                        if (Request.ID != null && Request.ID != 0)
                        {
                            var CostTypeDB = (await _unitOfWork.CostTypes.FindAllAsync(x => x.Id == Request.ID)).FirstOrDefault();
                            if (CostTypeDB != null)
                            {
                                // Update

                                if (CostTypeDB != null)
                                {
                                    CostTypeDB.Name = Request.Name;
                                    CostTypeDB.CreatedBy = validation.userID;
                                    CostTypeDB.CreationDate = DateTime.Now;
                                    _unitOfWork.Complete();
                                }


                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Cost Type!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Cost Type Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            var CostTypeDB = new CostType();


                            CostTypeDB.Name = Request.Name;
                            CostTypeDB.CreatedBy = validation.userID;
                            CostTypeDB.CreationDate = DateTime.Now;

                            _unitOfWork.CostTypes.Add(CostTypeDB);
                            var Res = _unitOfWork.Complete();
                            if (Res > 0)
                            {

                                Response.ID = CostTypeDB.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Cost Type!!";
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
    }
}
