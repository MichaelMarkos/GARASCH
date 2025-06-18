using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Attendance;
using NewGaras.Infrastructure.DTO.TaskProgress;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Attendance;
using NewGaras.Infrastructure.Models.Mail;
using NewGaras.Infrastructure.Models.Payroll.Filters;
using NewGaras.Infrastructure.Models.Payroll;
using NewGarasAPI.Models.HR;
using DocumentFormat.OpenXml.Spreadsheet;
using NewGaras.Infrastructure.DTO.Attendance.Payroll;
using NewGaras.Infrastructure.Helper.TenantService;

namespace NewGarasAPI.Controllers.HR
{
    [Route("HR/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {

        private readonly IAttendanceService _attendanceService;
        private readonly ITaskMangerProjectService _taskMangerProjectService;
        private readonly IMailService _mailService;
        private readonly IContractService _contractService;
        private readonly INotificationService _notificationService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public AttendanceController(IAttendanceService attendanceService, ITaskMangerProjectService taskMangerProjectService, IMailService mailService, IContractService contractService, ITenantService tenantService, INotificationService notificationService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _attendanceService = attendanceService;
            _helper = new Helper.Helper();
            _taskMangerProjectService = taskMangerProjectService;
            _mailService = mailService;
            _contractService = contractService;
            _notificationService = notificationService;
        }
        [HttpGet("GetHrUserAttendence")]
        public BaseResponseWithDataAndHeader<List<UserAttendance>> GetHrUserAttendence([FromHeader] GetEmployeeAttendenceHeader header)
        {
            BaseResponseWithDataAndHeader<List<UserAttendance>> Response = new BaseResponseWithDataAndHeader<List<UserAttendance>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _attendanceService.GetHrUserAttendence(header);
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

        [HttpGet("GetHrUserAttendanceList")]
        public async Task<BaseResponseWithDataAndHeader<List<UserAttendance>>> GetHrUserAttendanceList([FromHeader] GetUserAttendanceListHeader header)
        {
            BaseResponseWithDataAndHeader<List<UserAttendance>> Response = new BaseResponseWithDataAndHeader<List<UserAttendance>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _attendanceService.GetHrUserAttendanceList(header);
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

        [HttpPost("InsertWorkingTrackingByAttendance")]
        public BaseResponseWithId<long> InsertWorkingTrackingByAttendance([FromForm] AddTrackingByDailyAttendanceDto request)
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
                    Response = _attendanceService.AddWorkingHoursTracking(request, validation.userID);
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

        [HttpPost("UpdateWorkingHoursTrackingWithCheckout")]
        public BaseResponseWithId<long> UpdateWorkingHoursTrackingWithCheckout([FromForm] UpdateWorkingHoursTrackingWithCheckoutDto request)
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
                    Response = _attendanceService.UpdateWorkingHoursTrackingWithCheckout(request, validation.userID);
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
        [HttpPost("InsertWorkingTrackingByTask")]
        public BaseResponseWithId<long> InsertWorkingTrackingByTask([FromForm] AddTrackingByDailyTaskDto request)
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
                    if (request.TaskId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "Invalid TaskId";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var sd = _Context.TaskRequirements.ToList();
                    Response = _attendanceService.AddWorkingHoursTrackingByTask(request, validation.userID);
                    if (Response.Result)
                    {
                        //var task = _Context.Tasks.Find(request.TaskId);
                        //if (task != null)
                        //{
                        //    if (task.ProjectId != null)
                        //    {
                        //        var ProjectUsers = _taskMangerProjectService.GetUsersOfProjects((long)task.ProjectId).Data.ManagerUsersList;
                        //        if (ProjectUsers.Count() > 0)
                        //        {
                        //            foreach (var a in ProjectUsers)
                        //            {
                        //                _notificationService.CreateNotification(validation.userID, "Task Progress Need Approval", task.Description, Response.ID.ToString(), true, a.UserId, 0);
                        //                //var user = _Context.Users.Find(a.UserId);
                        //                //if (user != null)
                        //                //{
                        //                //    _mailService.SendMail(new MailData() { EmailToName = user.Email, EmailToId = user.Email, EmailSubject = "Task Progress Need Approval", EmailBody = "" });
                        //                //}
                        //            }
                        //        }
                        //        else
                        //        {
                        //            _notificationService.CreateNotification(validation.userID, "Task Progress Need Approval", task.Description, Response.ID.ToString(), true, task.CreatedBy, 0);
                        //            //var user = _Context.Users.Find(task.CreatedBy);
                        //            //if (user != null)
                        //            //{

                        //            //    _mailService.SendMail(new MailData() { EmailToName = user.Email, EmailToId = user.Email, EmailSubject = "Task Progress Need Approval", EmailBody = "" });
                        //            //}
                        //        }
                        //    }
                        //    else
                        //    {
                        //        _notificationService.CreateNotification(validation.userID, "Task Progress Need Approval", task.Description, Response.ID.ToString(), true, task.CreatedBy, 0);
                        //        //var user = _Context.Users.Find(task.CreatedBy);
                        //        //if (user != null)
                        //        //{

                        //        //    _mailService.SendMail(new MailData() { EmailToName = user.Email, EmailToId = user.Email, EmailSubject = "Task Progress Need Approval", EmailBody = "" });
                        //        //}
                        //    }
                        //}
                        //else
                        //{
                        //    Response.Result = false;
                        //    Error error = new Error();
                        //    error.ErrorCode = "Err102";
                        //    error.ErrorMSG = "Task is not exist";
                        //    Response.Errors.Add(error);
                        //    return Response;
                        //}
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

        [HttpGet("GetTaskProgressForUserList")]
        public BaseResponseWithData<List<GetTaskProgressForUser>> GetTaskProgressForUserList([FromHeader] long TaskId)
        {
            BaseResponseWithData<List<GetTaskProgressForUser>> Response = new BaseResponseWithData<List<GetTaskProgressForUser>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.GetTaskProgressForUserList(TaskId);
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

        [HttpGet("GetTaskProgress")]
        public BaseResponseWithData<GetTaskProgressForUser> GetTaskProgress([FromHeader] long ProgressId)
        {
            BaseResponseWithData<GetTaskProgressForUser> Response = new BaseResponseWithData<GetTaskProgressForUser>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.GetTaskProgress(ProgressId);
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

        [HttpPost("TaskProgressApprove")]
        public BaseResponseWithId<long> TaskProgressApprove([FromBody] TaskProgressApprovalDto request)
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
                    Response = _attendanceService.ApproveTaskPrpgress(request, validation.userID);
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
        /*[HttpPost("InsertWorkingTrackingByEnteringLocation")]
        public BaseResponseWithId<long> InsertWorkingTrackingByEnteringLocation([FromForm] AddTrackingByLocationDto request)
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
                    Response = _attendanceService.AddWorkingHoursTrackingByEnterLocation(request, validation.userID);
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

        [HttpGet("GetMonthlyAttendanceSummary")]
        public BaseResponseWithData<GetMonthlyAttendanceModel> GetAttendanceSummary([FromHeader] int year, [FromHeader] int month, [FromHeader] int BranchId, [FromHeader] int? DepartmentId)
        {
            BaseResponseWithData<GetMonthlyAttendanceModel> Response = new BaseResponseWithData<GetMonthlyAttendanceModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.GetMonthlyAttendance(month, year, BranchId, DepartmentId);
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

        [HttpPost("RequestAbsenceForUser")]
        public BaseResponseWithData<RequestAbsenceResponse> RequestAbsenceForUser([FromBody] RequestAbsenceDto request)
        {
            BaseResponseWithData<RequestAbsenceResponse> Response = new BaseResponseWithData<RequestAbsenceResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.RequestAbsence(request, validation.userID);
                    if (Response.Result)
                    {
                        var leaverequest = _Context.LeaveRequests.Find(Response.Data.Id);
                        //if (Response.Data.FirstReportToId != 0)
                        //{
                        //    _notificationService.CreateNotification(validation.userID, "vacation Need Approval", "vacation test Des.", Response.Data.Id.ToString(), // Attendance Id
                        //        true, Response.Data.FirstReportToId, 0);
                        //    var user = _Context.Users.Find(Response.Data.FirstReportToId);
                        //    if (user != null)
                        //    {
                        //        _mailService.SendMail(new MailData() { EmailToName = user.Email, EmailToId = user.Email, EmailSubject = "vacation Need Approval", EmailBody = "" });
                        //    }
                        //}
                        /*if (Response.Data.SecondReportToId != 0)
                        {
                            Common.CreateNotification(validation.userID, "vacation Need Approval", "vacation test Des.", Response.Data.Id.ToString(), // Attendance Id
                                true, Response.Data.SecondReportToId, 0, _Context);
                            var user = _Context.Users.Find(Response.Data.SecondReportToId);
                            if (user != null)
                            {
                                _mailService.SendMail(new MailData() { EmailToName = user.Email, EmailToId = user.Email, EmailSubject = "vacation Need Approval", EmailBody = "" });
                            }
                        }*/
                        //var hruser = _Context.HrUsers.Find(request.HrUserId);
                        //if (hruser != null && hruser.UserId != null)
                        //{
                        //    _notificationService.CreateNotification(validation.userID, "Vacation Request Is Sent", "vacation test Des.", Response.Data.Id.ToString(), // Attendance Id
                        //        true, hruser.UserId, 0);
                        //    var user = _Context.Users.Find(hruser.UserId);
                        //    _mailService.SendMail(new MailData() { EmailToName = user.Email, EmailToId = user.Email, EmailSubject = "vacation Request Sent", EmailBody = "" });
                        //}
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

        [HttpPost("ApproveAbsenceForUser")]
        public BaseResponseWithData<ApproveAbsenceResponse> ApproveAbsenceForUser([FromBody] ApproveAbsenceModel request)
        {
            BaseResponseWithData<ApproveAbsenceResponse> Response = new BaseResponseWithData<ApproveAbsenceResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.ApproveAbsence(request, validation.userID);

                    var HrJobTitle = _Context.JobTitles.Where(a => a.Name == "HR").Select(a => a.Id).FirstOrDefault();
                    var HRAdmin = _Context.Users.Where(a => a.JobTitleId == (HrJobTitle)).FirstOrDefault();

                    if(HRAdmin != null)
                    {
                        _notificationService.CreateNotification(validation.userID, "Vacation", "Request Approved", Response.Data.RequestId.ToString(), // Attendance Id
                            true, HRAdmin.Id, 0);
                        _mailService.SendMail(new MailData() { EmailToName = HRAdmin.Email, EmailToId = HRAdmin.Email, EmailSubject = "vacation Request Approved", EmailBody = "" });
                    }

                    var hruser = _Context.HrUsers.Find(Response.Data.HrUserId);
                    if (Response.Data.SecondReportTo != 0)
                    {
                        _notificationService.CreateNotification(validation.userID, "Vacation", "Need Approval", Response.Data.RequestId.ToString(), // Attendance Id
                            true, Response.Data.SecondReportTo, 0);
                        var user = _Context.Users.Find(Response.Data.SecondReportTo);
                        if (user != null)
                        {
                            _mailService.SendMail(new MailData() { EmailToName = user.Email, EmailToId = user.Email, EmailSubject = "Vacation Need Approval", EmailBody = "" });
                        }
                    }
                    if (hruser != null && hruser.UserId != null)
                    {
                        _notificationService.CreateNotification(validation.userID, "Vacation", "Approval Is Sent", Response.Data.RequestId.ToString(), // Attendance Id
                            true, hruser.UserId, 0);
                        var user = _Context.Users.Find(hruser.UserId);
                        if (user != null)
                        {
                            _mailService.SendMail(new MailData() { EmailToName = user.Email, EmailToId = user.Email, EmailSubject = "vacation Approval Sent", EmailBody = "" });
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

        [HttpGet("GetRequestedAbsenceBalance")]
        public BaseResponseWithData<GetRequestedAbsenceBalanceModel> GetRequestedAbsenceBalance([FromHeader] long requestId)
        {
            BaseResponseWithData<GetRequestedAbsenceBalanceModel> Response = new BaseResponseWithData<GetRequestedAbsenceBalanceModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.GetRequestedAbsenceBalance(requestId);
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

        [HttpGet("GetProgressForAllTasks")]
        public BaseResponseWithData<GetProgressForAllTasks> GetProgressForAllTasks([FromHeader] ProgressForAllTaskFilter filters)
        {
            BaseResponseWithData<GetProgressForAllTasks> Response = new BaseResponseWithData<GetProgressForAllTasks>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.GetProgressForAllTask(filters, validation.CompanyName);
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

        [HttpPost("UpdateTaskProgress")]
        public BaseResponseWithId<long> UpdateTaskProgress([FromForm] UpdateTaskProgressDto request)
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
                    Response = _attendanceService.UpdateTaskProgress(request, validation.userID);
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

        [HttpGet("GetWorkingHoursForTask")]
        public BaseResponseWithData<List<GetProgressForAllTasksDto>> GetWorkingHoursForTask([FromHeader] long TaskId, [FromHeader] long? HrUserId)
        {
            BaseResponseWithData<List<GetProgressForAllTasksDto>> Response = new BaseResponseWithData<List<GetProgressForAllTasksDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.GetWorkingHoursForTask(TaskId, HrUserId);
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

        [HttpGet("GetOpenWorkingHoursForAllTasks")]
        public BaseResponseWithData<List<GetOpenWorkingHoursForAllTasksDto>> GetOpenWorkingHoursForAllTasks([FromHeader] long HrUserId)
        {
            BaseResponseWithData<List<GetOpenWorkingHoursForAllTasksDto>> Response = new BaseResponseWithData<List<GetOpenWorkingHoursForAllTasksDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.GetOpenWorkingHoursForAllTasks(HrUserId);
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

        [HttpGet("GetMonthAttendaceSummaryForHrUser")]
        public BaseResponseWithData<List<GetMonthAttendaceSummaryForHrUserDto>> GetMonthAttendaceSummaryForHrUser([FromHeader] long HrUserID, [FromHeader] int month, [FromHeader] int year)
        {
            BaseResponseWithData<List<GetMonthAttendaceSummaryForHrUserDto>> response = new BaseResponseWithData<List<GetMonthAttendaceSummaryForHrUserDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;

            #region validaton
            if (HrUserID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Please Enter A valid user Id";
                response.Errors.Add(error);
                return response;
            }
            if (month == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Please Enter A valid Month";
                response.Errors.Add(error);
                return response;
            }
            if (month == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Please Enter A valid Year";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _attendanceService.MonthAttendaceSummaryForHrUser(HrUserID, month, year);
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

        [HttpGet("GetProgressByUserAndDate")]
        public BaseResponseWithData<List<GetProgressForTaskDto>> GetProgressByUser([FromHeader] long HrUserID, [FromHeader] int month, [FromHeader] int year, [FromHeader] int day)
        {
            BaseResponseWithData<List<GetProgressForTaskDto>> response = new BaseResponseWithData<List<GetProgressForTaskDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;

            #region validaton
            if (HrUserID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Please Enter A valid user Id";
                response.Errors.Add(error);
                return response;
            }
            if (month == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Please Enter A valid Month";
                response.Errors.Add(error);
                return response;
            }
            if (month == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Please Enter A valid Year";
                response.Errors.Add(error);
                return response;
            }
            #endregion
            try
            {
                if (response.Result)
                {
                    response = _attendanceService.GetProgressByUser(HrUserID, month, year, day);
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

        [HttpGet("GetAttendanceByDay")]
        public BaseResponseWithData<GetAttendanceByDayModel> GetAttendanceByDay([FromHeader] DateTime AttendanceDate, [FromHeader] int? BranchId, [FromHeader] int? DepartmentId)
        {
            BaseResponseWithData<GetAttendanceByDayModel> Response = new BaseResponseWithData<GetAttendanceByDayModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.GetAttendanceByDay(AttendanceDate, BranchId, DepartmentId);
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

        [HttpPost("UpdateTaskWorkingHours")]
        public BaseResponseWithData<UpdateTaskWorkingHoursResponse> UpdateTaskWorkingHours([FromHeader] long ProgressId, [FromHeader] bool ContinueWorking)
        {
            BaseResponseWithData<UpdateTaskWorkingHoursResponse> Response = new BaseResponseWithData<UpdateTaskWorkingHoursResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.UpdateTaskWorkingHours(ProgressId, ContinueWorking, validation.userID);
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

        [HttpGet("GeneratePayrollPdfForUser")]
        public BaseResponseWithData<PayRollDataModel> GeneratePayrollPdfForUser([FromHeader] string monthYear, [FromHeader] long HrUserId)
        {
            BaseResponseWithData<PayRollDataModel> Response = new BaseResponseWithData<PayRollDataModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _attendanceService.GeneratePayrollPdfForUser(monthYear, HrUserId);
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


        [HttpGet("GetMonthlyPayrollReport")]
        public BaseResponseWithData<GetMonthlyPayrollReport> GetMonthlyPayrollReport([FromHeader] GetMonthlyPayrollReportFilters filters)
        {
            BaseResponseWithData<GetMonthlyPayrollReport> response = new BaseResponseWithData<GetMonthlyPayrollReport>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region Validation
            if (filters.BranchID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Please Enter A valid Branch ID";
                response.Errors.Add(error);
                return response;
            }
            if (filters.Year == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Please Enter A valid year";
                response.Errors.Add(error);
                return response;
            }
            if (filters.Month == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Please Enter A valid month";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var payrollList = _attendanceService.GetMonthlyPayrollReport(filters, validation.CompanyName);
                    if (!payrollList.Result)
                    {
                        response.Errors.AddRange(payrollList.Errors);
                        return response;
                    }
                    response = payrollList;
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

        [HttpPost("UpdatePayRollStatus")]
        public BaseResponseWithId<long> UpdatePayRollStatus([FromForm] bool AllUsers, [FromForm] List<long> usersList, [FromForm] int branchId)
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
                    Response = _attendanceService.UpdatePayRollStatus(AllUsers, usersList, branchId);
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

        [HttpPost("AddNewAttendanceRecord")]
        public BaseResponseWithId<long> AddAttendance([FromForm] AddAttendanceModel request)
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
                    Response = _attendanceService.AddAttendance(request, validation.userID);
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

        [HttpPost("UpdateAttendanceRecord")]
        public BaseResponseWithId<long> UpdateAttendance([FromForm] AddAttendanceModel request)
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
                    Response = _attendanceService.UpdateAttendance(request, validation.userID);
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

        [HttpGet("GetHeadersOfAttendaceSummaryForHrUser")]
        public BaseResponseWithData<GetHeadersOfAttendaceSummaryForHrUserDto> GetHeadersOfAttendaceSummaryForHrUser([FromHeader] long HrUser, [FromHeader] int Month, [FromHeader] int Year)
        {
            BaseResponseWithData<GetHeadersOfAttendaceSummaryForHrUserDto> response = new BaseResponseWithData<GetHeadersOfAttendaceSummaryForHrUserDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    #region validation
                    var user = _Context.HrUsers.Where(x => x.Id == HrUser).FirstOrDefault();
                    if (user == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "This User not found";
                        response.Errors.Add(error);
                        return response;
                    }
                    /*if(BranchId == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Please Enter A valid Branch ID";
                        response.Errors.Add(error);
                        return response;
                    }*/
                    if (Month == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Please Enter A valid Month Number";
                        response.Errors.Add(error);
                        return response;
                    }
                    if (Year == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Please Enter A valid Year";
                        response.Errors.Add(error);
                        return response;
                    }
                    #endregion

                    var headersForMonthAttendance = _attendanceService.GetHeadersOfAttendaceSummaryForHrUser(1, Month, Year, HrUser);
                    if (!headersForMonthAttendance.Result)
                    {
                        response.Errors.AddRange(headersForMonthAttendance.Errors);
                        return response;
                    }
                    response = headersForMonthAttendance;
                }

            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
            }
            return response;



        }

        [HttpGet("GetPeriodAttendanceByPayRoll")]
        public BaseResponseWithData<List<GetPeriodAttendance>> GetPeriodAttendance([FromHeader] long payRollID)
        {
            BaseResponseWithData<List<GetPeriodAttendance>> response = new BaseResponseWithData<List<GetPeriodAttendance>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region validation 
            if (payRollID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "E-2";
                error.ErrorMSG = "please Enter a valid Pay Roll ID";
                response.Errors.Add(error);
            }
            #endregion

            try
            {
                var periodAttend = _attendanceService.GetPeriodAttendance(payRollID);
                if (!periodAttend.Result)
                {
                    response.Result = false;
                    response.Errors.AddRange(periodAttend.Errors);
                    return response;
                }
                response = periodAttend;
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

        [HttpGet("GetPeriodAbsenceByPayRoll")]
        public BaseResponseWithData<List<GetPeriodAbsenceDto>> GetPeriodAbsence([FromHeader] long payRollID)
        {
            BaseResponseWithData<List<GetPeriodAbsenceDto>> response = new BaseResponseWithData<List<GetPeriodAbsenceDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region validation 
            if (payRollID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "E-2";
                error.ErrorMSG = "please Enter a valid Pay Roll ID";
                response.Errors.Add(error);
            }
            #endregion

            try
            {
                var periodAttend = _attendanceService.GetPeriodAbsence(payRollID);
                if (!periodAttend.Result)
                {
                    response.Result = false;
                    response.Errors.AddRange(periodAttend.Errors);
                    return response;
                }
                response = periodAttend;
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

        [HttpPost("DownloadAttendanceSheet")]
        public BaseResponseWithData<string> DownloadAttendanceSheet([FromForm] bool AllUsers, [FromForm] List<long> usersList, [FromForm] int branchId)
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
                    Response = _attendanceService.DownloadAttendanceSheet(AllUsers, usersList, branchId, validation.userID, validation.CompanyName);
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
        [HttpPost("UploadAttendanceSheet")]
        public BaseResponseWithId<long> UploadAttendanceSheet([FromForm] UploadAttendanceSheetFile AttendanceSheetFile)
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
                    Response = _attendanceService.UploadAttendanceSheet(AttendanceSheetFile, validation.userID, validation.CompanyName);
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


        [HttpPost("DeleteWorkingHours")]
        public BaseResponseWithId<long> DeleteTaskProgress([FromHeader] long ProgressId)
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
                    Response = _attendanceService.DeleteTaskProgress(ProgressId, validation.userID);
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

        [HttpGet("GetAttendanceReport")]
        public BaseResponseWithData<string> GetAttendanceReport(GetAttendanceReportFilters filters)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = _attendanceService.GetAttendanceReport(filters, validation.CompanyName);
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

        [HttpPost("AddDeductionWorkingHours")]
        public BaseResponseWithId<long> AddDeductionWorkingHours(DeductWorkingHoursModel request)
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
                    _attendanceService.Validation = validation;
                    Response = _attendanceService.AddDeductionWorkingHours(request);
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

        [HttpPost("AddMissingAttendanceForUsers")]
        public BaseResponseWithId<long> AddDeductionWorkingHours([FromHeader] long HrUserId, [FromHeader] DateTime date)
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
                    _attendanceService.Validation = validation;
                    Response = _attendanceService.SumWorkingTrackingHoursForAttendance(HrUserId, date, validation.userID);
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

        [HttpPost("AddHolidayToBranchAttendance")]
        public BaseResponseWithId<long> AddHolidayToBranchAttendance([FromForm] AddHolidayToBranchAttendanceModel data)
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
                    _attendanceService.Validation = validation;
                    Response = _attendanceService.AddHolidayToBranchAttendance(data, validation.userID);
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
