using NewGaras.Domain.Models;
using NewGaras.Domain.Models.TaskManager;
using NewGaras.Infrastructure.DTO.Task;
using NewGaras.Infrastructure.DTO.TaskUnitRateService;
using NewGaras.Infrastructure.DTO.TaskUserReply;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Task;
using NewGaras.Infrastructure.Models.Task.Filters;
using NewGaras.Infrastructure.Models.Task.UsedInResponse;
using NewGarasAPI.Models.TaskManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface ITaskService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithData<TaskWorkFlosGroups> GetTaskGroupedByWorkFlowList(bool NotActive, long ProjectID, long ProjectSprintID, bool IsArchived, long UserID, int CurrentPage, int NumberOfItemsPerPage);
        public BaseResponseWithId<long> AddTaskUnitRateService(AddTaskUnitRateServiceDto Dto, long Creator);
        public BaseResponseWithId<long> EditTaskUnitRateService(EditTaskUnitRateServiceDto Dto, long Editor);
        public BaseResponseWithData<List<GetTaskUnitRateServiceDto>> GetTaskUnitRateServiceList(long TaskId);
        public BaseResponseWithData<GetTaskUnitRateServiceDto> GetTaskUnitRateServiceByID(long TaskUnitRateServiceID);
        public BaseResponseWithData<List<GetInventoryUOMDDL>> GetInventoryUOMDDL();
        public BaseResponseWithData<List<TaskDDL>> GetTaskDDL(long ProjectId);
        public BaseResponseWithData<List<GetTaskUnitRateServiceDto>> GetTaskUnitRateServiceListByProjectID(long ProjectID);
        public BaseResponseWithId<long> EditTaskUserReply(EditTaskUserReplyDto dto, string companyaName);
        public BaseResponseWithId<long> DeleteEditTaskUserReply(long Id);
        public GetTaskUserAndGroupResponse GetTaskDetails(long UserID, [FromHeader] long TaskID = 0);
        public BaseResponseWithId<long> DeleteTask(long TaskId);
        public GetTaskReplyResponse GetTaskReply([FromHeader] GetTaskReplyHeader header);
        public Task<BaseResponseWithId<long>> AddTask(AddTaskObjData request, long UserID, string CompanyName);
        public Task<BaseResponseWithId<long>> AddTaskRequirements(TaskRequirementModel request, long UserID);
        public Task<BaseResponseWithData<List<RequirementModel>>> GetTaskRequirmentsList(long TaskId);
        public Task<BaseResponseWithId<long>> EditTask(AddTaskObjData request, long UserId, string CompanyName);
        public Task<BaseResponseWithId<long>> AddTaskUserReply(GetTaskReplysData request, long UserID, string CompanyName);
        public Task<BaseResponseWithId<long>> AddDeleteTaskUserGroup(AddDeleteTaskUserGroupRequest request);
        public Task<BaseResponseWithId<long>> UpdateTaskFlagsOwnerReciever(GetTaskDetailsData request);
        public Task<GetTaskCategoryDDLResponse> GetTaskCategoryDDL();
        public Task<GetTaskTypeNameResponse> GetTaskTypeNameList();
        //public Task<GetTaskListsByStatusResponse> GetTaskListsByStatus(GetTaskHeader header);
        public BaseResponseWithId<long> DeleteTaskUnitRateService(long Id);
        public BaseResponseWithData<string> GetTasksListReportExcell(GetTasksListReportFilters filters, string CompName);
        public BaseResponseWithData<string> GetTaskProgressReport([FromHeader] long ProjectId, [FromHeader] long? TaskId, [FromHeader] long? InvoiceNumber, [FromHeader] string Type, [FromHeader] string UserName, [FromHeader] DateTime? From, [FromHeader] DateTime? To, string CompanyName);

        /*public Task<BaseResponse> AddTaskBrowserTabs(AddTaskBrowserTabsDtoList dto);
        public Task<BaseResponse> AddTaskApplicationOpen(AddTaskApplicationOpenList dto);
        public Task<BaseResponse> AddTaskScreenShot(AddTaskScreenShotList dto, string CompName);*/
        public Task<BaseResponseWithData<GetTaskBrowserTabsDtoList>> GetTaskBrowserTabs(long? UserID, long? TaskID);
        public Task<BaseResponseWithData<GetTaskAppsOpenList>> GetTaskAppsOpen(long? UserID, long? TaskID);
        public Task<BaseResponseWithData<GetTaskScreenShotDtoList>> GetTaskScreenShot(long? UserID, long? TaskID);
        public GetTaskResponse GetTask([FromHeader] GetTaskHeader header, long userID);

        public Task<BaseResponse> AddTaskMonitor(AddTaskMonitorDtoList dto, string companyName);

        public Task<BaseResponseWithData<GetTaskMonitorByUser>> GetTaskMonitoringListByUser([FromHeader] GetTaskMonitorFilters filters);
        public Task<BaseResponseWithData<GetTaskMonitorByTaskGroup>> GetTaskMonitoringListByTask([FromHeader] GetTaskMonitorFilters filters);
        public Task<BaseResponseWithData<List<GetTasksOfUser>>> GetTasksOfUserInDay([FromHeader] DateTime Day, [FromHeader] long UserID);

        public GetTaskTypePermissionResponse GetTaskTypePermissionList();

        public Task<GetTaskDetailsResponse> GetTaskDetailsPerID([FromHeader] long TaskID);

        public Task<BaseResponseWithId<long>> UpdateTaskReadStatus(AddTaskObjData Request);

        public Task<BaseResponseWithId<long>> AddTaskDetails(GetTaskDetailsData Request);

        public Task<BaseResponseWithId<long>> AddTaskFlagsPerUser(GetTaskDetailsData Request);
    }
}
