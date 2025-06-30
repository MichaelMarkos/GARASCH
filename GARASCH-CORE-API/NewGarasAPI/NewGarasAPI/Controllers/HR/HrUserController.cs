using AutoMapper;
using Azure;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Extensions.Hosting;
using NewGaras.Domain.DTO.HrUser;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services;
using NewGaras.Domain.Services.Family;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.DTO.VacationType;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.HR;
using NewGaras.Infrastructure.Models.HrUser;
using NewGaras.Infrastructure.Models.Mail;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.Notification;
using System.Reflection;



namespace NewGarasAPI.Controllers.HR
{
    [Route("[controller]")]
    [ApiController]
    public class HrUserController : ControllerBase
    {
        //private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        static string key;
        private GarasTestContext _Context;
        private readonly IMapper _mapper;
        private readonly ILogService _logService;
        private readonly IHrUserService _hrUserService;
        private readonly IMailService _mailService;
        private readonly ITenantService _tenantService;
        private readonly INotificationService _notificationService;
        public HrUserController(IUnitOfWork unitOfWork, IWebHostEnvironment host, IMapper mapper, ILogService logService, IHrUserService hrUserService, IMailService mailService,ITenantService tenantService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _logService = logService;
            _hrUserService = hrUserService;
            _mailService = mailService;
            _notificationService = notificationService;
        }


        #region APIs without services
        //[HttpGet("GetHrUser")]
        //public async Task<BaseResponseWithData<GetHrUserDto>> GetUser([FromHeader] long id)
        //{
        //    var response = new BaseResponseWithData<GetHrUserDto>();
        //    response.Result = true;
        //    response.Errors = new List<Error>();

        //    #region user Auth
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //    response.Errors = validation.errors;
        //    response.Result = validation.result;
        //    #endregion


        //    try
        //    {
        //        if (response.Result)
        //        {
        //            var UserDtoData = await _unitOfWork.HrUsers.FindAsync((HU => HU.Id == id), new[] { "User", "Department", "JobTitle", "Branch" });
        //            response.Data = _mapper.Map<GetHrUserDto>(UserDtoData);
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error err = new Error();
        //        err.ErrorCode = "E-1";
        //        err.errorMSG = "Exception :" + ex.Message;
        //        response.Errors.Add(err);
        //        return response;
        //    }
        //}


        //[HttpGet("GetUserCards")]
        //public async Task<BaseResponseWithData<List<HrUserCardDto>>> GetAll()
        //{
        //    var response = new BaseResponseWithData<List<HrUserCardDto>>();
        //    response.Result = true;
        //    response.Errors = new List<Error>();

        //    #region user Auth
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //    response.Errors = validation.errors;
        //    response.Result = validation.result;
        //    #endregion

        //    string BaseURL = "https://garascore.garassolutions.com\\";

        //    try
        //    {
        //        if (response.Result)
        //        {
        //            var data = await _unitOfWork.HrUsers.FindAllAsync((a => a.Active == true), new[] { "JobTitle" });
        //            response.Data = data.Select(x => new HrUserCardDto
        //            {
        //                Id = x.Id,
        //                FirstName = x.FirstName,
        //                MiddleName = x.MiddleName,
        //                LastName = x.LastName,
        //                Email = x.Email,
        //                Mobile = x.Mobile,
        //                JobTitle = x.JobTitle?.Name,
        //                ImgPath = x.ImgPath != null ? BaseURL + x.ImgPath : null
        //            }).ToList();
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error err = new Error();
        //        err.ErrorCode = "E-1";
        //        err.errorCode = "Exception :" + ex.Message;
        //        response.Errors.Add(err);
        //        return response;
        //    }
        //}


        //[HttpPost("CreateHrUser")]
        //public async Task<BaseResponseWithId<long>> CreateHrUser([FromForm] HrUserDto NewHrUser)
        //{
        //    var response = new BaseResponseWithId<long>()
        //    {
        //        Result = true,
        //        Errors = new List<Error>()
        //    };

        //    #region user Auth
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //    response.Errors = validation.errors;
        //    response.Result = validation.result;
        //    #endregion

        //    try
        //    {
        //        IFormFile ImgInMemory = null;
        //        var savedpath = "";
        //        if (NewHrUser.Photo != null)
        //        {
        //            ImgInMemory = NewHrUser.Photo;
        //        }
        //        var user = _mapper.Map<HrUser>(NewHrUser);
        //        user.ImgPath = null; //Common.SaveFileIFF(virtualPath, file, FileName, fileExtension, _host);
        //        user.CreationDate = DateTime.Now;
        //        user.ModifiedById = validation.userID;
        //        user.CreatedById = validation.userID;
        //        user.Modified = DateTime.Now;
        //        //user.OldId = 0;
        //        var HrUser = _unitOfWork.HrUsers.Add(user);
        //        _unitOfWork.Complete();
        //        response.ID = HrUser.Id;
        //        long lastUserId = HrUser.Id;

        //        if (ImgInMemory != null)
        //        {
        //            var fileExtension = ImgInMemory.FileName.Split('.').Last();
        //            var virtualPath = $"Attachments\\{validation.CompanyName}\\HrUser\\{HrUser.Id}\\";
        //            var FileName = System.IO.Path.GetFileNameWithoutExtension(ImgInMemory.FileName);
        //            HrUser.ImgPath = Common.SaveFileIFF(virtualPath, ImgInMemory, FileName, fileExtension, _host);
        //        }
        //        #region commented loops
        //        //foreach (var item in NewHrUser.hrContactInfos)
        //        //{
        //        //    var contact = _mapper.Map<HrContactInfo>(item);
        //        //    contact.HrUserId = lastUserId;
        //        //    _unitOfWork.ContactInfos.Add(contact);
        //        //    _unitOfWork.Complete();
        //        //}
        //        //foreach (var item in NewHrUser.hrUserAddresses)
        //        //{
        //        //    var address = _mapper.Map<HrUserAddress>(item);
        //        //    address.HrUserId = lastUserId;
        //        //    _unitOfWork.UserAddresses.Add(address);
        //        //    _unitOfWork.Complete();
        //        //}
        //        #endregion
        //        _unitOfWork.Complete();
        //        _logService.AddLog("Add New Employee", "HrUser", "", (int)response.ID, validation.userID);
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error err = new Error();
        //        err.ErrorCode = "E-1";
        //        err.errorMSG = "Exception :" + ex.Message;
        //        response.Errors.Add(err);
        //        return response;
        //    }

        //}


        //[HttpPost]
        //public async Task


        //[HttpPost("AddHrEmployeeToUser")]
        //public async Task<BaseResponseWithId<long>> AddHrEmployeeToUser([FromBody] AddHrEmployeeToUserDTO InData)
        //{
        //    var response = new BaseResponseWithId<long>()
        //    {
        //        Result = true,
        //        Errors = new List<Error>()
        //    };

        //    #region user Auth
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //    response.Errors = validation.errors;
        //    response.Result = validation.result;
        //    #endregion

        //    try
        //    {
        //        if (response.Result)
        //        {
        //            var HrUser = await _unitOfWork.HrUsers.FindAsync((HU => HU.Id == InData.HrUserId), new[] { "Department", "JobTitle", "Branch" });
        //            if (HrUser.IsUser == true && HrUser.UserId != null)
        //            {
        //                response.Result = false;
        //                Error err = new Error();
        //                err.ErrorCode = "E-20";
        //                err.errorMSG = "This HR Employee is already a User";
        //                response.Errors.Add(err);
        //                return response;
        //            }
        //            if (HrUser != null)
        //            {
        //                NewGaras.Infrastructure.Entities.User usr = new NewGaras.Infrastructure.Entities.User();

        //                if (InData.Email == null || InData.Email == "")
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-20";
        //                    err.errorMSG = "Please, enter a valid Email";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }
        //                usr.Email = InData.Email;

        //                if (InData.Password == null || InData.Password == "")
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-20";
        //                    err.errorMSG = "Please, enter a valid password";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }
        //                if (InData.ConfirmPass == null || InData.ConfirmPass == "")
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-20";
        //                    err.errorMSG = "Please, enter a valid Confirm password";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }
        //                if (InData.Password == InData.ConfirmPass)
        //                {
        //                    usr.Password = Encrypt_Decrypt.Encrypt(InData.Password, key);
        //                }
        //                else
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-20";
        //                    err.errorMSG = "The password and confirm password did not match!";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }

        //                #region add Data to user table
        //                usr.FirstName = HrUser.FirstName;
        //                usr.LastName = HrUser.LastName;
        //                usr.MiddleName = HrUser.MiddleName;
        //                usr.Mobile = HrUser.Mobile;
        //                usr.Active = HrUser.Active;
        //                usr.CreationDate = DateTime.Now;
        //                usr.CreatedBy = validation.userID;
        //                usr.ModifiedBy = validation.userID;
        //                usr.Modified = DateTime.Now;
        //                usr.Gender = HrUser.Gender;
        //                usr.BranchId = HrUser.BranchId;
        //                usr.DepartmentId = HrUser.DepartmentId;
        //                usr.JobTitleId = HrUser.JobTitleId;
        //                usr.PhotoUrl = HrUser.ImgPath;
        //                #endregion

        //                var User = _unitOfWork.Users.Add(usr);
        //                _unitOfWork.Complete();

        //                HrUser.IsUser = true;
        //                HrUser.UserId = User.Id;

        //                _unitOfWork.Complete();

        //                response.ID = User.Id;

        //                Common.CreateNotification(validation.userID, "Welcome , " + HrUser.FirstName, "Thank you for joining our system", "#", true, usr.Id, 0, _Context);
        //            }
        //            else
        //            {
        //                response.Result = false;
        //                Error err = new Error();
        //                err.ErrorCode = "E-1";
        //                err.errorMSG = "The ID of HrUser is Not Valid , no HrUser with this Id";
        //                response.Errors.Add(err);
        //                return response;
        //            }
        //        }

        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error err = new Error();
        //        err.ErrorCode = "E-1";
        //        err.errorMSG = "Exception :" + ex.Message;
        //        response.Errors.Add(err);
        //        return response;
        //    }

        //}


        //[HttpPost("EditHrEmployee")]
        //public async Task<BaseResponseWithId<long>> EditHrEmployee([FromForm] EditHrEmployeeDto NewHrData)
        //{
        //    var response = new BaseResponseWithId<long>()
        //    {
        //        Result = true,
        //        Errors = new List<Error>()
        //    };

        //    #region user Auth
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //    response.Errors = validation.errors;
        //    response.Result = validation.result;
        //    #endregion


        //    try
        //    {
        //        if (response.Result)
        //        {
        //            var HrUser = await _unitOfWork.HrUsers.FindAsync((HU => HU.Id == NewHrData.HrUserId), new[] { "User" });

        //            IFormFile ImgInMemory = null;
        //            var savedpath = "";
        //            if (NewHrData.Photo != null)
        //            {
        //                ImgInMemory = NewHrData.Photo;
        //            }

        //            if (HrUser != null)
        //            {
        //                #region validation 
        //                if (NewHrData.FirstName == null || NewHrData.FirstName == "")
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-1";
        //                    err.errorMSG = "please, Enter a valid FrstName :";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }
        //                if (NewHrData.ARLastName == null || NewHrData.ARLastName == "")
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-1";
        //                    err.errorMSG = "please, Enter a valid ArlastName :";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }
        //                if (NewHrData.LastName == null || NewHrData.LastName == "")
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-1";
        //                    err.errorMSG = "please, Enter a valid lastName :";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }
        //                if (NewHrData.MiddleName == null || NewHrData.MiddleName == "")
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-1";
        //                    err.errorMSG = "please, Enter a valid MiddleName :";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }
        //                if (NewHrData.Mobile == null || NewHrData.Mobile == "")
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-1";
        //                    err.errorMSG = "please, Enter a valid Mobile number :";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }
        //                if (NewHrData.Email == null || NewHrData.Email == "")
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-1";
        //                    err.errorMSG = "please, Enter a valid Email:";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }
        //                if (NewHrData.Gender == null || NewHrData.Gender == "")
        //                {
        //                    response.Result = false;
        //                    Error err = new Error();
        //                    err.ErrorCode = "E-1";
        //                    err.errorMSG = "please, Enter a valid Gender :";
        //                    response.Errors.Add(err);
        //                    return response;
        //                }
        //                if (NewHrData.IsUser == true)
        //                {
        //                    if (NewHrData.systemEmail == null || NewHrData.systemEmail == "")
        //                    {
        //                        response.Result = false;
        //                        Error err = new Error();
        //                        err.ErrorCode = "E-1";
        //                        err.errorMSG = "please, Enter a valid user Email :";
        //                        response.Errors.Add(err);
        //                        return response;
        //                    }
        //                    if (NewHrData.password == null || NewHrData.password == "")
        //                    {
        //                        response.Result = false;
        //                        Error err = new Error();
        //                        err.ErrorCode = "E-1";
        //                        err.errorMSG = "please, Enter a valid password :";
        //                        response.Errors.Add(err);
        //                        return response;
        //                    }
        //                    if (NewHrData.confirmPassword == null || NewHrData.confirmPassword == "")
        //                    {
        //                        response.Result = false;
        //                        Error err = new Error();
        //                        err.ErrorCode = "E-1";
        //                        err.errorMSG = "please, Enter a valid Confirm password :";
        //                        response.Errors.Add(err);
        //                        return response;
        //                    }
        //                }

        //                #endregion

        //                DateTime ForCheckOnly = new DateTime(1900, 01, 01);

        //                HrUser.FirstName = NewHrData.FirstName;
        //                //HrUser.Active = NewHrData.Active;
        //                HrUser.ModifiedById = validation.userID;
        //                HrUser.Modified = DateTime.Now;
        //                HrUser.ArlastName = NewHrData.ARLastName;
        //                HrUser.LastName = NewHrData.LastName;
        //                HrUser.MiddleName = NewHrData.MiddleName;
        //                //HrUser.IsUser = NewHrData
        //                HrUser.Mobile = NewHrData.Mobile;
        //                HrUser.Email = NewHrData.Email;
        //                HrUser.Gender = NewHrData.Gender;
        //                //------------------------------CanBeNull-------------------
        //                HrUser.ArfirstName = NewHrData.ARFirstName;
        //                HrUser.ArmiddleName = NewHrData.ARMiddleName;
        //                HrUser.BranchId = NewHrData.BranchID;
        //                HrUser.DepartmentId = NewHrData.DepartmentID;
        //                HrUser.JobTitleId = NewHrData.JobTitleID;
        //                if (NewHrData.LandLine != null)
        //                if (HrUser.DateOfBirth != NewHrData.DateOfBirth && NewHrData.DateOfBirth > ForCheckOnly)
        //                {
        //                    HrUser.LandLine = NewHrData.LandLine;
        //                }
        //                if (HrUser.DateOfBirth != NewHrData.DateOfBirth && NewHrData.DateOfBirth > ForCheckOnly)
        //                {
        //                    HrUser.DateOfBirth = NewHrData.DateOfBirth;
        //                }
        //                if (NewHrData.Photo != null)
        //                {
        //                    string FilePath = Path.Combine(_host.WebRootPath, HrUser.ImgPath ?? "");
        //                    if (System.IO.File.Exists(FilePath))
        //                    {
        //                        System.IO.File.Delete(FilePath);
        //                    }
        //                    //System.IO.File.Delete(HrUser.ImgPath);
        //                    var fileExtension = NewHrData.Photo.FileName.Split('.').Last();
        //                    var virtualPath = $"Attachments\\{validation.CompanyName}\\HrUser\\{HrUser.Id}\\";
        //                    var FileName = System.IO.Path.GetFileNameWithoutExtension(NewHrData.Photo.FileName);
        //                    HrUser.ImgPath = Common.SaveFileIFF(virtualPath, NewHrData.Photo, FileName, fileExtension, _host);
        //                }
        //                response.ID = HrUser.Id;

        //                if (HrUser.User != null)
        //                {

        //                    if (!string.IsNullOrWhiteSpace(NewHrData.systemEmail))
        //                    {

        //                        HrUser.User.Email = NewHrData.systemEmail;
        //                    }
        //                    if (!string.IsNullOrWhiteSpace(NewHrData.password) && !string.IsNullOrWhiteSpace(NewHrData.confirmPassword))
        //                    {

        //                        if (NewHrData.password == NewHrData.confirmPassword)
        //                        {
        //                            HrUser.User.Password = NewHrData.password;
        //                        }

        //                    }
        //                }
        //                else
        //                {
        //                    if (NewHrData.IsUser == true)
        //                    {
        //                        if (NewHrData.systemEmail != null || NewHrData.systemEmail != "" || NewHrData.password != null || NewHrData.password != "")
        //                        {
        //                            NewGaras.Infrastructure.Entities.User usr = new NewGaras.Infrastructure.Entities.User();

        //                            #region add Data to user table
        //                            usr.Password = Encrypt_Decrypt.Encrypt(NewHrData.password, key);
        //                            usr.Email = NewHrData.systemEmail;
        //                            usr.FirstName = HrUser.FirstName;
        //                            usr.LastName = HrUser.LastName;
        //                            usr.MiddleName = HrUser.MiddleName;
        //                            usr.Mobile = HrUser.Mobile;
        //                            usr.Active = HrUser.Active;
        //                            usr.CreationDate = DateTime.Now;
        //                            usr.CreatedBy = validation.userID;
        //                            usr.ModifiedBy = validation.userID;
        //                            usr.Modified = DateTime.Now;
        //                            usr.Gender = HrUser.Gender;
        //                            usr.BranchId = HrUser.BranchId;
        //                            usr.DepartmentId = HrUser.DepartmentId;
        //                            usr.JobTitleId = HrUser.JobTitleId;
        //                            usr.PhotoUrl = HrUser.ImgPath;
        //                            #endregion

        //                            var User = _unitOfWork.Users.Add(usr);
        //                            _unitOfWork.Complete();

        //                            HrUser.IsUser = true;
        //                            HrUser.UserId = User.Id;

        //                            //Common.CreateNotification(validation.userID, "Welcome , " + HrUser.FirstName, "Thank you for joining our system", "#", true, usr.Id, 0, _Context);
        //                        }
        //                    }
        //                }
        //                    _unitOfWork.Complete();

        //            }
        //        }

        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error err = new Error();
        //        err.ErrorCode = "E-1";
        //        err.errorMSG = "Exception :" + (ex.InnerException != null ? ex.InnerException?.Message : ex.Message);
        //        response.Errors.Add(err);
        //        return response;
        //    }
        //}

        #endregion



        [HttpGet("GetHrUser")]  //services Added
        public async Task<BaseResponseWithData<GetHrUserDto>> GetHrUser([FromHeader] long HrUserId)
        {
            var response = new BaseResponseWithData<GetHrUserDto>()
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
                    response = await _hrUserService.GetHrUser(HrUserId);
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


        [HttpGet("GetHrUserAddress")]  //services Added
        public async Task<BaseResponseWithData<List<GetHrUserAddressDto>>> GetHrUserAddress([FromHeader]long HrUserId)
        {
            var response = new BaseResponseWithData<List<GetHrUserAddressDto>>()
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
                    response = await _hrUserService.GetHrUserAddress(HrUserId);
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

        [HttpGet("GetHrUserContacts")]  //services Added
        public async Task<BaseResponseWithData<GetHrUserContactsDto>> GetHrUserContacts([FromHeader]long HrUserId)
        {
            var response = new BaseResponseWithData<GetHrUserContactsDto>()
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
                    response = await _hrUserService.GetHrUserContacts(HrUserId);
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

        [HttpGet("GetHrUserAttachments")]  //services Added
        public async Task<BaseResponseWithData<List<GetHrUserAttachmentDto>>> GetHrUserAttachments([FromHeader]long HrUserId)
        {
            var response = new BaseResponseWithData<List<GetHrUserAttachmentDto>> ()
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
                    response = await _hrUserService.GetHrUserAttachments(HrUserId);
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



        [HttpPost("AddChurchesAndPriestToHrUser")]  //services Added
        public async Task<BaseResponseWithId<long>> AddChurchesAndPriestToHrUser(AddChurchesAndPriestToHrUserDto dto)
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
                    _hrUserService.Validation = validation;
                    response = await _hrUserService.AddChurchesAndPriestToHrUser(dto);
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

        [HttpGet("GetUserCards")]   //services Added
        public async Task<BaseResponseWithDataAndHeader<List<HrUserCardDto>>> GetAll([FromHeader] string userName, [FromHeader] bool? active
            , [FromHeader] int? DepId, [FromHeader] int? jobTilteId, [FromHeader] int? BranchId, [FromHeader] bool? IsUser, [FromHeader] string Email, [FromHeader] string mobile
            , [FromHeader] bool? isDeleted, [FromHeader] bool? ActiveUser, [FromHeader] int currentPage = 1, [FromHeader] int numberOfItemsPerPage = 10)
        {   //userName is the serachKey in the view
            var response = new BaseResponseWithDataAndHeader<List<HrUserCardDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion


            try
            {
                if (response.Result)
                {
                    response = await _hrUserService.GetAll(currentPage, numberOfItemsPerPage, userName, active,
                        DepId, jobTilteId, BranchId, IsUser, Email, mobile, isDeleted, ActiveUser);
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


        [HttpGet("GetHrUserListDDl")]
        public BaseResponseWithDataAndHeader<HrUserListDDL> GetHrUserListDDl([FromHeader]long? DoctorspecialtyId, [FromHeader] string? searchKey,[FromHeader]int CurrentPage=1,[FromHeader] int NumberOfItemsPerPage=10)
        {
            var response = new BaseResponseWithDataAndHeader<HrUserListDDL>()
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
                    response = _hrUserService.GetHrUserListDDl(CurrentPage, NumberOfItemsPerPage, searchKey, DoctorspecialtyId);
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

        [HttpPost("CreateHrUser")]      //services Added
        public async Task<BaseResponseWithId<long>> CreateHrUsers([FromForm] HrUserDto NewHrUser)
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
                    response = await _hrUserService.CreateHrUser(NewHrUser, validation.userID, validation.CompanyName);
                }
                //_logService.AddLog("Add New Employee", "HrUser", "", (int)response.ID, validation.userID);
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

        [HttpPost("AddHrEmployeeToUser")]    //services Added
        public async Task<BaseResponseWithData<UserEmployeeResponse>> AddHrEmployeeToUser([FromBody] AddHrEmployeeToUserDTO InData)
        {
            var response = new BaseResponseWithData<UserEmployeeResponse>()
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
                    response = await _hrUserService.AddHrEmployeeToUserAsync(InData, validation.userID, key);
                    if (response.Result)
                    {
                        string UserName = response.Data != null ? response.Data.UserName : "";
                        _notificationService.CreateNotification(validation.userID, "Welcome , ", "Welcome " + UserName + " in The PERITI GROUP Solution", "#", true, response.Data.Id, 0);
                        var MailData = new MailData();
                        //MailData.EmailToName = InData.Email;
                        //MailData.EmailToId = InData.Email;

                        MailData.EmailToName = response.Data.UserEmail;
                        MailData.EmailToId = response.Data.UserEmail;
                        MailData.SenderMail = "noreply@theperitigroup.com";
                        

                        MailData.EmailSubject = "Welcome";
                        MailData.EmailBody = @"<p>Welcome to <strong>The Periti Group PMO</strong>! We are excited to have you join our project management platform, where you can manage projects efficiently and collaborate seamlessly with your team.</p>
<p><strong>Getting Started:</strong></p>
<ul>
    <li>To access the PMO system, bookmark the link below for easy access in the future: <a href='https://theperitigrouppt.com' target='_blank'>https://theperitigrouppt.com</a></li>
    <li><strong>First Steps:</strong> Once logged in, we recommend you start by completing your profile and exploring the dashboard to familiarize yourself with the key features.</li>
</ul>
<p><strong>Key Features:</strong></p>
<ul>
    <li><strong>Project Dashboard:</strong> View and manage all your projects in one place.</li>
    <li><strong>Task Management:</strong> Create, assign, and track tasks to ensure timely completion.</li>
    <li><strong>Collaboration Tools:</strong> Communicate with your team through integrated chat and messaging.</li>
    <li><strong>Reporting:</strong> Generate detailed reports to track project progress and performance.</li>
    <li><strong>Document Storage:</strong> Securely store and share project-related documents.</li>
</ul>
<p><strong>Resources and Support:</strong></p>
<ul>
    <li>For detailed instructions on how to use the system, find the below User Guide link:<a href='https://api.theperitigrouppt.com/Attachments/periti/Periti_User_Manual.pdf' target='_blank'>User Guide Link</a></li>
    <li><strong>Support Team:</strong> If you encounter any issues or have any questions, please contact our support team at <a href='mailto:b.end.vetanoia@gmail.com'>b.end.vetanoia@gmail.com</a>.</li>
</ul>
<p><strong>Best Practices:</strong></p>
<ul>
    <li><strong>Regular Updates:</strong> Keep your project information up to date to ensure accurate tracking and reporting.</li>
    <li><strong>Collaborate:</strong> Use the collaboration tools to communicate effectively with your team.</li>
    <li><strong>Monitor Progress:</strong> Regularly check the dashboard and reports to monitor project progress and address any issues promptly.</li>
</ul>
<p>We are confident that <strong>The Periti Group PMO</strong> will greatly enhance your project management capabilities. We look forward to seeing the successful projects you’ll manage with our system.</p>
<p>If you have any questions or need assistance, don’t hesitate to reach out.</p>
<p><strong>Welcome aboard!</strong></p>";
                        var Result = await _mailService.SendMail(MailData);
                        

                    }
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

        [HttpPost("EditHrEmployee")]    //services Added
        public async Task<BaseResponseWithId<long>> EditHrEmployee([FromForm] EditHrEmployeeDto NewHrData)
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
                    var UserCheck = await _hrUserService.GetHrUser(NewHrData.HrUserId);
                    if (!UserCheck.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(UserCheck.Errors);
                        return response;
                    }
                    var responseData = await _hrUserService.EditHrEmployee(NewHrData, validation.userID, validation.CompanyName, key);

                    if (responseData.Result == true && responseData.Data != null && responseData.Data.UserSystemId != 0)
                    {
                        _notificationService.CreateNotification(validation.userID, "Welcome , ", "Welcome " + responseData.Data.UserSystemName + " in The PERITI GROUP Solution", "#", true, responseData.Data.UserSystemId, 0);
                        var MailData = new MailData();
                        MailData.EmailToName = responseData.Data.UserSystemEmail;
                        MailData.EmailToId = responseData.Data.UserSystemEmail;
                        MailData.EmailSubject = "Welcome";
                        MailData.EmailBody = "Thank you for joining our system";
                        var Result = await _mailService.SendMail(MailData);
                    }

                    response.ID = responseData.Data?.HRUserId ?? 0;
                    response.Result = responseData.Result;
                    response.Errors = responseData.Errors;
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

        [HttpPost("RetriveDeletedUser")]
        public async Task<BaseResponseWithId<long>> RetriveDeletedUser([FromHeader] long userId)
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
                    var UserCheck = await _hrUserService.GetHrUser(userId);
                    if (!UserCheck.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(UserCheck.Errors);
                        return response;
                    }
                    var responseData = await _hrUserService.RetriveDeletedUser(userId);

                    response.ID = UserCheck.Data.Id;
                    response.Result = responseData.Result;
                    response.Errors = responseData.Errors;
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

        [HttpGet("GetAbsenceHistoryForUser")]
        public BaseResponseWithData<GetAbsenceHistoryModel> GetAbsenceHistoryForUser([FromHeader] GetAbsenceHistoryRequest request)
        {
            var response = new BaseResponseWithData<GetAbsenceHistoryModel>()
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
                    response = _hrUserService.GetAbsenceHistoryForUser(request);
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


        [HttpGet("GetHrUserAbsneceRequestsList")]
        public BaseResponseWithData<GetHrUserAbsneceRequestList> GetHrUserAbsneceRequest([FromHeader]long HrUserId, [FromHeader]int AbsenceTypeId)
        {
            var response = new BaseResponseWithData<GetHrUserAbsneceRequestList>()
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
                    response = _hrUserService.GetHrUserAbsneceRequest(HrUserId, AbsenceTypeId);
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

        [HttpGet("GetUsersReportExcell")]
        public BaseResponseWithData<string> GetUsersReportExcell([FromHeader]bool? Active, [FromHeader]int? DeptID, [FromHeader]long? teamID, [FromHeader]bool IsUser = true)
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
                    var data = _hrUserService.GetUsersReportExcell(Active, DeptID, teamID, validation.CompanyName, IsUser);
                    if (data != null)
                    {
                        Response = data;
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


        [HttpPost("AddVacationTypeForUser")]
        public BaseResponse AddVacationTypeForUser(AddVacationTypeForUserDto dto)
        {
            var response = new BaseResponse()
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
                    response = _hrUserService.AddVacationTypeForUser(dto, validation.userID);
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

        [HttpGet("GetListOfEmployeeInfo")]
        public ActionResult<GetListOfEmployeeResponse> GetEmployeeInfo(GetEmployeeInfoHeader header)
        {
            var response = new GetListOfEmployeeResponse()
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
                    var empInfo = _hrUserService.GetListOfEmployeeInfo(header, validation.userID);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpPost("AddEditEmployeeAttachments")]
        public ActionResult<BaseResponseWithID> AddEditEmployeeAttachments(AddEditEmployeeAttachmentsRequest request)
        {
            var response = new BaseResponseWithID()
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
                    var empInfo = _hrUserService.AddEditEmployeeAttachments(request, validation.CompanyName, validation.userID);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpGet("GetHREmployeeAttachment")]
        public ActionResult<HrEmployeeAttachmentResponse> GetHREmployeeAttachment([FromHeader] long attachmentId)
        {
            var response = new HrEmployeeAttachmentResponse()
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
                    var empInfo = _hrUserService.GetHREmployeeAttachment(attachmentId);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpGet("GetEmployeeDocuments")]
        public ActionResult<GetEmployeeExpiredDocumentsResponse> GetEmployeeDocuments([FromHeader] GetEmployeeDocumentsHeader header)
        {
            var response = new GetEmployeeExpiredDocumentsResponse()
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
                    var empInfo = _hrUserService.GetEmployeeDocuments(header);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpPost("AddEditJobTitle")]
        public async Task<ActionResult<BaseResponseWithID>> AddEditJobTitle(JobTitleData request)
        {
            var response = new BaseResponseWithID()
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
                    var empInfo =await _hrUserService.AddEditJobTitle(request,validation.userID);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpPost("EditEmployeeRole")]
        public async Task<BaseResponseWithId<long>> EditEmployeeRole(EmployeeRoleData request)
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
                    _hrUserService.Validation = validation;
                    var empInfo = await _hrUserService.EditEmployeeRoleNew(request);
                    if (!empInfo.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Errors);
                        return response;
                    }
                    response = empInfo;
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


        [HttpPost("AddEditEmployeeInfo")]
        public ActionResult<BaseResponseWithID> AddEditEmployeeInfo(EmployeeInfoDataList request)
        {
            var response = new BaseResponseWithID()
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
                    var empInfo =  _hrUserService.AddEditEmployeeInfo(request, validation.userID, validation.CompanyName);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpGet("GetTeamUser")]
        public ActionResult<GetTeamUserResponse> GetTeamUser([FromHeader] long TeamID = 0)
        {
            var response = new GetTeamUserResponse()
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
                    var empInfo = _hrUserService.GetTeamUser(TeamID);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpGet("GetTeamsIndex")]
        public ActionResult<GetTeamResponse> GetTeamsIndex()
        {
            var response = new GetTeamResponse()
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
                    var empInfo = _hrUserService.GetTeamsIndex();
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpGet("GetUserRoleAndGroup")]
        public async Task<ActionResult<GetUserRolegAndRoleResponse>> GetUserRoleAndGroup([FromHeader]int UserID = 0)
        {
            var response = new GetUserRolegAndRoleResponse()
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
                    var empInfo = await _hrUserService.GetUserRoleAndGroup(UserID);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpGet("GetUserRoleAndGroupNew")]

        public async Task<GetUserRolegAndRoleResponse> GetUserRoleAndGroupNew([FromHeader]long UserId)
        {
            var response = new GetUserRolegAndRoleResponse()
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
                    var empInfo = await _hrUserService.GetUserRoleAndGroupNew(UserId);
                    if (!empInfo.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Errors);
                        return response;
                    }
                    response = empInfo;
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

        [HttpGet("GetAbsenceTypeList")]
        public ActionResult<SelectDDLResponse> GetAbsenceTypeList()
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
                    var empInfo =  _hrUserService.GetAbsenceTypeList();
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpGet("GetVacationTypesList")]
        public ActionResult<ContractLeaveSettingListRespoonse> GetVacationTypesList()
        {
            ContractLeaveSettingListRespoonse response = new ContractLeaveSettingListRespoonse();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var empInfo = _hrUserService.GetVacationTypesList();
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpPost("AddEditVacationType")]
        public ActionResult<BaseResponseWithId> AddEditVacationType(ContractLeaveSettingRequest request)
        {
            var response = new BaseResponseWithId()
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
                    var empInfo = _hrUserService.AddEditVacationType(request, validation.userID);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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


        [HttpPost("EditContractDetails")]
        public ActionResult<BaseResponseWithId> EditContractDetails(EditContractDetailModel request)
        {
            var response = new BaseResponseWithId()
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
                    var empInfo = _hrUserService.EditContractDetails(request, validation.userID);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpPost("EditContractEmployeeAbsence")]
        public ActionResult<BaseResponseWithId> EditContractEmployeeAbsence(ContractLeaveEmployeeModel contractLeave)
        {
            var response = new BaseResponseWithId()
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
                    var empInfo = _hrUserService.EditContractEmployeeAbsence(contractLeave, validation.userID);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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


        [HttpGet("GetContractDetails")]
        public ActionResult<GetContractDetailsResponse> GetContractDetails([FromHeader] long userId = 0)
        {
            var response = new GetContractDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var empInfo = _hrUserService.GetContractDetails();
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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


        [HttpGet("GetJobTitlesDDL")]
        public ActionResult<JobTitlesDDLResponse> GetJobTitlesDDL()
        {
            var response = new JobTitlesDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var empInfo = _hrUserService.GetJobTitlesDDL();
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        //[HttpPost("AddAttendanceData")]
        //public ActionResult<BaseResponseWithID> AddAttendanceData(AddEmployeesAttendanceRequest request)
        //{
        //    var response = new BaseResponseWithID()
        //    {
        //        Result = true,
        //        Errors = new List<Error>()
        //    };

        //    #region user Auth
        //    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //    response.Errors = validation.errors;
        //    response.Result = validation.result;
        //    #endregion

        //    try
        //    {
        //        if (response.Result)
        //        {
        //            var empInfo = _hrUserService.AddAttendanceData(request, validation.userID);
        //            if (!empInfo.Value.Result)
        //            {
        //                response.Result = false;
        //                response.Errors.AddRange(empInfo.Value.Errors);
        //                return response;
        //            }
        //            response = empInfo.Value;
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error err = new Error();
        //        err.ErrorCode = "E-1";
        //        err.errorMSG = "Exception :" + ex.Message;
        //        response.Errors.Add(err);
        //        return response;
        //    }
        //}

        [HttpGet("GetEmployeeAttendence")]
        public async Task<ActionResult<UserAttendanceListResponse>> GetEmployeeAttendence([FromHeader] GetEmployeeAttendenceHeader header)
        {
            var response = new UserAttendanceListResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var empInfo =await _hrUserService.GetEmployeeAttendence(header);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpGet("GetUserAttendanceList")]
        public async Task<ActionResult<UserAttendanceListResponse>> GetUserAttendanceList(GetUserAttendanceListHeader header)
        {
            var response = new UserAttendanceListResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var empInfo = await _hrUserService.GetUserAttendanceList(header);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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

        [HttpGet("GetEmployeeAbsenceDetails")]
        public ActionResult<GetAbsenceDetailsResponse> GetEmployeeAbsenceDetails([FromHeader]long UserId = 0)
        {
            var response = new GetAbsenceDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var empInfo =  _hrUserService.GetEmployeeAbsenceDetails(UserId);
                    if (!empInfo.Value.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(empInfo.Value.Errors);
                        return response;
                    }
                    response = empInfo.Value;
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


        [HttpPost("CreateHrUserWorker")]
        public async Task<BaseResponseWithId<long>> CreateHrUserWorker(AddHrUserWorker Worker)
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
                    var worker =await _hrUserService.CreateHrUserWorker(Worker, validation.userID);
                    if (!worker.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(worker.Errors);
                        return response;
                    }
                    response = worker;
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


        [HttpPost("AddAddressToHrUser")]
        public async Task<BaseResponse> AddAddressToHrUser(AddHrUserAddessList dtos)
        {
            var response = new BaseResponse()
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

                    _hrUserService.Validation = validation;
                    var worker = await _hrUserService.AddAddressToHrUser(dtos);
                    if (!worker.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(worker.Errors);
                        return response;
                    }
                    response = worker;
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

        [HttpPost("AddAttachmentsToHrUser")]
        public async Task<BaseResponse> AddAttachmentsToHrUser([FromForm] List<HrUserAttachmentDto> Attachments)
        {
            var response = new BaseResponse()
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

                    _hrUserService.Validation = validation;
                    var worker = await _hrUserService.AddAttachmentsToHrUser(Attachments);
                    if (!worker.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(worker.Errors);
                        return response;
                    }
                    response = worker;
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

        [HttpPost("AddContactsToHrUser")]
        public async Task<BaseResponse> AddContactsToHrUser(HrUserContactsDto dto)
        {
            var response = new BaseResponse()
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

                    _hrUserService.Validation = validation;
                    var worker = await _hrUserService.AddContactsToHrUser(dto);
                    if (!worker.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(worker.Errors);
                        return response;
                    }
                    response = worker;
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

        [HttpPost("EditHrUserAddress")]
        public async Task<BaseResponseWithId<long>> EditHrUserAddress(HrUserAddressDto dto)
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

                    _hrUserService.Validation = validation;
                    var worker = await _hrUserService.EditHrUserAddress(dto);
                    if (!worker.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(worker.Errors);
                        return response;
                    }
                    response = worker;
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

        [HttpPost("EditHrUserSocialMedia")]
        public async Task<BaseResponseWithId<long>> EditHrUserSocialMedia(HrUserSocialMediaDto dto)
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

                    _hrUserService.Validation = validation;
                    var worker = await _hrUserService.EditHrUserSocialMedia(dto);
                    if (!worker.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(worker.Errors);
                        return response;
                    }
                    response = worker;
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

        [HttpPost("EditHrUserMobile")]
        public async Task<BaseResponseWithId<long>> EditHrUserMobile(HrUserMobileDto dto)
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

                    _hrUserService.Validation = validation;
                    var worker = await _hrUserService.EditHrUserMobile(dto);
                    if (!worker.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(worker.Errors);
                        return response;
                    }
                    response = worker;
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

        [HttpPost("EditHrUserLandLine")]
        public async Task<BaseResponseWithId<long>> EditHrUserLandLine(HrUserLandLineDto dto)
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

                    _hrUserService.Validation = validation;
                    var worker = await _hrUserService.EditHrUserLandLine(dto);
                    if (!worker.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(worker.Errors);
                        return response;
                    }
                    response = worker;
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

        [HttpPost("AddPersonStatus")]
        public BaseResponseWithId<long> AddPersonStatus(AddPersonStatusDTO dto)
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
                    response = _hrUserService.AddPersonStatus(dto);
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

        [HttpPost("EditPersonStatus")]
        public BaseResponseWithId<long> EditPersonStatus(EditPersonStatusDTO dto)
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
                    response = _hrUserService.EditPersonStatus(dto);
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

        [HttpGet("GetPersonStatusList")]
        public BaseResponseWithData<List<GetPersonStatusListDTO>> GetPersonStatusList()
        {
            var response = new BaseResponseWithData<List<GetPersonStatusListDTO>>()
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
                    var personStatusList =  _hrUserService.GetPersonStatusList();
                    if (!personStatusList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(personStatusList.Errors);
                        return response;
                    }
                    response = personStatusList;
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

        [HttpPost("AddHrUserStatus")]
        public BaseResponseWithId<long> AddHrUserStatus(AddHrUserStatusDTO dto)
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
                    response = _hrUserService.AddHrUserStatus(dto);
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

        [HttpPost("EditHrUserStatus")]
        public BaseResponseWithId<long> EditHrUserStatus(EditHrUserStatusDTO dto)
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
                    response = _hrUserService.EditHrUserStatus(dto);
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

        [HttpGet("GetHruserStatusList")]
        public BaseResponseWithData<List<GetHruserStatusListDTO>> GetHruserStatusList([FromHeader]long hruserID)
        {
            var response = new BaseResponseWithData<List<GetHruserStatusListDTO>>()
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
                    var personStatusList = _hrUserService.GetHruserStatusList(hruserID);
                    if (!personStatusList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(personStatusList.Errors);
                        return response;
                    }
                    response = personStatusList;
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
