using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Common;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.User;
using System.Net;
using System.Web;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Text;
using Azure.Core;
using NewGarasAPI.Models.Admin;
using DocumentFormat.OpenXml.InkML;
using System.Threading.Tasks;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.HR;

namespace NewGarasAPI.Controllers.HR
{
    [Route("[controller]")]
    [ApiController]
    public class HRController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IAdminService _adminService;
        private readonly IHrUserService _hrUserService;
        public HRController(IWebHostEnvironment host,ITenantService tenantService,IAdminService adminService,IHrUserService hrUserService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            key = "SalesGarasPass";
            _helper = new Helper.Helper();
            _host = host;
            _adminService=adminService;
            _hrUserService = hrUserService;
        }


        //[HttpGet("GetEmployeeInfo")]        //move to Hruser controller and has service
        //public ActionResult<GetListOfEmployeeInfo> GetEmployeeInfo([FromHeader] GetEmployeeInfoHeader header)
        //{
        //    GetListOfEmployeeInfo response = new GetListOfEmployeeInfo();
        //    response.Result = true;
        //    response.Errors = new List<Error>();

        //    var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        //    string baseURL = MyConfig.GetValue<string>("AppSettings:baseURL");
        //    try
        //    {

        //        /*IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        //        WebHeaderCollection Request.Headers = request.Headers;*/
        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //        response.Errors = validation.errors;
        //        response.Result = validation.result;
        //        if (response.Result)
        //        {
        //        var GetEmployeeResponseList = new List<EmployeeInfoData>();

        //        UserLogin login = new UserLogin();

        //        //string CompName = "";

        //        //if (CompName == "proauto")
        //        //{
        //        //    _Context = GlobalEntity.ContextGarasAuto;
        //        //}
        //        //else if (CompName == "marinaplt")
        //        //{
        //        //    _Context = GlobalEntity.ContextGarasMarina;
        //        //}
        //        //else if (CompName == "piaroma")
        //        //{
        //        //    _Context = GlobalEntity.ContextGarasAroma;
        //        //}


        //            if (!string.IsNullOrEmpty(header.SearchKey))
        //            {
        //                header.SearchKey = header.SearchKey;
        //                header.SearchKey = HttpUtility.UrlDecode(header.SearchKey);
        //            }
        //            /*int CurrentPage = 1;
        //            if (!string.IsNullOrEmpty(header.CurrentPage) && int.TryParse(header.CurrentPage, out CurrentPage))
        //            {
        //                int.TryParse(header.CurrentPage, out CurrentPage);
        //            }

        //            int NumberOfItemsPerPage = 10;
        //            if (!string.IsNullOrEmpty(header.NumberOfItemsPerPage) && int.TryParse(header.NumberOfItemsPerPage, out NumberOfItemsPerPage))
        //            {
        //                int.TryParse(header.NumberOfItemsPerPage, out NumberOfItemsPerPage);
        //            }

        //            long BranchID = 0;
        //            if (!string.IsNullOrEmpty(header.BranchID) && long.TryParse(header.BranchID, out BranchID))
        //            {
        //                long.TryParse(header.BranchID, out BranchID);
        //            }
        //            long DepartmentID = 0;
        //            if (!string.IsNullOrEmpty(header.DepartmentID) && long.TryParse(header.DepartmentID, out DepartmentID))
        //            {
        //                long.TryParse(header.DepartmentID, out DepartmentID);
        //            }

        //            long UserID = 0;
        //            if (!string.IsNullOrEmpty(header.UserID) && long.TryParse(header.UserID, out UserID))
        //            {
        //                long.TryParse(header.UserID, out UserID);
        //            }
        //            bool isExpired = false;
        //            if (!string.IsNullOrEmpty(header.isExpired))
        //            {
        //                isExpired = bool.Parse(header.isExpired);
        //            }
        //            DateTime expiredIn = DateTime.Now;
        //            if (!string.IsNullOrEmpty(header.expiredIn) && DateTime.TryParse(header.expiredIn, out expiredIn))
        //            {
        //                expiredIn = DateTime.Parse(header.expiredIn);
        //            }*/

        //            if (response.Result)
        //            {
        //                var GetEmployeeInfoDB = _Context.Users.AsQueryable();
        //                if (validation.userID != 1)
        //                {
        //                    GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.Id != 1);
        //                }
        //                if (!string.IsNullOrEmpty(header.SearchKey))
        //                {
        //                    GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.FirstName.Contains(header.SearchKey) ||
        //                                                                     x.LastName.Contains(header.SearchKey) ||
        //                                                                     x.MiddleName.Contains(header.SearchKey) ||
        //                                                                     x.Email.Contains(header.SearchKey) ||
        //                                                                     x.Mobile == header.SearchKey
        //                                                                     ).AsQueryable();
        //                }
        //                if (header.isExpired)
        //                {
        //                    GetEmployeeInfoDB = GetEmployeeInfoDB.Where(a => _Context.HremployeeAttachments.Where(x => x.EmployeeUserId == a.Id && x.Active && x.ExpiredDate < header.expiredIn).Count() > 0).AsQueryable();
        //                }
        //                if (header.DepartmentID != 0)
        //                {
        //                    GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.DepartmentId == header.DepartmentID).AsQueryable();
        //                }
        //                if (header.BranchID != 0)
        //                {
        //                    GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.BranchId == header.BranchID).AsQueryable();
        //                }
        //                if (header.UserID != 0)
        //                {
        //                    GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.Id == header.UserID).AsQueryable();
        //                }
        //                var EmployeePagingList = PagedList<User>.CreateOrdered(GetEmployeeInfoDB.OrderBy(x => x.FirstName), header.CurrentPage, header.NumberOfItemsPerPage);
        //                response.PaginationHeader = new PaginationHeader
        //                {
        //                    CurrentPage = header.CurrentPage,
        //                    TotalPages = EmployeePagingList.TotalPages,
        //                    ItemsPerPage = header.NumberOfItemsPerPage,
        //                    TotalItems = EmployeePagingList.TotalCount
        //                };



        //                if (EmployeePagingList != null)
        //                {

        //                    foreach (var GetEmployeeOBJ in EmployeePagingList)
        //                    {

        //                        var GetEmployeeResponse = new EmployeeInfoData();

        //                        GetEmployeeResponse.ID = (int)GetEmployeeOBJ.Id;

        //                        GetEmployeeResponse.EmployeeName = Common.GetUserName(GetEmployeeOBJ.Id, _Context);

        //                        GetEmployeeResponse.Age = GetEmployeeOBJ.Age;

        //                        GetEmployeeResponse.FirstName = GetEmployeeOBJ.FirstName;

        //                        GetEmployeeResponse.LastName = GetEmployeeOBJ.LastName;

        //                        GetEmployeeResponse.Active = GetEmployeeOBJ.Active;

        //                        GetEmployeeResponse.expiredDocumentsCount = _Context.HremployeeAttachments.Where(a => a.Active && a.EmployeeUserId == (int)GetEmployeeOBJ.Id && a.ExpiredDate < header.expiredIn).Count();

        //                        GetEmployeeResponse.MiddleName = GetEmployeeOBJ.MiddleName;

        //                        GetEmployeeResponse.BranchID = GetEmployeeOBJ.BranchId;

        //                        GetEmployeeResponse.JobTitleID = GetEmployeeOBJ.JobTitleId;


        //                        GetEmployeeResponse.Password = Encrypt_Decrypt.Decrypt(GetEmployeeOBJ.Password, key).Trim();

        //                        if (GetEmployeeOBJ.BranchId != 0 && GetEmployeeOBJ.BranchId != null)
        //                        {

        //                            GetEmployeeResponse.BranchName = Common.GetBranchName((int)GetEmployeeOBJ.BranchId, _Context);

        //                        }
        //                        GetEmployeeResponse.DepartmentID = GetEmployeeOBJ.DepartmentId;

        //                        if (GetEmployeeOBJ.DepartmentId != 0 && GetEmployeeOBJ.DepartmentId != null)
        //                        {
        //                            GetEmployeeResponse.DepartmentName = Common.GetDepartmentName((int)GetEmployeeOBJ.DepartmentId, _Context);

        //                        }


        //                        GetEmployeeResponse.Email = GetEmployeeOBJ.Email;

        //                        GetEmployeeResponse.Gender = GetEmployeeOBJ.Gender;

        //                        if (GetEmployeeOBJ.JobTitleId != 0 && GetEmployeeOBJ.JobTitleId != null)
        //                        {
        //                            GetEmployeeResponse.JobTitleName = Common.GetJobTitleName((int)GetEmployeeOBJ.DepartmentId, _Context);

        //                        }


        //                        GetEmployeeResponse.Mobile = GetEmployeeOBJ.Mobile;

        //                        GetEmployeeResponse.Photo = response.UserImageURL = baseURL + GetEmployeeOBJ.PhotoUrl;
        //                        /* "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(GetEmployeeOBJ.Id.ToString(), key)) + "&type=photo&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();*/







        //                        GetEmployeeResponseList.Add(GetEmployeeResponse);
        //                    }



        //                }

        //            }


        //            response.EmployeeInfoList = GetEmployeeResponseList;

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
        


    
        [HttpPost("AddEditJobTitle")]    //move to Hruser controller and has service
        public async Task<ActionResult<BaseResponseWithID>> AddEditJobTitle(JobTitleData request)
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
                        error.ErrorMSG = "Job Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
                            ID.Value = request.ID;


                            object[] param = new object[] { ID };
                            var JobTitleDB = _Context.Database.SqlQueryRaw<proc_JobTitleLoadByPrimaryKey_Result>("Exec proc_JobTitleLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();
                            if (JobTitleDB != null)
                            {
                                // Update

                                var JobTitleUpdateDB = await _Context.JobTitles.Where(x => x.Id == request.ID).FirstOrDefaultAsync();


                                if (JobTitleUpdateDB != null)
                                {
                                    JobTitleUpdateDB.Name = request.Name;
                                    JobTitleUpdateDB.Description = request.Description;
                                    JobTitleUpdateDB.Active = request.Active;
                                    JobTitleUpdateDB.ModifiedBy = validation.userID;
                                    JobTitleUpdateDB.ModifiedDate = DateTime.Now;
                                    await _Context.SaveChangesAsync();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Job Title!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Job Title  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert


                            var JobTitleDB = new JobTitle();


                            JobTitleDB.Name = request.Name;
                            JobTitleDB.Description = request.Description;
                            JobTitleDB.CreatedBy = validation.userID;
                            JobTitleDB.CreationDate = DateTime.Now;
                            JobTitleDB.ModifiedBy = validation.userID;
                            JobTitleDB.ModifiedDate = DateTime.Now;
                            JobTitleDB.Active = true;
                            _Context.JobTitles.Add(JobTitleDB);
                            var Res = await _Context.SaveChangesAsync();




                            if (Res > 0)
                            {

                                Response.ID = JobTitleDB.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Job Title!!";
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

        [HttpPost("EditEmployeeRole")]   //move to Hruser controller and has service
        public async Task<BaseResponseWithID> EditEmployeeRole(EmployeeRoleData request)
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

                    long UserID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["UserID"]) && long.TryParse(Request.Headers["UserID"], out UserID))
                    {
                        long.TryParse(Request.Headers["UserID"], out UserID);
                    }

                    if (Response.Result)
                    {
                        var user = _Context.HrUsers.Find((long)request.UserID);
                        if (user != null)
                        {
                            var ListUserRoleVM = request.userRolesData;
                            var ListUserRoleDB = await _Context.UserRoles.Where(x => x.HrUserId == request.UserID).ToListAsync();
                            if (ListUserRoleVM != null && ListUserRoleVM.Count() > 0)
                            {
                                var IDSListUserRoleDB = ListUserRoleDB.Select(x => x.Id).ToList();
                                var IDSListUserRoleVM = new List<long>();
                                foreach (var item in ListUserRoleVM)
                                {
                                    //if (item.ID != 0) // Edit
                                    //{
                                    //    var UpdateKeeperObjDB = ListUserRoleDB.Where(x => x.Id == item.ID).FirstOrDefault();
                                    //    UpdateKeeperObjDB.RoleId = item.RoleID;
                                    //    _Context.SaveChanges();
                                    //}
                                    //else //Insert
                                    //{
                                    var CheckIfExistBefore = ListUserRoleDB.Where(x => x.HrUserId == request.UserID && x.RoleId == item.RoleID).FirstOrDefault();
                                    if (CheckIfExistBefore == null)
                                    {
                                        if (user != null)
                                        {

                                            var InsertUserRolesObjDB = new UserRole();
                                            InsertUserRolesObjDB.HrUserId = request.UserID;
                                            InsertUserRolesObjDB.RoleId = item.RoleID;


                                            InsertUserRolesObjDB.CreatedBy = validation.userID;

                                            InsertUserRolesObjDB.CreationDate = DateTime.Now;
                                            if (user.IsUser)
                                            {
                                                InsertUserRolesObjDB.UserId = user.UserId;
                                            }
                                            _Context.UserRoles.Add(InsertUserRolesObjDB);
                                            var Res = _Context.SaveChanges();
                                            if (Res > 0)
                                            {
                                                IDSListUserRoleDB.Add(InsertUserRolesObjDB.Id);
                                                IDSListUserRoleVM.Add(InsertUserRolesObjDB.Id);
                                            }
                                        }
                                        //}
                                        else
                                        {
                                            IDSListUserRoleVM.Add(CheckIfExistBefore.Id);
                                        }
                                    }


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

                                var IDSListToRemove = IDSListUserRoleDB.Except(IDSListUserRoleVM).ToList();

                                var DeletedUserRoleListDB = ListUserRoleDB.Where(x => IDSListToRemove.Contains(x.Id)).ToList();
                                _Context.UserRoles.RemoveRange(DeletedUserRoleListDB);
                                _Context.SaveChanges();


                            }
                            else // List is empty must be deleted
                            {
                                // delete list from DB
                                _Context.UserRoles.RemoveRange(ListUserRoleDB);
                                _Context.SaveChanges();
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
                        error.ErrorMSG = "Faild To Insert this Role !!";
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
        [HttpPost("EditEmployeeRoleNew")]
        public async Task<BaseResponseWithId<long>> EditEmployeeRoleNew(EmployeeRoleData request)
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
                    _hrUserService.Validation = validation;
                    Response = await _hrUserService.EditEmployeeRoleNew(request);
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


        [HttpGet("GetJobTitle")]     //move to jobtitle controller and has service
        public GetJobTitleResponse GetJobTitle()
        {
            GetJobTitleResponse response = new GetJobTitleResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {


                    response = _adminService.GetJobTitle();

                   

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



        [HttpPost("AddEditEmployeeInfo")]       //move to hruser controller and has service
        public ActionResult<BaseResponseWithID> AddEditEmployeeInfo(EmployeeInfoDataList request)
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

                    if (string.IsNullOrEmpty(request.Email) || request.Email.Trim() == "")
                    {

                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P7";
                        error.ErrorMSG = "please write your Email";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.Password) || request.Password.Trim() == "")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your password";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.Mobile) || request.Mobile.Trim() == "")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your Mobile";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.FirstName) || request.FirstName.Trim() == "")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your FirstName";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.LastName) || request.LastName.Trim() == "")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your LastName";
                        Response.Errors.Add(error);
                    }
                    byte[] EmployeePhoto = null;
                    //if (string.IsNullOrEmpty(Request.Photo)){
                    if (request.Photo != null)
                    {
                        EmployeePhoto = Convert.FromBase64String(request.Photo);
                    }


                    if (Response.Result)
                    {


                        var modifiedUser = Common.GetUserName(validation.userID, _Context);


                        if (request.ID != 0 && request.ID != null)
                        {

                            var EmployeeDB = _Context.Users.Find((long)request.ID);

                            if (EmployeeDB != null)
                            {
                                DateTime DateOfBirth = DateTime.Now;
                                DateTime.TryParse(request.DateOfBirth, out DateOfBirth);
                                // Update
                                EmployeeDB.Password = Encrypt_Decrypt.Encrypt(request.Password, key);
                                EmployeeDB.FirstName = request.FirstName;
                                //mod by Gerges Abdullah
                                /*EmployeeDB.ArFirstName = request.ArFirstName;
                                EmployeeDB.ArMiddleName = request.ArMiddleName;
                                EmployeeDB.ArLastName = request.ArLastName;
                                EmployeeDB.LandLine = request.LandLine;
                                EmployeeDB.NationalityId = request.NationalityId;
                                EmployeeDB.MaritalStatusId = request.MaritalStatusId;
                                EmployeeDB.MilitaryStatusId = request.MilitaryStatuId;
                                EmployeeDB.DateOfBirth = DateOfBirth;*/
                                //----------------------------------------------------
                                EmployeeDB.LastName = request.LastName;
                                EmployeeDB.MiddleName = request.MiddleName;
                                EmployeeDB.Mobile = request.Mobile;
                                EmployeeDB.Active = request.Active;
                                EmployeeDB.Email = request.Email;
                                EmployeeDB.Age = request.Age;
                                if (request.Photo != null)
                                {
                                    EmployeeDB.Photo = EmployeePhoto;
                                }
                                EmployeeDB.Modified = DateTime.Now;
                                EmployeeDB.ModifiedBy = validation.userID;
                                EmployeeDB.BranchId = request.BranchID;
                                EmployeeDB.DepartmentId = request.DepartmentID;
                                EmployeeDB.JobTitleId = request.JobTitleID;
                                EmployeeDB.OldId = request.OldID;

                                var EmployeeDBUpdate = _Context.SaveChanges();

                                /* var EmployeeDBUpdate = _Context.proc_UserUpdate(request.ID,
                                                                                                    Encrypt_Decrypt.Encrypt(request.Password, key),
                                                                                                    request.FirstName,
                                                                                                    request.Email,
                                                                                                    request.Mobile,
                                                                                                    EmployeePhoto,
                                                                                                    request.Active,
                                                                                                    DateTime.Now,
                                                                                                    validation.userID,
                                                                                                    DateTime.Now,
                                                                                                     request.LastName,
                                                                                                     request.MiddleName,
                                                                                                     request.Age,
                                                                                                     request.Gender,
                                                                                                     validation.userID,
                                                                                                     request.BranchID,
                                                                                                     request.DepartmentID,
                                                                                                     request.JobTitleID,
                                                                                                     request.OldID
                                                                                                     );*/
                                if (EmployeeDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = (long)request.ID;
                                    if (request.Photo != null)
                                    {
                                        if (System.IO.File.Exists(_host.WebRootPath + '/' + EmployeeDB.PhotoUrl))
                                        {
                                            System.IO.File.Delete(_host.WebRootPath + '/' + EmployeeDB.PhotoUrl);
                                        }
                                        var FilePath = Common.SaveFile("Attachments/" + Request.Headers["CompanyName"] + "/UsersImages/", request.Photo, EmployeeDB.Id + "_" + EmployeeDB.FirstName + "_" + EmployeeDB.LastName, "png", _host);
                                        EmployeeDB.PhotoUrl = FilePath;
                                        _Context.SaveChanges();
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Employee!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Employee Doesn't Exist!!";
                                Response.Errors.Add(error);


                            }
                        }
                        else
                        {

                            DateTime DateOfBirth = DateTime.Now;
                            DateTime.TryParse(request.DateOfBirth, out DateOfBirth);
                            // Insert
                            var user = new User()
                            {
                                Password = Encrypt_Decrypt.Encrypt(request.Password, key),
                                FirstName = request.FirstName,

                                //mod by Gerges Adullah
                                /*ArFirstName = request.ArFirstName,
                                ArMiddleName = request.ArMiddleName,
                                ArLastName = request.ArLastName,
                                LandLine = request.LandLine,
                                NationalityId = request.NationalityId,
                                MilitaryStatusId = request.MilitaryStatuId,
                                MaritalStatusId = request.MaritalStatusId,
                                DateOfBirth = DateOfBirth,*/
                                //----------------------------------
                                LastName = request.LastName,
                                MiddleName = request.MiddleName,
                                Mobile = request.Mobile,
                                Active = request.Active,
                                Email = request.Email,
                                Age = request.Age,
                                Photo = EmployeePhoto,
                                Modified = DateTime.Now,
                                ModifiedBy = validation.userID,
                                BranchId = request.BranchID,
                                DepartmentId = request.DepartmentID,
                                JobTitleId = request.JobTitleID,
                                OldId = request.OldID,
                                CreatedBy = validation.userID,
                                CreationDate = DateTime.Now
                            };

                            /*  ObjectParameter EmployeeID = new ObjectParameter("ID", typeof(int));
                              var EmployeeInserted = _Context.proc_UserInsert(EmployeeID,
                                                                                                     Encrypt_Decrypt.Encrypt(Request.Password, key),
                                                                                                     Request.FirstName,
                                                                                                     Request.Email,
                                                                                                     Request.Mobile,
                                                                                                    EmployeePhoto,
                                                                                                     Request.Active,
                                                                                                     DateTime.Now,
                                                                                                     validation.userID,
                                                                                                     DateTime.Now,
                                                                                                     Request.LastName,
                                                                                                     Request.MiddleName,
                                                                                                     Request.Age,
                                                                                                     Request.Gender,
                                                                                                     validation.userID,
                                                                                                     Request.BranchID,
                                                                                                     Request.DepartmentID,
                                                                                                     Request.JobTitleID,
                                                                                                     Request.OldID
                                                                                             );*/
                            var newEmp = _Context.Users.Add(user);
                            var EmployeeInserted = _Context.SaveChanges();




                            long EInvoiceCompanyActivityInsertedID = 0;
                            if (EmployeeInserted > 0)
                            {
                                EInvoiceCompanyActivityInsertedID = user.Id;
                                Response.ID = EInvoiceCompanyActivityInsertedID;
                                if (request.Photo != null)
                                {
                                    var FilePath = Common.SaveFile("Attachments/" + Request.Headers["CompanyName"] + "/UsersImages/", request.Photo, user.Id + "_" + user.FirstName + "_" + user.LastName, "png", _host);
                                    user.PhotoUrl = FilePath;
                                    _Context.SaveChanges();
                                }
                            }

                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Employee !!";
                                Response.Errors.Add(error);
                            }


                        }
                    };




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

        
       
        [HttpGet("GetUserRoleAndGroup")]    //move to hruser controller and has service
        public async Task<ActionResult<GetUserRolegAndRoleResponse>> GetUserRoleAndGroup([FromHeader] int UserID = 0)
        {
            GetUserRolegAndRoleResponse response = new GetUserRolegAndRoleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                var GetUserRoleList = new List<GetUserRoleData>();
                var GetUserGroupList = new List<GetUserGroupData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        /*int UserID = 0;
                        if (!string.IsNullOrEmpty(Request.Headers["UserID"]) && int.TryParse(Request.Headers["UserID"], out UserID))
                        {
                            int.TryParse(Request.Headers["UserID"], out UserID);
                        }*/


                        var UserRoleDB = await _Context.UserRoles.Where(x => x.HrUserId == UserID).ToListAsync();
                        var UserGroupDB = await _Context.GroupUsers.Where(x => x.HrUserId == UserID).ToListAsync();

                        var roles = _Context.Roles.Where(x => UserRoleDB.Select(a => a.RoleId).Contains(x.Id)).ToList();
                        var Groups = _Context.Groups.Where(x => UserGroupDB.Select(a => a.GroupId).Contains(x.Id)).ToList();
                        var user = _Context.HrUsers.Where(x => x.Id == UserID).FirstOrDefault();
                        if (UserRoleDB != null && UserRoleDB.Count > 0)
                        {

                            foreach (var UserRoleOBJ in UserRoleDB)
                            {
                                var UserRoleOBJResponse = new GetUserRoleData();

                                UserRoleOBJResponse.ID = UserRoleOBJ.Id;

                                UserRoleOBJResponse.RoleName = roles.Where(x => x.Id == UserRoleOBJ.RoleId).FirstOrDefault()?.Name;

                                UserRoleOBJResponse.UserName = user?.FirstName + " " + user?.LastName;

                                UserRoleOBJResponse.RoleID = UserRoleOBJ.RoleId;


                                GetUserRoleList.Add(UserRoleOBJResponse);
                            }



                        }

                        if (UserGroupDB != null && UserGroupDB.Count > 0)
                        {

                            foreach (var UserGroupOBJ in UserGroupDB)
                            {
                                var UserGroupOBJResponse = new GetUserGroupData();

                                UserGroupOBJResponse.ID = UserGroupOBJ.Id;

                                UserGroupOBJResponse.GroupName = Groups.Where(x => x.Id == UserGroupOBJ.GroupId).FirstOrDefault()?.Name;

                                UserGroupOBJResponse.UserName = user?.FirstName + " " + user?.LastName;
                                UserGroupOBJResponse.GroupID = UserGroupOBJ.GroupId;


                                GetUserGroupList.Add(UserGroupOBJResponse);
                            }



                        }
                    }

                }
                response.UserRoleList = GetUserRoleList;
                response.UserGroupList = GetUserGroupList;

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


        [HttpGet("GetAbsenceTypeList")]     //move to hruser controller and has service
        public ActionResult<SelectDDLResponse> GetAbsenceTypeList()
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

                var List = new List<SelectDDL>();

                if (Response.Result)
                {

                    var ListDB = _Context.ContractLeaveSettings.ToList();
                    foreach (var item in ListDB)
                    {
                        var ItemCategoryrObj = new SelectDDL();
                        ItemCategoryrObj.ID = item.Id;
                        ItemCategoryrObj.Name = item.HolidayName;
                        List.Add(ItemCategoryrObj);
                    }


                    Response.DDLList = List;
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
     


        /*[HttpPost("AddContractDetails")]
        public ActionResult<BaseResponse> AddContractDetails(ContractDetailModel request)
        {
            BaseResponse Response = new BaseResponse();
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
                    if (request.Id != null)
                    {
                        //edit

                    }
                    else
                    {
                        //insert
                        var user = _Context.Users.Find(request.UserID);
                        if (user != null)
                        {
                            var detail = new ContractDetail()
                            {
                                UserId = request.UserID,
                                ContactTypeId = request.ContactTypeID,
                                StartDate = DateTime.Parse(request.StartDate),
                                EndDate = DateTime.Parse(request.EndDate),
                                ProbationPeriod = request.ProbationPeriod,
                                NoticedByCompany = request.NoticedByCompany,
                                NoticedByEmployee = request.NoticedByEmployee,
                                IsCurrent = request.IsCurrent,
                                IsAllowOverTime = request.IsAllowOverTime,
                                Isautomatic = request.IsAutomatic,
                                CreatedDate = DateTime.Now,
                                CreatedBy = validation.userID,
                                ModifiedBy = validation.userID,
                                ModifiedDate = DateTime.Now,

                            };
                            var contract = _Context.ContractDetails.Add(detail);
                            _Context.SaveChanges();

                            foreach (var leave in request.LeaveEmployees)
                            {
                                var contractLeaveEmp = new ContractLeaveEmployee()
                                {
                                    UserId = contract.Entity.UserId,
                                    ContractId = contract.Entity.Id,
                                    ContractLeaveSettingId = leave.ContractLeaveSettingID,
                                    Active = true,
                                    LeaveAllowed = leave.LeaveAllowed,
                                    Balance = leave.Balance,
                                    Used = leave.Used,
                                    Remain = leave.Remain,
                                    CreationDate = DateTime.Now,
                                    CreatedBy = validation.userID,
                                    ModifiedDate = DateTime.Now,
                                    ModifiedBy = validation.userID
                                };
                                _Context.ContractLeaveEmployees.Add(contractLeaveEmp);
                                _Context.SaveChanges();
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "User you Entered Is not Found";
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }*/
        [NonAction]
        public bool DeductEmployeAbsence(long EmployeeID, int AbsenceTypeID, bool IsDeduct)
        {
            bool Sucessed = false;
            if (EmployeeID != 0 && AbsenceTypeID != 0)
            {
                var EmployeeContractLeaveDB = _Context.ContractLeaveEmployees.Where(x => x.UserId == EmployeeID && x.ContractLeaveSettingId == AbsenceTypeID && x.LeaveAllowed == "Yes").FirstOrDefault();
                if (EmployeeContractLeaveDB != null)
                {
                    if (IsDeduct)
                    {
                        if (EmployeeContractLeaveDB.Remain > 0)
                        {
                            EmployeeContractLeaveDB.Used = EmployeeContractLeaveDB.Used + 1;
                            EmployeeContractLeaveDB.Remain = EmployeeContractLeaveDB.Remain - 1;
                            _Context.SaveChanges();
                            Sucessed = true;
                        }
                    }
                    else
                    {
                        if (EmployeeContractLeaveDB.Used > 0)
                        {
                            EmployeeContractLeaveDB.Used = EmployeeContractLeaveDB.Used - 1;
                            EmployeeContractLeaveDB.Remain = EmployeeContractLeaveDB.Remain + 1;
                            _Context.SaveChanges();
                            Sucessed = true;
                        }
                    }

                }
            }

            return Sucessed;
        }
        [NonAction]
        public bool CheckISAllowOverTimeAutomatic(long EmployeeID)
        {
            bool ISAllowOverTimeAutomatic = false;
            var ContractDetailOBJDB = _Context.ContractDetails.Where(x => x.UserId == EmployeeID && x.IsAllowOverTime && x.IsCurrent && x.Isautomatic).FirstOrDefault();

            if (ContractDetailOBJDB != null)
            {
                ISAllowOverTimeAutomatic = true;
            }

            return ISAllowOverTimeAutomatic;
        }

        [NonAction]
        public bool CheckISAttendanceDateOFFDayOrWeekEnd(DateTime AttendanceDate)
        {
            bool ISAttendanceDateOFFDayOrWeekEnd = false;
            var OffDay = _Context.OffDays.Where(x => x.Day.Year == AttendanceDate.Year && x.Day.Month == AttendanceDate.Month
                                                                  && x.Day.Day == AttendanceDate.Day && x.Active == true && x.AllowWorking == true).FirstOrDefault();

            var WeekEnd = _Context.WeekEnds.Where(x => x.Day.ToLower() == AttendanceDate.DayOfWeek.ToString().ToLower()).FirstOrDefault();

            if (OffDay != null || WeekEnd != null)
            {
                ISAttendanceDateOFFDayOrWeekEnd = true;
            }

            return ISAttendanceDateOFFDayOrWeekEnd;
        }
        [NonAction]
        public decimal CalculdateOffDayOverTime(int? CheckHourIn, int? CheckHourOut, int? CheckMinuteIn, int? CheckMinuteOut)
        {
            decimal OverTimeHour = 0;
            CheckHourIn = CheckHourIn != null ? (int)CheckHourIn : 0;
            CheckHourOut = CheckHourOut != null ? (int)CheckHourOut : 0;
            CheckMinuteIn = CheckMinuteIn != null ? (int)CheckMinuteIn : 0;
            CheckMinuteOut = CheckMinuteOut != null ? (int)CheckMinuteOut : 0;

            decimal CheckInTime = ((decimal)CheckHourIn * 60 + (decimal)CheckMinuteIn) / 60;
            decimal CheckOutTime = ((decimal)CheckHourOut * 60 + (decimal)CheckMinuteOut) / 60;
            if (CheckOutTime > CheckInTime)
            {
                OverTimeHour = CheckOutTime - CheckInTime;
            }

            return OverTimeHour;
        }

        [NonAction]
        public int CalcWorkingDaysPerMonth(DateTime AttendanceDate)
        {
            int WorkingDays = 0;
            int NOOfDaysPerMonth = DateTime.DaysInMonth(AttendanceDate.Year, AttendanceDate.Month);
            for (int Counter = 1; Counter <= NOOfDaysPerMonth; Counter++)
            {
                DateTime PayslipDatePerDay = new DateTime(AttendanceDate.Year, AttendanceDate.Month, AttendanceDate.Day);
                if (!CheckISAttendanceDateOFFDayOrWeekEnd(PayslipDatePerDay))
                {
                    WorkingDays++;
                }
            }
            return WorkingDays;
        }

        [NonAction]
        public bool UpdateEmployeeAttendencePayslip(long AttendenceID, BeforeUpdatRecordEmployeeHours OldRecord, long Creator)
        {
            decimal OverTimePerDay = 0;
            decimal DelayTimePerDay = 0;
            bool IsSuccessed = false;
            var UserAttendance = _Context.Attendances.Find(AttendenceID);
            // var UserAttendance = _context.proc_AttendanceLoadAll().Where(x => x.Active == true && x.EmployeeID == EmployeeID && x.AttendanceDate == PaySlipDate).FirstOrDefault();
            if (UserAttendance != null)
            {
                long EmployeeID = (long)UserAttendance.EmployeeId;
                bool CheckISAllowOverTime = CheckISAllowOverTimeAutomatic(EmployeeID);
                DateTime PaySlipDate = DateTime.Now;


                // Check if WeekEnd to calculate hours work is overtime + attendance overtime
                // Check if OffDay  to calculate Hour  work is overtime + attendance overtime
                // Check if subtract Attendatce Day with latest payslip date if bigger than offday with week end or absence ... is not completed 

                // Check if allow OverTime
                if (CheckISAllowOverTime)
                {
                    // OffDay or weekend
                    if (CheckISAttendanceDateOFFDayOrWeekEnd(UserAttendance.AttendanceDate.ToDateTime(TimeOnly.Parse("8:00 Am"))))
                    {
                        OverTimePerDay += CalculdateOffDayOverTime(UserAttendance.CheckInHour, UserAttendance.CheckOutHour, UserAttendance.CheckInMin, UserAttendance.CheckOutMin);
                    }
                }


                // 


                var EmployeeAttendancePayslipOBJDB = _Context.AttendancePaySlips.Where(x => x.EmployeeUserId == EmployeeID && x.CreationDate.Year == PaySlipDate.Year &&
                                                                                                             x.CreationDate.Month == PaySlipDate.Month).FirstOrDefault();
                if (CheckISAllowOverTime)
                {
                    if (UserAttendance.OverTimeHour != null && UserAttendance.OverTimeMin != null)
                    {
                        OverTimePerDay += ((decimal)UserAttendance.OverTimeHour * 60 + (decimal)UserAttendance.OverTimeMin) / 60;
                    }

                    if (UserAttendance.DelayHours != null && UserAttendance.DelayMin != null)
                    {
                        DelayTimePerDay += ((decimal)UserAttendance.DelayHours * 60 + (decimal)UserAttendance.DelayMin) / 60;
                    }
                }
                if (EmployeeAttendancePayslipOBJDB == null) //  Insert First Day in SelectedMonth on payslip
                {
                    if (UserAttendance.AbsenceTypeId == null)
                    {
                        int WorkingDays = CalcWorkingDaysPerMonth(UserAttendance.AttendanceDate.ToDateTime(TimeOnly.Parse("8:00 Am")));
                        var attendancePaySlip = new AttendancePaySlip()
                        {
                            EmployeeUserId = EmployeeID,
                            PaySlipDate = DateTime.Now,
                            Rev = 1,
                            IsCompleted = false,
                            OverTimeSum = OverTimePerDay < 0 ? 0 : OverTimePerDay,
                            DelaySum = DelayTimePerDay < 0 ? 0 : DelayTimePerDay,
                            CreationDate = DateTime.Now,
                            CreatedyBy = Creator,
                            ModifiedBy = Creator,
                            ModifiedDate = DateTime.Now,
                            NoOfworkingDays = WorkingDays
                        };
                        var PaySlipInsertion = _Context.AttendancePaySlips.Add(attendancePaySlip);
                        /*ObjectParameter PaySlipId = new ObjectParameter("ID", typeof(long));
                        var PaySlipInsertion = _Context.proc_AttendancePaySlipInsert(PaySlipId,
                                                                                       EmployeeID,
                                                                                       DateTime.Now,
                                                                                       1,
                                                                                       false,
                                                                                       OverTimePerDay < 0 ? 0 : OverTimePerDay,
                                                                                       DelayTimePerDay < 0 ? 0 : DelayTimePerDay,
                                                                                       DateTime.Now,
                                                                                       Creator,
                                                                                       DateTime.Now,
                                                                                       Creator,
                                                                                       WorkingDays);*/
                        if (PaySlipInsertion != null)
                        {
                            IsSuccessed = true;
                        }
                        _Context.SaveChanges();
                    }

                }// Update
                else
                {
                    if (!EmployeeAttendancePayslipOBJDB.IsCompleted)
                    {
                        if (CheckISAllowOverTime)
                        {

                            // merge OverTime - Delay
                            OverTimePerDay += EmployeeAttendancePayslipOBJDB.OverTimeSum;
                            DelayTimePerDay += EmployeeAttendancePayslipOBJDB.DelaySum;
                        }


                        var LastPayslipDateForThisUser = EmployeeAttendancePayslipOBJDB.PaySlipDate;
                        bool isCompleted = EmployeeAttendancePayslipOBJDB.IsCompleted;
                        int REV = EmployeeAttendancePayslipOBJDB.Rev;
                        if (LastPayslipDateForThisUser.Year == UserAttendance.AttendanceDate.Year && LastPayslipDateForThisUser.Month == UserAttendance.AttendanceDate.Month)
                        {
                            if (EmployeeAttendancePayslipOBJDB.NoOfworkingDays <= EmployeeAttendancePayslipOBJDB.Rev - 1)
                            {
                                isCompleted = false;
                            }
                            if (OldRecord != null)
                            {

                                // Case 1 : Check if old attend time  then update another attend time   => update Overtime and Delay 
                                if ((OldRecord.AbsenceTypeId == null || OldRecord.AbsenceTypeId == 0) && (UserAttendance.AbsenceTypeId == null || UserAttendance.AbsenceTypeId == 0))
                                {
                                    if (CheckISAllowOverTime)
                                    {
                                        decimal OldOverTime = ((decimal)OldRecord.OverTimeHours * 60 + OldRecord.OverTimeMin) / 60;
                                        decimal OldDelayTime = ((decimal)OldRecord.DelayHours * 60 + OldRecord.DeleyMin) / 60;

                                        OverTimePerDay -= OldOverTime;
                                        DelayTimePerDay -= OldDelayTime;
                                    }
                                }
                                // Case 2 : Check if old Attend time then Update to absence       => roll back overTime and Delay time and decrease (Rev)
                                else if ((OldRecord.AbsenceTypeId == null || OldRecord.AbsenceTypeId == 0) && UserAttendance.AbsenceTypeId != null && UserAttendance.AbsenceTypeId != 0)
                                {
                                    if (CheckISAllowOverTime)
                                    {
                                        decimal OldOverTime = ((decimal)OldRecord.OverTimeHours * 60 + OldRecord.OverTimeMin) / 60;
                                        decimal OldDelayTime = ((decimal)OldRecord.DelayHours * 60 + OldRecord.DeleyMin) / 60;

                                        OverTimePerDay -= OldOverTime;
                                        DelayTimePerDay -= OldDelayTime;
                                    }
                                    REV = REV > 0 ? REV - 1 : 0;
                                }
                                // Case 4 : Check if old Absence then Update to attend       =>  update rev
                                else if (OldRecord.AbsenceTypeId != null && OldRecord.AbsenceTypeId != 0 && (UserAttendance.AbsenceTypeId == null || UserAttendance.AbsenceTypeId == 0))
                                {
                                    REV = REV + 1;
                                }
                                // Case 3 : Check if old Absence then Update to another absence       => No update

                                var PaySlipUpdate = _Context.AttendancePaySlips.Find(EmployeeAttendancePayslipOBJDB.Id);
                                PaySlipUpdate.EmployeeUserId = EmployeeAttendancePayslipOBJDB.EmployeeUserId;
                                PaySlipUpdate.PaySlipDate = DateTime.Now;
                                PaySlipUpdate.Rev = REV;
                                PaySlipUpdate.IsCompleted = isCompleted;
                                PaySlipUpdate.OverTimeSum = OverTimePerDay < 0 ? 0 : OverTimePerDay;
                                PaySlipUpdate.DelaySum = DelayTimePerDay < 0 ? 0 : DelayTimePerDay;
                                PaySlipUpdate.CreationDate = EmployeeAttendancePayslipOBJDB.CreationDate;
                                PaySlipUpdate.CreatedyBy = EmployeeAttendancePayslipOBJDB.CreatedyBy;
                                PaySlipUpdate.ModifiedDate = DateTime.Now;
                                PaySlipUpdate.CreatedyBy = Creator;
                                PaySlipUpdate.NoOfworkingDays = EmployeeAttendancePayslipOBJDB.NoOfworkingDays;


                                var PaySlipUpdatation = _Context.SaveChanges();



                                /*var PaySlipUpdatation = _Context.proc_AttendancePaySlipUpdate(EmployeeAttendancePayslipOBJDB.ID,
                                                                                               EmployeeAttendancePayslipOBJDB.EmployeeUserId,
                                                                                               DateTime.Now,
                                                                                               REV,
                                                                                               isCompleted,
                                                                                               OverTimePerDay < 0 ? 0 : OverTimePerDay,
                                                                                               DelayTimePerDay < 0 ? 0 : DelayTimePerDay,
                                                                                               EmployeeAttendancePayslipOBJDB.CreationDate,
                                                                                               EmployeeAttendancePayslipOBJDB.CreatedyBy,
                                                                                               DateTime.Now,
                                                                                               Creator,
                                                                                               EmployeeAttendancePayslipOBJDB.NoOfworkingDays);*/
                                if (PaySlipUpdatation > 0)
                                {
                                    IsSuccessed = true;
                                }

                            }
                            else // 
                            // Update Payslip Not Abscence
                            {
                                if (UserAttendance.AbsenceTypeId == null || UserAttendance.AbsenceTypeId == 0)
                                {
                                    var PaySlipUpdate = _Context.AttendancePaySlips.Find(EmployeeAttendancePayslipOBJDB.Id);
                                    PaySlipUpdate.EmployeeUserId = EmployeeAttendancePayslipOBJDB.EmployeeUserId;
                                    PaySlipUpdate.PaySlipDate = DateTime.Now;
                                    PaySlipUpdate.Rev = REV + 1;
                                    PaySlipUpdate.IsCompleted = isCompleted;
                                    PaySlipUpdate.OverTimeSum = OverTimePerDay < 0 ? 0 : OverTimePerDay;
                                    PaySlipUpdate.DelaySum = DelayTimePerDay < 0 ? 0 : DelayTimePerDay;
                                    PaySlipUpdate.CreationDate = EmployeeAttendancePayslipOBJDB.CreationDate;
                                    PaySlipUpdate.CreatedyBy = EmployeeAttendancePayslipOBJDB.CreatedyBy;
                                    PaySlipUpdate.ModifiedDate = DateTime.Now;
                                    PaySlipUpdate.CreatedyBy = Creator;
                                    PaySlipUpdate.NoOfworkingDays = EmployeeAttendancePayslipOBJDB.NoOfworkingDays;


                                    var PaySlipUpdatation = _Context.SaveChanges();


                                    /* var PaySlipUpdatation = _Context.proc_AttendancePaySlipUpdate(EmployeeAttendancePayslipOBJDB.ID,
                                                                EmployeeAttendancePayslipOBJDB.EmployeeUserId,
                                                                DateTime.Now,
                                                                REV + 1,
                                                                isCompleted,
                                                                OverTimePerDay < 0 ? 0 : OverTimePerDay,
                                                                DelayTimePerDay < 0 ? 0 : DelayTimePerDay,
                                                                EmployeeAttendancePayslipOBJDB.CreationDate,
                                                                EmployeeAttendancePayslipOBJDB.CreatedyBy,
                                                                DateTime.Now,
                                                                Creator,
                                                                EmployeeAttendancePayslipOBJDB.NoOfworkingDays);*/
                                    if (PaySlipUpdatation > 0)
                                    {
                                        IsSuccessed = true;
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return IsSuccessed;
        }

        [NonAction]
        public bool UpdateEmployeAbsence(long EmployeeID, int AbsenceTypeID, int? ExistAbsenceType)
        {
            bool Sucessed = false;
            if (EmployeeID != 0 && AbsenceTypeID != 0)
            {
                if (ExistAbsenceType == null) // Insert First Time
                {
                    Sucessed = DeductEmployeAbsence(EmployeeID, AbsenceTypeID, true);
                }
                else // update 
                {
                    if (ExistAbsenceType != 0) // Change from absence  To another  Absence 
                    {
                        // check if have balance on AbsenceTypeID and decrease balance .... then increase balance from old absnece again
                        bool CeckHaveBalanceOnNewAbsnce = DeductEmployeAbsence(EmployeeID, AbsenceTypeID, true);
                        if (CeckHaveBalanceOnNewAbsnce)
                        {
                            Sucessed = DeductEmployeAbsence(EmployeeID, (int)ExistAbsenceType, false);
                        }
                    }
                    else if (ExistAbsenceType == 0)
                    {
                        // employee take this day attend then update to absence .... check if have balance
                        Sucessed = DeductEmployeAbsence(EmployeeID, AbsenceTypeID, true);
                    }
                }


            }

            return Sucessed;
        }

        //was GetUserAbsenceDetails
        [HttpGet("GetEmployeeAbsenceDetails")]
        public ActionResult<GetAbsenceDetailsResponse> GetEmployeeAbsenceDetails([FromHeader] long UserId = 0)
        {
            GetAbsenceDetailsResponse Response = new GetAbsenceDetailsResponse();
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
                    /*long UserId = 0;
                    if (string.IsNullOrEmpty(Request.Headers["UserId"]) || !long.TryParse(Request.Headers["UserId"], out UserId))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "InventoryItemID Is required";
                        Response.Errors.Add(error);
                    }*/
                    if (Response.Result)
                    {
                        var AbsenceDetails = new List<AbsenceDetailsViewModel>();
                        var details = _Context.ContractLeaveEmployees.Where(x => x.UserId == UserId);

                        if (details != null)
                        {
                            foreach (var item in details)
                            {
                                var absence = new AbsenceDetailsViewModel();
                                var holiday = _Context.ContractLeaveSettings.Where(x => x.Id == item.ContractLeaveSettingId).FirstOrDefault();
                                absence.ContractLeaveSettingID = holiday.Id;
                                absence.HolidayName = holiday.HolidayName;
                                absence.leaveAllowed = item.LeaveAllowed;
                                absence.balance = (int)item.Balance;
                                absence.remain = (int)item.Remain;
                                absence.used = (int)item.Used;
                                AbsenceDetails.Add(absence);
                            }
                            Response.Data = AbsenceDetails;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Invalid Inventory ItemID";
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        //[HttpGet("CheckUserDuplicates")]
        /*        public async Task<ActionResult<CheckUserDuplicatesResponse>> CheckDuplicates(CheckDuplicatesModel H)
                {
                    CheckUserDuplicatesResponse Response = new CheckUserDuplicatesResponse();
                    Response.Result = true;
                    Response.Errors = new List<Error>();
                    try
                    {
                        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                        Response.Errors = validation.errors;
                        Response.Result = validation.result;
                        if (Response.Result)
                        {
                            List<UserDuplicatesModel> duplicatesArNames, duplicatesNames, duplicatesEmails, duplicatesMobiles;
                            duplicatesArNames = duplicatesNames = duplicatesEmails = duplicatesMobiles = new List<UserDuplicatesModel>();
                            if (H.FirstName is not null && H.MiddleName is not null && H.LastName is not null)
                            {
                                duplicatesNames = _Context.Users.Where(x => x.LastName.Equals(H.LastName) && x.FirstName.Equals(H.FirstName) && x.MiddleName.Equals(H.MiddleName)).Select(x => new UserDuplicatesModel { Email = x.Email, Id = x.Id }).ToList();
                            }
                            if (H.ARFirstName is not null && H.ARMiddleName is not null && H.ARLastName is not null)
                            {
                                duplicatesArNames = _Context.Users.Where(x => x.ArFirstName == H.ARFirstName && x.ArMiddleName == H.ARMiddleName && x.ArLastName == H.ARLastName).Select(x => new UserDuplicatesModel { Email = x.Email, Id = x.Id }).ToList();
                            }
                            if (H.Email is not null)
                            {
                                duplicatesEmails = _Context.Users.Where(x => x.Email == H.Email).Select(x => new UserDuplicatesModel { Email = x.Email, Id = x.Id }).ToList();
                            }
                            if (H.Mobile is not null)
                            {
                                duplicatesMobiles = _Context.Users.Where(x => x.Mobile.Equals(H.Mobile)).Select(x => new UserDuplicatesModel { Email = x.Email, Id = x.Id }).ToList();
                            }
                            Response.UserDuplicates =  new List<List<UserDuplicatesModel>> { duplicatesNames, duplicatesArNames, duplicatesEmails, duplicatesMobiles };

                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Error Occured";
                            Response.Errors.Add(error);
                        }
                        return Response;
                    }
                    catch(Exception ex)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        Response.Errors.Add(error);
                        return Response;
                    }

                }
        */

    }

}
