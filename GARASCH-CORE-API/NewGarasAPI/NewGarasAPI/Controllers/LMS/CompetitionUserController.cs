
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.LMS;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.LMS;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.LMS;
using System.Security.Claims;
using System.Web;

namespace NewGarasAPI.Controllers.LMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionUserController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private IPermissionService _permissionService;
        private readonly IAuthLMsService _authService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public CompetitionUserController(IAuthLMsService authService , IUnitOfWork unitOfWork
           , IPermissionService permissionService , ITenantService tenantService)
        {
            _httpClient=new HttpClient();
            _unitOfWork=unitOfWork;
            _permissionService=permissionService;
            _authService=authService;
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
            _helper=new Helper.Helper();

        }


        private string ApplicationUserId
        {
            get
            {
                string UserId = User.FindFirstValue("uid");
                return UserId;
            }
        }

        [HttpPost("subscribeCompetitionForStudent")]
        public async Task<IActionResult> subscribeCompetitionForStudent([FromHeader] int competitionId , [FromHeader] long HrUserId)
        {

            BaseResponse Response = new BaseResponse();
            Response.Result=false;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    if(competitionId==0)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="competitionId is required";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var competition = _unitOfWork.Competitions.FindAll(x => x.Id == competitionId).FirstOrDefault() ;
                    if(competition==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="competition Not Found";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    if(competition.Status=="Completed")
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذة المادة مكتملة العدد";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    _unitOfWork.CompetitionUsers.Add(new CompetitionUser
                    {

                        HrUserId=HrUserId ,
                        CompetitionId=competitionId ,
                        CreationDateOfpending=_authService.TimeZoneEgypt() ,
                        EnrollmentStatus="pending" ,
                        CreationBy=ApplicationUserId ,
                    });
                    _unitOfWork.Complete();

                    if(_unitOfWork.CompetitionUsers.FindAll(x => x.CompetitionId==competitionId&&x.DelayOrWithdrawalStatus==null&&(x.EnrollmentStatus=="approved"||x.EnrollmentStatus=="pending")&&x.FinishStatus!="Finished").Count()>=(_unitOfWork.Competitions.FindAll(x => x.Id==competitionId).Select(y => y.Capacity).FirstOrDefault()))
                    {

                        competition.Status="مكتملة";
                        _unitOfWork.Competitions.Update(competition);
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

        [HttpPost("OrderWithdrawalOrDelay")]
        public IActionResult OrderWithdrawalOrDelay([FromHeader] int competitionId , [FromBody] CompetitionUserUpdateDTO dto , [FromHeader] long HrUserId)
        {

            BaseResponse Response = new BaseResponse();
            Response.Result=false;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    if(competitionId==0)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="competitionId is required";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var oldcompetition = _unitOfWork.CompetitionUsers.FindAll(x => x.CompetitionId == competitionId && x.HrUserId == HrUserId).FirstOrDefault();
                    if(oldcompetition!=null)
                    {
                        if(oldcompetition.EnrollmentStatus=="approved"&&oldcompetition.DelayOrWithdrawalStatus!="delay"&&oldcompetition.DelayOrWithdrawalStatus!="Withdraw")
                        {
                            if(dto.DelayOrWithdrawalStatus=="DelayRequest")
                            {
                                oldcompetition.DelayRequestStatus=dto.DelayOrWithdrawalStatus;                            // WithdrawRequest  or DelayRequest
                                oldcompetition.ReasonForDelayRequesr=dto.ReasonForDelayOrWithdrawal;
                                oldcompetition.DateOfDelaylRequest=_authService.TimeZoneEgypt();
                            }

                            if(dto.DelayOrWithdrawalStatus=="WithdrawRequest")
                            {
                                oldcompetition.WithdrawalRequestStatus=dto.DelayOrWithdrawalStatus;                            // WithdrawRequest  or DelayRequest
                                oldcompetition.ReasonForWithdrawalRequest=dto.ReasonForDelayOrWithdrawal;
                                oldcompetition.DateOfWithdrawalRequest=_authService.TimeZoneEgypt();
                            }
                        }
                        else
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="هذا المستخدم قد يكون غير مشترك او يكون مؤجل او يكون قد سحبت المادة مسبقا";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }

                    }
                    _unitOfWork.CompetitionUsers.Update(oldcompetition);
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
                error.ErrorMSG="competitionId is required";
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }


        [HttpGet("FilterEnrollmentStatus")]
        public async Task<IActionResult> FilterEnrollmentStatus([FromHeader] int? competitionId , [FromHeader] string? studentName , [FromHeader] int? specialId , [FromHeader] int? ProgramId , [FromHeader] int? YearId ,
                                                              [FromHeader] int PageNo , [FromHeader] int NoOfItems , [FromHeader] string? enrollmentstatus , [FromHeader] string? DelayOrWithdrawalStatus)
        {
            ResponsePagelistFilterEnrollmentVm Response = new ResponsePagelistFilterEnrollmentVm();
            Response.Result=true;
            Response.Errors=new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    string encodedstudentName = HttpUtility.UrlDecode(studentName);
                    string encodeEnrollmentstatus = HttpUtility.UrlDecode(enrollmentstatus);
                    string encodeDelayOrWithdrawalStatus = HttpUtility.UrlDecode(DelayOrWithdrawalStatus);

                    var filterenrollmentlist = new List<FilterEnrollmentStatusDto>();
                    var filterenrollment = new FilterEnrollmentStatusDto();
                    var filtersubject = new subjectlist();
                    var CompetitionUsersDb = _unitOfWork.CompetitionUsers.FindAll(x => x.Id > 0 , new[] { "Competition" , "HrUser" } );


                    if(PageNo==0)
                    {
                        PageNo=1;
                    }
                    if(NoOfItems==0)
                    {
                        NoOfItems=50;
                    }

                    if(ProgramId!=null)
                    {
                        var competitionIds =_unitOfWork.AssignedSubjects.FindAll(x=>x.ProgrammId == ProgramId).Select(x=>x.CompetitionId);
                        CompetitionUsersDb=CompetitionUsersDb.Where(x => (competitionIds.Contains((int)x.CompetitionId)));
                    }

                    if(YearId!=null)
                    {
                        var termIds = _unitOfWork.AcademicYears.FindAll(x=>x.YearId == YearId).Select(y=>y.Id);
                        var competitionIds =_unitOfWork.AssignedSubjects.FindAll(x=>(termIds.Contains(x.AcademicYearId))).Select(y=>y.CompetitionId);
                        CompetitionUsersDb=CompetitionUsersDb.Where(x => (competitionIds.Contains((int)x.CompetitionId)));
                    }

                    if(!string.IsNullOrEmpty(enrollmentstatus))
                    {
                        CompetitionUsersDb=CompetitionUsersDb.Where(x => x.EnrollmentStatus==encodeEnrollmentstatus);

                    }
                    if(!string.IsNullOrEmpty(DelayOrWithdrawalStatus))
                    {
                        CompetitionUsersDb=CompetitionUsersDb.Where(x => x.DelayOrWithdrawalStatus==encodeDelayOrWithdrawalStatus);

                    }



                    if(studentName!=null)
                    {
                        var student = _unitOfWork.HrUsers.FindAll(x => (x.FirstName + " " + x.MiddleName + " " + x.LastName) == encodedstudentName).FirstOrDefault();
                        CompetitionUsersDb=CompetitionUsersDb.Where(x => x.HrUserId==student.Id);


                    }


                    if(competitionId!=null)
                    {
                        CompetitionUsersDb=CompetitionUsersDb.Where(x => x.CompetitionId==competitionId);

                    }

                    if(specialId!=null)
                    {
                        var competitionIds =_unitOfWork.AssignedSubjects.FindAll(x=>x.SpecialdeptId == specialId).Select(x=>x.CompetitionId);
                        CompetitionUsersDb=CompetitionUsersDb.Where(x => (competitionIds.Contains((int)x.CompetitionId)));
                    }

                    var groupedUsers  = CompetitionUsersDb.GroupBy(u => u.HrUserId);
                    filterenrollmentlist=groupedUsers.Select(a => new FilterEnrollmentStatusDto
                    {
                        studentId=a.Key ,
                        studentName=a.First().HrUser.FirstName+" "+
                             a.First().HrUser.MiddleName+" "+
                             a.First().HrUser.LastName ,
                        subjectlists=a.Select(s => new subjectlist
                        {
                            subjectId=(int)s.CompetitionId ,
                            subjectName=s.Competition.Name ,
                            comptitionUserID=s.Id
                        }).ToList()
                    }).ToList();


                    var usersPagedList = PagedList<FilterEnrollmentStatusDto>.Create(filterenrollmentlist.AsQueryable().OrderByDescending(x => x.studentName), PageNo, NoOfItems);


                    Response.filterlist=usersPagedList;
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


        [HttpPost("adminChangeStatusCompetitionUser")]
        public async Task<IActionResult> adminChangeStatusCompetitionUser([FromBody] adminChangeStatusCompetitionUserVM dto , [FromHeader] long HrUserId)
        {

            BaseResponse Response = new BaseResponse();
            Response.Result=false;
            Response.Errors=new List<Error>();
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

                    var userIsAdmin= (userRoles.Any()&&userRoles.Contains("admin"));
                    var userIsAdminCompetition =  (userRoles.Any()&&userRoles.Contains( "adminCompetition"));


                    if(!await _permissionService.CheckUserHasPermissionManageCompetition(HrUserId , dto.competitionId)||!(userIsAdminCompetition||userIsAdmin))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="ليس لديك صلاحية لهذا الامر";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    var competitionuserDB = _unitOfWork.CompetitionUsers.FindAll(x=>x.Id > 0);
                    if(dto.listOfCompetitionUser.Count()>0)
                    {
                        foreach(var item in dto.listOfCompetitionUser)
                        {
                            var  oldcompeitionUser = competitionuserDB.Where(x=>x.CompetitionId == dto.competitionId && x.HrUserId == item.userId).FirstOrDefault();
                            var oldcompetition = _unitOfWork.Competitions.FindAll(x=>x.Id == dto.competitionId).FirstOrDefault();
                            if(item.statusOfCompetition=="approved"||item.statusOfCompetition=="reject")
                            {
                                oldcompeitionUser.CreationBy=ApplicationUserId;
                                oldcompeitionUser.EnrollmentStatus=item.statusOfCompetition;
                                oldcompeitionUser.CreationDateOfapprovedAndReject=_authService.TimeZoneEgypt();
                                _unitOfWork.CompetitionUsers.Update(oldcompeitionUser);
                                if(item.statusOfCompetition=="reject"&&(oldcompetition.Status!="OnHold"||oldcompetition.Status!="Pending "))
                                {
                                    oldcompetition.Status="Open";
                                    _unitOfWork.Competitions.Update(oldcompetition);
                                }
                            }

                            if(item.statusOfCompetition=="delay")
                            {
                                if(oldcompeitionUser.DelayOrWithdrawalStatus!="Withdraw"&&oldcompetition.Status!="OnHold"&&oldcompetition.Status!="Pending ")
                                {
                                    oldcompeitionUser.DelayOrWithdrawalStatus=item.statusOfCompetition;
                                    oldcompeitionUser.CreationByDelay=ApplicationUserId;
                                    oldcompeitionUser.DateOfDelay=_authService.TimeZoneEgypt();
                                    _unitOfWork.CompetitionUsers.Update(oldcompeitionUser);

                                    oldcompetition.Status="Open";
                                    _unitOfWork.Competitions.Update(oldcompetition);

                                }
                                else
                                {
                                    Response.Result=false;
                                    Error error = new Error();
                                    error.ErrorCode="Err10";
                                    error.ErrorMSG="قد تكون المادة مسحوبة او لم تبدا بعد او مغلقة";
                                    Response.Errors.Add(error);
                                    return BadRequest(Response);

                                }


                            }
                            if(item.statusOfCompetition=="Withdraw")
                            {
                                if(oldcompeitionUser.DelayOrWithdrawalStatus!="delay"&&oldcompetition.Status!="OnHold"&&oldcompetition.Status!="Pending ")
                                {
                                    oldcompeitionUser.DelayOrWithdrawalStatus=item.statusOfCompetition;
                                    oldcompeitionUser.CreationByWithdrawal=ApplicationUserId;
                                    oldcompeitionUser.DateOfWithdrawal=_authService.TimeZoneEgypt();
                                    _unitOfWork.CompetitionUsers.Update(oldcompeitionUser);

                                    oldcompetition.Status="Open";
                                    _unitOfWork.Competitions.Update(oldcompetition);

                                }
                                else
                                {
                                    Response.Result=false;
                                    Error error = new Error();
                                    error.ErrorCode="Err10";
                                    error.ErrorMSG="قد تكون المادة مؤجلة او لم تبدا بعد او مغلقة";
                                    Response.Errors.Add(error);
                                    return BadRequest(Response);

                                }
                            }
                            if(item.statusOfCompetition=="rejectDelay")
                            {
                                oldcompeitionUser.DelayRequestStatus=item.statusOfCompetition;
                                oldcompeitionUser.CreationByRejectDelaylRequest=ApplicationUserId;
                                oldcompeitionUser.DateOfRejectDelaylRequest=_authService.TimeZoneEgypt();
                                _unitOfWork.CompetitionUsers.Update(oldcompeitionUser);
                            }
                            if(item.statusOfCompetition=="rejectWithdraw")
                            {
                                oldcompeitionUser.WithdrawalRequestStatus=item.statusOfCompetition;
                                oldcompeitionUser.CreationByRejectWithdrawalRequest=ApplicationUserId;
                                oldcompeitionUser.DateOfRejectWithdrawalRequest=_authService.TimeZoneEgypt();
                                _unitOfWork.CompetitionUsers.Update(oldcompeitionUser);
                            }
                            if(item.statusOfCompetition=="suspended")
                            {
                                oldcompeitionUser.EnrollmentStatus=item.statusOfCompetition;
                                // oldcompeitionUser.CreationByRejectWithdrawalRequest=ApplicationUserId;
                                oldcompeitionUser.DateOfSuspension=_authService.TimeZoneEgypt();
                                _unitOfWork.CompetitionUsers.Update(oldcompeitionUser);
                            }
                        }


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


        [HttpGet("ResuiltStudentsInCompetition")]
        public async Task<IActionResult> ResuiltStudentsInCompetition( [FromHeader] long HrUserId , [FromHeader] int competitionId , [FromHeader] string? NameOfUser , [FromHeader] int PageNo = 1 , [FromHeader] int NoOfItems = 50)
        {

            ResponsePagelistResuiltStudentsInCompetitionVM Response = new ResponsePagelistResuiltStudentsInCompetitionVM();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    string encodedNameOfUser = HttpUtility.UrlDecode(NameOfUser);
                    var userIsAdmin = false;
                    var competitionDayUserLecturesDB = new List<CompetitionDayUser>();
                    var competitionDayUserMissionDB = new List<CompetitionDayUser>();
                    var competitionDayUserReserchDB = new List<CompetitionDayUser>();
                    var competitionDayUserQuizDB = new List<CompetitionDayUser>();
                    var competitionDayUserMidTermDB = new List<CompetitionDayUser>();
                    var competitionDayUserFinalDB = new List<CompetitionDayUser>();
                    var Data = new List<ResuiltStudentsInCompetition>();

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

                    userIsAdmin=(userRoles.Any()&&userRoles.Contains("admin"));
                    //  var userIsAdminCompetition =  (userRoles.Any()&&userRoles.Contains( "adminCompetition"));


                    var  IsMemberAdmin=_unitOfWork.CompetitionMemberAdmins.FindAll(x =>
                    x.HrUserId==HrUserId&&x.CompetitionId==competitionId).Any();
                    if(IsMemberAdmin==false&&userIsAdmin==false)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="ليس لديك صلاحية لهذا الامر";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var competition = _unitOfWork.Competitions.FindAll(x=>x.Id == competitionId).FirstOrDefault();



                    var competitionTypeDB = _unitOfWork.CompetitionTypes.FindAll(x=>x.Id > 0);
                    var StudentsDB = _unitOfWork.CompetitionUsers.FindAll(x=>x.CompetitionId == competitionId && x.EnrollmentStatus =="approved" && x.DelayOrWithdrawalStatus!= "delay" && x.DelayOrWithdrawalStatus !="Withdraw",new [] { "HrUser" } ).Select(y=>y.HrUser).ToList();
                    var competitionDayIdsLectures = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 1).Select(y=>y.Id);
                    var competitionDayMission = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 2);
                    var competitionDayIdsMission = competitionDayMission.Select(y=>y.Id);
                    var competitionDayReserch = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 3);
                    var competitionDayIdsReserch = competitionDayReserch.Select(y=>y.Id);
                    var competitionDayQuiz = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 4);
                    var competitionDayIdsQuiz = competitionDayQuiz.Select(y=>y.Id);
                    var competitionDayMidTerm = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 5);
                    var competitionDayIdsMidTerm = competitionDayMidTerm.Select(y=>y.Id);
                    var competitionDayFinal = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 6);
                    var competitionDayIdsFinal = competitionDayFinal.Select(y=>y.Id);
                    var AllCompetitionDayIds = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId).Select(x=>x.Id);
                    var competitiondayuserDB = _unitOfWork.CompetitionDayUsers.FindAll(X => (AllCompetitionDayIds.Contains((int)X.CompetitionDayId)) );
                    if(competitionDayIdsLectures.Count()>0)
                    {
                        competitionDayUserLecturesDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsLectures.Contains((int)x.CompetitionDayId));

                    }
                    if(competitionDayIdsMission.Count()>0)
                    {
                        competitionDayUserMissionDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsMission.Contains((int)x.CompetitionDayId) , new [] { "CompetitionDay" });

                    }
                    if(competitionDayIdsReserch.Count()>0)
                    {
                        competitionDayUserReserchDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsReserch.Contains((int)x.CompetitionDayId) , new [] { "CompetitionDay" });

                    }
                    if(competitionDayIdsQuiz.Count()>0)
                    {
                        competitionDayUserQuizDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsQuiz.Contains((int)x.CompetitionDayId) , new [] { "CompetitionDay" });

                    }
                    if(competitionDayIdsMidTerm.Count()>0)
                    {
                        competitionDayUserMidTermDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsMidTerm.Contains((int)x.CompetitionDayId) , new [] { "CompetitionDay" });

                    }
                    if(competitionDayIdsFinal.Count()>0)
                    {
                        competitionDayUserFinalDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsFinal.Contains((int)x.CompetitionDayId) , new [] { "CompetitionDay" });

                    }
                    if(NameOfUser!=null)
                    {
                        //var usersIds = _unitOfWork.ApplicationUsers.FindAll(x => x.FirstName.Contains(encodedNameOfUser) || x.MiddleName.Contains(encodedNameOfUser)  || x.LastName.Contains(encodedNameOfUser)).Select(x=>x.Id);
                        StudentsDB=StudentsDB.Where(x => x.FirstName.Contains(encodedNameOfUser)||x.MiddleName.Contains(encodedNameOfUser)||x.LastName.Contains(encodedNameOfUser)).ToList();
                    }
                    var UsersPagedList = PagedList<HrUser>.Create(StudentsDB.AsQueryable().OrderByDescending(x => x.FirstName), PageNo, NoOfItems);


                    foreach(var item in UsersPagedList)
                    {
                        var tempData = new ResuiltStudentsInCompetition();
                        tempData.UserId=item.Id;
                        //  tempData.SerialNum=(item.SerialNum)!=null ? (item.SerialNum) : null;
                        tempData.UserName=item.FirstName+" "+item.MiddleName+" "+item.LastName;
                        tempData.lecturesDegree=(decimal)competitionDayUserLecturesDB.Where(x => x.HrUserId==item.Id).Sum(x => x.UserScore);
                        tempData.MissionList=competitionDayMission.Select(a => new IdandName
                        {

                            TypeId=2 ,
                            CompetitionDayId=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault())!=null ? (competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault()) : null ,
                            NameOfCometitionDay=a.Name ,
                            Degree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault())!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault()) : 0 ,

                        }).ToList();


                        tempData.ResearchList=competitionDayReserch.Select(a => new IdandName
                        {
                            TypeId=2 ,
                            CompetitionDayId=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault())!=null ? (competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault()) : null ,
                            NameOfCometitionDay=a.Name ,
                            Degree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault())!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault()) : 0 ,

                        }).ToList();
                        tempData.QuizList=competitionDayQuiz.Select(a => new IdandName
                        {
                            TypeId=2 ,
                            CompetitionDayId=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault())!=null ? (competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault()) : null ,
                            NameOfCometitionDay=a.Name ,
                            Degree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault())!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault()) : 0 ,

                        }).ToList();
                        tempData.MidtermList=competitionDayMidTerm.Select(a => new IdandName
                        {
                            TypeId=2 ,
                            CompetitionDayId=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault())!=null ? (competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault()) : null ,
                            NameOfCometitionDay=a.Name ,
                            Degree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault())!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault()) : 0 ,

                        }).ToList();
                        tempData.FinalexamList=competitionDayFinal.Select(a => new IdandName
                        {
                            TypeId=2 ,
                            CompetitionDayId=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault())!=null ? (competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault()) : null ,
                            NameOfCometitionDay=a.Name ,
                            Degree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault())!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault()) : 0 ,

                        }).ToList();
                        tempData.TotalDegree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id).Sum(y => y.UserScore))!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id).Sum(y => y.UserScore)) : 0;



                        Data.Add(tempData);
                    }





                    Response.data=Data;
                    Response.CorrectionDone=competition.CorrectionDone!=null ? (bool)competition.CorrectionDone : false;
                    Response.PaginationHeader=new PaginationHeader
                    {
                        CurrentPage=PageNo ,
                        TotalPages=UsersPagedList.TotalPages ,
                        ItemsPerPage=NoOfItems ,
                        TotalItems=UsersPagedList.TotalCount
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

        [HttpGet("ResuiltStudentOfCompetition")]
        public async Task<IActionResult> ResuiltStudentOfCompetition([FromHeader] int competitionId , [FromHeader] long HrUserId)
        {

            BaseResponseWithData<ResuiltStudentOfCompetition> Response = new BaseResponseWithData<ResuiltStudentOfCompetition>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var competitionDayUserLecturesDB = new List<CompetitionDayUser>();
                    var competitionDayUserMissionDB = new List<CompetitionDayUser>();
                    var competitionDayUserReserchDB = new List<CompetitionDayUser>();
                    var competitionDayUserQuizDB = new List<CompetitionDayUser>();
                    var competitionDayUserMidTermDB = new List<CompetitionDayUser>();
                    var competitionDayUserFinalDB = new List<CompetitionDayUser>();
                    var Data = new List<ResuiltStudentsInCompetition>();


                    var competition = _unitOfWork.Competitions.FindAll(x=>x.Id == competitionId).FirstOrDefault();



                    var competitionTypeDB = _unitOfWork.CompetitionTypes.FindAll(x=>x.Id > 0);
                    var StudentsDB = _unitOfWork.CompetitionUsers.FindAll(x=>x.CompetitionId == competitionId && x.EnrollmentStatus =="approved" && x.DelayOrWithdrawalStatus!= "delay" && x.DelayOrWithdrawalStatus !="Withdraw",new [] { "HrUser" } ).Select(y=>y.HrUser).ToList();
                    var competitionDayIdsLectures = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 1).Select(y=>y.Id);
                    var competitionDayMission = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 2);
                    var competitionDayIdsMission = competitionDayMission.Select(y=>y.Id);
                    var competitionDayReserch = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 3);
                    var competitionDayIdsReserch = competitionDayReserch.Select(y=>y.Id);
                    var competitionDayQuiz = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 4);
                    var competitionDayIdsQuiz = competitionDayQuiz.Select(y=>y.Id);
                    var competitionDayMidTerm = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 5);
                    var competitionDayIdsMidTerm = competitionDayMidTerm.Select(y=>y.Id);
                    var competitionDayFinal = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId && x.TypeId == 6);
                    var competitionDayIdsFinal = competitionDayFinal.Select(y=>y.Id);
                    var AllCompetitionDayIds = _unitOfWork.CompetitionDays.FindAll(x=>x.CompetitionId == competitionId).Select(x=>x.Id);
                    var competitiondayuserDB = _unitOfWork.CompetitionDayUsers.FindAll(X => (AllCompetitionDayIds.Contains((int)X.CompetitionDayId)) );
                    if(competitionDayIdsLectures.Count()>0)
                    {
                        competitionDayUserLecturesDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsLectures.Contains((int)x.CompetitionDayId));

                    }
                    if(competitionDayIdsMission.Count()>0)
                    {
                        competitionDayUserMissionDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsMission.Contains((int)x.CompetitionDayId) , new [] { "CompetitionDay" });

                    }
                    if(competitionDayIdsReserch.Count()>0)
                    {
                        competitionDayUserReserchDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsReserch.Contains((int)x.CompetitionDayId) , new [] { "CompetitionDay" });

                    }
                    if(competitionDayIdsQuiz.Count()>0)
                    {
                        competitionDayUserQuizDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsQuiz.Contains((int)x.CompetitionDayId) , new [] { "CompetitionDay" });

                    }
                    if(competitionDayIdsMidTerm.Count()>0)
                    {
                        competitionDayUserMidTermDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsMidTerm.Contains((int)x.CompetitionDayId) , new [] { "CompetitionDay" });

                    }
                    if(competitionDayIdsFinal.Count()>0)
                    {
                        competitionDayUserFinalDB=(List<CompetitionDayUser>)_unitOfWork.CompetitionDayUsers.FindAll(x => competitionDayIdsFinal.Contains((int)x.CompetitionDayId) , new [] { "CompetitionDay" });

                    }

                    var item = _unitOfWork.HrUsers.FindAll(x => x.Id == HrUserId).FirstOrDefault();



                    var tempData = new ResuiltStudentOfCompetition();
                    tempData.UserId=item.Id;
                    //tempData.SerialNum=(item.SerialNum)!=null ? (item.SerialNum) : null;
                    tempData.UserName=item.FirstName+" "+item.MiddleName+" "+item.LastName;
                    tempData.lecturesDegree=(decimal)competitionDayUserLecturesDB.Where(x => x.HrUserId==item.Id).Sum(x => x.UserScore);
                    tempData.MissionList=competitionDayMission.Select(a => new IdandName
                    {

                        TypeId=2 ,
                        CompetitionDayId=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault())!=null ? (competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault()) : null ,
                        NameOfCometitionDay=a.Name ,
                        Degree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault())!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault()) : 0 ,

                    }).ToList();


                    tempData.ResearchList=competitionDayReserch.Select(a => new IdandName
                    {
                        TypeId=2 ,
                        CompetitionDayId=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault())!=null ? (competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault()) : null ,
                        NameOfCometitionDay=a.Name ,
                        Degree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault())!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault()) : 0 ,

                    }).ToList();
                    tempData.QuizList=competitionDayQuiz.Select(a => new IdandName
                    {
                        TypeId=2 ,
                        CompetitionDayId=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault())!=null ? (competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault()) : null ,
                        NameOfCometitionDay=a.Name ,
                        Degree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault())!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault()) : 0 ,

                    }).ToList();
                    tempData.MidtermList=competitionDayMidTerm.Select(a => new IdandName
                    {
                        TypeId=2 ,
                        CompetitionDayId=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault())!=null ? (competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault()) : null ,
                        NameOfCometitionDay=a.Name ,
                        Degree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault())!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault()) : 0 ,

                    }).ToList();
                    tempData.FinalexamList=competitionDayFinal.Select(a => new IdandName
                    {
                        TypeId=2 ,
                        CompetitionDayId=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault())!=null ? (competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.CompetitionDayId).FirstOrDefault()) : null ,
                        NameOfCometitionDay=a.Name ,
                        Degree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault())!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id&&x.CompetitionDayId==a.Id).Select(y => y.UserScore).FirstOrDefault()) : 0 ,

                    }).ToList();
                    tempData.TotalDegree=(competitiondayuserDB.Where(x => x.HrUserId==item.Id).Sum(y => y.UserScore))!=null ? ((decimal)competitiondayuserDB.Where(x => x.HrUserId==item.Id).Sum(y => y.UserScore)) : 0;

                    tempData.CorrectionDone=competition.CorrectionDone!=null ? (bool)competition.CorrectionDone : false;

                    tempData.DegreeOfCompetition=competition.SubjectScore;
                    tempData.gpa=_unitOfWork.ResultControls
                        .FindAll(x => x.UserId==ApplicationUserId&&x.CompetitionId==competitionId)
                        .Select(y => y.Gpa).FirstOrDefault();

                    Response.Data=tempData;
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



        [HttpPost("UpdateResuiltStudentsInCompetition")]
        public async Task<IActionResult> UpdateResuiltStudentsInCompetition(UpdateResuiltStudentsDto dto , [FromHeader] long HrUserId)
        {

            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var Data = new UpdateResuiltStudentsDto ();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var competitionuserDB = _unitOfWork.CompetitionDayUsers.FindAll(x=>x.Id > 0);
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

                    var userIsAdmin= (userRoles.Any()&&userRoles.Contains("admin"));
                   // var userIsStudent= (userRoles.Any()&&userRoles.Contains("student"));
                    var userIsDoctor = (userRoles.Any()&&userRoles.Contains( "doctor"));

                    



                    if(userIsAdmin==false&&userIsDoctor==false)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="ليس لديك صلاحية لهذا الامر";
                        Response.Errors.Add(error);
                        return BadRequest(Response);
                       
                    }

                    foreach(var item in dto.UpdateResuiltStudent)
                    {
                        foreach(var item_1 in item.UpdateDegree)
                        {
                            var competitionId = _unitOfWork.CompetitionDays.FindAll(x=>x.Id == item_1.competitionDayId).Select(y=>y.CompetitionId).FirstOrDefault();
                            var competition = _unitOfWork.Competitions.FindAll(x=>x.Id == competitionId).FirstOrDefault();
                            if(competition.CorrectionDone==true)
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.ErrorMSG="تم تصحيح هذة المادة لا يمكن التعديل عليها ";
                                Response.Errors.Add(error);
                                return BadRequest(Response);
                              
                            }
                            var competitionuser = competitionuserDB.Where(x=>x.HrUserId == item.userId && x.CompetitionDayId == item_1.competitionDayId).FirstOrDefault();
                            if(competitionuser!=null)
                            {
                                competitionuser.UserScore=item_1.degree;
                                _unitOfWork.CompetitionDayUsers.Update(competitionuser);
                            }
                        }
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

    }
}
