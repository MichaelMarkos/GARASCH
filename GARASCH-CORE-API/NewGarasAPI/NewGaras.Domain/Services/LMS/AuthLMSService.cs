using AutoMapper;
using Microsoft.AspNetCore.Hosting;

using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.LMS;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.LMS;
using NewGaras.Infrastructure.Models.LMS;


namespace NewGaras.Domain.Services.LMS
{
    public class AuthLMSService :  IAuthLMsService
    {

        private readonly IWebHostEnvironment _host;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IMailService _mailService;
        private readonly IUnitOfWork _unitOfWork;
        private GarasTestContext _Context;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HearderVaidatorOutput validation;
        public AuthLMSService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, GarasTestContext context,
                                         Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, INotificationService notificationService, IMailService mailService, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _Context = context;
            _environment = environment;
            _notificationService = notificationService;
            _mailService = mailService;
            _httpContextAccessor = httpContextAccessor;
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


        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png", ".jpeg", ".svg" };
        private new List<string> _allowedResourcesExtenstions = new List<string> { ".pdf", ".docs" };
        private long _maxAllowedPosterSize = 15728640;


        public DateTime TimeZoneEgypt()
        {
            TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            // Get the current datetime in Egypt
            DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            return egyptDateTime;
        }

        public async Task<List<string>> checkUserIsExistAsync(CheckUserIsExistModel model)
        {
            List<string> errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var CheckUserEmail =  _unitOfWork.HrUsers.FindAll(x=>x.Email == model.Email).FirstOrDefault();
                if (CheckUserEmail is not null && CheckUserEmail.Id !=  model.UserId)
                    errors.Add("هذا البريد الالكترونى مسجل بالفعل ");
            }

            //if (!string.IsNullOrWhiteSpace(model.UserName))
            //{

            //    var CheckUserName = _unitOfWork.HrUsers.FindAll(x => x.name == model.Email).FirstOrDefault();
            //    if (CheckUserName is not null && CheckUserName.Id != model.UserId)
            //        errors.Add("اسم المستخدم مسجل بالفعل");
            //}

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {

                var CheckUserPhoneIsExist = _unitOfWork.HrUserMobiles.FindAll(x => x.MobileNumber == model.PhoneNumber && x.Id != model.UserId).FirstOrDefault();
                if (CheckUserPhoneIsExist is not null)
                {
                    errors.Add("رقم الموبيل مسجل بالفعل");
                }
            }
            if (!string.IsNullOrWhiteSpace(model.NationalId))
            {

                var CheckUserNationalIdIsExist =_unitOfWork.HrUsers.FindAll(x => x.NationalId == model.NationalId && x.Id != model.UserId).FirstOrDefault();
                if (CheckUserNationalIdIsExist is not null)
                {
                    errors.Add("رقم البطاقة مسجل بالفعل");
                }
            }
            return errors;
        }


        public async Task<BaseResponseWithData<lectureTodayVM>> NextlectureToday(long HrUserId)
        {
            var response = new BaseResponseWithData<lectureTodayVM>();
            response.Errors = new List<Error>();
            response.Result = true;
            var _data = new lectureTodayVM();

            var competitions = _unitOfWork.Competitions.FindAll(x => x.Id > 0);
            try
            {

                TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                // Get the current datetime in Egypt
                DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);



                if (HrUserId <= 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid parameter";
                    response.Errors.Add(error);
                    return response;
                }
                var _user =  _unitOfWork.HrUsers.FindAllQueryable(x => x.Id == HrUserId ); //await _userManager.FindByIdAsync(ApplicationUserId);
                var rolesList =  _unitOfWork.UserRoles.FindAllQueryable(x=>x.UserId == HrUserId, new[] { "Role" }).Select(r=>r.Role.Name).ToList();

                if (_user.Any())
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "هذا المستخدم غير موجود";
                    response.Errors.Add(error);
                    return response;
                }

                //   _data=_mapper.Map<UserProfileDataModel>(_user);

                var competitionDayAtTime = new List<CompetitionDay>();

                if (rolesList.Contains("admin"))
                {
                    var competitionDaysDB = _unitOfWork.CompetitionDays.FindAll(x => x.Id > 0).ToList();

                    foreach (var item in competitions)
                    {

                        DateTime timeEg = TimeZoneEgypt();


                        competitionDayAtTime = competitionDaysDB.Where(x => x.CompetitionId == item.Id && ((DateTime)x.From).Date == egyptDateTime.Date && x.To >= egyptDateTime).OrderBy(y => y.To).ToList();



                        //    competitionDayAtTime=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&x.To>=timeEg).OrderBy(y => y.To).ToList();



                        if (competitionDayAtTime.Count() > 0)
                        {
                            _data.comppetitionDayAtTime = competitionDayAtTime

                           .Select(com => new FilterTabledDto
                           {
                               CompetitionId = (int)com.CompetitionId,
                               CompetitionName = competitions.Where(x => x.Id == com.CompetitionId).Select(y => y.Name).FirstOrDefault(),
                               CompetitionDayId = com.Id,
                               TypeId = com.TypeId,
                               Name = com.Name ?? null,
                               From = (com.From.ToString("yyyy-MM-dd HH:mm:ss")),
                               To = com.To.ToString("yyyy-MM-dd HH:mm:ss"),

                               NumberOfStudents = _unitOfWork.CompetitionUsers.FindAll(x => x.CompetitionId == item.Id).Count(),
                               NumberOfAttendce = _unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId == com.Id && x.Attendance == true).Count()
                           }).FirstOrDefault();
                        }


                    }



                    var competitionDayFirst = competitionDayAtTime.FirstOrDefault();
                    var competitionDayAtToday = new List<CompetitionDay>();
                    if (competitionDayFirst != null)
                    {
                        competitionDayAtToday = _unitOfWork.CompetitionDays.FindAll(x => x.Id == competitionDayFirst.Id && ((DateTime)x.From).Date >= ((DateTime)competitionDayFirst.From).Date).ToList();

                    }


                    if (competitionDayAtToday.Count() > 1 || (competitionDayAtToday.Count() == 1 && (competitionDaysDB.Where(x => ((DateTime)x.From).Date == egyptDateTime.Date && x.To <= egyptDateTime).Count() > 0)))
                    {
                        _data.comppetitionDayListFlag = true;
                    }
                    else
                    {
                        _data.comppetitionDayListFlag = false;
                    }

                }

                if (rolesList.Contains("doctor") || rolesList.Contains("adminCompetition"))
                {
                    var competitionsForDoctor = _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.HrUserId == HrUserId, new[] { "Competition" }).Select(y => y.Competition).ToList();
                    var competitionDaysDB = _unitOfWork.CompetitionDays.FindAll(x => x.Id > 0).ToList();


                    foreach (var item in competitionsForDoctor)
                    {

                        DateTime timeEg = TimeZoneEgypt();


                        competitionDayAtTime = competitionDaysDB.Where(x => x.CompetitionId == item.Id && ((DateTime)x.From).Date == egyptDateTime.Date && x.To >= egyptDateTime).OrderBy(y => y.To).ToList();


                        // competitionDayAtTime=_unitOfWork.CompetitionDays.FindAll(x => x.CompetitionId==item.Id&&x.To>=timeEg).OrderBy(y => y.To).ToList();


                        if (competitionDayAtTime.Count() > 0)
                        {
                            _data.comppetitionDayAtTime = competitionDayAtTime

                           .Select(com => new FilterTabledDto
                           {
                               CompetitionId = (int)com.CompetitionId,
                               CompetitionName = competitions.Where(x => x.Id == com.CompetitionId).Select(y => y.Name).FirstOrDefault(),
                               CompetitionDayId = com.Id,
                               TypeId = com.TypeId,
                               Name = com.Name ?? null,
                               From = (com.From.ToString("yyyy-MM-dd HH:mm:ss")),
                               To = com.To.ToString("yyyy-MM-dd HH:mm:ss"),

                               NumberOfStudents = _unitOfWork.CompetitionUsers.FindAll(x => x.CompetitionId == item.Id).Count(),
                               NumberOfAttendce = _unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId == com.Id && x.Attendance == true).Count()
                           }).FirstOrDefault();
                        }

                    }
                    var competitionDayFirst = competitionDayAtTime.FirstOrDefault();
                    var competitionDayAtToday = new List<CompetitionDay>();
                    if (competitionDayFirst != null)
                    {
                        competitionDayAtToday = _unitOfWork.CompetitionDays.FindAll(x => x.Id == competitionDayFirst.Id && ((DateTime)x.From).Date >= ((DateTime)competitionDayFirst.From).Date).ToList();

                    }


                    if (competitionDayAtToday.Count() > 1 || (competitionDayAtToday.Count() == 1 && (competitionDaysDB.Where(x => (competitionsForDoctor.Select(y => y.Id).ToList().Contains((int)x.CompetitionId)) && ((DateTime)x.From).Date == egyptDateTime.Date && x.To <= egyptDateTime).Count() > 0)))
                    {
                        _data.comppetitionDayListFlag = true;
                    }
                    else
                    {
                        _data.comppetitionDayListFlag = false;
                    }



                }

                if (rolesList.Contains("student"))
                {

                    var competitionIds = _unitOfWork.CompetitionUsers.FindAll(x => x.HrUserId == HrUserId && x.FinishStatus != "Finished" && x.EnrollmentStatus == "approved" && x.DelayOrWithdrawalStatus != "Withdraw" && x.DelayOrWithdrawalStatus != "delay").Select(y => y.CompetitionId).ToList();

                    //var id = item.Id;
                    var timeEg = TimeZoneEgypt();
                   


                    var competitionDaysDB = _unitOfWork.CompetitionDays.FindAll(x => x.Id > 0).ToList();

                    competitionDayAtTime = competitionDaysDB.Where(x => (competitionIds.Contains(x.CompetitionId)) && ((DateTime)x.From).Date == egyptDateTime.Date && x.To >= egyptDateTime).OrderBy(y => y.To).ToList();
                    var competitionDayAtToday = _unitOfWork.CompetitionDays.FindAll(x => (competitionIds.Contains(x.CompetitionId)) && ((DateTime)x.From).Date == egyptDateTime.Date).ToList();


                    if (competitionDayAtTime.Count() > 0)
                    {
                        _data.comppetitionDayAtTime = competitionDayAtTime

                       .Select(com => new FilterTabledDto
                       {
                           CompetitionId = (int)com.CompetitionId,
                           CompetitionName = competitions.Where(x => x.Id == com.CompetitionId).Select(y => y.Name).FirstOrDefault(),
                           CompetitionDayId = com.Id,
                           TypeId = com.TypeId,
                           Name = com.Name ?? null,
                           From = (com.From.ToString("yyyy-MM-dd HH:mm:ss")),
                           To = com.To.ToString("yyyy-MM-dd HH:mm:ss"),
                           AttendanceFlag = _unitOfWork.CompetitionDayUsers.FindAll(x => x.CompetitionDayId == com.Id && x.HrUserId == HrUserId && x.Attendance == true).Any()


                       }).FirstOrDefault();
                    }

                    var competitionDayFirst = competitionDayAtTime.FirstOrDefault();
                    // var competitionDayAtToday = new List<CompetitionDay>();
                    if (competitionDayFirst != null)
                    {
                        competitionDayAtToday = _unitOfWork.CompetitionDays.FindAll(x => x.Id == competitionDayFirst.Id && ((DateTime)x.From).Date >= ((DateTime)competitionDayFirst.From).Date).ToList();

                    }


                    if (competitionDayAtTime.Count() > 1 || (competitionDayAtToday.Count() >= 1 && (competitionDaysDB.Where(x => (competitionIds.Contains(x.CompetitionId)) && ((DateTime)x.From).Date == egyptDateTime.Date && x.To <= egyptDateTime).Count() > 0)))
                    {
                        _data.comppetitionDayListFlag = true;
                    }
                    else
                    {
                        _data.comppetitionDayListFlag = false;
                    }
                    _data.NumberOfMission = competitionDaysDB.Where(x => (competitionIds.Contains(x.CompetitionId)) && x.To >= egyptDateTime && (x.TypeId == 2 || x.TypeId == 3)).Count();
                    _data.NumberOfQuies = competitionDaysDB.Where(x => (competitionIds.Contains(x.CompetitionId)) && x.To >= egyptDateTime && (x.TypeId == 4 || x.TypeId == 5 || x.TypeId == 6)).Count();
                    _data.NumberOfNotices = _unitOfWork.Notices.FindAll(x => x.Date >= egyptDateTime).Count();

                }


                ///_data.Roles=(List<string>)rolesList;
                response.Data = _data;
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
        public BaseResponseWithData<long> CovertUserIdToHrUserId(long UserId)
        {
            var response = new BaseResponseWithData<long>();
            response.Errors = new List<Error>();
            response.Result = true;
            var HrUserId = _unitOfWork.HrUsers.FindAll(x => x.UserId == UserId).Select(h=> h.Id).FirstOrDefault();

            if (HrUserId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Invalid parameter";
                response.Errors.Add(error);
                return response;
            }
            response.Data = HrUserId;
            return response;
        }



    }
}
