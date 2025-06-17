using AutoMapper;
using Azure;
using iTextSharp.text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Consts;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Attendance;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Attendance;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.User;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.AspNetCore.Hosting;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using NewGaras.Infrastructure.DTO.TaskProgress;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = NewGarasAPI.Models.Common.Error;
using System.Diagnostics.Contracts;
using NewGaras.Infrastructure.Models.Payroll;
using NewGaras.Infrastructure.Models.Payroll.Filters;
using NewGaras.Infrastructure.DTO.Attendance.Payroll;
using MailKit.Search;
using NewGaras.Infrastructure.DTO.Salary;
using NewGaras.Infrastructure.Models.SalaryAllownce;
using NewGaras.Infrastructure.DTO.Salary.SalaryTax;
using NewGaras.Infrastructure.DTO.BranchSetting;
using System.Data;
using NewGaras.Infrastructure.DTO.TaskMangerProject;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Threading;
using NewGarasAPI.Models.Project.UsedInResponses;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Models.Mail;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using Color = System.Drawing.Color;
using SixLabors.ImageSharp.PixelFormats;
using DocumentFormat.OpenXml.Office2010.Excel;
using Org.BouncyCastle.Bcpg;
using DocumentFormat.OpenXml.Bibliography;
using System.Globalization;
using static ClosedXML.Excel.XLPredefinedFormat;
using DateTime = System.DateTime;


namespace NewGaras.Domain.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskMangerProjectService _taskMangerProjectService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IShiftService _shiftService;
        private readonly IMailService _mailService;
        private readonly INotificationService _notificationService;
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
        public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper, ITaskMangerProjectService taskMangerProjectService, IWebHostEnvironment webHost, IShiftService shiftService, IMailService mailService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _taskMangerProjectService = taskMangerProjectService;
            _host = webHost;
            _shiftService = shiftService;
            _mailService = mailService;
            _notificationService = notificationService;
        }

        public async Task<BaseResponseWithDataAndHeader<List<UserAttendance>>> GetHrUserAttendanceList([FromHeader] GetUserAttendanceListHeader header)
        {
            BaseResponseWithDataAndHeader<List<UserAttendance>> Response = new BaseResponseWithDataAndHeader<List<UserAttendance>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            var DayName = System.DateTime.Now.ToString("dddd");
            var UserAttendanceList = new List<UserAttendance>();
            if (Response.Result)
            {
                var OffDaysList = await _unitOfWork.OffDays.FindAllAsync(x => x.Active == true);
                var ListDBQuery = _unitOfWork.HrUsers.FindAll(x => x.Active == true).AsQueryable();
                var AttendanceByDateList = _unitOfWork.Attendances.GetAsQueryable();
                if (header.EmployeeId != 0)
                {
                    ListDBQuery = ListDBQuery.Where(x => x.Id == header.EmployeeId).AsQueryable();
                    DateTime fromDate = DateTime.Now;
                    DateTime toDate = DateTime.Now;
                    if (header.fromDate != DateTime.MinValue && header.toDate != DateTime.MinValue)
                    {
                        AttendanceByDateList = _unitOfWork.Attendances.FindAll(a => a.AttendanceDate >= DateOnly.FromDateTime(fromDate) && a.AttendanceDate <= DateOnly.FromDateTime(toDate)).AsQueryable();
                    }
                    else
                    {
                        AttendanceByDateList = _unitOfWork.Attendances.FindAll(a => a.AttendanceDate == DateOnly.FromDateTime(header.AttendanceDate)).AsQueryable();
                    }

                }
                else
                {
                    AttendanceByDateList = _unitOfWork.Attendances.FindAll(a => a.AttendanceDate == DateOnly.FromDateTime(header.AttendanceDate)).AsQueryable();
                }
                OffDaysList = OffDaysList.Where(x => x.Day.Date == header.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower()).ToList();
                var ListIDS = ListDBQuery.ToList().Select(x => x.Id).AsQueryable();
                var UserTeamsList = _unitOfWork.UserTeams.FindAll(UT => ListIDS.Contains((long)UT.HrUserId)).AsQueryable();
                if (header.notAttended)
                {
                    var Attendedids = AttendanceByDateList.Select(x => x.HrUserId).AsQueryable();
                    ListDBQuery = ListDBQuery.Where(x => !Attendedids.Contains(x.Id)).AsQueryable();
                }

                var OffDay = await _unitOfWork.OffDays.FindAsync(x => (x.Day.Date == header.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower()));

                bool IsOffDay = false;
                string OffDayName = null;
                string HolidayName = null;
                if (OffDay != null)
                {
                    IsOffDay = true;
                    OffDayName = OffDay.Holiday.Name;
                }
                var UserListPagination = PagedList<NewGaras.Infrastructure.Entities.HrUser>.Create(ListDBQuery.OrderBy(x => x.FirstName), header.CurrentPage, header.NumberOfItemsPerPage);
                Response.PaginationHeader = new PaginationHeader
                {
                    CurrentPage = header.CurrentPage,
                    TotalPages = UserListPagination.TotalPages,
                    ItemsPerPage = header.NumberOfItemsPerPage,
                    TotalItems = UserListPagination.TotalCount
                };
                foreach (var item in UserListPagination)
                {
                    var AttendanceByUser = AttendanceByDateList.Where(x => x.HrUserId == item.Id).ToList();
                    var Obj = new List<UserAttendance>();
                    if (AttendanceByUser != null && AttendanceByUser.Count > 0)
                    {
                        Obj = AttendanceByUser.Select(a => new UserAttendance()
                        {
                            UserName = item.FirstName + " " + item.LastName,
                            UserID = item.Id,
                            DepartmentId = item.DepartmentId,
                            DepartmentName = item.Department?.Name,
                            TeamList = UserTeamsList.Where(x => x.UserId == item.Id).Select(UT => new TeamModel { TeamId = UT.TeamId, TeamName = UT.Team.Name }).ToList(),
                            AttendanceId = a.Id,
                            CheckInHour = a.CheckInHour,
                            CheckOutHour = a.CheckOutHour,
                            CheckInMin = a.CheckInMin,
                            CheckOutMin = a.CheckOutMin,
                            AbsenceTypeId = a.AbsenceTypeId,
                            AbsenceTypeName = a.AbsenceType?.HolidayName,
                            IsApprovedAbsence = a.IsApprovedAbsence,
                            IsOffDay = OffDaysList.Where(x => (DateOnly.FromDateTime(x.Day.Date) == a.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower())).FirstOrDefault() != null ? true : false,
                            OffDayName = OffDaysList.Where(x => (DateOnly.FromDateTime(x.Day.Date) == a.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower())).FirstOrDefault() != null ? OffDaysList.Where(x => (DateOnly.FromDateTime(x.Day.Date) == a.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower())).FirstOrDefault().Holiday.Name : null,
                        }).ToList();
                    }
                    else
                    {
                        Obj.Add(new UserAttendance()
                        {
                            UserName = item.FirstName + " " + item.LastName,
                            UserID = item.Id,
                            DepartmentId = item.DepartmentId,
                            DepartmentName = item.Department?.Name,
                            TeamList = UserTeamsList.Where(x => x.UserId == item.Id).Select(UT => new TeamModel { TeamId = UT.TeamId, TeamName = UT.Team.Name }).ToList(),
                            IsOffDay = OffDaysList.Where(x => (x.Day.Date == header.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower())).FirstOrDefault() != null ? true : false,
                            OffDayName = OffDaysList.Where(x => (x.Day.Date == header.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower())).FirstOrDefault() != null ? OffDaysList.Where(x => (x.Day.Date == header.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower())).FirstOrDefault().Holiday.Name : null,
                        });
                    }
                    UserAttendanceList.AddRange(Obj.AsEnumerable());
                }
            }
            Response.Data = UserAttendanceList.ToList();
            return Response;
        }

        public BaseResponseWithDataAndHeader<List<UserAttendance>> GetHrUserAttendence([FromHeader] GetEmployeeAttendenceHeader header)
        {
            BaseResponseWithDataAndHeader<List<UserAttendance>> Response = new BaseResponseWithDataAndHeader<List<UserAttendance>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.PaginationHeader = new PaginationHeader();
            Expression<Func<Attendance, bool>> Criteria = (x => true);
            if (header.EmployeeId != 0)
            {
                var employee = _unitOfWork.HrUsers.Find(x => x.Id == header.EmployeeId);
                if (employee != null)
                {
                    Criteria =
                    a =>
                    (
                    a.HrUserId == header.EmployeeId &&
                    a.AttendanceDate >= DateOnly.FromDateTime(header.fromDate) &&
                    a.AttendanceDate <= DateOnly.FromDateTime(header.toDate)
                    );
                    var UserAttendanceList = new List<UserAttendance>();
                    var AttendanceByDateList = _unitOfWork.Attendances.FindAllPaging(Criteria, header.CurrentPage, header.NumberOfItemsPerPage,
                        orderBy: a => a.CreationDate, orderByDirection: ApplicationConsts.OrderByAscending);
                    /*AttendanceByDateList = AttendanceByDateList.Where(a => a.AttendanceDate >= DateOnly.FromDateTime(header.fromDate) && a.AttendanceDate <= DateOnly.FromDateTime(header.toDate)).AsQueryable();
                    var list = PagedList<Attendance>.Create(AttendanceByDateList.OrderBy(a => a.CreationDate), header.CurrentPage, header.NumberOfItemsPerPage);*/
                    if (AttendanceByDateList.Count() > 0)
                    {
                        foreach (var item in AttendanceByDateList)
                        {
                            var dayname = item.AttendanceDate.ToString("dddd").ToLower();
                            var attendance = new UserAttendance()
                            {
                                UserName = employee.FirstName + " " + employee.LastName,
                                UserID = employee.Id,
                                DepartmentId = employee.DepartmentId,
                                DepartmentName = employee.Department?.Name,
                                TeamList = _unitOfWork.UserTeams.FindAll(x => x.HrUserId == item.Id).Select(UT => new TeamModel { TeamId = UT.TeamId, TeamName = UT.Team.Name }).ToList(),
                                AttendanceId = item.Id,
                                CheckInHour = item.CheckInHour,
                                CheckOutHour = item.CheckOutHour,
                                CheckInMin = item.CheckInMin,
                                CheckOutMin = item.CheckOutMin,
                                AbsenceTypeId = item.AbsenceTypeId,
                                AbsenceTypeName = item.AbsenceType?.HolidayName,
                                IsApprovedAbsence = item.IsApprovedAbsence,
                                IsOffDay = _unitOfWork.OffDays.FindAll(x => x.Active == true && (DateOnly.FromDateTime(x.Day) == item.AttendanceDate || x.WeekEndDay.Trim().ToLower() == dayname)).FirstOrDefault() != null ? true : false,
                                OffDayName = _unitOfWork.OffDays.FindAll(x => x.Active == true && (DateOnly.FromDateTime(x.Day) == item.AttendanceDate || x.WeekEndDay.Trim().ToLower() == dayname)).FirstOrDefault() != null ? _unitOfWork.OffDays.FindAll(x => x.Active == true && (DateOnly.FromDateTime(x.Day) == item.AttendanceDate || x.WeekEndDay.Trim().ToLower() == dayname)).FirstOrDefault().Holiday.Name : null,

                            };
                            UserAttendanceList.Add(attendance);
                        }
                        Response.PaginationHeader.TotalItems = AttendanceByDateList.TotalCount;
                        Response.PaginationHeader.CurrentPage = AttendanceByDateList.CurrentPage;
                        Response.PaginationHeader.ItemsPerPage = AttendanceByDateList.PageSize;
                        Response.PaginationHeader.TotalPages = AttendanceByDateList.TotalPages;
                        Response.Data = UserAttendanceList;

                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "this employee doesn't have attendence in this date";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "this employee doesn't exist";
                    Response.Errors.Add(error);
                    return Response;
                }
                return Response;
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "EmployeeId must be provided";
                Response.Errors.Add(error);
                return Response;
            }
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371; // Earth's radius in kilometers

            // Convert latitude and longitude from degrees to radians
            double lat1Rad = DegreesToRadians(lat1);
            double lon1Rad = DegreesToRadians(lon1);
            double lat2Rad = DegreesToRadians(lat2);
            double lon2Rad = DegreesToRadians(lon2);

            // Difference in coordinates
            double dLat = lat2Rad - lat1Rad;
            double dLon = lon2Rad - lon1Rad;

            // Haversine formula
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // Calculate the distance
            double distance = EarthRadiusKm * c;
            return distance;
        }

        public double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public BaseResponseWithId<long> AddWorkingHoursTracking([FromForm] AddTrackingByDailyAttendanceDto request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response = ValidateAttendanceRequest(request);
            if (Response.Result)
            {
                var lastCheckin = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.CheckInTime != null && a.HrUserId == request.UserId).OrderByDescending(a => a.Id).FirstOrDefault();
                if (lastCheckin != null && lastCheckin.CheckOutTime == null)
                {
                    Response.Result = true;
                    Response.ID = 0;
                    return Response;
                }
                var WorkingTracking = new WorkingHourseTracking();

                #region Check user,Contract,Salary,Jobtitle,Shift Are found
                var user = _unitOfWork.HrUsers.GetById(request.UserId);
                if (user == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "User Not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
                var branch = _unitOfWork.Branches.GetById(user.BranchId ?? 0);
                if (branch == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "There's no branch found for this user";
                    Response.Errors.Add(error);
                    return Response;
                }
                var contract = _unitOfWork.Contracts.FindAll(a => a.HrUserId == user.Id && a.IsCurrent).FirstOrDefault();
                if (contract == null)
                {
                    Response.Result = false;

                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "User Doesn't have contract";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (contract.AllowLocationTracking == true)
                {
                    if (request.Latitude != null && request.Longitude != null)
                    {
                        if (branch.Latitude == null && branch.Longitude == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err104";
                            error.ErrorMSG = "This Branch doesn't have location info";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (contract.Diameter == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err105";
                            error.ErrorMSG = "User Contract doesn't have a diameter";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        var distance = CalculateDistance((double)request.Latitude, (double)request.Longitude, (double)branch.Latitude, (double)branch.Longitude);
                        var DistannceInMiles = distance / 1.6;
                        if (DistannceInMiles > contract.Diameter)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err106";
                            error.ErrorMSG = $"You have to be at least {contract.Diameter} miles away from your branch to be able to check in";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                }
                var data = GetDayTypeIdbyKnowingTheDate(request.Date, user.BranchId ?? 0);
                var weekdayId = data[0];
                var daytypeId = data[1];
                var salary = _unitOfWork.Salaries.FindAll(a => a.HrUserId == user.Id).LastOrDefault();
                if (salary == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "User Doesn't have Salary";
                    Response.Errors.Add(error);
                    return Response;
                }

                var jobtitleRate = _unitOfWork.JobTitles.FindAll(a => a.Id == user.JobTitleId).Select(a => a.HourlyRate).FirstOrDefault();
                if (jobtitleRate == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "User Doesn't have Jobtitle";
                    Response.Errors.Add(error);
                    return Response;
                }


                long? shiftId = null;
                if (contract.WorkingHours == null)
                {

                    var shift = _unitOfWork.BranchSchedules.FindAll(a => request.CheckIn >= a.From && request.CheckIn <= a.To && a.WeekDayId == weekdayId).FirstOrDefault();
                    if (shift == null && daytypeId != 2 && request.CheckIn != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = "you don't have Working hour in your Contract and There's no shift found in the entered time";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    else
                    {
                        shiftId = shift.Id;
                    }
                }
                if (request.CheckIn == new TimeOnly(0, 0, 0))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "check in Time is required";
                    Response.Errors.Add(error);
                    return Response;
                }
                /*var pastOpenWorkingHour = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.CheckOutTime == null && a.Date <= request.Date && a.TaskId == null).OrderBy(a => a.Id).LastOrDefault();
                if (pastOpenWorkingHour != null)
                {
                    if (contract.WorkingHours != null)
                    {
                        pastOpenWorkingHour.CheckOutTime = pastOpenWorkingHour.CheckInTime?.AddHours((double)contract.WorkingHours);
                        pastOpenWorkingHour.TotalHours = (decimal)contract.WorkingHours;
                        _unitOfWork.WorkingHoursTrackings.Update(pastOpenWorkingHour);
                        _unitOfWork.Complete();
                    }
                    else
                    {
                        var pastShift = _unitOfWork.BranchSchedules.FindAll(a => pastOpenWorkingHour.CheckInTime >= a.From && pastOpenWorkingHour.CheckOutTime <= a.To && a.WeekDayId == weekdayId).FirstOrDefault();
                        if (pastShift == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err103";
                            error.ErrorMSG = "There's no shift found in the entered time of the open workinghours and no working hours in the contract";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        pastOpenWorkingHour.CheckOutTime = pastShift.To;
                        _unitOfWork.WorkingHoursTrackings.Update(pastOpenWorkingHour);
                        _unitOfWork.Complete();
                    }
                }*/
                #endregion


                WorkingTracking.ShiftId = shiftId;
                WorkingTracking.CheckInTime = request.CheckIn;
                //WorkingTracking.CheckOutTime = request.checkOut;
                //WorkingTracking.TotalHours = (decimal)((request.checkOut - request.CheckIn).TotalMinutes / 60);
                WorkingTracking.BranchId = user.BranchId;
                WorkingTracking.HrUserId = request.UserId;
                WorkingTracking.Date = request.Date;
                WorkingTracking.WorkingHourRate = (salary.TotalGross * 12) / (52 * 40);
                WorkingTracking.JobTitleRate = (decimal)jobtitleRate;
                WorkingTracking.TaskRate = 0;
                WorkingTracking.ProgressNote = request.ProgressNote;
                WorkingTracking.ProgressRate = request.progressPErcent;
                WorkingTracking.DayTypeId = daytypeId;
                if (request.Latitude != null && request.Longitude != null)
                {
                    WorkingTracking.Longitude = request.Longitude;
                    WorkingTracking.Latitude = request.Latitude;
                }

                if (WorkingTracking.DayTypeId != 2 && request.CheckIn != null)
                {
                    var check = checkOverTimeAndDeduction(user, contract, request.Date, /*shift,*/ (TimeOnly)request.CheckIn);
                    if (check.Error == true)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err104";
                        error.ErrorMSG = "This Branch doesn't have A branch Setting";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    //WorkingTracking.OvertimeRate = check.overtimerate;
                    WorkingTracking.OverTimeAllowed = check.overtimeAllowed;
                    //WorkingTracking.DeductionRate = check.deductionrate;
                    //WorkingTracking.DelayingAllowed = check.deductionAllowed;
                    //WorkingTracking.OverTimeHours = check.overtimehours;
                    //WorkingTracking.DelayingHours = check.delayinghours;
                }
                else
                {
                    if (request.CheckIn != null)
                    {
                        var check = checkVacationOverTimeAndDeduction(user, contract, request.Date, /*shift,*/ (TimeOnly)request.CheckIn);
                        if (check.Error == true)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err104";
                            error.ErrorMSG = "This Branch doesn't have A branch Setting";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        //WorkingTracking.OvertimeRate = check.overtimerate;
                        WorkingTracking.OverTimeAllowed = check.overtimeAllowed;
                        //WorkingTracking.DeductionRate = check.deductionrate;
                        //WorkingTracking.DelayingAllowed = check.deductionAllowed;
                        //WorkingTracking.OverTimeHours = check.overtimehours;
                        //WorkingTracking.DelayingHours = check.delayinghours;
                    }
                }
                /*WorkingTracking.OvertimeRate = check.overtimerate;
                WorkingTracking.OverTimeAllowed = check.overtimeAllowed;
                WorkingTracking.DeductionRate = check.deductionrate;
                WorkingTracking.DelayingAllowed = check.deductionAllowed;
                WorkingTracking.OverTimeHours = check.overtimehours;
                WorkingTracking.DelayingHours = check.delayinghours;*/
                WorkingTracking.CreationDate = DateTime.Now;
                WorkingTracking.CreatedBy = creator;
                var addedWorkingTracking = _unitOfWork.WorkingHoursTrackings.Add(WorkingTracking);

                _unitOfWork.Complete();
                Response.ID = addedWorkingTracking.Id;
                SumWorkingTrackingHoursForAttendance(addedWorkingTracking.HrUserId, addedWorkingTracking.Date, creator);
            }
            return Response;

        }
        public BaseResponseWithId<long> UpdateWorkingHoursTrackingWithCheckout([FromForm] UpdateWorkingHoursTrackingWithCheckoutDto request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var WorkingTracking = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.Id == request.WorkingTrackingId, includes: new[] { "HrUser.ContractDetails", "Shift" }).FirstOrDefault();

            if (WorkingTracking == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "this Workng Hour Tracking Record Is not Found";
                Response.Errors.Add(error);
                return Response;
            }

            var CheckIn = new DateTime(WorkingTracking.Date.Year, WorkingTracking.Date.Month, WorkingTracking.Date.Day, WorkingTracking.CheckInTime?.Hour ?? 0, WorkingTracking.CheckInTime?.Minute ?? 0, WorkingTracking.CheckInTime?.Second ?? 0);

            var CheckOut = request.Checkout;

            if (CheckOut <= CheckIn)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you can not enter a check out time that is less or equal your previous entered check In time";
                Response.Errors.Add(error);
                return Response;
            }
            if (WorkingTracking.CheckOutTime != null && request.ProgressModifedByAdmin == false)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "this record is already has a checkout time";
                Response.Errors.Add(error);
                return Response;
            }

            if (request.ProgressModifedByAdmin == true)
            {
                var creatorAcc = _unitOfWork.Users.GetById(creator);
                var creatorName = creatorAcc.FirstName + " " + creatorAcc.LastName;
                WorkingTracking.ProgressNote = $"Progress Modified By {creatorName} From {(WorkingTracking.CheckOutDate ?? WorkingTracking.Date).ToString("dd/MM/yyyy")} {WorkingTracking.CheckOutTime} To {request.Checkout.Date.ToString("dd/MM/yyyy")} {TimeOnly.FromDateTime(request.Checkout)}";
            }

            WorkingTracking.CheckOutTime = TimeOnly.FromDateTime(request.Checkout);
            WorkingTracking.TotalHours = (decimal)((CheckOut - CheckIn).TotalMinutes / 60);
            if (WorkingTracking.DayTypeId == 2)
            {
                WorkingTracking.OverTimeHours = WorkingTracking.TotalHours;
            }
            WorkingTracking.CheckOutDate = request.Checkout;
            /*if (WorkingTracking.DayTypeId != 2)
            {
                var check = checkOverTimeAndDeduction(WorkingTracking.HrUser, WorkingTracking.HrUser.ContractDetails.Where(a=>a.IsCurrent).FirstOrDefault(), WorkingTracking.Date, WorkingTracking.Shift, (TimeOnly)WorkingTracking.CheckInTime, request.Checkout);
                WorkingTracking.OvertimeRate = check.overtimerate;
                WorkingTracking.OverTimeAllowed = check.overtimeAllowed;
                WorkingTracking.DeductionRate = check.deductionrate;
                WorkingTracking.DelayingAllowed = check.deductionAllowed;
                WorkingTracking.OverTimeHours = check.overtimehours;
                WorkingTracking.DelayingHours = check.delayinghours;
            }
            else
            {
                var check = checkVacationOverTimeAndDeduction(WorkingTracking.HrUser, WorkingTracking.HrUser.ContractDetails.Where(a => a.IsCurrent).FirstOrDefault(), WorkingTracking.Date, WorkingTracking.Shift, (TimeOnly)WorkingTracking.CheckInTime, request.Checkout);
                WorkingTracking.OvertimeRate = check.overtimerate;
                WorkingTracking.OverTimeAllowed = check.overtimeAllowed;
                WorkingTracking.DeductionRate = check.deductionrate;
                WorkingTracking.DelayingAllowed = check.deductionAllowed;
                WorkingTracking.OverTimeHours = check.overtimehours;
                WorkingTracking.DelayingHours = check.delayinghours;
            }*/

            var updatedWorkingTracking = _unitOfWork.WorkingHoursTrackings.Update(WorkingTracking);
            _unitOfWork.Complete();
            SumWorkingTrackingHoursForAttendance(updatedWorkingTracking.HrUserId, updatedWorkingTracking.Date, creator);
            Response.ID = updatedWorkingTracking.Id;
            return Response;

        }
        public List<int> GetDayTypeIdbyKnowingTheDate(DateTime day, int branchId)
        {
            List<int> data = new List<int>();
            int dayTypeId = 0;
            var weekdayId = _unitOfWork.WeekDays.FindAll(a => a.Day.Trim().ToLower() == day.DayOfWeek.ToString
            ().Trim().ToLower() && a.BranchId == branchId).Select(a => a.Id).FirstOrDefault();
            if (_unitOfWork.WeekDays.FindAll(a => a.Id == weekdayId && a.BranchId == branchId).FirstOrDefault()?.IsWeekEnd == true)
            {
                var daytypeId = _unitOfWork.DayTypes.FindAll(a => a.DayType1.ToLower().Trim().Contains("weekend")).Select(a => a.Id).FirstOrDefault();
                if (daytypeId != 0)
                {
                    dayTypeId = daytypeId;

                }
            }
            else if (_unitOfWork.VacationDays.FindAll(a => DateOnly.FromDateTime(day) >= DateOnly.FromDateTime(a.From) && DateOnly.FromDateTime(day) <= DateOnly.FromDateTime(a.To) && a.BranchId == branchId).FirstOrDefault() != null)
            {
                var daytypeId = _unitOfWork.DayTypes.FindAll(a => a.DayType1.ToLower().Trim().Contains("holiday")).Select(a => a.Id).FirstOrDefault();
                if (daytypeId != 0)
                {
                    dayTypeId = daytypeId;

                }
            }
            else
            {
                var daytypeId = _unitOfWork.DayTypes.FindAll(a => a.DayType1.ToLower().Trim().Contains("workingday")).Select(a => a.Id).FirstOrDefault();
                if (daytypeId != 0)
                {
                    dayTypeId = daytypeId;
                }
            }
            data.Add(weekdayId);
            data.Add(dayTypeId);
            return data;
        }
        public List<int> CalculateDayTypesOfMonth(int month, int branchId)
        {
            List<int> data = new List<int>();
            var firstDay = new DateOnly(DateTime.Now.Year, month, 1);
            var LastDay = firstDay.AddMonths(1).AddDays(-1);
            var workingDays = 0;
            var weekEndDays = 0;
            var Holidays = 0;
            var daytype = 0;
            while (firstDay <= LastDay)
            {
                daytype = GetDayTypeIdbyKnowingTheDate(firstDay.ToDateTime(new TimeOnly(0, 0, 0)), branchId)[1];
                if (daytype == 3) workingDays += 1;
                else if (daytype == 2) Holidays += 1;
                else if (daytype == 1) weekEndDays += 1;

                firstDay = firstDay.AddDays(1);
            }

            data.Add(workingDays);
            data.Add(Holidays);
            data.Add(weekEndDays);
            return data;
        }

        public BaseResponseWithId<long> ValidateAttendanceRequest(AddTrackingByDailyAttendanceDto request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if (request == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invalid Data";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.Id != null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Invalid Data,You shouldn't send an ID";
                Response.Errors.Add(error);
                return Response;
            }

            if (request.CheckIn == TimeOnly.MinValue)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Check In Time Isn't provided";
                Response.Errors.Add(error);
                return Response;
            }
            /*if (request.checkOut < request.CheckIn)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Check Out Time should be after check In time";
                Response.Errors.Add(error);
                return Response;
            }*/
            if (request.Date == DateTime.MinValue)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Date is Required";
                Response.Errors.Add(error);
                return Response;
            }


            return Response;
        }

        public BaseResponseWithData<GetRequestedAbsenceBalanceModel> GetRequestedAbsenceBalance(long requestId)
        {
            BaseResponseWithData<GetRequestedAbsenceBalanceModel> Response = new BaseResponseWithData<GetRequestedAbsenceBalanceModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            var returnedData = new GetRequestedAbsenceBalanceModel();
            var leaveRequest = _unitOfWork.LeaveRequests.FindAll(a => a.Id == requestId, includes: new[] { "VacationType", "HrUser", "FirstApprovedByNavigation" }).FirstOrDefault();
            if (leaveRequest == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Leave Request not found";
                Response.Errors.Add(error);
                return Response;
            }
            var thisMonth = leaveRequest.From.Month;
            var UsedAbsenceInMonth = _unitOfWork.LeaveRequests.FindAll(a => (a.FirstApproval == true && a.SecondApprovedBy == null) || (a.SecondApproval == true && a.FirstApprovedBy == null) || (a.FirstApproval == true && a.SecondApproval == true) && a.VacationTypeId == leaveRequest.VacationTypeId).Count();
            if (UsedAbsenceInMonth >= leaveRequest.VacationType.BalancePerMonth)
            {
                returnedData.WarningMessage = $"This User Has Used {UsedAbsenceInMonth} absence days from his {leaveRequest.VacationType.BalancePerMonth} limit of absence days in one month";
            }
            var conLeave = _unitOfWork.ContractLeaveEmployees.FindAll(a => a.ContractLeaveSettingId == leaveRequest.VacationTypeId && a.HrUserId == leaveRequest.HrUserId).OrderBy(a => a.Id).LastOrDefault();
            returnedData.AbsenceTypeId = (int)leaveRequest.VacationTypeId;
            returnedData.RequestId = requestId;
            returnedData.AbsenceName = leaveRequest.VacationType.HolidayName;
            returnedData.HrUserId = (long)leaveRequest.HrUserId;
            returnedData.UserName = leaveRequest.HrUser.FirstName + " " + leaveRequest.HrUser.LastName;
            returnedData.UserImg = leaveRequest.HrUser.ImgPath != null ? Globals.baseURL + "/" + leaveRequest.HrUser.ImgPath : null ;
            returnedData.Balance = (int)conLeave.Balance;
            returnedData.BalancePerMonth = conLeave.BalancePerMonth != null ? (decimal)conLeave.BalancePerMonth : 0;
            returnedData.AbsenceCause = leaveRequest.AbsenceCause;
            returnedData.AbsenceRejectionCause = leaveRequest.FirstRejectionCause+"\n"+leaveRequest.SecondRejectionCause;
            returnedData.Remain = (int)conLeave.Remain;
            returnedData.Used = (int)conLeave.Used;
            returnedData.RequestDateFrom = leaveRequest.From.ToShortDateString();
            returnedData.RequestDateTo = leaveRequest.To.ToShortDateString();
            returnedData.Approval = leaveRequest.FirstApproval;
            returnedData.ApprovedBy = leaveRequest.FirstApprovedByNavigation?.FirstName + " " + (leaveRequest.FirstApprovedByNavigation?.MiddleName != null ? leaveRequest.FirstApprovedByNavigation?.MiddleName + " " : null) + leaveRequest.FirstApprovedByNavigation?.LastName;
            Response.Data = returnedData;
            return Response;
        }
        public CheckOverTimeAndDeduction checkOverTimeAndDeduction(HrUser user, ContractDetail contract, DateTime date, /*BranchSchedule shift,*/ TimeOnly checkin)
        {
            CheckOverTimeAndDeduction data = new CheckOverTimeAndDeduction();
            var branch = _unitOfWork.BranchSetting.FindAll(a => a.BranchId == user.BranchId).FirstOrDefault();
            if (branch == null)
            {
                data.Error = true;
                return data;
            }
            if (branch.AllowAutomaticOvertime && (bool)contract.AllowOvertime /*&& shift != null*/)
            {
                data.overtimeAllowed = true;
                /*if (checkout > shift.To)
                {
                    decimal overtimehours = (decimal)((checkout - (TimeOnly)shift.To).TotalMinutes / 60);
                    data.overtimehours = overtimehours;
                }
                var overtimes = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => a.Rate > 0 && a.From >= shift.To && checkout >= a.To).ToList();
                if (overtimes.Count() == 0)
                {
                    overtimes = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => a.Rate > 0 && shift.To >= a.From && shift.To < a.To).ToList();
                }
                decimal OverTimeRate = 0;
                if (overtimes.Count > 0)
                {
                    OverTimeRate = overtimes.Select(a => a.Rate).Average();

                }
                if (OverTimeRate != 0)
                {
                    data.overtimerate = OverTimeRate;
                }
                else
                {
                    data.overtimerate = 0;
                }*/

            }
            else
            {
                data.overtimeAllowed = false;
                data.overtimerate = 0;
            }
            if (branch.AllowDelayingDeduction && (bool)contract.AllowBreakDeduction /*&& shift != null*/)
            {
                data.deductionAllowed = true;
                /*decimal deductionRate = 0;
                var deductions = new List<OverTimeAndDeductionRate>().AsEnumerable();
                if (checkin > shift.From)
                {
                    decimal delayingHours = (decimal)((checkin - (TimeOnly)shift.From).TotalMinutes / 60);
                    data.delayinghours = delayingHours;
                    deductions = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => (shift.From >= a.From && checkin <= a.To) && a.Rate < 0);
                    if (deductions.Count() == 0)
                    {
                        deductions = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => (shift.From >= a.From && shift.From < a.To) && a.Rate < 0);
                    }

                }
                else if (checkout < shift.To)
                {
                    decimal delayingHours = (decimal)(((TimeOnly)shift.To - checkout).TotalMinutes / 60);
                    data.delayinghours = delayingHours;
                    deductions = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => (checkout >= a.From && shift.To <= a.To) && a.Rate < 0);
                    if (deductions.Count() == 0)
                    {
                        deductions = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => (checkout >= a.From && shift.To < a.To) && a.Rate < 0);
                    }
                }
                if (deductions.Count() > 0)
                {
                    deductionRate = deductions.Select(a => a.Rate).Average();
                }
                if (deductionRate != 0)
                {
                    data.deductionrate = deductionRate;
                }
                else
                {
                    data.deductionrate = 0;
                }*/
            }
            else
            {
                data.deductionAllowed = false;
                data.deductionrate = 0;
            }
            return data;
        }
        public CheckOverTimeAndDeduction checkVacationOverTimeAndDeduction(HrUser user, ContractDetail contract, DateTime date, /*BranchSchedule shift,*/ TimeOnly checkin)
        {
            CheckOverTimeAndDeduction data = new CheckOverTimeAndDeduction();
            var branch = _unitOfWork.BranchSetting.FindAll(a => a.BranchId == user.BranchId).FirstOrDefault();
            if (branch == null)
            {
                data.Error = true;
                return data;
            }
            var vacationday = _unitOfWork.VacationDays.FindAll(a => date >= a.From && date <= a.To && a.BranchId == branch.Id).FirstOrDefault();
            if (branch != null && branch.AllowAutomaticOvertime && (bool)contract.AllowOvertime /*&& shift != null*/)
            {
                data.overtimeAllowed = true;
                /*if (checkout > shift.To)
                {
                    decimal overtimehours = (decimal)((checkout - (TimeOnly)shift.To).TotalMinutes / 60);
                    data.overtimehours = overtimehours;
                }
                var overtimes = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.Rate > 0 && a.From >= shift.To && checkout >= a.To && a.VacationDayId == vacationday.Id).ToList();
                if (overtimes.Count() == 0)
                {
                    overtimes = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.Rate > 0 && shift.To >= a.From && shift.To < a.To && a.VacationDayId == vacationday.Id).ToList();
                }
                decimal OverTimeRate = 0;
                if (overtimes.Count > 0)
                {
                    OverTimeRate = overtimes.Select(a => a.Rate).Average();

                }
                if (OverTimeRate != 0)
                {
                    data.overtimerate = OverTimeRate;
                }
                else
                {
                    data.overtimerate = 0;
                }*/

            }
            else
            {
                data.overtimeAllowed = false;
                data.overtimerate = 0;
            }
            if (branch.AllowDelayingDeduction && (bool)contract.AllowBreakDeduction /*&& shift != null*/)
            {
                data.deductionAllowed = true;
                /*decimal deductionRate = 0;
                var deductions = new List<VacationOverTimeAndDeductionRate>().AsEnumerable();
                if (checkin > shift.From)
                {
                    decimal delayingHours = (decimal)((checkin - (TimeOnly)shift.From).TotalMinutes / 60);
                    data.delayinghours = delayingHours;
                    deductions = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => (shift.From >= a.From && checkin <= a.To) && a.Rate < 0 && a.VacationDayId == vacationday.Id);
                    if (deductions.Count() == 0)
                    {
                        deductions = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => (shift.From >= a.From && shift.From < a.To) && a.Rate < 0 && a.VacationDayId == vacationday.Id);
                    }

                }
                else if (checkout < shift.To)
                {
                    decimal delayingHours = (decimal)(((TimeOnly)shift.To - checkout).TotalMinutes / 60);
                    data.delayinghours = delayingHours;
                    deductions = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => (checkout >= a.From && shift.To <= a.To) && a.Rate < 0 && a.VacationDayId == vacationday.Id);
                    if (deductions.Count() == 0)
                    {
                        deductions = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => (checkout >= a.From && shift.To < a.To) && a.Rate < 0 && a.VacationDayId == vacationday.Id);
                    }
                }
                if (deductions.Count() > 0)
                {
                    deductionRate = deductions.Select(a => a.Rate).Average();
                }
                if (deductionRate != 0)
                {
                    data.deductionrate = deductionRate;
                }
                else
                {
                    data.deductionrate = 0;
                }*/
            }
            else
            {
                data.deductionAllowed = false;
                data.deductionrate = 0;
            }
            return data;
        }

        public BaseResponseWithId<long> AddWorkingHoursTrackingByTask(AddTrackingByDailyTaskDto request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            #region Data Validation
            if (request == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invalid Data";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.Id != null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Invalid Data,You shouldn't send an ID";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.checkOut < request.CheckIn)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Check Out Time should be after check In time";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.Date == DateTime.MinValue)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Date is Required";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.TaskId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Task Id is Required";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.CheckIn != null && request.checkOut != null && request.checkOut < request.CheckIn)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "check out can't be before checkin";
                Response.Errors.Add(error);
                return Response;
            }
            /*if (string.IsNullOrEmpty(request.ProgressNote))
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Progress comment Is Required";
                Response.Errors.Add(error);
                return Response;
            }*/
            if (request.progressPErcent != null && request.progressPErcent < 0 || request.progressPErcent > 100)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Progress rate should be between 0 and 100";
                Response.Errors.Add(error);
                return Response;
            }
            #endregion
            if (Response.Result)
            {
                var WorkingTracking = new WorkingHourseTracking();

                #region variables of mails
                //long projectId = 0;
                string projectName = string.Empty;
                List<SendEmailTo> mangersIdList = new List<SendEmailTo>();
                #endregion

                #region Check user,Contract,Salary,Jobtitle,Task Are found
                var user = _unitOfWork.HrUsers.GetById(request.UserId);
                if (user == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "User Not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
                var data = GetDayTypeIdbyKnowingTheDate(request.Date, user.BranchId ?? 0);
                var weekdayId = data[0];
                var daytypeId = data[1];
                var salary = _unitOfWork.Salaries.FindAll(a => a.HrUserId == user.Id).LastOrDefault();
                if (salary == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "User Doesn't have Salary";
                    Response.Errors.Add(error);
                    return Response;
                }

                var jobtitleRate = _unitOfWork.JobTitles.FindAll(a => a.Id == user.JobTitleId).Select(a => a.HourlyRate).FirstOrDefault();
                if (jobtitleRate == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "User Doesn't have Jobtitle";
                    Response.Errors.Add(error);
                    return Response;
                }
                var contract = _unitOfWork.Contracts.FindAll(a => a.HrUserId == user.Id && a.IsCurrent).FirstOrDefault();
                if (contract == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "User Doesn't have contract";
                    Response.Errors.Add(error);
                    return Response;
                }
                var task = _unitOfWork.Tasks.GetById(request.TaskId);
                if (task == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Task Not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
                #endregion
                if (request.progressPErcent != null && request.progressPErcent > 0)
                {
                    var tasks = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.TaskId == request.TaskId && a.ProgressRate != null && a.ProgressRate > 0, take: null, skip: null, orderBy: a => a.CreationDate, orderByDirection: ApplicationConsts.OrderByDescending).ToList();
                    var lastProgress = tasks.FirstOrDefault();
                    if (lastProgress != null && lastProgress.ProgressRate > request.progressPErcent)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err105";
                        error.ErrorMSG = "Progress Rate Should be more than or equale the last percentage";
                        Response.Errors.Add(error);
                        return Response;
                    }

                }
                var taskss = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.TaskId == request.TaskId && a.CheckOutTime == null, take: null, skip: null, orderBy: a => a.CreationDate, orderByDirection: ApplicationConsts.OrderByDescending).ToList();
                var lastProgresss = taskss.FirstOrDefault();
                if (lastProgresss != null)
                {

                    if (lastProgresss.CheckInTime != null && lastProgresss.CheckOutTime == null && lastProgresss.TaskValidateDate != null)
                    {
                        lastProgresss.CheckOutTime = TimeOnly.FromDateTime((DateTime)lastProgresss.TaskValidateDate);
                        _unitOfWork.WorkingHoursTrackings.Update(lastProgresss);
                        _unitOfWork.Complete();
                    }
                }

                //WorkingTracking.ShiftId = shift.Id;
                WorkingTracking.CheckInTime = request.CheckIn;
                WorkingTracking.CheckOutTime = request.checkOut;
                WorkingTracking.BranchId = task.BranchId;
                WorkingTracking.HrUserId = request.UserId;
                WorkingTracking.Date = request.Date;
                WorkingTracking.WorkingHourRate = (salary.TotalGross * 12) / (52 * 40);
                WorkingTracking.JobTitleRate = (decimal)jobtitleRate;
                WorkingTracking.TaskRate = 0;
                WorkingTracking.TaskId = request.TaskId;
                WorkingTracking.ProgressNote = request.ProgressNote;
                WorkingTracking.ProgressRate = request.progressPErcent;
                WorkingTracking.DayTypeId = daytypeId;
                if (request.CheckIn != null)
                {
                    WorkingTracking.CheckInTime = request.CheckIn;
                    WorkingTracking.TaskValidateDate = new DateTime(WorkingTracking.Date.Year, WorkingTracking.Date.Month, WorkingTracking.Date.Day, WorkingTracking.CheckInTime.Value.Hour, WorkingTracking.CheckInTime.Value.Minute, WorkingTracking.CheckInTime.Value.Second);
                }
                if (request.checkOut != null)
                {
                    WorkingTracking.CheckOutTime = request.checkOut;
                }
                if (request.TotalHours > 0)
                {
                    WorkingTracking.TotalHours = request.TotalHours;
                }
                else if (request.CheckIn != null && request.checkOut != null)
                {
                    WorkingTracking.TotalHours = (decimal)(((TimeOnly)request.checkOut - (TimeOnly)request.CheckIn).TotalMinutes / 60);
                }
                if (task.ProjectId != null)
                {
                    WorkingTracking.ProjectId = task.ProjectId;

                    var project = _unitOfWork.Projects.FindAll(a => a.Id == (long)task.ProjectId, new[] { "SalesOffer" }).FirstOrDefault();
                    projectName = project?.SalesOffer?.ProjectName;
                    if (project == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err106";
                        error.ErrorMSG = "Project not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (project.Billable == true)
                    {
                        if (project.BillingTypeId == 1)
                        {
                            WorkingTracking.TaskRate = jobtitleRate ?? 0;
                        }
                        else if (project.BillingTypeId == 2)
                        {
                            if (salary.PaymentStrategyId == 2)
                            {
                                WorkingTracking.TaskRate = (project.BillingFactor * salary.TotalGross) ?? 0;
                            }
                            else
                            {
                                var rate = (salary.TotalGross * 12) / (52 * 40);
                                WorkingTracking.TaskRate = (project.BillingFactor * rate) ?? 0;

                            }
                        }
                    }
                    else
                    {
                        WorkingTracking.TaskRate = 1;
                    }
                }

                /*if (WorkingTracking.DayTypeId != 2)
                {
                    var check = checkOverTimeAndDeduction(user, contract, request.Date, shift, request.CheckIn, request.checkOut);
                    WorkingTracking.OvertimeRate = check.overtimerate;
                    WorkingTracking.OverTimeAllowed = check.overtimeAllowed;
                    WorkingTracking.DeductionRate = check.deductionrate;
                    WorkingTracking.DelayingAllowed = check.deductionAllowed;
                    WorkingTracking.OverTimeHours = check.overtimehours;
                    WorkingTracking.DelayingHours = check.delayinghours;
                }
                else
                {
                    var check = checkVacationOverTimeAndDeduction(user, contract, request.Date, shift, request.CheckIn, request.checkOut);
                    WorkingTracking.OvertimeRate = check.overtimerate;
                    WorkingTracking.OverTimeAllowed = check.overtimeAllowed;
                    WorkingTracking.DeductionRate = check.deductionrate;
                    WorkingTracking.DelayingAllowed = check.deductionAllowed;
                    WorkingTracking.OverTimeHours = check.overtimehours;
                    WorkingTracking.DelayingHours = check.delayinghours;
                }*/
                /*WorkingTracking.OvertimeRate = check.overtimerate;
                WorkingTracking.OverTimeAllowed = check.overtimeAllowed;
                WorkingTracking.DeductionRate = check.deductionrate;
                WorkingTracking.DelayingAllowed = check.deductionAllowed;
                WorkingTracking.OverTimeHours = check.overtimehours;
                WorkingTracking.DelayingHours = check.delayinghours;*/
                WorkingTracking.CreationDate = DateTime.Now;
                WorkingTracking.CreatedBy = creator;
                var addedWorkingTracking = _unitOfWork.WorkingHoursTrackings.Add(WorkingTracking);

                var Res = _unitOfWork.Complete();
                if (Res > 0) // Update Task Requirments Status
                {

                    if (request.TaskRequirmentsIds != null && request.TaskRequirmentsIds.Count() > 0)
                    {
                        var TaskRequirementsList = _unitOfWork.TaskRequirements.FindAll((x => x.TaskId == request.TaskId && request.TaskRequirmentsIds.Contains(x.Id)));
                        if (TaskRequirementsList.Count() > 0) // Update Status with finished
                        {
                            foreach (var TaskReq in TaskRequirementsList)
                            {
                                TaskReq.IsFinished = true;
                                TaskReq.WorkingHourTrackingId = addedWorkingTracking.Id;
                                _unitOfWork.TaskRequirements.Update(TaskReq);
                            }
                            _unitOfWork.Complete();
                        }
                    }
                }
                Response.ID = WorkingTracking.Id;
                //---------------------calculate the total Expensis of the project----------------------------------------
                /*var project = _unitOfWork.Projects.FindAll((a => a.Id == task.ProjectId)).FirstOrDefault();
                if(project.Tasks != null)
                {
                    var Expensis = project.Tasks.SelectMany(y => y.TaskExpensis).Sum(s => s.Amount);
                    var Costs = _taskMangerProjectService.GetCostsForAllTask(project.Id);
                    var totalExpensis = Expensis + Costs.Data;
                }*/
                //--------------------------------------------------------------------------------------------------------


                //-------------------------sending Mails--------------------------
                var taskReq = _unitOfWork.TaskRequirements.Find(a => a.WorkingHourTrackingId == WorkingTracking.Id);
                var ids = new List<long>();
                decimal taskReqPer = 0;
                string TaskReqName = string.Empty;
                if (taskReq != null)
                {
                    taskReqPer = taskReq.Percentage;
                    TaskReqName = taskReq.Name;
                }
                if (task.ProjectId != null)
                {

                    ids = _unitOfWork.ProjectAssignUsers.FindAll(a => a.ProjectId == task.ProjectId && a.RoleId == 146).Select(a => a.UserId).ToList();
                    if (ids != null && ids.Count() > 0)
                    {
                        var users = _unitOfWork.Users.FindAll(a => ids.Contains(a.Id)).Select(b => new { b.Id, b.Email });
                        foreach (var usr in users)
                        {
                            //var user = _Context.Users.Find(a.UserId);
                            if (user != null)
                            {
                                _mailService.SendMail(new MailData()
                                {
                                    EmailToName = usr.Email,
                                    EmailToId = usr.Email,
                                    EmailSubject = "Task Progress Need Approval",
                                    EmailBody = $"I hope this message finds you well.\r\n\r\nI am writing to request your approval for the progress made on the task/project titled Task Name : {task.Name}/{projectName}. We have reached a significant milestone, and I would like to ensure that the current deliverables meet our expectations and requirements before proceeding further.\r\n\r\nTask/Project Details:\r\n\r\n \t•Task/Project Name: {task.Name}/{projectName}\r\n \t• Description: {task.Description}\r\n \t• Current Status: {taskReqPer}\r\n \t• Milestone Reached: {TaskReqName}\r\n"
                                });

                                _notificationService.CreateNotification(creator, "Task Progress", "Need Approval", Response.ID.ToString(), true, usr.Id, 0);

                            }
                        }
                    }
                    else
                    {
                        //send to creator
                        var crator = _unitOfWork.Users.Find(a => a.Id == task.CreatedBy);
                        if (crator != null)
                        {

                            _mailService.SendMail(new MailData()
                            {
                                EmailToName = crator.Email,
                                EmailToId = crator.Email,
                                EmailSubject = "Task Progress Need Approval",
                                EmailBody = $"I hope this message finds you well.\r\n\r\nI am writing to request your approval for the progress made on the task/project titled Task Name : {task.Name}/{projectName}. We have reached a significant milestone, and I would like to ensure that the current deliverables meet our expectations and requirements before proceeding further.\r\n\r\nTask/Project Details:\r\n\r\n \t•Task/Project Name: {task.Name}/{projectName}\r\n \t• Description: {task.Description}\r\n \t• Current Status: {taskReqPer}\r\n \t• Milestone Reached: {TaskReqName}\r\n"
                            });

                            //_notificationService.CreateNotification(creator, "Task Progress Need Approval", task.Description, Response.ID.ToString(), true, usr.Id, 0);

                        }
                    }
                }
                else
                {
                    //send to creator
                    var crator = _unitOfWork.Users.Find(a => a.Id == task.CreatedBy);
                    if (crator != null)
                    {

                        _mailService.SendMail(new MailData()
                        {
                            EmailToName = crator.Email,
                            EmailToId = crator.Email,
                            EmailSubject = "Task Progress Need Approval",
                            EmailBody = $"I hope this message finds you well.\r\n\r\nI am writing to request your approval for the progress made on the task/project titled Task Name : {task.Name}/{projectName}. We have reached a significant milestone, and I would like to ensure that the current deliverables meet our expectations and requirements before proceeding further.\r\n\r\nTask/Project Details:\r\n\r\n \t•Task/Project Name: {task.Name}/{projectName}\r\n \t• Description: {task.Description}\r\n \t• Current Status: {taskReqPer}\r\n \t• Milestone Reached: {TaskReqName}\r\n"
                        });


                    }
                }
                //-------------------------------send Email to users from TaskPermission---------------------------------
                var TaskPermissionUsersIDs = _unitOfWork.TaskPermissions.FindAll(a => a.TaskId == request.TaskId && a.IsGroup == false).Select(b => b.UserGroupId).ToList();

                var usersTaskPermissionList = _unitOfWork.Users.FindAll(a => TaskPermissionUsersIDs.Contains(a.Id) && !ids.Contains(a.Id)).ToList();

                foreach (var usr in usersTaskPermissionList)
                {
                    //var user = _Context.Users.Find(a.UserId);
                    if (user != null)
                    {
                        _mailService.SendMail(new MailData()
                        {
                            EmailToName = usr.Email,
                            EmailToId = usr.Email,
                            EmailSubject = "Task Progress Need Approval",
                            EmailBody = $"I hope this message finds you well.\r\n\r\nI am writing to request your approval for the progress made on the task/project titled Task Name : {task.Name}/{projectName}. We have reached a significant milestone, and I would like to ensure that the current deliverables meet our expectations and requirements before proceeding further.\r\n\r\nTask/Project Details:\r\n\r\n \t•Task/Project Name: {task.Name}/{projectName}\r\n \t• Description: {task.Description}\r\n \t• Current Status: {taskReqPer}\r\n \t• Milestone Reached: {TaskReqName}\r\n"
                        });

                        _notificationService.CreateNotification(creator, "Task Progress", "Need Approval", Response.ID.ToString(), true, usr.Id, 0);

                    }
                }

                Response.ID = addedWorkingTracking.Id;

            }
            return Response;

        }

        public BaseResponseWithData<List<GetTaskProgressForUser>> GetTaskProgressForUserList([FromHeader] long TaskId)
        {

            BaseResponseWithData<List<GetTaskProgressForUser>> Response = new BaseResponseWithData<List<GetTaskProgressForUser>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            #region Data Validation
            if (TaskId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "TaskId is required";
                Response.Errors.Add(error);
                return Response;
            }


            var CheckTask = _unitOfWork.Tasks.Find((x => x.Id == TaskId));
            if (CheckTask == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "This Task is not exist";
                Response.Errors.Add(error);
                return Response;
            }
            #endregion
            if (Response.Result)
            {
                var taskReqList = _unitOfWork.TaskRequirements.FindAll(a => a.TaskId == TaskId);


                var TaskProgressForUserResonse = new List<GetTaskProgressForUser>();
                var ListTaskWorkingTrakingList = _unitOfWork.WorkingHoursTrackings.FindAll((x => x.TaskId == TaskId), new[] { "HrUser", "ApprovedByNavigation", "CreatedByNavigation" });
                var firstNotApprovedProgressId = ListTaskWorkingTrakingList.Where(a => a.WorkingHoursApproval == null).FirstOrDefault();
                TaskProgressForUserResonse = ListTaskWorkingTrakingList.Select(item => new GetTaskProgressForUser
                {
                    Id = item.Id,
                    HrUserId = item.HrUserId,
                    UserName = item.HrUser?.FirstName + " " + item.HrUser?.LastName,
                    UserImage = item.HrUser?.ImgPath != null ? Globals.baseURL + item.HrUser?.ImgPath : null,
                    progressrate = item.ProgressRate,
                    progressnote = item.ProgressNote,
                    ApprovedById = item.ApprovedBy,
                    TotalHours = item.TotalHours,
                    Date = item.Date.ToString(),
                    ApprovedByName = item.ApprovedBy != null ? item.ApprovedByNavigation?.FirstName + " " + item.ApprovedByNavigation?.LastName : null,
                    ApprovedByImage = item?.ApprovedByNavigation?.PhotoUrl != null ? Globals.baseURL + item?.ApprovedByNavigation?.PhotoUrl : null,
                    WorkingHoursApproval = item.WorkingHoursApproval,
                    FirstNotApprovedProgress = item.Id == (firstNotApprovedProgressId?.Id ?? 0),
                    CreatedBy = item.CreatedBy,
                    CreatorName = item.CreatedByNavigation != null ? item.CreatedByNavigation.FirstName + item.CreatedByNavigation.MiddleName + item.CreatedByNavigation.LastName : null,
                    UserImgPath = item.CreatedByNavigation.PhotoUrl != null ? Globals.baseURL + item.CreatedByNavigation.PhotoUrl : null,
                    TaskRequirementList = taskReqList.Where(a => a.WorkingHourTrackingId == item.Id).Select(req => new GetTaskRequirementDto
                    {
                        ID = req.Id,
                        IsFinished = req.IsFinished,
                        Name = req.Name,
                        Percentage = req.Percentage
                    }).ToList(),

                }).OrderByDescending(x => x.Id).ToList();

                Response.Data = TaskProgressForUserResonse;
            }
            return Response;
        }

        public BaseResponseWithData<GetTaskProgressForUser> GetTaskProgress([FromHeader] long progressId)
        {

            BaseResponseWithData<GetTaskProgressForUser> Response = new BaseResponseWithData<GetTaskProgressForUser>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            #region Data Validation
            if (progressId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Progress Id is required";
                Response.Errors.Add(error);
                return Response;
            }


            /*var CheckProgress =  _unitOfWork.WorkingHoursTrackings.Find((x => x.Id == progressId));
            if (CheckProgress == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "This Progress is not exist";
                Response.Errors.Add(error);
                return Response;
            }*/
            #endregion
            if (Response.Result)
            {
                var progress = _unitOfWork.WorkingHoursTrackings.FindAll((x => x.Id == progressId), new[] { "HrUser", "ApprovedByNavigation" }).FirstOrDefault();
                if (progress == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "This Progress is not exist";
                    Response.Errors.Add(error);
                    return Response;
                }
                var taskReqList = _unitOfWork.TaskRequirements.FindAll(a => a.WorkingHourTrackingId == progressId);
                var dtoTaskReqList = new List<GetTaskRequirementDto>();

                foreach (var taskReq in taskReqList)
                {
                    var req = new GetTaskRequirementDto();
                    req.ID = taskReq.Id;
                    req.Name = taskReq.Name;
                    req.IsFinished = taskReq.IsFinished;
                    req.Percentage = taskReq.Percentage;
                    dtoTaskReqList.Add(req);
                }

                var TaskProgressForUserResonse = new GetTaskProgressForUser()
                {
                    Id = progress.Id,
                    HrUserId = progress.HrUserId,
                    UserName = progress.HrUser?.FirstName + " " + progress.HrUser?.LastName,
                    UserImage = progress.HrUser?.ImgPath != null ? Globals.baseURL + progress.HrUser?.ImgPath : null,
                    progressrate = progress.ProgressRate,
                    progressnote = progress.ProgressNote,
                    ApprovedById = progress.ApprovedBy,
                    TotalHours = progress.TotalHours,
                    Date = progress.Date.ToShortDateString(),
                    ApprovedByName = progress.ApprovedBy != null ? progress.ApprovedByNavigation?.FirstName + " " + progress.ApprovedByNavigation?.LastName : null,
                    ApprovedByImage = progress?.ApprovedByNavigation?.PhotoUrl != null ? Globals.baseURL + progress?.ApprovedByNavigation?.PhotoUrl : null,
                    WorkingHoursApproval = progress.WorkingHoursApproval,
                    TaskRequirementList = dtoTaskReqList
                };

                Response.Data = TaskProgressForUserResonse;
            }
            return Response;
        }

        public BaseResponseWithId<long> ApproveTaskPrpgress([FromBody] TaskProgressApprovalDto request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            if (request.ProgressId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Progress Id is Requiored";
                Response.Errors.Add(error);
                return Response;
            }
            var checkProgress = _unitOfWork.WorkingHoursTrackings.GetById(request.ProgressId);
            if (checkProgress == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Progress is not Found";
                Response.Errors.Add(error);
                return Response;
            }
            if (checkProgress.WorkingHoursApproval != null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err105";
                if (checkProgress.WorkingHoursApproval == true) error.ErrorMSG = "This Task Progress is Already Approved";
                if (checkProgress.WorkingHoursApproval == false) error.ErrorMSG = "This Task Progress is Already Rejected";
                Response.Errors.Add(error);
                return Response;
            }
            checkProgress.WorkingHoursApproval = request.Approval;
            checkProgress.ApprovedBy = creator;

            if (request.Approval == false)
            {
                checkProgress.ProgressRate = 0;

                var taskReqList = _unitOfWork.TaskRequirements.FindAll((a => a.TaskId == checkProgress.TaskId));
                foreach (var taskReq in taskReqList)
                {
                    taskReq.WorkingHourTrackingId = null;
                    taskReq.IsFinished = false;
                }
            }
            var updatedprogress = _unitOfWork.WorkingHoursTrackings.Update(checkProgress);
            _unitOfWork.Complete();
            Response.ID = updatedprogress.Id;
            return Response;
        }

        /*public BaseResponseWithId<long> AddWorkingHoursTrackingByEnterLocation([FromForm] AddTrackingByLocationDto request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            #region Data Validation
            if (request == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invalid Data";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.Id != null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Invalid Data,You shouldn't send an ID";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.Latitude == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Latitude Is required";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.Longitude == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Longitude Is required";
                Response.Errors.Add(error);
                return Response;
            }

            if (request.CheckIn == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Check In Time Isn't provided";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.Date == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Date is Required";
                Response.Errors.Add(error);
                return Response;
            }
            #endregion
            if (Response.Result)
            {
                var WorkingTracking = new WorkingHourseTracking();

                var data = GetDayTypeIdbyKnowingTheDate(request.Date);
                var weekdayId = data[0];
                var daytypeId = data[1];

                #region Check user,Contract,Salary,Jobtitle Are found
                var user = _unitOfWork.HrUsers.GetById(request.UserId);
                if (user == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "User Not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
                var salary = _unitOfWork.Salaries.FindAll(a => a.HrUserId == user.Id).LastOrDefault();
                if (salary == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "User Doesn't have Salary";
                    Response.Errors.Add(error);
                    return Response;
                }

                var jobtitleRate = _unitOfWork.JobTitles.FindAll(a => a.Id == user.JobTitleId).Select(a => a.HourlyRate).FirstOrDefault();
                if (jobtitleRate == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "User Doesn't have Jobtitle";
                    Response.Errors.Add(error);
                    return Response;
                }
                var contract = _unitOfWork.Contracts.FindAll(a => a.HrUserId == user.Id && a.IsCurrent).FirstOrDefault();
                if (contract == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "User Doesn't have contract";
                    Response.Errors.Add(error);
                    return Response;
                }
                #endregion

                WorkingTracking.CheckInTime = request.CheckIn;
                WorkingTracking.CheckOutTime = null;
                WorkingTracking.BranchId = user.BranchId;
                WorkingTracking.HrUserId = request.UserId;
                WorkingTracking.Date = request.Date;
                WorkingTracking.WorkingHourRate = (salary.TotalGross * 12) / (52 * 40);
                WorkingTracking.JobTitleRate = (decimal)jobtitleRate;
                WorkingTracking.TaskRate = 0;
                WorkingTracking.DayTypeId = daytypeId;
                WorkingTracking.CreationDate = DateTime.Now;
                WorkingTracking.Latitude = request.Latitude;
                WorkingTracking.Longitude = request.Longitude;
                WorkingTracking.CreatedBy = creator;
                var addedWorkingTracking = _unitOfWork.WorkingHoursTrackings.Add(WorkingTracking);

                _unitOfWork.Complete();
                Response.ID = addedWorkingTracking.Id;
            }
            return Response;
        }*/


        public BaseResponseWithId<long> SumWorkingTrackingHoursForAttendance(long UserId, DateTime date, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var WorkingTrackings = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.HrUserId == UserId && DateOnly.FromDateTime(a.Date) == DateOnly.FromDateTime(date) && a.TaskId == null, includes: new[] { "Shift" }).OrderBy(a => a.Date).ToList();
            if (WorkingTrackings != null && WorkingTrackings.Count() > 0)
            {
                var check = true;
                var sumAttendance = _unitOfWork.Attendances.FindAll(a => a.HrUserId == UserId && a.AttendanceDate == DateOnly.FromDateTime(date), includes: new[] { "HrUser" }).FirstOrDefault();
                var user = _unitOfWork.HrUsers.FindAll(x => x.Id == UserId, includes: new[] { "ContractDetails" }).FirstOrDefault();
                var contract = user.ContractDetails.Where(a => a.IsCurrent).FirstOrDefault();
                if (user == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "User not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (sumAttendance == null)
                {
                    sumAttendance = new Attendance();
                    check = false;
                }
                decimal rate = WorkingTrackings.Average(a => a.WorkingHourRate);
                var shift = WorkingTrackings.FirstOrDefault().Shift;
                decimal shiftHours = contract?.WorkingHours != null ? (decimal)contract?.WorkingHours : shift != null ? (decimal)(((TimeOnly)shift.To - (TimeOnly)shift.From).TotalMinutes / 60) : 0;
                var nohours = WorkingTrackings.Where(a => a.TaskId == null).Sum(a => a.TotalHours) + (sumAttendance.HolidayHours ?? 0);
                var overtimehours = nohours > shiftHours ? nohours - shiftHours : 0;
                var delayHours = shiftHours > nohours ? shiftHours - WorkingTrackings.Sum(a => a.TotalHours) : 0;

                if (WorkingTrackings.Average(a => a.TaskRate) > 1)
                {
                    rate = WorkingTrackings.Average(a => a.TaskRate);
                }
                sumAttendance.HrUserId = UserId;
                if (WorkingTrackings.FirstOrDefault() != null)
                {
                    sumAttendance.DepartmentId = WorkingTrackings.FirstOrDefault().HrUser?.DepartmentId ?? 0;
                    if (sumAttendance.DepartmentId == 0)
                    {
                        var department = _unitOfWork.Departments.GetAll().FirstOrDefault();
                        sumAttendance.DepartmentId = department.Id;
                    }
                    var checkin = WorkingTrackings.FirstOrDefault()?.CheckInTime.Value;
                    var checkoutValue = WorkingTrackings.Where(a => a.CheckInTime != null).OrderByDescending(a => a.Id).FirstOrDefault()?.CheckOutTime;
                    TimeOnly? checkout = checkoutValue != null ? checkoutValue.Value : null;
                    sumAttendance.AttendanceDate = DateOnly.FromDateTime(WorkingTrackings.FirstOrDefault().Date);
                    sumAttendance.BranchId = user.BranchId;
                    sumAttendance.CheckInHour = checkin.Value.Hour;
                    sumAttendance.CheckInMin = checkin.Value.Minute;

                    if (checkout != null)
                    {
                        sumAttendance.CheckOutHour = checkout.Value.Hour;
                        sumAttendance.CheckOutMin = checkout.Value.Minute;
                        sumAttendance.CheckOutDate = WorkingTrackings.Where(a => a.CheckInTime != null).OrderByDescending(a => a.Id).FirstOrDefault()?.CheckOutDate;
                    }
                    else
                    {
                        sumAttendance.CheckOutHour = null;
                        sumAttendance.CheckOutMin = null;
                    }
                    sumAttendance.NoHours = (int)(nohours - (sumAttendance.HolidayHours ?? 0));
                    sumAttendance.NoMin = (int)(((nohours - (sumAttendance.HolidayHours ?? 0)) - sumAttendance.NoHours) * 60);
                    sumAttendance.TaskHours = WorkingTrackings.Where(a => a.TaskId != null).Select(a => a.TotalHours).Sum();

                    var location = WorkingTrackings.Where(a => a.Latitude != null && a.Longitude != null).OrderByDescending(a => a.Id).FirstOrDefault();
                    if (location != null)
                    {
                        sumAttendance.Longitude = location.Longitude;
                        sumAttendance.Latitude = location.Latitude;
                    }
                }
                if (shiftHours != 0)
                {
                    var overRate = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => a.Rate > 0 && a.BranchId == user.BranchId).Select(a => a.Rate).FirstOrDefault();
                    var delayRate = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => a.Rate < 0 && a.BranchId == user.BranchId).Select(a => a.Rate).FirstOrDefault();
                    if (sumAttendance.DayTypeId == 2)
                    {
                        var vacation = _unitOfWork.VacationDays.FindAll(a => date.Date >= a.From.Date && date.Date <= a.To.Date && a.BranchId == user.BranchId).FirstOrDefault();
                        if (vacation == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err1010";
                            error.ErrorMSG = "No Vacation Even The Day Type Is Vacation";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        overRate = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.Rate > 0 && a.VacationDayId == vacation.Id).Select(a => a.Rate).FirstOrDefault();
                        delayRate = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.Rate < 0 && a.VacationDayId == vacation.Id).Select(a => a.Rate).FirstOrDefault();
                    }
                    var checkout = WorkingTrackings.Where(a => a.CheckOutTime != null).OrderByDescending(a => a.Id).FirstOrDefault()?.CheckOutTime.Value;
                    if (checkout != null)
                    {
                        sumAttendance.OverTimeHour = (int)overtimehours;
                        sumAttendance.OverTimeMin = (int)((overtimehours - sumAttendance.OverTimeHour) * 60);
                        sumAttendance.DelayHours = (int)delayHours;
                        sumAttendance.DelayMin = (int)((delayHours - sumAttendance.DelayHours) * 60);
                        sumAttendance.OvertimeCost = overtimehours * overRate * rate;
                        sumAttendance.DeductionCost = delayHours * delayRate * rate;
                        if (sumAttendance.DayTypeId == 2)
                        {
                            sumAttendance.OvertimeCost = (nohours + overtimehours) * overRate * rate;
                        }
                    }
                }
                sumAttendance.DayTypeId = GetDayTypeIdbyKnowingTheDate(date, (int)user.BranchId)[1];

                sumAttendance.ShiftHours = shiftHours;
                sumAttendance.TaskHours = WorkingTrackings.Where(a => a.TaskId != null).Select(a => a.TotalHours).Sum();
                if (check)
                {
                    if (sumAttendance.CheckInHour != null || sumAttendance.CheckInMin != null || sumAttendance.CheckOutHour != null || sumAttendance.CheckOutMin != null)
                    {
                        var leave = _unitOfWork.ContractLeaveEmployees.FindAll(a => a.ContractLeaveSettingId == sumAttendance.AbsenceTypeId && a.HrUserId == sumAttendance.HrUserId).FirstOrDefault();
                        if (leave != null)
                        {
                            leave.Used = leave.Used - 1;
                            leave.Remain = leave.Balance - leave.Used;
                            _unitOfWork.ContractLeaveEmployees.Update(leave);
                            _unitOfWork.Complete();
                        }
                        sumAttendance.AbsenceTypeId = null;
                        sumAttendance.AbsenceCause = null;
                        sumAttendance.AbsenceRejectCause = null;
                        sumAttendance.IsApprovedAbsence = null;
                        sumAttendance.ApprovedByUserId = null;
                    }
                    sumAttendance.ModifiedBy = creator;
                    sumAttendance.ModificationDate = DateOnly.FromDateTime(DateTime.Now);
                    var updatedAttendance = _unitOfWork.Attendances.Update(sumAttendance);
                    _unitOfWork.Complete();
                    Response.ID = updatedAttendance.Id;
                }
                else
                {
                    sumAttendance.CreatedBy = creator;
                    sumAttendance.ModifiedBy = creator;
                    sumAttendance.CreationDate = DateOnly.FromDateTime(DateTime.Now);
                    sumAttendance.ModificationDate = DateOnly.FromDateTime(DateTime.Now);
                    var addedAttendance = _unitOfWork.Attendances.Add(sumAttendance);
                    _unitOfWork.Complete();
                    Response.ID = addedAttendance.Id;
                }

            }
            SumAttendanceForPayroll(UserId, creator, date);
            return Response;


        }

        public BaseResponseWithId<long> AddDeductionWorkingHours(DeductWorkingHoursModel request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var WorkingHours = _unitOfWork.WorkingHoursTrackings.GetById(request.WorkingHoursId);
                if (WorkingHours == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Working Hours not found";
                    Response.Errors.Add(error);
                    return Response;
                }

                WorkingHourseTracking DeductionHours = WorkingHours;
                DeductionHours.Id = 0;
                DeductionHours.CheckInTime = null;
                DeductionHours.CheckOutTime = null;
                DeductionHours.TotalHours = request.NoOfHours;
                _unitOfWork.WorkingHoursTrackings.Add(DeductionHours);
                _unitOfWork.Complete();
                SumWorkingTrackingHoursForAttendance(DeductionHours.HrUserId, DeductionHours.Date, validation.userID);
                Response.ID = DeductionHours.Id;
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

        public BaseResponseWithData<GetMonthlyAttendanceModel> GetMonthlyAttendance(int month, int year, int branchId, int? departmentId)
        {
            BaseResponseWithData<GetMonthlyAttendanceModel> Response = new BaseResponseWithData<GetMonthlyAttendanceModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            var Data = new GetMonthlyAttendanceModel();
            Data.AttendanceSummary = new List<GetAttendanceSum>();

            var UserNum = _unitOfWork.HrUsers.FindAll(a => a.Active).Count();

            if (branchId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invalid Branch Id";
                Response.Errors.Add(error);
                return Response;
            }
            var branch = _unitOfWork.Branches.GetById(branchId);
            if (branch == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "branch not found";
                Response.Errors.Add(error);
                return Response;
            }
            UserNum = _unitOfWork.HrUsers.FindAll(a => a.Active && a.BranchId == branchId).Count();
            if (departmentId != null)
            {
                var department = _unitOfWork.Departments.GetById((int)departmentId);
                if (department == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "department not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                UserNum = _unitOfWork.HrUsers.FindAll(a => a.Active && a.BranchId == branchId && a.DepartmentId == departmentId).Count();
            }

            if (month == 0)
            {
                month = DateTime.Now.Month;
            }
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            if (month <= 0 || month > 12)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Inavlid Value For Month";
                Response.Errors.Add(error);
                return Response;
            }
            if (year > DateTime.MaxValue.Year || year < DateTime.MinValue.Year)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invalid Value For Year";
                Response.Errors.Add(error);
                return Response;
            }
            Data.UsersNumbers = UserNum;
            //var datenow = DateTime.Now;
            var BranchSettingdb = _unitOfWork.BranchSetting.FindAll(a => a.BranchId == branchId).FirstOrDefault();
            if (BranchSettingdb == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invalid Branch Setting";
                Response.Errors.Add(error);
                return Response;
            }
            if (BranchSettingdb.PayrollFrom == null || BranchSettingdb.PayrollTo == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Branch Setting Payroll is required.";
                Response.Errors.Add(error);
                return Response;
            }
            if (BranchSettingdb != null)
            {
                DateOnly start = new DateOnly(year, month, (int)BranchSettingdb.PayrollFrom); // default in last month
                DateOnly end = new DateOnly(year, month, (int)BranchSettingdb.PayrollTo).AddMonths(1);
                int diffrence = (int)BranchSettingdb.PayrollTo - (int)BranchSettingdb.PayrollFrom;
                if (diffrence >= 15) // the same month
                {
                    start = new DateOnly(year, month, (int)BranchSettingdb.PayrollFrom); // in the same month
                }
                var Attendances = _unitOfWork.Attendances.FindAll(a => a.AttendanceDate >= start && a.AttendanceDate < end && a.BranchId == branchId, includes: new[] {"HrUser"});
                if (departmentId != null)
                {
                    Attendances = Attendances.Where(a => a.DepartmentId == departmentId);
                }
                while (start < end)
                {
                    var AttendanceDay = new GetAttendanceSum();
                    var attendance = Attendances.Where(a => a.AttendanceDate == start).ToList();
                    AttendanceDay.Date = start;

                    //Modify There
                    var daytypeId = GetDayTypeIdbyKnowingTheDate(new DateTime(start.Year, start.Month, start.Day), branchId)[1];
                    AttendanceDay.DayIs = _unitOfWork.DayTypes.FindAll(a => a.Id == daytypeId).FirstOrDefault().DayType1;
                    if (daytypeId == 2)
                    {
                        var vacation = _unitOfWork.VacationDays.FindAll(a => DateOnly.FromDateTime(a.From) <= start && start <= DateOnly.FromDateTime(a.To)).FirstOrDefault();
                        if (vacation != null)
                        {
                            AttendanceDay.VacationDayId = vacation.Id;
                            AttendanceDay.IsWorkingHoursApplied = vacation.IsWhApplied;
                            AttendanceDay.IsPaid = vacation.IsPaid;
                        }
                    }
                    if (attendance != null)
                    {
                        AttendanceDay.WeekDayName = start.DayOfWeek.ToString();
                        AttendanceDay.AttendanceNum = attendance.Where(a => a.HrUserId != null && a.CheckInHour != null).Distinct().Count();
                        AttendanceDay.AttendancePercentage = UserNum != 0 ? ((decimal)AttendanceDay.AttendanceNum / (decimal)UserNum) * 100 : 0;

                        AttendanceDay.VacationRequestsNum = attendance.Where(a => a.IsApprovedAbsence == true && a.CheckInHour == null).Count();
                        AttendanceDay.VacationRequestsUsers = attendance.Where(a=>a.IsApprovedAbsence == true && a.CheckInHour == null).
                            Select(a=>a.HrUser.FirstName+" "+(!string.IsNullOrWhiteSpace(a.HrUser.MiddleName)?a.HrUser.MiddleName+" ":null)+a.HrUser.LastName).ToList();
                        AttendanceDay.VacationRequestsPercentage = UserNum != 0 ? ((decimal)AttendanceDay.VacationRequestsNum / (decimal)UserNum) * 100 : 0;
                        AttendanceDay.AbsenceNum = UserNum - (AttendanceDay.AttendanceNum + AttendanceDay.VacationRequestsNum);
                    }

                    Data.AttendanceSummary.Add(AttendanceDay);
                    start = start.AddDays(1);
                }
            }
            var data = CalculateDayTypesOfMonth(month, branchId);
            Data.WorkingDaysMumber = data[0];
            Data.Holidays = data[1];
            Data.Weekends = data[2];
            Data.DaysOFMonth = data.Sum();
            Response.Data = Data;
            return Response;
        }

        public BaseResponseWithData<RequestAbsenceResponse> RequestAbsence([FromBody] RequestAbsenceDto request, long creator)
        {
            BaseResponseWithData<RequestAbsenceResponse> Response = new BaseResponseWithData<RequestAbsenceResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new RequestAbsenceResponse();
            if (request == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invalid Data";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.To.Date < request.From.Date)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "your vacation end date can't be before the first date";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.From.Date < DateTime.Now.Date)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "you can't request Vacation in the past";
                Response.Errors.Add(error);
                return Response;
            }
            var absencetype = _unitOfWork.ContractLeaveSetting.GetById(request.AbsenceTypeId);
            if (absencetype == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "This Absence type is not found";
                Response.Errors.Add(error);
                return Response;
            }
            var user = _unitOfWork.HrUsers.FindAll(a => a.Id == request.HrUserId, includes: new[] { "ContractDetails.ContractReportTos", }).FirstOrDefault();
            var contract = user.ContractDetails.Where(a => a.IsCurrent == true).FirstOrDefault();
            var reportTos = contract.ContractReportTos.ToList();
            var FirstReportTo = reportTos.FirstOrDefault()?.ReportToId ?? 0;
            Response.Data.FirstReportToId = FirstReportTo;
            long SecondReportTo = 0;
            if (reportTos.Count > 1)
            {
                SecondReportTo = reportTos.LastOrDefault()?.ReportToId ?? 0;
            }
            Response.Data.SecondReportToId = SecondReportTo;
            if (user == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err105";
                error.ErrorMSG = "This user is not found";
                Response.Errors.Add(error);
                return Response;
            }
            var leavecheck = _unitOfWork.LeaveRequests.FindAll(a => ((a.From.Date >= request.From.Date && a.From.Date <= request.To.Date) ||
            (a.To.Date >= request.From.Date && a.To.Date <= request.To.Date) ||
            (request.From.Date >= a.From.Date && request.From.Date <= a.To.Date) ||
            (request.To.Date >= a.From.Date && request.To.Date <= a.To.Date)) && a.HrUserId == request.HrUserId).FirstOrDefault();
            if (leavecheck != null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err106";
                error.ErrorMSG = "there's another Leave request is intersecting with the same period";
                Response.Errors.Add(error);
                return Response;
            }
            /*if (request.FirstApprovedBy != 0)
            {
                var user1 = _unitOfWork.Users.GetById(request.FirstApprovedBy);
                if (user1 == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err107";
                    error.ErrorMSG = "first user to approve not found";
                    Response.Errors.Add(error);
                    return Response;
                }

            }
            if (request.SecondApprovedBy != 0)
            {
                var user2 = _unitOfWork.Users.GetById(request.FirstApprovedBy);
                if (user2 == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err108";
                    error.ErrorMSG = "first user to approve not found";
                    Response.Errors.Add(error);
                    return Response;
                }

            }*/
            var LeaveRequest = new LeaveRequest();

            LeaveRequest.From = request.From;
            LeaveRequest.To = request.To;
            LeaveRequest.HrUserId = user.Id;
            LeaveRequest.VacationTypeId = request.AbsenceTypeId;
            LeaveRequest.AbsenceCause = request.AbsenceCause;
            LeaveRequest.CreationDate = DateTime.Now;
            LeaveRequest.ModifiedDate = DateTime.Now;
            LeaveRequest.CreatedBy = creator;
            LeaveRequest.ModifiedBy = creator;
            if (FirstReportTo != 0)
            {
                LeaveRequest.FirstApprovedBy = FirstReportTo;
            }
            if (SecondReportTo != 0)
            {
                LeaveRequest.SecondApprovedBy = SecondReportTo;
            }

            var checkrole = _unitOfWork.UserRoles.FindAll(a => a.UserId == creator && a.RoleId == 23).FirstOrDefault();
            if (checkrole != null)
            {
                Response.Data.FirstReportToId = 0;
                Response.Data.SecondReportToId = 0;
                LeaveRequest.FirstApproval = true;
                LeaveRequest.SecondApproval = true;
                LeaveRequest.FirstApprovedBy = creator;
                LeaveRequest.SecondApprovedBy = creator;
                LeaveRequest.FirstApprovalDate = DateTime.Now;
                LeaveRequest.SecondApprovalDate = DateTime.Now;
                //var contract = _unitOfWork.Contracts.FindAll(a => a.HrUserId == user.Id && a.IsCurrent).FirstOrDefault();
                if (contract != null)
                {
                    var leaveEmployee = _unitOfWork.ContractLeaveEmployees.FindAll(a => a.ContractLeaveSettingId == LeaveRequest.VacationTypeId && a.HrUserId == user.Id && a.ContractId == contract.Id && a.LeaveAllowed == "Allowed").FirstOrDefault();
                    var absencePeriod = (LeaveRequest.To.Date - LeaveRequest.From.Date).Days + 1;

                    if (leaveEmployee != null && leaveEmployee.Remain >= absencePeriod)
                    {
                        leaveEmployee.Used = leaveEmployee.Used + absencePeriod;
                        leaveEmployee.Remain = leaveEmployee.Balance - leaveEmployee.Used;
                        _unitOfWork.ContractLeaveEmployees.Update(leaveEmployee);
                        _unitOfWork.Complete();

                        var start = LeaveRequest.From.Date;
                        var end = LeaveRequest.To.Date;
                        while (start <= end)
                        {
                            var daytypeId = GetDayTypeIdbyKnowingTheDate(start, user.BranchId ?? 0)[1];
                            if (daytypeId != 1)
                            {
                                var attendance = new Attendance();
                                attendance.DepartmentId = user.DepartmentId ?? _unitOfWork.Departments.GetAll().FirstOrDefault()?.Id ?? 0;
                                attendance.AttendanceDate = DateOnly.FromDateTime(start);
                                attendance.AbsenceCause = LeaveRequest.AbsenceCause;
                                attendance.AbsenceTypeId = LeaveRequest.VacationTypeId;
                                attendance.IsApprovedAbsence = true;
                                attendance.ApprovedByUserId = LeaveRequest.FirstApprovedBy ?? LeaveRequest.SecondApprovedBy;
                                attendance.DayTypeId = daytypeId;
                                attendance.Active = true;
                                attendance.HrUserId = LeaveRequest.HrUserId;
                                attendance.BranchId = user.BranchId;
                                attendance.CreatedBy = creator;
                                attendance.ModifiedBy = creator;
                                attendance.CreationDate = DateOnly.FromDateTime(DateTime.Now);
                                attendance.ModificationDate = DateOnly.FromDateTime(DateTime.Now);
                                attendance.VacationHours = contract.WorkingHours ?? 0;
                                _unitOfWork.Attendances.Add(attendance);
                                _unitOfWork.Complete();
                                SumAttendanceForPayroll(user.Id, creator, start);
                            }
                            start = start.AddDays(1);
                        }



                        //-----------------------------------------------------------------------------------------
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err109";
                        error.ErrorMSG = "Your Leave Period Is more than the number of days left of your leave days";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
            }

            var addedRequest = _unitOfWork.LeaveRequests.Add(LeaveRequest);
            _unitOfWork.Complete();
            //-------------------------------send Notifications------------------------------------------------
            var HrJobTitle = _unitOfWork.JobTitles.FindAll(a => a.Name == "HR").Select(a => a.Id).FirstOrDefault();
            var HRAdmin = _unitOfWork.Users.FindAll(a => a.JobTitleId == (HrJobTitle)).FirstOrDefault();
            _notificationService.CreateNotification(creator, "Vacation", "Need Approval", addedRequest.Id.ToString(),
                               true, FirstReportTo, 0);
            if (HRAdmin != null)
            {
                _notificationService.CreateNotification(creator, "Vacation", "Need Approval", addedRequest.Id.ToString(),
                                   true, HRAdmin.Id, 0);
            }
            //-------------------------------send Email------------------------------------------------
            double totalNumOfRequestedDays = (request.To - request.From).TotalDays + 1;
            int numOfOffDays = 0;
            var branchId = _unitOfWork.Branches.Find(a => a.Id == user.BranchId);
            var vacationsDays = _unitOfWork.VacationDays.FindAll(a => a.BranchId == user.BranchId && (a.From > request.From && a.To < request.To));
            var startDate = request.From;
            var weekDays = _unitOfWork.WeekDays.GetAll();

            var firstNameOfReportingto = string.Empty;
            var lastNameOfReportingto = string.Empty;

            var generalVacationDays = _unitOfWork.VacationDays.FindAll(a => a.BranchId == user.BranchId);


            while (startDate <= request.To)     //calaculate weekend days in the vacation intervel in this  loop
            {
                string dayName = startDate.ToString("dddd");
                var day = weekDays.Where(a => a.Day.ToLower() == dayName.ToLower() && a.BranchId == user.BranchId).FirstOrDefault();
                var vacationInBranch = generalVacationDays.Where(a => a.From.Date <= startDate.Date && a.To.Date >= startDate.Date).FirstOrDefault();
                if ((day.IsWeekEnd ?? false) || vacationInBranch != null)
                {
                    numOfOffDays = numOfOffDays + 1;
                }
                startDate = startDate.AddDays(1);
            }
            var firstReportingToHrUser = _unitOfWork.HrUsers.GetById(FirstReportTo);
            if (firstReportingToHrUser != null)
            {
                firstNameOfReportingto = firstReportingToHrUser.FirstName;
                lastNameOfReportingto = firstReportingToHrUser.LastName;

            }
            var emailUser = _unitOfWork.Users.Find(a => a.Id == FirstReportTo);
            
            if (emailUser != null)
            {
                _mailService.SendMail(new MailData()
                {
                    EmailToName = emailUser.Email,
                    EmailToId = emailUser.Email,
                    EmailSubject = "vacation Need Approval",
                    EmailBody = $"Hi {firstNameOfReportingto + " " + lastNameOfReportingto},\r\n\r\nI hope this message finds you well.\r\n\r\nI am writing to formally request vacation leave for {user.FirstName+' '+user.LastName} from {request.From.ToShortDateString()} to {request.To.ToShortDateString()}, totaling {totalNumOfRequestedDays - (numOfOffDays)} business days.\r\n\r\nDetails:\r\n\r\n \t• Vacation Start Date: {request.From.ToShortDateString()}\r\n \t• Vacation End Date: {request.To.ToShortDateString()}\r\n \t• Total Days Requested: {totalNumOfRequestedDays - (numOfOffDays)}\r\n"
                });
                if (HRAdmin != null)
                {
                    _mailService.SendMail(new MailData()
                    {
                        EmailToName = HRAdmin.Email,
                        EmailToId = HRAdmin.Email,
                        EmailSubject = "vacation Need Approval",
                        EmailBody = $"Hi {firstNameOfReportingto + " " + lastNameOfReportingto},\r\n\r\nI hope this message finds you well.\r\n\r\nI am writing to formally request vacation leave for {user.FirstName + ' ' + user.LastName} from {request.From.ToShortDateString()} to {request.To.ToShortDateString()}, totaling {totalNumOfRequestedDays - (numOfOffDays)} business days.\r\n\r\nDetails:\r\n\r\n \t• Vacation Start Date: {request.From.ToShortDateString()}\r\n \t• Vacation End Date: {request.To.ToShortDateString()}\r\n \t• Total Days Requested: {totalNumOfRequestedDays - (numOfOffDays)}\r\n"
                    });
                }
            }

            var hruser = _unitOfWork.HrUsers.Find(a => a.Id == request.HrUserId);
            if (hruser != null && hruser.UserId != null)
            {

                var userOfHrUser = _unitOfWork.Users.GetById(hruser.UserId ?? 0);
                _mailService.SendMail(new MailData()
                {
                    EmailToName = userOfHrUser.Email,
                    EmailToId = userOfHrUser.Email,
                    EmailSubject = "vacation Request Sent",
                    EmailBody = $"Hi {firstNameOfReportingto + " " + lastNameOfReportingto},\r\n\r\nI hope this message finds you well.\r\n\r\nI am writing to formally request vacation leave for {user.FirstName + ' ' + user.LastName} from {request.From.ToShortDateString()} to {request.To.ToShortDateString()}, totaling {totalNumOfRequestedDays - (numOfOffDays)} business days.\r\n\r\nDetails:\r\n\r\n \t• Vacation Start Date: {request.From.ToShortDateString()}\r\n \t• Vacation End Date: {request.To.ToShortDateString()}\r\n \t• Total Days Requested: {totalNumOfRequestedDays - (numOfOffDays)}\r\n"
                });
            }
            Response.Data.Id = addedRequest.Id;


            return Response;
        }

        public int CalculateAbsenceDays(DateTime start, DateTime end, int branchId)
        {
            int days = 0;
            var daytype = 0;
            while (start <= end)
            {
                daytype = GetDayTypeIdbyKnowingTheDate(start, branchId)[1];
                if (daytype == 3)
                {
                    days++;
                }
                start = start.AddDays(1);
            }
            return days;

        }
        public BaseResponseWithData<ApproveAbsenceResponse> ApproveAbsence(ApproveAbsenceModel request, long creator)
        {
            BaseResponseWithData<ApproveAbsenceResponse> Response = new BaseResponseWithData<ApproveAbsenceResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new ApproveAbsenceResponse();
            if (request.RequestId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "request Id is Required";
                Response.Errors.Add(error);
                return Response;
            }
            var leaveRequest = _unitOfWork.LeaveRequests.GetById(request.RequestId);
            if (leaveRequest.SecondApprovedBy != null)
            {
                Response.Data.SecondReportTo = leaveRequest.SecondApprovedBy ?? 0;
            }
            if (leaveRequest == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "request is not Found";
                Response.Errors.Add(error);
                return Response;
            }
            if (leaveRequest.From < DateTime.Now.Date)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err105";
                error.ErrorMSG = "the Date of Attendance request is past";
                Response.Errors.Add(error);
                return Response;
            }
            if ((leaveRequest.FirstApproval != null && leaveRequest.SecondApprovedBy == null) ||
                (leaveRequest.SecondApproval != null && leaveRequest.FirstApprovedBy == null) ||
                (leaveRequest.FirstApproval != null && leaveRequest.SecondApproval != null))
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err105";
                if (leaveRequest.FirstApproval == true && leaveRequest.SecondApprovedBy == null) error.ErrorMSG = "This Request Is Already Approved";
                else if (leaveRequest.FirstApproval == false && leaveRequest.SecondApprovedBy == null) error.ErrorMSG = "This Request Is Already Rejected";
                else if (leaveRequest.SecondApproval == true && leaveRequest.FirstApprovedBy == null) error.ErrorMSG = "This Request Is Already Approved";
                else if (leaveRequest.SecondApproval == false && leaveRequest.FirstApprovedBy == null) error.ErrorMSG = "This Request Is Already Rejected";
                else if (leaveRequest.FirstApproval == true && leaveRequest.SecondApproval == true) error.ErrorMSG = "This Request Is Already Approved";
                else error.ErrorMSG = "This Request Is Already Rejected";
                Response.Errors.Add(error);
                return Response;
            }

            if (leaveRequest.FirstApproval == null)
            {
                leaveRequest.FirstApproval = request.Approval;
                leaveRequest.FirstRejectionCause = request.AbsenceRejectCause;
                leaveRequest.FirstApprovalDate = DateTime.Now;
            }
            else if (leaveRequest.SecondApproval == null)
            {
                leaveRequest.SecondApproval = request.Approval;
                leaveRequest.SecondRejectionCause = request.AbsenceRejectCause;
                leaveRequest.SecondApprovalDate = DateTime.Now;
            }

            if ((leaveRequest.FirstApproval == true && leaveRequest.SecondApprovedBy == null) ||
                (leaveRequest.SecondApproval == true && leaveRequest.FirstApprovedBy == null) ||
                (leaveRequest.FirstApproval == true && leaveRequest.SecondApproval == true))
            {
                var contract = _unitOfWork.Contracts.FindAll(a => a.HrUserId == leaveRequest.HrUserId && a.IsCurrent).FirstOrDefault();
                if (contract != null)
                {
                    var leaveEmployee = _unitOfWork.ContractLeaveEmployees.FindAll(a => a.ContractLeaveSettingId == leaveRequest.VacationTypeId && a.HrUserId == leaveRequest.HrUserId && a.ContractId == contract.Id).FirstOrDefault();
                    var user = _unitOfWork.HrUsers.GetById(leaveRequest.HrUserId);
                    var absencePeriod = CalculateAbsenceDays(leaveRequest.From.Date, leaveRequest.To.Date, user.BranchId ?? 0);
                    if (leaveEmployee != null && leaveEmployee.Remain >= absencePeriod)
                    {
                        leaveEmployee.Used = leaveEmployee.Used + absencePeriod;
                        leaveEmployee.Remain = leaveEmployee.Balance - leaveEmployee.Used;
                        _unitOfWork.ContractLeaveEmployees.Update(leaveEmployee);
                        _unitOfWork.Complete();
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err109";
                        error.ErrorMSG = "Your Leave Period Is more than the number of days left of your leave days";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
            }
            
            var updatedRequest = _unitOfWork.LeaveRequests.Update(leaveRequest);
            _unitOfWork.Complete();
            if ((updatedRequest.FirstApproval == true && updatedRequest.SecondApprovedBy == null) ||
                (updatedRequest.SecondApproval == true && updatedRequest.FirstApprovedBy == null) ||
                (updatedRequest.FirstApproval == true && updatedRequest.SecondApproval == true))
            {
                var user = _unitOfWork.HrUsers.GetById(updatedRequest.HrUserId);
                var contract = _unitOfWork.Contracts.FindAll(a => a.IsCurrent == true && a.HrUserId == user.Id).FirstOrDefault();
                var start = updatedRequest.From.Date;
                var end = updatedRequest.To.Date;
                var daytype = 0;
                while (start <= end)
                {
                    daytype = GetDayTypeIdbyKnowingTheDate(start, user.BranchId ?? 0)[1];
                    if (daytype == 3)
                    {
                        var attendance = new Attendance();
                        attendance.DepartmentId = user.DepartmentId ?? _unitOfWork.Departments.GetAll().FirstOrDefault()?.Id ?? 0;
                        attendance.AttendanceDate = DateOnly.FromDateTime(start);
                        attendance.AbsenceCause = updatedRequest.AbsenceCause;
                        attendance.AbsenceTypeId = updatedRequest.VacationTypeId;
                        attendance.IsApprovedAbsence = true;
                        attendance.ApprovedByUserId = updatedRequest.FirstApprovedBy ?? updatedRequest.SecondApprovedBy;
                        attendance.DayTypeId = daytype;
                        attendance.Active = true;
                        attendance.HrUserId = updatedRequest.HrUserId;
                        attendance.BranchId = user.BranchId;
                        attendance.CreatedBy = creator;
                        attendance.ModifiedBy = creator;
                        attendance.CreationDate = DateOnly.FromDateTime(DateTime.Now);
                        attendance.ModificationDate = DateOnly.FromDateTime(DateTime.Now);
                        attendance.VacationHours = contract?.WorkingHours ?? 0;
                        _unitOfWork.Attendances.Add(attendance);
                        _unitOfWork.Complete();
                        SumAttendanceForPayroll(updatedRequest.HrUserId, creator, start);
                    }
                    start = start.AddDays(1);
                }


            }
            Response.Data.RequestId = updatedRequest.Id;
            Response.Data.HrUserId = (long)updatedRequest.HrUserId;
            return Response;
        }
        public BaseResponseWithData<GetProgressForAllTasks> GetProgressForAllTask(ProgressForAllTaskFilter filters, string CompanyName)
        {
            BaseResponseWithData<GetProgressForAllTasks> Response = new BaseResponseWithData<GetProgressForAllTasks>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new GetProgressForAllTasks();
            Response.Data.groupedtasks = new List<Groupedtasks>();
            var tasks = new List<GetProgressForAllTasksDto>();
            if (filters.projectId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Project Id Is Required";
                Response.Errors.Add(error);
                return Response;
            }
            var Project = _unitOfWork.Projects.Find(a => a.Id == filters.projectId, includes: new[] { "Currency" });
            if (Project == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err106";
                error.ErrorMSG = "Project is not found";
                Response.Errors.Add(error);
                return Response;
            }

            var flags = new List<bool>() { filters.GroupByTask, filters.GroupByJobTitle, filters.GroupByDate, filters.GroupByUser };
            if (flags.Count(a => a == true) > 1)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err105";
                error.ErrorMSG = "You Can't Group By More Than One Criteria";
                Response.Errors.Add(error);
                return Response;
            }
            var TasksProgress = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.TaskId != null && a.ProjectId != null && filters.projectId == a.ProjectId, includes: new[] { "HrUser.JobTitle", "Task", "Project.SalesOffer" }).ToList();

            tasks = TasksProgress.Select(a => new GetProgressForAllTasksDto()
            {
                Id = a.Id,
                projectName = a.Project?.SalesOffer?.ProjectName,
                TaskName = a.Task?.Name,
                UserName = a.HrUser.FirstName + " " + a.HrUser.LastName,
                Date = a.Date.ToShortDateString(),
                ProgressNote = a.ProgressNote,
                ProgressPercent = a?.ProgressRate ?? 0,
                TotalHours = a.TotalHours,
                CheckIn = a.CheckInTime != null ? (TimeOnly)a.CheckInTime : new TimeOnly(0, 0, 0),
                CheckOut = a.CheckOutTime != null ? (TimeOnly)a.CheckOutTime : new TimeOnly(0, 0, 0),
                WorkingHourRate = filters.IsProjectInvoice ? a.TaskRate > 1 ? a.TaskRate : a.WorkingHourRate : 0,
                Cost = 0,
                JobTitle = a.HrUser.JobTitle.Name,
                IsInvoiced = _unitOfWork.ProjectInvoiceItems.FindAll(x => x.ItemId == a.Id && x.Type == "WTP").FirstOrDefault() != null ? true : false,
                InvoiceId = _unitOfWork.ProjectInvoiceItems.FindAll(x => x.ItemId == a.Id && x.Type == "WTP").FirstOrDefault()?.Id ?? 0
            }).ToList();
            tasks = tasks.Select(x => { x.Cost = filters.IsProjectInvoice ? x.TotalHours * x.WorkingHourRate : 0; return x; }).ToList();
            if (filters.GroupByTask || filters.GroupByDate || filters.GroupByUser || filters.GroupByJobTitle)
            {
                var finaltasks = new List<IGrouping<string, GetProgressForAllTasksDto>>();
                if (filters.GroupByTask && !filters.GroupByDate && !filters.GroupByUser && !filters.GroupByJobTitle)
                {
                    finaltasks = tasks.GroupBy(a => a.TaskName).ToList();
                }
                else if (!filters.GroupByTask && filters.GroupByDate && !filters.GroupByUser && !filters.GroupByJobTitle)
                {
                    finaltasks = tasks.GroupBy(a => a.Date).ToList();
                }
                else if (!filters.GroupByTask && !filters.GroupByDate && filters.GroupByUser && !filters.GroupByJobTitle)
                {
                    finaltasks = tasks.GroupBy(a => a.UserName).ToList();
                }
                else if (!filters.GroupByTask && !filters.GroupByDate && !filters.GroupByUser && filters.GroupByJobTitle)
                {
                    finaltasks = tasks.GroupBy(a => a.JobTitle).ToList();
                }
                if (finaltasks.Count() > 0)
                {
                    Response.Data.groupedtasks = finaltasks.Select(a => new Groupedtasks() { key = a.Key, tasks = [.. a] }).ToList();
                    ExcelPackage excel = new ExcelPackage();
                    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                    workSheet.TabColor = System.Drawing.Color.Black;
                    workSheet.DefaultRowHeight = 12;
                    workSheet.Row(1).Height = 20;
                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(1).Style.Font.Bold = true;
                    workSheet.Cells[1, 1].Value = "Project Name";
                    workSheet.Cells[1, 2].Value = "Task Name";
                    workSheet.Cells[1, 3].Value = "User Name";
                    workSheet.Cells[1, 4].Value = "User JobTitle";
                    workSheet.Cells[1, 5].Value = "Date";
                    workSheet.Cells[1, 6].Value = "Progress Comment";
                    workSheet.Cells[1, 7].Value = "Progress Percentage";
                    workSheet.Cells[1, 8].Value = "Total Working Hours";
                    workSheet.Cells[1, 9].Value = "Check In Time";
                    workSheet.Cells[1, 10].Value = "Check Out Time";
                    if (filters.IsProjectInvoice)
                    {
                        workSheet.Cells[1, 11].Value = "Working Hour rate";
                        workSheet.Cells[1, 12].Value = "Working Hour Cost";
                    }
                    int recordIndex = 2;
                    foreach (var task in finaltasks)
                    {
                        workSheet.Cells[recordIndex, 1, recordIndex, filters.IsProjectInvoice ? 12 : 10].Merge = true;
                        workSheet.Cells[recordIndex, 1, recordIndex, filters.IsProjectInvoice ? 12 : 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[recordIndex, 1, recordIndex, filters.IsProjectInvoice ? 12 : 10].Style.Fill.BackgroundColor.SetColor(color: Color.DeepSkyBlue);
                        workSheet.Cells[recordIndex, 1, recordIndex, filters.IsProjectInvoice ? 12 : 10].Style.Font.Bold = true;
                        workSheet.Cells[recordIndex, 1, recordIndex, filters.IsProjectInvoice ? 12 : 10].Style.Font.Color.SetColor(Color.White);
                        workSheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                        workSheet.Cells[recordIndex, 1].Style.Font.Size = 14;
                        workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        if (filters.GroupByDate)
                        {
                            workSheet.Cells[recordIndex, 1].Value = "Progress Made In " + task.Key;
                        }
                        else if (filters.GroupByUser)
                        {
                            workSheet.Cells[recordIndex, 1].Value = "Progress Made By " + task.Key;
                        }
                        else if (filters.GroupByTask)
                        {
                            workSheet.Cells[recordIndex, 1].Value = "Progress Made For " + task.Key + " Task";
                        }
                        else if (filters.GroupByJobTitle)
                        {
                            workSheet.Cells[recordIndex, 1].Value = "Progress Made By the Jobtitle " + task.Key;
                        }
                        recordIndex += 1;
                        foreach (var item in task)
                        {
                            workSheet.Cells[recordIndex, 1].Value = item.projectName;
                            workSheet.Cells[recordIndex, 2].Value = item.TaskName;
                            workSheet.Cells[recordIndex, 3].Value = item.UserName;
                            workSheet.Cells[recordIndex, 4].Value = item.JobTitle;
                            workSheet.Cells[recordIndex, 5].Value = item.Date;
                            workSheet.Cells[recordIndex, 6].Value = item.ProgressNote;
                            workSheet.Cells[recordIndex, 7].Style.Numberformat.Format = "#0\\.00%";
                            workSheet.Cells[recordIndex, 7].Value = item.ProgressPercent;
                            workSheet.Cells[recordIndex, 8].Value = item.TotalHours;
                            workSheet.Cells[recordIndex, 9].Value = item.CheckIn;
                            workSheet.Cells[recordIndex, 10].Value = item.CheckOut;
                            if (filters.IsProjectInvoice)
                            {
                                workSheet.Cells[recordIndex, 11].Value = item.WorkingHourRate;
                                workSheet.Cells[recordIndex, 12].Value = item.Cost;
                            }
                            workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            recordIndex += 1;
                        }
                        recordIndex++;
                    }
                    workSheet.Column(1).AutoFit();
                    workSheet.Column(2).AutoFit();
                    workSheet.Column(3).AutoFit();
                    workSheet.Column(4).AutoFit();
                    workSheet.Column(5).AutoFit();
                    workSheet.Column(6).AutoFit();
                    workSheet.Column(7).AutoFit();
                    workSheet.Column(8).AutoFit();
                    workSheet.Column(9).AutoFit();
                    workSheet.Column(10).AutoFit();
                    workSheet.Column(11).AutoFit();
                    workSheet.Column(12).AutoFit();
                    var path = $"Attachments\\{CompanyName}\\TaskProgress";
                    var savedPath = Path.Combine(_host.WebRootPath, path);
                    if (File.Exists(savedPath))
                        File.Delete(savedPath);

                    // Create excel file on physical disk  
                    Directory.CreateDirectory(savedPath);
                    //FileStream objFileStrm = File.Create(savedPath);
                    //objFileStrm.Close();
                    var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                    var excelPath = savedPath + $"\\TaskProgress_{date}.xlsx";
                    excel.SaveAs(excelPath);
                    // Write content to excel file  
                    //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                    //Close Excel package 
                    excel.Dispose();
                    Response.Data.SavedPath = Globals.baseURL + '\\' + path + $"\\TaskProgress_{date}.xlsx";
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err107";
                    error.ErrorMSG = "No Tasks Progress to be Downloaded";
                    Response.Errors.Add(error);
                    return Response;
                }
            }
            else
            {
                if (tasks.Count() > 0)
                {
                    var returnedTasks = PagedList<GetProgressForAllTasksDto>.Create(tasks.AsQueryable(), filters.CurrentPage, filters.PageSize);
                    Response.Data.tasks = tasks;
                    ExcelPackage excel = new ExcelPackage();
                    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                    workSheet.TabColor = System.Drawing.Color.Black;
                    workSheet.DefaultRowHeight = 12;
                    workSheet.Row(1).Height = 20;
                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(1).Style.Font.Bold = true;
                    workSheet.Cells[1, 1].Value = "Project Name";
                    workSheet.Cells[1, 2].Value = "Task Name";
                    workSheet.Cells[1, 3].Value = "User Name";
                    workSheet.Cells[1, 4].Value = "User Jobtitle";
                    workSheet.Cells[1, 5].Value = "Date";
                    workSheet.Cells[1, 6].Value = "Progress Comment";
                    workSheet.Cells[1, 7].Value = "Progress Percentage";
                    workSheet.Cells[1, 8].Value = "Total Working Hours";
                    workSheet.Cells[1, 9].Value = "Check In Time";
                    workSheet.Cells[1, 10].Value = "Check Out Time";
                    if (filters.IsProjectInvoice)
                    {
                        workSheet.Cells[1, 11].Value = "Working Hour rate";
                        workSheet.Cells[1, 12].Value = "Working Hour Cost";
                    }
                    int recordIndex = 2;
                    foreach (var task in tasks)
                    {
                        workSheet.Cells[recordIndex, 1].Value = task.projectName;
                        workSheet.Cells[recordIndex, 2].Value = task.TaskName;
                        workSheet.Cells[recordIndex, 3].Value = task.UserName;
                        workSheet.Cells[recordIndex, 4].Value = task.JobTitle;
                        workSheet.Cells[recordIndex, 5].Style.Numberformat.Format = "yyyy/mm/dd";
                        workSheet.Cells[recordIndex, 5].Value = task.Date;
                        workSheet.Cells[recordIndex, 6].Value = task.ProgressNote;
                        workSheet.Cells[recordIndex, 7].Style.Numberformat.Format = "#0\\.00%";
                        workSheet.Cells[recordIndex, 7].Value = task.ProgressPercent;
                        workSheet.Cells[recordIndex, 8].Value = task.TotalHours;
                        workSheet.Cells[recordIndex, 9].Value = task.CheckIn;
                        workSheet.Cells[recordIndex, 10].Value = task.CheckOut;
                        if (filters.IsProjectInvoice)
                        {
                            workSheet.Cells[recordIndex, 11].Value = task.WorkingHourRate;
                            workSheet.Cells[recordIndex, 12].Value = task.Cost;
                        }
                        workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        recordIndex++;
                    }
                    workSheet.Column(1).AutoFit();
                    workSheet.Column(2).AutoFit();
                    workSheet.Column(3).AutoFit();
                    workSheet.Column(4).AutoFit();
                    workSheet.Column(5).AutoFit();
                    workSheet.Column(6).AutoFit();
                    workSheet.Column(7).AutoFit();
                    workSheet.Column(8).AutoFit();
                    workSheet.Column(9).AutoFit();
                    workSheet.Column(10).AutoFit();
                    workSheet.Column(11).AutoFit();
                    workSheet.Column(12).AutoFit();

                    var path = $"Attachments\\{CompanyName}\\TaskProgress";
                    var savedPath = Path.Combine(_host.WebRootPath, path);
                    if (File.Exists(savedPath))
                        File.Delete(savedPath);

                    // Create excel file on physical disk  
                    Directory.CreateDirectory(savedPath);
                    //FileStream objFileStrm = File.Create(savedPath);
                    //objFileStrm.Close();
                    var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                    var excelPath = savedPath + $"\\TaskProgress_{date}.xlsx";
                    excel.SaveAs(excelPath);
                    // Write content to excel file  
                    //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                    //Close Excel package 
                    excel.Dispose();
                    Response.Data.SavedPath = Globals.baseURL + '\\' + path + $"\\TaskProgress_{date}.xlsx";
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err107";
                    error.ErrorMSG = "No Tasks Progress to be Downloaded";
                    Response.Errors.Add(error);
                    return Response;
                }
            }
            Response.Data.CurrencyName = Project.Currency?.Name;
            return Response;
        }
        public BaseResponseWithId<long> UpdateTaskProgress([FromBody] UpdateTaskProgressDto request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if (request.progressId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Invalid Data,Progress Id Is Required";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.progressrate < 0 || request.progressrate > 100)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Progress rate should be between 0 and 100";
                Response.Errors.Add(error);
                return Response;
            }
            var progress = _unitOfWork.WorkingHoursTrackings.GetById(request.progressId);
            if (progress == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Progress not found";
                Response.Errors.Add(error);
                return Response;
            }
            var taskId = progress.TaskId;
            if (request.progressrate > 0)
            {
                var LastProgressBefore = _unitOfWork.WorkingHoursTrackings.FindAll(x => x.TaskId == taskId && x.ProgressRate != null && x.CreationDate < progress.CreationDate, skip: null, take: null, orderBy: a => a.CreationDate, orderByDirection: ApplicationConsts.OrderByDescending).ToList().FirstOrDefault();

                var FirstProgressAfter = _unitOfWork.WorkingHoursTrackings.FindAll(x => x.TaskId == taskId && x.ProgressRate != null && x.CreationDate > progress.CreationDate, skip: null, take: null, orderBy: a => a.CreationDate, orderByDirection: ApplicationConsts.OrderByAscending).ToList().FirstOrDefault();

                if (LastProgressBefore?.ProgressRate > request.progressrate || request.progressrate > FirstProgressAfter?.ProgressRate)
                {
                    if (LastProgressBefore != null && FirstProgressAfter != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err104";
                        error.ErrorMSG = $"Progress Rate Should be more than Or Equal {LastProgressBefore.ProgressRate} and less than Or Equal {FirstProgressAfter.ProgressRate}";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                progress.ProgressRate = request.progressrate;
                progress.ProgressNote = request.progressnote;
            }
            if (request.CheckOut != null)
            {
                var checkin = new DateTime(progress.Date.Year, progress.Date.Month, progress.Date.Day, progress.CheckInTime?.Hour ?? 0, progress.CheckInTime?.Minute ?? 0, progress.CheckInTime?.Second ?? 0);
                progress.CheckOutTime = TimeOnly.FromDateTime((DateTime)request.CheckOut);
                progress.TotalHours = (decimal)(((DateTime)request.CheckOut - checkin).TotalMinutes / 60);
            }
            var updatedProgress = _unitOfWork.WorkingHoursTrackings.Update(progress);
            var res = _unitOfWork.Complete();
            if (updatedProgress.CheckOutTime != null)
            {
                UpdateAttendanceByTask(updatedProgress.HrUserId, updatedProgress.Date, creator);
            }
            if (res > 0) // Update Task Requirments Status
            {
                if (request.TaskRequirmentsIds != null && request.TaskRequirmentsIds.Count() > 0)
                {
                    var TaskRequirementsList = _unitOfWork.TaskRequirements.FindAll((x => x.TaskId == taskId));
                    if (TaskRequirementsList.Count() > 0) // Update Status with finished
                    {
                        foreach (var TaskReq in TaskRequirementsList)
                        {
                            if (request.TaskRequirmentsIds.Contains(TaskReq.Id))
                            {
                                TaskReq.IsFinished = true;
                                TaskReq.WorkingHourTrackingId = request.progressId;
                            }
                            else
                            {
                                if (request.progressId == TaskReq.WorkingHourTrackingId)
                                {
                                    TaskReq.IsFinished = false;
                                }
                            }

                            _unitOfWork.TaskRequirements.Update(TaskReq);
                        }
                        _unitOfWork.Complete();
                    }
                }
            }
            Response.ID = updatedProgress.Id;
            return Response;
        }
        public BaseResponseWithId<long> UpdateAttendanceByTask(long UserId, DateTime date, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var WorkingTrackings = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.HrUserId == UserId && a.Date.Date == date.Date && a.TaskId != null, includes: new[] { "Shift" }).OrderBy(a => a.Date);
            if (WorkingTrackings != null && WorkingTrackings.Count() > 0)
            {
                var sumAttendance = _unitOfWork.Attendances.FindAll(a => a.HrUserId == UserId && a.AttendanceDate == DateOnly.FromDateTime(date), includes: new[] { "HrUser" }).FirstOrDefault();

                if (sumAttendance != null)
                {
                    sumAttendance.TaskHours = WorkingTrackings.Select(a => a.TotalHours).Sum();
                    _unitOfWork.Attendances.Update(sumAttendance);
                    _unitOfWork.Complete();
                }

            }
            SumAttendanceForPayroll(UserId, creator, date);
            return Response;


        }
        public BaseResponseWithData<List<GetProgressForAllTasksDto>> GetWorkingHoursForTask(long TaskId, long? HrUserId)
        {
            BaseResponseWithData<List<GetProgressForAllTasksDto>> Response = new BaseResponseWithData<List<GetProgressForAllTasksDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = [];

            var workingHours = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.TaskId == TaskId && (HrUserId != null ? a.HrUserId == HrUserId : true), includes: new[] { "HrUser", "Task", "Project.SalesOffer" }).ToList();

            var list = workingHours.Select(a => new GetProgressForAllTasksDto
            {
                projectName = a.Project?.SalesOffer?.ProjectName,
                TaskName = a.Task?.Name,
                HrUserId = a.HrUserId,
                UserName = a.HrUser.FirstName + " " + a.HrUser.LastName,
                Date = a.Date.ToShortDateString(),
                ProgressNote = a.ProgressNote,
                ProgressPercent = (decimal)a.ProgressRate,
                TotalHours = a.TotalHours,
                CheckIn = a.CheckInTime != null ? (TimeOnly)a.CheckInTime : new TimeOnly(0, 0, 0),
                CheckOut = a.CheckOutTime != null ? (TimeOnly)a.CheckOutTime : new TimeOnly(0, 0, 0),
                Id = a.Id
            }).ToList();

            Response.Data = list;

            return Response;
        }
        public BaseResponseWithData<List<GetOpenWorkingHoursForAllTasksDto>> GetOpenWorkingHoursForAllTasks(long HrUserId)
        {
            BaseResponseWithData<List<GetOpenWorkingHoursForAllTasksDto>> Response = new BaseResponseWithData<List<GetOpenWorkingHoursForAllTasksDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = [];

            var workingHours = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.HrUserId == HrUserId && (a.CheckOutTime == null && a.CheckInTime != null) && a.TaskId != null, includes: new[] { "HrUser", "Task", "Project.SalesOffer" }).ToList();

            var list = workingHours.Select(a => new GetOpenWorkingHoursForAllTasksDto
            {
                Id = a.Id,
                TaskId = a.TaskId,
                TaskName = a.Task?.Name,
                CheckIn = (TimeOnly)a.CheckInTime,
                Date = a.Date.ToShortDateString(),
                LastResponseTime = a.TaskValidateDate != null ? TimeOnly.FromDateTime((DateTime)a.TaskValidateDate).ToString() : "",
            }).ToList();
            Response.Data = list;

            return Response;
        }
        public BaseResponseWithId<long> SumAttendanceForPayroll(long UserId, long creator, DateTime date)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var HrUser = _unitOfWork.HrUsers.FindAll(a => a.Id == UserId, includes: new[] { "Branch.BranchSettings" }).FirstOrDefault();
            if (HrUser == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Hr User is not found";
                Response.Errors.Add(error);
                return Response;
            }
            if (HrUser.Branch == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Branch is not found";
                Response.Errors.Add(error);
                return Response;
            }
            if (HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom == null && HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollTo == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Branch should have Payroll from and to";
                Response.Errors.Add(error);
                return Response;
            }
            DateOnly fromDate = new DateOnly(); // default in last month
            DateOnly ToDate = new DateOnly();
            if (date.Day >= (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom)
            {
                fromDate = new DateOnly(date.Year, date.Month, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom); // default in last month
                ToDate = new DateOnly(date.Year, date.Month, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollTo).AddMonths(1);
            }
            else
            {
                fromDate = new DateOnly(date.Year, date.Month, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom).AddMonths(-1);
                ToDate = new DateOnly(date.Year, date.Month, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollTo);
            }


            int diffrence = (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom - (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom;
            if (diffrence >= 15) // the same month
            {
                fromDate = new DateOnly(date.Year, date.Month, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom); // in the same month
            }


            var Attendances = _unitOfWork.Attendances.FindAll(a => a.HrUserId == UserId && a.AttendanceDate >= fromDate && a.AttendanceDate <= ToDate).OrderBy(a => a.AttendanceDate);

            if (Attendances != null && Attendances.Count() > 0)
            {
                var check = true;
                var sumPayroll = _unitOfWork.Payrolls.FindAll(a => a.HrUserId == UserId && a.From == fromDate && a.To == ToDate, includes: new[] { "HrUser" }).FirstOrDefault();
                if (sumPayroll == null)
                {
                    sumPayroll = new Payroll();
                    sumPayroll.From = fromDate;
                    sumPayroll.To = ToDate;
                    sumPayroll.IsPaid = false;
                    check = false;
                }
                //var summary = MonthAttendaceSummaryForHrUser(UserId,date.Month, DateTime.Now.Year).Data;
                sumPayroll.HrUserId = UserId;
                sumPayroll.BranchId = HrUser.BranchId;
                sumPayroll.TotalWorkingDays = Attendances.Where(a => a.DayTypeId == 3 || (a.DayTypeId != 3 && a.NoHours > 0 || a.NoMin > 0)).Count();
                sumPayroll.TotalWorkingHours = (decimal)(Attendances.Select(a => a.NoHours).Sum() + (Attendances.Select(a => a.NoMin).Sum() / (decimal)60));
                sumPayroll.TotalOvertimeHours = (decimal)(Attendances.Select(a => a.OverTimeHour).Sum() + (Attendances.Select(a => a.OverTimeMin).Sum() / (decimal)60));
                sumPayroll.TotalDelayHours = (decimal)(Attendances.Select(a => a.DelayHours).Sum() + (Attendances.Select(a => a.DelayMin).Sum() / (decimal)60));
                sumPayroll.HolidayHours = (decimal)(Attendances.Sum(a => a.HolidayHours ?? 0));
                sumPayroll.VacationHours = (decimal)(Attendances.Sum(a => a.VacationHours ?? 0));
                sumPayroll.OvertimeCost = Math.Round((decimal)Attendances.Select(a => a.OvertimeCost).Sum(), 2);
                sumPayroll.DeductionCost = (decimal)Attendances.Select(a => a.DeductionCost).Sum();
                sumPayroll.TotalAbsenceDays = Attendances.Where(a => a.DayTypeId == 3 && a.NoHours == 0 && a.NoMin == 0).Count();
                sumPayroll.VacationDaysNumber = Attendances.Where(a => a.AttendanceDate.Month == fromDate.Month && a.AttendanceDate.Year == fromDate.Month && a.IsApprovedAbsence == true && a.HrUserId == UserId).Count();
                if (check)
                {
                    /*sumPayroll.ModifiedBy = creator;
                    sumPayroll.ModifiedDate = DateTime.Now;*/
                    sumPayroll.Date = DateOnly.FromDateTime(date);
                    var updatedsumPayroll = _unitOfWork.Payrolls.Update(sumPayroll);
                    _unitOfWork.Complete();
                    Response.ID = sumPayroll.Id;
                }
                else
                {
                    sumPayroll.Date = DateOnly.FromDateTime(DateTime.Now);
                    sumPayroll.CreatedBy = creator;
                    sumPayroll.ModifiedBy = creator;
                    sumPayroll.CreationDate = DateTime.Now;
                    sumPayroll.ModifiedDate = DateTime.Now;
                    var addedsumPayroll = _unitOfWork.Payrolls.Add(sumPayroll);
                    _unitOfWork.Complete();
                    Response.ID = addedsumPayroll.Id;
                }
            }

            return Response;
        }
        public BaseResponseWithData<List<GetMonthAttendaceSummaryForHrUserDto>> MonthAttendaceSummaryForHrUser(long HrUserID, int month, int year)
        {
            BaseResponseWithData<List<GetMonthAttendaceSummaryForHrUserDto>> response = new BaseResponseWithData<List<GetMonthAttendaceSummaryForHrUserDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            var hrUser = _unitOfWork.HrUsers.GetById(HrUserID);
            if (hrUser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No HrUser with thid ID";
                response.Errors.Add(error);
                return response;
            }
            if (month < 0 || month > 12)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Inavlid Value For Month";
                response.Errors.Add(error);
                return response;
            }
            if (year > DateTime.MaxValue.Year || year < DateTime.MinValue.Year)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Inavlid Value For Year";
                response.Errors.Add(error);
                return response;
            }

            var Date = new DateTime(year, month, 1);
            var UserAttendancesList = _unitOfWork.Attendances.FindAll(a => a.HrUserId == HrUserID, new[] { "ApprovedByUser" });
            var dayTypesList = _unitOfWork.DayTypes.GetAll();
            var absenceTypeList = _unitOfWork.ContractLeaveSetting.GetAll();
            var attendanceList = new List<GetMonthAttendaceSummaryForHrUserDto>();

            while (Date.Month == month)
            {
                var attandenceDay = new GetMonthAttendaceSummaryForHrUserDto();
                var attendance = UserAttendancesList.Where(a => a.AttendanceDate == DateOnly.FromDateTime(Date)).FirstOrDefault();
                //var date = DateOnly.FromDateTime(Date);               //test if there is problems convert to string
                var DayName = DateOnly.FromDateTime(Date).DayOfWeek.ToString();
                attandenceDay.DayDate = DayName + " " + Date.ToShortDateString();

                #region declare vars for temp data
                var totalHours = 0;
                int totalMinutes = 0;

                int OverTimeHour = 0;
                int OverTimeMin = 0;

                int DelayHours = 0;
                int DelayMins = 0;
                #endregion

                if (attendance != null)
                {
                    //----------------------------save data to temp vars--------------------------------------------
                    if (attendance.NoHours != null)
                    {
                        totalHours = (int)attendance.NoHours;
                    }
                    if (attendance.NoMin != null) totalMinutes = (int)attendance.NoMin;

                    if (attendance.OverTimeHour != null) OverTimeHour = (int)attendance.OverTimeHour;
                    if (attendance.OverTimeMin != null) OverTimeMin = (int)attendance.OverTimeMin;

                    if (attendance.DelayHours != null) DelayHours = (int)attendance.DelayHours;
                    if (attendance.DelayMin != null) DelayMins = (int)attendance.DelayMin;
                    //----------------------------------------------------------------------------------------------

                    //----------------------------------add data to obj---------------------------------------------
                    attandenceDay.TotalHours = totalHours.ToString() + " H" + " " + totalMinutes.ToString() + " M";
                    attandenceDay.OverTime = OverTimeHour.ToString() + " H" + " " + OverTimeMin.ToString() + " M";
                    attandenceDay.Delay = DelayHours.ToString() + " H" + " " + DelayMins.ToString() + " M";
                    var hoursnum = (double)(totalHours + ((double)totalMinutes / 60));
                    attandenceDay.CheckOutDate = attendance.CheckOutHour == null ? "-" : attendance.CheckOutDate != null ? attendance.CheckOutDate?.ToString("yyyy-MM-dd") :
                        attendance.AttendanceDate.ToDateTime(new TimeOnly(attendance.CheckInHour ?? 0, attendance.CheckInMin ?? 0, 0)).AddHours((double)((attendance.NoHours ?? 0) + ((double)(attendance.NoMin ?? 0) / (double)60))).ToString("yyyy-MM-dd");
                    attandenceDay.HrUserId = HrUserID;
                    attandenceDay.From = (attendance.IsApprovedAbsence == true || attendance.CheckInHour == null) ? "" : new TimeOnly(attendance.CheckInHour ?? 0, attendance.CheckInMin ?? 0, 0).ToString();
                    attandenceDay.To = (attendance.IsApprovedAbsence == true || (attendance.HolidayHours > 0 && attendance.CheckInHour == null)) ? "" : attendance.CheckOutHour != null ? new TimeOnly(attendance.CheckOutHour ?? 0, attendance.CheckOutMin ?? 0, 0).ToString() : "Pending";
                    attandenceDay.HolidayHours = attendance.HolidayHours != null ? (int)attendance.HolidayHours + " H " + (int)((attendance.HolidayHours - (int)attendance.HolidayHours) * 60) + " M" : "0 H 0 M";
                    attandenceDay.VacationHours = attendance.VacationHours != null ? (int)attendance.VacationHours + " H " + (int)((attendance.VacationHours - (int)attendance.VacationHours) * 60) + " M" : "0 H 0 M";
                    attandenceDay.Longitude = attendance.Longitude;
                    attandenceDay.Latitude = attendance.Latitude;

                    if (attendance.AbsenceTypeId != null)
                    {
                        var absenceTypeName = absenceTypeList.Where(a => a.Id == attendance.AbsenceTypeId).FirstOrDefault().HolidayName;
                        if (attendance.ApprovedByUser != null)
                        {
                            attandenceDay.AbsenceDayApprovedById = attendance.ApprovedByUser.Id;
                            attandenceDay.AbsenceDayApprovedByName = attendance.ApprovedByUser.FirstName + attendance.ApprovedByUser.MiddleName + attendance.ApprovedByUser.LastName;
                        }

                    }
                    else
                    {
                        if (attendance.DayTypeId != null) attandenceDay.DayTypeID = (int)attendance.DayTypeId;
                        var dayTypeName = dayTypesList.Where(a => a.Id == attendance.DayTypeId).FirstOrDefault();
                        //if (dayTypeName != null) attandenceDay.DayTypeName = dayTypeName.DayType1;
                    }
                    var dayName = GetDayTypeIdbyKnowingTheDate(Date, hrUser.BranchId ?? 0)[1];
                    attandenceDay.DayTypeName = attendance.IsApprovedAbsence == true ? $"Leave, Approved By {attendance.ApprovedByUser.FirstName + ' ' + attendance.ApprovedByUser.LastName} in {attendance.ModificationDate}" : attendance.CheckInHour == null && attendance.CheckInMin == null && attendance.DayTypeId != 2 ? "Absence" : attendance.DayType.DayType1.ToString();

                }
                else
                {
                    attandenceDay.HrUserId = HrUserID;
                    attandenceDay.From = "";
                    attandenceDay.To = "";
                    attandenceDay.TotalHours = "0 H 0 M";
                    attandenceDay.OverTime = "0 H 0 M";
                    attandenceDay.Delay = "0 H 0 M";
                    var dayName = GetDayTypeIdbyKnowingTheDate(Date, hrUser.BranchId ?? 0)[1];
                    attandenceDay.DayTypeName = dayName == 3 ? "Absent" : dayTypesList.Where(a => a.Id == dayName).FirstOrDefault().DayType1;

                }
                //AttendanceDay.DayIs = _unitOfWork.DayTypes.FindAll(a => a.Id == daytypeId).FirstOrDefault().DayType1;
                attendanceList.Add(attandenceDay);
                Date = Date.AddDays(1);
            }
            response.Data = attendanceList;
            return response;
        }

        public BaseResponseWithData<GetHeadersOfAttendaceSummaryForHrUserDto> GetHeadersOfAttendaceSummaryForHrUser(int BranchId, int Month, int year, long? HrUserID = null)
        {
            BaseResponseWithData<GetHeadersOfAttendaceSummaryForHrUserDto> response = new BaseResponseWithData<GetHeadersOfAttendaceSummaryForHrUserDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region check in DB
            var hrUser = _unitOfWork.Branches.GetById(BranchId);
            if (hrUser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Branch with this ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                var days = CalculateDayTypesOfMonth(Month, BranchId);


                //var date
                var headersData = new GetHeadersOfAttendaceSummaryForHrUserDto();

                var attendances = _unitOfWork.Attendances.FindAll(a => a.AttendanceDate.Month == Month && a.AttendanceDate.Year == year && a.HrUserId == HrUserID);

                headersData.WorkingDay = days[0];
                headersData.DayOff = days[2];
                headersData.Vacation = days[1];
                headersData.PersonalVacation = attendances.Where(a => a.IsApprovedAbsence == true).Count();
                headersData.UserWorkingDay = attendances.Where(a => a.CheckInHour != null && a.DayTypeId == 3).Count();



                headersData.UserAbsentDays = headersData.WorkingDay >= (headersData.UserWorkingDay + headersData.PersonalVacation) ? (headersData.WorkingDay - (headersData.UserWorkingDay + headersData.PersonalVacation)) : 0;

                int iMonthNo = Month;
                DateTime dtDate = new DateTime(2000, iMonthNo, 1);
                string MonthFullName = dtDate.ToString("MMMM");
                headersData.Month = MonthFullName;
                headersData.Year = year;
                headersData.DaysOFMonth = days.Sum();

                response.Data = headersData;
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

        public BaseResponseWithData<List<GetProgressForTaskDto>> GetProgressByUser(long HrUserID, int month, int year, int day)
        {
            BaseResponseWithData<List<GetProgressForTaskDto>> response = new BaseResponseWithData<List<GetProgressForTaskDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            var hrUser = _unitOfWork.HrUsers.GetById(HrUserID);
            if (hrUser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No HrUser with thid ID";
                response.Errors.Add(error);
                return response;
            }
            if (month < 0 || month > 12)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Inavlid Value For Month";
                response.Errors.Add(error);
                return response;
            }
            if (day == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Day is mendatory";
                response.Errors.Add(error);
                return response;
            }
            if (year > DateTime.MaxValue.Year || year < DateTime.MinValue.Year)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Inavlid Value For Year";
                response.Errors.Add(error);
                return response;
            }
            try
            {
                var querydate = new DateTime(year, month, day);
                var workingHourTrackingList = _unitOfWork.WorkingHoursTrackings.FindAll((a => a.HrUserId == HrUserID));
                var ProgressForTaskList = new List<GetProgressForTaskDto>();



                var workingHourTracking = workingHourTrackingList.Where(a => a.Date >= querydate && a.Date < querydate.AddDays(1));

                if (workingHourTracking != null)
                {
                    foreach (var tracking in workingHourTracking)
                    {
                        var ProgressForTask = new GetProgressForTaskDto();
                        ProgressForTask.WorkingHoursId = tracking.Id;
                        ProgressForTask.ProgressRate = tracking.ProgressRate ?? 0;
                        ProgressForTask.Note = (tracking.ProgressNote ?? "").ToString();
                        ProgressForTask.TaskId = tracking.TaskId != null ? tracking.TaskId ?? 0 : 0;
                        ProgressForTask.ProjectID = tracking.ProjectId != null ? tracking.ProjectId ?? 0 : null;
                        ProgressForTask.Date = querydate.Date.ToString();
                        ProgressForTask.CheckInTime = tracking.CheckInTime.ToString();
                        ProgressForTask.CheckOutTime = tracking.CheckOutTime.ToString();
                        var date = DateOnly.FromDateTime(querydate.Date);
                        ProgressForTask.CheckOutDate = tracking.CheckOutDate != null ? tracking.CheckOutDate?.ToString("yyyy-MM-dd") : tracking.CheckInTime != null ? date.ToDateTime((TimeOnly)tracking.CheckInTime).AddHours((double)tracking.TotalHours).ToString("yyyy-MM-dd") : null;
                        if (tracking.TotalHours < 0)
                        {
                            ProgressForTask.CheckInTime = workingHourTracking.Where(a => a.CheckOutTime != null).OrderBy(a => a.Id).LastOrDefault().CheckOutTime.Value.AddHours((double)tracking.TotalHours).ToString();
                            ProgressForTask.CheckOutTime = workingHourTracking.Where(a => a.CheckOutTime != null).OrderBy(a => a.Id).LastOrDefault().CheckOutTime.Value.ToString();
                        }
                        ProgressForTask.TotalHours = tracking.TotalHours;
                        ProgressForTask.Longitude = tracking.Longitude;
                        ProgressForTask.Latitude = tracking.Latitude;
                        ProgressForTaskList.Add(ProgressForTask);
                    }
                }


                response.Data = ProgressForTaskList;
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

        public BaseResponseWithData<GetAttendanceByDayModel> GetAttendanceByDay(DateTime date, int? branchId, int? DepartmentId)
        {
            BaseResponseWithData<GetAttendanceByDayModel> Response = new BaseResponseWithData<GetAttendanceByDayModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new GetAttendanceByDayModel();
            var finalList = new List<AttendanceByDay>();
            if (date < DateTime.MinValue || date > DateTime.MaxValue)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invalid Data for A Date";
                Response.Errors.Add(error);
                return Response;
            }
            var users = _unitOfWork.HrUsers.FindAll(a => a.Active == true, includes: new[] { "JobTitle", "Department", "Team" }).ToList();
            if (branchId != null)
            {
                var branch = _unitOfWork.Branches.GetById((int)branchId);
                if (branch == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Branch Not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
                users = users.Where(a => a.BranchId == branchId).ToList();
            }
            if (DepartmentId != null)
            {
                var department = _unitOfWork.Departments.GetById((int)DepartmentId);
                if (department == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "Department Not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
                users = users.Where(a => a.DepartmentId == DepartmentId).ToList();
            }
            var userIds = users.Select(a => a.Id).ToList();
            Response.Data.UsersNumber = users.Count;
            Response.Data.Date = date.ToShortDateString();

            var AttendanceList = _unitOfWork.Attendances.FindAll(a => a.AttendanceDate == DateOnly.FromDateTime(date), includes: new[] { "HrUser.Branch", "HrUser.Team", "HrUser.JobTitle", "ApprovedByUser", "DayType" });


            foreach (var user in users)
            {
                var attenadance = AttendanceList.Where(a => a.HrUserId == user.Id).FirstOrDefault();
                if (attenadance != null)
                {
                    finalList.Add(new AttendanceByDay()
                    {
                        Id = attenadance.Id,
                        HrUserId = user.Id,
                        Name = user.FirstName + " " + user.LastName,
                        DepartmentName = user.Department?.Name,
                        TeamName = user.Team?.Name,
                        JobtitleName = user.JobTitle?.Name,
                        Date = attenadance.AttendanceDate.ToString(),
                        From = (attenadance.IsApprovedAbsence == true || attenadance.CheckInHour == null) ? "-" : new TimeOnly(attenadance.CheckInHour ?? 0, attenadance.CheckInMin ?? 0, 0).ToString(),
                        To = (attenadance.IsApprovedAbsence == true || (attenadance.HolidayHours > 0 && attenadance.CheckInHour == null)) ? "-" : attenadance.CheckOutHour != null ? new TimeOnly(attenadance.CheckOutHour ?? 0, attenadance.CheckOutMin ?? 0, 0).ToString() : "Current",
                        TotalHours = (attenadance.NoHours ?? 0) + " H " + (attenadance.NoMin ?? 0) + " M",
                        OverTimeHours = (attenadance.OverTimeHour ?? 0) + " H " + (attenadance.OverTimeMin ?? 0) + " M",
                        DelayHours = (attenadance.DelayHours ?? 0) + " H " + (attenadance.DelayMin ?? 0) + " M",
                        DayType = attenadance.IsApprovedAbsence == true ? $"Leave, Approved By {attenadance.ApprovedByUser.FirstName + ' ' + attenadance.ApprovedByUser.LastName} in {attenadance.ModificationDate}" : attenadance.CheckInHour == null && attenadance.CheckInMin == null && attenadance.DayTypeId != 2 ? "Absence" : attenadance.DayType.DayType1.ToString(),
                        HrUserImg = user.ImgPath != null ? Globals.baseURL + "/" + user.ImgPath : "",
                        DayTypeId = attenadance.DayTypeId,
                        HoursNum = Math.Round((decimal)((attenadance.NoHours ?? 0) + ((decimal)(attenadance.NoMin ?? 0) / (decimal)60)), 2),
                        CheckOutDate = attenadance.CheckOutHour == null ? "-" : attenadance.CheckOutDate != null ? attenadance.CheckOutDate?.ToString("yyyy-MM-dd") :
                        attenadance.AttendanceDate.ToDateTime(new TimeOnly(attenadance.CheckInHour ?? 0, attenadance.CheckInMin ?? 0, 0)).AddHours((double)((attenadance.NoHours ?? 0) + ((double)(attenadance.NoMin ?? 0) / (double)60))).ToString("yyyy-MM-dd"),
                        HolidayHours = attenadance.HolidayHours != null ? (int)attenadance.HolidayHours + " H " + (int)((attenadance.HolidayHours - (int)attenadance.HolidayHours) * 60) + " M" : "0 H 0 M",
                        VacationHours = attenadance.VacationHours != null ? (int)attenadance.VacationHours + " H " + (int)((attenadance.VacationHours - (int)attenadance.VacationHours) * 60) + " M" : "0 H 0 M",
                        Longitude = attenadance.Longitude,
                        Latitude = attenadance.Latitude
                    });
                }
                else
                {
                    var daytype = _unitOfWork.DayTypes.GetById(GetDayTypeIdbyKnowingTheDate(date, branchId ?? 0)[1])?.DayType1.ToString();
                    finalList.Add(new AttendanceByDay()
                    {
                        Id = null,
                        HrUserId = user.Id,
                        Name = user.FirstName + " " + user.LastName,
                        DepartmentName = user.Department?.Name,
                        TeamName = user.Team?.Name,
                        JobtitleName = user.JobTitle?.Name,
                        Date = DateOnly.FromDateTime(date).ToString(),
                        From = "-",
                        To = "-",
                        TotalHours = "0 H 0 M",
                        OverTimeHours = "0 H 0 M",
                        DelayHours = "0 H 0 M",
                        DayType = daytype != "Holiday" && daytype != "Weekend" ? "Absent" : daytype,
                        HrUserImg = user.ImgPath != null ? Globals.baseURL + "/" + user.ImgPath : "",
                        HoursNum = 0,
                        HolidayHours = "0 H 0 M",
                        VacationHours = "0 H 0 M"
                    });
                }
            }
            var above12 = finalList.Where(a => a.HoursNum > 12).OrderByDescending(a => a.HoursNum).ToList();
            var byAlphabetic = finalList.Where(a => a.HoursNum <= 12).OrderBy(a => a.Name).ToList();
            above12.AddRange(byAlphabetic);
            Response.Data.AttendanceList = above12;
            return Response;
        }

        public BaseResponseWithData<UpdateTaskWorkingHoursResponse> UpdateTaskWorkingHours(long ProgressId, bool validate, long creator)
        {
            BaseResponseWithData<UpdateTaskWorkingHoursResponse> Response = new BaseResponseWithData<UpdateTaskWorkingHoursResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new UpdateTaskWorkingHoursResponse();
            if (ProgressId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "progress Id Is Invalid";
                Response.Errors.Add(error);
                return Response;
            }
            var Progress = _unitOfWork.WorkingHoursTrackings.GetById(ProgressId);
            if (Progress == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Progress Is not Found";
                Response.Errors.Add(error);
                return Response;
            }
            if (!validate)
            {
                if (Progress.TaskValidateDate != null)
                {
                    var interval = (DateTime.Now - (DateTime)Progress.TaskValidateDate).TotalMinutes;
                    if (interval > 20)
                    {

                        Response.Data.IsNewProgress = true;
                        Response.Data.TimeInterval = (double)interval / (double)60;
                    }
                    else
                    {
                        Progress.TaskValidateDate = DateTime.Now;
                        _unitOfWork.WorkingHoursTrackings.Update(Progress);
                        _unitOfWork.Complete();
                        Response.Data.IsNewProgress = false;
                        Response.Data.TimeInterval = 0;
                    }
                }


            }
            else
            {
                Progress.TaskValidateDate = DateTime.Now;
                _unitOfWork.WorkingHoursTrackings.Update(Progress);
                _unitOfWork.Complete();
                Response.Data.IsNewProgress = false;
                Response.Data.TimeInterval = 0;
            }
            return Response;
        }
        public BaseResponseWithData<PayRollDataModel> GeneratePayrollPdfForUser(string monthYear, long HrUserId)
        {
            BaseResponseWithData<PayRollDataModel> Response = new BaseResponseWithData<PayRollDataModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var data = new PayRollDataModel();

            var HrUser = _unitOfWork.HrUsers.FindAll(a => a.Id == HrUserId, includes: new[] { "Branch.BranchSettings" }).FirstOrDefault();
            if (HrUser == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Hr User is not found";
                Response.Errors.Add(error);
                return Response;
            }
            if (HrUser.Branch == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Branch is not found";
                Response.Errors.Add(error);
                return Response;
            }
            if (HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom == null && HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollTo == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Branch Should Have Payroll From and To";
                Response.Errors.Add(error);
                return Response;
            }
            int month = 0;
            int year = 0;
            var dateFilter = DateTime.Now;
            if (DateTime.TryParseExact(monthYear, "yyyy-M", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFilter))
            {
                month = dateFilter.Month;
                year = dateFilter.Year;
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Invalid Date Filter";
                Response.Errors.Add(error);
                return Response;
            }


            DateOnly start = new DateOnly(dateFilter.Year, dateFilter.Month, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom); // default in last month
            DateOnly end = new DateOnly(dateFilter.Year, dateFilter.Month, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollTo).AddMonths(1);
            int diffrence = (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom - (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollTo;
            if (diffrence >= 15) // the same month
            {
                start = new DateOnly(dateFilter.Year, dateFilter.Month, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom); // in the same month
            }

            var sumPayroll = _unitOfWork.Payrolls.FindAll(a => a.HrUserId == HrUserId && a.From == start && a.To == end && a.IsPaid == false, includes: new[] { "HrUser.JobTitle", "HrUser.Department", "HrUser.ContractDetails.Salaries", "HrUser.ContractDetails.Salaries.SalaryAllownces", "HrUser.ContractDetails.Salaries.SalaryDeductionTaxes", "HrUser.ContractDetails.Salaries.Currency", "HrUser.Branch.OverTimeAndDeductionRates", "HrUser.ContractDetails.ContractLeaveEmployees.ContractLeaveSetting", "HrUser.ContractDetails.Salaries.PaymentMethod", "HrUser.ContractDetails.Salaries.PaymentStrategy" }).FirstOrDefault();

            if (sumPayroll != null)
            {

                var overTimeRate = sumPayroll.HrUser.Branch.OverTimeAndDeductionRates.Where(a => a.Rate > 0).Select(a => a.Rate).FirstOrDefault();
                var deductionRate = sumPayroll.HrUser.Branch.OverTimeAndDeductionRates.Where(a => a.Rate < 0).Select(a => a.Rate).FirstOrDefault();
                var contract = sumPayroll.HrUser.ContractDetails.Where(a => a.IsCurrent).FirstOrDefault();
                var leavetypes = contract?.ContractLeaveEmployees;
                var Client = _unitOfWork.Clients.FindAll(a => a.OwnerCoProfile == true).FirstOrDefault();
                data.CompanyName = Client?.Name;
                data.CompanyPhone = Client?.ClientMobiles.FirstOrDefault()?.Mobile;
                data.CompanyEmail = Client?.Email;
                data.UserName = sumPayroll.HrUser.FirstName + " " + sumPayroll.HrUser.MiddleName + " " + sumPayroll.HrUser.LastName;
                data.UserEmail = sumPayroll.HrUser.Email;
                data.DepartmentName = sumPayroll.HrUser.Department?.Name;
                data.JobtitleName = sumPayroll.HrUser.JobTitle?.Name;
                data.Date = DateTime.Now.ToShortDateString();
                data.FromDate = start.ToShortDateString();
                data.ToDate = end.ToShortDateString();
                data.WorkingHours = sumPayroll.TotalWorkingHours - sumPayroll.TotalOvertimeHours;
                data.WorkingHourRate = ((contract?.Salaries?.Where(a => a.IsCurrent == true).FirstOrDefault()?.TotalGross ?? 0) * 12) / (52 * 40);
                data.BasicRateTotal = data.WorkingHourRate * data.WorkingHours;
                data.WorkingDays = sumPayroll.TotalWorkingDays;
                data.OverTimeHours = sumPayroll.TotalOvertimeHours;
                data.DelayHours = sumPayroll.TotalDelayHours;
                data.OverTimeRate = overTimeRate * data.WorkingHourRate;
                data.DeductionRate = deductionRate * data.WorkingHourRate;
                data.OverTimeTotal = data.OverTimeRate * data.OverTimeHours;
                data.BranchName = HrUser.Branch.Name;
                data.DeductionTotal = Math.Abs(data.DeductionRate * data.DelayHours);
                data.SalaryDetais = _mapper.Map<GetSalaryDto>(contract?.Salaries.Where(a => a.IsCurrent == true).FirstOrDefault());
                data.SalaryAllownces = _mapper.Map<List<EditSalaryAllownce>>(contract?.Salaries?.FirstOrDefault()?.SalaryAllownces);
                data.Taxes = contract?.Salaries?.FirstOrDefault()?.SalaryDeductionTaxes?.Select(a => new PayslipTaxModel { Amount = a.Amount, TaxName = a.TaxName }).ToList();
                data.TotalDeductions = data.Taxes?.Select(a => a.Amount).Sum() ?? 0;
                data.TotalNetAmount = data.TotalGrossAmount - data.TotalDeductions;
                data.CurrencyName = contract?.Salaries?.FirstOrDefault()?.Currency.Name;
                data.Status = sumPayroll.IsPaid == true ? "Paid " + sumPayroll.ModifiedDate.ToShortDateString() : "not paid";
                data.LeaveTypes = leavetypes?.Select(a => new PayslipLeaveModel() { LeaveName = a.ContractLeaveSetting.HolidayName, Balance = a.BalancePerMonth ?? 0, Used = a.Used ?? 0, Remain = a.Remain ?? 0 }).ToList();
                data.Paymethod = contract?.Salaries?.FirstOrDefault()?.PaymentMethod?.Name;
                var daysnumbers = CalculateDayTypesOfMonth(dateFilter.Month, HrUser.BranchId ?? 0);
                data.WorkingDaysNum = daysnumbers[0];
                data.HoliyDaysNum = daysnumbers[1];
                data.WeekEndDaysNum = daysnumbers[2];
                data.HolidayHours = sumPayroll.HolidayHours ?? 0;
                data.HoliDaysHoursTotal = (sumPayroll.HolidayHours ?? 0) * data.WorkingHourRate;
                data.HolidayHourRate = data.WorkingHourRate;
                data.VacationHours = sumPayroll.VacationHours ?? 0;
                data.VacationHoursTotal = (sumPayroll.VacationHours ?? 0) * data.WorkingHourRate;
                data.VacationHourRate = data.WorkingHourRate;
                data.VacationDaysNum = _unitOfWork.Attendances.FindAll(a => a.AttendanceDate.Month == start.Month && a.AttendanceDate.Year == start.Year && a.IsApprovedAbsence == true && a.HrUserId == HrUserId).Count();
                data.TotalGrossAmount = data.BasicRateTotal + data.OverTimeTotal - data.DeductionTotal + data.HoliDaysHoursTotal + data.VacationHoursTotal;
                data.TotalNetAmount = data.TotalGrossAmount - data.TotalDeductions;
            }
            //else
            //{
            //    Response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err103";
            //    error.ErrorMSG = "This user Doesn't Have Payroll Record";
            //    Response.Errors.Add(error);
            //    return Response;
            //}

            Response.Data = data;
            return Response;
        }

        public BaseResponseWithId<long> UpdatePayRollStatus(bool AllUsers, List<long> usersList, int branchId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var users = _unitOfWork.HrUsers.GetAll().ToList();

            if (!AllUsers && usersList.Count > 0)
            {
                users = users.Where(a => usersList.Contains(a.Id)).ToList();
            }
            if (!AllUsers && branchId != 0)
            {
                users = users.Where(a => a.BranchId == branchId).ToList();
            }

            var usersIds = users.Select(a => a.Id).ToList();

            var payrolls = _unitOfWork.Payrolls.FindAll(a => usersIds.Contains(a.HrUserId)).ToList();

            payrolls = payrolls.Select(a => { a.IsPaid = true; a.ModifiedDate = DateTime.Now; return a; }).ToList();

            payrolls.ForEach(a => { _unitOfWork.Payrolls.Update(a); _unitOfWork.Complete(); });

            return Response;

        }

        public BaseResponseWithId<long> AddAttendance(AddAttendanceModel request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            if (request == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Inavlid Data";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.date > DateTime.Now)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "You can't Insert Attendance In the Future";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.CheckOut <= request.Checkin)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Check out can't be less than or equal the Check in time";
                Response.Errors.Add(error);
                return Response;
            }
            var hruser = _unitOfWork.HrUsers.FindAll(a => a.Id == request.HrUserId, includes: new[] { "Branch", "ContractDetails" }).FirstOrDefault();
            if (hruser == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Hr User is not found";
                Response.Errors.Add(error);
                return Response;
            }
            var department = _unitOfWork.Departments.GetById(hruser.DepartmentId ?? 0) ?? _unitOfWork.Departments.GetAll().FirstOrDefault();
            if (department == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "no department found";
                Response.Errors.Add(error);
                return Response;
            }
            var checkAttendance = _unitOfWork.Attendances.FindAll(a => a.HrUserId == hruser.Id && a.AttendanceDate == DateOnly.FromDateTime(request.date)).FirstOrDefault();
            if (checkAttendance != null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = $"User {hruser.FirstName + ' ' + hruser.LastName} Already Have an Attendance Record";
                Response.Errors.Add(error);
                return Response;
            }
            var contract = hruser.ContractDetails.Where(a => a.IsCurrent).FirstOrDefault();
            if (contract == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err105";
                error.ErrorMSG = "This UserDoesn't have contract";
                Response.Errors.Add(error);
                return Response;
            }
            var data = GetDayTypeIdbyKnowingTheDate(request.Checkin.Date, hruser.BranchId ?? 0);
            var weekdayId = data[0];
            var daytypeId = data[1];
            var shift = _unitOfWork.BranchSchedules.FindAll(a => TimeOnly.FromDateTime(request.Checkin) >= a.From && TimeOnly.FromDateTime(request.Checkin) <= a.To && a.WeekDayId == weekdayId).FirstOrDefault();
            decimal shiftHours = contract?.WorkingHours != null ? (decimal)contract?.WorkingHours : shift != null ? (decimal)(((TimeOnly)shift.To - (TimeOnly)shift.From).TotalMinutes / 60) : 0;
            var Attendance = new Attendance();
            Attendance.HrUserId = hruser.Id;
            Attendance.DepartmentId = department.Id;
            Attendance.DayTypeId = daytypeId;
            Attendance.CheckInHour = request.Checkin.Hour;
            Attendance.CheckInMin = request.Checkin.Minute;
            Attendance.CheckOutHour = request.CheckOut.Hour;
            Attendance.CheckOutMin = request.CheckOut.Minute;
            var period = request.CheckOut.Subtract(request.Checkin);
            Attendance.NoHours = period.Hours;
            Attendance.BranchId = hruser.BranchId;
            var minDifference = period.Minutes;
            Attendance.NoMin = minDifference;
            if (minDifference < 0)
            {
                Attendance.NoHours--;
                Attendance.NoMin = -minDifference;
            }
            var totalhours = Attendance.NoHours + (Attendance.NoMin / 60);
            var overtimeHours = totalhours > shiftHours ? totalhours - shiftHours : 0;
            var delayHours = shiftHours > totalhours ? shiftHours - totalhours : 0;
            var overtimeRate = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => a.BranchId == hruser.BranchId && a.Rate > 0).Select(a => a.Rate).FirstOrDefault();
            var delayRate = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => a.BranchId == hruser.BranchId && a.Rate < 0).Select(a => a.Rate).FirstOrDefault();
            if (daytypeId == 2)
            {
                var vacation = _unitOfWork.VacationDays.FindAll(a => DateOnly.FromDateTime(request.Checkin) >= DateOnly.FromDateTime(a.From) && DateOnly.FromDateTime(request.Checkin) <= DateOnly.FromDateTime(a.To) && a.BranchId == hruser.BranchId).FirstOrDefault();
                if (vacation != null)
                {
                    overtimeRate = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.VacationDayId == vacation.Id && a.Rate > 0).Select(a => a.Rate).FirstOrDefault();
                    delayRate = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.VacationDayId == vacation.Id && a.Rate < 0).Select(a => a.Rate).FirstOrDefault();
                    Attendance.HolidayHours = shiftHours;
                    overtimeHours = (decimal)period.TotalHours;
                }
            }
            Attendance.OverTimeHour = (int)overtimeHours;
            Attendance.DelayHours = (int)delayHours;
            Attendance.OverTimeMin = (int)(((decimal)overtimeHours - Attendance.OverTimeHour) * 60);
            Attendance.DelayMin = (int)(((decimal)delayHours - Attendance.DelayHours) * 60);
            Attendance.AttendanceDate = DateOnly.FromDateTime(request.Checkin);
            Attendance.OvertimeCost = overtimeHours * overtimeRate;
            Attendance.DeductionCost = delayHours * delayRate;
            Attendance.ShiftHours = shiftHours;
            Attendance.CreatedBy = creator;
            Attendance.CheckOutDate = request.CheckOut.Date;
            Attendance.CreationDate = DateOnly.FromDateTime(DateTime.Now);
            Attendance.ModifiedBy = creator;
            Attendance.ModificationDate = DateOnly.FromDateTime(DateTime.Now);
            var AddedAttendance = _unitOfWork.Attendances.Add(Attendance);
            _unitOfWork.Complete();
            Response.ID = AddedAttendance.Id;
            SumAttendanceForPayroll(request.HrUserId, creator, request.Checkin);
            return Response;

        }

        public BaseResponseWithId<long> UpdateAttendance(AddAttendanceModel request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            if (request == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Inavlid Data";
                Response.Errors.Add(error);
                return Response;
            }
            var Attendance = _unitOfWork.Attendances.FindAll(a => a.Id == (long)request.Id, includes: new[] { "HrUser" }).FirstOrDefault();
            if (Attendance == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Attendance not found";
                Response.Errors.Add(error);
                return Response;
            }
            var hruser = Attendance.HrUser;
            Attendance.CheckInHour = request.Checkin.Hour;
            Attendance.CheckInMin = request.Checkin.Minute;
            Attendance.CheckOutHour = request.CheckOut.Hour;
            Attendance.CheckOutMin = request.CheckOut.Minute;
            var period = request.CheckOut.Subtract(request.Checkin);
            Attendance.NoHours = period.Hours;
            var minDifference = period.Minutes;
            Attendance.NoMin = minDifference;
            if (minDifference < 0)
            {
                Attendance.NoHours--;
                Attendance.NoMin = -minDifference;
            }
            var totalhours = Attendance.NoHours + (Attendance.NoMin / 60);
            var overtimeHours = totalhours > Attendance.ShiftHours ? totalhours - Attendance.ShiftHours : 0;
            var delayHours = Attendance.ShiftHours > totalhours ? Attendance.ShiftHours - totalhours : 0;
            var overtimeRate = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => a.BranchId == hruser.BranchId && a.Rate > 0).Select(a => a.Rate).FirstOrDefault();
            var delayRate = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => a.BranchId == hruser.BranchId && a.Rate < 0).Select(a => a.Rate).FirstOrDefault();
            if (Attendance.DayTypeId == 2)
            {
                var vacation = _unitOfWork.VacationDays.FindAll(a => DateOnly.FromDateTime(request.Checkin) >= DateOnly.FromDateTime(a.From) && DateOnly.FromDateTime(request.Checkin) <= DateOnly.FromDateTime(a.To) && a.BranchId == hruser.BranchId).FirstOrDefault();
                overtimeRate = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.VacationDayId == vacation.Id && a.Rate > 0).Select(a => a.Rate).FirstOrDefault();
                delayRate = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.VacationDayId == vacation.Id && a.Rate < 0).Select(a => a.Rate).FirstOrDefault();
                overtimeHours = (decimal)period.TotalHours;
            }
            Attendance.OverTimeHour = (int)overtimeHours;
            Attendance.DelayHours = (int)delayHours;
            Attendance.OverTimeMin = (int)(((decimal)overtimeHours - Attendance.OverTimeHour) * 60);
            Attendance.DelayMin = (int)(((decimal)delayHours - Attendance.DelayHours) * 60);
            Attendance.OvertimeCost = overtimeHours * overtimeRate;
            Attendance.DeductionCost = delayHours * delayRate;
            Attendance.CheckOutDate = request.CheckOut.Date;
            Attendance.ModifiedBy = creator;
            Attendance.ModificationDate = DateOnly.FromDateTime(DateTime.Now);
            var UpdatedAttendance = _unitOfWork.Attendances.Update(Attendance);
            _unitOfWork.Complete();
            Response.ID = UpdatedAttendance.Id;
            SumAttendanceForPayroll(hruser.Id, creator, request.Checkin);
            return Response;

        }

        //var HrUser = _unitOfWork.HrUsers.FindAll(a => a.Id == HrUserId, includes: new[] { "Branch.BranchSettings" }).FirstOrDefault();
        //if (HrUser == null)
        //{
        //    Response.Result = false;
        //    Error error = new Error();
        //    error.ErrorCode = "Err101";
        //    error.ErrorMSG = "Hr User is not found";
        //    Response.Errors.Add(error);
        //    return Response;
        //}
        //if (HrUser.Branch == null)
        //{
        //    Response.Result = false;
        //    Error error = new Error();
        //    error.ErrorCode = "Err102";
        //    error.ErrorMSG = "Branch is not found";
        //    Response.Errors.Add(error);
        //    return Response;
        //}
        //    if (HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom == null && HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollTo == null)
        //    {
        //        Response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err103";
        //        error.ErrorMSG = "Branch Should Have Payroll From and To";
        //        Response.Errors.Add(error);
        //        return Response;
        //    }
        //    var datenow = DateTime.Now;
        //    DateOnly start = new DateOnly(datenow.Year, datenow.Month - 1, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom); // default in last month
        //    DateOnly end = new DateOnly(datenow.Year, datenow.Month, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollTo);
        //    int diffrence = (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom - (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollTo;
        //    if (diffrence >= 15) // the same month
        //    {
        //        start = new DateOnly(datenow.Year, datenow.Month, (int)HrUser.Branch.BranchSettings.FirstOrDefault()?.PayrollFrom); // in the same month
        //    }

        //    var sumPayroll = _unitOfWork.Payrolls.FindAll(a => a.HrUserId == HrUserId && a.Date >= start && a.Date < end, includes: new[] { "HrUser.JobTitle", "HrUser.Department", "HrUser.ContractDetails.Salaries", "HrUser.ContractDetails.Salaries.SalaryAllownces", "HrUser.ContractDetails.Salaries.SalaryDeductionTaxes" }).FirstOrDefault();

        //    if (sumPayroll == null)
        //    {
        //        Response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err103";
        //        error.ErrorMSG = "This user Doesn't Have Payroll Record";
        //        Response.Errors.Add(error);
        //        return Response;
        //    }
        //    var contract = sumPayroll.HrUser.ContractDetails.Where(a => a.IsCurrent).FirstOrDefault();
        //    var Client = _unitOfWork.Clients.FindAll(a => a.OwnerCoProfile == true).FirstOrDefault();
        //    data.CompanyName = Client?.Name;
        //    data.CompanyPhone = Client?.ClientMobiles.FirstOrDefault()?.Mobile;
        //    data.CompanyEmail = Client?.Email;
        //    data.UserName = sumPayroll.HrUser.FirstName + " " + sumPayroll.HrUser.MiddleName + " " + sumPayroll.HrUser.LastName;
        //    data.UserEmail = sumPayroll.HrUser.Email;
        //    data.DepartmentName = sumPayroll.HrUser.Department?.Name;
        //    data.JobtitleName = sumPayroll.HrUser.JobTitle.Name;
        //    data.Date = DateTime.Now.ToShortDateString();
        //    data.FromDate = start.ToShortDateString();
        //    data.ToDate = end.ToShortDateString();
        //    data.AbsenceDays = sumPayroll.TotalAbsenceDays;
        //    data.WorkingDays = sumPayroll.TotalWorkingDays;
        //    data.OverTimeHours = sumPayroll.TotalOvertimeHours;
        //    data.DelayHours = sumPayroll.TotalDelayHours;
        //    data.SalaryDetais = _mapper.Map<GetSalaryDto>(contract.Salaries.FirstOrDefault());
        //    data.SalaryAllownces = _mapper.Map<List<EditSalaryAllownce>>(contract.Salaries.FirstOrDefault().SalaryAllownces);
        //    data.Taxes = _mapper.Map<List<GetSalaryTaxDto>>(contract.Salaries.FirstOrDefault().SalaryDeductionTaxes);

        //// Montholy Payroll Report

        public BaseResponseWithData<GetMonthlyPayrollReport> GetMonthlyPayrollReport(GetMonthlyPayrollReportFilters filters, string companyname)
        {
            BaseResponseWithData<GetMonthlyPayrollReport> response = new BaseResponseWithData<GetMonthlyPayrollReport>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            if (filters.Month < 0 || filters.Month > 12)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Inavlid Value For Month";
                response.Errors.Add(error);
                return response;
            }
            if (filters.Year > DateTime.MaxValue.Year || filters.Year < DateTime.MinValue.Year)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Inavlid Value For Year";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                var activeHruserNumber = _unitOfWork.HrUsers.FindAll(a => a.Active == true && a.BranchId == filters.BranchID).Count();
                var branchSettings = _unitOfWork.BranchSetting.FindAll(a => a.BranchId == filters.BranchID).FirstOrDefault();
                //var paymentMethods = _unitOfWork.PaymentMethods.GetAll();
                if (branchSettings?.PayrollFrom == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This Branch has No PayRoll from";
                    response.Errors.Add(error);
                    return response;
                }
                if (branchSettings.PayrollTo == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This Branch has No PayRoll To";
                    response.Errors.Add(error);
                    return response;
                }

                response.Data = new GetMonthlyPayrollReport();
                #region query from filters
                Expression<Func<HrUser, bool>> criteria = (a => true);

                criteria = a =>
                (
                (filters.BranchID != null ? a.BranchId == filters.BranchID : true) &&
                (filters.DepartmentID != null ? a.DepartmentId == filters.DepartmentID : true) &&
                (filters.HrUserID != null ? a.Id == filters.HrUserID : true)
                );

                var hrUserIds = new List<long>();                           //list <long> for all user IDs
                hrUserIds.AddRange(_unitOfWork.HrUsers.FindAll(criteria).Select(a => a.Id));


                if (filters.PaymentMethodID != null)
                {
                    var hrUserIdsForPaymentMethods = _unitOfWork.Salaries.FindAll(a => a.PaymentMethodId == filters.PaymentMethodID).Select(a => a.HrUserId ?? 0).ToList();        //to get all ids of HrUsers that have paymentMethod with this ID
                    if (hrUserIdsForPaymentMethods.Count() > 0)
                    {
                        hrUserIds = hrUserIds.Intersect(hrUserIdsForPaymentMethods).ToList();
                    }
                    else
                    {
                        hrUserIds = hrUserIdsForPaymentMethods;
                    }
                }
                #endregion


                //-------------------------payroll table-------------------------------------
                var startDate = new DateOnly(filters.Year, filters.Month, branchSettings.PayrollFrom ?? 0);
                var endDate = new DateOnly(filters.Year, filters.Month, branchSettings.PayrollTo ?? 0).AddMonths(1);

                Expression<Func<Payroll, bool>> payrollCriteria = (a => true);

                payrollCriteria = a =>
                (
                (a.Date >= startDate && a.Date <= endDate) &&
                (hrUserIds.Contains(a.HrUserId))
                );

                var payrollList = _unitOfWork.Payrolls.FindAll(payrollCriteria, new[] { "HrUser", "HrUser.Department", "HrUser.Team", "HrUser.JobTitle", "HrUser.Salaries", "HrUser.Salaries.Contract", "HrUser.Salaries.PaymentMethod", "HrUser.ContractDetails", "HrUser.Branch.OverTimeAndDeductionRates" }).ToList();

                var payrollResponseList = new List<GetMonthlyPayrollReportDto>();

                for (int i = 0; i < payrollList.Count; i++)
                {
                    var a = payrollList[i];
                    var contract = a.HrUser.ContractDetails.Where(x => x.IsCurrent).FirstOrDefault();
                    var WorkingHourRate = ((contract?.Salaries?.Where(a => a.IsCurrent == true).FirstOrDefault()?.TotalGross ?? 0) * 12) / (52 * 40);
                    var BasicRateTotal = WorkingHourRate * (a.TotalWorkingHours - a.TotalOvertimeHours);
                    var overTimeRate = a.HrUser.Branch.OverTimeAndDeductionRates.Where(a => a.Rate > 0).Select(a => a.Rate).FirstOrDefault();
                    var OverTimeRate = overTimeRate * WorkingHourRate;
                    var OverTimeTotal = OverTimeRate * a.TotalOvertimeHours;
                    var deductionRate = a.HrUser.Branch.OverTimeAndDeductionRates.Where(a => a.Rate < 0).Select(a => a.Rate).FirstOrDefault();
                    var DeductionRate = deductionRate * WorkingHourRate;
                    var DeductionTotal = Math.Abs(DeductionRate * a.TotalDelayHours);
                    var HoliDaysHoursTotal = (a.HolidayHours ?? 0) * WorkingHourRate;
                    var VacationHoursTotal = (a.VacationHours ?? 0) * WorkingHourRate;
                    var payrollResponse = new GetMonthlyPayrollReportDto();
                    var Taxes = contract?.Salaries.Where(a => a.IsCurrent == true)?.FirstOrDefault()?.SalaryDeductionTaxes?.Select(a => new PayslipTaxModel { Amount = a.Amount, TaxName = a.TaxName }).ToList();
                    var TotalDeductions = Taxes?.Select(a => a.Amount).Sum() ?? 0;
                    payrollResponse.PayrollID = a.Id;
                    payrollResponse.HrUserID = a.HrUserId;
                    payrollResponse.UserFullName = $"{a.HrUser?.FirstName} {a.HrUser?.MiddleName} {a.HrUser?.LastName}".Trim();
                    payrollResponse.DepartmentID = a.HrUser?.DepartmentId ?? 0;
                    payrollResponse.DepartmentName = a.HrUser?.Department?.Name;
                    payrollResponse.TeamID = a.HrUser?.TeamId ?? 0;
                    payrollResponse.TeamName = a.HrUser?.Team?.Name;
                    payrollResponse.JobtitleID = a.HrUser?.JobTitleId ?? 0;
                    payrollResponse.JobTitleName = a.HrUser?.JobTitle?.Name;
                    payrollResponse.TotalWorkingdays = a.TotalWorkingDays;
                    payrollResponse.TotalOverTimeHours = a.TotalOvertimeHours; // This will always be 0. Is this intentional?
                    payrollResponse.TotalWorkingHours = a.TotalWorkingHours - a.TotalOvertimeHours;
                    payrollResponse.TotalDelayHours = a.TotalDelayHours;
                    payrollResponse.VacationDaysNumber = a.VacationDaysNumber;
                    payrollResponse.PaidVacationHours = a.VacationHours ?? 0;
                    payrollResponse.HolidayHours = a.HolidayHours ?? 0;
                    payrollResponse.TotalAbsenceDays = a.TotalAbsenceDays;
                    payrollResponse.Allowances = a.Allowances;
                    payrollResponse.Tax = a.Taxs;
                    payrollResponse.Insurance = a.Insurance;
                    payrollResponse.IsPaid = a.IsPaid;
                    payrollResponse.OtherDeductions = a.OtherDeductions;
                    payrollResponse.OtherIncome = a.OtherIncome;
                    payrollResponse.PaymentMethodID = a.HrUser?.Salaries.Where(s => s.ContractId == a.HrUser.ContractDetails.FirstOrDefault(c => c.IsCurrent)?.Id).FirstOrDefault()?.PaymentMethodId;
                    payrollResponse.PaymentMethodName = a.HrUser?.Salaries.Where(s => s.ContractId == a.HrUser.ContractDetails.FirstOrDefault(c => c.IsCurrent)?.Id).Select(s => s.PaymentMethod?.Name).FirstOrDefault();
                    payrollResponse.TotalGross = BasicRateTotal + OverTimeTotal - DeductionTotal + HoliDaysHoursTotal + VacationHoursTotal;
                    payrollResponse.TotalNet = payrollResponse.TotalGross - TotalDeductions;



                    payrollResponseList.Add(payrollResponse);
                }
                //data.BasicRateTotal + data.OverTimeTotal - data.DeductionTotal + data.HoliDaysHoursTotal + data.VacationHoursTotal;
                response.Data.Year = filters.Year;
                response.Data.Month = filters.Month;
                response.Data.PaymentMethodID = filters.PaymentMethodID;
                response.Data.BranchID = filters.BranchID;
                response.Data.DepartmentID = filters.DepartmentID;
                response.Data.TotalNumberOfActiveUsers = activeHruserNumber;
                response.Data.NumberOfHrUser = payrollResponseList.Count();
                response.Data.PayrollMonthList = payrollResponseList;

                var days = CalculateDayTypesOfMonth(filters.Month, filters.BranchID);
                response.Data.WorkingDays = days[0];
                response.Data.DaysOff = days[2];
                response.Data.Vacation = days[1];
                response.Data.DaysOFMonth = days.Sum();

                if (filters.DownloadExcel)
                {
                    ExcelPackage excel = new ExcelPackage();
                    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                    workSheet.TabColor = System.Drawing.Color.Black;
                    workSheet.DefaultRowHeight = 12;
                    workSheet.Row(1).Height = 20;
                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(1).Style.Font.Bold = true;
                    workSheet.Cells[1, 1].Value = "Name";
                    workSheet.Cells[1, 2].Value = "Department, Team, Job Title";
                    workSheet.Cells[1, 3].Value = "Working Days";
                    workSheet.Cells[1, 4].Value = "Total Working Hours";
                    workSheet.Cells[1, 5].Value = "Overtime Hours";
                    workSheet.Cells[1, 6].Value = "Deduction Hours";
                    workSheet.Cells[1, 7].Value = "Paid Vacation Hours";
                    workSheet.Cells[1, 8].Value = "Holiday Hours";
                    workSheet.Cells[1, 9].Value = "Vacations, Absence";
                    workSheet.Cells[1, 10].Value = "Allowance";
                    workSheet.Cells[1, 11].Value = "Tax";
                    workSheet.Cells[1, 12].Value = "Insurance";
                    workSheet.Cells[1, 13].Value = "Other Deductions";
                    workSheet.Cells[1, 14].Value = "Other Income";
                    workSheet.Cells[1, 15].Value = "Payment Method";
                    workSheet.Cells[1, 16].Value = "Total Gross";
                    workSheet.Cells[1, 17].Value = "Total Net";
                    workSheet.Cells[1, 18].Value = "Is Paid";

                    int recordIndex = 2;
                    foreach (var payroll in payrollResponseList)
                    {
                        workSheet.Cells[recordIndex, 1].Value = payroll.UserFullName;
                        workSheet.Cells[recordIndex, 2].Value = (payroll.DepartmentName ?? "") + ", " + (payroll.TeamName ?? "") + ", " + (payroll.JobTitleName ?? "");
                        workSheet.Cells[recordIndex, 3].Style.Numberformat.Format = "0.00";
                        workSheet.Cells[recordIndex, 3].Value = payroll.TotalWorkingdays;
                        workSheet.Cells[recordIndex, 4].Style.Numberformat.Format = "0.00";
                        workSheet.Cells[recordIndex, 4].Value = payroll.TotalWorkingHours;
                        workSheet.Cells[recordIndex, 5].Style.Numberformat.Format = "0.00";
                        workSheet.Cells[recordIndex, 5].Value = payroll.TotalOverTimeHours;
                        workSheet.Cells[recordIndex, 6].Style.Numberformat.Format = "0.00";
                        workSheet.Cells[recordIndex, 6].Value = payroll.TotalDelayHours;
                        workSheet.Cells[recordIndex, 7].Style.Numberformat.Format = "0.00";
                        workSheet.Cells[recordIndex, 7].Value = payroll.PaidVacationHours;
                        workSheet.Cells[recordIndex, 8].Style.Numberformat.Format = "0.00";
                        workSheet.Cells[recordIndex, 8].Value = payroll.HolidayHours;
                        workSheet.Cells[recordIndex, 9].Value = payroll.VacationDaysNumber + ", " + payroll.TotalAbsenceDays;
                        workSheet.Cells[recordIndex, 10].Style.Numberformat.Format = "#,##0.00";
                        workSheet.Cells[recordIndex, 10].Value = payroll.Allowances;
                        workSheet.Cells[recordIndex, 11].Style.Numberformat.Format = "#,##0.00";
                        workSheet.Cells[recordIndex, 11].Value = payroll.Tax;
                        workSheet.Cells[recordIndex, 12].Style.Numberformat.Format = "#,##0.00";
                        workSheet.Cells[recordIndex, 12].Value = payroll.Insurance;
                        workSheet.Cells[recordIndex, 13].Style.Numberformat.Format = "#,##0.00";
                        workSheet.Cells[recordIndex, 13].Value = payroll.OtherDeductions;
                        workSheet.Cells[recordIndex, 14].Style.Numberformat.Format = "#,##0.00";
                        workSheet.Cells[recordIndex, 14].Value = payroll.OtherIncome;
                        workSheet.Cells[recordIndex, 15].Value = payroll.PaymentMethodName ?? "";
                        workSheet.Cells[recordIndex, 16].Style.Numberformat.Format = "#,##0.00";
                        workSheet.Cells[recordIndex, 16].Value = payroll.TotalGross;
                        workSheet.Cells[recordIndex, 17].Value = payroll.TotalNet;
                        workSheet.Cells[recordIndex, 17].Style.Numberformat.Format = "#,##0.00";
                        workSheet.Cells[recordIndex, 18].Value = payroll.IsPaid == true ? "Yes" : payroll.IsPaid == false ? "No" : "";

                        workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        recordIndex++;
                    }
                    workSheet.Column(1).AutoFit();
                    workSheet.Column(2).AutoFit();
                    workSheet.Column(3).AutoFit();
                    workSheet.Column(4).AutoFit();
                    workSheet.Column(5).AutoFit();
                    workSheet.Column(6).AutoFit();
                    workSheet.Column(7).AutoFit();
                    workSheet.Column(8).AutoFit();
                    workSheet.Column(9).AutoFit();
                    workSheet.Column(10).AutoFit();
                    workSheet.Column(11).AutoFit();
                    workSheet.Column(12).AutoFit();
                    workSheet.Column(13).AutoFit();
                    workSheet.Column(14).AutoFit();
                    workSheet.Column(15).AutoFit();
                    workSheet.Column(16).AutoFit();
                    workSheet.Column(17).AutoFit();

                    var path = $@"Attachments\{companyname}\PayrollReports";
                    var savedPath = Path.Combine(_host.WebRootPath, path);
                    if (File.Exists(savedPath))
                        File.Delete(savedPath);

                    // Create excel file on physical disk  
                    Directory.CreateDirectory(savedPath);
                    //FileStream objFileStrm = File.Create(savedPath);
                    //objFileStrm.Close();
                    var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                    var excelPath = savedPath + $"\\PayrollReport_{date}.xlsx";
                    excel.SaveAs(excelPath);
                    // Write content to excel file  
                    //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                    //Close Excel package 
                    excel.Dispose();
                    response.Data.PayrollReportPath = Globals.baseURL + @"\" + path + $@"\PayrollReport_{date}.xlsx";
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

        public BaseResponseWithData<List<GetPeriodAttendance>> GetPeriodAttendance(long payRollID)
        {
            var response = new BaseResponseWithData<List<GetPeriodAttendance>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check In DB
            var payroll = _unitOfWork.Payrolls.GetById(payRollID);
            if (payroll == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-2";
                err.ErrorMSG = "No payRoll with this ID";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var attendanceList = _unitOfWork.Attendances.FindAll(a => a.AttendanceDate > payroll.From && a.AttendanceDate < payroll.To && a.HrUserId == payroll.HrUserId).ToList();
                var PeriodAttendanceList = new List<GetPeriodAttendance>();
                foreach (var attendance in attendanceList)
                {
                    var PeriodAttendance = new GetPeriodAttendance();
                    #region declare vars for temp data
                    var totalHours = 0;
                    int totalMinutes = 0;

                    int OverTimeHour = 0;
                    int OverTimeMin = 0;

                    int DelayHours = 0;
                    int DelayMins = 0;

                    int checkInHours = 0;
                    int CheckInMinutes = 0;

                    int checkOutHours = 0;
                    int checkOutMinutes = 0;
                    #endregion

                    //----------------------------save data to temp vars--------------------------------------------
                    if (attendance.NoHours != null)
                    {
                        totalHours = (int)attendance.NoHours;
                    }
                    if (attendance.NoMin != null) totalMinutes = (int)attendance.NoMin;

                    if (attendance.OverTimeHour != null) OverTimeHour = (int)attendance.OverTimeHour;
                    if (attendance.OverTimeMin != null) OverTimeMin = (int)attendance.OverTimeMin;

                    if (attendance.DelayHours != null) DelayHours = (int)attendance.DelayHours;
                    if (attendance.DelayMin != null) DelayMins = (int)attendance.DelayMin;

                    if (attendance.CheckInHour != null) checkInHours = (int)attendance.CheckInHour;
                    if (attendance.CheckInMin != null) CheckInMinutes = (int)attendance.CheckInMin;

                    if (attendance.CheckOutHour != null) checkOutHours = (int)attendance.CheckOutHour;
                    if (attendance.CheckOutMin != null) checkOutMinutes = (int)attendance.CheckOutMin;
                    //----------------------------------------------------------------------------------------------

                    //----------------------------------add data to obj---------------------------------------------
                    PeriodAttendance.TotalHours = totalHours.ToString() + "h" + " " + totalMinutes.ToString() + "m";
                    PeriodAttendance.OverTime = OverTimeHour.ToString() + "h" + " " + OverTimeMin.ToString() + "m";
                    PeriodAttendance.Delay = DelayHours.ToString() + "h" + " " + DelayMins.ToString() + "m";
                    PeriodAttendance.checkIn = new TimeOnly(checkInHours, CheckInMinutes, 0).ToString();
                    PeriodAttendance.CheckOut = new TimeOnly(checkOutHours, checkOutMinutes, 0).ToString();

                    PeriodAttendance.ID = attendance.Id;
                    PeriodAttendance.HrUserId = payroll.HrUserId;
                    //-----------------------------------------------------------------------------------------------



                    PeriodAttendanceList.Add(PeriodAttendance);
                }
                response.Data = PeriodAttendanceList;
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

        public BaseResponseWithData<List<GetPeriodAbsenceDto>> GetPeriodAbsence(long payRollID)
        {
            var response = new BaseResponseWithData<List<GetPeriodAbsenceDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check In DB
            var payroll = _unitOfWork.Payrolls.GetById(payRollID);
            if (payroll == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-2";
                err.ErrorMSG = "No payRoll with this ID";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var attendanceList = _unitOfWork.Attendances.FindAll((a => a.AttendanceDate > payroll.From && a.AttendanceDate < payroll.To && a.HrUserId == payroll.HrUserId && a.AbsenceTypeId != null), new[] { "AbsenceType", "ApprovedByUser" }).ToList();
                var PeriodAttendanceList = new List<GetPeriodAbsenceDto>();
                foreach (var attendance in attendanceList)
                {
                    var PeriodAttendance = new GetPeriodAbsenceDto();
                    PeriodAttendance.ID = attendance.Id;
                    PeriodAttendance.HrUserId = attendance.HrUserId ?? 0;
                    PeriodAttendance.AbsenceTypeID = attendance.AbsenceTypeId ?? 0;
                    PeriodAttendance.AbsenceTypeName = attendance?.AbsenceType?.HolidayName;
                    PeriodAttendance.IsApprovedAbsence = attendance.IsApprovedAbsence;
                    PeriodAttendance.ApprovedByUserID = attendance.ApprovedByUserId;
                    PeriodAttendance.ApprovedByUserName = attendance.ApprovedByUser != null ? attendance?.ApprovedByUser?.FirstName + " " + attendance?.ApprovedByUser?.MiddleName + " " + attendance?.ApprovedByUser?.LastName : null;



                    PeriodAttendanceList.Add(PeriodAttendance);
                }
                response.Data = PeriodAttendanceList;
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

        public BaseResponseWithData<string> DownloadAttendanceSheet(bool AllUsers, List<long> usersList, int branchId, long creator, string CompanyName)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var users = _unitOfWork.HrUsers.FindAll(a => true, includes: new[] { "Branch", "Department", "Team" }).ToList();

            if (!AllUsers && usersList.Count > 0)
            {
                users = users.Where(a => usersList.Contains(a.Id)).ToList();
            }
            if (!AllUsers && branchId != 0)
            {
                users = users.Where(a => a.BranchId == branchId).ToList();
            }
            var path = $"Attachments\\{CompanyName}\\Templates\\EmployeeAttendance.xlsx";
            var sheetpath = Path.Combine(_host.WebRootPath, path);
            if (File.Exists(sheetpath))
            {
                ExcelPackage package = new ExcelPackage(new FileInfo(sheetpath));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                var counter = 0;
                var firstrow = 3;
                for (int i = 0; i < users.Count; i++)
                {
                    worksheet.Cells[firstrow + i, 1].Value = i + 1;
                    worksheet.Cells[firstrow + i, 2].Value = users[i].Id;
                    worksheet.Cells[firstrow + i, 3].Value = users[i].FirstName + " " + users[i].LastName;
                    worksheet.Cells[firstrow + i, 4].Value = users[i].Branch?.Name;
                    worksheet.Cells[firstrow + i, 5].Value = users[i].Department?.Name;
                    worksheet.Cells[firstrow + i, 6].Value = users[i].Team?.Name;
                    worksheet.Row(firstrow + i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                worksheet.Column(1).AutoFit();
                worksheet.Column(2).AutoFit();
                worksheet.Column(3).AutoFit();
                worksheet.Column(4).AutoFit();
                worksheet.Column(5).AutoFit();
                worksheet.Column(6).AutoFit();

                var newpath = $"Attachments\\{CompanyName}\\AttendanceSheets";
                var savedPath = Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var excelPath = savedPath + $"\\AttendanceSheet.xlsx";
                package.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                package.Dispose();
                Response.Data = Globals.baseURL + '\\' + newpath + $"\\AttendanceSheet.xlsx";
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Attendance Template Sheet Not Found";
                Response.Errors.Add(error);
                return Response;
            }
            return Response;

        }

        public BaseResponseWithId<long> UploadAttendanceSheet(UploadAttendanceSheetFile AttendanceSheetFile, long creator, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            var AttendanceSheet = AttendanceSheetFile.AttendanceSheet;
            var date = AttendanceSheetFile.date;
            if (date > DateTime.Now)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you can't insert Attendance in future";
                Response.Errors.Add(error);
                return Response;
            }
            if (AttendanceSheet != null && AttendanceSheet.Length != 0)
            {
                using (var stream = new MemoryStream())
                {
                    AttendanceSheet.CopyTo(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows - 2;
                        if (rowCount <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "sheet is empty";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        var backupDepartment = _unitOfWork.Departments.GetAll().FirstOrDefault();
                        for (int i = 0; i < rowCount; i++)
                        {
                            if (worksheet.Cells[i + 3, 2].Value != null)
                            {
                                var hruser = _unitOfWork.HrUsers.Find(a => a.Id == long.Parse(worksheet.Cells[i + 3, 2].Value.ToString()), includes: new[] { "Branch", "Department", "Team" });
                                if (hruser == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err101";
                                    error.ErrorMSG = $"User not found At Index {i + 1}";
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    var checkAttendance = _unitOfWork.Attendances.FindAll(a => a.HrUserId == hruser.Id && a.AttendanceDate == DateOnly.FromDateTime(date)).FirstOrDefault();
                                    if (checkAttendance != null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err102";
                                        error.ErrorMSG = $"User {hruser.FirstName + ' ' + hruser.LastName} Already Have an Attendance Record";
                                        Response.Errors.Add(error);
                                    }
                                    else
                                    {
                                        var data = GetDayTypeIdbyKnowingTheDate(date, hruser.BranchId ?? 0);
                                        var weekdayId = data[0];
                                        var daytypeId = data[1];
                                        /*if (worksheet.Cells[i + 3, 8].Value == null || worksheet.Cells[i + 3, 9].Value == null ||
                                            worksheet.Cells[i + 3, 10].Value == null || worksheet.Cells[i + 3, 10].Value == null)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err103";
                                            error.ErrorMSG = $"the check in time or check out time is wrong for user {hruser.FirstName + ' ' + hruser.LastName}";
                                            Response.Errors.Add(error);
                                        }*/
                                        if (worksheet.Cells[i + 3, 8].Value != null && worksheet.Cells[i + 3, 9].Value != null &&
                                            worksheet.Cells[i + 3, 10].Value != null && worksheet.Cells[i + 3, 10].Value != null)
                                        {
                                            var Checkin = new TimeOnly(int.Parse(worksheet.Cells[i + 3, 7].Value.ToString()), int.Parse(worksheet.Cells[i + 3, 8].Value.ToString()), 0);
                                            var CheckOut = new TimeOnly(int.Parse(worksheet.Cells[i + 3, 9].Value.ToString()), int.Parse(worksheet.Cells[i + 3, 10].Value.ToString()), 0);
                                            if (CheckOut <= Checkin)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err103";
                                                error.ErrorMSG = "check out can't be less than or equal the check in time";
                                                Response.Errors.Add(error);
                                            }
                                            else
                                            {
                                                var shift = _unitOfWork.BranchSchedules.FindAll(a => Checkin >= a.From && Checkin <= a.To && a.WeekDayId == weekdayId).FirstOrDefault();
                                                if (shift == null)
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err104";
                                                    error.ErrorMSG = $"shift not found for user {hruser.FirstName + ' ' + hruser.LastName}";
                                                    Response.Errors.Add(error);
                                                }
                                                else
                                                {
                                                    decimal shiftHours = shift != null ? (decimal)(((TimeOnly)shift.To - (TimeOnly)shift.From).TotalMinutes / 60) : 0;
                                                    var Attendance = new Attendance();
                                                    Attendance.HrUserId = hruser.Id;
                                                    Attendance.DepartmentId = hruser.Department?.Id ?? backupDepartment.Id;
                                                    Attendance.DayTypeId = daytypeId;
                                                    Attendance.CheckInHour = Checkin.Hour;
                                                    Attendance.CheckInMin = Checkin.Minute;
                                                    Attendance.CheckOutHour = CheckOut.Hour;
                                                    Attendance.CheckOutMin = CheckOut.Minute;
                                                    Attendance.NoHours = Attendance.CheckOutHour - Attendance.CheckInHour;
                                                    Attendance.BranchId = hruser.BranchId;
                                                    var minDifference = Attendance.CheckOutMin - Attendance.CheckInMin;
                                                    Attendance.NoMin = minDifference;
                                                    if (minDifference < 0)
                                                    {
                                                        Attendance.NoHours--;
                                                        Attendance.NoMin = -minDifference;
                                                    }
                                                    var totalhours = Attendance.NoHours + (Attendance.NoMin / 60);
                                                    var overtimeHours = totalhours > shiftHours ? totalhours - shiftHours : 0;
                                                    var delayHours = shiftHours > totalhours ? shiftHours - totalhours : 0;
                                                    var overtimeRate = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => a.BranchId == hruser.BranchId && a.Rate > 0).Select(a => a.Rate).FirstOrDefault();
                                                    var delayRate = _unitOfWork.OverTimeAndDeductionRates.FindAll(a => a.BranchId == hruser.BranchId && a.Rate < 0).Select(a => a.Rate).FirstOrDefault();
                                                    if (daytypeId == 2)
                                                    {
                                                        var vacation = _unitOfWork.VacationDays.FindAll(a => date >= a.From && date <= a.To && a.BranchId == hruser.BranchId).FirstOrDefault();
                                                        overtimeRate = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.VacationDayId == vacation.Id && a.Rate > 0).Select(a => a.Rate).FirstOrDefault();
                                                        delayRate = _unitOfWork.VacationOverTimeAndDeductionRates.FindAll(a => a.VacationDayId == vacation.Id && a.Rate < 0).Select(a => a.Rate).FirstOrDefault();
                                                    }
                                                    Attendance.OverTimeHour = (int)overtimeHours;
                                                    Attendance.DelayHours = (int)delayHours;
                                                    Attendance.OverTimeMin = (int)(((decimal)overtimeHours - Attendance.OverTimeHour) * 60);
                                                    Attendance.DelayMin = (int)(((decimal)delayHours - Attendance.DelayHours) * 60);
                                                    Attendance.AttendanceDate = DateOnly.FromDateTime(date);
                                                    Attendance.OvertimeCost = overtimeHours * overtimeRate;
                                                    Attendance.DeductionCost = delayHours * delayRate;
                                                    Attendance.ShiftHours = shiftHours;
                                                    Attendance.CreatedBy = creator;
                                                    Attendance.CreationDate = DateOnly.FromDateTime(DateTime.Now);
                                                    Attendance.ModifiedBy = creator;
                                                    Attendance.ModificationDate = DateOnly.FromDateTime(DateTime.Now);
                                                    if (worksheet.Cells[i + 3, 11].Value != null)
                                                    {
                                                        var absence = _unitOfWork.ContractLeaveSetting.FindAll(a => a.HolidayName.Trim().ToLower() == worksheet.Cells[i + 3, 11].Value.ToString().Trim().ToLower()).FirstOrDefault();
                                                        if (absence != null)
                                                        {
                                                            Attendance.AbsenceTypeId = absence.Id;
                                                        }
                                                        else
                                                        {
                                                            Response.Result = false;
                                                            Error error = new Error();
                                                            error.ErrorCode = "Err105";
                                                            error.ErrorMSG = $"Absence Type not Found for User {hruser.FirstName + ' ' + hruser.LastName}";
                                                            Response.Errors.Add(error);
                                                        }
                                                    }
                                                    var AddedAttendance = _unitOfWork.Attendances.Add(Attendance);
                                                    _unitOfWork.Complete();
                                                    Response.ID = AddedAttendance.Id;
                                                    SumAttendanceForPayroll(hruser.Id, creator, date);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Attendance Sheet Not Found";
                Response.Errors.Add(error);
                return Response;
            }
            return Response;
        }

        public BaseResponseWithId<long> DeleteTaskProgress(long ProgressId, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.ID = 0;

            if (ProgressId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Progress Id Is required";
                Response.Errors.Add(error);
                return Response;
            }
            var progress = _unitOfWork.WorkingHoursTrackings.GetById(ProgressId);
            if (progress == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Progress is not found";
                Response.Errors.Add(error);
                return Response;
            }
            var check = progress.CheckInTime != null && progress.CheckOutTime != null;
            var userid = progress.HrUserId;
            var date = progress.Date;
            var requirement = progress.TaskRequirements.FirstOrDefault();
            if (requirement != null)
            {
                requirement.IsFinished = false;
                _unitOfWork.Complete();
            }
            _unitOfWork.WorkingHoursTrackings.Delete(progress);
            _unitOfWork.Complete();
            if (check)
            {
                SumWorkingTrackingHoursForAttendance(userid, date, creator);
            }

            return Response;
        }

        public BaseResponseWithData<string> GetAttendanceReport(GetAttendanceReportFilters filters, string CompanyName)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>()
            {
                Errors = new List<Error>(),
                Result = true
            };
            try
            {
                var From = filters.From ?? DateOnly.MinValue;
                var To = filters.To ?? DateOnly.MaxValue;
                if (filters.From != null && filters.To == null)
                {
                    From = (DateOnly)filters.From;
                    To = new DateOnly(From.Year, From.Month, 1).AddMonths(1).AddDays(-1);
                }
                else if (filters.From == null && filters.To != null)
                {
                    To = (DateOnly)filters.To;
                    From = new DateOnly(To.Year, To.Month, 1);
                }
                else if (filters.From == null && filters.To == null)
                {
                    From = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);
                    To = From.AddMonths(1).AddDays(-1);
                }
                var Attendance = _unitOfWork.Attendances.FindAllQueryable(a => a.AttendanceDate >= From && a.AttendanceDate <= To, includes: new[] { "HrUser" });
                if (filters.BranchId != null)
                {
                    Attendance = Attendance.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                }
                if (filters.DepartmentId != null)
                {
                    Attendance = Attendance.Where(a => a.DepartmentId == filters.DepartmentId).AsQueryable();
                }
                var AttendanceList = Attendance.ToList();
                if (!AttendanceList.Any())
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Attendance List";
                    response.Errors.Add(error);
                    return response;
                }
                var Grouped = AttendanceList.GroupBy(a => a.HrUserId);
                var package = new ExcelPackage();
                var dt = new System.Data.DataTable("Grid");

                dt.Columns.AddRange(new DataColumn[12] {
                new DataColumn("User Name"),
                new DataColumn("Attendance Days"),
                new DataColumn("Absence Days"),
                new DataColumn("Vacation/Leave Days"),
                new DataColumn("Holidays"),
                new DataColumn("Check In Date"),
                new DataColumn("Check In"),
                new DataColumn("Check Out"),
                new DataColumn("Total Working Hours"),
                new DataColumn("OverTime Hours"),
                new DataColumn("Holiday Hours"),
                new DataColumn("Vacation Hours"),
                });

                foreach (var item in Grouped)
                {
                    var data = GetHeadersOfAttendaceSummaryForHrUser(item.FirstOrDefault().BranchId ?? 0, From.Month, From.Year, item.Key);
                    var data2 = CalculateDayTypesOfMonth(From.Month, item.FirstOrDefault().BranchId ?? 0);
                    var NoHours = (item.Sum(a => a.NoHours) + TimeSpan.FromMinutes((double)item.Sum(a => a.NoMin)).Hours) + " Hours And " + TimeSpan.FromMinutes((double)item.Sum(a => a.NoMin)).Minutes + " Minutes";

                    var OverHours = (item.Sum(a => a.OverTimeHour) + TimeSpan.FromMinutes((double)item.Sum(a => a.OverTimeMin)).Hours) + " Hours And " + TimeSpan.FromMinutes((double)item.Sum(a => a.OverTimeMin)).Minutes + " Minutes";

                    var Holidays = item.Sum(a => a.HolidayHours);

                    var HolidayHours = (int)Holidays + " Hours And " + (int)(Holidays - ((int)Holidays)) + " Minutes";

                    var Vacations = item.Sum(a => a.VacationHours);

                    var VacationHours = (int)Vacations + " Hours And " + (int)(Vacations - ((int)Vacations)) + " Minutes";

                    var checkin = item.FirstOrDefault().CheckInHour!=null? new TimeSpan(item.FirstOrDefault().CheckInHour??0,item.FirstOrDefault().CheckInMin??0,0) : TimeSpan.Zero;
                    DateTime? checkinD = checkin!=TimeSpan.Zero? DateTime.Today.Add(checkin):null;
                    var checkout = item.LastOrDefault().CheckOutHour!=null? new TimeSpan(item.LastOrDefault().CheckOutHour??0,item.LastOrDefault().CheckOutMin??0,0) : TimeSpan.Zero;
                    DateTime? checkoutD =checkout!=TimeSpan.Zero? DateTime.Today.Add(checkout):null;

                    dt.Rows.Add(
                    item.Select(a => a.HrUser.FirstName + " " + a.HrUser.LastName).FirstOrDefault(),
                    data.Data.UserWorkingDay,
                    data.Data.UserAbsentDays,
                    data.Data.PersonalVacation,
                    data2[1],
                    item.FirstOrDefault()?.AttendanceDate.ToString("dd/MM/yyyy"),
                    checkinD!=null?checkinD?.ToString("h:m:s tt"):"Pending",
                    checkoutD!=null?checkoutD?.ToString("h:m:s tt"):"Pending",
                    NoHours,
                    OverHours,
                    HolidayHours,
                    VacationHours
                    );
                }
                
                var workSheet = package.Workbook.Worksheets.Add("AttendanceReport");
                workSheet.DefaultRowHeight = 12;
                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[1, 1, 1, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[1, 1, 1, 12].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 174, 81));
                ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);
                if (From == To)
                {
                    workSheet.Column(2).Hidden = true;
                    workSheet.Column(3).Hidden = true;
                    workSheet.Column(4).Hidden = true;
                    workSheet.Column(5).Hidden = true;
                }
                else if (To > From)
                {
                    workSheet.Column(6).Hidden = true;
                    workSheet.Column(7).Hidden = true;
                    workSheet.Column(8).Hidden = true;
                }
                for (int i = 1; i <= excelRangeBase.Columns; i++)
                {
                    workSheet.Column(i).AutoFit();
                }
                for (int i = 1; i <= excelRangeBase.Rows; i++)
                {
                    workSheet.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                workSheet.View.FreezePanes(2, 1);
                var newpath = $"Attachments\\{CompanyName}\\AttendanceReports";
                var savedPath = System.IO.Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                {
                    File.Delete(savedPath);
                }
                Directory.CreateDirectory(savedPath);
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\AttendanceReport_{date}.xlsx";
                package.SaveAs(excelPath);
                package.Dispose();
                response.Data = Globals.baseURL + '\\' + newpath + $"\\AttendanceReport_{date}.xlsx";
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

        public BaseResponseWithId<long> AddHolidayToBranchAttendance(AddHolidayToBranchAttendanceModel data, long userId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            var vacationDay = _unitOfWork.VacationDays.Find(a => a.Id == data.vacationDayID);
            if (vacationDay == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "No vacation Day with this ID";
                response.Errors.Add(error);
                return response;
            }

            if (vacationDay.IsPaid == false)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "This vacation Day is not paid";
                response.Errors.Add(error);
                return response;
            }

            var branch = _unitOfWork.Branches.GetById(data.branchID);
            if (branch == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "no branch with this ID";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                var hruserList = _unitOfWork.HrUsers.FindAll(a => a.BranchId == data.branchID && a.Active == true, new[] { "ContractDetails" }).ToList();

                var DaysInVacation = vacationDay.To - vacationDay.From;
                var numberOfDaysInVacation = DaysInVacation.Days;

                int counter = 0;
                while (counter <= numberOfDaysInVacation)
                {
                    var day = vacationDay.From.AddDays(counter);
                    var dayType = GetDayTypeIdbyKnowingTheDate(day, data.branchID);

                    if (dayType[1] != 1)
                    {
                        var attendanceList = _unitOfWork.Attendances.FindAll(a => a.AttendanceDate == DateOnly.FromDateTime(day));


                        foreach (var hruser in hruserList)
                        {
                            //var hruser = hruserList.Where(a => a.Id == attendance.HrUserId).FirstOrDefault();
                            var attendance = attendanceList.Where(a => a.HrUserId == hruser.Id).FirstOrDefault();


                            var hruserCurrentContract = hruser.ContractDetails.Where(a => a.IsCurrent == true).FirstOrDefault();
                            if (attendance != null)
                            {
                                if (hruserCurrentContract != null)
                                {
                                    attendance.HolidayHours = hruserCurrentContract.WorkingHours;
                                    attendance.DayTypeId = 2;
                                    _unitOfWork.Complete();
                                    //2 is the ID for holiday in the lookup table
                                    SumAttendanceForPayroll(hruser.Id, userId, day);
                                }

                            }
                            else
                            {
                                var newAttendance = new Attendance();

                                if (hruser.DepartmentId == null)
                                {
                                    var department = _unitOfWork.Departments.GetAll().FirstOrDefault();
                                    newAttendance.DepartmentId = department.Id;
                                }
                                else
                                {
                                    newAttendance.DepartmentId = hruser.DepartmentId ?? 0;

                                }
                                newAttendance.AttendanceDate = DateOnly.FromDateTime(day.Date);
                                newAttendance.CreatedBy = userId;
                                newAttendance.CreationDate = DateOnly.FromDateTime(DateTime.Now);
                                newAttendance.ModifiedBy = userId;
                                newAttendance.ModificationDate = DateOnly.FromDateTime(DateTime.Now);
                                newAttendance.Active = true;
                                newAttendance.HolidayHours = hruserCurrentContract.WorkingHours;
                                newAttendance.HrUserId = hruser.Id;
                                newAttendance.BranchId = data.branchID;
                                newAttendance.DayTypeId = 2;        //2 is the ID for holiday in the lookup table
                                _unitOfWork.Attendances.Add(newAttendance);
                                _unitOfWork.Complete();
                                SumAttendanceForPayroll(hruser.Id, userId, day);
                            }

                        }

                        vacationDay.IsWhApplied = true;
                        _unitOfWork.Complete();
                    }

                    counter++;
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

