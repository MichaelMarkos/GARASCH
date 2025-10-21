

using AutoMapper;
using Azure;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.LMS;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.LMS;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.LMS;
using NewGarasAPI.Helpers;
using NewGarasAPI.Models.User;
using System.Linq.Expressions;
using System.Web;


namespace NewGarasAPI.Controllers.LMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResultControlService _ResultControlService;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private IPermissionService _permissionService;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png", ".jpeg", ".svg" };
        private long _maxAllowedPosterSize = 15728640;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        public CompetitionController(IUnitOfWork unitOfWork , IMapper mapper ,
            IHttpContextAccessor httpContextAccessor , Microsoft.AspNetCore.Hosting.IHostingEnvironment environment ,
            IPermissionService permissionService , IResultControlService ResultControlService , ITenantService tenantService)
        {
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _httpContextAccessor=httpContextAccessor;
            _environment=environment;
            _permissionService=permissionService;
            _ResultControlService=ResultControlService;
            _helper=new Helper.Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);

        }

        private string BaseURL
        {
            get
            {
                var uri = _httpContextAccessor?.HttpContext?.Request;
                string Host = uri?.Scheme + "://" + uri?.Host.Value.ToString();
                return Host;
            }
        }

        private DateTime TimeZoneEgypt()
        {
            TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            // Get the current datetime in Egypt
            DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            return egyptDateTime;
        }
        private long ApplicationUserId
        {
            get
            {
                long UserId = 1245 ;
                return UserId;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromHeader] long HrUserId , [FromHeader] string? SearchKey , [FromHeader] string? VerifyCode)
        {
            BaseResponseWithData<List<CompetitionDTO>> Response = new  BaseResponseWithData<List<CompetitionDTO>>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                var data = new List<CompetitionDTO>();
                if(Response.Result)
                {

                    //string ApplicationUserId = User.FindFirstValue("uid");
                    var CompetitionsList = await _unitOfWork.Competitions.FindAllAsync((x => x.Active == true &&
                        ((SearchKey == null || x.Name.ToLower().Contains(SearchKey.ToLower())) ||
                         ((x.Description != null
                             ? x.Description.ToLower().Trim().Contains(SearchKey)
                             : true))) &&
                        // if code null or 0 => back all Competitions have not code else select specific code
                        ((x.Code != null || !string.IsNullOrWhiteSpace(VerifyCode)) && x.Code != "0"
                            ? (!string.IsNullOrWhiteSpace(VerifyCode) ? x.Code.Trim() == VerifyCode.Trim() : false)
                            : true) || x.CompetitionUsers.Where(x => x.HrUserId == ApplicationUserId).Count() >
                        0),
                    new[] { "CompetitionUser", "CompetitionMemberAdmin" });

                    //var data = _mapper.Map<IEnumerable<CompetitionDTO>>(CompetitionsList);
                    data=CompetitionsList.Select(competition => new CompetitionDTO
                    {
                        Id=competition.Id ,
                        Name=competition.Name ,
                        Description=competition.Description ,
                        Code=competition.Code ,
                        Objective=competition.Objective ,
                        Days=competition.Days ,
                        StudyingHours=competition.StudyingHours ,
                        // SolvedPercent = competition.SolvedPercent,
                        CreationBy=competition.CreationBy ,
                        CreationDate=competition.CreationDate ,
                        // ShowAnswers = competition.ShowAnswers,
                        // ShowRanks = competition.ShowRanks,
                        // ShowScores = competition.ShowScores,
                        IsOwner=competition.HrUserId==HrUserId ,
                        IsMemberAdmin=competition.CompetitionMemberAdmins.Where(x =>
                        x.HrUserId==HrUserId&&x.CompetitionId==competition.Id).Any() ,
                        // ShowCertificate = competition.ShowCertificate,
                        ImagePath=competition.ImagePath!=null ? BaseURL+competition.ImagePath : "" ,
                        Visable=string.IsNullOrWhiteSpace(competition.Code) ? true : false ,
                        IsJoined=
                       competition.CompetitionUsers.Any(x => x.HrUserId==HrUserId) ? true : false ,
                        TotalScore=competition.CompetitionUsers.Where(x => x.HrUserId==HrUserId)
                       .Select(x => x.TotalScore).FirstOrDefault() ,
                        //competition.ShowCertificate != true ? competition.CompetitionUser.Where(x => x.ApplicationUserId == ApplicationUserId).Select(x => x.TotalScore).FirstOrDefault() :0,
                        RemainDay=
                       competition.Days!=null
                           ? (competition.Days-(_unitOfWork.CompetitionDays.Count((x =>
                               x.CompetitionId==competition.Id&&x.Date<DateTime.Now))))
                           : 0 ,
                        TotalRank=competition.CompetitionUsers.OrderByDescending(x => x.TotalScore).Count() ,
                        RankNo=competition.CompetitionUsers.OrderByDescending(x => x.TotalScore).Select((s , i) => new
                        {
                            HrUserId = s.HrUserId ,
                            TotalScore = s.TotalScore ,
                            //Rank = i+1
                            Rank = (from o in competition.CompetitionUsers.ToList()
                                    where o.TotalScore>=s.TotalScore
                                    select o).Count() /*+ 1*/
                        }).Where(x => x.HrUserId==HrUserId).Select(x => x.Rank).FirstOrDefault()
                    }).ToList();
                    // var rank = CompetitionsList.GroupBy(d => d.Id)
                    //.SelectMany(g => g.Select((x,i) => new { g.Key, Item = x, Rank = i + 1 }));

                }
                return Ok(data);

            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.errorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }


        //[Authorize(Roles = "admin")]
        //[HttpGet("GetCompetitorsUserList")]
        //public async Task<IActionResult> GetCompetitorsUserAdminAsync([FromHeader] int? CompetitionId ,
        //   [FromHeader] int? CompetitionDayId , [FromHeader] string? FilterUserName ,
        //   [FromHeader] string? FilterPhoneNumber , [FromHeader] string? FilterNationalId , [FromHeader] int? NoOfItems ,
        //   [FromHeader] int? PageNo , [FromHeader] string? SortBy , [FromHeader] string? SearchGeneralUser)
        //{
        //    BaseResponseWithDataPaging<IEnumerable<CompetitorUserInfoDTO>> Response =
        //        new BaseResponseWithDataPaging<IEnumerable<CompetitorUserInfoDTO>>();
        //    Response.Result=true;
        //    Response.Errors=new List<string>();
        //    try
        //    {
        //        int CurrentPage = 0;
        //        int TotalPages = 0;
        //        int ItemsPerPage = 0;
        //        int TotalItems = 0;

        //        #region Header Data


        //        string ExpSortBy = "score"; // Default
        //        Expression<Func<CompetitionUser, object>> CompetitionUserOrderby = (x => x.TotalScore);
        //        Expression<Func<CompetitionDayUser, object>> CompetitionDayUserOrderby = (x => x.UserScore);
        //        if(SortBy!=null)
        //        {
        //            if(SortBy.Trim().ToLower()=="name")
        //            {
        //                CompetitionUserOrderby=(x => x.HrUser.FirstName);
        //                CompetitionDayUserOrderby=(x => x.HrUser.FirstName);
        //            }
        //        }

        //        //, Take + Skip, Skip

        //        #endregion

        //        IEnumerable<CompetitorUserInfoDTO> data = new List<CompetitorUserInfoDTO>();
        //        string FilterUserNameTemp = null;
        //        if(!string.IsNullOrWhiteSpace(FilterUserName))
        //        {
        //            FilterUserNameTemp=HttpUtility.UrlDecode(FilterUserName);
        //            FilterUserNameTemp=Extension.SearchedKey(FilterUserNameTemp);
        //        }

        //        /*
        //         ################################### Note ############################################
        //         if filter with KEy Search Gernal USer
        //        search in all seystem with identical UserName to reset password
        //        else
        //        get all users competitors
        //         */
        //        if(!string.IsNullOrWhiteSpace(SearchGeneralUser))
        //        {
        //            SearchGeneralUser=HttpUtility.UrlDecode(SearchGeneralUser);
        //            var _userList = await _userManager.Users.Where(x =>
        //                (x.PhoneNumber != null ? x.PhoneNumber.Trim() == SearchGeneralUser.Trim() : false) ||
        //                (x.UserName != null ? x.UserName.Trim() == SearchGeneralUser.Trim() : false)).ToListAsync();
        //            data=_mapper.Map<IEnumerable<CompetitorUserInfoDTO>>(_userList);
        //        }
        //        else
        //        {
        //            if(CompetitionId!=null)
        //            {
        //                var UserComPetitorsList = await _unitOfWork.CompetitionUsers.FindAllPagingAsync(
        //                    (x => x.CompetitionId == CompetitionId &&
        //                          (FilterPhoneNumber != null
        //                              ? x.ApplicationUser.PhoneNumber == FilterPhoneNumber.Trim()
        //                              : true) &&
        //                          (FilterNationalId != null
        //                              ? x.ApplicationUser.NationalId == FilterNationalId.Trim()
        //                              : true) &&
        //                          (FilterUserNameTemp != null
        //                              ? EF.Functions.Like(x.ApplicationUser.UserName, FilterUserNameTemp)
        //                              : true)), PageNo, NoOfItems, new[]
        //                    {
        //                        "ApplicationUser", "ApplicationUser.CompetitionDayUser", "Competition",
        //                        "Competition.CompetitionDay",
        //                        //"Competition.CompetitionUser" 
        //                    }, CompetitionUserOrderby, "DESC");
        //                data=_mapper.Map<IEnumerable<CompetitorUserInfoDTO>>(UserComPetitorsList);
        //                CurrentPage=PageNo??0;
        //                TotalPages=UserComPetitorsList.TotalPages;
        //                ItemsPerPage=NoOfItems??100;
        //                TotalItems=UserComPetitorsList.TotalCount;
        //            }
        //            else if(CompetitionDayId!=null)
        //            {
        //                var UserComPetitorsListByDay = await _unitOfWork.CompetitionDayUsers.FindAllPagingAsync(
        //                    (x => x.CompetitionDayId == CompetitionDayId &&
        //                          (FilterPhoneNumber != null
        //                              ? x.ApplicationUser.PhoneNumber == FilterPhoneNumber.Trim()
        //                              : true) &&
        //                          (FilterNationalId != null
        //                              ? x.ApplicationUser.NationalId == FilterNationalId.Trim()
        //                              : true)), PageNo, NoOfItems,
        //                    new[]
        //                    {
        //                        "ApplicationUser", "ApplicationUser.CompetitionDayUser",
        //                        "CompetitionDay.CompetitionDayUser"
        //                    }, CompetitionDayUserOrderby, "DESC");
        //                data=_mapper.Map<IEnumerable<CompetitorUserInfoDTO>>(UserComPetitorsListByDay);
        //                CurrentPage=PageNo??0;
        //                TotalPages=UserComPetitorsListByDay.TotalPages;
        //                ItemsPerPage=NoOfItems??100;
        //                TotalItems=UserComPetitorsListByDay.TotalCount;
        //            }
        //        }

        //        Response.Data=data;
        //        Response.Result=true;
        //        Response.TotalPages=TotalPages;
        //        Response.TotalItems=TotalItems;
        //        Response.PageNo=CurrentPage;
        //        Response.NoOfItems=ItemsPerPage;
        //        return Ok(Response);
        //    }
        //    catch(Exception ex)
        //    {
        //        Response.Result=false;
        //        string ErrorMSG = "Exception :" + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
        //        Response.Errors.Add(ErrorMSG);
        //        return BadRequest(Response);
        //    }
        //}

        //[Authorize(Roles = "admin")]
        //[HttpGet("ExportExcelCompetitorsUser")]
        //public async Task<IActionResult> ExportExcelCompetitorsUserAdminAsync([FromHeader] int? CompetitionId ,
        //    [FromHeader] int? CompetitionDayId , [FromHeader] string? FilterUserName ,
        //    [FromHeader] string? FilterPhoneNumber , [FromHeader] string? FilterNationalId , [FromHeader] string? SortBy)
        //{
        //    BaseResponse Response = new BaseResponse();
        //    Response.Result=true;
        //    Response.Errors=new List<string>();
        //    try
        //    {
        //        IEnumerable<CompetitorUserInfoDTO> data = new List<CompetitorUserInfoDTO>();
        //        string FilterUserNameTemp = null;
        //        if(!string.IsNullOrWhiteSpace(FilterUserName))
        //        {
        //            FilterUserNameTemp=HttpUtility.UrlDecode(FilterUserName);
        //            FilterUserNameTemp=Extension.SearchedKey(FilterUserNameTemp);
        //        }

        //        // Validate First CompetitionId or CompetitionDayId
        //        if(CompetitionId!=null)
        //        {
        //            IEnumerable<CompetitionUser> UserComPetitorsList = new List<CompetitionUser>();
        //            UserComPetitorsList=await _unitOfWork.CompetitionUsers.FindAllAsync(
        //                (x => x.CompetitionId==CompetitionId&&
        //                      (FilterPhoneNumber!=null
        //                          ? x.ApplicationUser.PhoneNumber==FilterPhoneNumber.Trim()
        //                          : true)&&
        //                      (FilterNationalId!=null
        //                          ? x.ApplicationUser.NationalId==FilterNationalId.Trim()
        //                          : true)&&
        //                      (FilterUserNameTemp!=null
        //                          ? EF.Functions.Like(x.ApplicationUser.UserName , FilterUserNameTemp)
        //                          : true)) ,
        //                new []
        //                {
        //                    "ApplicationUser", "ApplicationUser.CompetitionDayUser", "Competition.CompetitionDay"
        //                });
        //            data=_mapper.Map<IEnumerable<CompetitorUserInfoDTO>>(UserComPetitorsList);
        //        }
        //        else if(CompetitionDayId!=null)
        //        {
        //            IEnumerable<CompetitionDayUser> UserComPetitorsListByDay = new List<CompetitionDayUser>();
        //            UserComPetitorsListByDay=await _unitOfWork.CompetitionDayUsers.FindAllAsync(
        //                (x => x.CompetitionDayId==CompetitionDayId&&
        //                      (FilterPhoneNumber!=null
        //                          ? x.ApplicationUser.PhoneNumber==FilterPhoneNumber.Trim()
        //                          : true)&&
        //                      (FilterNationalId!=null
        //                          ? x.ApplicationUser.NationalId==FilterNationalId.Trim()
        //                          : true)) ,
        //                new []
        //                {
        //                    "ApplicationUser", "ApplicationUser.CompetitionDayUser",
        //                    "CompetitionDay.CompetitionDayUser"
        //                });
        //            data=_mapper.Map<IEnumerable<CompetitorUserInfoDTO>>(UserComPetitorsListByDay);
        //        }

        //        //                var R =  new ExcelResult<CompetitorUserInfoDTO>(data, "CompetitorsUsers", "CompetitorsUsers");
        //        //using System.Data;  
        //        DataTable dt = new DataTable("Grid");
        //        dt.Columns.AddRange(new DataColumn [6]
        //        {
        //            new DataColumn("UserName"), new DataColumn("PhoneNumber"), new DataColumn("Score"),
        //            new DataColumn("CountOfDays"), new DataColumn("RankNo"), new DataColumn("TotalRank")
        //        });
        //        //data.Select(item => new DataTable { item.UserId,item.UserName,ite });
        //        foreach(var item in data)
        //        {
        //            dt.Rows.Add(item.UserName , item.PhoneNumber , item.Score , item.CountOfDays , item.RankNo ,
        //                item.TotalRank);
        //        }

        //        //using ClosedXML.Excel; 
        //        using(MemoryStream stream = new MemoryStream())
        //        {
        //            var workbook = new XLWorkbook();
        //            var worksheet = workbook.Worksheets.Add(dt);
        //            workbook.SaveAs(stream);
        //            stream.Seek(0 , SeekOrigin.Begin);
        //            string FolderRoot = Path.Combine(this._environment.WebRootPath, "Competitions/ExcelReports");
        //            if(Directory.Exists(FolderRoot))
        //            {
        //                Directory.Delete(FolderRoot , true);
        //            }

        //            var IMGPathDB = "Competitions/ExcelReports/";
        //            string SaveIMGPath = Path.Combine(this._environment.WebRootPath, IMGPathDB);
        //            if(!System.IO.Directory.Exists(SaveIMGPath))
        //            {
        //                System.IO.Directory.CreateDirectory(SaveIMGPath); //Create directory if it doesn't exist
        //            }

        //            string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + "CompetitorsUsers.xlsx";
        //            SaveIMGPath=SaveIMGPath+FileName;
        //            using FileStream fileStream = new(SaveIMGPath, FileMode.Create);
        //            IMGPathDB="/"+IMGPathDB+FileName;
        //            stream.CopyTo(fileStream);
        //            return Ok(BaseURL+IMGPathDB);
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        Response.Result=false;
        //        string ErrorMSG = "Exception :" + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
        //        Response.Errors.Add(ErrorMSG);
        //        return BadRequest(Response);
        //    }
        //}

        //[Authorize(Roles = "admin,adminCompetition")]
        //[HttpPost]
        //public async Task<IActionResult> CreateAsync([FromForm] CompetitionCreateDTO dto)
        //{
        //    BaseResponse Response = new BaseResponse();
        //    Response.Result=true;
        //    Response.Errors=new List<string>();
        //    try
        //    {
        //        if(!ModelState.IsValid)
        //            return BadRequest(ModelState);
        //        if(dto.Image!=null)
        //        {
        //            if(!_allowedExtenstions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
        //            {
        //                Response.Errors.Add("Image Only .png , .jpg , .jpeg and .svg images are allowed!");
        //                return BadRequest(Response);
        //            }

        //            if(dto.Image.Length>_maxAllowedPosterSize)
        //            {
        //                Response.Errors.Add("Max allowed size for Cover Image greater than 5MB!");
        //                return BadRequest(Response);
        //            }
        //        }

        //        if(dto.CertificateTempImg!=null)
        //        {
        //            if(!_allowedExtenstions.Contains(Path.GetExtension(dto.CertificateTempImg.FileName).ToLower()))
        //            {
        //                Response.Errors.Add(
        //                    "Certificate Templete Img Only .png , .jpg , .jpeg and .svg images are allowed!");
        //                return BadRequest(Response);
        //            }

        //            if(dto.CertificateTempImg.Length>_maxAllowedPosterSize)
        //            {
        //                Response.Errors.Add("Max allowed size for Cover Certificate Temp Img greater than 5MB!");
        //                return BadRequest(Response);
        //            }
        //        }

        //        if(dto.SolvedPercent!=null)
        //        {
        //            if(dto.SolvedPercent<=0||dto.SolvedPercent>100)
        //            {
        //                Response.Errors.Add("Invalid Solved Percent");
        //                return BadRequest(Response);
        //            }
        //        }

        //        // validation Competitions
        //        if(!string.IsNullOrWhiteSpace(dto.Code))
        //        {
        //            //validate code not itteration
        //        }

        //        //Image
        //        string IMGPathDB = null;
        //        if(dto.Image!=null)
        //        {
        //            IMGPathDB="Competitions/";
        //            string SaveIMGPath = Path.Combine(this._environment.WebRootPath, IMGPathDB);
        //            if(!System.IO.Directory.Exists(SaveIMGPath))
        //            {
        //                System.IO.Directory.CreateDirectory(SaveIMGPath); //Create directory if it doesn't exist
        //            }

        //            string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_CompetetionLogo_" +
        //                              dto.Image.FileName.Trim().Replace(" ", "");
        //            SaveIMGPath=SaveIMGPath+FileName;
        //            using FileStream fileStream = new(SaveIMGPath, FileMode.Create);
        //            IMGPathDB="/"+IMGPathDB+FileName;
        //            dto.Image.CopyTo(fileStream);
        //        }

        //        string CertificateTempImgPathDB = null;
        //        if(dto.CertificateTempImg!=null)
        //        {
        //            CertificateTempImgPathDB="Competitions/";
        //            string SaveIMGPathCert = Path.Combine(this._environment.WebRootPath, CertificateTempImgPathDB);
        //            if(!System.IO.Directory.Exists(SaveIMGPathCert))
        //            {
        //                System.IO.Directory.CreateDirectory(SaveIMGPathCert); //Create directory if it doesn't exist
        //            }

        //            string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_CompetitionCertificateTempImg" +
        //                              dto.CertificateTempImg.FileName.Trim().Replace(" ", "");
        //            SaveIMGPathCert=SaveIMGPathCert+FileName;
        //            using FileStream fileStream = new(SaveIMGPathCert, FileMode.Create);
        //            CertificateTempImgPathDB="/"+CertificateTempImgPathDB+FileName;
        //            dto.CertificateTempImg.CopyTo(fileStream);
        //        }

        //        int CompetitionId = dto.Id;
        //        var _competition = _mapper.Map<Competition>(dto);
        //        if(CompetitionId==0)
        //        {
        //            _competition.Active=true;
        //            if(!string.IsNullOrWhiteSpace(IMGPathDB))
        //                _competition.ImagePath=IMGPathDB;
        //            // _competition.CertificateTempImg = CertificateTempImgPathDB;
        //            _competition.CreationBy="system";
        //            _competition.CreationDate=DateTime.Now;
        //            _competition.ApplicationUserId=ApplicationUserId;
        //            await _unitOfWork.Competitions.AddAsync(_competition);
        //        }
        //        else
        //        {
        //            // Get existing entity by id (Example)
        //            _competition=await _unitOfWork.Competitions.GetByIdAsync(CompetitionId);
        //            if(_competition==null)
        //            {
        //                Response.Result=false;
        //                string ErrorMSG = "Invalid competition Id";
        //                Response.Errors.Add(ErrorMSG);
        //                return BadRequest(Response);
        //            }

        //            _mapper.Map<CompetitionCreateDTO , Competition>(dto , _competition);
        //            if(!string.IsNullOrWhiteSpace(IMGPathDB))
        //            {
        //                // check if have image before for remove first
        //                if(!string.IsNullOrWhiteSpace(_competition.ImagePath))
        //                {
        //                    string webRootPath = _environment.WebRootPath;
        //                    string fullPath = webRootPath + _competition.ImagePath;
        //                    if(System.IO.File.Exists(fullPath))
        //                    {
        //                        System.IO.File.Delete(fullPath);
        //                    }
        //                }

        //                _competition.ImagePath=IMGPathDB;
        //            }

        //            //if (!string.IsNullOrWhiteSpace(CertificateTempImgPathDB))
        //            //{
        //            //    // check if have CertificateTempImg before for remove first
        //            //    if (!string.IsNullOrWhiteSpace(_competition.CertificateTempImg))
        //            //    {
        //            //        string webRootPath = _environment.WebRootPath;
        //            //        string fullPath = webRootPath + _competition.CertificateTempImg;
        //            //        if (System.IO.File.Exists(fullPath))
        //            //        {
        //            //            System.IO.File.Delete(fullPath);
        //            //        }
        //            //    }
        //            //    _competition.CertificateTempImg = CertificateTempImgPathDB;
        //            //}
        //            _unitOfWork.Competitions.Update(_competition);
        //        }

        //        _unitOfWork.Complete();
        //        return Ok(Response);
        //    }
        //    catch(Exception ex)
        //    {
        //        Response.Result=false;
        //        string ErrorMSG = "Exception :" + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
        //        Response.Errors.Add(ErrorMSG);
        //        return BadRequest(Response);
        //    }
        //}

        //[Authorize(Roles = "admin,adminCompetition")]
        [Authorize]
        [HttpPost("EditCompetition")]
        public async Task<IActionResult> EditCompetitionAsync([FromForm] CompetitionCreateDTO dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    if(!ModelState.IsValid)
                        return BadRequest(ModelState);
                    if(dto.Id==0)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);
                    }
                    //if (!await _permissionService.CheckUserHasPermissionManageCompetition(ApplicationUserId, dto.Id))
                    //{
                    //    Response.Result = false;
                    //    Response.Errors.Add("ليس لديه الصلاحيه للتعديل على هذه المسابقه ");
                    //    return Ok(Response);
                    //}

                    if(dto.Image!=null)
                    {
                        if(!_allowedExtenstions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorMSG="Image Only .png , .jpg , .jpeg and .svg images are allowed!";
                            Response.Errors.Add(error);
                            return BadRequest(Response);
                        }

                        if(dto.Image.Length>_maxAllowedPosterSize)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorMSG="Max allowed size for Cover Image greater than 5MB!";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }

                    if(dto.CertificateTempImg!=null)
                    {
                        if(!_allowedExtenstions.Contains(Path.GetExtension(dto.CertificateTempImg.FileName).ToLower()))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorMSG="Certificate Templete Img Only .png , .jpg , .jpeg and .svg images are allowed!";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }

                        if(dto.CertificateTempImg.Length>_maxAllowedPosterSize)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorMSG="Max allowed size for Cover Certificate Temp Img greater than 5MB!";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }

                    if(dto.SolvedPercent!=null)
                    {
                        if(dto.SolvedPercent<=0||dto.SolvedPercent>100)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorMSG="Invalid Solved Percent";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }

                    // validation Competitions
                    if(!string.IsNullOrWhiteSpace(dto.Code))
                    {
                        //validate code not itteration
                    }

                    //Image
                    string IMGPathDB = null;
                    if(dto.Image!=null)
                    {
                        IMGPathDB="Competitions/";
                        string SaveIMGPath = Path.Combine(this._environment.WebRootPath, IMGPathDB);
                        if(!System.IO.Directory.Exists(SaveIMGPath))
                        {
                            System.IO.Directory.CreateDirectory(SaveIMGPath); //Create directory if it doesn't exist
                        }

                        string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_CompetetionLogo_" +
                                      dto.Image.FileName.Trim().Replace(" ", "");
                        SaveIMGPath=SaveIMGPath+FileName;
                        using FileStream fileStream = new(SaveIMGPath, FileMode.Create);
                        IMGPathDB="/"+IMGPathDB+FileName;
                        dto.Image.CopyTo(fileStream);
                    }

                    string CertificateTempImgPathDB = null;
                    if(dto.CertificateTempImg!=null)
                    {
                        CertificateTempImgPathDB="Competitions/";
                        string SaveIMGPathCert = Path.Combine(this._environment.WebRootPath, CertificateTempImgPathDB);
                        if(!System.IO.Directory.Exists(SaveIMGPathCert))
                        {
                            System.IO.Directory.CreateDirectory(SaveIMGPathCert); //Create directory if it doesn't exist
                        }

                        string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_CompetitionCertificateTempImg" +
                                      dto.CertificateTempImg.FileName.Trim().Replace(" ", "");
                        SaveIMGPathCert=SaveIMGPathCert+FileName;
                        using FileStream fileStream = new(SaveIMGPathCert, FileMode.Create);
                        CertificateTempImgPathDB="/"+CertificateTempImgPathDB+FileName;
                        dto.CertificateTempImg.CopyTo(fileStream);
                    }

                    int CompetitionId = dto.Id;
                    var _competition = _mapper.Map<Competition>(dto);
                    if(CompetitionId!=0)
                    {
                        // Get existing entity by id (Example)
                        _competition=await _unitOfWork.Competitions.GetByIdAsync(CompetitionId);
                        if(_competition==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorMSG="Invalid competition Id";
                            Response.Errors.Add(error);
                            return BadRequest(Response);


                        }

                        _mapper.Map<CompetitionCreateDTO , Competition>(dto , _competition);
                        if(!string.IsNullOrWhiteSpace(IMGPathDB))
                        {
                            // check if have image before for remove first
                            if(!string.IsNullOrWhiteSpace(_competition.ImagePath))
                            {
                                string webRootPath = _environment.WebRootPath;
                                string fullPath = webRootPath + _competition.ImagePath;
                                if(System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }

                            _competition.ImagePath=IMGPathDB;
                        }

                        //if (!string.IsNullOrWhiteSpace(CertificateTempImgPathDB))
                        //{
                        //    // check if have CertificateTempImg before for remove first
                        //    if (!string.IsNullOrWhiteSpace(_competition.CertificateTempImg))
                        //    {
                        //        string webRootPath = _environment.WebRootPath;
                        //        string fullPath = webRootPath + _competition.CertificateTempImg;
                        //        if (System.IO.File.Exists(fullPath))
                        //        {
                        //            System.IO.File.Delete(fullPath);
                        //        }
                        //    }
                        //    _competition.CertificateTempImg = CertificateTempImgPathDB;
                        //}
                        _unitOfWork.Competitions.Update(_competition);
                    }

                    _unitOfWork.Complete();
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);

              
            }
        }

        // Competions User

        [Authorize]
        [HttpPost("addCompetitionUser")]
        public async Task<IActionResult> CreateCompetitionUserAsync([FromForm] CompetitionUserDTO dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    //if (!ModelState.IsValid)
                    //    return BadRequest(ModelState);
                    var checkCompetitionsIsValid =
                    await _unitOfWork.Competitions.FindAsync((x => x.Id == dto.CompetitionId));
                    if(checkCompetitionsIsValid==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorMSG="Invalid Competition Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    if(!string.IsNullOrWhiteSpace(checkCompetitionsIsValid.Code))
                    {
                        if(string.IsNullOrWhiteSpace(dto.VerifyCode))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorMSG="يرجى ادخال كود المسابقه اول";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }

                        if(checkCompetitionsIsValid.Code!=dto.VerifyCode.Trim())
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorMSG="كود المسابقه خطاء لا يمكنك الاشتراك بها";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }

                    var checkCompetitionUserIsExistList = await _unitOfWork.CompetitionUsers.CountAsync((c =>
                    c.CompetitionId == dto.CompetitionId && c.HrUserId == dto.HrUserId));
                    if(checkCompetitionUserIsExistList>=1)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorMSG="هذا المستخدم مشترك بالفعل فى هذه المسابقه";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var _competitionUser = new CompetitionUser(); // _mapper.Map<CompetitionUser>(dto);
                    _competitionUser.CompetitionId=dto.CompetitionId;
                    _competitionUser.HrUserId=dto.HrUserId;
                    _competitionUser.TotalScore=0;
                    _competitionUser.CreationBy="system";
                    _competitionUser.CreationDateOfapprovedAndReject=DateTime.Now;
                    _competitionUser.EnrollmentStatus="approved";
                    await _unitOfWork.CompetitionUsers.AddAsync(_competitionUser);
                    _unitOfWork.Complete();
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost("deleteCompetition")]
        public async Task<IActionResult> DeleteAsync([FromHeader] int id)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    var _Competition = await _unitOfWork.Competitions.GetByIdAsync(id);
                    if(_Competition==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="لا توجد هذه المسابقة ليتم حذفها";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    //var _unitAttachments = await _unitOfWork.UnitAttachments.FindAllAsync((x => x.UnitId == id));
                    //if (_unitAttachments.Count() > 0)
                    //{
                    //    foreach (var item in _unitAttachments)
                    //    {
                    //        string UnitAttachPath = this.Environment.WebRootPath + item.FilePath; // Path.Combine(this.Environment.WebRootPath, item.FilePath);
                    //        //var AttachmentPath = HttpContext.Current.Server.MapPath(LicenseAttachmentDb.AttachmentPath);
                    //        if (System.IO.File.Exists(UnitAttachPath))
                    //        {
                    //            System.IO.File.Delete(UnitAttachPath);
                    //        }
                    //    }
                    //    _unitOfWork.UnitAttachments.DeleteRange(_unitAttachments);
                    //}
                    if(!string.IsNullOrWhiteSpace(_Competition.ImagePath))
                    {
                        string CompetitionIMG = this._environment.WebRootPath + _Competition.ImagePath;
                        if(System.IO.File.Exists(CompetitionIMG))
                        {
                            System.IO.File.Delete(CompetitionIMG);
                        }
                    }

                    _unitOfWork.Competitions.Delete(_Competition);
                    var res = _unitOfWork.Complete();
                    if(res==0)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="عملية مسح المسابقه لم تكتمل ...يرجى التواصل مع الادمن";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }


        [Authorize]
        [HttpPost("addListCompetitionMemberAdmin")] //new controll
        public async Task<IActionResult> addListCompetitionMemberAdmin(CompetitionMemberAdminCreateNewDTO dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var res = 0;
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    var checkCompetitionsIsValid =
                    await _unitOfWork.Competitions.FindAsync((x => x.Id == dto.CompetitionId));
                    if(checkCompetitionsIsValid==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Competition Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);


                    }

                    //if (checkCompetitionsIsValid.ApplicationUserId != ApplicationUserId)
                    //{
                    //    Response.Result = false;
                    //    Response.Errors.Add("ليس لديك الصلاحيه لاضافة عضو فى هذه المسابقه");
                    //    return BadRequest(Response);
                    //}
                    foreach(var item in dto.ApplicationUserIds)
                    {
                        var checkUserIsValid = await _unitOfWork.HrUsers.FindAsync((x => x.Id == item));
                        if(checkUserIsValid==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="Invalid User Id";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }

                        var checkCompetitionUserIsExistList = await _unitOfWork.CompetitionMemberAdmins.CountAsync((c =>
                        c.CompetitionId == dto.CompetitionId && c.HrUserId == item));
                        var userRoleDb = _unitOfWork.UserRoles.FindAllQueryable(x=>x.Id > 0 ,new []{"Role"});
                        var hrUserDb = _unitOfWork.HrUsers.FindAllQueryable(x=>x.Id > 0);
                        if(checkCompetitionUserIsExistList==0)
                        {
                            var UserId = hrUserDb.Where(x=>x.Id == item ).Select(y=>y.UserId).FirstOrDefault();
                            var role = userRoleDb.Where(x=>x.UserId == UserId).Select(r=>r.Role.Name).FirstOrDefault();
                            var _competitionMemberAdmin =
                            new CompetitionMemberAdmin(); // _mapper.Map<CompetitionUser>(dto);
                            _competitionMemberAdmin.CompetitionId=dto.CompetitionId;
                            _competitionMemberAdmin.HrUserId=item;
                            _competitionMemberAdmin.CreationDate=DateTime.Now;
                            _competitionMemberAdmin.RoleName=role;
                            await _unitOfWork.CompetitionMemberAdmins.AddAsync(_competitionMemberAdmin);
                            res=_unitOfWork.Complete();
                            if(res>0)
                            {
                                Response.Result=true;
                            }
                            else
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.ErrorMSG="\"لم يتم اكتمال العمليه و يرجى المحاوله مره اخرى";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }
                        }
                    }
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }


        [Authorize]
        [HttpPost("removeCompetitionMemberAdmin")] //new controll
        public async Task<IActionResult> RemoveCompetitionMemberAdminAsync(
            [FromForm] CompetitionMemberAdminCreateDTO dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                var data = new List<CompetitionDTO>();
                if(Response.Result)
                {

                    var checkCompetitionsIsValid =
                    await _unitOfWork.Competitions.FindAsync((x => x.Id == dto.CompetitionId));
                    if(checkCompetitionsIsValid==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Competition Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var checkUserIsValid =
                    await _unitOfWork.HrUsers.FindAsync((x => x.Id == dto.ApplicationUserId));
                    if(checkUserIsValid==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid User Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var checkCompetitionUserIsExist = await _unitOfWork.CompetitionMemberAdmins.FindAsync((c =>
                    c.CompetitionId == dto.CompetitionId && c.HrUserId == dto.ApplicationUserId));
                    if(checkCompetitionUserIsExist==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا المستخدم ليس عضو ادمن  فى هذه المسابقه";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    _unitOfWork.CompetitionMemberAdmins.Delete(checkCompetitionUserIsExist);
                    var res = _unitOfWork.Complete();
                    if(res>0)
                    {
                        Response.Result=true;
                    }
                    else
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="لم يتم اكتمال العمليه و يرجى المحاوله مره اخرى";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }



        [HttpGet("GetCompetitionMemberAdmin")] //new control
        public async Task<IActionResult> GetCompetitionMemberAdminListAsync([FromHeader] int? CompetitionId)
        {
            var Response = new BaseResponseWithData<CompetitorUserInfoDTO2>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    CompetitorUserInfoDTO2 data = new CompetitorUserInfoDTO2();
                    if(CompetitionId==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Competition Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);
                    }

                    var checkCompetitionsIsValid = await _unitOfWork.Competitions.FindAsync((x => x.Id == CompetitionId));
                    if(checkCompetitionsIsValid==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Competition Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var CompetMemberAdminList =
                    await _unitOfWork.CompetitionMemberAdmins.FindAllAsync((c => c.CompetitionId == CompetitionId && (c.RoleName == "management" || c.RoleName == "adminCompetition")),
                        new[] { "ApplicationUser" });
                    var CompetMemberdoctorList =
                    await _unitOfWork.CompetitionMemberAdmins.FindAllAsync((c => c.CompetitionId == CompetitionId && (c.RoleName == "doctor" || c.RoleName == "assistant")),
                        new[] { "ApplicationUser" });
                    if(CompetMemberAdminList.Count()>=1)
                    {
                        var test  =  _mapper.Map<IEnumerable<CompetitorUserInfoDTO>>(CompetMemberAdminList);
                        data.adminlist=(List<CompetitorUserInfoDTO>)test;
                    }
                    if(CompetMemberdoctorList.Count()>=1)
                    {
                        var test  = _mapper.Map<IEnumerable<CompetitorUserInfoDTO>>(CompetMemberdoctorList);
                        data.doctorlist=(List<CompetitorUserInfoDTO>)test;
                    }

                    Response.Data=data;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        //[Authorize(Roles = "admin")]
        [Authorize]
        [HttpGet("GetCompetitionAnalysis")]
        public async Task<IActionResult> GetCompetitionAnalysisAsync([FromHeader] int? CompetitionId ,[FromHeader] long HrUserId)
        {
            var Response = new BaseResponseWithData<CompetitionAnalysisDTO>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    if(CompetitionId==null)
                    {

                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Competition Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);
                    }

                    if(!await _permissionService.CheckUserHasPermissionManageCompetition(HrUserId , CompetitionId??0))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="\"ليس لديك الصلاحيه \"";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var dto = new CompetitionAnalysisDTO();
                    if(CompetitionId!=null)
                    {
                        var UserComPetitorsList =
                        await _unitOfWork.CompetitionUsers.FindAllAsync((x => x.Id != 0), new[] { "HrUser" });
                        dto.CountOfSubscribers=UserComPetitorsList.Count();
                        dto.PercentMaleSubscribers=dto.CountOfSubscribers!=0
                            ? (String.Format("{0:0.0}" ,
                                (UserComPetitorsList.Where(x => x.HrUser?.Gender=="Male").Count()/
                                 dto.CountOfSubscribers)*100)+"%")
                            : "0%";
                        dto.PercentFemaleSubscribers=dto.CountOfSubscribers!=0
                            ? (String.Format("{0:0.0}" ,
                                (UserComPetitorsList.Where(x => x.HrUser?.Gender=="Female").Count()/
                                 dto.CountOfSubscribers)*100)+"%")
                            : "0%";
                        dto.PercentOtherSubscribers=dto.CountOfSubscribers!=0
                            ? (String.Format("{0:0.0}" ,
                                (UserComPetitorsList.Where(x => x.HrUser?.Gender is null).Count()/
                                 dto.CountOfSubscribers)*100)+"%")
                            : "0%";
                        dto.CountOfMaleSubscribers=
                            UserComPetitorsList.Where(x => x.HrUser?.Gender=="Male").Count();
                        dto.CountOfFemaleSubscribers=
                            UserComPetitorsList.Where(x => x.HrUser?.Gender=="Female").Count();
                        dto.CountOfOtherSubscribers=
                            UserComPetitorsList.Where(x => x.HrUser?.Gender is null).Count();
                        dto.CountOfSubscribersLessThan20=UserComPetitorsList
                            .Where(x => x.HrUser?.DateOfDeath?.CalculateAge()<=20).Count();
                        dto.CountOfSubscribersBetween20To30=UserComPetitorsList.Where(x =>
                            x.HrUser?.DateOfDeath?.CalculateAge()>20&&
                            x.HrUser?.DateOfDeath?.CalculateAge()<=30).Count();
                        dto.CountOfSubscribersBetween30To40=UserComPetitorsList.Where(x =>
                            x.HrUser?.DateOfDeath?.CalculateAge()>30&&
                            x.HrUser?.DateOfDeath?.CalculateAge()<=40).Count();
                        dto.CountOfSubscribersBetween40To50=UserComPetitorsList.Where(x =>
                            x.HrUser?.DateOfDeath?.CalculateAge()>40&&
                            x.HrUser?.DateOfDeath?.CalculateAge()<=50).Count();
                        dto.CountOfSubscribersBetween50To60=UserComPetitorsList.Where(x =>
                            x.HrUser?.DateOfDeath?.CalculateAge()>50&&
                            x.HrUser?.DateOfDeath?.CalculateAge()<=60).Count();
                        dto.CountOfSubscribersAbove60=UserComPetitorsList
                            .Where(x => x.HrUser?.DateOfDeath?.CalculateAge()>60).Count();
                        dto.CountOfSubscribersWithoutAge=
                            UserComPetitorsList.Where(x => x.HrUser?.DateOfDeath is null).Count();

                        // var ChurchSubscribersList = UserComPetitorsList.Where(x => x.ApplicationUser?.ChurchName is not null).ToList();
                        // dto.CountOfChurchSubscribers = UserComPetitorsList.Select(x => x.ApplicationUser?.ChurchName).Distinct().Count();

                        //var ChurchNameGrouping = ChurchSubscribersList.GroupBy(x => x.ApplicationUser?.ChurchName).Select(x => new ChurchPercent
                        //{
                        //    ChurchName = x.Key,
                        //    Percent = dto.CountOfSubscribers != 0 ? (String.Format("{0:0.0}",
                        //    (x.Count() / dto.CountOfSubscribers) * 100) + "%") : "0%",
                        //    PercentPerDecimal = dto.CountOfSubscribers != 0 ? (x.Count() / dto.CountOfSubscribers ?? 0) * 100 : 0
                        //}).OrderByDescending(x => x.PercentPerDecimal).Take(10).ToList();

                        // dto.ChurchPercent = ChurchNameGrouping;
                    }

                    Response.Data=dto;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }


        [Authorize]
        [HttpPost("CreateCompetitionAsync")] //new controll
        public async Task<IActionResult> CreateCompetitionAsync([FromForm] CompetitionCreateNewDTO dto)
        {
            BaseResponseWithData<int> Response = new BaseResponseWithData<int>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    if(dto.Image!=null)
                    {
                        if(!_allowedExtenstions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="Image Only .png , .jpg , .jpeg and .svg images are allowed!";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }

                        if(dto.Image.Length>_maxAllowedPosterSize)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="Max allowed size for Cover Image greater than 5MB!";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }

                    string IMGPathDB = null;
                    if(dto.Id==0)
                    {
                        var imageOfSubject = _unitOfWork.Subjects.FindAll(x => x.Id == dto.SubjectId)
                        .Select(y => y.ImagePath).FirstOrDefault();
                        if(dto.Image!=null)
                        {
                            IMGPathDB="Competitions/";
                            string SaveIMGPath = Path.Combine(this._environment.WebRootPath, IMGPathDB);
                            if(!System.IO.Directory.Exists(SaveIMGPath))
                            {
                                System.IO.Directory.CreateDirectory(SaveIMGPath); //Create directory if it doesn't exist
                            }

                            string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_CompetetionLogo_" +
                                          dto.Image.FileName.Trim().Replace(" ", "");
                            SaveIMGPath=SaveIMGPath+FileName;
                            using FileStream fileStream = new(SaveIMGPath, FileMode.Create);
                            IMGPathDB="/"+IMGPathDB+FileName;
                            dto.Image.CopyTo(fileStream);
                        }

                        dto.ImagePath=IMGPathDB;
                        if(imageOfSubject!=null&&dto.Image==null)
                        {
                            dto.ImagePath=imageOfSubject;
                        }

                        var tempdata = await _unitOfWork.Competitions.AddAsync(_mapper.Map<Competition>(dto));
                        _unitOfWork.Complete();
                        dto.CompetitionId=tempdata.Id;
                        dto.Active=true;
                        if(dto.SpecialdeptId!=0)
                        {
                            //dto.ProgramId=_unitOfWork.Academiclevels.FindAll(x => x.Id==dto.AcademiclevelId)
                            //    .Select(y => y.ProgramId).FirstOrDefault()??null;
                            await _unitOfWork.AssignedSubjects.AddAsync(_mapper.Map<AssignedSubject>(dto));
                        }

                        if((dto.SpecialdeptId==0)&&dto.deptId!=null)
                        {
                            var specialdept =
                            _unitOfWork.Specialdepts.Add(new Specialdept
                            {
                                DeptartmentId = (int)dto.deptId,
                                Name = "عام"
                            });
                            _unitOfWork.Complete();
                            dto.SpecialdeptId=specialdept.Id;

                            //dto.ProgramId=_unitOfWork.Academiclevels.FindAll(x => x.Id==dto.AcademiclevelId)
                            //    .Select(y => y.ProgramId).FirstOrDefault()??null;
                            await _unitOfWork.AssignedSubjects.AddAsync(_mapper.Map<AssignedSubject>(dto));
                        }

                        _unitOfWork.Complete();
                    }

                    if(dto.Id!=0)
                    {
                        var _competition = _mapper.Map<Competition>(dto);
                        List<CompetitionType> competitionTypes = new ();
                        _competition=_unitOfWork.Competitions.FindAll(x => x.Id==dto.Id).FirstOrDefault();
                        var oldImage = _competition.ImagePath;
                        var _AssignedSubject = _unitOfWork.AssignedSubjects.FindAll(x => x.CompetitionId == dto.Id)
                        .FirstOrDefault();
                        if(_competition!=null)
                        {

                            if(dto.SubjectScore!=_competition.SubjectScore)
                            {
                                var tempdata = _unitOfWork.CompetitionTypes .FindAll(x => x.CompetitionId == dto.Id );
                                foreach(var item in tempdata)
                                {

                                    item.TotalScore=(double)(dto.SubjectScore*(item.Percentage/100));
                                    competitionTypes.Add(item);
                                }
                            }
                            _mapper.Map<CompetitionCreateNewDTO , Competition>(dto , _competition);
                            dto.CompetitionId=dto.Id;
                            // _mapper.Map<CompetitionCreateNewDTO, AssignedSubject>(dto, _AssignedSubject);
                            //_competition.Name = dto.Name;
                            if(dto.Image!=null)
                            {
                                IMGPathDB="Competitions/";
                                string SaveIMGPath = Path.Combine(this._environment.WebRootPath, IMGPathDB);
                                if(!System.IO.Directory.Exists(SaveIMGPath))
                                {
                                    System.IO.Directory.CreateDirectory(SaveIMGPath); //Create directory if it doesn't exist
                                }

                                string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_CompetetionLogo_" +
                                              dto.Image.FileName.Trim().Replace(" ", "");
                                SaveIMGPath=SaveIMGPath+FileName;
                                using FileStream fileStream = new(SaveIMGPath, FileMode.Create);
                                IMGPathDB="/"+IMGPathDB+FileName;
                                dto.Image.CopyTo(fileStream);
                                dto.ImagePath=IMGPathDB;
                                _competition.ImagePath=IMGPathDB;
                            }
                            else
                            {
                                _competition.ImagePath=oldImage;
                            }


                        }


                        _unitOfWork.Competitions.Update(_competition);
                        _AssignedSubject.SubjectId=(int)dto.SubjectId;
                        _unitOfWork.AssignedSubjects.Update(_AssignedSubject);
                        _unitOfWork.CompetitionTypes.UpdateRange(competitionTypes);
                        _unitOfWork.Complete();
                    }

                    Response.Data=(int)dto.CompetitionId;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [Authorize]
        [HttpPost("CreateCompetitionType")] //new controll
        public async Task<IActionResult> CreateCompetitionType(CompetitionTypeDto dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    var subjectscore = _unitOfWork.Competitions.FindAll(x => x.Id == dto.CompetitionId)
                    .Select(y => y.SubjectScore).FirstOrDefault();
                    var competitiontpes = new List<CompetitionType>();
                    foreach(var item in dto.typeLists)
                    {
                        var tempdata = _unitOfWork.CompetitionTypes
                        .FindAll(x => x.CompetitionId == dto.CompetitionId && x.TypeId == item.TypeId).FirstOrDefault();
                        if(tempdata==null)
                        {
                            var tempdata2 = await _unitOfWork.CompetitionTypes.AddAsync(new CompetitionType
                            {
                                CompetitionId = dto.CompetitionId,
                                TypeId = item.TypeId,
                                Qty = item.Qty,
                                TotalScore = (int)item.TotalScore,
                                Percentage = item.percentage != null? ((double)item.TotalScore / (double)subjectscore) * 100 : 0
                            });
                            competitiontpes.Add(tempdata2);
                            _unitOfWork.Complete();
                        }
                        else
                        {
                            tempdata.Qty=item.Qty;
                            tempdata.TotalScore=(int)item.TotalScore;
                            tempdata.Percentage=item.percentage!=null ? (((double)item.TotalScore/(double)subjectscore)*100) : 0;
                            _unitOfWork.CompetitionTypes.Update(tempdata);
                            _unitOfWork.Complete();
                        }
                    }
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [Authorize]
        [HttpGet("GetCompetitionTypeAndCompetitionDetailsforCompetition")] //new controll
        public async Task<IActionResult> GetCompetitionTypeAndCompetitionDetailsforCompetition(
      [FromHeader] int competitionId , [FromHeader] long HrUserId , [FromHeader] int? typeId = 1)
        {
            BaseResponseWithData<CompetitionTypeDto> Response = new BaseResponseWithData<CompetitionTypeDto>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var competitiondto = new CompetitionTypeDto();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    var competitionsUsersDB = _unitOfWork.CompetitionUsers.FindAll(x=>x.FinishStatus != "Finished" );
                    var competitionDaysUsersDB = _unitOfWork.CompetitionDayUsers.FindAll(x=>x.Id > 0 );
                    var competitionDayDB = _unitOfWork.CompetitionDays.FindAll(x => x.Id > 0 , new[] { "Lecturer" });
                    var _user = _unitOfWork.HrUsers.FindAll(x => x.Id == HrUserId).FirstOrDefault();
                    var userRole =  _unitOfWork.UserRoles.FindAll(x=>x.UserId == _user.UserId,new []{"Role"}).Select(n=>n.Role.Name).FirstOrDefault();
                    var assignedSubject = _unitOfWork.AssignedSubjects
               .FindAll(x => x.CompetitionId == competitionId, new[] { "Academiclevel", "Programm" ,"AcademicYear"})
               .FirstOrDefault();
                    var hallsDB = _unitOfWork.Halls.FindAll(x => x.Id > 0);
                    if(userRole.Contains("student"))
                    {
                        var AllTempdate = new List<TypeList>();
                        var specilTypelist = new List<FilterTabledDto>();
                        //var ALLspecialTypelist  = new List<FilterTabledDto>();
                        var competition = _unitOfWork.Competitions.FindAll(x => x.Id == competitionId).FirstOrDefault();
                        competitiondto.CompetitionId=competitionId;
                        competitiondto.ImagePath=competition.ImagePath!=null ? BaseURL+competition.ImagePath : null;
                        competitiondto.Name=competition.Name;
                        competitiondto.Active=competition.Active;
                        competitiondto.Accreditedhours=competition.Accreditedhours;
                        competitiondto.StudyingHours=competition.StudyingHours;
                        competitiondto.SubjectScore=competition.SubjectScore;
                        competitiondto.NumbersOfStudents=_unitOfWork.CompetitionUsers
                            .FindAll(x => x.CompetitionId==competitionId).Count();
                        competitiondto.NumbersOfAdmin=_unitOfWork.CompetitionMemberAdmins
                            .FindAll(x => x.CompetitionId==competitionId&&(x.RoleName.Contains("adminCompetition")||x.RoleName.Contains("management"))).Count();
                        competitiondto.NumbersOfDoctor=_unitOfWork.CompetitionMemberAdmins
                            .FindAll(x => x.CompetitionId==competitionId&&(x.RoleName.Contains("doctor"))).Count();
                        competitiondto.SpecialDept=assignedSubject.SpecialdeptId;
                        competitiondto.LevelId=assignedSubject.AcademiclevelId;
                        competitiondto.LevelName=assignedSubject.Academiclevel.Name;
                        competitiondto.ProgramName=assignedSubject.Programm?.Name;
                        competitiondto.AcademicYearName=assignedSubject.AcademicYear.Term;
                        //competitiondto.ProgramName=assignedSubject.Programm?.Name??null;
                        //competitiondto.LectureName=competition.ApplicationUser.FirstName+" "+competition.ApplicationUser.MiddleName;
                        var TotalScoreStudent = competitionDaysUsersDB.Where(x => x.HrUserId == HrUserId)
                   .Select(x => x.UserScore).Sum();
                        competitiondto.TotalScoreStudent=TotalScoreStudent==null ? 0 : TotalScoreStudent;
                        var TypeIds = _unitOfWork.CompetitionTypes
                   .FindAll(x => x.CompetitionId == competitionId, new[] { "Type" }).Select(y => y.TypeId)
                   .ToList();
                        var TypeDB = _unitOfWork.Types.FindAll(x => x.Id > 0).ToList();
                        if(TypeIds.Count()>0)
                        {
                            var competitiontype = _unitOfWork.CompetitionTypes
                       .FindAll(x => x.CompetitionId == competitionId).ToList();
                            foreach(var item in TypeIds)
                            {
                                var tempdate = new TypeList();
                                tempdate.TypeId=item;
                                tempdate.TypeName=TypeDB.Where(x => x.Id==item).Select(y => y.Name).FirstOrDefault()??
                                                    "";
                                tempdate.Qty=competitiontype.Where(x => x.TypeId==item).Select(y => y.Qty)
                                    .FirstOrDefault();
                                tempdate.TotalScore=competitiontype.Where(x => x.TypeId==item).Select(y => y.TotalScore)
                                    .FirstOrDefault();
                                tempdate.percentage=competitiontype.Where(x => x.TypeId==item).Select(y => y.Percentage)
                                    .FirstOrDefault();
                                tempdate.AllowAudience=competitiontype.Where(x => x.TypeId==item)
                                    .Select(y => y.AllowAudience).FirstOrDefault();
                                var competitondaytype =competitionDayDB
                           .Where(x => x.CompetitionId == competition.Id && x.TypeId == item).Count();
                                var competitondayIds = competitionDayDB
                           .Where(x => x.CompetitionId == competition.Id && x.TypeId == item).Select(y => y.Id)
                           .ToList();
                                foreach(var itemId in competitondayIds)
                                {
                                    var scoreforstudent = competitionDaysUsersDB
                               .Where(x => x.CompetitionDayId == itemId && x.HrUserId == HrUserId)
                               .Select(y => y.UserScore).FirstOrDefault() ?? 0;
                                    tempdate.TotalScoreForStudent=
                                        (tempdate.TotalScoreForStudent??0)+(decimal)scoreforstudent;
                                }

                                var competitiontypeTotal = _unitOfWork.CompetitionTypes
                           .FindAll(x => x.CompetitionId == competition.Id && x.TypeId == item).Select(y => y.Qty)
                           .FirstOrDefault();
                                var remainingNum = competitiontypeTotal - competitondaytype;
                                if(competitiontypeTotal==0)
                                {
                                    remainingNum=0;
                                }

                                remainingNum=competitiontypeTotal-competitondaytype;
                                tempdate.remainingNumber=remainingNum;
                                specilTypelist=competitionDayDB.Where(x => x.CompetitionId==competitionId&&x.TypeId==item
                                       ).Select(com => new FilterTabledDto
                                       {
                                           CompetitionDayId=com.Id ,
                                           TypeId=item ,
                                           Name=com.Name??"" ,
                                           From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
                                           To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
                                           lecturerName=
                                            com.Lecturer?.FirstName+" "+com.Lecturer?.MiddleName ,
                                           levelName=assignedSubject?.Academiclevel?.Name ,
                                           ProgramName=assignedSubject?.Programm?.Name??null ,
                                           hallName=hallsDB.Where(x => x.Id==com.HallId).Select(y => y.Name).FirstOrDefault() ,
                                           hallid=com.HallId ,
                                           FromScore=com.FromScore ,
                                           AttendanceFlag=competitionDaysUsersDB.Where(x => x.HrUserId==HrUserId&&x.CompetitionDayId==com.Id).Select(r => r.Attendance).FirstOrDefault()==true ? true :
                                             (com.To<TimeZoneEgypt()&&com.To!=null) ? false : null ,
                                           UserScore=competitionDaysUsersDB
                .Where(x => x.HrUserId==HrUserId&&x.CompetitionDayId==com.Id)
                .Select(r => r.UserScore)
                .FirstOrDefault()!=null ?
                 competitionDaysUsersDB
                .Where(x => x.HrUserId==HrUserId&&x.CompetitionDayId==com.Id)
                .Select(r => r.UserScore)
                .FirstOrDefault() :
                competitionDaysUsersDB
                    .Where(x => x.HrUserId==HrUserId&&x.CompetitionDayId==com.Id&&com.To<TimeZoneEgypt())
                    .Any() ? 0 : null
                                       }).ToList();
                                tempdate.specialtypeList=specilTypelist;
                                AllTempdate.Add(tempdate);
                            }
                        }
                        else
                        {
                            var alltypeIds = _unitOfWork.Types.FindAll(x => x.Id > 0).Select(y => y.Id).ToList();
                            foreach(var item in alltypeIds)
                            {
                                var tempdate = new TypeList();
                                tempdate.TypeId=item;
                                tempdate.TypeName=TypeDB.Where(x => x.Id==item).Select(y => y.Name).FirstOrDefault()??
                                                    "";
                                tempdate.Qty=0;
                                tempdate.TotalScore=0;
                                tempdate.percentage=0;
                                tempdate.remainingNumber=0;
                                tempdate.TotalScoreForStudent=0;
                                specilTypelist=competitionDayDB
                                    .Where(x => x.CompetitionId==competitionId&&x.TypeId==typeId
                                       ).Select(com => new FilterTabledDto
                                       {
                                           CompetitionDayId=com.Id ,
                                           Name=com.Name ,
                                           From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
                                           To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
                                           lecturerName=
                                            com.Lecturer?.FirstName+" "+com.Lecturer?.MiddleName ,
                                           levelName=assignedSubject.Academiclevel?.Name ,
                                           ProgramName=assignedSubject.Programm?.Name??null ,
                                           hallName=hallsDB.Where(x => x.Id==com.HallId).Select(y => y.Name).FirstOrDefault() ,
                                           hallid=com.HallId ,
                                           AttendanceFlag=competitionDaysUsersDB.Where(x => x.HrUserId==HrUserId&&x.CompetitionDayId==com.Id).Select(r => r.Attendance).FirstOrDefault()==true ? true :
                                             (com.To<TimeZoneEgypt()&&com.To!=null)==true ? false : null ,
                                           UserScore=competitionDaysUsersDB
                .Where(x => x.HrUserId==HrUserId&&x.CompetitionDayId==com.Id)
                .Select(r => r.UserScore)
                .FirstOrDefault()!=null ?
                 competitionDaysUsersDB
                .Where(x => x.HrUserId==HrUserId&&x.CompetitionDayId==com.Id)
                .Select(r => r.UserScore)
                .FirstOrDefault() :
                competitionDaysUsersDB
                    .Where(x => x.HrUserId==HrUserId&&x.CompetitionDayId==com.Id&&com.To<TimeZoneEgypt())
                    .Any() ? 0 : null


                                       }).ToList();
                                tempdate.specialtypeList=specilTypelist;
                                AllTempdate.Add(tempdate);
                            }
                        }

                        //competitiondto.typeLists.specialtypeList = specilTypelist;
                        competitiondto.typeLists=AllTempdate;
                        Response.Result=true;
                        Response.Data=competitiondto;
                    }
                    else if(userRole.Contains("admin"))
                    {
                        var AllTempdate = new List<TypeList>();
                        var specilTypelist = new List<FilterTabledDto>();
                        //var ALLspecialTypelist  = new List<FilterTabledDto>();
                        var competition = _unitOfWork.Competitions.FindAll(x => x.Id == competitionId).FirstOrDefault();
                        competitiondto.CompetitionId=competitionId;
                        competitiondto.ImagePath=competition.ImagePath!=null ? BaseURL+competition.ImagePath : null;
                        competitiondto.Name=competition.Name;
                        competitiondto.Active=competition.Active;
                        competitiondto.Accreditedhours=competition.Accreditedhours;
                        competitiondto.StudyingHours=competition.StudyingHours;
                        competitiondto.SubjectScore=competition.SubjectScore;
                        competitiondto.NumbersOfStudents=_unitOfWork.CompetitionUsers
                            .FindAll(x => x.CompetitionId==competitionId).Count();
                        competitiondto.NumbersOfAdmin=_unitOfWork.CompetitionMemberAdmins
                             .FindAll(x => x.CompetitionId==competitionId&&(x.RoleName.Contains("adminCompetition")||x.RoleName.Contains("management"))).Count();
                        competitiondto.NumbersOfDoctor=_unitOfWork.CompetitionMemberAdmins
                            .FindAll(x => x.CompetitionId==competitionId&&(x.RoleName.Contains("doctor"))).Count();
                        //var TotalScoreStudent = _unitOfWork.CompetitionUsers.FindAll(x => x.ApplicationUserId == userId).Select(x => x.TotalScore).FirstOrDefault();
                        competitiondto.TotalScoreStudent=null;
                        competitiondto.SpecialDept=assignedSubject.SpecialdeptId;
                        competitiondto.LevelId=assignedSubject.AcademiclevelId;
                        competitiondto.LevelName=assignedSubject.Academiclevel?.Name;
                        competitiondto.ProgramName=assignedSubject.Programm?.Name;
                        competitiondto.AcademicYearName=assignedSubject.AcademicYear?.Term;

                        competitiondto.NumbersOfRequestDelay=competitionsUsersDB.Where(x => x.DelayRequestStatus=="DelayRequest"&&x.DelayRequestStatus!="rejectDelay"&&x.DelayOrWithdrawalStatus!="delay"&&x.CompetitionId==competitionId).Count()!=0 ? (competitionsUsersDB.Where(x => x.DelayRequestStatus=="DelayRequest"&&x.DelayRequestStatus!="rejectDelay"&&x.DelayOrWithdrawalStatus!="delay"&&x.CompetitionId==competitionId).Count()) : null;
                        competitiondto.NumbersOfRequestWithdraw=competitionsUsersDB.Where(x => x.WithdrawalRequestStatus=="WithdrawRequest"&&x.WithdrawalRequestStatus!="rejectWithdraw"&&x.DelayOrWithdrawalStatus!="Withdraw"&&x.CompetitionId==competitionId).Count()!=0 ? (competitionsUsersDB.Where(x => x.WithdrawalRequestStatus=="WithdrawRequest"&&x.WithdrawalRequestStatus!="rejectWithdraw"&&x.DelayOrWithdrawalStatus!="delay"&&x.CompetitionId==competitionId).Count()) : null;
                        competitiondto.NumbersOfRejectDelay=competitionsUsersDB.Where(x => x.DelayRequestStatus=="rejectDelay"&&x.CompetitionId==competitionId).Count()!=0 ? (competitionsUsersDB.Where(x => x.DelayRequestStatus=="rejectDelay"&&x.CompetitionId==competitionId).Count()) : null;
                        competitiondto.NumbersOfRejectWithdraw=competitionsUsersDB.Where(x => x.WithdrawalRequestStatus=="rejectWithdraw"&&x.CompetitionId==competitionId).Count()!=0 ? (competitionsUsersDB.Where(x => x.WithdrawalRequestStatus=="rejectWithdraw"&&x.CompetitionId==competitionId).Count()) : null;
                        competitiondto.NumbersOfPending=competitionsUsersDB.Where(x => x.EnrollmentStatus=="pending"&&x.CompetitionId==competitionId).Count()!=0 ? (competitionsUsersDB.Where(x => x.EnrollmentStatus=="pending"&&x.CompetitionId==competitionId).Count()) : null;
                        //competitiondto.ProgramName=assignedSubject.Programm?.Name??null;
                        //competitiondto.LectureName=competition.ApplicationUser.FirstName+" "+competition.ApplicationUser.MiddleName;
                        var TypeIds = _unitOfWork.CompetitionTypes
                   .FindAll(x => x.CompetitionId == competitionId, new[] { "Type" }).Select(y => y.TypeId)
                   .ToList();
                        if(TypeIds.Count()>0)
                        {
                            var competitiontype = _unitOfWork.CompetitionTypes
                       .FindAll(x => x.CompetitionId == competitionId).ToList();
                            var TypeDB = _unitOfWork.Types.FindAll(x => x.Id > 0).ToList();
                            foreach(var item in TypeIds)
                            {
                                var tempdate = new TypeList();
                                tempdate.TypeId=item;
                                tempdate.TypeName=TypeDB.Where(x => x.Id==item).Select(y => y.Name).FirstOrDefault()??
                                                    "";
                                tempdate.Qty=competitiontype.Where(x => x.TypeId==item).Select(y => y.Qty)
                                    .FirstOrDefault();
                                tempdate.TotalScore=competitiontype.Where(x => x.TypeId==item).Select(y => y.TotalScore)
                                    .FirstOrDefault();
                                tempdate.TotalScoreForStudent=null;
                                tempdate.percentage=competitiontype.Where(x => x.TypeId==item).Select(y => y.Percentage)
                                    .FirstOrDefault();
                                tempdate.AllowAudience=competitiontype.Where(x => x.TypeId==item)
                                    .Select(y => y.AllowAudience).FirstOrDefault();

                                var competitondaytype = _unitOfWork.CompetitionDays
                           .FindAll(x => x.CompetitionId == competition.Id && x.TypeId == item).Count();
                                var competitondayIds = _unitOfWork.CompetitionDays
                           .FindAll(x => x.CompetitionId == competition.Id && x.TypeId == item).Select(y => y.Id)
                           .ToList();
                                foreach(var itemId in competitondayIds)
                                {
                                    //var scoreforstudent = _unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId == itemId && x.ApplicationUserId == userId).Select(y => y.UserScore).FirstOrDefault() ?? 0;
                                    tempdate.TotalScoreForStudent=null;
                                }

                                var competitiontypeTotal = _unitOfWork.CompetitionTypes
                           .FindAll(x => x.CompetitionId == competition.Id && x.TypeId == item).Select(y => y.Qty)
                           .FirstOrDefault();
                                var remainingNum = competitiontypeTotal - competitondaytype;
                                if(competitiontypeTotal==0)
                                {
                                    remainingNum=0;
                                }

                                remainingNum=competitiontypeTotal-competitondaytype;
                                tempdate.remainingNumber=remainingNum;
                                specilTypelist=competitionDayDB
                                    .Where(x => x.CompetitionId==competitionId&&x.TypeId==item).Select(com => new FilterTabledDto
                                    {
                                        CompetitionDayId=com.Id ,
                                        TypeId=item ,
                                        Name=com.Name??"" ,
                                        From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
                                        To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
                                        lecturerName=
                                            (com.Lecturer?.FirstName+" "+com.Lecturer?.MiddleName)??
                                            null ,
                                        levelName=(assignedSubject.Academiclevel?.Name)??null ,
                                        ProgramName=(assignedSubject.Programm?.Name)??null ,
                                        hallName=hallsDB.Where(x => x.Id==com.HallId).Select(y => y.Name).FirstOrDefault() ,
                                        hallid=com.HallId ,
                                        FromScore=com.FromScore ,

                                    }).ToList();
                                tempdate.specialtypeList=specilTypelist;
                                AllTempdate.Add(tempdate);
                            }
                        }
                        else
                        {
                            var alltypeIds = _unitOfWork.Types.FindAll(x => x.Id > 0).Select(y => y.Id).ToList();
                            foreach(var item in alltypeIds)
                            {
                                var tempdate = new TypeList();
                                tempdate.TypeId=item;
                                tempdate.Qty=0;
                                tempdate.TotalScore=0;
                                tempdate.percentage=0;
                                tempdate.remainingNumber=0;
                                tempdate.TotalScoreForStudent=null;
                                specilTypelist=competitionDayDB
                                    .Where(x => x.CompetitionId==competitionId&&x.TypeId==typeId).Select(com => new FilterTabledDto
                                    {
                                        CompetitionDayId=com.Id ,
                                        Name=com.Name ,
                                        From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
                                        To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
                                        lecturerName=
                                            (com.Lecturer?.FirstName+" "+com.Lecturer?.MiddleName)??
                                            null ,
                                        levelName=assignedSubject.Academiclevel?.Name ,
                                        ProgramName=assignedSubject.Programm?.Name??null ,
                                        hallName=hallsDB.Where(x => x.Id==com.HallId).Select(y => y.Name).FirstOrDefault() ,
                                        hallid=com.HallId ,
                                        FromScore=com.FromScore ,
                                    }).ToList();
                                tempdate.specialtypeList=specilTypelist;
                                AllTempdate.Add(tempdate);
                            }
                        }

                        //competitiondto.typeLists.specialtypeList = specilTypelist;
                        competitiondto.typeLists=AllTempdate;
                        Response.Result=true;
                        Response.Data=competitiondto;
                    }

                    else
                    {
                        var AllTempdate = new List<TypeList>();
                        var specilTypelist = new List<FilterTabledDto>();
                        //var ALLspecialTypelist  = new List<FilterTabledDto>();
                        var competition = _unitOfWork.Competitions.FindAll(x => x.Id == competitionId).FirstOrDefault();
                        competitiondto.CompetitionId=competitionId;
                        competitiondto.ImagePath=competition.ImagePath!=null ? BaseURL+competition.ImagePath : null;
                        competitiondto.Name=competition.Name;
                        competitiondto.Active=competition.Active;
                        competitiondto.Accreditedhours=competition.Accreditedhours;
                        competitiondto.StudyingHours=competition.StudyingHours;
                        competitiondto.SubjectScore=competition.SubjectScore;
                        competitiondto.NumbersOfStudents=_unitOfWork.CompetitionUsers
                            .FindAll(x => x.CompetitionId==competitionId).Count();
                        competitiondto.NumbersOfAdmin=_unitOfWork.CompetitionMemberAdmins
                             .FindAll(x => x.CompetitionId==competitionId&&(x.RoleName.Contains("adminCompetition")||x.RoleName.Contains("management"))).Count();
                        competitiondto.NumbersOfDoctor=_unitOfWork.CompetitionMemberAdmins
                            .FindAll(x => x.CompetitionId==competitionId&&(x.RoleName.Contains("doctor"))).Count();
                        //var TotalScoreStudent = _unitOfWork.CompetitionUsers.FindAll(x => x.ApplicationUserId == userId).Select(x => x.TotalScore).FirstOrDefault();
                        competitiondto.TotalScoreStudent=null;
                        competitiondto.SpecialDept=assignedSubject.SpecialdeptId;
                        competitiondto.LevelId=assignedSubject.AcademiclevelId;
                        competitiondto.LevelName=assignedSubject.Academiclevel?.Name;
                        competitiondto.ProgramName=assignedSubject.Programm?.Name;
                        competitiondto.AcademicYearName=assignedSubject.AcademicYear?.Term;


                        competitiondto.NumbersOfRequestDelay=competitionsUsersDB.Where(x => x.DelayRequestStatus=="DelayRequest"&&x.DelayRequestStatus!="rejectDelay"&&x.DelayOrWithdrawalStatus!="delay"&&x.CompetitionId==competitionId).Count()!=0 ? (competitionsUsersDB.Where(x => x.DelayRequestStatus=="DelayRequest"&&x.DelayRequestStatus!="rejectDelay"&&x.DelayOrWithdrawalStatus!="delay"&&x.CompetitionId==competitionId).Count()) : null;
                        competitiondto.NumbersOfRequestWithdraw=competitionsUsersDB.Where(x => x.WithdrawalRequestStatus=="WithdrawRequest"&&x.WithdrawalRequestStatus!="rejectWithdraw"&&x.DelayOrWithdrawalStatus!="Withdraw"&&x.CompetitionId==competitionId).Count()!=0 ? (competitionsUsersDB.Where(x => x.WithdrawalRequestStatus=="WithdrawRequest"&&x.WithdrawalRequestStatus!="rejectWithdraw"&&x.DelayOrWithdrawalStatus!="delay"&&x.CompetitionId==competitionId).Count()) : null;
                        competitiondto.NumbersOfRejectDelay=competitionsUsersDB.Where(x => x.DelayRequestStatus=="rejectDelay"&&x.CompetitionId==competitionId).Count()!=0 ? (competitionsUsersDB.Where(x => x.DelayRequestStatus=="rejectDelay"&&x.CompetitionId==competitionId).Count()) : null;
                        competitiondto.NumbersOfRejectWithdraw=competitionsUsersDB.Where(x => x.WithdrawalRequestStatus=="rejectWithdraw"&&x.CompetitionId==competitionId).Count()!=0 ? (competitionsUsersDB.Where(x => x.WithdrawalRequestStatus=="rejectWithdraw"&&x.CompetitionId==competitionId).Count()) : null;
                        competitiondto.NumbersOfPending=competitionsUsersDB.Where(x => x.EnrollmentStatus=="pending"&&x.CompetitionId==competitionId).Count()!=0 ? (competitionsUsersDB.Where(x => x.EnrollmentStatus=="pending"&&x.CompetitionId==competitionId).Count()) : null;
                        //competitiondto.ProgramName=assignedSubject.Programm?.Name??null;
                        //competitiondto.LectureName=competition.ApplicationUser.FirstName+" "+competition.ApplicationUser.MiddleName;
                        var TypeIds = _unitOfWork.CompetitionTypes
                   .FindAll(x => x.CompetitionId == competitionId, new[] { "Type" }).Select(y => y.TypeId)
                   .ToList();
                        if(TypeIds.Count()>0)
                        {
                            var competitiontype = _unitOfWork.CompetitionTypes
                       .FindAll(x => x.CompetitionId == competitionId).ToList();
                            var TypeDB = _unitOfWork.Types.FindAll(x => x.Id > 0).ToList();
                            foreach(var item in TypeIds)
                            {
                                var tempdate = new TypeList();
                                tempdate.TypeId=item;
                                tempdate.TypeName=TypeDB.Where(x => x.Id==item).Select(y => y.Name).FirstOrDefault()??
                                                    "";
                                tempdate.Qty=competitiontype.Where(x => x.TypeId==item).Select(y => y.Qty)
                                    .FirstOrDefault();
                                tempdate.TotalScore=competitiontype.Where(x => x.TypeId==item).Select(y => y.TotalScore)
                                    .FirstOrDefault();
                                tempdate.TotalScoreForStudent=null;
                                tempdate.percentage=competitiontype.Where(x => x.TypeId==item).Select(y => y.Percentage)
                                    .FirstOrDefault();
                                tempdate.AllowAudience=competitiontype.Where(x => x.TypeId==item)
                                    .Select(y => y.AllowAudience).FirstOrDefault();
                                var competitondaytype = _unitOfWork.CompetitionDays
                           .FindAll(x => x.CompetitionId == competition.Id && x.TypeId == item).Count();
                                var competitondayIds = _unitOfWork.CompetitionDays
                           .FindAll(x => x.CompetitionId == competition.Id && x.TypeId == item).Select(y => y.Id)
                           .ToList();
                                foreach(var itemId in competitondayIds)
                                {
                                    //var scoreforstudent = _unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId == itemId && x.ApplicationUserId == userId).Select(y => y.UserScore).FirstOrDefault() ?? 0;
                                    tempdate.TotalScoreForStudent=null;
                                }

                                var competitiontypeTotal = _unitOfWork.CompetitionTypes
                           .FindAll(x => x.CompetitionId == competition.Id && x.TypeId == item).Select(y => y.Qty)
                           .FirstOrDefault();
                                var remainingNum = competitiontypeTotal - competitondaytype;
                                if(competitiontypeTotal==0)
                                {
                                    remainingNum=0;
                                }

                                remainingNum=competitiontypeTotal-competitondaytype;
                                tempdate.remainingNumber=remainingNum;
                                specilTypelist=competitionDayDB
                                    .Where(x => x.CompetitionId==competitionId&&x.TypeId==item).Select(com => new FilterTabledDto
                                    {
                                        CompetitionDayId=com.Id ,
                                        TypeId=item ,
                                        Name=com.Name??"" ,
                                        From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
                                        To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
                                        lecturerName=
                                            (com.Lecturer?.FirstName+" "+com.Lecturer?.MiddleName)??
                                            null ,
                                        levelName=(assignedSubject.Academiclevel?.Name)??null ,
                                        ProgramName=(assignedSubject.Programm?.Name)??null ,
                                        hallName=hallsDB.Where(x => x.Id==com.HallId).Select(y => y.Name).FirstOrDefault() ,
                                        hallid=com.HallId ,
                                        FromScore=com.FromScore ,
                                    }).ToList();
                                tempdate.specialtypeList=specilTypelist;
                                AllTempdate.Add(tempdate);
                            }
                        }
                        else
                        {
                            var alltypeIds = _unitOfWork.Types.FindAll(x => x.Id > 0).Select(y => y.Id).ToList();
                            foreach(var item in alltypeIds)
                            {
                                var tempdate = new TypeList();
                                tempdate.TypeId=item;
                                tempdate.Qty=0;
                                tempdate.TotalScore=0;
                                tempdate.percentage=0;
                                tempdate.remainingNumber=0;
                                tempdate.TotalScoreForStudent=null;
                                specilTypelist=competitionDayDB
                                    .Where(x => x.CompetitionId==competitionId&&x.TypeId==typeId).Select(com => new FilterTabledDto
                                    {
                                        CompetitionDayId=com.Id ,
                                        Name=com.Name ,
                                        From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
                                        To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
                                        lecturerName=
                                            com.Lecturer?.FirstName+" "+com.Lecturer?.MiddleName ,
                                        levelName=assignedSubject.Academiclevel?.Name ,
                                        ProgramName=assignedSubject.Programm?.Name??null ,
                                        hallName=hallsDB.Where(x => x.Id==com.HallId).Select(y => y.Name).FirstOrDefault() ,
                                        hallid=com.HallId ,
                                        FromScore=com.FromScore ,
                                    }).ToList();
                                tempdate.specialtypeList=specilTypelist;
                                AllTempdate.Add(tempdate);
                            }
                        }

                        //competitiondto.typeLists.specialtypeList = specilTypelist;
                        competitiondto.typeLists=AllTempdate;
                        Response.Result=true;
                        Response.Data=competitiondto;
                    }

                }

                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }




        //[Authorize]
        [HttpGet("GetSummrySubjects")] //new controll
        public async Task<IActionResult> GetSummrySubjects([FromHeader] int yearId , [FromHeader] int acadmyYearId = 0 ,
            [FromHeader] int deparmentId = 0 , [FromHeader] int LevelId = 0)
        {
            var Response = new BaseResponseWithData<List<Term>>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    if(yearId>0)
                    {
                        var year = _unitOfWork.Years.FindAll(x => x.Id == yearId).FirstOrDefault();
                        if(year==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="لم تضاف  هذة السنة";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }
                    else
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="يجب تحديد السنة الدراسية";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var competitondaytype = _unitOfWork.CompetitionDays.FindAll(x => x.TypeId == 1).ToList();
                    var competitiontype = _unitOfWork.CompetitionTypes.FindAll(x => x.TypeId == 1).ToList();
                    var SubjectList = _unitOfWork.Competitions.FindAll((x => x.Id > 0)).Select(sub => new Sub
                    {
                        Id = sub.Id,
                        Name = sub.Name,
                        Description = sub.Description,
                        Objective = sub.Objective,
                        Active = sub.Active,
                        StudyingHours = sub.StudyingHours,
                        Accreditedhours = sub.Accreditedhours,
                        RequiedofSubject = sub.RequiedofSubject,
                        SubjectScore = sub.SubjectScore,
                        Code = sub.Code,
                        CreationBy = sub.CreationBy,
                        CreationDate = sub.CreationDate,
                        ImagePath = BaseURL + sub.ImagePath,
                        RemineNumberLectures =
                        (competitiontype.Where(x => x.CompetitionId == sub.Id).Select(y => y.Qty)
                            .FirstOrDefault()) - (competitondaytype.Where(x => x.CompetitionId == sub.Id).Count()),
                        TotalNumberLectures =
                        competitiontype.Where(x => x.CompetitionId == sub.Id).Select(y => y.Qty).FirstOrDefault()
                    }).ToList();
                    var AcademicYearList = new List<AcademicYear>();
                    var DepartmentList = new List<Dept>();
                    var levelList = new List<Academiclevel>();
                    if(acadmyYearId>0)
                    {
                        AcademicYearList=_unitOfWork.AcademicYears
                            .FindAll(x => x.YearId==yearId&&x.Id==acadmyYearId).ToList();
                        if(AcademicYearList==null)
                            Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="لم تضاف ترمات لهذة السنة";
                        Response.Errors.Add(error);

                    }

                    else
                    {
                        AcademicYearList=_unitOfWork.AcademicYears
                            .FindAll(x => x.YearId==yearId&&x.From<=DateTime.Now&&DateTime.Now<=x.To).ToList();
                        if(AcademicYearList.Count()==0)
                        {
                            var tempdate = _unitOfWork.AcademicYears
                            .FindAll(x => x.YearId == yearId && x.From >= DateTime.Now).OrderBy(d => d.From)
                            .FirstOrDefault();
                            if(tempdate!=null)
                                AcademicYearList.Add(tempdate);
                        }

                        if(AcademicYearList.Count==0)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="لم تضاف ترمات لهذة السنة";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }

                    if(deparmentId>0)
                    {
                        DepartmentList=_unitOfWork.Depts.FindAll(d => d.Id==deparmentId , new [] { "specialdepts" })
                            .ToList();
                    }
                    else
                    {
                        DepartmentList=_unitOfWork.Depts.FindAll(d => d.Id>0 , new [] { "specialdepts" }).ToList();
                    }

                    var SpecialdeptsList = _unitOfWork.Specialdepts.FindAll(d => d.Id > 0).ToList();
                    if(LevelId>0)
                    {
                        levelList=_unitOfWork.Academiclevels.FindAll((x => x.Id==LevelId)).ToList();
                    }
                    else
                    {
                        levelList=_unitOfWork.Academiclevels.FindAll((x => x.Id>0)).ToList();
                    }

                    // var competitionIds = _unitOfWork.AssignedSubjects.FindAll(x => x.AcademicYearId == acadmyYearId).Select(y => y.CompetitionId).ToList();
                    var summury = AcademicYearList.Select(yea => new Term
                    {
                        TermId = yea.Id,
                        Termname =
                        _unitOfWork.AcademicYears.FindAll(x => x.Id == yea.Id).Select(c => c.Term).FirstOrDefault(),
                        from = _unitOfWork.AcademicYears.FindAll(x => x.Id == yea.Id).Select(c => c.From).FirstOrDefault(),
                        to = _unitOfWork.AcademicYears.FindAll(x => x.Id == yea.Id).Select(c => c.To).FirstOrDefault(),
                        levels = levelList.Select(lev => new Leveling
                        {
                            levelId = lev.Id,
                            levelname = lev.Name,
                            depart = DepartmentList.Select(d => new Depart
                            {
                                deptId = d.Id,
                                departname = d.Name,
                                Spical = SpecialdeptsList.Where(sp => sp.DeptartmentId == d.Id).Select(sp => new Spical
                                {
                                    spicalId = sp.Id,
                                    spicalname = sp.Name,
                                    sub = SubjectList.Where(sub =>
                                        _unitOfWork.AssignedSubjects
                                            .FindAll(x =>
                                                x.AcademicYearId == yea.Id && x.SpecialdeptId == sp.Id &&
                                                x.AcademiclevelId == lev.Id).Select(x => x.CompetitionId).ToList()
                                            .Contains(sub.Id))
                                    //sub = SubjectList.Where(sub => sub.SpecialdeptId == sp.Id && sub.AcademiclevelId == lev.Id && sub.AcademicYearId == acadmyYearId)
                                    .ToList()
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList();

                    Response.Data=summury;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        //[Authorize]
        [HttpGet("GetSummrySubjectstest")] //new controll
        public async Task<IActionResult> GetSummrySubjectstest([FromHeader] int yearId ,
            [FromHeader] int acadmyYearId = 0 , [FromHeader] int deparmentId = 0 , [FromHeader] int LevelId = 0 ,
            [FromHeader] int ProgramId = 0)
        {
            var Response = new BaseResponseWithData<List<Term>>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    if(yearId>0)
                    {
                        var year = _unitOfWork.Years.FindAll(x => x.Id == yearId).FirstOrDefault();
                        if(year==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="لم تضاف  هذة السنة";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }
                    else
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="لم تضاف  هذة السنة";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var competitondaytype = _unitOfWork.CompetitionDays.FindAll(x => x.TypeId == 1).ToList();
                    var competitiontype = _unitOfWork.CompetitionTypes.FindAll(x => x.TypeId == 1).ToList();
                    var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll((x => x.Id > 0));
                    var SubjectList = _unitOfWork.Competitions.FindAll((x => x.Id > 0)).Select(sub => new Sub
                    {
                        Id = sub.Id,
                        Name = sub.Name,
                        Description = sub.Description,
                        Objective = sub.Objective,
                        Active = sub.Active,
                        StudyingHours = sub.StudyingHours,
                        Accreditedhours = sub.Accreditedhours,
                        RequiedofSubject = sub.RequiedofSubject,
                        SubjectScore = sub.SubjectScore,
                        Code = sub.Code,
                        CreationBy = sub.CreationBy,
                        CreationDate = sub.CreationDate,
                        ImagePath = sub.ImagePath != null ? BaseURL + sub.ImagePath : null,
                        RemineNumberLectures =
                        (competitiontype.Where(x => x.CompetitionId == sub.Id).Select(y => y.Qty)
                            .FirstOrDefault()) - (competitondaytype.Where(x => x.CompetitionId == sub.Id).Count()),
                        TotalNumberLectures =
                        competitiontype.Where(x => x.CompetitionId == sub.Id).Select(y => y.Qty).FirstOrDefault()
                    }).ToList();
                    var AcademicYearList = new List<AcademicYear>();
                    var DepartmentList = new List<Dept>();
                    var levelList = new List<Academiclevel>();
                    if(acadmyYearId>0)
                    {
                        AcademicYearList=_unitOfWork.AcademicYears
                            .FindAll(x => x.YearId==yearId&&x.Id==acadmyYearId).ToList();
                        if(AcademicYearList==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="لم تضاف ترمات لهذة السنة";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }
                    else
                    {
                        AcademicYearList=_unitOfWork.AcademicYears
                            .FindAll(x => x.YearId==yearId&&x.From<=DateTime.Now&&DateTime.Now<=x.To).ToList();
                        if(AcademicYearList.Count()==0)
                        {
                            var tempdate = _unitOfWork.AcademicYears
                            .FindAll(x => x.YearId == yearId && x.From >= DateTime.Now).OrderBy(d => d.From)
                            .FirstOrDefault();
                            if(tempdate!=null)
                                AcademicYearList.Add(tempdate);
                        }

                        if(AcademicYearList.Count==0)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="لم تضاف ترمات لهذة السنة";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }

                    //var SPecialListIDS = _unitOfWork.sp.FindAll(x=>x.)
                    //if (true)
                    //{

                    //}
                    if(deparmentId>0)
                    {
                        DepartmentList=_unitOfWork.Depts.FindAll(d => d.Id==deparmentId , new [] { "specialdepts" })
                            .ToList();
                    }
                    else
                    {
                        DepartmentList=_unitOfWork.Depts.FindAll(d => d.Id>0 , new [] { "specialdepts" }).ToList();
                    }

                    var SpecialdeptsList = _unitOfWork.Specialdepts.FindAll(d => d.Id > 0).ToList();
                    if(LevelId>0)
                    {
                        levelList=_unitOfWork.Academiclevels.FindAll(x => x.Id==LevelId , new [] { "Programm" }).ToList();
                    }
                    else
                    {
                        levelList=_unitOfWork.Academiclevels.FindAll(x => x.Id>0 , new [] { "Programm" }).ToList();
                    }

                    if(ProgramId>0)
                    {
                        levelList=_unitOfWork.Academiclevels.FindAll((x => x.ProgramId==ProgramId)).ToList();
                    }

                    var summury = AcademicYearList.Select(yea => new Term
                    {
                        TermId = yea.Id,
                        Termname =
                        _unitOfWork.AcademicYears.FindAll(x => x.Id == yea.Id).Select(c => c.Term).FirstOrDefault(),
                        from =
                        _unitOfWork.AcademicYears.FindAll(x => x.Id == yea.Id).Select(c => c.From).FirstOrDefault(),
                        to = _unitOfWork.AcademicYears.FindAll(x => x.Id == yea.Id).Select(c => c.To).FirstOrDefault(),
                        levels = levelList.Select(lev => new Leveling
                        {
                            levelId = lev.Id,
                            levelname = lev.Name + " " + lev.Program?.Name,
                            programmId = lev.ProgramId,
                            depart = DepartmentList
                            .Where(dep => SubjectList.Where(sub =>
                                AssignSubjectList.Where(x =>
                                        x.AcademicYearId == yea.Id &&
                                        SpecialdeptsList.Where(sp => sp.DeptartmentId == dep.Id).Select(y => y.Id)
                                            .ToList().Contains(x.SpecialdeptId) && x.AcademiclevelId == lev.Id)
                                    .Select(x => x.CompetitionId).Contains(sub.Id)).Count() > 0).Select(d =>
                                new Depart
                                {
                                    deptId = d.Id,
                                    departname = d.Name,
                                    Spical = SpecialdeptsList.Where(sp => sp.DeptartmentId == d.Id).Select(
                                        sp => new Spical
                                        {
                                            spicalId = sp.Id,
                                            spicalname = sp.Name,
                                            sub = SubjectList.Where(sub =>
                                                    AssignSubjectList
                                                        .Where(x => x.AcademicYearId == yea.Id &&
                                                                    x.SpecialdeptId == sp.Id &&
                                                                    x.AcademiclevelId == lev.Id)
                                                        .Select(x => x.CompetitionId).Contains(sub.Id))
                                                .ToList().ToList()
                                        }).ToList()
                                }).ToList()
                        }).ToList()
                    }).ToList();
                    Response.Data=summury;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }



        [Authorize]
        [HttpGet("GetCompetitionsForUser")] //new
        public async Task<IActionResult> GetCompetitionsForUser([FromHeader] long userId , [FromHeader] int YearId , [FromHeader] int TermId = 0)
        {
            BaseResponseWithData<List<CompetitinForUserAndAdmin>> Response =
                new BaseResponseWithData<List<CompetitinForUserAndAdmin>>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            if(YearId==0)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG="يجب ادخال السنة الدراسية ";
                Response.Errors.Add(error);
                return BadRequest(Response);

            }

            var competitionDb = new List<Competition>();
            var competitionListbyYear = new List<Competition>();
            var data = new List<CompetitinForUserAndAdmin>();
            try
            {
                var _user =  _unitOfWork.HrUsers.FindAll(x=>x.Id == userId).FirstOrDefault();
                if(_user==null)
                {
                    Response.Result=false;
                    Error error = new Error();
                    error.ErrorCode="Err10";
                    error.ErrorMSG="هذا المستخدم غير موجود";
                    Response.Errors.Add(error);
                    return BadRequest(Response);

                }

                var competitondaytype = new int();
                var competitiontypeTotal = new int();
                var remainingNumber = new int();
                var TotalStudents = new int();
                bool userIsAdmin = false;
                var tempdata = new List<CompetitinForUserAndAdmin>();
                var competitionMemberAdmin = _unitOfWork.CompetitionMemberAdmins
                    .FindAll(x => x.HrUserId == userId, new[] { "Competition" }).Select(y => y.Competition)
                    .ToList();
                var acadmyYearIds = _unitOfWork.AcademicYears.FindAll(x => x.YearId == YearId).Select(y => y.Id)
                    .ToList();
                var assignSubjectDB = _unitOfWork.AssignedSubjects.FindAll(x => x.Id > 0,
                    new[] { "Academiclevel", "Specialdept", "AcademicYear", "Programm" });
                var DoctorCompetitionDB = _unitOfWork.CompetitionMemberAdmins.FindAll(
                    x => (x.RoleName == "doctor" || x.RoleName == "assistant") , new[] { "HrUser" });
                var UserRoles = _unitOfWork.UserRoles.FindAllQueryable(x=>x.UserId == _user.UserId ,new []{"Role"}).Select( n=>n.Role.Name);
                if(UserRoles.Any()&&UserRoles.Contains("admin"))
                {
                    userIsAdmin=true;
                }


                ///userIsAdmin=await _userManager.IsInRoleAsync(_user , "admin");

                if(userIsAdmin)
                {
                    if(TermId==0)
                    {
                        foreach(var item in acadmyYearIds)
                        {
                            var competitionbyYear = _unitOfWork.AssignedSubjects
                                .FindAll(x => x.AcademicYearId == item, new[] { "Competition" })
                                .Select(y => y.Competition).ToList();
                            if(competitionbyYear.Count()>0)
                            {
                                competitionListbyYear.AddRange(competitionbyYear);
                            }
                        }

                        competitionDb=competitionListbyYear;
                        if(competitionDb.Count()>0)
                        {
                            foreach(var competition in competitionDb)
                            {
                                var tempdata2 = new CompetitinForUserAndAdmin();
                                tempdata2.doctorslistDtos=new List<DoctorslistDto>();
                                competitondaytype=_unitOfWork.CompetitionDays
                                    .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Count();
                                competitiontypeTotal=_unitOfWork.CompetitionTypes
                                    .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Select(y => y.Qty)
                                    .FirstOrDefault();
                                remainingNumber=competitiontypeTotal-competitondaytype;
                                if(competitiontypeTotal==0)
                                {
                                    remainingNumber=0;
                                }

                                remainingNumber=competitiontypeTotal-competitondaytype;
                                tempdata2.remainingNumber=remainingNumber;
                                tempdata2.Id=competition.Id;
                                tempdata2.Name=competition.Name;
                                tempdata2.Accreditedhours=competition.Accreditedhours;
                                tempdata2.Active=competition.Active;
                                tempdata2.StudyingHours=competition.StudyingHours;
                                tempdata2.SubjectScore=competition.SubjectScore;
                                tempdata2.Capacity=competition.Capacity;
                                tempdata2.ImagePath=competition.ImagePath!=null
                                    ? BaseURL+competition.ImagePath
                                    : null;
                                tempdata2.LevelName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.Academiclevel?.Name).FirstOrDefault();
                                tempdata2.LevelId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.AcademiclevelId).FirstOrDefault();
                                tempdata2.programName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.Programm?.Name).FirstOrDefault()??null;
                                tempdata2.programId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.ProgrammId).FirstOrDefault()??null;
                                tempdata2.SpecialdeptId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.SpecialdeptId).FirstOrDefault();
                                tempdata2.SpecialdeptName=assignSubjectDB
                                    .Where(x => x.CompetitionId==competition.Id).Select(y => y.Specialdept?.Name)
                                    .FirstOrDefault();
                                tempdata2.deptName=_unitOfWork.Specialdepts
                                    .FindAll(x => x.Id==tempdata2.SpecialdeptId , new [] { "dept" })
                                    .Select(y => y.Deptartment.Name).FirstOrDefault();
                                tempdata2.AcademicYearId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.AcademicYearId).FirstOrDefault();
                                tempdata2.AcademicYearName=assignSubjectDB
                                    .Where(x => x.CompetitionId==competition.Id).Select(y => y.AcademicYear?.Term)
                                    .FirstOrDefault();
                                var DoctorCompetition =
                                    DoctorCompetitionDB.Where(x => x.CompetitionId == competition.Id);
                                if(DoctorCompetition.Count()>0)
                                {
                                    foreach(var item in DoctorCompetition)
                                    {
                                        var tempdatadto = new DoctorslistDto();
                                        tempdatadto.DoctorId=(long)item.HrUserId;
                                        tempdatadto.DoctorName=
                                            DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                                .Select(y =>
                                                    y.HrUser.FirstName+" "+y.HrUser.MiddleName)
                                                .FirstOrDefault()??null;
                                        tempdatadto.Image=
                                            (DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                                .Select(y => y.HrUser.ImgPath).FirstOrDefault())!=null
                                                ? (BaseURL+DoctorCompetition
                                                    .Where(x => (x.RoleName=="doctor"||x.RoleName=="assistant")&&
                                                                x.HrUserId==item.HrUserId)
                                                    .Select(y => y.HrUser.ImgPath).FirstOrDefault())
                                                : null;
                                        tempdata2.doctorslistDtos.Add(tempdatadto);
                                    }
                                }

                                tempdata2.Status=competition.Status;
                                tempdata2.TotalStudents=_unitOfWork.CompetitionUsers
                                    .FindAll(x => x.CompetitionId==competition.Id).Count();
                                tempdata.Add(tempdata2);
                            }

                            data=tempdata;
                        }
                    }
                    else
                    {
                        var competitionTem = _unitOfWork.AssignedSubjects
                            .FindAll(x => x.AcademicYearId == TermId, new[] { "Competition" })
                            .Select(y => y.Competition).ToList();
                        var netcompetition = competitionTem;
                        foreach(var competition in netcompetition)
                        {
                            var tempdata2 = new CompetitinForUserAndAdmin();
                            tempdata2.doctorslistDtos=new List<DoctorslistDto>();
                            competitondaytype=_unitOfWork.CompetitionDays
                                .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Count();
                            competitiontypeTotal=_unitOfWork.CompetitionTypes
                                .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Select(y => y.Qty)
                                .FirstOrDefault();
                            if(competitiontypeTotal==0)
                            {
                                remainingNumber=0;
                            }

                            remainingNumber=competitiontypeTotal-competitondaytype;
                            tempdata2.remainingNumber=remainingNumber;
                            tempdata2.Id=competition.Id;
                            tempdata2.Name=competition.Name;
                            tempdata2.Accreditedhours=competition.Accreditedhours;
                            tempdata2.Active=competition.Active;
                            tempdata2.StudyingHours=competition.StudyingHours;
                            tempdata2.SubjectScore=competition.SubjectScore;
                            tempdata2.Capacity=competition.Capacity;
                            tempdata2.ImagePath=
                                competition.ImagePath!=null ? BaseURL+competition.ImagePath : null;
                            tempdata2.LevelName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.Academiclevel?.Name).FirstOrDefault();
                            tempdata2.LevelId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.AcademiclevelId).FirstOrDefault();
                            tempdata2.programName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.Programm?.Name).FirstOrDefault()??null;
                            tempdata2.programId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.ProgrammId).FirstOrDefault()??null;
                            tempdata2.SpecialdeptId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.SpecialdeptId).FirstOrDefault();
                            tempdata2.SpecialdeptName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.Specialdept?.Name).FirstOrDefault();
                            tempdata2.deptName=_unitOfWork.Specialdepts
                                .FindAll(x => x.Id==tempdata2.SpecialdeptId , new [] { "dept" })
                                .Select(y => y.Deptartment.Name).FirstOrDefault();
                            tempdata2.AcademicYearId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.AcademicYearId).FirstOrDefault();
                            tempdata2.AcademicYearName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.AcademicYear?.Term).FirstOrDefault();
                            var DoctorCompetition = DoctorCompetitionDB.Where(x => x.CompetitionId == competition.Id);
                            if(DoctorCompetition.Count()>0)
                            {
                                foreach(var item in DoctorCompetition)
                                {
                                    var tempdatadto = new DoctorslistDto();
                                    tempdatadto.DoctorId=item.HrUserId;
                                    tempdatadto.DoctorName=
                                        DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                            .Select(y =>
                                                y.HrUser.FirstName+" "+y.HrUser.MiddleName)
                                            .FirstOrDefault()??null;
                                    tempdatadto.Image=
                                        (DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                            .Select(y => y.HrUser.ImgPath).FirstOrDefault())!=null
                                            ? (BaseURL+DoctorCompetition
                                                .Where(x => (x.RoleName=="doctor"||x.RoleName=="assistant")&&
                                                            x.HrUserId==item.HrUserId)
                                                .Select(y => y.HrUser.ImgPath).FirstOrDefault())
                                            : null;
                                    tempdata2.doctorslistDtos.Add(tempdatadto);
                                }
                            }

                            tempdata2.Status=competition.Status;
                            tempdata2.TotalStudents=_unitOfWork.CompetitionUsers
                                .FindAll(x => x.CompetitionId==competition.Id).Count();
                            tempdata.Add(tempdata2);
                        }

                        data=tempdata;
                    }
                }
                else if(competitionMemberAdmin.Count()>0)
                {
                    if(TermId==0)
                    {
                        foreach(var item in acadmyYearIds)
                        {
                            var competitionbyYear = _unitOfWork.AssignedSubjects
                                .FindAll(x => x.AcademicYearId == item, new[] { "Competition" })
                                .Select(y => y.Competition).ToList();
                            if(competitionbyYear.Count()>0)
                            {
                                competitionListbyYear.AddRange(competitionbyYear);
                            }
                        }

                        competitionDb=competitionListbyYear.Intersect(competitionMemberAdmin).ToList();
                        if(competitionDb.Count()>0)
                        {
                            foreach(var competition in competitionDb)
                            {
                                var tempdata2 = new CompetitinForUserAndAdmin();
                                tempdata2.doctorslistDtos=new List<DoctorslistDto>();
                                competitondaytype=_unitOfWork.CompetitionDays
                                    .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Count();
                                competitiontypeTotal=_unitOfWork.CompetitionTypes
                                    .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Select(y => y.Qty)
                                    .FirstOrDefault();
                                remainingNumber=competitiontypeTotal-competitondaytype;
                                if(competitiontypeTotal==0)
                                {
                                    remainingNumber=0;
                                }

                                remainingNumber=competitiontypeTotal-competitondaytype;
                                tempdata2.remainingNumber=remainingNumber;
                                tempdata2.Id=competition.Id;
                                tempdata2.Name=competition.Name;
                                tempdata2.Accreditedhours=competition.Accreditedhours;
                                tempdata2.Active=competition.Active;
                                tempdata2.StudyingHours=competition.StudyingHours;
                                tempdata2.SubjectScore=competition.SubjectScore;
                                tempdata2.Capacity=competition.Capacity;
                                tempdata2.ImagePath=competition.ImagePath!=null
                                    ? BaseURL+competition.ImagePath
                                    : null;
                                tempdata2.LevelName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.Academiclevel?.Name).FirstOrDefault();
                                tempdata2.LevelId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.AcademiclevelId).FirstOrDefault();
                                tempdata2.programName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.Programm?.Name).FirstOrDefault()??null;
                                tempdata2.programId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.ProgrammId).FirstOrDefault()??null;
                                tempdata2.SpecialdeptId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.SpecialdeptId).FirstOrDefault();
                                tempdata2.SpecialdeptName=assignSubjectDB
                                    .Where(x => x.CompetitionId==competition.Id).Select(y => y.Specialdept?.Name)
                                    .FirstOrDefault();
                                tempdata2.deptName=_unitOfWork.Specialdepts
                                    .FindAll(x => x.Id==tempdata2.SpecialdeptId , new [] { "dept" })
                                    .Select(y => y.Deptartment.Name).FirstOrDefault();
                                tempdata2.AcademicYearId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.AcademicYearId).FirstOrDefault();
                                tempdata2.AcademicYearName=assignSubjectDB
                                    .Where(x => x.CompetitionId==competition.Id).Select(y => y.AcademicYear?.Term)
                                    .FirstOrDefault();
                                var DoctorCompetition =
                                    DoctorCompetitionDB.Where(x => x.CompetitionId == competition.Id);
                                if(DoctorCompetition.Count()>0)
                                {
                                    foreach(var item in DoctorCompetition)
                                    {
                                        var tempdatadto = new DoctorslistDto();
                                        tempdatadto.DoctorId=item.HrUserId;
                                        tempdatadto.DoctorName=
                                            DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                                .Select(y =>
                                                    y.HrUser.FirstName+" "+y.HrUser.MiddleName)
                                                .FirstOrDefault()??null;
                                        tempdatadto.Image=
                                            (DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                                .Select(y => y.HrUser.ImgPath).FirstOrDefault())!=null
                                                ? (BaseURL+DoctorCompetition
                                                    .Where(x => (x.RoleName=="doctor"||x.RoleName=="assistant")&&
                                                                x.HrUserId==item.HrUserId)
                                                    .Select(y => y.HrUser.ImgPath).FirstOrDefault())
                                                : null;
                                        tempdata2.doctorslistDtos.Add(tempdatadto);
                                    }
                                }

                                tempdata2.Status=competition.Status;
                                tempdata2.TotalStudents=_unitOfWork.CompetitionUsers
                                    .FindAll(x => x.CompetitionId==competition.Id).Count();
                                tempdata.Add(tempdata2);
                            }

                            data=tempdata;
                        }
                    }
                    else
                    {
                        var competitionTem = _unitOfWork.AssignedSubjects
                            .FindAll(x => x.AcademicYearId == TermId, new[] { "Competition" })
                            .Select(y => y.Competition).ToList();
                        var netcompetition = competitionTem.Intersect(competitionMemberAdmin);
                        foreach(var competition in netcompetition)
                        {
                            var tempdata2 = new CompetitinForUserAndAdmin();
                            tempdata2.doctorslistDtos=new List<DoctorslistDto>();
                            competitondaytype=_unitOfWork.CompetitionDays
                                .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Count();
                            competitiontypeTotal=_unitOfWork.CompetitionTypes
                                .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Select(y => y.Qty)
                                .FirstOrDefault();
                            if(competitiontypeTotal==0)
                            {
                                remainingNumber=0;
                            }

                            remainingNumber=competitiontypeTotal-competitondaytype;
                            tempdata2.remainingNumber=remainingNumber;
                            tempdata2.Id=competition.Id;
                            tempdata2.Name=competition.Name;
                            tempdata2.Accreditedhours=competition.Accreditedhours;
                            tempdata2.Active=competition.Active;
                            tempdata2.StudyingHours=competition.StudyingHours;
                            tempdata2.SubjectScore=competition.SubjectScore;
                            tempdata2.Capacity=competition.Capacity;
                            tempdata2.ImagePath=
                                competition.ImagePath!=null ? BaseURL+competition.ImagePath : null;
                            tempdata2.LevelName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.Academiclevel?.Name).FirstOrDefault();
                            tempdata2.LevelId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.AcademiclevelId).FirstOrDefault();
                            tempdata2.programName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.Programm?.Name).FirstOrDefault()??null;
                            tempdata2.programId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.ProgrammId).FirstOrDefault()??null;
                            tempdata2.SpecialdeptId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.SpecialdeptId).FirstOrDefault();
                            tempdata2.SpecialdeptName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.Specialdept?.Name).FirstOrDefault();
                            tempdata2.deptName=_unitOfWork.Specialdepts
                                .FindAll(x => x.Id==tempdata2.SpecialdeptId , new [] { "dept" })
                                .Select(y => y.Deptartment.Name).FirstOrDefault();
                            tempdata2.AcademicYearId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.AcademicYearId).FirstOrDefault();
                            tempdata2.AcademicYearName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                .Select(y => y.AcademicYear?.Term).FirstOrDefault();
                            var DoctorCompetition = DoctorCompetitionDB.Where(x => x.CompetitionId == competition.Id);
                            if(DoctorCompetition.Count()>0)
                            {
                                foreach(var item in DoctorCompetition)
                                {
                                    var tempdatadto = new DoctorslistDto();
                                    tempdatadto.DoctorId=item.HrUserId;
                                    tempdatadto.DoctorName=
                                        DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                            .Select(y =>
                                                y.HrUser.FirstName+" "+y.HrUser.MiddleName)
                                            .FirstOrDefault()??null;
                                    tempdatadto.Image=
                                        (DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                            .Select(y => y.HrUser.ImgPath).FirstOrDefault())!=null
                                            ? (BaseURL+DoctorCompetition
                                                .Where(x => (x.RoleName=="doctor"||x.RoleName=="assistant")&&
                                                            x.HrUserId==item.HrUserId)
                                                .Select(y => y.HrUser.ImgPath).FirstOrDefault())
                                            : null;
                                    tempdata2.doctorslistDtos.Add(tempdatadto);
                                }
                            }

                            tempdata2.Status=competition.Status;
                            tempdata2.TotalStudents=_unitOfWork.CompetitionUsers
                                .FindAll(x => x.CompetitionId==competition.Id).Count();
                            tempdata.Add(tempdata2);
                        }

                        data=tempdata;
                    }
                }
                else
                {
                    if(TermId==0)
                    {
                        var competitionUsers = _unitOfWork.CompetitionUsers
                            .FindAll(x => x.HrUserId == userId, new[] { "Competition" })
                            .Select(y => y.Competition).ToList();
                        if(competitionUsers.Count()>0)
                        {
                            foreach(var item in acadmyYearIds)
                            {
                                //var tempdata2 = new CompetitinForUserAndAdmin();
                                var competitionByYear = _unitOfWork.AssignedSubjects
                                    .FindAll((x => x.AcademicYearId == item), new[] { "Competition" })
                                    .Select(y => y.Competition).ToList();
                                competitionDb=competitionUsers.Intersect(competitionByYear).ToList();
                                if(competitionDb.Count>0)
                                {
                                    foreach(var competition in competitionDb)
                                    {
                                        var tempdata2 = new CompetitinForUserAndAdmin();
                                        tempdata2.doctorslistDtos=new List<DoctorslistDto>();
                                        competitondaytype=_unitOfWork.CompetitionDays
                                            .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Count();
                                        competitiontypeTotal=_unitOfWork.CompetitionTypes
                                            .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1)
                                            .Select(y => y.Qty).FirstOrDefault();
                                        remainingNumber=competitiontypeTotal-competitondaytype;
                                        if(competitiontypeTotal==0)
                                        {
                                            remainingNumber=0;
                                        }

                                        remainingNumber=competitiontypeTotal-competitondaytype;
                                        tempdata2.remainingNumber=remainingNumber;
                                        tempdata2.Id=competition.Id;
                                        tempdata2.Name=competition.Name;
                                        tempdata2.Accreditedhours=competition.Accreditedhours;
                                        tempdata2.Active=competition.Active;
                                        tempdata2.StudyingHours=competition.StudyingHours;
                                        tempdata2.SubjectScore=competition.SubjectScore;
                                        tempdata2.Capacity=competition.Capacity;
                                        tempdata2.ImagePath=competition.ImagePath!=null
                                            ? BaseURL+competition.ImagePath
                                            : null;
                                        tempdata2.LevelName=assignSubjectDB
                                            .Where(x => x.CompetitionId==competition.Id)
                                            .Select(y => y.Academiclevel?.Name).FirstOrDefault();
                                        tempdata2.LevelId=assignSubjectDB
                                            .Where(x => x.CompetitionId==competition.Id)
                                            .Select(y => y.AcademiclevelId).FirstOrDefault();
                                        tempdata2.programName=
                                            assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                                .Select(y => y.Programm?.Name).FirstOrDefault()??null;
                                        tempdata2.programId=
                                            assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                                .Select(y => y.ProgrammId).FirstOrDefault()??null;
                                        tempdata2.SpecialdeptId=assignSubjectDB
                                            .Where(x => x.CompetitionId==competition.Id).Select(y => y.SpecialdeptId)
                                            .FirstOrDefault();
                                        tempdata2.SpecialdeptName=assignSubjectDB
                                            .Where(x => x.CompetitionId==competition.Id)
                                            .Select(y => y.Specialdept?.Name).FirstOrDefault();
                                        tempdata2.deptName=_unitOfWork.Specialdepts
                                            .FindAll(x => x.Id==tempdata2.SpecialdeptId , new [] { "dept" })
                                            .Select(y => y.Deptartment.Name).FirstOrDefault();
                                        tempdata2.AcademicYearId=assignSubjectDB
                                            .Where(x => x.CompetitionId==competition.Id).Select(y => y.AcademicYearId)
                                            .FirstOrDefault();
                                        tempdata2.AcademicYearName=assignSubjectDB
                                            .Where(x => x.CompetitionId==competition.Id)
                                            .Select(y => y.AcademicYear?.Term).FirstOrDefault();
                                        var DoctorCompetition =
                                            DoctorCompetitionDB.Where(x => x.CompetitionId == competition.Id);
                                        if(DoctorCompetition.Count()>0)
                                        {
                                            foreach(var item2 in DoctorCompetition)
                                            {
                                                var tempdatadto = new DoctorslistDto();
                                                tempdatadto.DoctorId=item2.HrUserId;
                                                tempdatadto.DoctorName=DoctorCompetition
                                                    .Where(x => x.HrUserId==item2.HrUserId)
                                                    .Select(y =>
                                                        y.HrUser.FirstName+" "+
                                                        y.HrUser.MiddleName).FirstOrDefault()??null;
                                                tempdatadto.Image=
                                                    (DoctorCompetition
                                                        .Where(x => x.HrUserId==item2.HrUserId)
                                                        .Select(y => y.HrUser.ImgPath).FirstOrDefault())!=
                                                    null
                                                        ? (BaseURL+DoctorCompetition
                                                            .Where(x =>
                                                                (x.RoleName=="doctor"||x.RoleName=="assistant")&&
                                                                x.HrUserId==item2.HrUserId)
                                                            .Select(y => y.HrUser.ImgPath).FirstOrDefault())
                                                        : null;

                                                tempdata2.doctorslistDtos.Add(tempdatadto);
                                            }
                                        }

                                        tempdata2.Status=competition.Status;
                                        tempdata2.TotalStudents=_unitOfWork.CompetitionUsers
                                            .FindAll(x => x.CompetitionId==competition.Id).Count();
                                        tempdata.Add(tempdata2);
                                    }
                                }

                                data=tempdata;
                            }
                        }
                        //data = tempdata.Select(sup => new CompetitinForUserAndAdmin
                        //{
                        //    Name = sup.Name,
                        //    Accreditedhours = sup.Accreditedhours,
                        //    StudyingHours = sup.StudyingHours,
                        //    SubjectScore = sup.SubjectScore,
                        //    Active = sup.Active,
                        //    remainingNumber = remainingNumber
                        //}).ToList();
                    }
                    else
                    {
                        var competitionbyYear = _unitOfWork.AssignedSubjects
                            .FindAll(x => x.AcademicYearId == TermId, new[] { "Competition" })
                            .Select(y => y.Competition).ToList();
                        var competitionUsers = _unitOfWork.CompetitionUsers
                            .FindAll(x => x.HrUserId == userId, new[] { "Competition" })
                            .Select(y => y.Competition).ToList();
                        competitionDb=competitionUsers.Intersect(competitionbyYear).ToList();
                        if(competitionDb.Count()>0)
                        {
                            foreach(var competition in competitionDb)
                            {
                                var tempdata2 = new CompetitinForUserAndAdmin();
                                tempdata2.doctorslistDtos=new List<DoctorslistDto>();
                                competitondaytype=_unitOfWork.CompetitionDays
                                    .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Count();
                                competitiontypeTotal=_unitOfWork.CompetitionTypes
                                    .FindAll(x => x.CompetitionId==competition.Id&&x.TypeId==1).Select(y => y.Qty)
                                    .FirstOrDefault();
                                if(competitiontypeTotal==0)
                                {
                                    remainingNumber=0;
                                }

                                remainingNumber=competitiontypeTotal-competitondaytype;
                                tempdata2.remainingNumber=remainingNumber;
                                tempdata2.Id=competition.Id;
                                tempdata2.Name=competition.Name;
                                tempdata2.Accreditedhours=competition.Accreditedhours;
                                tempdata2.Active=competition.Active;
                                tempdata2.StudyingHours=competition.StudyingHours;
                                tempdata2.SubjectScore=competition.SubjectScore;
                                tempdata2.Capacity=competition.Capacity;
                                tempdata2.ImagePath=competition.ImagePath!=null
                                    ? BaseURL+competition.ImagePath
                                    : null;
                                tempdata2.LevelName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.Academiclevel?.Name).FirstOrDefault();
                                tempdata2.LevelId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.AcademiclevelId).FirstOrDefault();
                                tempdata2.programName=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.Programm?.Name).FirstOrDefault()??null;
                                tempdata2.programId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.ProgrammId).FirstOrDefault()??null;
                                tempdata2.SpecialdeptId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.SpecialdeptId).FirstOrDefault();
                                tempdata2.SpecialdeptName=assignSubjectDB
                                    .Where(x => x.CompetitionId==competition.Id).Select(y => y.Specialdept?.Name)
                                    .FirstOrDefault();
                                tempdata2.deptName=_unitOfWork.Specialdepts
                                    .FindAll(x => x.Id==tempdata2.SpecialdeptId , new [] { "dept" })
                                    .Select(y => y.Deptartment.Name).FirstOrDefault();
                                tempdata2.AcademicYearId=assignSubjectDB.Where(x => x.CompetitionId==competition.Id)
                                    .Select(y => y.AcademicYearId).FirstOrDefault();
                                tempdata2.AcademicYearName=assignSubjectDB
                                    .Where(x => x.CompetitionId==competition.Id).Select(y => y.AcademicYear?.Term)
                                    .FirstOrDefault();
                                var DoctorCompetition =
                                    DoctorCompetitionDB.Where(x => x.CompetitionId == competition.Id);
                                if(DoctorCompetition.Count()>0)
                                {
                                    foreach(var item in DoctorCompetition)
                                    {
                                        var tempdatadto = new DoctorslistDto();
                                        tempdatadto.DoctorId=item.HrUserId;
                                        tempdatadto.DoctorName=
                                            DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                                .Select(y =>
                                                    y.HrUser.FirstName+" "+y.HrUser.MiddleName)
                                                .FirstOrDefault()??null;
                                        tempdatadto.Image=
                                            (DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                                .Select(y => y.HrUser.ImgPath).FirstOrDefault())!=null
                                                ? (BaseURL+DoctorCompetition
                                                    .Where(x => (x.RoleName=="doctor"||x.RoleName=="assistant")&&
                                                                x.HrUserId==item.HrUserId)
                                                    .Select(y => y.HrUser.ImgPath).FirstOrDefault())
                                                : null;
                                        tempdata2.doctorslistDtos.Add(tempdatadto);
                                    }
                                }

                                tempdata2.Status=competition.Status;
                                tempdata2.TotalStudents=_unitOfWork.CompetitionUsers
                                    .FindAll(x => x.CompetitionId==competition.Id).Count();
                                tempdata.Add(tempdata2);
                            }
                        }

                        data=tempdata;
                    }
                }

                Response.Result=true;
                Response.Data=data;
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }



        // [Authorize]
        [HttpGet("GetCompetitionsById")]
        public async Task<IActionResult> GetCompetitionsById([FromHeader] int competitionId)
        {
            BaseResponseWithData<CompetitionCreateNewDTO>
                Response = new BaseResponseWithData<CompetitionCreateNewDTO>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var DTO = new CompetitionCreateNewDTO();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    var SubjectRelationshipDB =
                    _unitOfWork.SubjectRelationship.FindAll(x => x.Id > 0, new[] { "SubSubject" });
                    var DoctorCompetition = _unitOfWork.CompetitionMemberAdmins.FindAll(
                    x => (x.CompetitionId == competitionId && (x.RoleName == "doctor" || x.RoleName == "assistant"  ) ),
                    new[] { "HrUser" });
                    var TempData = await _unitOfWork.Competitions.GetByIdAsync(competitionId);
                    var assignsubject = _unitOfWork.AssignedSubjects.FindAll(x => x.CompetitionId == competitionId,
                    new[] { "Academiclevel", "Specialdept", "AcademicYear", "Programm" , "Subject" }).FirstOrDefault();
                    Response.Result=true;
                    DTO=_mapper.Map<CompetitionCreateNewDTO>(TempData);
                    DTO.ImagePath=TempData.ImagePath!=null ? BaseURL+TempData.ImagePath : null;
                    DTO.SubjectId=assignsubject.SubjectId;
                    DTO.SpecialdeptId=assignsubject.SpecialdeptId;
                    DTO.SpecialdeptName=assignsubject.Specialdept.Name;
                    DTO.AcademicYearName=assignsubject.AcademicYear.Term;
                    DTO.AcademiclevelName=assignsubject.Academiclevel.Name;
                    DTO.AcademiclevelId=assignsubject.AcademiclevelId;
                    DTO.AcademicYearId=assignsubject.AcademicYearId;
                    DTO.deptId=assignsubject.Specialdept.DeptartmentId;
                    DTO.GPAScale=assignsubject.Subject?.Gpascale??null;
                    DTO.ApprovedHours=assignsubject.Subject?.ApprovedHours??null;
                    DTO.deptName=_unitOfWork.Depts.FindAll(x => x.Id==DTO.deptId).Select(y => y.Name).FirstOrDefault();
                    DTO.doctorslistDtos=new List<DoctorslistDto>();
                    if(DoctorCompetition.Count()>0)
                    {
                        foreach(var item in DoctorCompetition)
                        {
                            var tempdata = new DoctorslistDto();
                            tempdata.DoctorId=item.HrUserId;
                            tempdata.DoctorName=
                                DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                    .Select(y => y.HrUser.FirstName+" "+y.HrUser.MiddleName)
                                    .FirstOrDefault()??null;
                            tempdata.Image=
                                (DoctorCompetition.Where(x => x.HrUserId==item.HrUserId)
                                    .Select(y => y.HrUser.ImgPath).FirstOrDefault())!=null
                                    ? (BaseURL+DoctorCompetition
                                        .Where(x => (x.RoleName=="doctor"||x.RoleName=="assistant")&&
                                                    x.HrUserId==item.HrUserId)
                                        .Select(y => y.HrUser.ImgPath).FirstOrDefault())
                                    : null;
                            DTO.doctorslistDtos.Add(tempdata);
                        }
                    }

                    DTO.subjectsListRequried=SubjectRelationshipDB
                        .Where(x => x.MainSubjectId==DTO.SubjectId&&x.Status=="with").Select(a =>
                            new SubjectsRequried { Id=a.SubSubjectId , Name=a.SubSubject.Name }).ToList();
                    DTO.subjectsListReject=SubjectRelationshipDB
                        .Where(x => x.MainSubjectId==DTO.SubjectId&&x.Status=="without").Select(a =>
                            new SubjectsReject { Id=a.SubSubjectId , Name=a.SubSubject.Name }).ToList();
                    Response.Data=DTO;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [Authorize]
        [HttpGet("GetCompetitionsByspecialdeptIdandlevelId")]
        public async Task<IActionResult> GetCompetitionsByspecialdeptIdandlevelId([FromHeader] int specialdeptId , [FromHeader] int levelId , [FromHeader] int yearId ,
                                                                                  [FromHeader] int termId , [FromHeader] int programId)
        {
            BaseResponseWithData<List<Competition>> Response = new BaseResponseWithData<List<Competition>>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var DTO = new CompetitionCreateNewDTO();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    var query = _unitOfWork.AssignedSubjects.FindAllQueryable(x=>x.Id > 0).AsQueryable();

                    // Handle year/term filters first as they're more specific
                    if(yearId>0&&termId==0)
                    {
                        var termIds = _unitOfWork.AcademicYears
            .FindAllQueryable(x => x.YearId == yearId)
            .Select(y => y.Id);

                        query=query.Where(c => termIds.Contains(c.AcademicYearId));
                    }
                    else if(termId>0)
                    {
                        query=query.Where(c => c.AcademicYearId==termId);
                    }

                    // Apply program filter if specified
                    if(programId>0)
                    {
                        query=query.Where(c => c.ProgrammId==programId);
                    }

                    // Apply special department and level filters
                    if(specialdeptId>0&&levelId>0)
                    {
                        query=query.Where(x => x.SpecialdeptId==specialdeptId&&x.AcademiclevelId==levelId);
                    }
                    else if(specialdeptId>0)
                    {
                        query=query.Where(x => x.SpecialdeptId==specialdeptId);
                    }
                    else if(levelId>0)
                    {
                        query=query.Where(x => x.AcademiclevelId==levelId);
                    }

                    var competitionIds =query.Select(x => x.CompetitionId).ToList();

                    Response.Data=(List<Competition>)_unitOfWork.Competitions.FindAll(x => competitionIds.Contains(x.Id));
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }






        [Authorize]
        [HttpGet("DistributionOfSubjectsToStudent")]
        public async Task<IActionResult> DistributionOfSubjectsToStudent([FromHeader] string enroll , [FromHeader] long userId)
        {
            BaseResponseWithData<List<DistributionOfSubjectsToStudent>> Response = new BaseResponseWithData<List<DistributionOfSubjectsToStudent>>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var DTO = new List<DistributionOfSubjectsToStudent>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    if(userId<=0)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="يجب ادخال الرقم التعريفي للمستخدم ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var UserDepartment = _unitOfWork.UserDepartment.FindAll(x => x.HrUserId == userId, new[] { "Academiclevel" }).FirstOrDefault();
                    if(UserDepartment==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا الطالب غير مسجل في قسم او شعبة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var  levelId=UserDepartment?.AcademiclevelId;
                    var SpecialdeptId=UserDepartment?.SpecialdeptId;
                    var YearId=UserDepartment?.YearId;


                    var _user = _unitOfWork.HrUsers.FindAll(x=>x.Id == userId).FirstOrDefault();
                    if(_user==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا المستخدم غير موجود";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }



                    var conpetitionDb = _unitOfWork.Competitions.FindAll(x => x.Id > 0);
                    var AllcompetitionuserDB = _unitOfWork.CompetitionUsers.FindAll(x => x.Id > 0, new[] { "Competition" });
                    //  var competitionuserDB = AllcompetitionuserDB.Where(x => x.ApplicationUserId == ApplicationUserId  );
                    var competitionuserDB = AllcompetitionuserDB.Where(x => x.HrUserId == userId && x.FinishStatus != "Finished" );
                    var assignSubjectDB = _unitOfWork.AssignedSubjects.FindAll(x => x.Id > 0 ,new []{ "Academiclevel", "Specialdept", "Programm" , "AcademicYear" });
                    var terms = _unitOfWork.AcademicYears.FindAll(x => x.Id>0);
                    var term = terms.Where(x => x.YearId == YearId).Select(y=>y.Id);
                    var assignsubjectForStudent = assignSubjectDB.Where(x => x.SpecialdeptId == SpecialdeptId && x.AcademiclevelId == levelId && term.Contains(x.AcademicYearId));
                    var memberAdminDB = _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.Id > 0, new[] { "HrUser" });

                    var competitondaytype = _unitOfWork.CompetitionDays.FindAll(x=>x.TypeId == 1);
                    var competitiontypeTotal = _unitOfWork.CompetitionTypes.FindAll(x=>x.TypeId == 1);


                    if(enroll=="subcrib")
                    {
                        foreach(var item in competitionuserDB.Where(x => x.EnrollmentStatus!="reject"&&x.DelayOrWithdrawalStatus!="Withdraw"))
                        {
                            var tempdata = new List<DoctorsDetals>();
                            var tempData = new DistributionOfSubjectsToStudent();
                            tempData.Name=item.Competition.Name;
                            tempData.competitionId=(int)item.CompetitionId;
                            tempData.ImagePath=item.Competition.ImagePath!=null ? BaseURL+item.Competition.ImagePath : null;
                            var competitondaytypeCount =competitondaytype.Where(x => x.CompetitionId==item.CompetitionId);
                            var competitiontypeTotalCount = competitiontypeTotal.Where(x => x.CompetitionId==item.CompetitionId);
                            tempData.remainingNumber=(competitiontypeTotalCount.Select(r => r.Qty).FirstOrDefault())-competitondaytypeCount.Count();
                            tempData.StudyingHours=item.Competition.StudyingHours;
                            tempData.Accreditedhours=item.Competition.Accreditedhours;
                            tempData.SubjectScore=item.Competition.SubjectScore;
                            tempData.LevelId=assignsubjectForStudent.Where(x => x.CompetitionId==(int)item.CompetitionId).Select(x => x.AcademiclevelId).FirstOrDefault();
                            tempData.SpecialDeptId=assignsubjectForStudent.Where(x => x.CompetitionId==(int)item.CompetitionId).Select(x => x.SpecialdeptId).FirstOrDefault();
                            tempData.ProgramId=assignsubjectForStudent.Where(x => x.CompetitionId==(int)item.CompetitionId).Select(x => x.ProgrammId).FirstOrDefault();
                            tempData.AcademicYearId=assignsubjectForStudent.Where(x => x.CompetitionId==(int)item.CompetitionId).Select(x => x.AcademicYearId).FirstOrDefault();
                            tempData.LevelName=assignsubjectForStudent.Where(x => x.CompetitionId==(int)item.CompetitionId).Select(x => x.Academiclevel.Name).FirstOrDefault();
                            tempData.SpecialDeptName=assignsubjectForStudent.Where(x => x.CompetitionId==(int)item.CompetitionId).Select(x => x.Specialdept.Name).FirstOrDefault();
                            tempData.AcademicYearName=assignsubjectForStudent.Where(x => x.CompetitionId==(int)item.CompetitionId).Select(x => x.AcademicYear.Term).FirstOrDefault();
                            tempData.ProgramName=assignsubjectForStudent.Where(x => x.CompetitionId==(int)item.CompetitionId).Select(x => x.Programm?.Name).FirstOrDefault();
                            if((memberAdminDB.Where(x => x.CompetitionId==item.CompetitionId).Count())>0)
                            {

                                foreach(var item1 in (memberAdminDB.Where(X => X.CompetitionId==item.CompetitionId).Select(y => y.HrUser).ToList()))
                                {
                                    var doctor = new DoctorsDetals ();

                                    doctor.doctorId=item1.Id;
                                    doctor.doctorName=item1.FirstName+" "+item1.MiddleName;
                                    doctor.image=item1.ImgPath!=null ? BaseURL+item1.ImgPath : null;
                                    tempdata.Add(doctor);
                                }


                            }
                            tempData.doctorslist=tempdata;
                            if(item.EnrollmentStatus=="approved"&&(!(item.DelayRequestStatus=="DelayRequest"))&&(!(item.WithdrawalRequestStatus=="WithdrawRequest")))
                            {
                                tempData.statusOfStudent="مشترك";
                                tempData.statusOfSubject=item.Competition.Status=="Open" ? "مفتوحة" :
                                                          item.Competition.Status=="OnHold" ? "مغلقة" :
                                                          item.Competition.Status=="Pending" ? "لم تبدا بعد" :
                                                          item.Competition.Status=="Completed" ? " مكتملة" : null;
                            }


                            if(item.EnrollmentStatus=="pending")
                            {
                                tempData.statusOfStudent="منتظر";
                                tempData.statusOfSubject=item.Competition.Status=="Open" ? "مفتوحة" :
                                                                                     item.Competition.Status=="OnHold" ? "مغلقة" :
                                                                                     item.Competition.Status=="Pending" ? "لم تبدا بعد" :
                                                                                     item.Competition.Status=="Completed" ? " مكتملة" : null;
                            }
                            if(item.EnrollmentStatus=="suspended")
                            {
                                tempData.statusOfStudent="موقوف";
                                tempData.statusOfSubject=item.Competition.Status=="Open" ? "مفتوحة" :
                                                         item.Competition.Status=="OnHold" ? "مغلقة" :
                                                         item.Competition.Status=="Pending" ? "لم تبدا بعد" :
                                                         item.Competition.Status=="Completed" ? " مكتملة" : null;
                            }
                            if(item.DelayOrWithdrawalStatus=="delay"&&!(item.EnrollmentStatus=="suspended"))
                            {
                                tempData.statusOfStudent="مؤجل";
                                tempData.statusOfSubject=item.Competition.Status=="Open" ? "مفتوحة" :
                                                          item.Competition.Status=="OnHold" ? "مغلقة" :
                                                          item.Competition.Status=="Pending" ? "لم تبدا بعد" :
                                                          item.Competition.Status=="Completed" ? " مكتملة" : null;
                            }
                            if(item.DelayRequestStatus=="DelayRequest"&&(!(item.DelayOrWithdrawalStatus=="delay"))&&!(item.EnrollmentStatus=="suspended"))
                            {
                                tempData.statusOfStudent="طلب تاجيل";
                                tempData.statusOfSubject=item.Competition.Status=="Open" ? "مفتوحة" :
                                                           item.Competition.Status=="OnHold" ? "مغلقة" :
                                                           item.Competition.Status=="Pending" ? "لم تبدا بعد" :
                                                           item.Competition.Status=="Completed" ? " مكتملة" : null;
                            }
                            if(item.WithdrawalRequestStatus=="WithdrawRequest"&&(!(item.DelayOrWithdrawalStatus=="Withdraw"))&&!(item.EnrollmentStatus=="suspended"))
                            {
                                tempData.statusOfStudent="طلب سحب";
                                tempData.statusOfSubject=item.Competition.Status=="Open" ? "مفتوحة" :
                                                          item.Competition.Status=="OnHold" ? "مغلقة" :
                                                          item.Competition.Status=="Pending" ? "لم تبدا بعد" :
                                                          item.Competition.Status=="Completed" ? " مكتملة" : null;
                            }

                            DTO.Add(tempData);
                        }
                    }



                    if(enroll=="same level")
                    {

                        var competitionIdsForlevelStudent = assignsubjectForStudent.Select(x => x.CompetitionId).ToList();
                        var competiotionstudent = _unitOfWork.Competitions.FindAll(x=>x.Status == "Finished").Select(y=>y.Id).ToList();
                        competitionIdsForlevelStudent=competitionIdsForlevelStudent.Except(competiotionstudent).ToList();
                        var competitionSubscripIds = competitionuserDB.Where(x => !(x.EnrollmentStatus == "reject") && !(x.DelayOrWithdrawalStatus == "Withdraw") && x.CompetitionId != null).Select(x => (int)x.CompetitionId).ToList();
                        var commocCompetition = competitionIdsForlevelStudent.Intersect(competitionSubscripIds).ToList();
                        var competitionNotSubscripIds = competitionIdsForlevelStudent.Except(commocCompetition);


                        foreach(var item in competitionNotSubscripIds)
                        {

                            var tempData = new DistributionOfSubjectsToStudent();
                            tempData.Name=conpetitionDb.Where(x => x.Id==item).Select(a => a.Name).FirstOrDefault();
                            tempData.competitionId=item;
                            tempData.ImagePath=(conpetitionDb.Where(x => x.Id==item).Select(a => a.ImagePath).FirstOrDefault())!=null ? BaseURL+(conpetitionDb.Where(x => x.Id==item).Select(a => a.ImagePath).FirstOrDefault()) : null;
                            var competitondaytypeCount =competitondaytype.Where(x => x.CompetitionId==item);
                            var competitiontypeTotalCount = competitiontypeTotal.Where(x => x.CompetitionId==item);
                            tempData.remainingNumber=(competitiontypeTotalCount.Select(r => r.Qty).FirstOrDefault())-competitondaytypeCount.Count();
                            tempData.StudyingHours=conpetitionDb.Where(x => x.Id==item).Select(a => a.StudyingHours).FirstOrDefault();
                            tempData.Accreditedhours=conpetitionDb.Where(x => x.Id==item).Select(a => a.Accreditedhours).FirstOrDefault();
                            tempData.SubjectScore=conpetitionDb.Where(x => x.Id==item).Select(a => a.SubjectScore).FirstOrDefault();
                            tempData.LevelId=assignsubjectForStudent.Where(x => x.CompetitionId==item).Select(x => x.AcademiclevelId).FirstOrDefault();
                            tempData.SpecialDeptId=assignsubjectForStudent.Where(x => x.CompetitionId==item).Select(x => x.SpecialdeptId).FirstOrDefault();
                            tempData.ProgramId=assignsubjectForStudent.Where(x => x.CompetitionId==item).Select(x => x.ProgrammId).FirstOrDefault();
                            tempData.AcademicYearId=assignsubjectForStudent.Where(x => x.CompetitionId==item).Select(x => x.AcademicYearId).FirstOrDefault();
                            tempData.LevelName=assignsubjectForStudent.Where(x => x.CompetitionId==item).Select(x => x.Academiclevel.Name).FirstOrDefault();
                            tempData.SpecialDeptName=assignsubjectForStudent.Where(x => x.CompetitionId==item).Select(x => x.Specialdept.Name).FirstOrDefault();
                            tempData.AcademicYearName=assignsubjectForStudent.Where(x => x.CompetitionId==item).Select(x => x.AcademicYear.Term).FirstOrDefault();
                            tempData.ProgramName=assignsubjectForStudent.Where(x => x.CompetitionId==item).Select(x => x.Programm.Name).FirstOrDefault();
                            if(memberAdminDB.Where(X => X.CompetitionId==item).Count()>0)
                            {
                                var tempdata = new List<DoctorsDetals>();

                                foreach(var item1 in (memberAdminDB.Where(X => X.CompetitionId==item).Select(y => y.HrUser).ToList()))
                                {
                                    var doctor = new DoctorsDetals ();

                                    doctor.doctorId=item1.Id;
                                    doctor.doctorName=item1.FirstName+" "+item1.MiddleName;
                                    doctor.image=item1.ImgPath!=null ? BaseURL+item1.ImgPath : null;
                                    tempdata.Add(doctor);
                                }
                                tempData.doctorslist=tempdata;

                            }

                            if((conpetitionDb.Where(x => x.Id==item).Select(a => a.Capacity).FirstOrDefault())>=(AllcompetitionuserDB.Where(x => x.CompetitionId==item&&x.EnrollmentStatus!="reject").Count()))
                            {
                                tempData.statusOfSubject="مفتوحة";
                                tempData.statusOfStudent="متاح للاشتراك";

                            }
                            if((conpetitionDb.Where(x => x.Id==item).Select(a => a.Status).FirstOrDefault()=="Pending"))
                            {
                                tempData.statusOfSubject="لم تبدا بعد";
                                tempData.statusOfStudent=null;

                            }

                            if((conpetitionDb.Where(x => x.Id==item).Select(a => a.Status).FirstOrDefault()=="OnHold"))
                            {
                                tempData.statusOfSubject="مغلق";
                                tempData.statusOfStudent=null;

                            }

                            if((conpetitionDb.Where(x => x.Id==item).Select(a => a.Status).FirstOrDefault())=="Completed")
                            {
                                tempData.statusOfSubject="مكتملة";
                                tempData.statusOfStudent=null;
                            }
                            if(competitionuserDB.Where(x => (x.EnrollmentStatus=="reject")&&x.CompetitionId==item).Any())
                            {
                                tempData.statusOfStudent="مرفوض";
                                tempData.statusOfSubject=null;
                            }
                            if(competitionuserDB.Where(x => (x.DelayOrWithdrawalStatus=="Withdraw")&&x.CompetitionId==item).Any())
                            {
                                tempData.statusOfStudent="تم السحب";
                                tempData.statusOfSubject=null;
                            }

                            DTO.Add(tempData);
                        }
                    }



                    if(enroll=="same program")
                    {
                        var tempdata = new List<DoctorsDetals>();

                        var programmIdForStudent = _unitOfWork.Academiclevels.FindAll(x => x.Id == levelId).Select(y => y.ProgramId).FirstOrDefault();
                        var allcompetitionsForProgramOnly = assignSubjectDB.Where(x => x.ProgrammId == programmIdForStudent).Select(x => x.CompetitionId).ToList();
                        var allcompetitionsForProgramAndLevellStudent = assignSubjectDB.Where(x => x.ProgrammId == programmIdForStudent&& x.AcademiclevelId == levelId && term.Contains(x.AcademicYearId) &&x.SpecialdeptId==SpecialdeptId).Select(x => x.CompetitionId).ToList();
                        var allcompetitionsForProgram = allcompetitionsForProgramOnly.Except(allcompetitionsForProgramAndLevellStudent).ToList();
                        var test = AllcompetitionuserDB.Where(x => x.HrUserId == userId && (x.EnrollmentStatus == "approved" || x.EnrollmentStatus == "pending")&& x.DelayOrWithdrawalStatus != "Withdraw").Select(y =>(int) y.CompetitionId).ToList();
                        var allcompetitionsForProgramhhh = allcompetitionsForProgram.Except(test).ToList();
                        //var competitionsForStudentExpectRejectAndWithdraw = competitionuserDB.Where(x => x.EnrollmentStatus != "reject" && !(x.DelayOrWithdrawalStatus == "Withdraw") ).Select(y => (int)y.CompetitionId).ToList();
                        //var remaincompetitionIds = allcompetitionsForProgram.Except(competitionsForStudentExpectRejectAndWithdraw).ToList();
                        var compettionfinished = _unitOfWork.Competitions.FindAll(x=>x.Status == "Finished").Select(y=>y.Id).ToList();

                        allcompetitionsForProgramhhh=allcompetitionsForProgramhhh.Except(compettionfinished).ToList();

                        foreach(var item in allcompetitionsForProgramhhh)
                        {

                            var tempData = new DistributionOfSubjectsToStudent();
                            tempData.Name=conpetitionDb.Where(x => x.Id==item).Select(a => a.Name).FirstOrDefault();
                            tempData.competitionId=item;
                            tempData.ImagePath=(conpetitionDb.Where(x => x.Id==item).Select(a => a.ImagePath).FirstOrDefault())!=null ? BaseURL+(conpetitionDb.Where(x => x.Id==item).Select(a => a.ImagePath).FirstOrDefault()) : null;
                            var competitondaytypeCount =competitondaytype.Where(x => x.CompetitionId==item);
                            var competitiontypeTotalCount = competitiontypeTotal.Where(x => x.CompetitionId==item);
                            tempData.remainingNumber=(competitiontypeTotalCount.Select(r => r.Qty).FirstOrDefault())-competitondaytypeCount.Count();
                            tempData.StudyingHours=conpetitionDb.Where(x => x.Id==item).Select(a => a.StudyingHours).FirstOrDefault();
                            tempData.Accreditedhours=conpetitionDb.Where(x => x.Id==item).Select(a => a.Accreditedhours).FirstOrDefault();
                            tempData.SubjectScore=conpetitionDb.Where(x => x.Id==item).Select(a => a.SubjectScore).FirstOrDefault();
                            tempData.LevelId=assignSubjectDB.Where(x => x.CompetitionId==item).Select(x => x.AcademiclevelId).FirstOrDefault();
                            tempData.SpecialDeptId=assignSubjectDB.Where(x => x.CompetitionId==item).Select(x => x.SpecialdeptId).FirstOrDefault();
                            tempData.ProgramId=assignSubjectDB.Where(x => x.CompetitionId==item).Select(x => x.ProgrammId).FirstOrDefault();
                            tempData.AcademicYearId=assignSubjectDB.Where(x => x.CompetitionId==item).Select(x => x.AcademicYearId).FirstOrDefault();
                            tempData.LevelName=assignSubjectDB.Where(x => x.CompetitionId==item).Select(x => x.Academiclevel.Name).FirstOrDefault();
                            tempData.SpecialDeptName=assignSubjectDB.Where(x => x.CompetitionId==item).Select(x => x.Specialdept.Name).FirstOrDefault();
                            tempData.AcademicYearName=assignSubjectDB.Where(x => x.CompetitionId==item).Select(x => x.AcademicYear.Term).FirstOrDefault();
                            tempData.ProgramName=assignSubjectDB.Where(x => x.CompetitionId==item).Select(x => x.Programm.Name).FirstOrDefault();
                            if(memberAdminDB.Where(X => X.CompetitionId==item).Count()>0)
                            {
                                var doctor = new DoctorsDetals ();
                                foreach(var item1 in (memberAdminDB.Where(X => X.CompetitionId==item).Select(y => y.HrUser).ToList()))
                                {
                                    doctor.doctorId=item1.Id;
                                    doctor.doctorName=item1.FirstName+" "+item1.MiddleName;
                                    doctor.image=item1.ImgPath!=null ? BaseURL+item1.ImgPath : null;
                                    tempdata.Add(doctor);
                                }
                            }
                            tempData.doctorslist=tempdata;

                            if((conpetitionDb.Where(x => x.Id==item).Select(a => a.Capacity).FirstOrDefault())>=(AllcompetitionuserDB.Where(x => x.CompetitionId==item&&x.EnrollmentStatus!="reject"&&x.DelayOrWithdrawalStatus!="Withdraw").Count()))
                            {
                                tempData.statusOfSubject="مفتوحة";
                                tempData.statusOfStudent="متاح للاشتراك";

                            }
                            if((conpetitionDb.Where(x => x.Id==item).Select(a => a.Status).FirstOrDefault()=="Pending"))
                            {
                                tempData.statusOfSubject="لم تبدا بعد";
                                tempData.statusOfStudent=null;

                            }
                            if((conpetitionDb.Where(x => x.Id==item).Select(a => a.Status).FirstOrDefault()=="OnHold"))
                            {
                                tempData.statusOfSubject="مغلق";
                                tempData.statusOfStudent=null;

                            }


                            if(conpetitionDb.Where(x => x.Id==item).Select(a => a.Status).FirstOrDefault()=="Completed")
                            {
                                tempData.statusOfSubject="مكتملة";
                                tempData.statusOfStudent=null;

                            }
                            if(competitionuserDB.Where(x => (x.EnrollmentStatus=="reject")&&x.CompetitionId==item).Any())
                            {
                                tempData.statusOfStudent="مرفوض";
                                tempData.statusOfSubject=null;
                            }
                            if(competitionuserDB.Where(x => (x.DelayOrWithdrawalStatus=="Withdraw")&&x.CompetitionId==item).Any())
                            {
                                tempData.statusOfStudent="تم السحب";
                                tempData.statusOfSubject=null;
                            }

                            DTO.Add(tempData);
                        }
                    }



                    Response.Data=DTO;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }




        [Authorize]
        [HttpGet("FinishedSubjectsToStudent")]
        public async Task<IActionResult> FinishedSubjectsToStudent([FromHeader] int YearId , [FromHeader] int ProgramId , [FromHeader] string status , [FromHeader] long userId)
        {
            BaseResponseWithData<List<ProgramList>> Response = new BaseResponseWithData<List<ProgramList>>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    var dto = new List<ProgramList>();

                    var _user =  _unitOfWork.HrUsers.FindAll(x=>x.Id == userId).FirstOrDefault();
                    if(_user==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا المستخدم غير موجود";
                        Response.Errors.Add(error);
                        return BadRequest(Response);
                    }

                    var userRoles = _unitOfWork.UserRoles.FindAllQueryable(x=>x.Id > 0 ,new[]{"Role"}).Select(n=>n.Role.Name);

                    var userIsadmin= (userRoles.Any()&&userRoles.Contains("admin"));
                    var userIsStudent= (userRoles.Any()&&userRoles.Contains("student"));
                    var userIsdoctor = (userRoles.Any()&&userRoles.Contains( "doctor"));
                    var userIsadminCompetition =  (userRoles.Any()&&userRoles.Contains( "adminCompetition"));
                    var userIsadmincontrol = (userRoles.Any()&&userRoles.Contains( "admincontrol"));


                    //var roleOfUerId = await _userManager.IsInRoleAsync(_userStudent, "student");

                    if(!(userIsStudent||userIsdoctor||userIsadminCompetition||userIsadmin||userIsadmincontrol))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا المستخدم ليس له صلاحية  ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var YearList = _unitOfWork.Years.FindAll(x=>x.Id > 0);
                    var AcademicYearList = new List<AcademicYear>();
                    var programListDB = _unitOfWork.Programm.FindAll(x => x.Id > 0);

                    // var conpetitionDb = _unitOfWork.Competitions.FindAll(x => x.Id > 0);
                    // var Allcompetitionuser = _unitOfWork.CompetitionUsers.FindAll(x => x.ApplicationUserId == ApplicationUserId && x.FinishStatus == "منتهية").Select(a => a.Competition);
                    var Deptlist = _unitOfWork.Depts.FindAll(x => x.Id > 0);
                    var memberAdminDB = _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.Id > 0 , new[] { "HrUser" });


                    if(YearId>0)
                    {
                        AcademicYearList=_unitOfWork.AcademicYears
                            .FindAll(x => x.YearId==YearId).ToList();
                        //if(AcademicYearList==null)
                        //{
                        //    Response.Result=false;
                        //    Response.Errors.Add("لم تضاف ترمات لهذة السنة");
                        //    return BadRequest(Response);
                        //}
                    }
                    else
                    {
                        AcademicYearList=_unitOfWork.AcademicYears
                            .FindAll(x => x.Id>0).ToList();

                    }
                    if(ProgramId>0)
                    {
                        programListDB=programListDB.Where((x => x.Id==ProgramId)).ToList();
                    }
                    if(status=="finished")
                    {
                        //  var hany =_ResultControlService.SumStudentSubjectsForYear(userId , 1);


                        if(userIsStudent)
                        {
                            var AllcompetitionuserIds = _unitOfWork.CompetitionUsers.FindAll(x => x.HrUserId == userId && x.FinishStatus == "Finished" && x.EnrollmentStatus != "suspended" && x.EnrollmentStatus != "pending" && x.EnrollmentStatus != "reject" && x.DelayOrWithdrawalStatus != "delay" && x.DelayOrWithdrawalStatus != "Withdraw").Select(a => a.CompetitionId);
                            var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll(x => AllcompetitionuserIds.Contains(x.CompetitionId), new[] { "Specialdept", "Academiclevel", "Competition" });
                            var ResultControlForProgramsDB = _unitOfWork.ResultControlForPrograms.FindAll(x=>x.Id > 0);
                            var ResultControlDB = _unitOfWork.ResultControls.FindAll(x=>x.Id > 0);



                            var summry = programListDB.Where(pro => AssignSubjectList.Where(x => x.ProgrammId == pro.Id).Any()).Select(pro => new ProgramList
                            {
                                Id = pro.Id,
                                Name = pro.Name,
                                GpaForProgram = _unitOfWork.ResultControlForPrograms.FindAll(x=>x.ProgrammId == pro.Id).Select(r=>r.TotalGpa).FirstOrDefault(),
                                yearList = YearList.Where(yea => AssignSubjectList.Where(x => x.ProgrammId == pro.Id && (AcademicYearList.Where(y => y.YearId == yea.Id).Any())).Any())
                        .Select(yea => new YearList
                        {
                            Id = yea.Id,
                            Name = yea.Name,
                            GpaForYear = _ResultControlService.SumStudentSubjectsForYear(userId , yea.Id).Data,
                            termLists = AcademicYearList.Where(x => x.YearId == yea.Id).Select(term => new TermList
                            {
                                Id = term.Id,
                                Name = term.Term,
                                GpaForTerm = _unitOfWork.ResultControlForStudents.FindAll(x=>x.ProgrammId == pro.Id && x.AcademicYearId == term.Id).Select(y=>y.TotalGpa).FirstOrDefault(),
                                NumOfCompetitions = AssignSubjectList.Where(x => x.ProgrammId == pro.Id && x.AcademicYearId == term.Id).Count(),


                            }).ToList()
                        }).ToList()
                            }).ToList();
                            dto=summry;
                        }
                        if((userIsdoctor||userIsadminCompetition))
                        {
                            var AllcompetitionDoctorIds = _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.HrUserId == userId ).Select(a => a.CompetitionId).ToList();
                            var AllcompetitionIds = _unitOfWork.Competitions.FindAll(x => x.Status == "Finished").Select(a =>(int) a.Id).ToList();
                            var AllcompetitionuserIds = AllcompetitionDoctorIds.Intersect(AllcompetitionIds).ToList();
                            var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll(x => AllcompetitionuserIds.Contains(x.CompetitionId), new[] { "Specialdept", "Academiclevel", "Competition" });

                            var summry = programListDB.Where(pro => AssignSubjectList.Where(x => x.ProgrammId == pro.Id).Any()).Select(pro => new ProgramList
                            {
                                Id = pro.Id,
                                Name = pro.Name,
                                yearList = YearList
                        .Select(yea => new YearList
                        {
                            Id = yea.Id,
                            Name = yea.Name ,

                            termLists = AcademicYearList.Where(x => x.YearId == yea.Id).Select(term => new TermList
                            {
                                Id = term.Id,
                                Name = term.Term,
                                NumOfCompetitions = AssignSubjectList.Where(x => x.ProgrammId == pro.Id && x.AcademicYearId == term.Id).Count(),


                            }).ToList()
                        }).ToList()
                            }).ToList();
                            dto=summry;
                        }
                        if((userIsadmin||userIsadmincontrol))
                        {

                            var AllcompetitionIds = _unitOfWork.Competitions.FindAll(x => x.Status == "Finished").Select(a => a.Id);
                            var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll(x => AllcompetitionIds.Contains(x.CompetitionId), new[] { "Specialdept", "Academiclevel", "Competition" });


                            //var summry = programListDB.Where(pro => AssignSubjectList.Where(x => x.ProgrammId == pro.Id).Any()).Select(pro => new ProgramList
                            //  yearList=YearList.Where(yea => AssignSubjectList.Where(x => x.ProgrammId==pro.Id&&(AcademicYearList.Where(y => y.YearId==yea.Id).Any())).Any())


                            var summry = programListDB.Select(pro => new ProgramList
                            {
                                Id = pro.Id,
                                Name = pro.Name,
                                NumOfYears = YearList.Count() ,
                                yearList = YearList
                        .Select(yea => new YearList
                        {
                            Id = yea.Id,
                            Name = yea.Name,
                            NumOfTerms = AcademicYearList.Count() ,
                            termLists = AcademicYearList.Where(x => x.YearId == yea.Id).Select(term => new TermList
                            {
                                Id = term.Id,
                                Name = term.Term,
                                NumOfCompetitions = AssignSubjectList.Where(x => x.ProgrammId == pro.Id && x.AcademicYearId == term.Id).Count(),

                            }).ToList()
                        }).ToList()
                            }).ToList();
                            dto=summry;
                        }
                    }
                    if(status=="current")
                    {
                        if(userIsdoctor||userIsadminCompetition)
                        {
                            var AllcompetitionDoctorIds = _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.HrUserId == userId ).Select(a => a.CompetitionId).ToList();
                            var AllfinishedCompetitionIds = _unitOfWork.Competitions.FindAll(x => x.Status == "Finished").Select(r=>r.Id);
                            var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll(x => AllcompetitionDoctorIds.Contains(x.CompetitionId) && !(AllfinishedCompetitionIds.Contains(x.CompetitionId)) , new[] { "Specialdept", "Academiclevel", "Competition" });
                            var competition = _unitOfWork.Competitions.FindAll(x=>x.Id > 0);

                            var summry = programListDB.Where(pro => AssignSubjectList.Where(x => x.ProgrammId == pro.Id).Any()).Select(pro => new ProgramList
                            {
                                Id = pro.Id,
                                Name = pro.Name,
                                NumOfYears = YearList.Count() ,
                                yearList = YearList.Select(yea => new YearList
                                {
                                    Id = yea.Id,
                                    Name = yea.Name,
                                    NumOfTerms = AcademicYearList.Count() ,
                                    termLists = AcademicYearList.Where(x => x.YearId == yea.Id).Select(term => new TermList
                                    {
                                        Id = term.Id,
                                        Name = term.Term,
                                        NumOfCompetitions =  AssignSubjectList.Where(x => x.ProgrammId == pro.Id && x.AcademicYearId == term.Id ).Count(),

                                    }).ToList()
                                }).ToList()

                            }).ToList();
                            dto=summry;

                        }



                        if(userIsadmin||userIsadmincontrol)
                        {
                            var AllfinishedCompetitionIds = _unitOfWork.Competitions.FindAll(x => x.Status == "Finished").Select(r=>r.Id);
                            var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll(x => x.Id > 0  &&  !(AllfinishedCompetitionIds.Contains(x.CompetitionId)) , new [] { "Specialdept", "Academiclevel", "Competition" });
                            var competition = _unitOfWork.Competitions.FindAll(x=>x.Id > 0);

                            var summry = programListDB.Where(pro => AssignSubjectList.Where(x => x.ProgrammId == pro.Id).Any()).Select(pro => new ProgramList
                            {
                                Id = pro.Id,
                                Name = pro.Name,
                                NumOfYears = YearList.Count() ,
                                yearList = YearList.Select(yea => new YearList
                                {
                                    Id = yea.Id,
                                    Name = yea.Name,
                                    NumOfTerms = AcademicYearList.Count() ,

                                    termLists = AcademicYearList.Where(x => x.YearId == yea.Id).Select(term => new TermList
                                    {
                                        Id = term.Id,
                                        Name = term.Term,
                                        NumOfCompetitions = AssignSubjectList.Where(x => x.ProgrammId == pro.Id && x.AcademicYearId == term.Id ).Count(),

                                    }).ToList()
                                }).ToList()

                            }).ToList();

                            dto=summry;
                        }
                    }

                    Response.Result=true;
                    Response.Data=dto;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }

        [Authorize]
        [HttpGet("competitionByProgramAndTerm")]
        public async Task<IActionResult> competitionByProgramAndTerm([FromHeader] int TermId , [FromHeader] int ProgramId , [FromHeader] long userId , [FromHeader] string status , [FromHeader] int PageNo = 1 , [FromHeader] int NoOfItems = 20)
        {
            BaseResponseWithDataAndHeader<List<FinishCompetitionVM>> Response = new BaseResponseWithDataAndHeader<List<FinishCompetitionVM>>();

            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                //  var data = new List<CompetitionDTO>();
                if(Response.Result)
                {

                    var _user =  _unitOfWork.HrUsers.FindAll(x=>x.Id == userId).FirstOrDefault();
                    if(_user==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا المستخدم غير موجود";
                        Response.Errors.Add(error);

                    }

                    var userRoles = _unitOfWork.UserRoles.FindAllQueryable(x=>x.Id > 0 ,new[]{"Role"}).Select(n=>n.Role.Name);

                    var userIsadmin= (userRoles.Any()&&userRoles.Contains("admin"));
                    var userIsStudent= (userRoles.Any()&&userRoles.Contains("student"));
                    var userIsdoctor = (userRoles.Any()&&userRoles.Contains( "doctor"));
                    var userIsadminCompetition =  (userRoles.Any()&&userRoles.Contains( "adminCompetition"));
                    var userIsadmincontrol = (userRoles.Any()&&userRoles.Contains( "admincontrol"));




                    if(!(userIsStudent||userIsdoctor||userIsadminCompetition||userIsadmin))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا المستخدم ليس له صلاحية  ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var programListDB = _unitOfWork.Programm.FindAll(x => x.Id > 0);

                    // var conpetitionDb = _unitOfWork.Competitions.FindAll(x => x.Id > 0);
                    // var Allcompetitionuser = _unitOfWork.CompetitionUsers.FindAll(x => x.ApplicationUserId == ApplicationUserId && x.FinishStatus == "منتهية").Select(a => a.Competition);
                    var Deptlist = _unitOfWork.Depts.FindAll(x => x.Id > 0);
                    var memberAdminDB = _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.Id > 0 , new[] { "HrUser" });



                    var   CompetitionList = new List<FinishCompetitionVM>();

                    if(status=="current")
                    {
                        if(userIsdoctor||userIsadminCompetition)
                        {
                            var AllcompetitionDoctorIds = _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.HrUserId == userId ).Select(a => a.CompetitionId).ToList();
                            var Allcompetitionuser = _unitOfWork.CompetitionUsers.FindAll(x => x.Id>0 && x.EnrollmentStatus != "reject"   );
                            var AllfinishedCompetitionIds = _unitOfWork.Competitions.FindAll(x => x.Status == "Finished").Select(r=>r.Id);
                            var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll(x => AllcompetitionDoctorIds.Contains(x.CompetitionId) &&  !(AllfinishedCompetitionIds.Contains(x.CompetitionId)), new[] { "Specialdept", "Academiclevel", "Competition" });
                            var competition = _unitOfWork.Competitions.FindAll(x=>x.Id > 0);




                            CompetitionList=AssignSubjectList.Where(x => x.ProgrammId==ProgramId&&x.AcademicYearId==TermId).Select(assinsubjest => new FinishCompetitionVM
                            {
                                competitionId=assinsubjest.CompetitionId ,
                                Name=assinsubjest?.Competition.Name ,
                                ImagePath=assinsubjest.Competition.ImagePath!=null ? BaseURL+assinsubjest.Competition.ImagePath : null ,
                                LevelName=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.Academiclevel.Name).FirstOrDefault() ,
                                LevelId=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.AcademiclevelId).FirstOrDefault() ,
                                SpecialDeptName=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.Specialdept.Name).FirstOrDefault() ,
                                SpecialDeptId=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.SpecialdeptId).FirstOrDefault() ,
                                DeptName=Deptlist.Where(y => y.Id==(AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.Specialdept.DeptartmentId).FirstOrDefault())).Select(w => w.Name).FirstOrDefault() ,
                                doctorsName=memberAdminDB.Where(X => X.CompetitionId==assinsubjest.CompetitionId).Select(y => y.HrUser.FirstName+" "+y.HrUser.MiddleName).ToList() ,
                                capacityofCompetition=assinsubjest.Competition?.Capacity??0 ,
                                ProgramId=ProgramId ,
                                ProgramName=programListDB.Where(x => x.Id==ProgramId).Select(p => p.Name).FirstOrDefault() ,
                                StudentsNum=(Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.DelayOrWithdrawalStatus!="delay"&&x.DelayOrWithdrawalStatus!="Withdraw").Count()) ,
                                statusOfSubject=((competition.Where(x => x.Id==assinsubjest.CompetitionId).Select(y => y.Status).FirstOrDefault()=="Open")&&((competition.Where(x => x.Id==assinsubjest.CompetitionId).Select(y => y.Capacity).FirstOrDefault())>=(Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Count()))) ? "متاحة للاشتراك  " :
                                        ((competition.Where(x => x.Id==assinsubjest.CompetitionId).Select(y => y.Status).FirstOrDefault()=="OnHold")) ? "مغلقة" :
                                         ((competition.Where(x => x.Id==assinsubjest.CompetitionId).Select(y => y.Status).FirstOrDefault()=="Pending")) ? "لم تبدا بعد" :
                                         ((competition.Where(x => x.Id==assinsubjest.CompetitionId).Select(y => y.Capacity).FirstOrDefault())<=(Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Count())) ? "مكتمل" : null ,
                                DelayRequestNum=Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.DelayRequestStatus=="DelayRequest"&&x.DelayOrWithdrawalStatus!="delay").Count() ,
                                WithdrawRequestNum=Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.WithdrawalRequestStatus=="WithdrawRequest"&&x.DelayOrWithdrawalStatus!="Withdraw").Count() ,
                                PendingNum=Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.EnrollmentStatus=="pending").Count() ,


                            }).ToList();

                        }



                        if(userIsadmin)
                        {
                            var Allcompetitionuser = _unitOfWork.CompetitionUsers.FindAll(x => x.Id>0 && x.EnrollmentStatus != "reject");
                            var AllfinishedCompetitionIds = _unitOfWork.Competitions.FindAll(x => x.Status == "Finished").Select(r=>r.Id);
                            var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll(x => x.Id > 0 &&  !(AllfinishedCompetitionIds.Contains(x.CompetitionId))  , new [] { "Specialdept", "Academiclevel", "Competition" });
                            var competition = _unitOfWork.Competitions.FindAll(x=>x.Id > 0);



                            CompetitionList=AssignSubjectList.Where(x => x.ProgrammId==ProgramId&&x.AcademicYearId==TermId).Select(assinsubjest => new FinishCompetitionVM
                            {
                                competitionId=assinsubjest.CompetitionId ,
                                Name=assinsubjest.Competition.Name ,
                                ImagePath=assinsubjest.Competition.ImagePath!=null ? BaseURL+assinsubjest.Competition.ImagePath : null ,
                                LevelName=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.Academiclevel.Name).FirstOrDefault() ,
                                SpecialDeptName=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.Specialdept.Name).FirstOrDefault() ,
                                LevelId=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.AcademiclevelId).FirstOrDefault() ,
                                DeptName=Deptlist.Where(y => y.Id==(AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.Specialdept.DeptartmentId).FirstOrDefault())).Select(w => w.Name).FirstOrDefault() ,
                                SpecialDeptId=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.SpecialdeptId).FirstOrDefault() ,
                                doctorsName=memberAdminDB.Where(X => X.CompetitionId==assinsubjest.CompetitionId).Select(y => y.HrUser.FirstName+" "+y.HrUser.MiddleName).ToList() ,
                                capacityofCompetition=assinsubjest.Competition?.Capacity??0 ,
                                ProgramId=ProgramId ,
                                ProgramName=programListDB.Where(x => x.Id==ProgramId).Select(p => p.Name).FirstOrDefault() ,
                                StudentsNum=(Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.DelayOrWithdrawalStatus!="delay"&&x.DelayOrWithdrawalStatus!="Withdraw").Count()) ,
                                statusOfSubject=((competition.Where(x => x.Id==assinsubjest.CompetitionId).Select(y => y.Status).FirstOrDefault()=="Open")&&((competition.Where(x => x.Id==assinsubjest.CompetitionId).Select(y => y.Capacity).FirstOrDefault())>=(Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Count()))) ? "متاحة للاشتراك  " :
                                                        ((competition.Where(x => x.Id==assinsubjest.CompetitionId).Select(y => y.Status).FirstOrDefault()=="OnHold")) ? "مغلقة" :
                                                         ((competition.Where(x => x.Id==assinsubjest.CompetitionId).Select(y => y.Status).FirstOrDefault()=="Pending")) ? "لم تبدا بعد" :
                                                         ((competition.Where(x => x.Id==assinsubjest.CompetitionId).Select(y => y.Capacity).FirstOrDefault())<=(Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Count())) ? "مكتمل" : null ,
                                DelayRequestNum=Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.DelayRequestStatus=="DelayRequest"&&x.DelayOrWithdrawalStatus!="delay").Count() ,
                                WithdrawRequestNum=Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.WithdrawalRequestStatus=="WithdrawRequest"&&x.DelayOrWithdrawalStatus!="Withdraw").Count() ,
                                PendingNum=Allcompetitionuser.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.EnrollmentStatus=="pending").Count() ,

                            }).ToList();

                        }
                    }
                    else if(status=="finished")
                    {
                        if(userIsdoctor||userIsadminCompetition)
                        {
                            var AllcompetitionDoctorIds = _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.HrUserId == userId ).Select(a => a.CompetitionId).ToList();
                            var AllcompetitionIds= _unitOfWork.Competitions.FindAll(x => x.Status == "Finished").Select(a =>(int) a.Id).ToList();
                            var AllcompetitionIdsForDoctor = AllcompetitionDoctorIds.Intersect(AllcompetitionIds).ToList();
                            var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll(x => AllcompetitionIdsForDoctor.Contains(x.CompetitionId), new[] { "Specialdept", "Academiclevel", "Competition" });

                            CompetitionList=AssignSubjectList.Where(x => x.ProgrammId==ProgramId&&x.AcademicYearId==TermId).Select(assinsubjest => new FinishCompetitionVM
                            {
                                competitionId=assinsubjest.CompetitionId ,
                                Name=assinsubjest.Competition.Name ,
                                ImagePath=assinsubjest.Competition.ImagePath!=null ? BaseURL+assinsubjest.Competition.ImagePath : null ,
                                LevelName=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.Academiclevel.Name).FirstOrDefault() ,
                                LevelId=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.AcademiclevelId).FirstOrDefault() ,
                                SpecialDeptName=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.Specialdept.Name).FirstOrDefault() ,
                                SpecialDeptId=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.SpecialdeptId).FirstOrDefault() ,
                                DeptName=Deptlist.Where(y => y.Id==(AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.Specialdept?.DeptartmentId).FirstOrDefault())).Select(w => w.Name).FirstOrDefault() ,
                                ProgramId=ProgramId ,
                                ProgramName=programListDB.Where(x => x.Id==ProgramId).Select(p => p.Name).FirstOrDefault() ,
                                statusOfSubject="منتهية" ,
                                doctorsName=memberAdminDB.Where(X => X.CompetitionId==assinsubjest.CompetitionId).Select(y => y.HrUser.FirstName+" "+y.HrUser.MiddleName).ToList() ,
                                capacityofCompetition=assinsubjest.Competition.Capacity ,
                                //StudentsNum=AllcompetitionuserIds.Count() ,
                            }).ToList();
                        }
                        if(userIsadmin||userIsadmincontrol)
                        {

                            var AllcompetitionIds = _unitOfWork.Competitions.FindAll(x => x.Status == "Finished").Select(a => a.Id);
                            var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll(x => AllcompetitionIds.Contains(x.CompetitionId), new[] { "Specialdept", "Academiclevel", "Competition" });


                            CompetitionList=AssignSubjectList.Where(x => x.ProgrammId==ProgramId&&x.AcademicYearId==TermId).Select(assinsubjest => new FinishCompetitionVM
                            {
                                competitionId=assinsubjest.CompetitionId ,
                                Name=assinsubjest.Competition.Name ,
                                ImagePath=assinsubjest.Competition.ImagePath!=null ? BaseURL+assinsubjest.Competition.ImagePath : null ,
                                LevelName=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.Academiclevel.Name).FirstOrDefault() ,
                                LevelId=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.AcademiclevelId).FirstOrDefault() ,
                                SpecialDeptName=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==TermId).Select(y => y.Specialdept.Name).FirstOrDefault() ,
                                SpecialDeptId=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==TermId).Select(y => y.SpecialdeptId).FirstOrDefault() ,
                                DeptName=Deptlist.Where(y => y.Id==(AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.Specialdept?.DeptartmentId).FirstOrDefault())).Select(w => w.Name).FirstOrDefault() ,
                                ProgramId=ProgramId ,
                                ProgramName=programListDB.Where(x => x.Id==ProgramId).Select(p => p.Name).FirstOrDefault() ,
                                statusOfSubject="منتهية" ,
                                doctorsName=memberAdminDB.Where(X => X.CompetitionId==assinsubjest.CompetitionId).Select(y => y.HrUser.FirstName+" "+y.HrUser.MiddleName).ToList() ,
                                capacityofCompetition=assinsubjest.Competition.Capacity ,
                                // StudentsNum=AllcompetitionuserIds.Count() ,
                            }).ToList();

                        }
                        if(userIsStudent)
                        {
                            var AllcompetitionuserIds = _unitOfWork.CompetitionUsers.FindAll(x => x.HrUserId == userId && x.FinishStatus == "Finished" && x.EnrollmentStatus != "suspended" && x.EnrollmentStatus != "pending" && x.EnrollmentStatus != "reject" && x.DelayOrWithdrawalStatus != "delay" && x.DelayOrWithdrawalStatus != "Withdraw").Select(a => a.CompetitionId);
                            var AssignSubjectList = _unitOfWork.AssignedSubjects.FindAll(x => AllcompetitionuserIds.Contains(x.CompetitionId), new[] { "Specialdept", "Academiclevel", "Competition" });
                            var ResultControlForProgramsDB = _unitOfWork.ResultControlForPrograms.FindAll(x=>x.Id > 0);
                            var ResultControlDB = _unitOfWork.ResultControls.FindAll(x=>x.HrUserId == userId);


                            var  Gpa =ResultControlDB.Where(x => x.CompetitionId== 5).Select(y => y.Gpa).FirstOrDefault() ;


                            CompetitionList=AssignSubjectList.Where(x => x.ProgrammId==ProgramId&&x.AcademicYearId==TermId).Select(assinsubjest => new FinishCompetitionVM
                            {
                                competitionId=assinsubjest.CompetitionId ,
                                Name=assinsubjest.Competition.Name ,
                                ImagePath=assinsubjest.Competition.ImagePath!=null ? BaseURL+assinsubjest.Competition.ImagePath : null ,
                                LevelName=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.Academiclevel.Name).FirstOrDefault() ,
                                LevelId=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.AcademiclevelId).FirstOrDefault() ,
                                SpecialDeptName=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.Specialdept.Name).FirstOrDefault() ,
                                SpecialDeptId=AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.SpecialdeptId).FirstOrDefault() ,
                                DeptName=Deptlist.Where(y => y.Id==(AssignSubjectList.Where(x => x.CompetitionId==assinsubjest.CompetitionId&&x.AcademicYearId==TermId&&x.ProgrammId==ProgramId).Select(y => y.Specialdept?.DeptartmentId).FirstOrDefault())).Select(w => w.Name).FirstOrDefault() ,
                                ProgramId=ProgramId ,
                                ProgramName=programListDB.Where(x => x.Id==ProgramId).Select(p => p.Name).FirstOrDefault() ,
                                statusOfSubject="منتهية" ,
                                Gpa=ResultControlDB.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.Gpa).FirstOrDefault() ,
                                GeneralGrade=ResultControlDB.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.GeneralGrade).FirstOrDefault() ,
                                Grade=ResultControlDB.Where(x => x.CompetitionId==assinsubjest.CompetitionId).Select(y => y.Grade).FirstOrDefault() ,
                                doctorsName=memberAdminDB.Where(X => X.CompetitionId==assinsubjest.CompetitionId).Select(y => y.HrUser?.FirstName+" "+y.HrUser?.MiddleName).ToList() ,

                            }).ToList();
                        }

                    }
                    var usersPagedList = PagedList<FinishCompetitionVM>.Create(  CompetitionList.AsQueryable() ,PageNo, NoOfItems);


                    Response.Result=true;
                    Response.Data=usersPagedList;


                    Response.PaginationHeader=new PaginationHeader
                    {
                        CurrentPage=PageNo ,
                        TotalPages=usersPagedList.TotalPages ,
                        ItemsPerPage=NoOfItems ,
                        TotalItems=usersPagedList.TotalCount
                    };

                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }




    }

}
