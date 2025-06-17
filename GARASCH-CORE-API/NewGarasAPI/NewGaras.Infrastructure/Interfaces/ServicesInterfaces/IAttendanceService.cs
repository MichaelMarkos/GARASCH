using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.Attendance;
using NewGaras.Infrastructure.DTO.TaskProgress;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Attendance;
using NewGaras.Infrastructure.Models.Payroll.Filters;
using NewGaras.Infrastructure.Models.Payroll;
using NewGarasAPI.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.DTO.Attendance.Payroll;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IAttendanceService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithDataAndHeader<List<UserAttendance>> GetHrUserAttendence([FromHeader] GetEmployeeAttendenceHeader header);

        public Task<BaseResponseWithDataAndHeader<List<UserAttendance>>> GetHrUserAttendanceList([FromHeader] GetUserAttendanceListHeader header);

        public BaseResponseWithId<long> AddWorkingHoursTracking([FromForm] AddTrackingByDailyAttendanceDto request,long creator);

        public List<int> GetDayTypeIdbyKnowingTheDate(DateTime day,int branchId);

        public List<int> CalculateDayTypesOfMonth(int month, int branchId);

        public BaseResponseWithId<long> ValidateAttendanceRequest(AddTrackingByDailyAttendanceDto request);

        public CheckOverTimeAndDeduction checkOverTimeAndDeduction(HrUser user,ContractDetail contract ,DateTime date,/*BranchSchedule shift,*/TimeOnly checkin);

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
        public double DegreesToRadians(double degrees);
        public BaseResponseWithId<long> AddWorkingHoursTrackingByTask([FromForm] AddTrackingByDailyTaskDto request, long creator);
        public BaseResponseWithId<long> UpdateWorkingHoursTrackingWithCheckout([FromForm] UpdateWorkingHoursTrackingWithCheckoutDto request,long creator);
        public  BaseResponseWithData<List<GetTaskProgressForUser>> GetTaskProgressForUserList([FromHeader] long TaskId);
        //BaseResponseWithData<List<GetTaskProgressForUser>> GetTaskProgressForUser()
        //public BaseResponseWithId<long> AddWorkingHoursTrackingByEnterLocation([FromForm] AddTrackingByLocationDto request,long creator);
        public BaseResponseWithData<GetTaskProgressForUser> GetTaskProgress([FromHeader] long progressId);
        public BaseResponseWithId<long> SumWorkingTrackingHoursForAttendance(long UserId, DateTime date,long creator);
        public BaseResponseWithData<GetMonthlyAttendanceModel> GetMonthlyAttendance(int month, int year, int branchId, int? departmentId);
        public BaseResponseWithId<long> ApproveTaskPrpgress([FromBody] TaskProgressApprovalDto request,long creator);

        public BaseResponseWithData<RequestAbsenceResponse> RequestAbsence([FromBody] RequestAbsenceDto request,long creator);

        public BaseResponseWithData<GetProgressForAllTasks> GetProgressForAllTask(ProgressForAllTaskFilter filters , string CompanyName);
        public BaseResponseWithId<long> UpdateTaskProgress([FromBody] UpdateTaskProgressDto request,long creator);
        public BaseResponseWithData<GetRequestedAbsenceBalanceModel> GetRequestedAbsenceBalance(long AttendanceId);
        public BaseResponseWithData<ApproveAbsenceResponse> ApproveAbsence(ApproveAbsenceModel request, long creator);

        public BaseResponseWithData<List<GetProgressForAllTasksDto>> GetWorkingHoursForTask(long TaskId,long? HrUserId);

        public BaseResponseWithData<List<GetOpenWorkingHoursForAllTasksDto>> GetOpenWorkingHoursForAllTasks(long HrUserId);
        public BaseResponseWithData<List<GetMonthAttendaceSummaryForHrUserDto>> MonthAttendaceSummaryForHrUser(long HrUserID, int month, int year);
        public BaseResponseWithData<List<GetProgressForTaskDto>> GetProgressByUser(long HrUserID, int month, int year, int day);
        public BaseResponseWithData<GetAttendanceByDayModel> GetAttendanceByDay(DateTime date, int? branchId, int? DepartmentId);

        public BaseResponseWithData<UpdateTaskWorkingHoursResponse> UpdateTaskWorkingHours(long ProgressId, bool validate,long creator);
        public CheckOverTimeAndDeduction checkVacationOverTimeAndDeduction(HrUser user, ContractDetail contract, DateTime date, /*BranchSchedule shift,*/ TimeOnly checkin);

        public BaseResponseWithId<long> SumAttendanceForPayroll(long UserId, long creator,DateTime date);
        public BaseResponseWithData<PayRollDataModel> GeneratePayrollPdfForUser(string monthYear, long HrUserId);
        public BaseResponseWithData<GetMonthlyPayrollReport> GetMonthlyPayrollReport(GetMonthlyPayrollReportFilters filters,string companyname);
        public BaseResponseWithId<long> UpdatePayRollStatus(bool AllUsers, List<long> usersList, int branchId);
        public BaseResponseWithId<long> AddAttendance(AddAttendanceModel request, long creator);
        public BaseResponseWithId<long> UpdateAttendance(AddAttendanceModel request, long creator);
        public BaseResponseWithData<GetHeadersOfAttendaceSummaryForHrUserDto> GetHeadersOfAttendaceSummaryForHrUser(int BranchId, int Month, int year, long? HrUserID = null);
        public BaseResponseWithId<long> UpdateAttendanceByTask(long UserId, DateTime date, long creator);
        public BaseResponseWithData<List<GetPeriodAttendance>> GetPeriodAttendance(long payRollID);
        public BaseResponseWithData<List<GetPeriodAbsenceDto>> GetPeriodAbsence(long payRollID);
        public BaseResponseWithData<string> DownloadAttendanceSheet(bool AllUsers, List<long> usersList, int branchId, long creator, string CompanyName);
        public BaseResponseWithId<long> UploadAttendanceSheet(UploadAttendanceSheetFile AttendanceSheetFile, long creator, string CompanyName);
        public BaseResponseWithId<long> DeleteTaskProgress(long ProgressId, long creator);
        public int CalculateAbsenceDays(DateTime start, DateTime end, int branchId);

        public BaseResponseWithData<string> GetAttendanceReport(GetAttendanceReportFilters filters, string CompanyName);

        public BaseResponseWithId<long> AddDeductionWorkingHours(DeductWorkingHoursModel request);

        public BaseResponseWithId<long> AddHolidayToBranchAttendance(AddHolidayToBranchAttendanceModel data, long userId);

    }
}
