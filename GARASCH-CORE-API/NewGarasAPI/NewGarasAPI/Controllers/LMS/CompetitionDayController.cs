using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.LMS;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.LMS;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.LMS;
using System.Web;

namespace NewGarasAPI.Controllers.LMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionDayController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthLMsService _authService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResultControlService _ResultControlService;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private IPermissionService _permissionService;
        private new List<string> _allowedResourcesExtenstions = new List<string> { ".pdf", ".docs" };
        private long _maxAllowedPosterSize = 15728640;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        public CompetitionDayController(IAuthLMsService authService , IUnitOfWork unitOfWork , IMapper mapper ,
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
            _authService=authService;
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


        [Authorize]
        //[Authorize(Roles = "admin")]
        [HttpPost("CreateCompetitionDayResource")]
        public async Task<IActionResult> CreateCompetitionDayResourceAsync([FromForm] CompetitionDayResourceCreateDTO dto)
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

                    var checkCompetitionDayIsValid = await _unitOfWork.CompetitionDays.FindAsync((x => x.Id == dto.CompetitionDayId));
                    if(checkCompetitionDayIsValid==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="هذا اليوم غير موجود بالمسابقة ارجو التاكد منه اولا";
                        Response.Errors.Add(error);
                        return BadRequest(Response);


                    }
                    if(!await _permissionService.CheckUserHasPermissionManageCompetition(dto.HrUserId , checkCompetitionDayIsValid.CompetitionId))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="ليس لديك صلاحية لاضافة يوم فى هذه المسابقه ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    int CompetitionDayResourceId = dto.Id;
                    if(CompetitionDayResourceId==0)
                    {
                        // check if Already Exist Resource For this Day
                        var checkCompetitionDayResourcesIsExist = await _unitOfWork.CompetitionDayResources.FindAsync((x => x.CompetitionDayId == dto.CompetitionDayId));
                        if(checkCompetitionDayResourcesIsExist!=null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.errorMSG=" بالفعل تم اضافة مصادر الى هذا اليوم يمكنك التعديل به فقط";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }

                    dto.bookName=HttpUtility.UrlDecode(dto.bookName);
                    dto.fromChapterName=HttpUtility.UrlDecode(dto.fromChapterName);
                    dto.toChapterName=HttpUtility.UrlDecode(dto.toChapterName);


                    var _competitionDayResource = _mapper.Map<CompetitionDayResource>(dto);
                    if(CompetitionDayResourceId==0)
                    {
                        //_competitionDayResource.PDFLink1 = FilePathDB;
                        _competitionDayResource.CreationBy="system";
                        _competitionDayResource.CreationDate=DateTime.Now;
                        await _unitOfWork.CompetitionDayResources.AddAsync(_competitionDayResource);
                    }
                    else
                    {
                        // Get existing entity by id (Example)
                        _competitionDayResource=await _unitOfWork.CompetitionDayResources.GetByIdAsync(CompetitionDayResourceId);
                        if(_competitionDayResource==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.errorMSG="Invalid competition Day Resource Id";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                        //_competitionDayResource.PDFLink1 = FilePathDB;
                        _mapper.Map<CompetitionDayResourceCreateDTO , CompetitionDayResource>(dto , _competitionDayResource);

                        _unitOfWork.CompetitionDayResources.Update(_competitionDayResource);
                    }

                    _unitOfWork.Complete();
                }
                //dto.Id = _competitionDayResource.Id;
                return Ok(Response);
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

        [Authorize]
        [HttpGet("GetCompetitionDayResource")]
        public async Task<IActionResult> GetCompetitionDayResourceAsync([FromHeader] int CompetitionDayId)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=false;
            Response.Errors=new List<Error>();
            var data = new List<CompetitionDTO>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                var dto = new CompetitionDayResourceDTO ();
                if(Response.Result)
                {
                    var checkCompetitionDayIsValid = await _unitOfWork.CompetitionDays.FindAsync((x => x.Id == CompetitionDayId));
                    if(checkCompetitionDayIsValid==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="خطاء بيوم المسابقة يرجى التاكد منه";
                        Response.Errors.Add(error);
                        return BadRequest(Response);


                    }
                    // check if Already Exist Resource For this Day
                    var checkCompetitionDayResourcesIsExist = await _unitOfWork.CompetitionDayResources.FindAsync((x => x.CompetitionDayId == CompetitionDayId));
                    if(checkCompetitionDayResourcesIsExist==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="لا توجد اى مصادر تم اضافتها الى هذا اليوم";
                        Response.Errors.Add(error);
                        return BadRequest(Response);


                    }

                    dto=_mapper.Map<CompetitionDayResourceDTO>(checkCompetitionDayResourcesIsExist);


                }
                Response.Result=true;
                return Ok(dto);
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

        [HttpGet("GetByIdIncludeResourcesForAttendance")]
        public async Task<IActionResult> GetByIdIncludeResourcesForAttendance([FromHeader] int competitionDayid , [FromHeader] string userId)
        {
            BaseResponseWithData<CompetitionDayResourceByIdDTO2> Response = new BaseResponseWithData<CompetitionDayResourceByIdDTO2>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var data = new CompetitionDayResourceByIdDTO2();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {

                    var competitionIdsForCompetitionDays = _unitOfWork.CompetitionDays.FindAll(x=>x.Id == competitionDayid).Select(y=>y.CompetitionId);
                    var checkuser = await _permissionService.CheckUserHasPermissionManageCompetitionDay(userId, competitionDayid);
                    if(checkuser)
                    {
                        data.numberOfAttendce=_unitOfWork.CompetitionDayUsers.FindAll(x => x.Attendance==true&&x.CompetitionDayId==competitionDayid).Count();
                        data.NumberOfStudents=_unitOfWork.CompetitionUsers.FindAll(x => competitionIdsForCompetitionDays.Contains(x.CompetitionId)&&x.EnrollmentStatus=="approved"&&x.DelayOrWithdrawalStatus!="delay"&&x.DelayOrWithdrawalStatus!="Withdraw").Count();
                        data.competitionDayid=competitionDayid;
                        // Step 1: Get the HallId from CompetitionDays
                        var hallId = _unitOfWork.CompetitionDays
                                     .FindAll(x => x.Id == competitionDayid)
                                                             .Select(r => r.HallId)
                                                                                 .FirstOrDefault();

                        // Step 2: Get the Hall Name using the HallId
                        data.hallName=_unitOfWork.Halls
                            .FindAll(x => x.Id==hallId)
                            .Select(Y => Y.Name)
                            .FirstOrDefault();
                    }
                    var competitionDayIncludeResources =  _unitOfWork.CompetitionDays.FindAll((x => x.Id == competitionDayid ), new[] { "CompetitionDayResource" , "Lecturer" }).FirstOrDefault();
                    if(competitionDayIncludeResources==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="Invalid Competition Day Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }


                    data.CompetitionDayByIdDTO=_mapper.Map<CompetitionDayByIdDTO>(competitionDayIncludeResources);
                    if(data.CompetitionDayByIdDTO.lecturerId>0)
                    {
                        data.CompetitionDayByIdDTO.LectureName=_unitOfWork.HrUsers.FindAll(x => x.Id==data.CompetitionDayByIdDTO.lecturerId).Select(Y => Y.FirstName+" "+Y.MiddleName).FirstOrDefault();

                    }
                    if(data.CompetitionDayByIdDTO.HallId!=null)
                    {
                        data.CompetitionDayByIdDTO.HallName=_unitOfWork.Halls.FindAll(x => x.Id==data.CompetitionDayByIdDTO.HallId).Select(Y => Y.Name).FirstOrDefault();

                    }
                    data.CompetitionDayRescorcesByIdDTO=_mapper.Map<CompetitionDayRescorcesByIdDTO>(competitionDayIncludeResources.CompetitionDayResources);
                    Response.Data=data;
                }
                return Ok(Response);

            }
            catch(Exception ex)
            {

                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.errorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [Authorize]

        [HttpGet("CompetitionDayMidTermAllUsersAndFilterUsers")]                                     //new controll
        public async Task<IActionResult> CompetitionDayMidTermAllUsersAndFilterUsers([FromHeader] int competitionId ,
                                                                                  [FromHeader] int competitionDayId , [FromHeader] string? NameOfUser ,
                                                                                  [FromHeader] long userId , [FromHeader] int PageNo = 1 , [FromHeader] int NoOfItems = 20)
        {
            BaseResponseWithDataAndHeader<MidTermDto> Response = new BaseResponseWithDataAndHeader<MidTermDto>();

            Response.Result=true;
            Response.Errors=new List<Error>();
            var Alldata = new MidTermDto();
            var missionUsers = new List<MidTermUser>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    string encodedNameOfUser = HttpUtility.UrlDecode(NameOfUser);

                    if(!await _permissionService.CheckUserHasPermissionManageCompetition(userId , competitionId))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="ليس لديك صلاحية  فى هذه المادة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    //var UploadFileDB = _unitOfWork.UploadFilebyStudent.FindAll(x => x.CompetitionDayId == competitionDayId).ToList();
                    var competitionDayUserDB = _unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId == competitionDayId).ToList();
                    // var UserslistDb = _unitOfWork.CompetitionUsers.FindAll(x => x.CompetitionId == competitionId, new[] { "ApplicationUser" }).Select(y => y.ApplicationUser).AsQueryable();
                    var UserslistDb = Enumerable.Empty<HrUser>().AsQueryable();

                    if(NameOfUser!=null)
                    {
                        UserslistDb=_unitOfWork.CompetitionUsers.FindAll(x => (x.HrUser.FirstName+" "+x.HrUser.MiddleName+" "+x.HrUser.LastName).Contains(encodedNameOfUser)&&x.CompetitionId==competitionId
                                                                              , new [] { "HrUser" }).Select(y => y.HrUser).AsQueryable();

                    }
                    else
                    {
                        UserslistDb=_unitOfWork.CompetitionUsers.FindAllQueryable(x => x.CompetitionId==competitionId , new [] { "HrUser" }).Select(y => y.HrUser);

                    }

                    Alldata.DateOfmidTerm=_unitOfWork.CompetitionDays.FindAll(x => x.Id==competitionDayId).Select(y => y.From).FirstOrDefault();
                    Alldata.NumberOfStudent=UserslistDb.Count();
                    Alldata.FromScore=_unitOfWork.CompetitionDays.FindAll(x => x.Id==competitionDayId).Select(y => y.FromScore).FirstOrDefault()??0;
                    var usersPagedList = PagedList<HrUser>.Create(UserslistDb,PageNo, NoOfItems);

                    foreach(var item in usersPagedList)
                    {
                        var missionuser = new MidTermUser();

                        missionuser.userId=item.Id;
                        missionuser.userName=item.FirstName+" "+item.MiddleName+" "+item.LastName;
                        missionuser.Date=competitionDayUserDB.Where(X => X.HrUserId==item.Id).Select(y => y.CreationDate).FirstOrDefault();
                        //missionuser.DateOfupload = UploadFileDB.Where(x => x.UserId == item.Id).Select(y => y.DateTime).FirstOrDefault();
                        missionuser.status=competitionDayUserDB.Where(X => X.HrUserId==item.Id).Select(y => y.UserScore).FirstOrDefault()!=null ? "تم الرصد" : "لم يتم الرصد ";
                        missionuser.degree=competitionDayUserDB.Where(x => x.HrUserId==item.Id).Select(y => y.UserScore).FirstOrDefault();
                        missionUsers.Add(missionuser);
                    }
                    Response.PaginationHeader=new PaginationHeader
                    {
                        CurrentPage=PageNo ,
                        TotalPages=usersPagedList.TotalPages ,
                        ItemsPerPage=NoOfItems ,
                        TotalItems=usersPagedList.TotalCount
                    };
                    Alldata.userlist=missionUsers;
                    Response.Data=Alldata;
                }
                return Ok(Response);

            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.errorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);

            }

        }

        [Authorize]
        [HttpGet("CompetitionDayMissionForDoctorAndStudent")]                                     //new controll
        public async Task<IActionResult> CompetitionDayMissionForDoctorAndStudent([FromHeader] int competitionId , [FromHeader] int competitionDayId , [FromHeader] long userId)
        {
            BaseResponseWithData<MissionViewModel> Response = new BaseResponseWithData<MissionViewModel>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var Alldata = new MissionViewModel();
            var userIsStudent = false;
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    if(userId!=null&&competitionId!=0)
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

                        userIsStudent=(userRoles.Any()&&userRoles.Contains("student"));
                    }
                    if(userIsStudent)
                    {
                        var UploadFileDB = _unitOfWork.UploadFilebyStudent.FindAll(x => x.CompetitionDayId == competitionDayId).ToList();
                        var competitionDayUserDB = _unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId == competitionDayId).ToList();

                        var missionuser = new MissionViewModel();

                        missionuser.DateOfupload=UploadFileDB.Where(x => x.HrUserId==userId).Select(y => y.DateTime).FirstOrDefault();
                        missionuser.status=missionuser.DateOfupload!=null ? "تم الرفع " : "لم يتم الرفع";
                        missionuser.degree=competitionDayUserDB.Where(x => x.HrUserId==userId).Select(y => y.UserScore).FirstOrDefault();
                        missionuser.corrected=UploadFileDB.Where(x => x.HrUserId==userId).Select(y => y.Corrected).FirstOrDefault();
                        missionuser.ContentCompetitionDay=_unitOfWork.CompetitionDays.FindAll(x => x.Id==competitionDayId).Select(y => y.ContentCompetitionDay).FirstOrDefault();
                        missionuser.NumberOfStudent=null;
                        missionuser.pdfUrl=BaseURL+UploadFileDB.Where(x => x.HrUserId==userId).Select(y => y.Uploadfile).FirstOrDefault();
                        missionuser.comment=UploadFileDB.Where(x => x.HrUserId==userId).Select(y => y.Comment).FirstOrDefault();
                        Response.Result=true;
                        Response.Data=missionuser;
                        return Ok(Response);
                    }
                    else
                    {
                        if(!await _permissionService.CheckUserHasPermissionManageCompetition(userId , competitionId))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.errorMSG="ليس لديك صلاحية  فى هذه المادة ";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                        var UploadFileDB = _unitOfWork.UploadFilebyStudent.FindAll(x => x.CompetitionDayId == competitionDayId).ToList();
                        Alldata.Date=_unitOfWork.CompetitionDays.FindAll(x => x.Id==competitionDayId).Select(y => y.To).FirstOrDefault();
                        Alldata.startDate=_unitOfWork.CompetitionDays.FindAll(x => x.Id==competitionDayId).Select(y => y.From).FirstOrDefault();
                        Alldata.NumberOfStudent=UploadFileDB.Count();
                        Alldata.ContentCompetitionDay=_unitOfWork.CompetitionDays.FindAll(x => x.Id==competitionDayId).Select(y => y.ContentCompetitionDay).FirstOrDefault();
                        Response.Result=true;
                        Response.Data=Alldata;
                    }
                }
                return Ok(Response);

            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.errorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);

            }

        }

        [Authorize]
        [HttpGet("FilterTableByDurationHalls")]                                     //new controll
        public async Task<IActionResult> FilterTableByDurationHalls([FromHeader] FilterTableViewModel dto)
        {
            BaseResponseWithData<List<FilterTabledDto>> Response = new BaseResponseWithData<List<FilterTabledDto>>
            {
                Data = new List<FilterTabledDto>(),
                Result = true,
                Errors = new List<Error>()
            };
            var competitionIds = new List<int>();
            var competitionDayFilter = new List<FilterTabledDto>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var userIsStudent = false;
                    var userIsAdmin = false;

                    if(dto.userId!=null)
                    {
                        var _user =  _unitOfWork.HrUsers.FindAll(x=>x.Id == dto.userId).FirstOrDefault();
                        if(_user==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="هذا المستخدم غير موجود";
                            Response.Errors.Add(error);

                        }

                        var userRoles = _unitOfWork.UserRoles.FindAllQueryable(x=>x.Id > 0 ,new[]{"Role"}).Select(n=>n.Role.Name);

                        userIsStudent=(userRoles.Any()&&userRoles.Contains("student"));
                        userIsStudent=(userRoles.Any()&&userRoles.Contains("admin"));
                    }



                    var competitionDays = _unitOfWork.CompetitionDays.FindAll(x=>x.Id > 0);

                    if(dto.StartTime!=null&&dto.EndTime!=null)
                    {
                        competitionDays=competitionDays.Where(x => x.From.Date>=dto.StartTime?.Date&&x.To.Date<=dto.EndTime?.Date);
                    }
                    else
                    {
                        competitionDays=competitionDays.Where(x => ((DateTime)x.From).Date==((DateTime)dto.TimeToday).Date);
                    }

                    if(dto.CompetitionId>0)
                    {
                        competitionDays=competitionDays.Where(x => x.CompetitionId==dto.CompetitionId);
                    }
                    if(dto.TypeId>0)
                    {
                        competitionDays=competitionDays.Where(x => x.TypeId==dto.TypeId);
                        //  هذا الرقم متفق عليه لايجاد الامتحانات 
                        if(dto.TypeId==56)
                        {
                            competitionDays=competitionDays.Where(x => x.TypeId==5||x.TypeId==6);

                        }
                    }

                    if(dto.HallId>0)
                    {
                        competitionDays=competitionDays.Where(x => x.HallId==dto.HallId);
                    }
                    if(dto.SpecialDeptId>0&&dto.LevelId==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="يجب ادخال رقم الدفعة  ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    if(dto.SpecialDeptId==null&&dto.LevelId>0)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="يجب ادخال رقم القسم  ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    if(dto.SpecialDeptId>0&&dto.LevelId>0)
                    {

                        if(userIsStudent)
                        {

                            var checkUserSpecialDeptAndLevel = _unitOfWork.UserDepartment.FindAll(x => x.HrUserId == dto.userId).FirstOrDefault();
                            if(!(checkUserSpecialDeptAndLevel.SpecialdeptId==dto.SpecialDeptId&&checkUserSpecialDeptAndLevel.AcademiclevelId==dto.LevelId))
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="ليس لديك صلاحية لهذة الاقسام ";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }
                            competitionIds=_unitOfWork.AssignedSubjects
                        .FindAll(x => x.SpecialdeptId==dto.SpecialDeptId&&x.AcademiclevelId==dto.LevelId)
                        .Select(x => x.CompetitionId)
                        .ToList();
                            competitionDays=competitionDays.Where(x => competitionIds.Contains((int)x.CompetitionId));
                        }

                        if(userIsAdmin)
                        {
                            competitionIds=_unitOfWork.AssignedSubjects
                           .FindAll(x => x.SpecialdeptId==dto.SpecialDeptId&&x.AcademiclevelId==dto.LevelId)
                           .Select(x => x.CompetitionId)
                           .ToList();
                            competitionDays=competitionDays.Where(x => competitionIds.Contains((int)x.CompetitionId));
                        }
                        if(!userIsAdmin&&!userIsStudent)
                        {
                            var competitionIdsForAdmin = _unitOfWork.CompetitionMemberAdmins
                                                         .FindAll(x => x.HrUserId == dto.userId )
                                                         .Select(y => y.CompetitionId);
                            var assignedSubjects = await _unitOfWork.AssignedSubjects.FindAllAsync(x => competitionIdsForAdmin.Contains(x.CompetitionId));
                            var adminspecialdeptIds = assignedSubjects.Select(x => x.SpecialdeptId);
                            var admimlevelIds = assignedSubjects.Select(x => x.AcademiclevelId);
                            if(!(adminspecialdeptIds.Contains((int)dto.SpecialDeptId)&&admimlevelIds.Contains((int)dto.LevelId)))
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="ليس لديك صلاحية لهذة الاقسام والدفعات ";
                                Response.Errors.Add(error);
                                return BadRequest(Response);


                            }

                            competitionIds=_unitOfWork.AssignedSubjects
                                      .FindAll(x => x.SpecialdeptId==dto.SpecialDeptId&&x.AcademiclevelId==dto.LevelId&&competitionIdsForAdmin.Contains((int)x.CompetitionId))
                                      .Select(x => x.CompetitionId)
                                      .ToList();
                            competitionDays=competitionDays.Where(x => competitionIds.Contains((int)x.CompetitionId));
                        }
                    }
                    else
                    {
                        if(userIsStudent)
                        {

                            //    var checkUserSpecialDeptAndLevel = _unitOfWork.UserDepartment.FindAll(x => x.UserId == dto.userId).FirstOrDefault();

                            //    competitionIds=_unitOfWork.AssignedSubjects
                            //.FindAll(x => x.SpecialdeptId==checkUserSpecialDeptAndLevel.SpecialdeptId&&x.AcademiclevelId==checkUserSpecialDeptAndLevel.AcademiclevelId)
                            //.Select(x => x.CompetitionId)
                            //.ToList();
                            var competitionUserIds = _unitOfWork.CompetitionUsers.FindAll(x => x.HrUserId == dto.userId).Select(y => y.CompetitionId);
                            competitionDays=competitionDays.Where(x => competitionUserIds.Contains((int)x.CompetitionId));
                        }

                        if(!(userIsAdmin||userIsStudent))
                        {
                            var competitionIdsForAdmin = _unitOfWork.CompetitionMemberAdmins
                                                         .FindAll(x => x.HrUserId == dto.userId )
                                                         .Select(y => y.CompetitionId);
                            //var assignedSubjects = await _unitOfWork.AssignedSubjects.FindAllAsync(x => competitionIdsForAdmin.Contains(x.CompetitionId));
                            //var adminspecialdeptIds = assignedSubjects.Select(x => x.SpecialdeptId);
                            //var admimlevelIds = assignedSubjects.Select(x => x.AcademiclevelId);
                            //if(!(adminspecialdeptIds.Contains((int)dto.SpecialDeptId)&&admimlevelIds.Contains((int)dto.LevelId)))
                            //{
                            //    response.Result=false;
                            //    response.Errors.Add("ليس لديك صلاحية لهذة الاقسام والدفعات ");
                            //    return Ok(response);

                            //}

                            //competitionIds=_unitOfWork.AssignedSubjects
                            //          .FindAll(x => (admimlevelIds.Contains(x.AcademiclevelId)) && adminspecialdeptIds.Contains(x.SpecialdeptId))
                            //          .Select(x => x.CompetitionId)
                            //          .ToList();
                            competitionDays=competitionDays.Where(x => competitionIdsForAdmin.Contains((int)x.CompetitionId));
                        }
                    }

                    var halls = _unitOfWork.Halls.FindAll(x=>x.Id>0).ToList();
                    var assignment = _unitOfWork.AssignedSubjects.FindAll(x=>x.Id > 0,new []{"Academiclevel","Specialdept","Competition","Programm"});
                    var AdminCompetition  = _unitOfWork.CompetitionMemberAdmins.FindAll(x=>x.Id > 0,new []{"ApplicationUser"});
                    var specialdeptList  = _unitOfWork.Specialdepts.FindAll(x=>x.Id > 0 ,new []{"Deptartment"});
                    if(competitionDays.Count()>0)
                    {
                        competitionDayFilter=competitionDays
                            .Select(com => new FilterTabledDto
                            {
                                CompetitionDayId=com.Id ,
                                Name=com.Name ,
                                NameCompetition=assignment.Where(x => x.CompetitionId==com.CompetitionId).Select(w => w.Competition.Name).FirstOrDefault() ,
                                levelName=assignment.Where(x => x.CompetitionId==com.CompetitionId).Select(w => w.Academiclevel.Name).FirstOrDefault() ,

                                DeptName=specialdeptList.
                                Where(x => x.Id==(assignment.Where(x => x.CompetitionId==com.CompetitionId).Select(w => w.Specialdept.Id).FirstOrDefault())).Select(w => w.Deptartment.Name).FirstOrDefault() ,
                                specialDeptName=assignment.Where(x => x.CompetitionId==com.CompetitionId).Select(w => w.Specialdept.Name).FirstOrDefault() ,
                                DoctorName=AdminCompetition.Where(x => x.CompetitionId==com.CompetitionId&&x.RoleName=="doctor").
                                               Select(y => y.HrUser.FirstName+" "+y.HrUser.MiddleName).FirstOrDefault() ,
                                hallName=halls.Where(x => x.Id==com.HallId).Select(y => y.Name).FirstOrDefault() ,
                                From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
                                To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,

                                lecturerName=(_unitOfWork.HrUsers.FindAll(x => x.Id==com.LecturerId).Select(y => y.FirstName).FirstOrDefault()+" "+_unitOfWork.HrUsers.FindAll(x => x.Id==com.LecturerId).Select(y => y.MiddleName).FirstOrDefault()+" "+_unitOfWork.HrUsers.FindAll(x => x.Id==com.LecturerId).Select(y => y.LastName).FirstOrDefault())??null ,
                                ProgramName=assignment.Where(x => x.CompetitionId==com.CompetitionId).Select(w => w.Programm?.Name).FirstOrDefault()??null ,
                                TypeId=com.TypeId ,
                            })
                            .ToList();
                    }

                    Response.Data=competitionDayFilter;
                }
                return Ok(Response);


            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.errorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [Authorize]
        [HttpPost("UploadFilebyStudents")]                                     //new controll
        public async Task<IActionResult> UploadFilebyStudents([FromForm] UploadFilebyStudentsDto dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var data = new List<FilterTabledDto>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var uploadDb = _unitOfWork.UploadFilebyStudent.FindAll(x => x.HrUserId == dto.UserId && x.CompetitionDayId == dto.CompetitionDayId).FirstOrDefault();

                    string ImagePath = null;

                    if(dto.Uploadfile!=null)
                    {
                        if(!_allowedResourcesExtenstions.Contains(Path.GetExtension((dto.Uploadfile.FileName).ToLower())))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.errorMSG="Only .pdf or .docs files are allowed!";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                        if((dto.Uploadfile.Length>_maxAllowedPosterSize))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.errorMSG="Max allowed size for Cover Image greater than 10MB!";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                        string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + dto.Uploadfile.FileName.Trim().Replace(" ", "");
                        ImagePath="UploadFilebyStudents/";
                        string SaveImagePath = Path.Combine(this._environment.WebRootPath, ImagePath);
                        if(!System.IO.Directory.Exists(SaveImagePath))
                        {
                            System.IO.Directory.CreateDirectory(SaveImagePath); //Create directory if it doesn't exist
                        }
                        SaveImagePath=SaveImagePath+FileName;
                        using FileStream fileStream = new(SaveImagePath, FileMode.Create);
                        ImagePath="/"+ImagePath+FileName;
                        dto.Uploadfile.CopyTo(fileStream);
                        dto.uploadfile=ImagePath;
                    }
                    var timeNow = _authService.TimeZoneEgypt();
                    var competitionDay =  _unitOfWork.CompetitionDays.FindAll((x => x.Id == dto.CompetitionDayId)).FirstOrDefault();
                    if(competitionDay.To<timeNow)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="لقد نفذ الوقت لهذة العملية";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }


                    if(uploadDb==null)
                    {

                        _unitOfWork.UploadFilebyStudent.Add(new UploadFilebyStudent
                        {
                            HrUserId=dto.UserId ,
                            CompetitionDayId=dto.CompetitionDayId ,
                            Uploadfile=dto.uploadfile ,
                            Active=dto.Active ,
                            DateTime=_authService.TimeZoneEgypt()
                        });
                        _unitOfWork.Complete();
                    }
                    else
                    {
                        var oldPdf = uploadDb.Uploadfile;
                        uploadDb.Uploadfile=ImagePath;
                        _unitOfWork.UploadFilebyStudent.Update(uploadDb);
                        var pdfUpdateResponse = _unitOfWork.Complete();
                        if(pdfUpdateResponse>0)
                        {
                            if(!string.IsNullOrWhiteSpace(oldPdf)&&dto.Uploadfile!=null)
                            {
                                string DeleteOldpdf = this._environment.WebRootPath + oldPdf;
                                if(System.IO.File.Exists(DeleteOldpdf))
                                {
                                    System.IO.File.Delete(DeleteOldpdf);
                                }
                            }
                            //_unitOfWork.Complete();
                        }

                    }
                }
                Response.Result=true;
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.errorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);

            }

        }
        [HttpGet("CompetitionDayMissionAllUsersAndFilterUsers")]
        public async Task<IActionResult> CompetitionDayMissionAllUsersAndFilterUsers([FromHeader] int competitionId ,
                                                                                    [FromHeader] int competitionDayId , [FromHeader] bool subscrib ,
                                                                                    [FromHeader] long userId , [FromHeader] string? NameOfUser ,
                                                                                    [FromHeader] int PageNo = 1 , [FromHeader] int NoOfItems = 20)
        {
            BaseResponseWithDataAndHeader<MissionDto> Response = new BaseResponseWithDataAndHeader<MissionDto>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var missionUsers = new List<MissionUser>();
            var Alldata = new MissionDto();
            string encodedNameOfUser = HttpUtility.UrlDecode(NameOfUser);

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    if(!await _permissionService.CheckUserHasPermissionManageCompetition(userId , competitionId))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="ليس لديك صلاحية  فى هذه المادة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var UploadFileDB = _unitOfWork.UploadFilebyStudent.FindAll(x =>x.CompetitionDayId==competitionDayId).ToList();
                    var competitionDayUserDB = _unitOfWork.CompetitionDayUsers.FindAll(x =>x.CompetitionDayId==competitionDayId).ToList();
                    // var UserslistDb = _unitOfWork.CompetitionUsers.FindAllAsQueryable(x => x.CompetitionId == competitionId, new[] { "ApplicationUser" }).Select(y => y.ApplicationUser).AsQueryable();
                    var UserslistDb = Enumerable.Empty<HrUser>().AsQueryable();

                    if(NameOfUser!=null)
                    {
                        UserslistDb=_unitOfWork.CompetitionUsers.FindAllQueryable(x => (x.HrUser.FirstName+" "+x.HrUser.MiddleName+" "+x.HrUser.LastName).Contains(encodedNameOfUser)&&x.CompetitionId==competitionId
                                                                              , new [] { "HrUser" }).Select(y => y.HrUser);
                        if(subscrib==true)
                        {
                            var  UsersOfsubscrib=_unitOfWork.UploadFilebyStudent.FindAll(x => x.CompetitionDayId==competitionDayId
                            , new [] { "HrUser" }).Select(y => y.HrUser).AsQueryable();
                            UserslistDb=UserslistDb.Intersect(UsersOfsubscrib).AsQueryable();

                        }

                    }
                    else
                    {
                        UserslistDb=_unitOfWork.CompetitionUsers.FindAllQueryable(x => x.CompetitionId==competitionId , new [] { "HrUser" }).Select(y => y.HrUser);
                        if(subscrib==true)
                        {
                            var  UsersOfsubscrib=_unitOfWork.UploadFilebyStudent.FindAll(x => x.CompetitionDayId==competitionDayId
                            , new [] { "HrUser" }).Select(y => y.HrUser).AsQueryable();
                            UserslistDb=UserslistDb.Intersect(UsersOfsubscrib).AsQueryable();

                        }
                    }



                    Alldata.DateOfMission=_unitOfWork.CompetitionDays.FindAll(x => x.Id==competitionDayId).Select(y => y.From).FirstOrDefault();
                    Alldata.NumberOfStudent=UserslistDb.Count();
                    Alldata.FromScore=_unitOfWork.CompetitionDays.FindAll(x => x.Id==competitionDayId).Select(y => y.FromScore).FirstOrDefault()??0;



                    var usersPagedList = PagedList<HrUser>.Create(UserslistDb,PageNo, NoOfItems);
                    foreach(var item in usersPagedList)
                    {
                        var missionuser = new MissionUser();

                        missionuser.userId=item.Id;
                        missionuser.userName=item.FirstName+" "+item.MiddleName+" "+item.LastName;
                        missionuser.DateOfupload=UploadFileDB.Where(x => x.HrUserId==item.Id).Select(y => y.DateTime).FirstOrDefault();
                        missionuser.status=missionuser.DateOfupload!=null ? "تم الرفع " : "لم يتم الرفع";
                        missionuser.degree=competitionDayUserDB.Where(x => x.HrUserId==item.Id).Select(y => y.UserScore).FirstOrDefault();
                        var file = UploadFileDB.Where(x => x.HrUserId == item.Id).Select(y => y.Uploadfile).FirstOrDefault() ?? null;
                        missionuser.ShowFile=file!=null ? BaseURL+file : null;
                        missionuser.comment=UploadFileDB.Where(x => x.HrUserId==item.Id).Select(y => y.Comment).FirstOrDefault()??null;
                        missionUsers.Add(missionuser);
                    }

                    Alldata.userlist=missionUsers;
                    //    var userlist =missionUsers ;


                    Response.PaginationHeader=new PaginationHeader
                    {
                        CurrentPage=PageNo ,
                        TotalPages=usersPagedList.TotalPages ,
                        ItemsPerPage=NoOfItems ,
                        TotalItems=usersPagedList.TotalCount
                    };

                    Response.Data=Alldata;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.errorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }

        [Authorize]
        [HttpPost("AddResultMisionToUser")]                                     //new controll
        public async Task<IActionResult> AddResultMisionToUser(AddResultMissionViewModel dto , [FromHeader] long userId)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    if(!await _permissionService.CheckUserHasPermissionManageCompetition(userId , dto.competitionId))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="ليس لديك صلاحية  فى هذه المادة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var competitionDay = _unitOfWork.CompetitionDays.FindAll(x => x.Id == dto.competitionDayId ).FirstOrDefault();
                    if(competitionDay==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="هذة المسابقة غير موجودة";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    if(dto.Degree==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="يجب ادخال رقم ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var competitionDayuser = _unitOfWork.CompetitionDayUsers.FindAll(c => c.CompetitionDayId == dto.competitionDayId && c.HrUserId == dto.UserIdForStudent).FirstOrDefault();



                    if(competitionDayuser==null)
                    {
                        _unitOfWork.CompetitionDayUsers.Add(new CompetitionDayUser
                        {
                            UserScore=dto.Degree ,
                            IsFinished=true ,
                            Attendance=true ,
                            HrUserId=dto.UserIdForStudent ,
                            CompetitionDayId=dto.competitionDayId ,
                            CreationDate=_authService.TimeZoneEgypt()

                        });
                    }
                    else
                    {
                        competitionDayuser.UserScore=dto.Degree;
                        competitionDayuser.IsFinished=true;
                        competitionDayuser.Attendance=true;
                        competitionDayuser.CreationDate=_authService.TimeZoneEgypt();
                        _unitOfWork.CompetitionDayUsers.Update(competitionDayuser);
                    }


                    var uploadfileuser = _unitOfWork.UploadFilebyStudent.FindAll(x => x.HrUserId == dto.UserIdForStudent && x.CompetitionDayId == dto.competitionDayId).FirstOrDefault();
                    if(uploadfileuser!=null)
                    {
                        uploadfileuser.Corrected=true;
                        uploadfileuser.Comment=dto.comment;

                        _unitOfWork.UploadFilebyStudent.Update(uploadfileuser);
                    }

                    var competitionUser = _unitOfWork.CompetitionUsers.FindAll(c => c.CompetitionId == dto.competitionId && c.HrUserId == dto.UserIdForStudent).FirstOrDefault();
                    if(competitionUser!=null)
                    {
                        competitionUser.TotalScore+=dto.Degree;
                        _unitOfWork.CompetitionUsers.Update(competitionUser);
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
                error.errorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }

        //[Authorize]
        [HttpPost("AddAndUpdateCompetitionDayInTable")]                                     //new controll
        public async Task<IActionResult> AddAndUpdateCompetitionDayInTable([FromHeader] long userId , [FromForm] CompetitionDayCreateDTO dto)
        {
            BaseResponseWithData<CompetitionDayCreateDTO> Response = new BaseResponseWithData<CompetitionDayCreateDTO>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var competitionDaysDB = _unitOfWork.CompetitionDays.FindAll(x => x.Id > 0);
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            Response.Errors=validation.errors;
            Response.Result=validation.result;


            if(dto.TypeId!=2&&dto.TypeId!=3)
            {
                try
                {
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

                        var userIsAdmin= (userRoles.Any()&&userRoles.Contains("admin"));
                        var userIsStudent= (userRoles.Any()&&userRoles.Contains("student"));
                        var userIsdoctor = (userRoles.Any()&&userRoles.Contains( "doctor"));
                        var userIsadminCompetition =  (userRoles.Any()&&userRoles.Contains( "adminCompetition"));


                        if(!(userIsAdmin||userIsdoctor||userIsadminCompetition))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.errorMSG="هذا المستخدم ليس له صلاحية";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                        if(userIsdoctor||userIsadminCompetition)
                        {
                            var competitionForAdmin = _unitOfWork.CompetitionMemberAdmins
                                                         .FindAll(x => x.HrUserId == _user.Id && x.CompetitionId == dto.CompetitionId)
                                                         .Any();
                            if(!competitionForAdmin)
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="ليس لديك صلاحية لهذة الاقسام والدفعات ";
                                Response.Errors.Add(error);
                                return BadRequest(Response);


                            }
                        }


                        var checkCompetitionsIsValid = await _unitOfWork.Competitions.FindAsync((x => x.Id == dto.CompetitionId), new[] { "CompetitionDay" });
                        if(checkCompetitionsIsValid==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.errorMSG="Invalid Competition Id";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }


                        int CompetitionDayId = dto.Id;
                        var _competitionDay = _mapper.Map<CompetitionDay>(dto);
                        var AssignedSubjectlistDB = _unitOfWork.AssignedSubjects.FindAll(x=>x.Id > 0);
                        var levelforcompetition = AssignedSubjectlistDB.Where(x=>x.CompetitionId == dto.CompetitionId).Select(y=>y.AcademiclevelId).FirstOrDefault();
                        var specialdeptforcompetition = AssignedSubjectlistDB.Where(x=>x.CompetitionId == dto.CompetitionId).Select(y=>y.SpecialdeptId).FirstOrDefault();
                        var competitionIdslist = AssignedSubjectlistDB.Where(x=>x.AcademiclevelId == levelforcompetition && x.SpecialdeptId == specialdeptforcompetition).
                    Select(y=>y.CompetitionId).ToList();
                        var hall = false;
                        var checkcompetitionDayReserved = competitionDaysDB.Where(x => x.From<=dto.To && x.To>=dto.From && competitionIdslist.Contains((int)x.CompetitionId)  && x.TypeId != 2 && x.TypeId != 3 ).Any();

                        if(CompetitionDayId==0)
                        {

                            // Comment by michael 2022
                            if(checkcompetitionDayReserved)
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG=" This competitionDay is already booked.";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }

                            if(dto.HallId!=null)
                            {
                                hall=competitionDaysDB.Where(x => x.HallId==dto.HallId&&x.From<=dto.To&&x.To>=dto.From).Any();

                            }


                            if(hall)
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG=" This hall is already booked.";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }

                            var totoaldegreeOfType = _unitOfWork.CompetitionTypes
                            .FindAll(x => x.CompetitionId == dto.CompetitionId && x.TypeId == dto.TypeId)
                            .Select(y => y.TotalScore).FirstOrDefault();
                            var checkTotalDegreeOfType = competitionDaysDB
                            .Where(x => x.CompetitionId == dto.CompetitionId && x.TypeId == dto.TypeId)
                            .Sum(y => y.FromScore);
                            if((decimal)totoaldegreeOfType<(checkTotalDegreeOfType+dto.FromScore))
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="مراجعة الدرجة المدخلة قد يكون اكبر من المسموح";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }

                            var newcompetitionDay = await _unitOfWork.CompetitionDays.AddAsync(_competitionDay);
                            _unitOfWork.Complete();

                            var Resources = new CompetitionDayResource();
                            if(dto.Resources?.bookId!=null||dto.Resources?.bookName!=null||dto.Resources?.youtubeLink!=null||dto.Resources?.youtubeLink2!=null||dto.Resources?.youtubeLink3!=null||dto.Resources?.PDFLink1!=null||dto.Resources?.PDFLink2!=null||dto.Resources?.PDFLink3!=null)
                            {
                                var _competitionRescours = _mapper.Map<CompetitionDayResource>(dto.Resources);
                                _competitionRescours.CompetitionDayId=newcompetitionDay.Id;
                                await _unitOfWork.CompetitionDayResources.AddAsync(_competitionRescours);
                                _unitOfWork.Complete();
                            }
                        }


                        else
                        {
                            // Get existing entity by id (Example)
                            _competitionDay=await _unitOfWork.CompetitionDays.GetByIdAsync(CompetitionDayId);
                            var checkcompetitionDayReservedUpdate =_unitOfWork.CompetitionDays.FindAll(x => x.From<=dto.To && x.To>=dto.From && x.Id != dto.Id && competitionIdslist.Contains((int)x.CompetitionId) && x.TypeId != 2 && x.TypeId != 3).Any();
                            // Comment By Michael 2025-2-12
                            if(checkcompetitionDayReservedUpdate)
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG=" This competitionDay is already booked.";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }
                            var hallupdate = _unitOfWork.CompetitionDays.FindAll(x => x.HallId == dto.HallId && x.From <= dto.To && x.To >= dto.From && x.Id != dto.Id).Any();
                            if(hallupdate)
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG=" This hall is already booked.";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }

                            if(_competitionDay==null)
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="Invalid competition Day Id";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }

                            _mapper.Map(dto , _competitionDay);

                            var totoaldegreeOfType = _unitOfWork.CompetitionTypes
                            .FindAll(x => x.CompetitionId == dto.CompetitionId && x.TypeId == dto.TypeId)
                            .Select(y => y.TotalScore).FirstOrDefault();
                            var checkTotalDegreeOfType = competitionDaysDB
                            .Where(x => x.CompetitionId == dto.CompetitionId && x.TypeId == dto.TypeId)
                            .Sum(y => y.FromScore);
                            var checkDegreeOfCompetitionDAy = competitionDaysDB.Where(x=>x.Id == dto.Id).Select(a=>a.FromScore).FirstOrDefault();

                            if((decimal)totoaldegreeOfType<((checkTotalDegreeOfType-checkDegreeOfCompetitionDAy)+dto.FromScore))
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="مراجعة الدرجة المدخلة قد يكون اكبر من المسموح";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }
                            _unitOfWork.CompetitionDays.Update(_competitionDay);
                            var temp = _unitOfWork.CompetitionDayResources.FindAll(x => x.CompetitionDayId == dto.Id).Select(y => y.Id).FirstOrDefault();
                            var _competitionDayRescourse = await _unitOfWork.CompetitionDayResources.GetByIdAsync(temp);
                            if(_competitionDayRescourse!=null)
                            {
                                _unitOfWork.CompetitionDayResources.Delete(_competitionDayRescourse);
                                _unitOfWork.Complete();
                            }
                            if(dto.Resources?.bookId!=null||dto.Resources?.bookName!=null||dto.Resources?.youtubeLink!=null||dto.Resources?.youtubeLink2!=null||dto.Resources?.youtubeLink3!=null||dto.Resources?.PDFLink1!=null||dto.Resources?.PDFLink2!=null||dto.Resources?.PDFLink3!=null)
                            {
                                var _competitionRescours = _mapper.Map<CompetitionDayResource>(dto.Resources);
                                _competitionRescours.CompetitionDayId=dto.Id;
                                await _unitOfWork.CompetitionDayResources.AddAsync(_competitionRescours);
                            }
                        }

                        _unitOfWork.Complete();

                        dto.Id=_competitionDay.Id;
                    }
                    Response.Result=true;
                    Response.Data=dto;
                    return Ok(Response);
                }
                catch(Exception ex)
                {
                    Response.Result=false;
                    Error error = new Error();
                    error.ErrorCode="Err10";
                    string ErrorMSG = "Exception :" + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    Response.Errors.Add(error);
                    return BadRequest(Response);

                }
            }
            else
            {
                try
                {
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

                        var userIsAdmin= (userRoles.Any()&&userRoles.Contains("admin"));
                        var userIsdoctor = (userRoles.Any()&&userRoles.Contains( "doctor"));
                        var userIsadminCompetition =  (userRoles.Any()&&userRoles.Contains( "adminCompetition"));

                        if(!(userIsAdmin||userIsdoctor||userIsadminCompetition))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.errorMSG="هذا المستخدم ليس له صلاحية";
                            Response.Errors.Add(error);
                            return BadRequest(Response);
                        }
                        if(userIsdoctor||userIsadminCompetition)
                        {
                            var competitionForAdmin = _unitOfWork.CompetitionMemberAdmins
                                                         .FindAll(x => x.HrUserId == _user.Id && x.CompetitionId == dto.CompetitionId)
                                                         .Any();
                            if(!competitionForAdmin)
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="ليس لديك صلاحية لهذة الاقسام والدفعات ";
                                Response.Errors.Add(error);
                                return BadRequest(Response);



                            }
                        }
                        var checkCompetitionsIsValid = await _unitOfWork.Competitions.FindAsync((x => x.Id == dto.CompetitionId), new[] { "CompetitionDay" });
                        if(checkCompetitionsIsValid==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.errorMSG="Invalid Competition Id";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }


                        int CompetitionDayId = dto.Id;
                        var _competitionDay = _mapper.Map<CompetitionDay>(dto);
                        var AssignedSubjectlistDB = _unitOfWork.AssignedSubjects.FindAll(x=>x.Id > 0);
                        var levelforcompetition = AssignedSubjectlistDB.Where(x=>x.CompetitionId == dto.CompetitionId).Select(y=>y.AcademiclevelId).FirstOrDefault();
                        var specialdeptforcompetition = AssignedSubjectlistDB.Where(x=>x.CompetitionId == dto.CompetitionId).Select(y=>y.SpecialdeptId).FirstOrDefault();
                        var competitionIdslist = AssignedSubjectlistDB.Where(x=>x.AcademiclevelId == levelforcompetition && x.SpecialdeptId == specialdeptforcompetition).
                    Select(y=>y.CompetitionId).ToList();



                        if(CompetitionDayId==0)
                        {
                            var totoaldegreeOfType = _unitOfWork.CompetitionTypes
                            .FindAll(x => x.CompetitionId == dto.CompetitionId && x.TypeId == dto.TypeId)
                            .Select(y => y.TotalScore).FirstOrDefault();
                            var checkTotalDegreeOfType = competitionDaysDB
                            .Where(x => x.CompetitionId == dto.CompetitionId && x.TypeId == dto.TypeId)
                            .Sum(y => y.FromScore);
                            if((decimal)totoaldegreeOfType<(checkTotalDegreeOfType+dto.FromScore))
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="مراجعة الدرجة المدخلة قد يكون اكبر من المسموح";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }
                            var newcompetitionDay = await _unitOfWork.CompetitionDays.AddAsync(_competitionDay);
                            _unitOfWork.Complete();

                            var Resources = new CompetitionDayResource();
                            if(dto.Resources?.bookId!=null||dto.Resources?.bookName!=null||dto.Resources?.youtubeLink!=null||dto.Resources?.youtubeLink2!=null||dto.Resources?.youtubeLink3!=null||dto.Resources?.PDFLink1!=null||dto.Resources?.PDFLink2!=null||dto.Resources?.PDFLink3!=null)
                            {
                                var _competitionRescours = _mapper.Map<CompetitionDayResource>(dto.Resources);
                                _competitionRescours.CompetitionDayId=newcompetitionDay.Id;
                                await _unitOfWork.CompetitionDayResources.AddAsync(_competitionRescours);
                                _unitOfWork.Complete();
                            }
                        }

                        else
                        {
                            _competitionDay=await _unitOfWork.CompetitionDays.GetByIdAsync(CompetitionDayId);

                            if(_competitionDay==null)
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="Invalid competition Day Id";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }
                            var totoaldegreeOfType = _unitOfWork.CompetitionTypes
                            .FindAll(x => x.CompetitionId == dto.CompetitionId && x.TypeId == dto.TypeId)
                            .Select(y => y.TotalScore).FirstOrDefault();
                            var checkTotalDegreeOfType = competitionDaysDB
                            .Where(x => x.CompetitionId == dto.CompetitionId && x.TypeId == dto.TypeId)
                            .Sum(y => y.FromScore);
                            if((decimal)totoaldegreeOfType<(checkTotalDegreeOfType+dto.FromScore))
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="مراجعة الدرجة المدخلة قد يكون اكبر من المسموح";
                                Response.Errors.Add(error);
                                return BadRequest(Response);

                            }
                            _mapper.Map(dto , _competitionDay);

                            _unitOfWork.CompetitionDays.Update(_competitionDay);
                            var temp = _unitOfWork.CompetitionDayResources.FindAll(x => x.CompetitionDayId == dto.Id).Select(y => y.Id).FirstOrDefault();
                            var _competitionDayRescourse = await _unitOfWork.CompetitionDayResources.GetByIdAsync(temp);
                            if(_competitionDayRescourse!=null)
                            {
                                _unitOfWork.CompetitionDayResources.Delete(_competitionDayRescourse);
                                _unitOfWork.Complete();
                            }
                            if(dto.Resources?.bookId!=null||dto.Resources?.bookName!=null||dto.Resources?.youtubeLink!=null||dto.Resources?.youtubeLink2!=null||dto.Resources?.youtubeLink3!=null||dto.Resources?.PDFLink1!=null||dto.Resources?.PDFLink2!=null||dto.Resources?.PDFLink3!=null)
                            {
                                var _competitionRescours = _mapper.Map<CompetitionDayResource>(dto.Resources);
                                _competitionRescours.CompetitionDayId=dto.Id;
                                await _unitOfWork.CompetitionDayResources.AddAsync(_competitionRescours);
                            }
                        }

                        _unitOfWork.Complete();

                        dto.Id=_competitionDay.Id;
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
                    string ErrorMSG = "Exception :" + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    Response.Errors.Add(error);
                    return BadRequest(Response);

                }
            }


        }

        [Authorize]
        [HttpGet("CompetitionDayToDayForUserId")]                                     //new controll
        public async Task<IActionResult> CompetitionDayToDayForUserId([FromHeader] long HrUserIdForAdminister , [FromHeader] long HrUserIdForStudent , [FromHeader] DateTime? dateTime = null , [FromHeader] int typeId = 0)
        {
            BaseResponseWithData<List<FilterTabledDto>> Response = new BaseResponseWithData<List<FilterTabledDto>>();
            Response.Data=new List<FilterTabledDto>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var data = new List<FilterTabledDto>();
            bool userIsAdmin = false;
            bool userIsdoctor = false;
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var _user =  _unitOfWork.HrUsers.FindAll(x=>x.Id == HrUserIdForAdminister).FirstOrDefault();
                    if(_user==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا المستخدم غير موجود";
                        Response.Errors.Add(error);

                    }

                    var userRoles = _unitOfWork.UserRoles.FindAllQueryable(x=>x.Id > 0 ,new[]{"Role"}).Select(n=>n.Role.Name);

                    userIsAdmin=(userRoles.Any()&&userRoles.Contains("admin"));
                    userIsdoctor=(userRoles.Any()&&userRoles.Contains("doctor"));


                    if(userIsAdmin)
                    {
                        var competitions = await _unitOfWork.Competitions.FindAllAsync(x => x.Id > 0);
                        foreach(var item in competitions)
                        {

                            DateTime time = dateTime ?? DateTime.Today;
                            DateTime startTime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
                            DateTime endTime = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);

                            var competitionDayListDB = new List<CompetitionDay>();
                            if(typeId==0)
                            {
                                competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime).ToList();

                            }
                            else
                            {
                                competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime&&x.TypeId==typeId).ToList();

                            }
                            var competitiondayfilter = new List<FilterTabledDto>();
                            competitiondayfilter=competitionDayListDB

                                .Select(com => new FilterTabledDto
                                {
                                    CompetitionDayId=com.Id ,
                                    TypeId=com.TypeId ,
                                    Name=com.Name??null ,
                                    NameCompetition=item.Name ,
                                    From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
                                    To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
                                    NumberOfStudents=_unitOfWork.CompetitionUsers.FindAll(x => x.CompetitionId==item.Id).Count() ,
                                    NumberOfAttendce=_unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId==com.Id&&x.Attendance==true).Count()
                                }).ToList();
                            data.AddRange(competitiondayfilter);
                        }
                    }
                    else if(userIsdoctor)
                    {
                        var competitions =  _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.HrUserId == HrUserIdForAdminister , new[] { "Competition" }).Select(y => y.Competition).ToList();
                        foreach(var item in competitions)
                        {

                            DateTime time = dateTime ?? DateTime.Today;
                            DateTime startTime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
                            DateTime endTime = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);

                            var competitionDayListDB = new List<CompetitionDay>();
                            if(typeId==0)
                            {
                                competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime).ToList();

                            }
                            else
                            {
                                competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime&&x.TypeId==typeId).ToList();

                            }
                            var competitiondayfilter = new List<FilterTabledDto>();
                            competitiondayfilter=competitionDayListDB

                                .Select(com => new FilterTabledDto
                                {
                                    CompetitionDayId=com.Id ,
                                    TypeId=com.TypeId ,
                                    Name=com.Name??null ,
                                    NameCompetition=item.Name ,
                                    From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
                                    To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
                                    NumberOfStudents=_unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId==com.Id).Count() ,
                                    NumberOfAttendce=_unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId==com.Id&&x.Attendance==true).Count()
                                }).ToList();
                            data.AddRange(competitiondayfilter);
                        }
                    }
                    else
                    {
                        var competitions = _unitOfWork.CompetitionUsers.FindAll(x => x.HrUserId == HrUserIdForStudent, new[] { "Competition" }).Select(y => y.Competition).ToList();
                        foreach(var item in competitions)
                        {

                            DateTime time = dateTime ?? DateTime.Today;
                            DateTime startTime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
                            DateTime endTime = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);

                            var competitionDayListDB = new List<CompetitionDay>();
                            if(typeId==0)
                            {
                                competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime).ToList();

                            }
                            else
                            {
                                competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime&&x.TypeId==typeId).ToList();

                            }
                            var competitiondayfilter = new List<FilterTabledDto>();

                            competitiondayfilter=competitionDayListDB

                                .Select(com => new FilterTabledDto
                                {
                                    CompetitionDayId=com.Id ,
                                    TypeId=com.TypeId ,
                                    Name=com.Name??null ,
                                    NameCompetition=item.Name ,
                                    From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
                                    To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
                                    AttendanceFlag=_unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId==com.Id&&x.HrUserId==HrUserIdForStudent&&x.Attendance==true).Any()
                                }).ToList();
                            data.AddRange(competitiondayfilter);
                        }
                    }
                    Response.Result=true;
                    Response.Data=data;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                string ErrorMSG = "Exception :" + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);

            }

        }

        [HttpGet("FilterLecturesByDuration")]                                     //new controll
        public async Task<IActionResult> FilterLecturesByDuration([FromHeader] FilterTableViewModel dto)
        {
            BaseResponseWithData<List<FilterTabledDto3>> Response = new BaseResponseWithData<List<FilterTabledDto3>>
            {
                Data = new List<FilterTabledDto3>(),
                Result = true,
                Errors = new List<Error>()
            };
            var competitionIds = new List<int>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var userIsStudent = false;
                    var userIsAdmin = false;
                    var _user =  _unitOfWork.HrUsers.FindAll(x=>x.Id == dto.userId).FirstOrDefault();
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

                    userIsAdmin=(userRoles.Any()&&userRoles.Contains("admin"));
                    userIsStudent=(userRoles.Any()&&userRoles.Contains("student"));


                    var competitionDays = _unitOfWork.CompetitionDays.GetAll();
                    var competitions = _unitOfWork.Competitions.FindAll(x=>x.Id > 0 );

                    if(dto.StartTime!=null&&dto.EndTime!=null)
                    {
                        competitionDays=competitionDays.Where(x => x.From.Date>=dto.StartTime?.Date&&x.To.Date<=dto.EndTime?.Date&&x.TypeId==dto.TypeId);
                    }
                    else
                    {
                        competitionDays=competitionDays.Where(x => ((DateTime)x.From).Date==((DateTime)dto.TimeToday).Date&&x.TypeId==dto.TypeId);
                    }

                    if(dto.CompetitionId>0)
                    {
                        competitionDays=competitionDays.Where(x => x.CompetitionId==dto.CompetitionId);
                    }

                    if(dto.HallId>0)
                    {
                        competitionDays=competitionDays.Where(x => x.HallId==dto.HallId);
                    }

                    if(dto.SpecialDeptId>0&&dto.LevelId>0)
                    {
                        if(dto.SpecialDeptId==null||dto.LevelId==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            string ErrorMSG = "يجب ادخال رقم الدفعة والقسم للطالب ";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }



                        if(!userIsAdmin&&!userIsStudent)
                        {
                            var competitionIdsForAdmin = _unitOfWork.CompetitionMemberAdmins
                                                     .FindAll(x => x.HrUserId == dto.userId && x.CompetitionId == dto.CompetitionId)
                                                     .Select(y => y.CompetitionId);
                            var assignedSubjects = await _unitOfWork.AssignedSubjects.FindAllAsync(x => competitionIdsForAdmin.Contains(x.CompetitionId));
                            var adminspecialdeptIds = assignedSubjects.Select(x => x.SpecialdeptId);
                            var admimlevelIds = assignedSubjects.Select(x => x.AcademiclevelId);
                            if(!(adminspecialdeptIds.Contains((int)dto.SpecialDeptId)&&admimlevelIds.Contains((int)dto.LevelId)))
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                string ErrorMSG = "ليس لديك صلاحية لهذة الاقسام والدفعات ";
                                Response.Errors.Add(error);
                                return BadRequest(Response);


                            }

                            competitionIds=_unitOfWork.AssignedSubjects
                                        .FindAll(x => x.SpecialdeptId==dto.SpecialDeptId&&x.AcademiclevelId==dto.LevelId)
                                        .Select(x => x.CompetitionId)
                                        .ToList();
                            competitionDays=competitionDays.Where(x => competitionIds.Contains(x.CompetitionId));
                        }

                        else if(userIsStudent)
                        {
                            competitionIds=_unitOfWork.AssignedSubjects
                                       .FindAll(x => x.SpecialdeptId==dto.SpecialDeptId&&x.AcademiclevelId==dto.LevelId)
                                       .Select(x => x.CompetitionId)
                                       .ToList();
                            competitionDays=competitionDays.Where(x => competitionIds.Contains(x.CompetitionId));
                        }
                    }

                    var halls = _unitOfWork.Halls.FindAll(x => x.Id > 0).ToList();
                    var competitionUsers = _unitOfWork.CompetitionUsers.FindAll(x => x.Id > 0);
                    var CompetitionDayUsers = _unitOfWork.CompetitionDayUsers.FindAll(x => x.Id > 0);
                    var AllLevelDb = _unitOfWork.Academiclevels.FindAll(x=>x.Id > 0);
                    var AllSpecialDeptDb = _unitOfWork.Specialdepts.FindAll(x=>x.Id > 0 , new[]{"Deptartment" });
                    var AllUserDb = _unitOfWork.CompetitionMemberAdmins.FindAll(x=>x.Id != null , new[]{"HrUser"});
                    var AllassignSubjectDb = _unitOfWork.AssignedSubjects.FindAll(x=>x.Id > 0);
                    var competitionDayFilter = competitionDays
                        .Select(com => new FilterTabledDto2
                        {
                            CompetitionDayId = com.Id,
                            CompetitionId =(int) com.CompetitionId,
                            Name = com.Name,
                            date = com.From != null ? (DateTime)com.From.Date : null,
                            From = com.From,
                            To = com.To,
                            hallid = com.HallId,
                            Location = halls.Where(x => x.Id == com.HallId).Select(y => y.Location).FirstOrDefault(),
                            hallName = halls.Where(x => x.Id == com.HallId).Select(y => y.Name).FirstOrDefault(),
                            NameCompetition = competitions.Where(x=>x.Id == com.CompetitionId).Select(y=>y.Name).FirstOrDefault(),
                            ImagePath = competitions.Where(x=>x.Id == com.CompetitionId).Select(y=>y.ImagePath).FirstOrDefault() != null  ? (BaseURL + competitions.Where(x=>x.Id == com.CompetitionId).Select(y=>y.ImagePath).FirstOrDefault() ) : null ,
                            //levelName = AllLevelDb.
                            //Where(W => W.Id == (_unitOfWork.AssignedSubjects.FindAll(x=>x.CompetitionId == com.CompetitionId).
                            //Select(y=>y.AcademicYearId).FirstOrDefault())).Select(y=>y.Name).FirstOrDefault() ,
                            levelName = AllLevelDb.
                            Where(W => W.Id == (AllassignSubjectDb.Where(x=>x.CompetitionId == com.CompetitionId).Select(e=>e.AcademiclevelId).FirstOrDefault() )).Select(x=>x.Name).FirstOrDefault(),
                            specialDeptName = AllSpecialDeptDb.
                            Where(W => W.Id == (AllassignSubjectDb.Where(x=>x.CompetitionId == com.CompetitionId).Select(e=>e.SpecialdeptId).FirstOrDefault() )).Select(x=>x.Deptartment.Name + "-" + x.Name).FirstOrDefault(),
                            NameOfDoctor =AllUserDb.Where(x=>x.CompetitionId == com.CompetitionId) .Select(y=>y.HrUser.FirstName +" "+ y.HrUser.MiddleName).FirstOrDefault(),
                            NumberOfStudents = userIsStudent == false? competitionUsers.Where(x => x.CompetitionId == com.CompetitionId).Count() : null,
                            NumberOfAttendce = userIsStudent == false ? CompetitionDayUsers.Where(x => x.CompetitionDayId == com.Id && x.Attendance == true).Count() : null,
                            //levelName = AllLevelDb.
                            //Where(W => W.Id == (from o in AllassignSubjectDb.ToList() where o.CompetitionId == com.CompetitionId select o.AcademiclevelId).FirstOrDefault() 
                            //                    ).Select(x=>x.Name).FirstOrDefault() 
                        })
                        .ToList();
                    var alldata = new List<FilterTabledDto3>();
                    if(dto.TimeToday!=null)
                    {
                        var netdata = new FilterTabledDto3();
                        netdata.datetime=dto.TimeToday;
                        netdata.Allcompetitionday=competitionDayFilter.Where(x => x.date==dto.TimeToday?.Date).ToList();
                        alldata.Add(netdata);
                    }
                    if(dto.StartTime!=null&&dto.EndTime!=null)
                    {
                        for(DateTime date = (DateTime)dto.StartTime ;date<=dto.EndTime ;date=date.AddDays(1))
                        {
                            var netdata2 = new FilterTabledDto3();
                            netdata2.datetime=date;
                            netdata2.Allcompetitionday=competitionDayFilter.Where(x => x.date==date).ToList();
                            alldata.Add(netdata2);
                        }
                    }
                    Response.Data=alldata;
                }
                return Ok(Response);


            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                string ErrorMSG = "Exception :" + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [HttpGet("CompetitionDayAttendanceAllUsersAndFilterUsers")]                                     //new controll
        public async Task<IActionResult> CompetitionDayAttendanceAllUsersAndFilterUsers([FromHeader] int competitionId ,
                                                                                  [FromHeader] int competitionDayId , [FromHeader] long HrUserId ,
                                                                                  [FromHeader] long userIdSearch , [FromHeader] string? NameOfUser , [FromHeader] int PageNo = 1 , [FromHeader] int NoOfItems = 20)
        {
            BaseResponseWithDataAndHeader<AttendanceDto> Response = new BaseResponseWithDataAndHeader<AttendanceDto>();

            var Alldata = new  AttendanceDto();

            var missionUsers = new List<AttendanceUser>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            string encodedNameOfUser = HttpUtility.UrlDecode(NameOfUser);

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var _user =  _unitOfWork.HrUsers.FindAll(x=>x.Id == HrUserId).FirstOrDefault();
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
                    var userIsstudent= (userRoles.Any()&&userRoles.Contains("student"));
                    var userIsdoctor = (userRoles.Any()&&userRoles.Contains( "doctor"));
                    var userIsadminCompetition =  (userRoles.Any()&&userRoles.Contains( "adminCompetition"));
                    var userIsadmincontrol = (userRoles.Any()&&userRoles.Contains( "admincontrol"));



                    var competitionDayUserDB = _unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId == competitionDayId).ToList();
                    var UserslistDb = _unitOfWork.CompetitionUsers.FindAllQueryable(x => x.CompetitionId == competitionId, new[] { "HrUser" }).Select(y => y.HrUser);
                    // var gg = _unitOfWork.CompetitionUsers.FindAll(x => x.CompetitionId == competitionId, new[] { "ApplicationUser" }).Select(y => y.ApplicationUser);
                    Alldata.NumberOfStudent=UserslistDb.Count();

                    if(NameOfUser!=null)
                    {
                        var users = _unitOfWork.HrUsers.FindAllQueryable(x => (x.FirstName + " " + x.MiddleName + " " + x.LastName).Contains(encodedNameOfUser));
                        UserslistDb=UserslistDb.Intersect(users);
                    }
                    if(userIsstudent)
                    {
                        UserslistDb=_unitOfWork.HrUsers.FindAllQueryable(x => x.Id==userIdSearch);
                    }

                    Alldata.NumberOfAttendanceStudent=competitionDayUserDB.Where(x => x.Attendance==true).Count();
                    Alldata.FromScore=_unitOfWork.CompetitionDays.FindAll(x => x.Id==competitionDayId).Select(y => y.FromScore).FirstOrDefault()??0;
                    var usersPagedList = PagedList<HrUser>.Create(UserslistDb ,PageNo, NoOfItems);

                    foreach(var item in usersPagedList)
                    {
                        var missionuser = new AttendanceUser();

                        missionuser.userId=item.Id;
                        //  missionuser.SerialNum=item.SerialNum;
                        missionuser.userName=item.FirstName+" "+item.MiddleName+" "+item.LastName;
                        missionuser.ImagePath=item.ImgPath!=null ? BaseURL+item.ImgPath : null;
                        var test = competitionDayUserDB.Where(X => X.HrUserId==item.Id).FirstOrDefault();
                        if(test!=null)
                        {
                            missionuser.Date=competitionDayUserDB.Where(X => X.HrUserId==item.Id).Select(y => y.CreationDate).FirstOrDefault();

                        }
                        else
                        {
                            missionuser.Date=null;
                        }
                        //missionuser.DateOfupload = UploadFileDB.Where(x => x.UserId == item.Id).Select(y => y.DateTime).FirstOrDefault();
                        missionuser.status=competitionDayUserDB.Where(X => X.HrUserId==item.Id).Select(y => y.Attendance).FirstOrDefault()==true ? "الغاء الحضور" : "تسجيل الحضور";
                        //  missionuser.degree = competitionDayUserDB.Where(x => x.ApplicationUserId==item.Id).Select(y => y.UserScore).FirstOrDefault();
                        missionUsers.Add(missionuser);
                    }

                    Alldata.userlist=missionUsers;
                    Response.Data=Alldata;

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
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }


        [Authorize]
        [HttpPost("AttendanceForUserbyadminordoctor")]                                     //new controll
        public async Task<IActionResult> AttendanceForUserbyadminordoctor([FromHeader] long userId , [FromHeader] long userIdStudent , [FromHeader] int competitionDayId)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var competitiondayUser = _unitOfWork.CompetitionDayUsers.FindAll(c => c.HrUserId == userIdStudent && c.CompetitionDayId == competitionDayId)
                                             .FirstOrDefault();
                    var competitiondayUserRepeatAttendance = _unitOfWork.CompetitionDayUsers.
                    FindAll(c => c.HrUserId == userIdStudent && c.CompetitionDayId == competitionDayId && c.Attendance == true && c.IsFinished == true)
                                             .FirstOrDefault();
                    var CheckCompetitionDayDb = await _unitOfWork.CompetitionDays.GetByIdAsync(competitionDayId);

                    if(CheckCompetitionDayDb==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="لا يوجد محاضرة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var competitionId = _unitOfWork.CompetitionDays.FindAll(x => x.Id == competitionDayId).Select(c => c.CompetitionId).FirstOrDefault();

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

                    var userIsadminCompetition =  (userRoles.Any()&&userRoles.Contains( "adminCompetition"));



                    if(!await _permissionService.CheckUserHasPermissionManageCompetition(userId , (int)competitionId)||!(userIsadminCompetition||userIsadmin))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="ليس لديك صلاحية  فى هذه المادة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    if(competitiondayUserRepeatAttendance!=null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="تم تسجيل الحضور لهذا الطالب ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var competitionUser = _unitOfWork.CompetitionUsers.FindAll(c => c.CompetitionId == CheckCompetitionDayDb.CompetitionId && c.HrUserId == userIdStudent)
                    .FirstOrDefault();
                    if(competitionUser==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا المستخدم لم يتم الاشتراك في هذه المادة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    if(competitiondayUser==null)
                    {
                        _unitOfWork.CompetitionDayUsers.Add(new CompetitionDayUser
                        {
                            HrUserId=userIdStudent ,
                            CompetitionDayId=competitionDayId ,
                            UserScore=CheckCompetitionDayDb.FromScore ,
                            FromScore=CheckCompetitionDayDb.FromScore ,
                            Attendance=true ,
                            IsFinished=true ,
                            CreationBy=userId.ToString() ,
                            CreationDate=_authService.TimeZoneEgypt()
                        });

                    }



                    _unitOfWork.Complete();
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);


            }

        }


        //[Authorize]
        //[HttpGet("CompetitionDayToDayForUserId")]                                     //new controll
        //public async Task<IActionResult> CompetitionDayToDayForUserId([FromHeader] long userId , [FromHeader] DateTime? dateTime = null , [FromHeader] int typeId = 0)
        //{
        //    BaseResponseWithData<List<FilterTabledDto>> Response = new BaseResponseWithData<List<FilterTabledDto>>();
        //    Response.Data=new List<FilterTabledDto>();
        //    Response.Result=true;
        //    Response.Errors=new List<Error>();
        //    var data = new List<FilterTabledDto>();
        //    bool userIsAdmin = false;
        //    bool userIsdoctor = false;
        //    try
        //    {
        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //        Response.Errors=validation.errors;
        //        Response.Result=validation.result;

        //        if(Response.Result)
        //        {
        //            var _user =  _unitOfWork.HrUsers.FindAll(x=>x.Id == userId).FirstOrDefault();
        //            if(_user==null)
        //            {
        //                Response.Result=false;
        //                Error error = new Error();
        //                error.ErrorCode="Err10";
        //                error.ErrorMSG="هذا المستخدم غير موجود";
        //                Response.Errors.Add(error);

        //            }

        //            var userRoles = _unitOfWork.UserRoles.FindAllQueryable(x=>x.Id > 0 ,new[]{"Role"}).Select(n=>n.Role.Name);

        //            userIsAdmin=(userRoles.Any()&&userRoles.Contains("admin"));
        //            userIsdoctor=(userRoles.Any()&&userRoles.Contains("doctor"));



        //            if(userIsAdmin)
        //            {
        //                var competitions = await _unitOfWork.Competitions.FindAllAsync(x => x.Id > 0);
        //                foreach(var item in competitions)
        //                {

        //                    DateTime time = dateTime ?? DateTime.Today;
        //                    DateTime startTime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
        //                    DateTime endTime = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);

        //                    var competitionDayListDB = new List<CompetitionDay>();
        //                    if(typeId==0)
        //                    {
        //                        competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime).ToList();

        //                    }
        //                    else
        //                    {
        //                        competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime&&x.TypeId==typeId).ToList();

        //                    }
        //                    var competitiondayfilter = new List<FilterTabledDto>();
        //                    competitiondayfilter=competitionDayListDB

        //                        .Select(com => new FilterTabledDto
        //                        {
        //                            CompetitionDayId=com.Id ,
        //                            TypeId=com.TypeId ,
        //                            Name=com.Name??null ,
        //                            NameCompetition=item.Name ,
        //                            From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
        //                            To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
        //                            NumberOfStudents=_unitOfWork.CompetitionUsers.FindAll(x => x.CompetitionId==item.Id).Count() ,
        //                            NumberOfAttendce=_unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId==com.Id&&x.Attendance==true).Count()
        //                        }).ToList();
        //                    data.AddRange(competitiondayfilter);
        //                }
        //            }
        //            else if(userIsdoctor)
        //            {
        //                var competitions =  _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.HrUserId == userId , new[] { "Competition" }).Select(y => y.Competition).ToList();
        //                foreach(var item in competitions)
        //                {

        //                    DateTime time = dateTime ?? DateTime.Today;
        //                    DateTime startTime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
        //                    DateTime endTime = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);

        //                    var competitionDayListDB = new List<CompetitionDay>();
        //                    if(typeId==0)
        //                    {
        //                        competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime).ToList();

        //                    }
        //                    else
        //                    {
        //                        competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime&&x.TypeId==typeId).ToList();

        //                    }
        //                    var competitiondayfilter = new List<FilterTabledDto>();
        //                    competitiondayfilter=competitionDayListDB

        //                        .Select(com => new FilterTabledDto
        //                        {
        //                            CompetitionDayId=com.Id ,
        //                            TypeId=com.TypeId ,
        //                            Name=com.Name??null ,
        //                            NameCompetition=item.Name ,
        //                            From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
        //                            To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
        //                            NumberOfStudents=_unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId==com.Id).Count() ,
        //                            NumberOfAttendce=_unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId==com.Id&&x.Attendance==true).Count()
        //                        }).ToList();
        //                    data.AddRange(competitiondayfilter);
        //                }
        //            }
        //            else
        //            {
        //                var competitions = _unitOfWork.CompetitionUsers.FindAll(x => x.HrUserId == userId, new[] { "Competition" }).Select(y => y.Competition).ToList();
        //                foreach(var item in competitions)
        //                {

        //                    DateTime time = dateTime ?? DateTime.Today;
        //                    DateTime startTime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
        //                    DateTime endTime = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);

        //                    var competitionDayListDB = new List<CompetitionDay>();
        //                    if(typeId==0)
        //                    {
        //                        competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime).ToList();

        //                    }
        //                    else
        //                    {
        //                        competitionDayListDB=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&startTime<x.From&&x.To<endTime&&x.TypeId==typeId).ToList();

        //                    }
        //                    var competitiondayfilter = new List<FilterTabledDto>();

        //                    competitiondayfilter=competitionDayListDB

        //                        .Select(com => new FilterTabledDto
        //                        {
        //                            CompetitionDayId=com.Id ,
        //                            TypeId=com.TypeId ,
        //                            Name=com.Name??null ,
        //                            NameCompetition=item.Name ,
        //                            From=com.From.ToString("yyyy-MM-dd HH:mm:ss") ,
        //                            To=com.To.ToString("yyyy-MM-dd HH:mm:ss") ,
        //                            AttendanceFlag=_unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId==com.Id&&x.HrUserId==userId&&x.Attendance==true).Any()
        //                        }).ToList();
        //                    data.AddRange(competitiondayfilter);
        //                }
        //            }
        //            Response.Result=true;
        //            Response.Data=data;
        //        }
        //        return Ok(Response);
        //    }
        //    catch(Exception ex)
        //    {
        //        Response.Result=false;
        //        Error error = new Error();
        //        error.ErrorCode="Err10";
        //        error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
        //        Response.Errors.Add(error);
        //        return BadRequest(Response);
        //    }

        //}

        [HttpGet("GenerateQr")]                 //new  
        public async Task<IActionResult> GenerateQr([FromHeader] int CompetitionDayId)
        {
            BaseResponseWithData<GenerateQrViewModel> Response = new BaseResponseWithData<GenerateQrViewModel>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var TempData = new GenerateQrViewModel();

                    TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                    TempData.dateGenerateQr=EncodeToBase64(egyptDateTime.ToString());
                    TempData.competitionDayId=CompetitionDayId;
                    Response.Result=true;
                    Response.Data=TempData;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {

                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [HttpPost("attendencecalulateQr")]                 //new   
        public async Task<IActionResult> attendencecalulateQr(attendencecalulateViewModel dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            double lat1 =0;
            double long1 =0;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var CheckCompetitionDayDb = await  _unitOfWork.CompetitionDays.FindAllAsync(x => x.Id == dto.CompetitionDayId);

                    if(CheckCompetitionDayDb==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="لا يوجد محاضرة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var hallIdcompetitionDay = CheckCompetitionDayDb.FirstOrDefault().HallId;
                    if(hallIdcompetitionDay==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="لا يوجد قاعة لهذة المحاضرة او لا يوجد محاضرة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);


                    }

                    var halls = await _unitOfWork.Halls.FindAllAsync(x => x.Id == hallIdcompetitionDay) ;
                    var hallLocation = halls.FirstOrDefault();

                    if(hallLocation!=null)
                    {
                        lat1=hallLocation.Latitude!=null ? (double)hallLocation?.Latitude : 0;
                        long1=hallLocation?.Longitude!=null ? (double)hallLocation?.Longitude : 0;


                    }

                    if(!(lat1==0||long1==0||lat1==null||long1==null))
                    {
                        var distance = _authService.Haversine((double)lat1,(double)long1,(double)dto.latitude,(double)dto.longitude);
                        if(distance>200)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="موقعك الحالي ليس فى مكان المحاضرة";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                    }



                    TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

                    //var dateEncrpt = HttpUtility.UrlDecode(dto.dateGenerateQr);

                    //DateTime dateTime = ConvertFromUnixTimestampMilliseconds(long.Parse( dateEncrpt));


                    var dateEncrpt = DecodeFromBase64(dto.dateGenerateQr);
                    //DateTime dateTime = ConvertFromUnixTimestampSeconds(long.Parse( dateEncrpt));
                    // DateTime dateTime = dateTime.now(); //DateTimeOffset.FromUnixTimeSeconds(dateEncrpt).DateTime;
                    if(!DateTime.TryParse(dateEncrpt , out DateTime dateTime))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="يرجى اعادة المحاوله";
                        Response.Errors.Add(error);
                        return BadRequest(Response);


                    }
                    if(!(egyptDateTime<=(dateTime.AddSeconds(60))&&egyptDateTime>=dateTime))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="الوقت قد نفذ لتنفيذ العملية";
                        Response.Errors.Add(error);
                        return BadRequest(Response);



                    }
                    var competitionUser = _unitOfWork.CompetitionUsers.FindAll(c => c.CompetitionId == CheckCompetitionDayDb.FirstOrDefault().CompetitionId && c.HrUserId == dto.userId)
                  .FirstOrDefault();
                    if(competitionUser==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا المستخدم لم يتم الاشتراك في هذه المادة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    //var competitionDayUser = _unitOfWork.CompetitionDayUser.FindAll(x => x.ApplicationUserId == dto.UserId && x.CompetitionDayId == dto.CompetitionDayId).FirstOrDefault();
                    //var competitionDayUser = _unitOfWork.CompetitionDayUsers.Find(x => x.ApplicationUserId == dto.UserId && x.CompetitionDayId == dto.CompetitionDayId);
                    // Update Total Score

                    var  checkAttendenceForStudent=_unitOfWork.CompetitionDayUsers.FindAll(x => x.HrUserId==dto.userId&&x.CompetitionDayId==dto.CompetitionDayId&&x.Attendance==true&&x.IsFinished==true).FirstOrDefault();
                    if(checkAttendenceForStudent!=null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذا المستخدم تم تسجيل الحضور له في هذه المادة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    _unitOfWork.CompetitionDayUsers.Add(new CompetitionDayUser
                    {
                        HrUserId=dto.userId ,
                        CompetitionDayId=dto.CompetitionDayId ,
                        UserScore=CheckCompetitionDayDb.FirstOrDefault().FromScore ,
                        FromScore=CheckCompetitionDayDb.FirstOrDefault().FromScore ,
                        Attendance=true ,
                        IsFinished=true ,
                        CreationBy=dto.userId.ToString() ,
                        CreationDate=_authService.TimeZoneEgypt()
                    });

                    if(competitionUser!=null)
                    {
                        competitionUser.TotalScore+=CheckCompetitionDayDb.FirstOrDefault().FromScore??0;
                        _unitOfWork.CompetitionUsers.Update(competitionUser);
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
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);

            }
        }

        [Authorize]
        [HttpPost("attendanceRemove")]                 //new   
        public async Task<IActionResult> attendanceRemove([FromHeader] long userIdStudent , [FromHeader] int competitionDayId)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var CheckCompetitionDayDb = _unitOfWork.CompetitionDays.FindAll(x => x.Id == competitionDayId).FirstOrDefault();

                    var competitionUser = _unitOfWork.CompetitionUsers.FindAll(c => c.CompetitionId == CheckCompetitionDayDb.CompetitionId && c.HrUserId == userIdStudent)
                  .FirstOrDefault();
                    var competitionDayUser = _unitOfWork.CompetitionDayUsers.
                    FindAll(x=>x.CompetitionDayId == competitionDayId && x.HrUserId == userIdStudent).FirstOrDefault();

                    if(competitionUser!=null&&competitionDayUser!=null)
                    {
                        competitionUser.TotalScore-=CheckCompetitionDayDb.FromScore??0;
                        _unitOfWork.CompetitionUsers.Update(competitionUser);
                        _unitOfWork.CompetitionDayUsers.Delete(competitionDayUser);

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
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }


        [Authorize]

        [HttpGet("CompetitionDayAllNextMissionAndReaserchForUser")]                                     //new controll
        public async Task<IActionResult> CompetitionDayAllNextMissionAndReaserchForUser([FromHeader] bool Expired , [FromHeader] int termId , [FromHeader] long HrUserId)
        {
            BaseResponseWithDataAndHeader<List<AllNextMissionAndReaserchVM>> Response = new BaseResponseWithDataAndHeader<List<AllNextMissionAndReaserchVM>>();

            Response.Result=true;
            Response.Errors=new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                    var assignSubjectDB = _unitOfWork.AssignedSubjects.FindAll(x => x.Id > 0, new[] { "Academiclevel", "AcademicYear", "Specialdept", "Competition", "Programm" }).ToList();

                    var competitionIds = _unitOfWork.CompetitionUsers.FindAll(x => x.HrUserId == HrUserId && x.FinishStatus != "Finished" && x.EnrollmentStatus == "approved" && x.DelayOrWithdrawalStatus != "Withdraw" && x.DelayOrWithdrawalStatus != "delay").Select(y => (int)y.CompetitionId).ToList();
                    if(termId!=0)
                    {
                        var assignSubjectIds = assignSubjectDB.Where(x => x.AcademicYearId == termId).Select(c => c.CompetitionId).ToList();
                        competitionIds=competitionIds.Intersect(assignSubjectIds).ToList();
                    }
                    var competitionDaysDB = _unitOfWork.CompetitionDays.FindAll(x => (competitionIds.Contains((int)x.CompetitionId)) && x.To >= egyptDateTime && (x.TypeId == 2 || x.TypeId == 3));
                    var NumberOfMission = competitionDaysDB.Where(x => (competitionIds.Contains((int)x.CompetitionId)) && x.To >= egyptDateTime && (x.TypeId == 2 || x.TypeId == 3));
                    var uploadDB = _unitOfWork.UploadFilebyStudent.FindAllQueryable(x => x.HrUserId == HrUserId).AsQueryable();
                    var competitionDayUser = _unitOfWork.CompetitionDayUsers.FindAllQueryable(x => x.HrUserId == HrUserId).AsQueryable();
                    if(Expired==true)
                    {
                        competitionDaysDB=_unitOfWork.CompetitionDays.FindAll(x => (competitionIds.Contains((int)x.CompetitionId))&&x.To<egyptDateTime&&(x.TypeId==2||x.TypeId==3));

                    }

                    var competitionDay = competitionDaysDB.Select(com =>
                {
                    var assignSubject = assignSubjectDB.FirstOrDefault(c => c.CompetitionId == com.CompetitionId);
                    var upload = uploadDB.FirstOrDefault(x => x.CompetitionDayId == com.Id);
                    var imagePath = assignSubject?.Competition?.ImagePath;
                    //var formattedDate = upload?.DateTime?.ToString("yyyy-MM-dd HH:mm:ss");

                    return new AllNextMissionAndReaserchVM
                    {
                        competitionDayId= com.Id,
                        competitionId=(int) com.CompetitionId,
                        typeId= com.TypeId,
                        competitionDayName= com.Name,
                        competitionName = assignSubject?.Competition?.Name,
                        levelName = assignSubject?.Academiclevel?.Name,
                        termName = assignSubject?.AcademicYear?.Term,
                        SpecialDeptName = assignSubject?.Specialdept?.Name,
                        programName = assignSubject?.Programm?.Name,
                        ImagePath = !string.IsNullOrEmpty(imagePath) ? BaseURL + imagePath : null,
                        status = upload != null ? "تم الرفع" : "لم يتم الرفع",
                        comment = upload?.Comment,
                        DateOfupload = upload?.DateTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                        DateCompetitionDay = com.From.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndDateCompetitionDay = com.To.ToString("yyyy-MM-dd HH:mm:ss"),
                        ShowFile = upload?.Uploadfile != null ? BaseURL + upload.Uploadfile : null ,
                        degreeOfCompetitionDay = com.FromScore ,
                        degree = competitionDayUser.Where(x=>x.CompetitionDayId == com.Id).Select(c=>c.UserScore).FirstOrDefault()
                    };
                }).ToList();



                    Response.Data=competitionDay;
                }

                return Ok(Response);

            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }


        private string EncodeToBase64(string plainText)
        {
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private string DecodeFromBase64(string base64EncodedData)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

    }
}
