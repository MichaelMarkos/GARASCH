using Azure;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Admin;
using NewGaras.Infrastructure.Models.Admin.Responses;
using NewGaras.Infrastructure.Models.Client;
using NewGaras.Infrastructure.Models.Mail;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.Admin;
using NewGarasAPI.Models.Admin.Responses;
using NewGarasAPI.Models.Admin.UsedInAdminResponses;
using NewGarasAPI.Models.Common;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.Project.Headers;
using NewGarasAPI.Models.Project.UsedInResponses;
using NewGarasAPI.Models.User;
using Newtonsoft.Json;
using System.Net;
using System.Web;
using Group = NewGaras.Infrastructure.Entities.Group;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IAdminService _adminService;
        public AdminController(IWebHostEnvironment host,ITenantService tenantService, IAdminService adminService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            key = "SalesGarasPass";
            _helper = new Helper.Helper();
            _host = host;
            _adminService = adminService;
        }

        [HttpGet("GetCurrencyList")]        //service Added
        public GetCurrencyResponse GetCurrencyList([FromHeader] string CompanyName = "")
        {
            GetCurrencyResponse response = new GetCurrencyResponse();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                //var CompanyName = Request.Headers["CompanyName"].ToString().ToLower();
                

                if (response.Result)
                {
                    #region old code
                    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    //var CurrencyDB = _Context.Currencies.ToList();
                    //if (CompanyName == "marinapltq")
                    //{
                    //    CurrencyDB = CurrencyDB.Where(x => x.Id == 5).ToList();
                    //}

                    //if (CurrencyDB != null && CurrencyDB.Count > 0)
                    //{

                    //    foreach (var CurrencyDBOBJ in CurrencyDB)
                    //    {
                    //        var CurrencyDBOBJResponse = new CurrencyData();

                    //        CurrencyDBOBJResponse.ID = CurrencyDBOBJ.Id;

                    //        CurrencyDBOBJResponse.CurrencyName = CurrencyDBOBJ.Name;

                    //        CurrencyDBOBJResponse.ShortCurrencyName = CurrencyDBOBJ.ShortName;

                    //        CurrencyDBOBJResponse.Active = CurrencyDBOBJ.Active;

                    //        CurrencyDBOBJResponse.IsLocal = CurrencyDBOBJ.IsLocal;


                    //        CurrencyResponseList.Add(CurrencyDBOBJResponse);
                    //    }



                    //}
                    #endregion
                    var currencyList = _adminService.GetCurrencyList(CompanyName);
                    if (!currencyList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(currencyList.Errors);
                        return response;
                    }
                    response = currencyList;
                }

                //response.CurrencyList = CurrencyResponseList;
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

        [HttpPost("AddNewCurrency")]        //service Added
        public async Task<BaseResponseWithID> AddNewCurrency(CurrencyData request)
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
                    #region old code
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
                        var loginuserName = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var CurrencyDb = await _Context.Currencies.Where(x => x.Id == request.ID).FirstOrDefaultAsync();
                            var CurrencyCheck = await _Context.Currencies.Where(x => x.IsLocal == true).FirstOrDefaultAsync();

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
                                        CurrencyDb.ModifiedBy = validation.userID.ToString();
                                        CurrencyDb.Modified = DateTime.Now;
                                        await _Context.SaveChangesAsync();
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

                            var CurrencyCheck = await _Context.Currencies.Where(x => x.IsLocal == true).FirstOrDefaultAsync();

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
                                CurrencyDb.ModifiedBy = validation.userID.ToString();
                                CurrencyDb.Modified = DateTime.Now;
                                CurrencyDb.CreatedBy = validation.userID.ToString();
                                CurrencyDb.CreationDate = DateTime.Now;
                                _Context.Currencies.Add(CurrencyDb);
                                var Res = await _Context.SaveChangesAsync();



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



                    #endregion

                    var newCurrency =await _adminService.AddNewCurrency(request, validation.userID);
                    if (!newCurrency.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(newCurrency.Errors);
                        return Response;
                    }
                    Response = newCurrency;
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

        [HttpPost("AddEditTeamIndex")]      //service Added
        public async Task<BaseResponseWithID> AddEditTeamIndex(TeamData request)
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
                    #region old code
                    ////check sent data
                    //if (Request == null)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P12";
                    //    error.ErrorMSG = "Please Insert a Valid Data.";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}

                    //if (string.IsNullOrEmpty(request.Name))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = "Team Name Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}


                    //if (Response.Result)
                    //{
                    //    var modifiedUser = Common.GetUserName(validation.userID, _Context);

                    //    if (request.ID != null && request.ID != 0)
                    //    {

                    //        var TeamDB = await _Context.Teams.Where(x => x.Id == request.ID).FirstOrDefaultAsync();

                    //        if (TeamDB != null)
                    //        {
                    //            // Update

                    //            if (TeamDB != null)
                    //            {
                    //                TeamDB.Name = request.Name;
                    //                TeamDB.Description = request.Description;
                    //                TeamDB.DepartmentId = request.DepartmentID;
                    //                TeamDB.Active = request.Active;
                    //                TeamDB.ModifiedBy = validation.userID;
                    //                TeamDB.ModifiedDate = DateTime.Now;
                    //                await _Context.SaveChangesAsync();
                    //            }
                    //            else
                    //            {
                    //                Response.Result = false;
                    //                Error error = new Error();
                    //                error.ErrorCode = "Err25";
                    //                error.ErrorMSG = "Faild To Update this Team!!";
                    //                Response.Errors.Add(error);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "This Team Doesn't Exist!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Insert
                    //        var Res = 0;

                    //        var TeamDB = new Team();

                    //        var DepartmentIDCheck = await _Context.Departments.Where(x => x.Id == request.DepartmentID).FirstOrDefaultAsync();
                    //        if (DepartmentIDCheck != null)
                    //        {

                    //            TeamDB.Name = request.Name;
                    //            TeamDB.Description = request.Description;
                    //            TeamDB.DepartmentId = request.DepartmentID;
                    //            TeamDB.Active = request.Active;
                    //            TeamDB.CreatedBy = validation.userID;
                    //            TeamDB.CreatedDate = DateTime.Now;
                    //            TeamDB.ModifiedBy = validation.userID;
                    //            TeamDB.ModifiedDate = DateTime.Now;
                    //            _Context.Teams.Add(TeamDB);
                    //            Res = await _Context.SaveChangesAsync();
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "This Department ID  Doesn't Exist!!";
                    //            Response.Errors.Add(error);
                    //        }




                    //        if (Res > 0)
                    //        {

                    //            Response.ID = TeamDB.Id;
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "Faild To Insert this Team!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }

                    #endregion

                    var teamIndex = await _adminService.AddEditTeamIndex(request, validation.userID);
                    if (!teamIndex.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(teamIndex.Errors);
                        return Response;
                    }
                    Response = teamIndex;
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

        [HttpGet("GetExpensisType")]        //service Added
        public async Task<GetExpensisTypeResponse> GetExpensisType()
        {
            GetExpensisTypeResponse response = new GetExpensisTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                var GetExpensisTypeResponseList = new List<ExpensisTypeData>();
                if (response.Result)
                {
                     #region old code
                        ////var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        //var GetExpensisTypeDB = await _Context.ExpensisTypes.ToListAsync();


                        //if (GetExpensisTypeDB != null && GetExpensisTypeDB.Count > 0)
                        //{

                        //    foreach (var GetExpensisTypeOBJ in GetExpensisTypeDB)
                        //    {
                        //        var ExpensisTypeDBResponse = new ExpensisTypeData();

                        //        ExpensisTypeDBResponse.ID = (int)GetExpensisTypeOBJ.Id;

                        //        ExpensisTypeDBResponse.ExpensisTypeName = GetExpensisTypeOBJ.ExpensisTypeName;

                        //        ExpensisTypeDBResponse.Description = GetExpensisTypeOBJ.Description;

                        //        ExpensisTypeDBResponse.Active = GetExpensisTypeOBJ.Active;

                        //        GetExpensisTypeResponseList.Add(ExpensisTypeDBResponse);
                        //    }



                        //}
                        #endregion
                    var expensisType = await _adminService.GetExpensisType();
                    if (!expensisType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(expensisType.Errors);
                        return response;
                    }
                    response = expensisType;
                    

                }
                //response.ExpensisTypeList = GetExpensisTypeResponseList;
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

        [HttpPost("AddEditExpensisType")]       //service Added
        public async Task<ActionResult<BaseResponseWithID>> AddEditExpensisType(ExpensisTypeData request)
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
                    #region old cold
                    ////check sent data
                    //if (Request == null)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P12";
                    //    error.ErrorMSG = "Please Insert a Valid Data.";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}








                    //if (string.IsNullOrEmpty(request.ExpensisTypeName))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = "Expensis Type Name Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}


                    //if (Response.Result)
                    //{
                    //    var modifiedUser = Common.GetUserName(validation.userID, _Context);

                    //    if (request.ID != null && request.ID != 0)
                    //    {
                    //        var ExpensisTypeDB = await _Context.ExpensisTypes.Where(x => x.Id == request.ID).FirstOrDefaultAsync();
                    //        if (ExpensisTypeDB != null)
                    //        {
                    //            // Update





                    //            if (ExpensisTypeDB != null)
                    //            {
                    //                ExpensisTypeDB.ExpensisTypeName = request.ExpensisTypeName;
                    //                ExpensisTypeDB.Description = request.Description;
                    //                ExpensisTypeDB.Active = request.Active;
                    //                ExpensisTypeDB.ModifiedBy = validation.userID;
                    //                ExpensisTypeDB.ModifiedDate = DateTime.Now;
                    //                await _Context.SaveChangesAsync();
                    //            }
                    //            else
                    //            {
                    //                Response.Result = false;
                    //                Error error = new Error();
                    //                error.ErrorCode = "Err25";
                    //                error.ErrorMSG = "Faild To Update this Expensis Type!!";
                    //                Response.Errors.Add(error);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "This Expensis Type Doesn't Exist!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Insert

                    //        var ExpensisTypeDB = new ExpensisType();


                    //        ExpensisTypeDB.ExpensisTypeName = request.ExpensisTypeName;
                    //        ExpensisTypeDB.Description = request.Description;
                    //        ExpensisTypeDB.CreatedBy = validation.userID;
                    //        ExpensisTypeDB.CreationDate = DateTime.Now;
                    //        ExpensisTypeDB.ModifiedBy = validation.userID;
                    //        ExpensisTypeDB.ModifiedDate = DateTime.Now;
                    //        _Context.ExpensisTypes.Add(ExpensisTypeDB);
                    //        var Res = await _Context.SaveChangesAsync();

                    //        if (Res > 0)
                    //        {

                    //            Response.ID = ExpensisTypeDB.Id;
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "Faild To Insert this Expensis Type!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }


                    //}
                    #endregion


                    var expensisType = await _adminService.AddEditExpensisType(request, validation.userID);
                    if (!expensisType.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(expensisType.Errors);
                        return Response;
                    }
                    Response = expensisType;

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

        [HttpGet("GetIncomeType")]      //service Added
        public async Task<GetIncomeTypeResponse> GetIncomeType()
        {
            GetIncomeTypeResponse response = new GetIncomeTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                var GetIncomeTypeResponseList = new List<IncomeTypeData>();
                if (response.Result)
                {

                    var incomeType = await _adminService.GetIncomeType();
                    if (!incomeType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(incomeType.Errors);
                        return response;
                    }
                    response = incomeType;


                }
                //response.IncomeTypeList = GetIncomeTypeResponseList;
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
        [HttpPost("AddEditIncomeType")]     //service Added
        public async Task<ActionResult<BaseResponseWithID>> AddEditIncomeType(IncomeTypeData request)
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

                    var incomeType = await _adminService.AddEditIncomeType(request, validation.userID);
                    if (!incomeType.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(incomeType.Errors);
                        return Response;
                    }
                    Response = incomeType;
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

        [HttpGet("GetShippingMethod")]      //service Added
        public async Task<ActionResult<GetShippingMethodResponse>> GetShippingMethod()
        {
            GetShippingMethodResponse response = new GetShippingMethodResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var incomeType = await _adminService.GetShippingMethod();
                    if (!incomeType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(incomeType.Errors);
                        return response;
                    }
                    response = incomeType;
                    
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

        [HttpPost("AddEditShippingMethod")]     //service Added
        public async Task<BaseResponseWithID> AddEditShippingMethod(ShippingMethodData request)
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

                    var shippingMethod = await _adminService.AddEditShippingMethod(request);
                    if (!shippingMethod.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(shippingMethod.Errors);
                        return Response;
                    }
                    Response = shippingMethod;

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

        [HttpGet("GetCRMContactType")]          //service Added
        public async Task<GetCRMContactTypeResponse> GetCRMContactType()
        {
            GetCRMContactTypeResponse response = new GetCRMContactTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var GetCRMContactTypeResponseList = new List<CRMContactTypeData>();
                if (response.Result)
                {
                    #region old code
                    //    if (response.Result)
                    //    {

                    //        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    //        var GetCRMContactTypeDB = await _Context.CrmcontactTypes.ToListAsync();


                    //        if (GetCRMContactTypeDB != null && GetCRMContactTypeDB.Count > 0)
                    //        {

                    //            foreach (var GetCRMContactTypeDBOBJ in GetCRMContactTypeDB)
                    //            {
                    //                var GetCRMContactTypeResponse = new CRMContactTypeData();

                    //                GetCRMContactTypeResponse.ID = (int)GetCRMContactTypeDBOBJ.Id;

                    //                GetCRMContactTypeResponse.Name = GetCRMContactTypeDBOBJ.Name;

                    //                GetCRMContactTypeResponse.Active = GetCRMContactTypeDBOBJ.Active;




                    //                GetCRMContactTypeResponseList.Add(GetCRMContactTypeResponse);
                    //            }



                    //        }

                    //    }

                    //}
                    //response.CRMContactTypeList = GetCRMContactTypeResponseList;
                    #endregion

                    var CRMContactType = await _adminService.GetCRMContactType();
                    if (!CRMContactType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(CRMContactType.Errors);
                        return response;
                    }
                    response = CRMContactType;
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

        [HttpPost("AddEditCRMContactType")]     //service Added
        public BaseResponseWithID AddEditCRMContactType(CRMContactTypeData request)
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
                    #region old code
                    ////check sent data
                    //if (Request == null)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P12";
                    //    error.ErrorMSG = "Please Insert a Valid Data.";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}
                    //if (string.IsNullOrEmpty(request.Name))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = " Name Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}


                    //if (Response.Result)
                    //{
                    //    var modifiedUser = Common.GetUserName(validation.userID, _Context);

                    //    if (request.ID != null && request.ID != 0)
                    //    {
                    //        /*                            var CRMContactTypeDB = _Context.proc_CRMContactTypeLoadByPrimaryKey(Request.ID).FirstOrDefault();
                    //        */
                    //        var CRMContactTypeDB = _Context.CrmcontactTypes.Find(request.ID);

                    //        if (CRMContactTypeDB != null)
                    //        {
                    //            // Update

                    //            CRMContactTypeDB.Name = request.Name;
                    //            CRMContactTypeDB.Active = request.Active;
                    //            CRMContactTypeDB.ModifiedDate = DateTime.Now;
                    //            CRMContactTypeDB.ModifiedBy = validation.userID;
                    //            /*var CRMContactTypeUpdate = _Context.proc_CRMContactTypeUpdate(Request.ID,
                    //                                                                               Request.Name,
                    //                                                                               Request.Active,
                    //                                                                               CRMContactTypeDB.CreationDate,
                    //                                                                               CRMContactTypeDB.CreatedBy,
                    //                                                                               //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                    //                                                                               DateTime.Now,
                    //                                                                                 validation.userID
                    //                                                                                );*/
                    //            var CRMContactTypeUpdate = _Context.SaveChanges();
                    //            if (CRMContactTypeUpdate > 0)
                    //            {
                    //                Response.Result = true;
                    //                Response.ID = request.ID ?? 0;
                    //            }
                    //            else
                    //            {
                    //                Response.Result = false;
                    //                Error error = new Error();
                    //                error.ErrorCode = "Err25";
                    //                error.ErrorMSG = "Faild To Update this CRM Contact Type!!";
                    //                Response.Errors.Add(error);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "This CRM Contact Type Doesn't Exist!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Insert
                    //        var crmInsert = new CrmcontactType()
                    //        {
                    //            Name = request.Name,
                    //            Active = request.Active,
                    //            CreationDate = DateTime.Now,
                    //            CreatedBy = validation.userID,
                    //            ModifiedDate = DateTime.Now,
                    //            ModifiedBy = validation.userID,
                    //        };

                    //        /*ObjectParameter CRMContactTypeID = new ObjectParameter("ID", typeof(int));
                    //        var CRMContactTypeInsert = _Context.proc_CRMContactTypeInsert(CRMContactTypeID,
                    //                                                                        Request.Name,
                    //                                                                        Request.Active,
                    //                                                                        DateTime.Now,
                    //                                                                        validation.userID,
                    //                                                                        null,
                    //                                                                        null
                    //                                                                       );*/

                    //        _Context.CrmcontactTypes.Add(crmInsert);
                    //        var CRMContactTypeInsert = _Context.SaveChanges();

                    //        if (CRMContactTypeInsert > 0)
                    //        {
                    //            var CRMContactTypeInsertID = long.Parse(crmInsert.Id.ToString());
                    //            Response.ID = CRMContactTypeInsertID;
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "Faild To Insert this CRM Contact Type!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }



                    //}
                    #endregion

                    var CRMContactType = _adminService.AddEditCRMContactType(request, validation.userID);
                    if (!CRMContactType.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(CRMContactType.Errors);
                        return Response;
                    }
                    Response = CRMContactType;

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

        [HttpGet("GetCRMRecievedType")]     //service Added
        public async Task<GetCRMRecievedTypeResponse> GetCRMRecievedType()
        {
            GetCRMRecievedTypeResponse response = new GetCRMRecievedTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var GetCRMRecievedTypeList = new List<CRMRecievedTypeData>();
                if (response.Result)
                {
                    #region old code
                    //if (response.Result)
                    //{

                    //    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    //    var GetCRMRecievedTypeDB = await _Context.CrmrecievedTypes.ToListAsync();


                    //    if (GetCRMRecievedTypeDB != null && GetCRMRecievedTypeDB.Count > 0)
                    //    {

                    //        foreach (var GetCRMRecievedTypeOBJ in GetCRMRecievedTypeDB)
                    //        {
                    //            var GetCRMRecievedTypeResponse = new CRMRecievedTypeData();

                    //            GetCRMRecievedTypeResponse.ID = (int)GetCRMRecievedTypeOBJ.Id;

                    //            GetCRMRecievedTypeResponse.Name = GetCRMRecievedTypeOBJ.Name;

                    //            GetCRMRecievedTypeResponse.Active = GetCRMRecievedTypeOBJ.Active;




                    //            GetCRMRecievedTypeList.Add(GetCRMRecievedTypeResponse);
                    //        }



                    //    }

                    //}
                    #endregion

                    var CRMContactType = await _adminService.GetCRMRecievedType();
                    if (!CRMContactType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(CRMContactType.Errors);
                        return response;
                    }
                    response = CRMContactType;
                }
                //response.CRMRecievedTypeList = GetCRMRecievedTypeList;
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

        [HttpPost("AddEditCRMRecievedType")]       //service Added 
        public ActionResult<BaseResponseWithID> AddEditCRMRecievedType(CRMRecievedTypeData request)
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
                        #region old code
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        //if (request.ID != null && request.ID != 0)
                        //{
                        //    var CRMRecievedTypeDB = _Context.CrmrecievedTypes.Find(request.ID);
                        //    if (CRMRecievedTypeDB != null)
                        //    {
                        //        // Update
                        //        /*var CRMRecievedTypeUpdate = _Context.proc_CRMRecievedTypeUpdate(Request.ID,
                        //                                                                           Request.Name,
                        //                                                                           Request.Active,
                        //                                                                           CRMRecievedTypeDB.CreationDate,
                        //                                                                           CRMRecievedTypeDB.CreatedBy,
                        //                                                                           //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                        //                                                                           DateTime.Now,
                        //                                                                             validation.userID
                        //                                                                            );*/
                        //        CRMRecievedTypeDB.Name = request.Name;
                        //        CRMRecievedTypeDB.Active = request.Active;
                        //        CRMRecievedTypeDB.ModifiedDate = DateTime.Now;
                        //        CRMRecievedTypeDB.ModifiedBy = validation.userID;

                        //        var CRMRecievedTypeUpdate = _Context.SaveChanges();

                        //        if (CRMRecievedTypeUpdate > 0)
                        //        {
                        //            Response.Result = true;
                        //            Response.ID = request.ID ?? 0;
                        //        }
                        //        else
                        //        {
                        //            Response.Result = false;
                        //            Error error = new Error();
                        //            error.ErrorCode = "Err25";
                        //            error.ErrorMSG = "Failed To Update this CRM Reecieved Type!!";
                        //            Response.Errors.Add(error);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        Response.Result = false;
                        //        Error error = new Error();
                        //        error.ErrorCode = "Err25";
                        //        error.ErrorMSG = "This CRM Reecieved Type Doesn't Exist!!";
                        //        Response.Errors.Add(error);
                        //    }
                        //}
                        //else
                        //{
                        //    // Insert
                        //    /*ObjectParameter CRMReecievedTypeID = new ObjectParameter("ID", typeof(int));
                        //    var CRMReecievedTypeInsert = _Context.proc_CRMRecievedTypeInsert(CRMReecievedTypeID,
                        //                                                                    Request.Name,
                        //                                                                    Request.Active,
                        //                                                                    DateTime.Now,
                        //                                                                    validation.userID,
                        //                                                                    null,
                        //                                                                    null
                        //                                                                   );*/
                        //    var CrmRecieved = new CrmrecievedType()
                        //    {
                        //        Name = request.Name,
                        //        Active = request.Active,
                        //        CreationDate = DateTime.Now,
                        //        CreatedBy = validation.userID,
                        //        ModifiedBy = validation.userID,
                        //        ModifiedDate = DateTime.Now,
                        //    };

                        //    _Context.CrmrecievedTypes.Add(CrmRecieved);

                        //    var CRMReecievedTypeInsert = _Context.SaveChanges();

                        //    if (CRMReecievedTypeInsert > 0)
                        //    {
                        //        var CRMReecievedTypeInsertID = long.Parse(CrmRecieved.Id.ToString());
                        //        Response.ID = CRMReecievedTypeInsertID;
                        //    }
                        //    else
                        //    {
                        //        Response.Result = false;
                        //        Error error = new Error();
                        //        error.ErrorCode = "Err25";
                        //        error.ErrorMSG = "Faild To Insert this CRM Recieved Type!!";
                        //        Response.Errors.Add(error);
                        //    }
                        //}

                        #endregion

                        var CrmRecivedType = _adminService.AddEditCRMRecievedType(request, validation.userID);
                        if (!CrmRecivedType.Result)
                        {
                            Response.Result = false;
                            Response.Errors.AddRange(CrmRecivedType.Errors);
                            return Response;
                        }
                        Response = CrmRecivedType;
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

        [HttpGet("GetDailyReportThrough")]          //service Added 
        public async Task<GetDailyReportThroughResponse> GetDailyReportThrough()
        {
            GetDailyReportThroughResponse response = new GetDailyReportThroughResponse();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var GetDailyReportThroughDataResponseList = new List<DailyReportThroughData>();
                if (response.Result)
                {
                    #region old code
                    //if (response.Result)
                    //{

                    //    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    //    var GetDailyReportThroughDB = await _Context.DailyReportThroughs.ToListAsync();


                    //    if (GetDailyReportThroughDB != null && GetDailyReportThroughDB.Count > 0)
                    //    {

                    //        foreach (var GetDailyReportThroughOBJ in GetDailyReportThroughDB)
                    //        {
                    //            var DailyReportThroughDataResponse = new DailyReportThroughData();

                    //            DailyReportThroughDataResponse.ID = (int)GetDailyReportThroughOBJ.Id;

                    //            DailyReportThroughDataResponse.Name = GetDailyReportThroughOBJ.Name;

                    //            DailyReportThroughDataResponse.Description = GetDailyReportThroughOBJ.Description;

                    //            DailyReportThroughDataResponse.Active = GetDailyReportThroughOBJ.Active;




                    //            GetDailyReportThroughDataResponseList.Add(DailyReportThroughDataResponse);
                    //        }



                    //    }

                    //}
                    #endregion

                    var DailyReportThrough =await _adminService.GetDailyReportThrough();
                    if (!DailyReportThrough.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(DailyReportThrough.Errors);
                        return response;
                    }
                    response = DailyReportThrough;
                }
                //response.DailyReportThroughList = GetDailyReportThroughDataResponseList;
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

        [HttpPost("AddEditDailyReportThrough")]    //service Added
        public BaseResponseWithID AddEditDailyReportThrough(DailyReportThroughData request)
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
                    #region old code
                    //check sent data
                    //if (Request == null)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P12";
                    //    error.ErrorMSG = "Please Insert a Valid Data.";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}








                    //if (string.IsNullOrEmpty(request.Name))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = "Daily Report Through Name Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}


                    //if (Response.Result)
                    //{
                    //    var modifiedUser = Common.GetUserName(validation.userID, _Context);

                    //    if (request.ID != null && request.ID != 0)
                    //    {
                    //        var DailyReportThroughDB = _Context.DailyReportThroughs.Find(request.ID);
                    //        if (DailyReportThroughDB != null)
                    //        {
                    //            // Update
                    //            /*var DailyReportThroughUpdate = _Context.proc_DailyReportThroughUpdate(Request.ID,
                    //                                                                               Request.Name,
                    //                                                                               Request.Description,
                    //                                                                               DailyReportThroughDB.CreatedBy,
                    //                                                                               DailyReportThroughDB.CreationDate,
                    //                                                                               validation.userID,
                    //                                                                               //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                    //                                                                               DateTime.Now,
                    //                                                                                Request.Active
                    //                                                                                );*/
                    //            DailyReportThroughDB.Name = request.Name;
                    //            DailyReportThroughDB.Description = request.Description;
                    //            DailyReportThroughDB.ModifedDate = DateTime.Now;
                    //            DailyReportThroughDB.ModifiedBy = validation.userID;
                    //            DailyReportThroughDB.Active = request.Active;

                    //            var DailyReportThroughUpdate = _Context.SaveChanges();

                    //            if (DailyReportThroughUpdate > 0)
                    //            {
                    //                Response.Result = true;
                    //                Response.ID = request.ID ?? 0;
                    //            }
                    //            else
                    //            {
                    //                Response.Result = false;
                    //                Error error = new Error();
                    //                error.ErrorCode = "Err25";
                    //                error.ErrorMSG = "Faild To Update this Daily Reprt Through !!";
                    //                Response.Errors.Add(error);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "This Daily Reprt Through  Doesn't Exist!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Insert
                    //        /* ObjectParameter DailyReportThroughID = new ObjectParameter("ID", typeof(long));
                    //         var DailyReportThroughInsert = _Context.proc_DailyReportThroughInsert(DailyReportThroughID,
                    //                                                                         Request.Name,
                    //                                                                         Request.Description,

                    //                                                                         validation.userID,
                    //                                                                         DateTime.Now,
                    //                                                                         null,
                    //                                                                         null,
                    //                                                                             Request.Active
                    //                                                                        );*/

                    //        var DailyReport = new DailyReportThrough()
                    //        {
                    //            Name = request.Name,
                    //            Description = request.Description,
                    //            CreatedBy = validation.userID,
                    //            CreationDate = DateTime.Now,
                    //            ModifedDate = DateTime.Now,
                    //            ModifiedBy = validation.userID,
                    //            Active = request.Active
                    //        };

                    //        _Context.DailyReportThroughs.Add(DailyReport);

                    //        var DailyReportThroughInsert = _Context.SaveChanges();

                    //        if (DailyReportThroughInsert > 0)
                    //        {
                    //            var DailyReportThroughIDInsert = long.Parse(DailyReport.Id.ToString());
                    //            Response.ID = DailyReportThroughIDInsert;
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "Faild To Insert this Daily Reprt Through!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }



                    //}
                    #endregion

                    var DailyReportThrough = _adminService.AddEditDailyReportThrough(request, validation.userID);
                    if (!DailyReportThrough.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(DailyReportThrough.Errors);
                        return Response;
                    }
                    Response = DailyReportThrough;
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



        [HttpGet("GetDeliveryAndShippingMethod")]       //service Added
        public async Task<GetDeliveryAndShippingMethodResponse> GetDeliveryAndShippingMethod()
        {
            GetDeliveryAndShippingMethodResponse response = new GetDeliveryAndShippingMethodResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var DeliveryAndShippingMethodDataList = new List<DeliveryAndShippingMethodData>();
                if (response.Result)
                {
                    #region old code
                    //if (response.Result)
                    //{

                    //    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    //    var GetDeliveryAndShippingMethodDB = await _Context.DeliveryAndShippingMethods.ToListAsync();


                    //    if (GetDeliveryAndShippingMethodDB != null && GetDeliveryAndShippingMethodDB.Count > 0)
                    //    {

                    //        foreach (var GetDeliveryAndShippingMethodDBBJ in GetDeliveryAndShippingMethodDB)
                    //        {
                    //            var GetDeliveryAndShippingMethodResponse = new DeliveryAndShippingMethodData();

                    //            GetDeliveryAndShippingMethodResponse.ID = (int)GetDeliveryAndShippingMethodDBBJ.Id;

                    //            GetDeliveryAndShippingMethodResponse.Name = GetDeliveryAndShippingMethodDBBJ.Name;

                    //            GetDeliveryAndShippingMethodResponse.Active = GetDeliveryAndShippingMethodDBBJ.Active;




                    //            DeliveryAndShippingMethodDataList.Add(GetDeliveryAndShippingMethodResponse);
                    //        }



                    //    }

                    //}
                    #endregion

                    var DeliveryAndShippingMethod =await _adminService.GetDeliveryAndShippingMethod();
                    if (!DeliveryAndShippingMethod.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(DeliveryAndShippingMethod.Errors);
                        return response;
                    }
                    response = DeliveryAndShippingMethod;
                }
                //response.DeliveryAndShippingMethodList = DeliveryAndShippingMethodDataList;
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

        [HttpPost("AddEditDeliveryAndShippingMethod")]      //service Added
        public BaseResponseWithID AddEditDeliveryAndShippingMethod(DeliveryAndShippingMethodData request)
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
                    #region old code
                    ////check sent data
                    //if (Request == null)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P12";
                    //    error.ErrorMSG = "Please Insert a Valid Data.";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}








                    //if (string.IsNullOrEmpty(request.Name))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = "Delivery and Shipping Method Name Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}



                    //if (Response.Result)
                    //{
                    //    var modifiedUser = Common.GetUserName(validation.userID, _Context);

                    //    if (request.ID != null && request.ID != 0)
                    //    {
                    //        var DeliveryAndShippingMethodDB = _Context.DeliveryAndShippingMethods.Find(request.ID);
                    //        if (DeliveryAndShippingMethodDB != null)
                    //        {
                    //            // Update
                    //            /*var DeliveryAndShippingMethodUpdate = _Context.proc_DeliveryAndShippingMethodUpdate(Request.ID,
                    //                                                                               Request.Name,
                    //                                                                                Request.Active,
                    //                                                                               DeliveryAndShippingMethodDB.CreatedBy,
                    //                                                                               DeliveryAndShippingMethodDB.CreationDate,
                    //                                                                               validation.userID,
                    //                                                                               //long.Parse(Encrypt_Decrypt.Decrypt(ModifiedBy.ToString(), key)),
                    //                                                                               DateTime.Now
                    //                                                                                );*/
                    //            DeliveryAndShippingMethodDB.Name = request.Name;
                    //            DeliveryAndShippingMethodDB.Active = request.Active;
                    //            DeliveryAndShippingMethodDB.ModifiedBy = validation.userID;
                    //            DeliveryAndShippingMethodDB.ModifiedDate = DateTime.Now;

                    //            var DeliveryAndShippingMethodUpdate = _Context.SaveChanges();

                    //            if (DeliveryAndShippingMethodUpdate > 0)
                    //            {
                    //                Response.Result = true;
                    //                Response.ID = request.ID ?? 0;
                    //            }
                    //            else
                    //            {
                    //                Response.Result = false;
                    //                Error error = new Error();
                    //                error.ErrorCode = "Err25";
                    //                error.ErrorMSG = "Faild To Update this Delivery And Shipping Method  !!";
                    //                Response.Errors.Add(error);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "This Delivery And Shipping Method Doesn't Exist!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Insert
                    //        /*ObjectParameter DeliveryAndShippingMethodID = new ObjectParameter("ID", typeof(int));
                    //        var DeliveryAndShippingMethodInsert = _Context.proc_DeliveryAndShippingMethodInsert(DeliveryAndShippingMethodID,
                    //                                                                        Request.Name,
                    //                                                                        Request.Active,
                    //                                                                        validation.userID,
                    //                                                                        DateTime.Now,
                    //                                                                        null,
                    //                                                                        null
                    //                                                                       );*/
                    //        var Delivery = new DeliveryAndShippingMethod()
                    //        {
                    //            Name = request.Name,
                    //            Active = request.Active,
                    //            CreatedBy = validation.userID,
                    //            CreationDate = DateTime.Now,
                    //            ModifiedBy = validation.userID,
                    //            ModifiedDate = DateTime.Now,
                    //        };
                    //        _Context.DeliveryAndShippingMethods.Add(Delivery);
                    //        var DeliveryAndShippingMethodInsert = _Context.SaveChanges();
                    //        if (DeliveryAndShippingMethodInsert > 0)
                    //        {
                    //            var DeliveryAndShippingMethodIDInsert = long.Parse(Delivery.Id.ToString());
                    //            Response.ID = DeliveryAndShippingMethodIDInsert;
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "Faild To Insert this Delivery And Shipping Method!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }



                    //}

                    #endregion

                    var DeliveryAndShippingMethod = _adminService.AddEditDeliveryAndShippingMethod(request, validation.userID);
                    if (!DeliveryAndShippingMethod.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(DeliveryAndShippingMethod.Errors);
                        return Response;
                    }
                    Response = DeliveryAndShippingMethod;

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

        [HttpGet("GetSalesExtraCostType")]      //service Added
        public async Task<GetSalesExtraCostTypeResponse> GetSalesExtraCostType()
        {
            GetSalesExtraCostTypeResponse response = new GetSalesExtraCostTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var SalesExtraCostTypeList = new List<SalesExtraCostTypeData>();
                if (response.Result)
                {
                    #region old code
                    //if (response.Result)
                    //{

                    //    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    //    var SalesExtraCostTypeDB = await _Context.SalesExtraCostTypes.ToListAsync();


                    //    if (SalesExtraCostTypeDB != null && SalesExtraCostTypeDB.Count > 0)
                    //    {

                    //        foreach (var SalesExtraCostTypeDBBJ in SalesExtraCostTypeDB)
                    //        {
                    //            var SalesExtraCostTypeResponse = new SalesExtraCostTypeData();

                    //            SalesExtraCostTypeResponse.ID = (int)SalesExtraCostTypeDBBJ.Id;

                    //            SalesExtraCostTypeResponse.Name = SalesExtraCostTypeDBBJ.Name;

                    //            SalesExtraCostTypeResponse.Active = SalesExtraCostTypeDBBJ.Active;




                    //            SalesExtraCostTypeList.Add(SalesExtraCostTypeResponse);
                    //        }



                    //    }

                    //}
                    #endregion

                    var SalesExtraCostType = await _adminService.GetSalesExtraCostType();
                    if (!SalesExtraCostType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(SalesExtraCostType.Errors);
                        return response;
                    }
                    response = SalesExtraCostType;
                }
                //response.SalesExtraCostTypeList = SalesExtraCostTypeList;
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

        [HttpPost("AddEditSalesExtraCostType")]         //service Added
        public BaseResponseWithID AddEditSalesExtraCostType(SalesExtraCostTypeData request)
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

                    var SalesExtraCost = _adminService.AddEditSalesExtraCostType(request);
                    if (!SalesExtraCost.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(SalesExtraCost.Errors);
                        return Response;
                    }
                    Response = SalesExtraCost;


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

        [HttpGet("GetDailyTransactionToGeneralType")]       //service Added
        public async Task<ActionResult<GetDailyTransactionToGeneralTypeResponse>> GetDailyTransactionToGeneralType()
        {
            GetDailyTransactionToGeneralTypeResponse response = new GetDailyTransactionToGeneralTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var DailyTransactionToGeneralTypeList = new List<DailyTransactionToGeneralTypeData>();
                if (response.Result)
                {

                    var DailyTransactionToGeneralType = await _adminService.GetDailyTransactionToGeneralType();
                    if (!DailyTransactionToGeneralType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(DailyTransactionToGeneralType.Errors);
                        return response;
                    }
                    response = DailyTransactionToGeneralType;


                }
                //response.DailyTransactionToGeneralTypeList = DailyTransactionToGeneralTypeList;
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

        [HttpPost("AddEditDailyTransactionToGeneralType")]      //service Added
        public ActionResult<BaseResponseWithID> AddEditDailyTransactionToGeneralType(DailyTransactionToGeneralTypeData request)
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

                    var DailyTransactionToGeneralType = _adminService.AddEditDailyTransactionToGeneralType(request,validation.userID);
                    if (!DailyTransactionToGeneralType.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(DailyTransactionToGeneralType.Errors);
                        return Response;
                    }
                    Response = DailyTransactionToGeneralType;

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

        [HttpGet("GetDailyTranactionBeneficiaryToType")]        //service Added
        public async Task<GetDailyTranactionBeneficiaryToTypeResponse> GetDailyTranactionBeneficiaryToType()
        {
            GetDailyTranactionBeneficiaryToTypeResponse response = new GetDailyTranactionBeneficiaryToTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var DailyTranactionBeneficiaryToTypeList = new List<DailyTranactionBeneficiaryToTypeData>();
                if (response.Result)
                {


                    var SalesExtraCostType = await _adminService.GetDailyTranactionBeneficiaryToType();
                    if (!SalesExtraCostType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(SalesExtraCostType.Errors);
                        return response;
                    }
                    response = SalesExtraCostType;

                }
                //response.DailyTransactionToTypeList = DailyTranactionBeneficiaryToTypeList;
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

        [HttpPost("AddEditDailyTranactionBeneficiaryToType")]   //service Added
        public BaseResponseWithID AddEditDailyTranactionBeneficiaryToType(DailyTranactionBeneficiaryToTypeData request)
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

                    var DailyTransactionToGeneralType = _adminService.AddEditDailyTranactionBeneficiaryToType(request, validation.userID);
                    if (!DailyTransactionToGeneralType.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(DailyTransactionToGeneralType.Errors);
                        return Response;
                    }
                    Response = DailyTransactionToGeneralType;


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

        [HttpGet("GetPurchasePOInvoiceTaxIncludedType")]        //service Added
        public async Task<GetPurchasePOInvoiceTaxIncludedTypeResponse> GetPurchasePOInvoiceTaxIncludedType()
        {
            GetPurchasePOInvoiceTaxIncludedTypeResponse response = new GetPurchasePOInvoiceTaxIncludedTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var PurchasePOInvoiceTaxIncludedTypeResponse = new List<PurchasePOInvoiceTaxIncludedTypeData>();
                if (response.Result)
                {

                    var PurchasePOInvoiceTaxIncludedType = await _adminService.GetPurchasePOInvoiceTaxIncludedType();
                    if (!PurchasePOInvoiceTaxIncludedType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(PurchasePOInvoiceTaxIncludedType.Errors);
                        return response;
                    }
                    response = PurchasePOInvoiceTaxIncludedType;



                }
                //response.PurchasePOInvoiceTaxIncludedTypeList = PurchasePOInvoiceTaxIncludedTypeResponse;
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

        [HttpPost("AddEditPurchasePOInvoiceTaxIncludedType")]   //service Added
        public BaseResponseWithID AddEditPurchasePOInvoiceTaxIncludedType(PurchasePOInvoiceTaxIncludedTypeData request)
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
                    #region old code
                    ////check sent data
                    //if (Request == null)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P12";
                    //    error.ErrorMSG = "Please Insert a Valid Data.";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}




                    //if (string.IsNullOrEmpty(request.Name))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = " Name Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}



                    //if (Response.Result)
                    //{
                    //    var modifiedUser = Common.GetUserName(validation.userID, _Context);

                    //    if (request.ID != null && request.ID != 0)
                    //    {
                    //        var PurchasePOInvoiceTaxIncludedTypeDB = _Context.PurchasePoinvoiceTaxIncludedTypes.Find((long)request.ID);
                    //        if (PurchasePOInvoiceTaxIncludedTypeDB != null)
                    //        {
                    //            // Update
                    //            /*var PurchasePOInvoiceTaxIncludedTypeDBUpdate = _Context.proc_PurchasePOInvoiceTaxIncludedTypeUpdate(Request.ID,
                    //                                                                               Request.Name,
                    //                                                                                Request.Active


                    //                                                                                );*/
                    //            PurchasePOInvoiceTaxIncludedTypeDB.Name = request.Name;
                    //            PurchasePOInvoiceTaxIncludedTypeDB.Active = request.Active;
                    //            var PurchasePOInvoiceTaxIncludedTypeDBUpdate = _Context.SaveChanges();
                    //            if (PurchasePOInvoiceTaxIncludedTypeDBUpdate > 0)
                    //            {
                    //                Response.Result = true;
                    //                Response.ID = request.ID ?? 0;
                    //            }
                    //            else
                    //            {
                    //                Response.Result = false;
                    //                Error error = new Error();
                    //                error.ErrorCode = "Err25";
                    //                error.ErrorMSG = "Failed To Update this Purchase PO Invoice Tax Included Type !!";
                    //                Response.Errors.Add(error);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "This Purchase PO Invoice Tax Included Type Doesn't Exist!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Insert
                    //        /*ObjectParameter PurchasePOInvoiceTaxIncludedTypeID = new ObjectParameter("ID", typeof(int));
                    //        var PurchasePOInvoiceTaxIncludedTypeInsert = _Context.proc_PurchasePOInvoiceTaxIncludedTypeInsert(PurchasePOInvoiceTaxIncludedTypeID,
                    //                                                                        Request.Name,
                    //                                                                        Request.Active



                    //                                                                       );*/
                    //        var Purchase = new PurchasePoinvoiceTaxIncludedType()
                    //        {
                    //            Name = request.Name,
                    //            Active = request.Active,
                    //        };
                    //        _Context.PurchasePoinvoiceTaxIncludedTypes.Add(Purchase);
                    //        var PurchasePOInvoiceTaxIncludedTypeInsert = _Context.SaveChanges();
                    //        if (PurchasePOInvoiceTaxIncludedTypeInsert > 0)
                    //        {
                    //            var PurchasePOInvoiceTaxIncludedTypeIDInsert = long.Parse(Purchase.Id.ToString());
                    //            Response.ID = PurchasePOInvoiceTaxIncludedTypeIDInsert;
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "Failed To Insert this Purchase PO Invoice Tax Included Type!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }



                    //}

                    #endregion

                    var PurchasePOInvoiceTaxIncludedType = _adminService.AddEditPurchasePOInvoiceTaxIncludedType(request);
                    if (!PurchasePOInvoiceTaxIncludedType.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(PurchasePOInvoiceTaxIncludedType.Errors);
                        return Response;
                    }
                    Response = PurchasePOInvoiceTaxIncludedType;

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

        [HttpGet("GetPurchasePOInvoiceNotIncludeTaxType")]     //service Added 
        public async Task<GetPurchasePOInvoiceNotIncludeTaxTypeResponse> GetPurchasePOInvoiceNotIncludeTaxType()
        {
            GetPurchasePOInvoiceNotIncludeTaxTypeResponse response = new GetPurchasePOInvoiceNotIncludeTaxTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var PurchasePOInvoiceNotIncludeTaxTypeResponseList = new List<PurchasePOInvoiceNotIncludeTaxTypeData>();
                if (response.Result)
                {
                    #region old code
                    //if (response.Result)
                    //{

                    //    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    //    var PurchasePOInvoiceNotIncludeTaxTypeDB = await _Context.PurchasePoinvoiceNotIncludedTaxTypes.ToListAsync();


                    //    if (PurchasePOInvoiceNotIncludeTaxTypeDB != null && PurchasePOInvoiceNotIncludeTaxTypeDB.Count > 0)
                    //    {

                    //        foreach (var PurchasePOInvoiceNotIncludeTaxTypeOBJ in PurchasePOInvoiceNotIncludeTaxTypeDB)
                    //        {
                    //            var PurchasePOInvoiceTaxIncludedTypeResponse = new PurchasePOInvoiceNotIncludeTaxTypeData();

                    //            PurchasePOInvoiceTaxIncludedTypeResponse.ID = (int)PurchasePOInvoiceNotIncludeTaxTypeOBJ.Id;

                    //            PurchasePOInvoiceTaxIncludedTypeResponse.Name = PurchasePOInvoiceNotIncludeTaxTypeOBJ.Name;

                    //            PurchasePOInvoiceTaxIncludedTypeResponse.Active = PurchasePOInvoiceNotIncludeTaxTypeOBJ.Active;




                    //            PurchasePOInvoiceNotIncludeTaxTypeResponseList.Add(PurchasePOInvoiceTaxIncludedTypeResponse);
                    //        }



                    //    }

                    //}

                    #endregion

                    var PurchasePOInvoiceNotIncludeTaxType = await _adminService.GetPurchasePOInvoiceNotIncludeTaxType();
                    if (!PurchasePOInvoiceNotIncludeTaxType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(PurchasePOInvoiceNotIncludeTaxType.Errors);
                        return response;
                    }
                    response = PurchasePOInvoiceNotIncludeTaxType;

                }
                //response.PurchasePOInvoiceNotIncludeTaxTypeList = PurchasePOInvoiceNotIncludeTaxTypeResponseList;
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

        [HttpPost("AddEditPurchasePOInvoiceNotIncludeTaxType")]     //service Added
        public ActionResult<BaseResponseWithID> AddEditPurchasePOInvoiceNotIncludeTaxType(PurchasePOInvoiceNotIncludeTaxTypeData request)
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
                    #region old code
                    ////check sent data
                    //if (Request == null)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P12";
                    //    error.ErrorMSG = "Please Insert a Valid Data.";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}




                    //if (string.IsNullOrEmpty(request.Name))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = " Name Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}



                    //if (Response.Result)
                    //{
                    //    var modifiedUser = Common.GetUserName(validation.userID, _Context);

                    //    if (request.ID != null && request.ID != 0)
                    //    {
                    //        var PurchasePOInvoiceNotIncludeTaxTypeDB = _Context.PurchasePoinvoiceNotIncludedTaxTypes.Find((long)request.ID);
                    //        if (PurchasePOInvoiceNotIncludeTaxTypeDB != null)
                    //        {
                    //            // Update
                    //            /* var PurchasePOInvoiceNotIncludeTaxTypeDBUpdate = _Context.proc_PurchasePOInvoiceNotIncludedTaxTypeUpdate(Request.ID,
                    //                                                                                Request.Name,
                    //                                                                                 Request.Active
                    //                                                                                 );*/
                    //            PurchasePOInvoiceNotIncludeTaxTypeDB.Name = request.Name;
                    //            PurchasePOInvoiceNotIncludeTaxTypeDB.Active = request.Active;
                    //            var PurchasePOInvoiceNotIncludeTaxTypeDBUpdate = _Context.SaveChanges();

                    //            if (PurchasePOInvoiceNotIncludeTaxTypeDBUpdate > 0)
                    //            {
                    //                Response.Result = true;
                    //                Response.ID = request.ID ?? 0;
                    //            }
                    //            else
                    //            {
                    //                Response.Result = false;
                    //                Error error = new Error();
                    //                error.ErrorCode = "Err25";
                    //                error.ErrorMSG = "Failed To Update this Purchase PO Invoice Not Include Tax Type !!";
                    //                Response.Errors.Add(error);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "This Purchase PO Invoice Not Include Tax Type Doesn't Exist!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Insert
                    //        /* ObjectParameter PurchasePOInvoiceNotIncludeTaxTypeID = new ObjectParameter("ID", typeof(int));
                    //         var PurchasePOInvoiceNotIncludeTaxTypeInsert = _Context.proc_PurchasePOInvoiceNotIncludedTaxTypeInsert(PurchasePOInvoiceNotIncludeTaxTypeID,
                    //                                                                         Request.Name,
                    //                                                                         Request.Active
                    //                                                                        );*/

                    //        var purchase = new PurchasePoinvoiceNotIncludedTaxType()
                    //        {
                    //            Name = request.Name,
                    //            Active = request.Active,
                    //        };
                    //        _Context.PurchasePoinvoiceNotIncludedTaxTypes.Add(purchase);
                    //        var PurchasePOInvoiceNotIncludeTaxTypeInsert = _Context.SaveChanges();
                    //        if (PurchasePOInvoiceNotIncludeTaxTypeInsert > 0)
                    //        {
                    //            var PurchasePOInvoiceNotIncludeTaxTypeIDInsert = long.Parse(purchase.Id.ToString());
                    //            Response.ID = PurchasePOInvoiceNotIncludeTaxTypeIDInsert;
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "Failed To Insert this Purchase PO Invoice Not Include Tax Type!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }



                    //}

                    #endregion

                    var PurchasePOInvoiceTaxIncludedType = _adminService.AddEditPurchasePOInvoiceNotIncludeTaxType(request);
                    if (!PurchasePOInvoiceTaxIncludedType.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(PurchasePOInvoiceTaxIncludedType.Errors);
                        return Response;
                    }
                    Response = PurchasePOInvoiceTaxIncludedType;


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

        [HttpGet("GetPurchasePOInvoiceExtraFeesType")]         //service Added
        public async Task<GetPurchasePOInvoiceExtraFeesTypeResponse> GetPurchasePOInvoiceExtraFeesType()
        {
            GetPurchasePOInvoiceExtraFeesTypeResponse response = new GetPurchasePOInvoiceExtraFeesTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var PurchasePOInvoiceExtraFeesType = new List<PurchasePOInvoiceExtraFeesTypeData>();
                if (response.Result)
                {
                    #region old code
                    //if (response.Result)
                    //{

                    //    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    //    var GetPurchasePOInvoiceExtraFeesTypeDB = await _Context.PurchasePoinvoiceExtraFeesTypes.ToListAsync();


                    //    if (GetPurchasePOInvoiceExtraFeesTypeDB != null && GetPurchasePOInvoiceExtraFeesTypeDB.Count > 0)
                    //    {

                    //        foreach (var GetPurchasePOInvoiceExtraFeesTypeOBJ in GetPurchasePOInvoiceExtraFeesTypeDB)
                    //        {
                    //            var GetPurchasePOInvoiceExtraFeesTypeResponse = new PurchasePOInvoiceExtraFeesTypeData();

                    //            GetPurchasePOInvoiceExtraFeesTypeResponse.ID = (int)GetPurchasePOInvoiceExtraFeesTypeOBJ.Id;

                    //            GetPurchasePOInvoiceExtraFeesTypeResponse.Name = GetPurchasePOInvoiceExtraFeesTypeOBJ.Name;

                    //            GetPurchasePOInvoiceExtraFeesTypeResponse.Active = GetPurchasePOInvoiceExtraFeesTypeOBJ.Active;




                    //            GetPurchasePOInvoiceExtraFeesTypeResponseList.Add(GetPurchasePOInvoiceExtraFeesTypeResponse);
                    //        }



                    //    }

                    //}

                    #endregion

                    var PurchasePOInvoiceExtraFeesType = await _adminService.GetPurchasePOInvoiceExtraFeesType();
                    if (!PurchasePOInvoiceExtraFeesType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(PurchasePOInvoiceExtraFeesType.Errors);
                        return response;
                    }
                    response = PurchasePOInvoiceExtraFeesType;

                }
                //response.PurchasePOInvoiceExtraFeesTypeList = PurchasePOInvoiceExtraFeesType;
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

        [HttpPost("AddEditPurchasePOInvoiceExtraFeesType")]         //service Added
        public ActionResult<BaseResponseWithID> AddEditPurchasePOInvoiceExtraFeesType(PurchasePOInvoiceExtraFeesTypeData request)
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

                    var PurchasePOInvoiceNotIncludeTaxType =  _adminService.AddEditPurchasePOInvoiceExtraFeesType(request);
                    if (!PurchasePOInvoiceNotIncludeTaxType.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(PurchasePOInvoiceNotIncludeTaxType.Errors);
                        return Response;
                    }
                    Response = PurchasePOInvoiceNotIncludeTaxType;

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

        [HttpGet("GetPurchasePOInvoiceDeductionType")]          //service Added
        public async Task<GetPurchasePOInvoiceDeductionTypeResponse> GetPurchasePOInvoiceDeductionType()
        {
            GetPurchasePOInvoiceDeductionTypeResponse response = new GetPurchasePOInvoiceDeductionTypeResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var GetPurchasePOInvoiceDeductionTypeResponseList = new List<PurchasePOInvoiceDeductionTypeData>();
                if (response.Result)
                {


                    var PurchasePOInvoiceExtraFeesType = await _adminService.GetPurchasePOInvoiceDeductionType();
                    if (!PurchasePOInvoiceExtraFeesType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(PurchasePOInvoiceExtraFeesType.Errors);
                        return response;
                    }
                    response = PurchasePOInvoiceExtraFeesType;


                }
                //response.PurchasePOInvoiceDeductionTypeList = GetPurchasePOInvoiceDeductionTypeResponseList;
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

        [HttpPost("AddEditPurchasePOInvoiceDeductionType")]     //service Added
        public BaseResponseWithID AddEditPurchasePOInvoiceDeductionType(PurchasePOInvoiceDeductionTypeData request)
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

                    var AddEditPurchasePOInvoiceDeductionType = _adminService.AddEditPurchasePOInvoiceDeductionType(request);
                    if (!AddEditPurchasePOInvoiceDeductionType.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(AddEditPurchasePOInvoiceDeductionType.Errors);
                        return Response;
                    }
                    Response = AddEditPurchasePOInvoiceDeductionType;

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

        [HttpGet("GetPurchasePaymentMethod")]           //service Added
        public async Task<GetPurchasePaymentMethodResponse> GetPurchasePaymentMethod()
        {
            GetPurchasePaymentMethodResponse response = new GetPurchasePaymentMethodResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var GetPurchasePaymentMethodResponseList = new List<PurchasePaymentMethodData>();
                if (response.Result)
                {

                    var PurchasePOInvoiceExtraFeesType = await _adminService.GetPurchasePaymentMethod();
                    if (!PurchasePOInvoiceExtraFeesType.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(PurchasePOInvoiceExtraFeesType.Errors);
                        return response;
                    }
                    response = PurchasePOInvoiceExtraFeesType;

                }
                //response.PurchasePaymentMethodList = GetPurchasePaymentMethodResponseList;
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

        [HttpPost("AddEditPurchasePaymentMethode")]     //service Added
        public BaseResponseWithID AddEditPurchasePaymentMethode(PurchasePaymentMethodData request)
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

                    var AddEditPurchasePaymentMethode = _adminService.AddEditPurchasePaymentMethode(request);
                    if (!AddEditPurchasePaymentMethode.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(AddEditPurchasePaymentMethode.Errors);
                        return Response;
                    }
                    Response = AddEditPurchasePaymentMethode;

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

        [HttpGet("GetSpecialityForClient")]     //service Added
        public async Task<GetSpecialityForClientResponse> GetSpecialityForClient()
        {
            GetSpecialityForClientResponse response = new GetSpecialityForClientResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var SpecialityForClient = new List<SpecialityForClientDataList>();
                if (response.Result)
                {

                    var SpecialityForClient = await _adminService.GetSpecialityForClient();
                    if (!SpecialityForClient.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(SpecialityForClient.Errors);
                        return response;
                    }
                    response = SpecialityForClient;


                }
                //response.SpecialityforClientList = GetClientSpecialityResponseList;
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

        [HttpPost("AddEditSpecialityforClient")]        //service Added
        public BaseResponseWithID AddEditSpecialityforClient(SpecialityForClientDataList request)
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

                    var SpecialityforClient = _adminService.AddEditSpecialityforClient(request, validation.userID);
                    if (!SpecialityforClient.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(SpecialityforClient.Errors);
                        return Response;
                    }
                    Response = SpecialityforClient;

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

        [HttpGet("GetSpecialitySupplier")]           //service Added
        public async Task<GetSpecialitySupplierResponse> GetSpecialitySupplier()
        {
            GetSpecialitySupplierResponse response = new GetSpecialitySupplierResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var GetSpecialitySupplierList = new List<SpecialitySupplierResponseDataList>();
                if (response.Result)
                {


                    var SpecialityForClient = await _adminService.GetSpecialitySupplier();
                    if (!SpecialityForClient.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(SpecialityForClient.Errors);
                        return response;
                    }
                    response = SpecialityForClient;



                }
                //response.SpecialitySupplierResponseList = GetSpecialitySupplierList;
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

        [HttpPost("AddEditSpecialitySupplier")]     //Service Added
        public BaseResponseWithID AddEditSpecialitySupplier(SpecialitySupplierResponseDataList request)
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

                    var SpecialityforClient = _adminService.AddEditSpecialitySupplier(request, validation.userID);
                    if (!SpecialityforClient.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(SpecialityforClient.Errors);
                        return Response;
                    }
                    Response = SpecialityforClient;



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

        [HttpGet("GetDepartment")]      //Service Added
        public GetDepartmentResponse GetDepartment([FromHeader] string DepartmentName, [FromHeader] int? BranchID)
        {
            GetDepartmentResponse response = new GetDepartmentResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;



                //var department = new List<DepartmentData>();
                if (response.Result)
                {
                    var department = _adminService.GetDepartment(DepartmentName, BranchID);
                    if (!department.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(department.Errors);
                        return response;
                    }
                    response = department;


                }
                //response.DepartmentResponseList = GetDepartmentList;
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

        [HttpPost("AddEditDepartment")]     //Service Added
        public BaseResponseWithID AddEditDepartment(DepartmentData request)
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

                    var department = _adminService.AddEditDepartment(request, validation.userID);
                    if (!department.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(department.Errors);
                        return Response;
                    }
                    Response = department;

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

        [HttpGet("GetBranches")]        //Service Added
        public GetBranchesResponse GetBranches()
        {
            GetBranchesResponse response = new GetBranchesResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                
                if (response.Result)
                {


                    var branches = _adminService.GetBranches();
                    if (!branches.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(branches.Errors);
                        return response;
                    }
                    response = branches;
                    return response;

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
        [HttpPost("AddEditBranches")]       //Service Added
        public BaseResponseWithID AddEditBranches(BranchData request)
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
                    #region old code
                    ////check sent data
                    //if (Request == null)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P12";
                    //    error.ErrorMSG = "Please Insert a Valid Data.";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}




                    //if (string.IsNullOrEmpty(request.Name))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = " Name Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}
                    //if (string.IsNullOrEmpty(request.Address))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err26";
                    //    error.ErrorMSG = " Address Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}
                    //long CountryID = 0;

                    //if (request.CountryID != 0)
                    //{

                    //    CountryID = (long)request.CountryID;
                    //    var CountryIDDB = _Context.Countries.Where(a => a.Id == CountryID).FirstOrDefault();

                    //    if (CountryIDDB == null)
                    //    {
                    //        Response.Result = false;
                    //        Error error = new Error();
                    //        error.ErrorCode = "Err25";
                    //        error.ErrorMSG = "Country Doesn't Exist";
                    //        Response.Errors.Add(error);
                    //    }

                    //}

                    //else
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = "Country Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}



                    //long GovernorateID = 0;

                    //if (request.GovernorateID != 0)
                    //{

                    //    GovernorateID = (long)request.GovernorateID;
                    //    var GovernorateIDDB = _Context.Governorates.Where(a => a.Id == GovernorateID).FirstOrDefault();

                    //    if (GovernorateIDDB == null)
                    //    {
                    //        Response.Result = false;
                    //        Error error = new Error();
                    //        error.ErrorCode = "Err27";
                    //        error.ErrorMSG = "Governorate Doesn't Exist";
                    //        Response.Errors.Add(error);
                    //    }

                    //}

                    //else
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err28";
                    //    error.ErrorMSG = "Governorate Is Mandatory";
                    //    Response.Errors.Add(error);
                    //}


                    //if (Response.Result)
                    //{
                    //    var modifiedUser = Common.GetUserName(validation.userID, _Context);

                    //    if (request.ID != null && request.ID != 0)
                    //    {
                    //        var BranchDB = _Context.Branches.Find((int)request.ID);
                    //        if (BranchDB != null)
                    //        {
                    //            // Update
                    //            /*var BranchDBUpdate = _Context.proc_BranchUpdate(Request.ID,
                    //                                                                               Request.Name,
                    //                                                                               Request.Description,
                    //                                                                               Request.Address,
                    //                                                                               Request.Tel,
                    //                                                                               Request.Fax,
                    //                                                                               Request.Email,
                    //                                                                               Request.Active,
                    //                                                                               BranchDB.CreatedBy,
                    //                                                                               BranchDB.CreationDate,
                    //                                                                               validation.userID,
                    //                                                                               DateTime.Now,
                    //                                                                               Request.CountryID,
                    //                                                                               Request.GovernorateID
                    //                                                                                );*/
                    //            BranchDB.Name = request.Name;
                    //            BranchDB.Description = request.Description;
                    //            BranchDB.Address = request.Address;
                    //            BranchDB.Telephone = request.Tel;
                    //            BranchDB.Fax = request.Fax;
                    //            BranchDB.Email = request.Email;
                    //            BranchDB.Active = request.Active;
                    //            BranchDB.CountryId = request.CountryID;
                    //            BranchDB.GovernorateId = request.GovernorateID;
                    //            BranchDB.ModifiedDate = DateTime.Now;
                    //            BranchDB.ModifiedBy = validation.userID;
                    //            var BranchDBUpdate = _Context.SaveChanges();
                    //            if (BranchDBUpdate > 0)
                    //            {
                    //                Response.Result = true;
                    //                Response.ID = request.ID ?? 0;
                    //            }
                    //            else
                    //            {
                    //                Response.Result = false;
                    //                Error error = new Error();
                    //                error.ErrorCode = "Err25";
                    //                error.ErrorMSG = "Failed To Update this Branch !!";
                    //                Response.Errors.Add(error);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "This Branch  Doesn't Exist!!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Insert
                    //        /*ObjectParameter BranchID = new ObjectParameter("ID", typeof(int));
                    //        var BranchIDInsert = _Context.proc_BranchInsert(BranchID,
                    //                                                                        Request.Name,
                    //                                                                        Request.Description,
                    //                                                                        Request.Address,
                    //                                                                        Request.Tel,
                    //                                                                        Request.Fax,
                    //                                                                        Request.Email,
                    //                                                                        Request.Active,
                    //                                                                        validation.userID,
                    //                                                                        DateTime.Now,
                    //                                                                        null,
                    //                                                                        null,
                    //                                                                        Request.CountryID,
                    //                                                                        Request.GovernorateID
                    //                                                                       );*/
                    //        var branch = new Branch()
                    //        {
                    //            Name = request.Name,
                    //            Description = request.Description,
                    //            Address = request.Address,
                    //            Telephone = request.Tel,
                    //            Fax = request.Fax,
                    //            Email = request.Email,
                    //            Active = request.Active,
                    //            CountryId = request.CountryID,
                    //            GovernorateId = request.GovernorateID,
                    //            ModifiedDate = DateTime.Now,
                    //            ModifiedBy = validation.userID,
                    //            CreationDate = DateTime.Now,
                    //            CreatedBy = validation.userID,
                    //        };
                    //        _Context.Branches.Add(branch);
                    //        var BranchIDInsert = _Context.SaveChanges();

                    //        if (BranchIDInsert > 0)
                    //        {
                    //            var BranchInsert = long.Parse(branch.Id.ToString());
                    //            Response.ID = BranchInsert;
                    //        }
                    //        else
                    //        {
                    //            Response.Result = false;
                    //            Error error = new Error();
                    //            error.ErrorCode = "Err25";
                    //            error.ErrorMSG = "Failed To Insert this Branch !!";
                    //            Response.Errors.Add(error);
                    //        }
                    //    }



                    //}
                    #endregion
                    var Branches = _adminService.AddEditBranches(request, validation.userID);
                    if (!Branches.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(Branches.Errors);
                        return Response;
                    }
                    Response = Branches;

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

        [HttpGet("GetTermsAndConditions")]      //Service Added
        public async Task<GetTermsAndConditionsResponse> GetTermsAndConditions()
        {
            GetTermsAndConditionsResponse response = new GetTermsAndConditionsResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var GetTermsAndConditionsList = new List<TermsAndConditionsData>();
                if (response.Result)
                {


                    var termsAndConditions = await _adminService.GetTermsAndConditions();
                    if (!termsAndConditions.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(termsAndConditions.Errors);
                        return response;
                    }
                    return response;
                }
                //response.TermsAndConditionsList = GetTermsAndConditionsList;
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

        [HttpPost("AddEditTermsAndConditions")]     //Service Added
        public BaseResponseWithID AddEditTermsAndConditions(TermsAndConditionsData request)
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
                    var TermsAndConditions = _adminService.AddEditTermsAndConditions(request, validation.userID);
                    if(!TermsAndConditions.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(TermsAndConditions.Errors);
                        return Response;
                    }
                    return Response;
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

        //[HttpGet("GetAreasList")]       //Service Added
        //public async Task<SelectDDLResponse> GetAreasList()
        //{
        //    SelectDDLResponse response = new SelectDDLResponse();
        //    response.Result = true;
        //    response.Errors = new List<Error>();



        //    try
        //    {

        //        //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        //        //WebHeaderCollection headers = request.Headers;
        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //        response.Errors = validation.errors;
        //        response.Result = validation.result;



        //        if (response.Result)
        //        {


        //            var areasList = await _adminService.GetAreasList(GovernorateId);
        //            if (!areasList.Result)
        //            {
        //                response.Result = false;
        //                response.Errors.AddRange(areasList.Errors);
        //                return response;
        //            }
        //            response = areasList;
        //            return response;

        //        }



        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.InnerException.Message;
        //        response.Errors.Add(error);

        //        return response;
        //    }

        //}

        [HttpPost("AddEditArea")]       //Service Added
        public BaseResponseWithID AddEditArea(AreaData request)
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

                    _adminService.Validation = validation;
                    var area = _adminService.AddEditArea(request);
                    if (!area.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(area.Errors);
                        return Response;
                    }
                    Response = area;
                    return Response;

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

        [HttpPost("AddEditGeographicalName")]       //Service Added
        public BaseResponseWithID AddEditGeographicalName(GeographicalNameData request)
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

                    _adminService.Validation = validation;
                    var area = _adminService.AddEditGeographicalName(request);
                    if (!area.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(area.Errors);
                        return Response;
                    }
                    Response = area;
                    return Response;

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
        [HttpPost("AddEditCountry")]        //Service Added
        public BaseResponseWithID AddEditCountry(CountryData request)
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
                    _adminService.Validation = validation;
                    var country = _adminService.AddEditCountry(request);
                    if (!country.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(country.Errors);
                        return Response;
                    }
                    Response = country;
                    return Response;
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

        [HttpPost("AddEditGovernorate")]       //under testing
        public BaseResponseWithID AddEditGovernorate(GovernorateData request)
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
                    _adminService.Validation = validation;
                    var Governorate = _adminService.AddEditGovernorate(request);
                    if (!Governorate.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(Governorate.Errors);
                        return Response;
                    }

                    Response = Governorate;
                    return Response;
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

        [HttpPost("AddEditCity")]       //under testing
        public BaseResponseWithID AddEditCity(CityData request)
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
                    _adminService.Validation = validation;
                    var city = _adminService.AddEditCity(request);
                    if (!city.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(city.Errors);
                        return Response;
                    }

                    Response = city;
                    return Response;
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

        [HttpPost("AddEditDistrict")]       //under testing
        public BaseResponseWithID AddEditDistrict(DistrictData request)
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
                    _adminService.Validation = validation;
                    var district = _adminService.AddEditDistrict(request);
                    if (!district.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(district.Errors);
                        return Response;
                    }

                    Response = district;
                    return Response;
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

        //[HttpGet("GetCountryGovernorateArea")]      //Service Added
        //public async Task<GetCountryGovernorateAreaResponse> GetCountryGovernorateArea([FromHeader] bool allData = true)
        //{
        //    GetCountryGovernorateAreaResponse response = new GetCountryGovernorateAreaResponse();
        //    response.Result = true;
        //    response.Errors = new List<Error>();



        //    try
        //    {

        //        //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        //        //WebHeaderCollection headers = request.Headers;
        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //        response.Errors = validation.errors;
        //        response.Result = validation.result;

        //        //var GetCountryGovernorateAreaResponseList = new List<TreeViewCountr>();
        //        /*bool allData = true;
        //        if (!string.IsNullOrEmpty(headers["allData"]) && bool.TryParse(headers["allData"], out allData))
        //        {
        //            bool.TryParse(headers["allData"], out allData);
        //        }*/




        //        if (response.Result)
        //        {
        //            #region old code
        //            //var ClientAddressList = await _Context.ClientAddresses.Where(x => x.Active == true).ToListAsync();

        //            //var Countries = await _Context.Countries.ToListAsync();

        //            //var TreeDtoObj = Countries.Select(c => new TreeViewCountr
        //            //{
        //            //    id = "Country-" + c.Id.ToString(),
        //            //    title = c.Name,
        //            //    parentId = "",
        //            //    CountOfClient = ClientAddressList.Where(x => x.CountryId == c.Id).Select(x => x.ClientId).Distinct().Count()
        //            //}).ToList();

        //            //var Govenorates = await _Context.Governorates.ToListAsync();
        //            //var GovernorateDto = Govenorates.Select(c => new TreeViewCountr
        //            //{
        //            //    id = "Governorate-" + c.Id.ToString(),
        //            //    title = c.Name,
        //            //    parentId = "Country-" + c.CountryId.ToString(),
        //            //    CountOfClient = ClientAddressList.Where(x => x.GovernorateId == c.Id).Select(x => x.ClientId).Distinct().Count()
        //            //}).ToList();

        //            //TreeDtoObj.AddRange(GovernorateDto);


        //            //var Areas = await _Context.Areas.ToListAsync();
        //            //var AreasDto = Areas.Select(c => new TreeViewCountr
        //            //{
        //            //    id = "Area-" + c.Id.ToString(),
        //            //    title = c.Name,
        //            //    parentId = "Governorate-" + c.GovernorateId.ToString()
        //            //    ,
        //            //    CountOfClient = ClientAddressList.Where(x => x.AreaId == c.Id).Select(x => x.ClientId).Distinct().Count()
        //            //}).ToList();

        //            //TreeDtoObj.AddRange(AreasDto);
        //            //if (!allData)
        //            //{
        //            //    TreeDtoObj = TreeDtoObj.Where(x => x.CountOfClient > 0).ToList();

        //            //}
        //            //var trees = Common.BuildTreeViews("", TreeDtoObj);
        //            //response.GetCountryGovernorateAreaResponseList = trees;
        //            #endregion
        //            var CountryGovernorateArea =await _adminService.GetCountryGovernorateArea(allData);
        //            if (!CountryGovernorateArea.Result)
        //            {
        //                response.Result = false;
        //                response.Errors.AddRange(CountryGovernorateArea.Errors);
        //                return response;
        //            }
        //            response = CountryGovernorateArea;
        //            return response;
        //        }

        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.InnerException.Message;
        //        response.Errors.Add(error);

        //        return response;
        //    }

        //}

        [HttpPost("AddEditRole")]        //Service Added
        public BaseResponseWithId<long> AddEditRole(RoleData request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    _adminService.Validation = validation;
                    Response = _adminService.AddEditRole(request);
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


        [HttpGet("GetRole")]        //service Added
        public async Task<GetRoleResponse> GetRole()
        {
            GetRoleResponse response = new GetRoleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                //var GetRoleList = new List<RoleData>();




                if (response.Result)
                {

                    var Role = await _adminService.GetRole();
                    if (!Role.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(Role.Errors);
                        return response;
                    }
                    response = Role;
                    return response;

                }


                //response.RoleDataList = GetRoleList;
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

        [HttpPost("AddGroupRole")]      //Service Added
        public BaseResponseWithID AddGroupRole(AddGroupRoleData request)
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

                    var GroupRole = _adminService.AddGroupRole(request, validation.userID);
                    if (!GroupRole.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(GroupRole.Errors);
                        return Response;
                    }

                    return Response;

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

        [HttpPost("EditGroupRole")]     //Service Added
        public BaseResponseWithID EditGroupRole(EditGroupData request)
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

                #region old code
                //if (Response.Result)
                //{

                //    //check sent data
                //    if (Request == null)
                //    {
                //        Response.Result = false;
                //        Error error = new Error();
                //        error.ErrorCode = "Err-P12";
                //        error.ErrorMSG = "Please Insert a Valid Data.";
                //        Response.Errors.Add(error);
                //        return Response;
                //    }
                //    if (request.GroupID == null)
                //    {
                //        Response.Result = false;
                //        Error error = new Error();
                //        error.ErrorCode = "Err-P12";
                //        error.ErrorMSG = "Please Insert a Group ID.";
                //        Response.Errors.Add(error);
                //        return Response;
                //    }

                //    int counter = 0;
                //    int GroupRolecounter = 0;

                //    var GroupUserDB = _Context.GroupUsers.Where(x => x.GroupId == request.GroupID).ToList();
                //    var GroupRoleDB = _Context.GroupRoles.Where(x => x.GroupId == request.GroupID).ToList();

                //    if (Response.Result)
                //    {
                //        var modifiedUser = Common.GetUserName(validation.userID, _Context);

                //        if (request.GroupID != null && request.GroupID != 0)
                //        {
                //            var GroupDB = _Context.Groups.Find(request.GroupID);
                //            if (GroupDB != null)
                //            {
                //                // Update
                //                /*var GroupUpdate = _Context.proc_GroupUpdate(Request.GroupID,
                //                                                                                   Request.Name,
                //                                                                                   Request.Description,
                //                                                                                   Request.Active,
                //                                                                                   GroupDB.CreatedBy,
                //                                                                                    GroupDB.CreationDate,
                //                                                                                   validation.userID,
                //                                                                                   DateTime.Now
                //                                                                                    );*/
                //                GroupDB.Name = request.Name;
                //                GroupDB.Description = request.Description;
                //                GroupDB.Active = (bool)request.Active;
                //                GroupDB.ModifiedBy = validation.userID;
                //                GroupDB.ModifiedDate = DateTime.Now;
                //                var GroupUpdate = _Context.SaveChanges();
                //                if (GroupUpdate > 0)
                //                {
                //                    Response.Result = true;

                //                    if (request.UserID != null)
                //                    {

                //                        if (GroupUserDB.Count() < request.UserID.Count())
                //                        {
                //                            foreach (var GroupUser in request.UserID)
                //                            {

                //                                if (!GroupUserDB.Select(x => x.UserId).ToList().Contains(GroupUser))
                //                                {

                //                                    /*ObjectParameter GroupUserID = new ObjectParameter("ID", typeof(long));
                //                                    var GroupUserInsertion = _Context.proc_Group_UserInsert(GroupUserID,
                //                                       Request.GroupID,
                //                                       (int)GroupUser,
                //                                       validation.userID,
                //                                       DateTime.Now.Date,
                //                                       true
                //                                        );*/
                //                                    var Groupuser = new GroupUser()
                //                                    {
                //                                        GroupId = (long)request.GroupID,
                //                                        UserId = (int)GroupUser,
                //                                        //HrUserId = GroupUser,
                //                                        CreatedBy = validation.userID,
                //                                        CreationDate = DateTime.Now,
                //                                    };
                //                                    _Context.GroupUsers.Add(Groupuser);
                //                                    _Context.SaveChanges();
                //                                }
                //                            }


                //                        }
                //                        GroupUserDB = _Context.GroupUsers.Where(x => x.GroupId == request.GroupID).ToList();
                //                        if (GroupUserDB.Count() > request.UserID.Count())
                //                        {


                //                            var GroupUserListDB = _Context.GroupUsers.Where(x => x.GroupId == request.GroupID).ToList();


                //                            if (request.UserID != null)
                //                            {
                //                                if (request.UserID.Count() >= 0)
                //                                {

                //                                    var UsersIDSForDelete = GroupUserDB.Where(x=>x.UserId !=null).Select(x => (long)x.UserId).ToList().Except(request.UserID.ToList());

                //                                    if (UsersIDSForDelete.Count() > 0)
                //                                    {

                //                                        foreach (var deletedUserId in UsersIDSForDelete)
                //                                        {
                //                                            var deletedId = GroupUserListDB.Where(x => x.UserId == deletedUserId).FirstOrDefault();
                //                                            var UserDeletion = _Context.GroupUsers.Remove(deletedId);
                //                                        }
                //                                    }

                //                                }
                //                            }





                //                        }
                //                        GroupUserDB = _Context.GroupUsers.Where(x => x.GroupId == request.GroupID).ToList();
                //                        if (GroupUserDB.Count() == request.UserID.Count())
                //                        {
                //                            foreach (var GroupUser in request.UserID)
                //                            {
                //                                var ID = GroupUserDB[counter].Id;

                //                                /*var GroupUserUpdate = _Context.proc_Group_UserUpdate(ID,
                //                                     Request.GroupID,
                //                                     (int)GroupUser,
                //                                     validation.userID,
                //                                     DateTime.Now.Date,
                //                                     true


                //                              );*/
                //                                GroupUserDB[counter].GroupId = (long)request.GroupID;
                //                                GroupUserDB[counter].UserId = GroupUser;
                //                                GroupUserDB[counter].Active = true;
                //                                _Context.SaveChanges();
                //                                counter++;
                //                            }
                //                        };
                //                    }



                //                    counter = 0;

                //                    if (request.RoleID != null)
                //                    {
                //                        //Group Role Count < Requested Role ID
                //                        if (GroupRoleDB.Count() < request.RoleID.Count())
                //                        {
                //                            foreach (var GroupRole in request.RoleID)
                //                            {

                //                                if (!GroupRoleDB.Select(x => x.RoleId).ToList().Contains(GroupRole))
                //                                {

                //                                    /*ObjectParameter GroupRoleID = new ObjectParameter("ID", typeof(long));
                //                                    var GroupRoleInsertion = _Context.proc_GroupRoleInsert(GroupRoleID,
                //                                       (int)GroupRole,
                //                                       Request.GroupID,
                //                                       validation.userID,
                //                                       DateTime.Now.Date

                //                                        );*/
                //                                    var Grouperole = new GroupRole()
                //                                    {
                //                                        RoleId = (int)GroupRole,
                //                                        GroupId = (long)request.GroupID,
                //                                        CreatedBy = validation.userID,
                //                                        CreationDate = DateTime.Now
                //                                    };
                //                                    _Context.GroupRoles.Add(Grouperole); _Context.SaveChanges();
                //                                }
                //                            }
                //                        }
                //                        //END

                //                        //Group Role Count > Requested Role ID
                //                        GroupRoleDB = _Context.GroupRoles.Where(x => x.GroupId == (long)request.GroupID).ToList();
                //                        if (GroupRoleDB.Count() > request.RoleID.Count())
                //                        {


                //                            var GroupRoleListDB = _Context.GroupRoles.Where(x => x.GroupId == request.GroupID).ToList();


                //                            if (request.RoleID != null)
                //                            {
                //                                if (request.RoleID.Count() > 0)
                //                                {
                //                                    //var GroupUserList = Request.EditGroupUser.Where(x => x.GroupID != null).Select(x => x.GroupID).ToList();

                //                                    //var DeleteIds = GroupUserListDB.Where(db => GroupUserList.All(nw => nw != db)).ToArray();

                //                                    var RolesIDSForDelete = GroupRoleDB.Select(x => x.RoleId).ToList().Except(request.RoleID.ToList());

                //                                    if (RolesIDSForDelete.Count() > 0)
                //                                    {

                //                                        foreach (var deletedRoleId in RolesIDSForDelete)
                //                                        {
                //                                            var deletedId = GroupRoleListDB.Where(x => x.RoleId == deletedRoleId).FirstOrDefault();
                //                                            var RoleDeletion = _Context.GroupRoles.Remove(deletedId);
                //                                        }
                //                                    }

                //                                }
                //                            }



                //                        }
                //                        //End


                //                        //Group Role Count == Requested Role ID
                //                        GroupRoleDB = _Context.GroupRoles.Where(x => x.GroupId == (long)request.GroupID).ToList();
                //                        if (GroupRoleDB.Count() == request.RoleID.Count())
                //                        {
                //                            foreach (var GroupRole in request.RoleID)
                //                            {
                //                                var RoleID = GroupRoleDB[counter].Id;

                //                                /*var GroupRoleUpdate = _Context.proc_GroupRoleUpdate(RoleID,
                //                                      (int)GroupRole,
                //                                     Request.GroupID,
                //                                     validation.userID,
                //                                     DateTime.Now.Date

                //                              );*/
                //                                GroupRoleDB[counter].RoleId = (int)GroupRole;
                //                                GroupRoleDB[counter].GroupId = (long)request.GroupID;
                //                                _Context.SaveChanges();
                //                                counter++;
                //                            }
                //                        };
                //                        //End
                //                    }

                //                }
                //                else
                //                {
                //                    Response.Result = false;
                //                    Error error = new Error();
                //                    error.ErrorCode = "Err25";
                //                    error.ErrorMSG = "Faild To Update this Group Role!!";
                //                    Response.Errors.Add(error);
                //                }
                //            }
                //            else
                //            {
                //                Response.Result = false;
                //                Error error = new Error();
                //                error.ErrorCode = "Err25";
                //                error.ErrorMSG = "This Group Role Doesn't Exist!!";
                //                Response.Errors.Add(error);
                //            }



                //        }




                //    }




                //}

                #endregion

                if(Response.Result)
                {
                    var GroupRole = _adminService.EditGroupRole(request, validation.userID);
                    if (!GroupRole.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(GroupRole.Errors);
                        return Response;
                    }

                    return Response;
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
        [HttpPost("AddEditGroup")]      //Service Added
        public BaseResponseWithID AddEditGroup(GroupData request)
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

                    var Group = _adminService.AddEditGroup(request, validation.userID);
                    if (!Group.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(Group.Errors);
                        return Response;
                    }

                    Response = Group;
                    return Response;


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

        [HttpGet("GetGroupDetails")]        //service Added
        public async Task<GetGroupRoleResponse> GetGroupDetails([FromHeader] long GroupID = 0)
        {
            GetGroupRoleResponse response = new GetGroupRoleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                /*long GroupID = 0;
                if (!string.IsNullOrEmpty(headers["GroupID"]) && long.TryParse(headers["GroupID"], out GroupID))
                {
                    long.TryParse(headers["GroupID"], out GroupID);
                }*/

                if (response.Result)
                {

                    var group = await _adminService.GetGroupDetails(GroupID);
                    if (!group.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(group.Errors);
                        return response;
                    }
                    response = group;
                    return response;

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

        [HttpGet("GetGender")]      //service Added
        public GetGenderResponse GetGender()
        {
            GetGenderResponse response = new GetGenderResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                var GetGenderResponseList = new List<GenderData>();




                if (response.Result)
                {

                    var gender =  _adminService.GetGender();
                    if (!gender.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(gender.Errors);
                        return response;
                    }
                    response = gender;
                    return response;

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

        [HttpGet("GetImportantDateList")]       //service Added
        public async Task<GetImportantDateResponse> GetImportantDateList([FromHeader] int ImpDateId = 0)
        {
            GetImportantDateResponse response = new GetImportantDateResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string baseURL = MyConfig.GetValue<string>("AppSettings:baseURL");



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                /*                int ImpDateId = 0;
                                if (!string.IsNullOrEmpty(headers["ImpDateId"]) && int.TryParse(headers["ImpDateId"], out ImpDateId))
                                {
                                    int.TryParse(headers["ImpDateId"], out ImpDateId);
                                }*/
                
                if (response.Result)
                {
                    var ImportantDateList = await _adminService.GetImportantDateList();
                    if (!ImportantDateList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(ImportantDateList.Errors);
                        return response;
                    }
                    response = ImportantDateList;
                    return response;

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
        [HttpPost("AddImportantDate")]      //service Added
        public async Task<BaseResponseWithID> AddImportantDate(AddImportantDateRequest request)
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
                    var ImportantDate = await _adminService.AddImportantDate(request, validation.userID, validation.CompanyName);
                    if (!ImportantDate.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(ImportantDate.Errors);
                        return Response;
                    }
                    Response = ImportantDate;
                    return Response;
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

        [HttpGet("GetRoleModule")]       //service Added
        public GetRoleModuleResponse GetRoleModule([FromHeader] long ModuleID = 0)
        {
            GetRoleModuleResponse response = new GetRoleModuleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                


                /*long ModuleID = 0;
                if (!string.IsNullOrEmpty(headers["ModuleID"]) && long.TryParse(headers["ModuleID"], out ModuleID))
                {
                    long.TryParse(headers["ModuleID"], out ModuleID);
                }*/

                if (response.Result)
                {

                    var roleModule =  _adminService.GetRoleModule();
                    if (!roleModule.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(roleModule.Errors);
                        return response;
                    }
                    response = roleModule;
                    return response;

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

        [HttpPost("AddEditRoleModule")]     //service Added
        public BaseResponseWithID AddEditRoleModule(RoleModuleData request)
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
                    var RoleModule =  _adminService.AddEditRoleModule(request, validation.userID);
                    if (!RoleModule.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(RoleModule.Errors);
                        return Response;
                    }
                    Response = RoleModule;
                    return Response;
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

        [HttpGet("GetModule")]      //service Added
        public async Task<GetModuleResponse> GetModule()
        {
            GetModuleResponse response = new GetModuleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {

                    var module = await _adminService.GetModule();
                    if (!module.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(module.Errors);
                        return response;
                    }
                    response = module;
                    return response;

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

        /*        [HttpGet("GetBundleModule")]
        public async Task<ActionResult<GetBundleModuleResponse>> GetBundleModule()
        {
            GetBundleModuleResponse response = new GetBundleModuleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                var GetVehicleBrandPerModelList = new List<TreeView>();





                if (response.Result)
                {

                    var BundleModule = await _Context.BundleModules.ToListAsync();
                    var TreeDtoObj = BundleModule.Select(c => new TreeViewDto
                    {
                        id = c.Id.ToString(),
                        title = c.BundleOrModuleName,
                        parentId = c.ParentId.ToString()
                    }).ToList();




                    //TreeDtoObj.AddRange(modelsDto);

                    var trees = Common.BuildTreeViews("", TreeDtoObj);
                    response.GetBundleModuleList = trees;
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

        }*/

        [HttpGet("GetTax")]         //service Added
        public async Task<GetTaxResponse> GetTax()
        {
            GetTaxResponse response = new GetTaxResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                
                if (response.Result)
                {
                    var tax = await _adminService.GetTax();
                    if (!tax.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(tax.Errors);
                        return response;
                    }
                    response = tax;
                    return response;
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

        [HttpPost("AddEditTax")]        //service Added
        public BaseResponseWithID AddEditTax(TaxData request)
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

                    var tax = _adminService.AddEditTax(request, validation.userID);
                    if (!tax.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(tax.Errors);
                        return Response;
                    }
                    Response = tax;
                    return Response;


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

        [HttpGet("GetDBTablesName")]        //service Added
        public GetDBTablesNameResponse GetDBTablesName()
        {
            GetDBTablesNameResponse response = new GetDBTablesNameResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;


                if (response.Result)
                {
                    var DBTablesName = _adminService.GetDBTablesName();
                    if (!DBTablesName.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(DBTablesName.Errors);
                        return response;
                    }
                    response = DBTablesName;
                    return response;
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
        [HttpGet("GetTablesCloumns")]       //service Added
        public GetTablesCloumnsResponse GetTablesCloumns()
        {
            GetTablesCloumnsResponse response = new GetTablesCloumnsResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                var GetDBTablesNameDataList = new List<GetDBTablesNameData>();




                if (response.Result)
                {

                    var tablesCloumns = _adminService.GetTablesCloumns();
                    if (!tablesCloumns.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(tablesCloumns.Errors);
                        return response;
                    }
                    response = tablesCloumns;
                    return response;
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

        #region method try
        //[HttpGet("GetUserListDDL")]
        //public GetUserListDDLResponse GetUserListDDL()
        //{
        //    GetUserListDDLResponse response = new GetUserListDDLResponse()
        //    {
        //        result = true, 
        //        errors = new List<Error>()
        //    };

        //    #region user Auth
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //    response.errors = validation.errors;
        //    response.result = validation.result;
        //    #endregion
        //    try
        //    {
        //        if (response.result)
        //        {
        //            var usersListQuery = _Context.Users.ToList();

        //            List<MiniUserDDL> usersList = new List<MiniUserDDL>();

        //            foreach (var user in usersListQuery)
        //            {
        //                MiniUserDDL tempUser = new MiniUserDDL();

        //                tempUser.Id = user.Id;
        //                tempUser.Name = user.FirstName + " " + user.MiddleName + " " + user.LastName;

        //                usersList.Add(tempUser);
        //            }
        //            response.UsersList = usersList;
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.InnerException.Message;
        //        response.errors.Add(error);

        //        return response;
        //    }
        //}
        #endregion

        [HttpGet("GetUserListDDL")]         //Service Added
        public GetUserListDDLResponse GetUserListDDL([FromHeader] int GroupId, [FromHeader] long projectId)
        {
            GetUserListDDLResponse response = new GetUserListDDLResponse()
            {
                result = true,
                errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.errors = validation.errors;
            response.result = validation.result;
            #endregion

            try
            {
                if (response.result)
                {
                    var userDDL = _adminService.GetUserListDDL(GroupId, projectId);
                    if (!userDDL.result)
                    {
                        response.result = false;
                        response.errors.AddRange(userDDL.errors);
                        return response;
                    }
                    response = userDDL;
                    return response;
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

        //[HttpPost]
        //public ActionResult<BaseResponseWithID> UpdateProjectManager([FromHeader]long projectId , [FromHeader]long UserId)
        //{

        //}

        [HttpPost("AssignPManagerToProject")]       //Service Added
        public BaseResponseWithID AssignPManagerToProject([FromHeader] long projectId, [FromHeader]long pManagerId)
        {
            BaseResponseWithID response = new BaseResponseWithID()
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
                    var AssignPManager = _adminService.AssignPManagerToProject(projectId, pManagerId);
                    if (!AssignPManager.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(AssignPManager.Errors);
                        return response;
                    }
                    response = AssignPManager;
                    return response;
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

        [NonAction]
        public async Task<decimal> CurrencyConverterAsync(string from, string to, decimal amount)
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
                string BaseCurrencyConverterApiAddress = "https://api.exchangerate.host/";
                string CurrencyConvertorAddress = "convert?format=json";
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

        [HttpGet("GetUserData")]        //already exists
        public async Task<LoginResponseAdmin> GetUserData()
        {
            LoginResponseAdmin response = new LoginResponseAdmin();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                string baseURL = MyConfig.GetValue<string>("AppSettings:baseURL");
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    //var CheckUserDB = await _Context.Users.Where(x => x.Id == validation.userID).FirstOrDefaultAsync();
                    var CheckUserDB = await _Context.Users.Where(x => x.Id == validation.userID).Include(x => x.JobTitle).Include(x => x.Department).Include(x => x.Branch).FirstOrDefaultAsync();

                    //var CheckUserDB = await _Context.V_UserInfo.Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == PassEncrypted).FirstOrDefaultAsync();

                    if (CheckUserDB != null)
                    {
                        if (CheckUserDB.Active)
                        {
                            //////////////////////


                            if (CheckUserDB.PhotoUrl != null)
                            {
                                // user.UserImage = DBUser.Photo;
                                //var baseURL = OperationContext.Current.Host.BaseAddresses[0].Authority;

                                response.UserImageURL = baseURL + CheckUserDB.PhotoUrl;
                                //baseURL + "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(CheckUserDB.Id.ToString(), key)) + "&type=photo&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();
                            }
                            //long UserSessionID = 0;
                            //var CheckSessionOpen = await _Context.UserSessions.Where(x => x.Active == true && x.UserID == CheckUserDB.ID && x.EndDate > DateTime.Now).OrderByDescending(x => x.EndDate).FirstOrDefaultAsync();
                            //if (CheckSessionOpen == null)
                            //{
                            //    var UserSessionObj = new UserSession(); // DB
                            //    UserSessionObj.UserID = CheckUserDB.ID;
                            //    UserSessionObj.Active = true;
                            //    UserSessionObj.CreationDate = DateTime.Now;
                            //    UserSessionObj.EndDate = DateTime.Now.AddDays(1);
                            //    UserSessionObj.ModifiedBy = "System";
                            //    _Context.UserSessions.Add(UserSessionObj);

                            //      await _Context.SaveChangesAsync();
                            //    UserSessionID = (long)UserSessionObj.ID;
                            //}
                            //else
                            //{
                            //    UserSessionID = CheckSessionOpen.ID;
                            //}
                            //var CheckSessionOpen = await _Context.UserSessions.Where(x => x.Active == true && x.UserID == CheckUserDB.ID && x.EndDate > DateTime.Now).OrderByDescending(x => x.CreationDate).FirstOrDefaultAsync();
                            //if (CheckSessionOpen != null)
                            //{

                            response.Result = true;
                            response.Data = Request.Headers["UserToken"]; // HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(CheckSessionOpen.ID.ToString(), key));
                            response.UserID = Encrypt_Decrypt.Encrypt(CheckUserDB.Id.ToString(), key);
                            response.UserIDNO = CheckUserDB.Id;
                            response.UserName = CheckUserDB.FirstName + " " + CheckUserDB.LastName;
                            response.Jobtitle = CheckUserDB?.JobTitle?.Name; // CheckUserDB.UserJobTitle != null ? CheckUserDB.UserJobTitle : "";
                            response.DepartmentName = CheckUserDB.Department?.Name; // != null ? CheckUserDB.UserDepartmentName : "";
                            response.BranchName = CheckUserDB.Branch?.Name; // CheckUserDB.UserBranchName != null ? CheckUserDB.UserBranchName : "";
                            response.BranchID = CheckUserDB.BranchId;
                            var MainCompanyProfile = await _Context.Clients.Where(a => a.OwnerCoProfile == true).FirstOrDefaultAsync();
                            var MainCompanyProfileAddress = MainCompanyProfile?.ClientAddresses?.FirstOrDefault();
                            if (MainCompanyProfileAddress != null)
                            {
                                response.CountryId = MainCompanyProfileAddress.CountryId;
                                response.CountryName = MainCompanyProfileAddress.Country?.Name;
                            }
                            // Not From UserID => To User Id
                            var NotificationCount = _Context.Notifications.Where(x => (x.FromUserId == CheckUserDB.Id || x.FromUserId == null) && x.New == true).Count();
                            response.NotificationCount = NotificationCount;

                            var UserName = Common.GetUserName(CheckUserDB.Id,_Context);


                            var TaskCountFromTaskCount = _Context.Tasks.Where(x => x.TaskDetails.Where(y => y.Status == "Open").Any() &&
                            (x.CreatedBy == CheckUserDB.Id || x.TaskPermissions.Where(p => p.UserGroupId == CheckUserDB.Id).Any()
                            )).Count();

                            response.TaskCount = TaskCountFromTaskCount;

                            //response.Jobtitle = CheckUserDB.JobTitleID != null ? Common.GetJobTitleName((int)CheckUserDB.JobTitleID) : "";
                            //response.DepartmentName = CheckUserDB.DepartmentID != null ? Common.GetDepartmentName((int)CheckUserDB.DepartmentID) : "";
                            //response.BranchName = CheckUserDB.BranchID != null ? Common.GetBranchName((int)CheckUserDB.BranchID) : "";
                            var LocalCurrency = await _Context.Currencies.Where(x => x.IsLocal == true).FirstOrDefaultAsync();
                            if (LocalCurrency != null)
                            {
                                response.LocalCurrencyId = LocalCurrency.Id;
                                response.LocalCurrencyName = LocalCurrency.Name;
                            }


                            // Fill User role List 
                            var RoleList = new List<Roles>();
                            var RoleListDB = await _Context.VUserRoles.Where(x => x.UserId == CheckUserDB.Id).ToListAsync();
                            foreach (var UserRoleOBJ in RoleListDB)
                            {
                                RoleList.Add(new Roles
                                {
                                    RoleID = UserRoleOBJ.RoleId,
                                    RoleName = UserRoleOBJ.RoleName
                                });
                            }



                            // Fill User Group List 
                            var GroupList = new List<GroupRoles>();
                            var GroupListDB = await _Context.VGroupUserBranches.Where(x => x.UserId == CheckUserDB.Id).ToListAsync();
                            foreach (var UserGroupOBJ in GroupListDB)
                            {
                                GroupList.Add(new GroupRoles
                                {
                                    GroupID = (long)UserGroupOBJ.GroupId,
                                    GroupName = UserGroupOBJ.GroupName
                                });
                            }
                            //var GroupList = new List<GroupRoles>();
                            //var GroupListDB = _Context.proc_Group_UserLoadAll().Where(x => x.UserID == CheckUserDB.ID).ToList();
                            //foreach (var UserGroupOBJ in GroupListDB)
                            //{
                            //        GroupList.Add(new GroupRoles
                            //        {
                            //            GroupID = (long)UserGroupOBJ.GroupID,
                            //            GroupName = Common.GetGroupName(UserGroupOBJ.GroupID)
                            //        });
                            //}
                            var SpecialityList = new List<SelectDDL>();
                            SpecialityList = await _Context.CompanySpecialties.Select(x => new SelectDDL
                            {
                                ID = x.SpecialityId,
                                Name = x.SpecialityName
                            }).ToListAsync();

                            response.SpecialityList = SpecialityList;
                            response.RoleList = RoleList;
                            response.GroupList = GroupList;
                            //}




                            // from supplier Is Owner
                            var ClientInfo = _Context.Clients.Where(x => x.OwnerCoProfile == true).FirstOrDefault();
                            if (ClientInfo != null)
                            {
                                if (ClientInfo.HasLogo == true && ClientInfo.Logo != null)
                                {
                                    response.CompanyImg = Globals.baseURL + "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(ClientInfo.Id.ToString(), key)) + "&type=client&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();
                                }
                                response.CompanyInfo = ClientInfo.Name;
                            }
                            return response;

                        }
                        else
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P9";
                            error.ErrorMSG = "This Email is not active ";
                            response.Errors.Add(error);

                        }

                    }
                    else
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P11";
                        error.ErrorMSG = "Invalid User";
                        response.Errors.Add(error);

                    }
                    //if (CheckUserDB != null)
                    //{
                    //    if (CheckUserDB.Active)
                    //    {
                    //        if (CheckUserDB.Photo != null)
                    //        {
                    //            response.UserImageURL = baseURL + "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(CheckUserDB.ID.ToString(), key)) + "&type=photo&CompName=" + headers["CompanyName"].ToString().ToLower();
                    //        }

                    //        response.Result = true;
                    //        response.UserID = Encrypt_Decrypt.Encrypt(CheckUserDB.ID.ToString(), key);
                    //        response.UserName = CheckUserDB.FirstName + " " + CheckUserDB.LastName;
                    //        response.Jobtitle = CheckUserDB.UserJobTitle != null ? CheckUserDB.UserJobTitle : "";
                    //        response.DepartmentName = CheckUserDB.UserDepartmentName != null ? CheckUserDB.UserDepartmentName : "";
                    //        response.BranchName = CheckUserDB.UserBranchName != null ? CheckUserDB.UserBranchName : "";
                    //        response.Data = headers["UserToken"];

                    //        // Fill User role List 
                    //        var RoleList = new List<Roles>();
                    //        var RoleListDB = _Context.V_UserRole.Where(x => x.UserID == CheckUserDB.ID).ToList();
                    //        foreach (var UserRoleOBJ in RoleListDB)
                    //        {
                    //            RoleList.Add(new Roles
                    //            {
                    //                RoleID = UserRoleOBJ.RoleID,
                    //                RoleName = UserRoleOBJ.RoleName //Common.GetRoleName(UserRoleOBJ.RoleID)
                    //            });
                    //        }

                    //        // Fill User Group List 
                    //        var GroupList = new List<GroupRoles>();
                    //        var GroupListDB = _Context.V_GroupUser_Branch.Where(x => x.UserID == CheckUserDB.ID).ToList();
                    //        foreach (var UserGroupOBJ in GroupListDB)
                    //        {
                    //            GroupList.Add(new GroupRoles
                    //            {
                    //                GroupID = (long)UserGroupOBJ.GroupID,
                    //                GroupName = UserGroupOBJ.GroupName
                    //            });
                    //        }

                    //        response.RoleList = RoleList;
                    //        response.GroupList = GroupList;
                    //        return response;

                    //    }
                    //    else
                    //    {
                    //        response.Result = false;
                    //        Error error = new Error();
                    //        error.ErrorCode = "Err-P9";
                    //        error.ErrorMSG = "This Email was not active ";
                    //        response.Errors.Add(error);

                    //    }

                    //}

                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }

        }

        [HttpGet("GetUserList")]        //service exists
        public async Task<UserDDLResponse> GetUserList([FromHeader] int BranchId = 0, [FromHeader] int RoleId = 0, [FromHeader] long GroupId = 0, [FromHeader] bool WithTeam = false)
        {
            UserDDLResponse Response = new UserDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

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

                #region old code
                //var DDLList = new List<UserDDL>();
                //if (Response.Result)
                //{
                //    List<long> UsersIds = new List<long>();

                //    var ListDBQuery = _Context.Users.Where(x => x.Active == true).AsQueryable();
                //    if (BranchId != 0)
                //    {
                //        ListDBQuery = ListDBQuery.Where(a => a.BranchId == BranchId);
                //    }
                //    if (RoleId != 0)
                //    {
                //        UsersIds.AddRange(await _Context.UserRoles.Where(a => a.RoleId == RoleId).Select(a => (long)a.UserId).ToListAsync());

                //        /*foreach (var roleUser in RoleUsers)
                //        {
                //            UsersIds.Add(roleUser);
                //        }*/
                //    }
                //    if (GroupId != 0)
                //    {
                //        UsersIds.AddRange(await _Context.GroupUsers.Where(a => a.GroupId == GroupId).Select(a => (long)a.UserId).ToListAsync());
                //        /*foreach (var grpUser in GroupUsers)
                //        {
                //            UsersIds.Add(grpUser);
                //        }*/
                //    }

                //    if (UsersIds.Count() > 0)
                //    {
                //        ListDBQuery = ListDBQuery.Where(a => UsersIds.Contains(a.Id)).Distinct();
                //    }

                //    var ListDB = ListDBQuery.ToList();
                //    //if (headers["SearchKey"] != null && headers["SearchKey"] != "")
                //    //{

                //    //    string SearchKey = headers["SearchKey"];

                //    //    var ListIDSMobileClient = _Context.proc_ClientMobileLoadAll().Where(x => x.Active == true && x.Mobile.ToLower().Contains(SearchKey.ToLower())).Select(x => x.ClientID).Distinct().ToList();
                //    //    var ListIDSContactPersonClient = _Context.proc_ClientContactPersonLoadAll().Where(x => x.Active == true && (x.Mobile.ToLower().Contains(SearchKey.ToLower())
                //    //                                                                                                      || x.Email != null ? x.Email.ToLower().Contains(SearchKey.ToLower()) : false)
                //    //                                                                                                      ).Select(x => x.ClientID).Distinct().ToList();
                //    //    ListDB = ListDB.Where(x => x.Name.ToLower().Contains(SearchKey.ToLower())
                //    //                            || (x.Email != null ? x.Email.ToLower().Contains(SearchKey.ToLower()) : false)
                //    //                            || ListIDSMobileClient.Contains(x.ID) || ListIDSContactPersonClient.Contains(x.ID)
                //    //                            ).ToList();
                //    //}
                //    if (ListDB.Count > 0)
                //    {
                //        foreach (var user in ListDB)
                //        {
                //            var DLLObj = new UserDDL();
                //            DLLObj.ID = user.Id;
                //            DLLObj.Email = user.Email.Trim(); ;
                //            DLLObj.BranchId = user.BranchId;
                //            DLLObj.Department = user.Department?.Name; // != null ? Common.GetDepartmentName((int)user.DepartmentID) : "";
                //            DLLObj.JobTitleName = user.JobTitle?.Name; // != null ? Common.GetJobTitleName((int)user.JobTitleID) : "";
                //            DLLObj.BranchName = user.Branch?.Name; // != null ? Common.GetBranchName((int)user.BranchID) : "";
                //            DLLObj.Name = user.FirstName.Trim() + " " + user.LastName.Trim();

                //            if (user.PhotoUrl != null)
                //            {
                //                DLLObj.Image = Globals.baseURL + user.PhotoUrl;
                //                //+ "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(user.Id.ToString(), key)) + "&type=photo&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();
                //            }

                //            // Fill User role List 
                //            var RoleList = new List<Roles>();
                //            var RoleListDB = await _Context.VUserRoles.Where(x => x.UserId == user.Id).ToListAsync();
                //            foreach (var UserRoleOBJ in RoleListDB)
                //            {
                //                RoleList.Add(new Roles
                //                {
                //                    RoleID = UserRoleOBJ.RoleId,
                //                    RoleName = UserRoleOBJ.RoleName // Common.GetRoleName(UserRoleOBJ.RoleID)
                //                });
                //            }

                //            // Fill User Group List 
                //            var GroupList = new List<GroupRoles>();
                //            var GroupListDB = await _Context.GroupUsers.Where(x => x.UserId == user.Id && x.Active == true).ToListAsync();
                //            foreach (var UserGroupOBJ in GroupListDB)
                //            {
                //                GroupList.Add(new GroupRoles
                //                {
                //                    GroupID = UserGroupOBJ.GroupId,
                //                    GroupName = Common.GetGroupName(UserGroupOBJ.GroupId, _Context)
                //                }); ;
                //            }
                //            DLLObj.RoleList = RoleList;
                //            DLLObj.GroupList = GroupList;
                //            DDLList.Add(DLLObj);
                //        }
                //    }
                //}
                //Response.DDLList = DDLList.Distinct().ToList();
                #endregion

                if (Response.Result)
                {
                    var userList =await _adminService.GetUserList(BranchId, RoleId, GroupId, WithTeam);
                    if (!userList.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(userList.Errors);
                        return Response;
                    }
                    Response = userList;
                    return Response;
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

        [HttpGet("GetGroup")]       //service exists
        public async Task<GetGroupResponse> GetGroup()
        {
            GetGroupResponse response = new GetGroupResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                

                if (response.Result)
                {

                    var Group =await _adminService.GetGroup();
                    if (!Group.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(Group.Errors);
                        return response;
                    }
                    response = Group;
                    return response;

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

        //[HttpGet("GetDashboard")]
        //public DashboardResponse GetDashboard()
        //{
        //    DashboardResponse Response = new DashboardResponse();
        //    Response.Result = true;
        //    Response.Errors = new List<Error>();
        //    try
        //    {
        //        //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        //        //WebHeaderCollection headers = request.Headers;
        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers,ref _Context);
        //        Response.Errors = validation.errors;
        //        Response.Result = validation.result;

        //        var DashboardInfoObj = new DashboardInfo();
        //        if (Response.Result)
        //        {
        //            // Call Constructor Common Class 
        //            // Common._Context = _Context;

        //            //var ResponseAccountAndFinanceIncomeStatment = GetAccountsAndFinanceIncomeStatment();
        //            //DashboardInfoObj.AccountsAndFinance = ResponseAccountAndFinanceIncomeStatment.NetProfit;
        //            DashboardInfoObj.AccountsAndFinance = Common.GetNetProfitIncomeStatment(_Context);
        //            // API Inventory
        //            //var ResponseInventory = GetAccountAndFinanceInventoryStoreItemReportList();
        //            // DashboardInfoObj.InventoryAndStores = ResponseInventory.TotalStockBalanceValue;
        //            DashboardInfoObj.InventoryAndStores = Common.GetTotalAmountInventoryItem(0,_Context); //0 =>  All Items
        //                                                                                         // API supplier Report Sales Force
        //                                                                                         // var ResponseClientReport = GetAccountAndFinanceClientReportList();
        //                                                                                         // DashboardInfoObj.SalesForceAndClients = ResponseClientReport.TotalSalesVolume;
        //                                                                                         //DashboardInfoObj.TotalFinalOfferPrice = Common.GetTotalSalesOfferProjectFinalPriceAmount();
        //                                                                                         //DashboardInfoObj.TotalProjectExtraCost = Common.GetTotalSalesOfferProjectExtraCostsAmount();
        //                                                                                         //DashboardInfoObj.TotalFinalOfferPriceWithInternalType = Common.GetTotalSalesOfferProjectAmountForOffterTypeInternal();
        //                                                                                         //DashboardInfoObj.SalesForceAndClients = DashboardInfoObj.TotalFinalOfferPrice + DashboardInfoObj.TotalProjectExtraCost;
        //            DashboardInfoObj.SalesForceAndClients = Common.GetTotalSalesForceClientReportAmount(_Context);

        //            // API supplier Report Sales Force
        //            // var ResponseSupplierReport = GetAccountAndFinanceSupplierReportList();
        //            // DashboardInfoObj.PurchasingAndSuppliers = ResponseSupplierReport.TotalSalesVolume;
        //            DashboardInfoObj.PurchasingAndSuppliers = Common.GetTotalPurchasingAndSupplierReportAmount(_Context);

        //            // Project Detailes
        //            DashboardInfoObj.CountOFOpenProject = Common.GetTotalCountOfProjects("open",_Context);
        //            DashboardInfoObj.CountOFClosedProject = Common.GetTotalCountOfProjects("closed",_Context);
        //            var TotalAmountProject = Common.GetTotalCostOfProjects("AllProject",_Context); // Default all
        //            var TotalCollectionProject = Common.GetTotalCollectedCostOfProjects("AllProject",_Context); // Default all
        //            DashboardInfoObj.TotalCollectionActiveProjects = TotalCollectionProject;
        //            DashboardInfoObj.PercentCollectionActiveProjects = TotalAmountProject != 0 ? (TotalCollectionProject / TotalAmountProject) * 100 : 0;


        //            Response.Data = DashboardInfoObj;
        //        }
        //        return Response;

        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
        //        Response.Errors.Add(error);
        //        return Response;
        //    }
        //}

        [HttpPost("EditEmployeeGroup")]     //service exists
        public async Task<BaseResponseWithID> EditEmployeeGroup(EmployeeGroupData request)
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

                #region old code
                //if (Response.Result)
                //{

                //    //check sent data
                //    if (Request == null)
                //    {
                //        Response.Result = false;
                //        Error error = new Error();
                //        error.ErrorCode = "Err-P12";
                //        error.ErrorMSG = "Please Insert a Valid Data.";
                //        Response.Errors.Add(error);
                //        return Response;
                //    }

                //    long UserID = 0;
                //    if (!string.IsNullOrEmpty(Request.Headers["UserID"]) && long.TryParse(Request.Headers["UserID"], out UserID))
                //    {
                //        long.TryParse(Request.Headers["UserID"], out UserID);
                //    }

                //    if (Response.Result)
                //    {
                //        var user = _Context.HrUsers.Find((long)request.UserID);
                //        if (user != null)
                //        {
                //            var ListUserGroupVM = request.userGroupData;
                //            var ListUserGroupDB = await _Context.GroupUsers.Where(x => x.HrUserId == request.UserID || x.UserId == user.UserId).ToListAsync();
                //            if (ListUserGroupVM != null && ListUserGroupVM.Count() > 0)
                //            {
                //                var IDSListUserGroupDB = ListUserGroupDB.Select(x => x.Id).ToList();
                //                var IDSListUserGroupVM = new List<long>();



                //                foreach (var item in ListUserGroupVM)
                //                {
                //                    //if (item.ID != 0) // Edit
                //                    //{
                //                    //    var UpdateKeeperObjDB = ListUserGroupDB.Where(x => x.Id == item.ID).FirstOrDefault();
                //                    //    UpdateKeeperObjDB.GroupId = item.GroupID;
                //                    //    _Context.SaveChanges();
                //                    //}
                //                    //else //Insert
                //                    //{
                //                        var CheckIfExistBefore = ListUserGroupDB.Where(x => (x.HrUserId == request.UserID || x.UserId == user.UserId) && x.GroupId == item.GroupID).FirstOrDefault();
                //                        if (CheckIfExistBefore == null)
                //                        {
                //                            var hrUser = _Context.HrUsers.Find((long)request.UserID);
                //                            var InsertUserGroupsObjDB = new GroupUser();
                //                            InsertUserGroupsObjDB.HrUserId = request.UserID;
                //                            InsertUserGroupsObjDB.GroupId = item.GroupID;


                //                            InsertUserGroupsObjDB.CreatedBy = validation.userID;

                //                            InsertUserGroupsObjDB.CreationDate = DateTime.Now;
                //                            InsertUserGroupsObjDB.Active = true;
                //                            if (hrUser != null && hrUser.IsUser)
                //                            {
                //                                InsertUserGroupsObjDB.UserId = hrUser.UserId;
                //                            }
                //                            _Context.GroupUsers.Add(InsertUserGroupsObjDB);
                //                            var Res = _Context.SaveChanges();
                //                            if (Res > 0)
                //                            {
                //                                IDSListUserGroupDB.Add(InsertUserGroupsObjDB.Id);
                //                                IDSListUserGroupVM.Add(InsertUserGroupsObjDB.Id);
                //                            }
                //                        }
                //                        else
                //                        {
                //                            IDSListUserGroupVM.Add(CheckIfExistBefore.Id);
                //                        }
                //                    //}
                //                }


                //                //int Counter = ListKeeperVM.Count() >= ListKeeperDB.Count() ? ListKeeperVM.Count() : ListKeeperDB.Count();

                //                //for (int Count = 0; Count < ListKeeperVM.Count(); Count++)
                //                //{
                //                //    if (ListKeeperVM[Count].ID != 0) // Edit
                //                //    {
                //                //        var UpdateKeeperObjDB = ListKeeperDB.Where(x => x.ID == ListKeeperVM[Count].ID).FirstOrDefault();
                //                //        UpdateKeeperObjDB.UserID = ListKeeperVM[Count].UserID;
                //                //        _Context.SaveChanges();
                //                //    }
                //                //    else //Insert
                //                //    {
                //                //        var CheckIfExistBefore = ListKeeperDB.Where(x => x.UserID == ListKeeperVM[Count].UserID).FirstOrDefault();
                //                //        if (CheckIfExistBefore == null)
                //                //        {
                //                //            var InsertKeeperObjDB =new InventoryStoreKeeper();
                //                //            InsertKeeperObjDB.InventoryStoreID = Request.ID;
                //                //            InsertKeeperObjDB.UserID = ListKeeperVM[Count].UserID;
                //                //            InsertKeeperObjDB.Active = ListKeeperVM[Count].Active;
                //                //            InsertKeeperObjDB.CreatedBy = validation.userID;
                //                //            InsertKeeperObjDB.ModifiedBy = validation.userID;
                //                //            InsertKeeperObjDB.CreationDate = DateTime.Now;
                //                //            InsertKeeperObjDB.ModifiedDate = DateTime.Now;
                //                //            _Context.InventoryStoreKeepers.Add(InsertKeeperObjDB);
                //                //           var Res =  _Context.SaveChanges();
                //                //            if (Res > 0)
                //                //            {
                //                //                IDSListKeebersDB.Add(InsertKeeperObjDB.ID);
                //                //                IDSListKeebersVM.Add(InsertKeeperObjDB.ID);
                //                //            }
                //                //        }
                //                //    }
                //                //}

                //                var IDSListToRemove = IDSListUserGroupDB.Except(IDSListUserGroupVM).ToList();

                //                var DeletedUserGroupListDB = ListUserGroupDB.Where(x => IDSListToRemove.Contains(x.Id)).ToList();
                //                _Context.GroupUsers.RemoveRange(DeletedUserGroupListDB);
                //                _Context.SaveChanges();


                //            }
                //            else // List is empty must be deleted
                //            {
                //                // delete list from DB
                //                _Context.GroupUsers.RemoveRange(ListUserGroupDB);
                //                _Context.SaveChanges();
                //            }
                //        }
                //        else
                //        {
                //            Response.Result = false;
                //            Error error = new Error();
                //            error.ErrorCode = "Err25";
                //            error.ErrorMSG = "User is not Found !!";
                //            Response.Errors.Add(error);
                //        }




                //    }
                //    else
                //    {
                //        Response.Result = false;
                //        Error error = new Error();
                //        error.ErrorCode = "Err25";
                //        error.ErrorMSG = "Faild To Insert this User Group !!";
                //        Response.Errors.Add(error);
                //    }


                //}

                #endregion

                if (Response.Result)
                {

                    var EmployeeGroup = await _adminService.EditEmployeeGroup(request, validation.userID);
                    if (!EmployeeGroup.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(EmployeeGroup.Errors);
                        return Response;
                    }
                    Response = EmployeeGroup;
                    return Response;

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
        [HttpPost("EditEmployeeGroupNew")]      //service exists

        public async Task<BaseResponseWithID> EditEmployeeGroupNew(EmployeeGroupData request)
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

                    var EmployeeGroup = await _adminService.EditEmployeeGroupNew(request, validation.userID);
                    if (!EmployeeGroup.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(EmployeeGroup.Errors);
                        return Response;
                    }
                    Response = EmployeeGroup;
                    return Response;

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





        [HttpGet("GetTeamList")]        //service Added
        public async Task<ActionResult<BaseResponseWithData<List<SelectDDL>>>> GetTeamList([FromHeader] int? DepartmentId)
        {
            var response = new BaseResponseWithData<List<SelectDDL>>();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                if (response.Result)
                {

                    var teamList = await _adminService.GetTeamList(DepartmentId);
                    if (!teamList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(teamList.Errors);
                        return response;
                    }
                    response = teamList;
                    return response;

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

        [HttpGet("GetEmployeeList")]        //service Added
        public GetEmployeeListResponse GetEmployeeList(GetEmployeeListFilters filters)
        {
            var response = new GetEmployeeListResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _adminService.Validation = validation;
                    var teamList = _adminService.GetEmployeeList(filters);
                    if (!teamList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(teamList.Errors);
                        return response;
                    }
                    response = teamList;
                    return response;

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

        [HttpGet("TopSellingProductExcel")]        //service Added
        public async Task<BaseResponseWithMessage<string>> TopSellingProductExcel(GetMyProjectsDetailsCRMHeaders headers)
        {
            var response = new BaseResponseWithMessage<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _adminService.Validation = validation;
                    response =await _adminService.TopSellingProductExcel(headers);
                   
                    return response;

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


        [HttpGet("GetDashboard")]       //service Added
        public async Task<DashboardResponse> GetDashboard()
        {
            DashboardResponse Response = new DashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                var DashboardInfoObj = new DashboardInfo();
                if (Response.Result)
                {
                    var dashBoard = _adminService.GetDashboard();
                    if (!dashBoard.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(dashBoard.Errors);
                        return Response;
                    }
                    Response = dashBoard;
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

        [HttpPost("DeleteGroupRole")]     
        public BaseResponseWithId<long> DeleteGroupRole([FromHeader] long GroupId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
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

                    Response = _adminService.DeleteGroupRole(GroupId);


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

        [HttpGet("GetAddEmployeeScreenData")]       //service Added
        public GetAddEmployeeScreenDataResponse GetAddEmployeeScreenData()
        {
            GetAddEmployeeScreenDataResponse Response = new GetAddEmployeeScreenDataResponse();
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
                    Response = _adminService.GetAddEmployeeScreenData();
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

        [HttpPost("AddEditBundleModule")]       //service Added
        public BaseResponseWithId<long> AddEditBundleModule(BundleModuleData request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _adminService.AddEditBundleModule(request,validation.userID);
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

        [HttpGet("GetTermsCategoryDDL")]       //service Added
        public async Task<GetTermsCategoryResponse> GetTermsCategoryDDL()
        {
            GetTermsCategoryResponse Response = new GetTermsCategoryResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _adminService.GetTermsCategoryDDL();
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


        [HttpPost("AddEditTermsCategory")]       //service Added
        public BaseResponseWithId<int> AddEditTermsCategory(CategoryofTermsandConditionsData request)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _adminService.AddEditTermsCategory(request, validation.userID);
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


        [HttpGet("GetSupportedByList")]       //service Added
        public async Task<GetSupportedByResponse> GetSupportedByList()
        {
            GetSupportedByResponse Response = new GetSupportedByResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _adminService.GetSupportedByList();
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

        [HttpPost("AddEditDeleteSupportedBy")]       //service Added
        public async Task<BaseResponseWithId<long>> AddEditDeleteSupportedBy(AddEditDeleteSupportedByRequest request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _adminService.AddEditDeleteSupportedBy(request, validation.userID);
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


        [HttpPost("DeleteArea")]       //service Added
        public BaseResponseWithId<long> DeleteArea([FromHeader] long AreaId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _adminService.DeleteArea(AreaId);
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

        [HttpPost("UserSoftDelete")]       //service Added
        public BaseResponseWithId<long> UserSoftDelete([FromHeader] long UserId, [FromHeader] bool Active)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    _adminService.Validation = validation;
                    Response = _adminService.UserSoftDelete(UserId, Active);
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

        [HttpGet("MapPlaceDetails")]       //service Added
        public async Task<GoogleMapsRespone> MapPlaceDetails([FromHeader] string Url)
        {
            GoogleMapsRespone Response = new GoogleMapsRespone();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _adminService.MapPlaceDetails(Url);
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

        [HttpGet("GetCostType")]       //service Added
        public async Task<GetCostTypeResponse> GetCostType()
        {
            GetCostTypeResponse Response = new GetCostTypeResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _adminService.GetCostType();
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


        [HttpPost("AddEditCostType")]       //service Added
        public async Task<BaseResponseWithID> AddEditCostType(CostTypeData request)
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
                    _adminService.Validation = validation;
                    Response = await _adminService.AddEditCostType(request);
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
