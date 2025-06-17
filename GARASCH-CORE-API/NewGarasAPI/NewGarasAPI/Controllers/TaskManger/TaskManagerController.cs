using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.DTO.HrUser;
using NewGaras.Domain.Models;
using NewGaras.Domain.Models.TaskManager;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.DTO.Salary.AllowncesType;
using NewGaras.Infrastructure.DTO.Task;
using NewGaras.Infrastructure.DTO.TaskExpensis;
using NewGaras.Infrastructure.DTO.TaskMangerProject;
using NewGaras.Infrastructure.DTO.TaskUnitRateService;
using NewGaras.Infrastructure.DTO.TaskUserReply;
using NewGaras.Infrastructure.DTO.User;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Mail;
using NewGaras.Infrastructure.Models.Task;
using NewGaras.Infrastructure.Models.Task.Filters;
using NewGaras.Infrastructure.Models.Task.UsedInResponse;
using NewGaras.Infrastructure.Models.TaskExpensis.Filters;
using NewGaras.Infrastructure.Models.TaskMangerProject.Filters;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Admin;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.Project.UsedInResponses;
using NewGarasAPI.Models.TaskManager;
using NewGarasAPI.Models.User;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
//using static QRCoder.PayloadGenerator;

namespace NewGarasAPI.Controllers.TaskManger
{
    [Route("[controller]")]
    [ApiController]
    public class TaskManagerController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITaskExpensisService _taskExpensisService;
        private readonly ITaskMangerProjectService _taskMangerProjectService;
        private readonly ITaskService _taskService;
        private readonly IMailService _mailService;
        private readonly IMapper _mapper;
        private readonly ITenantService _tenantService;
        private readonly INotificationService _notificationService;
        public TaskManagerController(IWebHostEnvironment host, ITaskExpensisService taskExpensisService, ITaskMangerProjectService taskMangerProjectService, IMapper mapper, IMailService mailService, ITaskService taskService,ITenantService tenantService, INotificationService notificationService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            key = "SalesGarasPass";
            _helper = new Helper.Helper();
            _host = host;
            _taskExpensisService = taskExpensisService;
            _taskMangerProjectService = taskMangerProjectService;
            _taskService = taskService;
            _mailService = mailService;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        //public ActionResult<GetTaskResponse> GetTask([FromHeader] GetTaskHeader header)
        //{
        //    GetTaskResponse response = new GetTaskResponse();
        //    response.Result = true;
        //    response.Errors = new List<Error>();

        //    try
        //    {

        //        //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        //        //WebHeaderCollection headers = request.Headers;
        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //        response.Errors = validation.errors;
        //        response.Result = validation.result;


        //        var GetTaskList = new List<TasksData>();
        //        if (response.Result)
        //        {


        //            long TaskID = 0;
        //            if (!string.IsNullOrEmpty(headers["TaskID"]) && long.TryParse(headers["TaskID"], out TaskID))
        //            {
        //                long.TryParse(headers["TaskID"], out TaskID);
        //            }
        //            long TaskTypeID = 0;
        //            if (!string.IsNullOrEmpty(headers["TaskTypeID"]) && long.TryParse(headers["TaskTypeID"], out TaskTypeID))
        //            {
        //                long.TryParse(headers["TaskTypeID"], out TaskTypeID);
        //            }
        //            //long TaskUserID = 0;
        //            //if (!string.IsNullOrEmpty(headers["TaskUserID"]) && long.TryParse(headers["TaskUserID"], out TaskUserID))
        //            //{
        //            //    long.TryParse(headers["TaskUserID"], out TaskUserID);
        //            //}
        //            long ToUserID = 0;
        //            if (!string.IsNullOrEmpty(headers["ToUserID"]) && long.TryParse(headers["ToUserID"], out ToUserID))
        //            {
        //                long.TryParse(headers["ToUserID"], out ToUserID);
        //            }
        //            string Status = null;
        //            if (!string.IsNullOrEmpty(headers["Status"]))
        //            {
        //                Status = headers["Status"];
        //            }
        //            string TaskCategory = null;
        //            if (!string.IsNullOrEmpty(headers["TaskCategory"]))
        //            {
        //                TaskCategory = headers["TaskCategory"];
        //            }
        //            string Priority = null;
        //            if (!string.IsNullOrEmpty(headers["Priority"]))
        //            {
        //                Priority = headers["Priority"];
        //            }
        //            string TaskName = null;
        //            if (!string.IsNullOrEmpty(headers["TaskName"]))
        //            {
        //                TaskName = headers["TaskName"];
        //            }
        //            bool? NeedApproval = null;
        //            if (!string.IsNullOrEmpty(headers["NeedApproval"]))
        //            {
        //                NeedApproval = bool.Parse(headers["NeedApproval"]);
        //            }
        //            bool? Read = null;
        //            if (!string.IsNullOrEmpty(headers["Read"]))
        //            {
        //                Read = bool.Parse(headers["Read"]);
        //            }
        //            bool? Flag = null;
        //            if (!string.IsNullOrEmpty(headers["Flag"]))
        //            {
        //                Flag = bool.Parse(headers["Flag"]);
        //            }
        //            bool? Star = null;
        //            if (!string.IsNullOrEmpty(headers["Star"]))
        //            {
        //                Star = bool.Parse(headers["Star"]);
        //            }
        //            bool? IsFinished = null;
        //            if (!string.IsNullOrEmpty(headers["IsFinished"]))
        //            {
        //                IsFinished = bool.Parse(headers["IsFinished"]);
        //            }

        //            DateTime DateFrom = DateTime.MinValue;
        //            if (!string.IsNullOrEmpty(header.DateFrom) && DateTime.TryParse(header.DateFrom, out DateFrom))
        //            {

        //                DateFrom = DateTime.Parse(header.DateFrom);
        //            }

        //            DateTime DateTo = DateTime.MinValue;
        //            if (!string.IsNullOrEmpty(header.DateTo) && DateTime.TryParse(header.DateTo, out DateTo))
        //            {
        //                DateTo = DateTime.Parse(header.DateTo);
        //            }


        //            string SearchKey = "";
        //            if (!string.IsNullOrEmpty(headers["SearchKey"]))
        //            {
        //                SearchKey = HttpUtility.UrlDecode(headers["SearchKey"]);
        //            }


        //            int CurrentPage = 1;
        //            if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
        //            {
        //                int.TryParse(headers["CurrentPage"], out CurrentPage);
        //            }


        //            int NumberOfItemsPerPage = 10;
        //            if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
        //            {
        //                int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
        //            }




        //            if (response.Result)
        //            {


        //            Default: In the First must be back all tasks[Creator - Reciver]


        //                //var TasksPermissionDB = _Context.TaskPermissions.Where(x => x.UserGroupID == TaskUserID && x.IsGroup == false).Select(x => x.TaskID).ToList();

        //                //var TasksUserDetailsDB = _Context.TaskFlagsOwnerRecievers.Where(x => x.Read == false && x.Task.TaskPermissions.Where(y => y.UserGroupID == TaskUserID && y.IsGroup == false).Any()).Select(x => x.TaskID).ToList();

        //                List<long> IDsTasksWithPermissions = new List<long>();
        //                var TasksDBList = _Context.Tasks.Where(x => x.CreatedBy == validation.userID);
        //                int TaskCreatorcount = 0;




        //                if (header.ToUserID == 0)
        //                {

        //                    // Default if user loged in   - in group and already assigned task
        //                    var GroupUserDB = _Context.GroupUsers.Where(x => x.UserId == validation.userID && x.Active == true).Select(x => x.GroupId).ToList();


        //                    // Recived in group or recieved as a user
        //                    IDsTasksWithPermissions = _Context.TaskPermissions.Where(x => (GroupUserDB.Contains(x.UserGroupId) && x.IsGroup == true)
        //                                                                             ||
        //                                                                             (x.UserGroupId == validation.userID && x.IsGroup == false)
        //                                                                             ).Select(x => x.TaskId).ToList();
        //                    //TasksPermissionDB.AddRange(TaskPermissionIDs);


        //                }
        //                else
        //                if (header.ToUserID != 0)
        //                {
        //                    var GroupUserDB = _Context.GroupUsers.Where(x => x.UserId == header.ToUserID && x.Active == true).Select(x => x.GroupId).ToList();

        //                    TasksDBList = TasksDBList.Where(x => x.TaskPermissions.Where(y => (y.IsGroup == false && y.UserGroupId == header.ToUserID)
        //                                                                      || (y.IsGroup == true && (GroupUserDB.Contains(y.UserGroupId)))).Any());

        //                    //var TasksCreatorDetailsDB = _Context.TaskFlagsOwnerRecievers.Where(x => x.Read == false && TasksCreatorDBIDs.Contains(x.TaskID)).Select(x => x.TaskID).ToList();


        //                }
        //                var TasksCreatorDBIDs = TasksDBList.Select(x => x.Id).ToList();
        //                TaskCreatorcount = TasksCreatorDBIDs.Count();
        //                IDsTasksWithPermissions.AddRange(TasksCreatorDBIDs);






        //                //1
        //                bool TaskDetails = false;
        //                var TaskDetailsFilterTaskIDs = new List<long>();
        //                var TaskDetailsFilters = _Context.TaskDetails.AsQueryable();
        //                if (header.Status != null)
        //                {
        //                    TaskDetailsFilters = TaskDetailsFilters.Where(a => a.Status.Trim() == header.Status.Trim()).AsQueryable();
        //                    TaskDetails = true;
        //                }
        //                if (header.Priority != null)
        //                {
        //                    TaskDetailsFilters = TaskDetailsFilters.Where(x => x.Priority.Trim() == header.Priority.Trim()).AsQueryable();
        //                    TaskDetails = true;
        //                }
        //                if (header.NeedApproval != null)
        //                {
        //                    TaskDetailsFilters = TaskDetailsFilters.Where(x => x.NeedApproval == header.NeedApproval).AsQueryable();
        //                    TaskDetails = true;
        //                }
        //                if (header.Delayed)
        //                {
        //                    TaskDetailsFilters = TaskDetailsFilters.Where(a => a.Status.Trim() == "Waiting Approval" || a.Status.Trim() == "Open").AsQueryable();
        //                    TaskDetails = true;
        //                }


        //                //2
        //                bool TaskIsFinished = false;
        //                var TaskIsFinishedTaskIDs = new List<long>();
        //                var TaskIsFinishedFiltersDB = _Context.TaskUserReplies.AsQueryable();
        //                if (header.IsFinished != null)
        //                {
        //                    TaskIsFinishedFiltersDB = TaskIsFinishedFiltersDB.Where(a => a.IsFinished == header.IsFinished).AsQueryable();
        //                    TaskIsFinished = true;
        //                }





        //                //3
        //                bool TaskFlagsOwnerReciever = false;
        //                var TaskFlagsOwnerRecieverFilterTaskIDs = new List<long>();
        //                var TaskFlagsOwnerRecieverFilters = _Context.TaskFlagsOwnerRecievers.AsQueryable();
        //                if (header.Read != false && header.Read != null)
        //                {
        //                    TaskFlagsOwnerRecieverFilters = TaskFlagsOwnerRecieverFilters.Where(x => x.Read == header.Read && x.UserId == validation.userID).AsQueryable();
        //                    TaskFlagsOwnerReciever = true;
        //                }

        //                if (header.Flag != false && header.Flag != null)
        //                {
        //                    TaskFlagsOwnerRecieverFilters = TaskFlagsOwnerRecieverFilters.Where(x => x.Flag == header.Flag && x.UserId == validation.userID).AsQueryable();
        //                    TaskFlagsOwnerReciever = true;
        //                }

        //                if (header.Star != false && header.Star != null)
        //                {
        //                    TaskFlagsOwnerRecieverFilters = TaskFlagsOwnerRecieverFilters.Where(x => x.Star == header.Star && x.UserId == validation.userID).AsQueryable();
        //                    TaskFlagsOwnerReciever = true;
        //                }
        //                List<TaskDetail> TaskDetailsListDB = null;
        //                if (TaskDetails == true)
        //                {
        //                    TaskDetailsListDB = TaskDetailsFilters.ToList();
        //                    TaskDetailsFilterTaskIDs = TaskDetailsFilters.Select(x => x.TaskId).ToList();
        //                }

        //                List<TaskFlagsOwnerReciever> TaskFlagesOwnersReciebertListDB = null;
        //                if (TaskFlagsOwnerReciever == true)
        //                {
        //                    TaskFlagesOwnersReciebertListDB = TaskFlagsOwnerRecieverFilters.ToList();
        //                    TaskFlagsOwnerRecieverFilterTaskIDs = TaskFlagsOwnerRecieverFilters.Select(x => x.TaskId).ToList();
        //                }

        //                List<TaskUserReply> TaskUserReplyDB = null;
        //                if (TaskIsFinished == true)
        //                {
        //                    TaskUserReplyDB = TaskIsFinishedFiltersDB.ToList();
        //                    TaskIsFinishedTaskIDs = TaskIsFinishedFiltersDB.Select(x => x.TaskId).ToList();
        //                }

        //                //Will Union With Type 2 Also
        //                var resultIDs = TaskDetailsFilterTaskIDs.Union(TaskFlagsOwnerRecieverFilterTaskIDs).Union(TaskIsFinishedTaskIDs).ToList();

        //                var FinalTaskIDs = new List<long>();
        //                if (TaskDetails || TaskFlagsOwnerReciever || TaskIsFinished)
        //                {
        //                    if (resultIDs.Count() > 0)
        //                    {
        //                        FinalTaskIDs = IDsTasksWithPermissions.Intersect(resultIDs).ToList();
        //                    }
        //                }
        //                else
        //                {
        //                    FinalTaskIDs = IDsTasksWithPermissions.ToList();
        //                }

        //                var TasksFilterList = _Context.Tasks.AsQueryable();
        //                if (header.TaskTypeID != 0)
        //                {
        //                    TasksFilterList = TasksFilterList.Where(x => x.TaskTypeId == header.TaskTypeID).AsQueryable();
        //                }
        //                if (header.Delayed)
        //                {
        //                    TasksFilterList = TasksFilterList.Where(a => a.ExpireDate < DateTime.Now);
        //                    var test = TasksDBList.ToList();
        //                }
        //                if (!string.IsNullOrWhiteSpace(header.TaskName))
        //                {
        //                    TasksFilterList = TasksFilterList.Where(x => x.Name.Contains(header.TaskName)).AsQueryable();
        //                }
        //                if (header.TaskCategory != null)
        //                {
        //                    TasksFilterList = TasksFilterList.Where(x => x.TaskCategory == header.TaskCategory).AsQueryable();

        //                }
        //                if (header.TaskID != 0)
        //                {
        //                    TasksFilterList = TasksFilterList.Where(x => x.Id == header.TaskID).AsQueryable();
        //                }
        //                if (DateFrom != DateTime.MinValue)
        //                {
        //                    TasksFilterList = TasksFilterList.Where(x => x.CreationDate.Date >= DateFrom.Date).AsQueryable();

        //                }
        //                if (DateTo != DateTime.MinValue)
        //                {
        //                    TasksFilterList = TasksFilterList.Where(x => x.CreationDate.Date <= DateTo.Date).AsQueryable();

        //                }
        //                // Filter by searchKey in Title Subject - body - description
        //                if (!string.IsNullOrWhiteSpace(header.SearchKey))
        //                {
        //                    TasksFilterList = TasksFilterList.Where(x => x.TaskSubject.Contains(header.SearchKey) || x.Description.Contains(header.SearchKey) || x.Name.Contains(header.SearchKey)).AsQueryable();
        //                }


        //                TasksFilterList = TasksFilterList.OrderByDescending(x => x.CreationDate);
        //                var startFrom = ((header.CurrentPage - 1) * header.NumberOfItemsPerPage) + 1;

        //                var TaskPagedIds = FinalTaskIDs.OrderByDescending(item => item).Skip((header.CurrentPage - 1) * header.NumberOfItemsPerPage).Take(header.NumberOfItemsPerPage).ToList();

        //                var Tasks = TasksFilterList.Where(x => FinalTaskIDs.Contains(x.Id));
        //                var TasksList = PagedList<NewGaras.Infrastructure.Entities.Task>.Create(Tasks, header.CurrentPage, header.NumberOfItemsPerPage);
        //                if (TasksList != null)
        //                {

        //                    foreach (var TasksObjDB in TasksList)
        //                    {
        //                        var GetEmployeeResponse = new TasksData();
        //                        var TaskUserFlagsList = new List<TaskUserGroupData>();

        //                        var TaskDetailsObjDB = _Context.TaskDetails.Where(x => x.TaskId == TasksObjDB.Id).FirstOrDefault();
        //                        if (TaskDetailsObjDB != null)
        //                        {

        //                            GetEmployeeResponse.Piriority = TaskDetailsObjDB.Priority;
        //                            GetEmployeeResponse.NeedApproval = TaskDetailsObjDB.NeedApproval;
        //                            if (TaskDetailsObjDB.CreatorAttachement != null)
        //                            {
        //                                GetEmployeeResponse.taskAttachements = Globals.baseURL + TaskDetailsObjDB.CreatorAttachement.TrimStart('~');

        //                            }
        //                        }


        //                        var Taskdb = _Context.Tasks.Where(x => x.Id == TasksObjDB.Id).FirstOrDefault();

        //                        if (Taskdb != null)
        //                        {
        //                            GetEmployeeResponse.CreationDate = Taskdb.CreationDate.ToString();
        //                            GetEmployeeResponse.TaskTypeID = Taskdb.TaskTypeId;
        //                        }
        //                        var TaskOwnerCreationID = _Context.Tasks.Where(x => x.Id == TasksObjDB.Id).Select(x => x.CreatedBy).FirstOrDefault();






        //                        bool IsTaskOwner = false;

        //                        var TaskUserRepliesDb = _Context.TaskUserReplies.Where(a => a.TaskId == TasksObjDB.Id).ToList();


        //                        if (TaskUserRepliesDb != null && TaskUserRepliesDb.Any())
        //                        {
        //                            var TaskFinishedObjDB = TaskUserRepliesDb.Where(x => x.IsFinished == true).FirstOrDefault();
        //                            if (TaskFinishedObjDB != null)
        //                            {
        //                                GetEmployeeResponse.IsFinished = true;

        //                            }
        //                            else
        //                            {
        //                                GetEmployeeResponse.IsFinished = false;
        //                            }

        //                            var LastTaskStatus = TaskUserRepliesDb.Where(x => x.TaskId == TasksObjDB.Id).LastOrDefault();
        //                            if (LastTaskStatus != null)
        //                            {
        //                                if (LastTaskStatus.IsFinished == null && LastTaskStatus.Approval == null)
        //                                {
        //                                    GetEmployeeResponse.TaskStatus = "Need Approval";
        //                                }
        //                                else if (LastTaskStatus.IsFinished == true)
        //                                {
        //                                    GetEmployeeResponse.TaskStatus = "Waiting Approval";
        //                                }
        //                                else if (LastTaskStatus.Approval != null && LastTaskStatus.Approval == false)
        //                                {
        //                                    GetEmployeeResponse.TaskStatus = "Rejected";
        //                                }
        //                                else if (LastTaskStatus.Approval != null && LastTaskStatus.Approval == true)
        //                                {
        //                                    GetEmployeeResponse.TaskStatus = "Approved";
        //                                }
        //                            }

        //                            GetEmployeeResponse.RejectedNo = TaskUserRepliesDb.Where(a => a.Approval == false).Count();
        //                        }

        //                        var TaskStatusDB = _Context.TaskDetails.Where(x => x.TaskId == TasksObjDB.Id).FirstOrDefault();
        //                        if (TaskStatusDB != null)
        //                        {
        //                            GetEmployeeResponse.Status = TaskStatusDB.Status;

        //                        }

        //                        GetEmployeeResponse.ID = TasksObjDB.Id;
        //                        GetEmployeeResponse.Active = TasksObjDB.Active;
        //                        GetEmployeeResponse.CreatedBy = TasksObjDB.CreatedBy;
        //                        if (TasksObjDB.BranchId != 0 && TasksObjDB.BranchId != null)
        //                        {
        //                            GetEmployeeResponse.BranchName = Common.GetBranchName((int)TasksObjDB.BranchId, _Context);
        //                        }

        //                        GetEmployeeResponse.CreatoreName = Common.GetUserName(TasksObjDB.CreatedBy, _Context);

        //                        if (TasksObjDB.CreatedBy == validation.userID)
        //                        {
        //                            GetEmployeeResponse.IsTaskOwner = true;
        //                        }

        //                        GetEmployeeResponse.Description = TasksObjDB.Description;
        //                        GetEmployeeResponse.ExpireDate = TasksObjDB.ExpireDate.ToString();
        //                        GetEmployeeResponse.Name = TasksObjDB.Name;

        //                        GetEmployeeResponse.TaskSubject = TasksObjDB.TaskSubject;


        //                        var CreatorPhoto = Common.GetUserPhoto((long)TasksObjDB.CreatedBy, _Context);
        //                        if (CreatorPhoto != null)
        //                        {
        //                            GetEmployeeResponse.CreatorPhoto = Globals.baseURL + CreatorPhoto; "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(TasksObjDB.CreatedBy.ToString(), key)) + "&type=photo&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();
        //                        }


        //                        GetEmployeeResponse.TaskCount = FinalTaskIDs.Count();
        //                        GetEmployeeResponse.TaskCreatorCount = TaskCreatorcount;
        //                        //GetEmployeeResponse.TaskUserCount = TaskUsercount;




        //                        if (TasksObjDB.TaskTypeId != 0 && TasksObjDB.TaskTypeId != null)
        //                        {
        //                            GetEmployeeResponse.TaskTypeName = Common.GetTaskTypeName(TasksObjDB.TaskTypeId, _Context);
        //                        }

        //                        //GetEmployeeResponse.RequestDate = TasksObjDB.RequestDate;
        //                        GetEmployeeResponse.TaskUser = TasksObjDB.TaskUser;

        //                        GetEmployeeResponse.TaskCategory = TasksObjDB.TaskCategory;
        //                        GetEmployeeResponse.TaskPage = TasksObjDB.TaskPage;
        //                        GetEmployeeResponse.GroupReply = TasksObjDB.GroupReply;



        //                        var UserFlags = _Context.TaskFlagsOwnerRecievers.Where(x => x.TaskId == TasksObjDB.Id).ToList();
        //                        //  var TaskFlagsDB = _Context.TaskFlagsOwnerRecievers.ToList();
        //                        foreach (var item in UserFlags)
        //                        {


        //                            var UserGroupObj = new TaskUserGroupData();


        //                            UserGroupObj.TaskID = item.TaskId;
        //                            UserGroupObj.UserGroupID = item.UserId;
        //                            UserGroupObj.Flag = item.Flag;
        //                            UserGroupObj.Read = item.Read;
        //                            UserGroupObj.Star = item.Star;



        //                            TaskUserFlagsList.Add(UserGroupObj);

        //                        }



        //                        GetEmployeeResponse.TaskUserGroupList = TaskUserFlagsList;
        //                        GetTaskList.Add(GetEmployeeResponse);
        //                    }

        //                    response.TasksList = GetTaskList;


        //                }

        //            }

        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
        //        response.Errors.Add(error);

        //        return response;
        //    }

        //}

        [HttpGet("GetTaskList")]
        public GetTaskResponse GetTask([FromHeader] GetTaskHeader header)
        {
            GetTaskResponse response = new GetTaskResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;


                var GetTaskList = new List<TasksData>();
                //var tasks =new List<GetTaskIndex>().AsQueryable();
                if (response.Result)
                {
                    response = _taskService.GetTask(header,validation.userID);
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
        [HttpGet("GetTaskReply")]
        public ActionResult<GetTaskReplyResponse> GetTaskReply([FromHeader] GetTaskReplyHeader header)
        {
            GetTaskReplyResponse response = new GetTaskReplyResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                var GetTaskReplyDataList = new List<GetTaskReplysData>();


                if (response.Result)
                {
                    response = _taskService.GetTaskReply(header);

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

        [HttpPost("AddTask")]
        public async Task<ActionResult<BaseResponseWithId<long>>> AddTask(AddTaskObjData request)
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
                    Response =await _taskService.AddTask(request, validation.userID, validation.CompanyName);
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

        [HttpPost("AddTaskRequirementsTest")]
        public async Task<ActionResult<BaseResponseWithId<long>>> AddTaskRequirements([FromBody] TaskRequirementModel request)
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
                    Response = await _taskService.AddTaskRequirements(request, validation.userID);
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

        [HttpGet("GetTaskRequirmentsList")]
        public async Task<BaseResponseWithData<List<RequirementModel>>> GetTaskRequirmentsList([FromHeader] long TaskId)
        {
            BaseResponseWithData<List<RequirementModel>> response = new BaseResponseWithData<List<RequirementModel>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (TaskId == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "TaskId is required";
                    response.Errors.Add(error);
                    return response;
                }

                if (response.Result)
                {
                    response = await _taskService.GetTaskRequirmentsList(TaskId);

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

        [HttpPost("EditTask")]
        public async Task<ActionResult<BaseResponseWithId<long>>> EditTask(AddTaskObjData request)
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
                    Response = await _taskService.EditTask(request, validation.userID, validation.CompanyName);
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

        [HttpPost("AddTaskUserReply")]
        public async Task<ActionResult<BaseResponseWithId<long>>> AddTaskUserReply(GetTaskReplysData request)
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
                    Response = await _taskService.AddTaskUserReply(request, validation.userID, validation.CompanyName);
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

        [HttpPost("TestTransaction")]
        public async Task<ActionResult<BaseResponseWithID>> TestTransaction()
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                //HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                //Response.Errors = validation.errors;
                //Response.Result = validation.result;

                var TaskDetails = new GetTaskDetailsData();





                if (Response.Result)
                {
                    //using (var context = new SchoolContext())
                    //{
                    //    context.Database.Log = Console.Write;

                    //    using (DbContextTransaction transaction = context.Database.BeginTransaction())
                    //    {
                    //        try
                    //        {
                    //            var standard = context.Standards.Add(new Standard() { StandardName = "1st Grade" });

                    //            context.Students.Add(new Student()
                    //            {
                    //                FirstName = "Rama2",
                    //                StandardId = standard.StandardId
                    //            });
                    //            context.SaveChanges();

                    //            context.Courses.Add(new Course() { CourseName = "Computer Science" });
                    //            context.SaveChanges();

                    //            transaction.Commit();
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            transaction.Rollback();
                    //            Console.WriteLine("Error occurred.");
                    //        }
                    //    }
                    //}
                    using (var context = new GarasTestContext(_tenantService))
                    {
                        //context.Database.Log = Console.Write;

                        using (var transaction = context.Database.BeginTransaction())
                        {
                            try
                            {
                                var standard = context.WeekDays.Add(new WeekDay() { Day = "saturday" });
                                context.SaveChanges();
                                context.WorkingHours.Add(new WorkingHour()
                                {
                                    Day = 1,//int.Parse(standard),
                                    IntervalFromHour = 12,
                                    IntervalToHour = 12,
                                    CreationDate = DateTime.Now,
                                    Modified = DateTime.Now,
                                    ModifiedBy = "test",
                                    IntervalFromMin = 1,
                                    IntervalToMin = 1,
                                    IntervalMin = 1,
                                    StartDate = DateTime.Now,
                                    EndDate = DateTime.Now

                                }); ; ;
                                context.SaveChanges();

                                //context.Courses.Add(new Course() { CourseName = "Computer Science" });
                                //context.SaveChanges();

                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                Console.WriteLine("Error occurred.");
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

        [HttpPost("AddDeleteTaskUserGroup")]
        public async Task<ActionResult<BaseResponseWithId<long>>> AddDeleteTaskUserGroup(AddDeleteTaskUserGroupRequest request)
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
                    Response = await _taskService.AddDeleteTaskUserGroup(request);
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

        [HttpPost("UpdateTaskFlagsOwnerReciever")]      
        public async Task<ActionResult<BaseResponseWithId<long>>> UpdateTaskFlagsOwnerReciever(GetTaskDetailsData request)
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
                    Response = await _taskService.UpdateTaskFlagsOwnerReciever(request);
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

        [HttpGet("GetTaskCategoryDDL")]     
        public async Task<GetTaskCategoryDDLResponse> GetTaskCategoryDDL()
        {
            GetTaskCategoryDDLResponse response = new GetTaskCategoryDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _taskService.GetTaskCategoryDDL();
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

        [HttpGet("GetTaskDDL")]     //has service
        public BaseResponseWithData<List<TaskDDL>> GetTaskDDL([FromHeader] long ProjectId)
        {
            BaseResponseWithData<List<TaskDDL>> response = new BaseResponseWithData<List<TaskDDL>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                var GetTaskCategoryDDLList = new List<TaskDDL>();
                if (response.Result)
                {
                    response = _taskService.GetTaskDDL(ProjectId);
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


        [HttpGet("GetTaskTypeNameList")]
        public async Task<GetTaskTypeNameResponse> GetTaskTypeNameList()
        {
            GetTaskTypeNameResponse response = new GetTaskTypeNameResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _taskService.GetTaskTypeNameList();
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

        [HttpGet("GetTaskListsByStatus")]   
        public async Task<GetTaskListsByStatusResponse> GetTaskListsByStatus([FromHeader] GetTaskHeader header)
        {
            GetTaskListsByStatusResponse response = new GetTaskListsByStatusResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var ReceivedTasks = new GetTaskResponse();
                    var OpenTasks = new GetTaskResponse();
                    var WaitingTasks = new GetTaskResponse();
                    var ClosedTasks = new GetTaskResponse();
                    header.Status = "Received";
                    ReceivedTasks = GetTask(header);
                    header.Status = "Open";
                    OpenTasks = GetTask(header) ;
                    header.Status = "Waiting Approval";
                    WaitingTasks = GetTask(header) ;
                    header.Status = "Closed";
                    ClosedTasks = GetTask(header);



                    response.OpenTasksList = OpenTasks.TasksList;
                    response.OpenCount = OpenTasks.TaskCount;
                    response.ReceivedTasksList = ReceivedTasks.TasksList;
                    response.ReceivedCount = ReceivedTasks.TaskCount;
                    response.WaitingTasksList = WaitingTasks.TasksList;
                    response.WaitingCount = WaitingTasks.TaskCount;
                    response.ClosedTasksList = ClosedTasks.TasksList;
                    response.ClosedCount = ClosedTasks.TaskCount;
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


        //----------------------------------------task Expensis (with services)-----------------------------------------------
        [HttpPost("AddTaskExpensis")]
        public async Task<BaseResponseWithId<long>> AddTaskExpensis([FromForm] AddTaskExpensisDto dto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (dto.Amount == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Amount Can not be null";
                response.Errors.Add(error);
                return response;
            }
            if (dto.Curruncy == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Curruncy Can not be null";
                response.Errors.Add(error);
                return response;
            }
            if (dto.TaskId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Task ID Can not be null";
                response.Errors.Add(error);
                return response;
            }
            if (dto.ExpensisTypeId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "Expensis Type ID Can not be null";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var newTaskExpensis = _taskExpensisService.AddTaskExpensis(dto, validation.userID, validation.CompanyName);
                    if (!newTaskExpensis.Result)
                    {
                        response.Errors.AddRange(newTaskExpensis.Errors);
                        response.Result = false;
                        return response;
                    }
                    if (newTaskExpensis != null)
                    {
                        var taskExpensis = _taskExpensisService.GetTaskExpensisByID(newTaskExpensis.Data.ID);
                        //-----------------------------------Get Mangers of the project by task ID-----------------------------
                        var projectMangers = _taskMangerProjectService.GetMangersOfProjectByTaskID(taskExpensis.Data.TaskId).Data;

                        if (projectMangers != null && projectMangers.Count() > 0)
                        {
                            foreach (var manger in projectMangers)
                            {
                                _notificationService.CreateNotification(validation.userID, "Task Expensis", "Need Approval", newTaskExpensis.Data.ID.ToString(), true, manger.Id, 0);
                                await _mailService.SendMail(new MailData() { EmailToName = manger.Email, EmailToId = manger.Email, EmailSubject = "Task Expensis need Approval", EmailBody = "" });
                            }
                            if (newTaskExpensis.Data.TotalExpensis > newTaskExpensis.Data.Budget)
                            {
                                foreach (var manger in projectMangers)
                                {
                                    _notificationService.CreateNotification(validation.userID, "Expensis", "exceeds The Project Budget", newTaskExpensis.Data.ID.ToString(), true, manger.Id, 0);
                                    await _mailService.SendMail(new MailData() { EmailToName = manger.Email, EmailToId = manger.Email, EmailSubject = "Expensis exceeds The Project Budget", EmailBody = "" });
                                }
                            }
                        }
                    }

                    response.ID = newTaskExpensis.Data.ID;
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

        [HttpPost("EditTaskExpensis")]
        public BaseResponseWithId<long> EditTaskExpesis([FromForm] EditTaskExpensisDto Dto)
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

            #region Validation
            if (Dto.Id == 0 || Dto.Id == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The ID Field Is required";
                response.Errors.Add(error);

                return response;
            }
            if (Dto.Amount == 0 || Dto.Amount == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The Amount Field Is required";
                response.Errors.Add(error);

                return response;
            }
            if (Dto.Curruncy == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The Curruncy Field Is required";
                response.Errors.Add(error);

                return response;
            }
            if (Dto.TaskId == 0 || Dto.TaskId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The Task ID Field Is required";
                response.Errors.Add(error);

                return response;
            }
            if (Dto.ExpensisTypeId == 0 || Dto.ExpensisTypeId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "The ExpensisType ID Field Is required";
                response.Errors.Add(error);

                return response;
            }

            #endregion

            try
            {
                if (response.Result)
                {
                    var EditedTaskExpensis = _taskExpensisService.EditTaskExpensis(Dto, validation.userID, validation.CompanyName);
                    if (!EditedTaskExpensis.Result)
                    {
                        response.Errors.AddRange(EditedTaskExpensis.Errors);
                        response.Result = false;
                        return response;
                    }
                    if (EditedTaskExpensis != null)
                    {
                        var taskExpensis = _taskExpensisService.GetTaskExpensisByID(EditedTaskExpensis.Data.ID);
                        //-----------------------------------Get Mangers of the project by task ID-----------------------------
                        var projectMangers = _taskMangerProjectService.GetMangersOfProjectByTaskID(taskExpensis.Data.TaskId).Data;

                        if (projectMangers.Count() > 0)
                        {
                            foreach (var manger in projectMangers)
                            {
                                _notificationService.CreateNotification(validation.userID, "Task Expensis", "Need Approval", EditedTaskExpensis.Data.ID.ToString(), true, manger.Id, 0);
                                _mailService.SendMail(new MailData() { EmailToName = manger.Email, EmailToId = manger.Email, EmailSubject = "Task Expensis need Approval", EmailBody = "" });
                            }
                            if (EditedTaskExpensis.Data.TotalExpensis > EditedTaskExpensis.Data.Budget)
                            {
                                foreach (var manger in projectMangers)
                                {
                                    _notificationService.CreateNotification(validation.userID, "Expensis", "Expensis exceeds The Project Budget", EditedTaskExpensis.Data.ID.ToString(), true, manger.Id, 0);
                                    _mailService.SendMail(new MailData() { EmailToName = manger.Email, EmailToId = manger.Email, EmailSubject = "Expensis exceeds The Project Budget", EmailBody = "" });
                                }
                            }
                        }
                    }

                    response.ID = EditedTaskExpensis.Data.ID;
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

        [HttpGet("GetTaskExpensisList")]
        public BaseResponseWithData<List<GetTaskExpensisDto>> GetTaskExpensisList([FromHeader] long TaskID)
        {
            var response = new BaseResponseWithData<List<GetTaskExpensisDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;

            #region Validation
            if (TaskID == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "The TaskId filed is required";
                response.Errors.Add(err);
                return response;
            }
            #endregion
            try
            {
                if (response.Result)
                {
                    var taskExpensis = _taskExpensisService.GetTaskExpensisList(TaskID);
                    if (!taskExpensis.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskExpensis.Errors);
                        return response;
                    }
                    response.Data = taskExpensis.Data;
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

        [HttpPost("AcceptTaskExpensisByManger")]
        public BaseResponseWithId<long> AcceptTaskExpensisByManger([FromBody] AcceptTaskExpensisByMangerDto Dto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;

            if (Dto.ExpsensisID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorMSG = "You Must Add Task Expensis ID";
                error.ErrorCode = "Err-04";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                if (response.Result)
                {

                    var taskExpensis = _taskExpensisService.AcceptTaskExpensisByManger(Dto.ExpsensisID, Dto.Approved, validation.userID);
                    if (taskExpensis == null)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskExpensis.Errors);
                        return response;
                    }
                    response.ID = taskExpensis.ID;
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

        [HttpGet("GetExpensisForAllTasks")]
        public BaseResponseWithData<GetExpensisForAllTasksDto> GetExpensisForAllTasks([FromHeader] GetExpensisForAllTasks filters)
        {
            BaseResponseWithData<GetExpensisForAllTasksDto> response = new BaseResponseWithData<GetExpensisForAllTasksDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user validation
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (filters.ProjectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorMSG = "Project Id Is Required";
                error.ErrorCode = "Err-04";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var expensis = _taskExpensisService.GetExpensisForAllTasks(filters, validation.CompanyName);
                    if (!expensis.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(expensis.Errors);
                        return response;
                    }
                    response = expensis;
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

        [HttpGet("GetTaskExpensisByID")]
        public BaseResponseWithData<GetTaskExpensisDto> GetTaskExpensisByID([FromHeader] long TaskExpensisID)
        {
            var response = new BaseResponseWithData<GetTaskExpensisDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            if (TaskExpensisID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorMSG = "please Enter a valid Expensis ID";
                error.ErrorCode = "Err-04";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                if (response.Result)
                {
                    var taskExpensis = _taskExpensisService.GetTaskExpensisByID(TaskExpensisID);
                    if (taskExpensis.Data == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorMSG = "No Task Expensis with This ID";
                        error.ErrorCode = "Err-04";
                        response.Errors.Add(error);
                        return response;
                    }
                    response = taskExpensis;
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

        //-----------------------------------------ProjectTaskManger (with services)-------------------------------------------
        [HttpPost("AddTaskMangerProject")]
        public BaseResponseWithId<long> AddTaskMangerProject([FromForm] AddTaskMangerProjectDto Dto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region Validaton
            DateTime startDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.StartDate, out startDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid Start Date:";
                response.Errors.Add(err);
                return response;
            }
            DateTime endDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.StartDate, out endDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid End Date:";
                response.Errors.Add(err);
                return response;
            }
            if (string.IsNullOrWhiteSpace(Dto.ProjectName))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project Name";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.ClientID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project Client";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.Budget == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project Budget";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.CurrencyID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project Currency ID";
                response.Errors.Add(error);
                return response;
            }
            //if (Dto.ContactPersonID == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //if (Dto.ProjectContactPersonCountryId == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person Country Id";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //if (Dto.ProjectContactPersonGovernorateID == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person Government Id";
            //    response.Errors.Add(error);
            //    return response;
            //}


            //if (string.IsNullOrWhiteSpace(Dto.ProjectContactPersonName))
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person Name";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //if (string.IsNullOrWhiteSpace(Dto.ProjectContactPersonMob))
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person Mobile";
            //    response.Errors.Add(error);
            //    return response;
            //}
            #endregion

            try
            {
                if (response.Result)
                {
                    var newTaskExpensis = _taskMangerProjectService.AddTaskMangerProject(Dto, validation.userID, validation.CompanyName);
                    if (!newTaskExpensis.Result)
                    {
                        response.Errors.AddRange(newTaskExpensis.Errors);
                        response.Result = false;
                        return response;
                    }
                    response.ID = newTaskExpensis.ID;
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

        [HttpGet("GetTaskMangerProject")]
        public BaseResponseWithData<GetTaskMangerProjectDto> GetTaskMangerProject([FromHeader] long TaskMangerProjectID)
        {
            var response = new BaseResponseWithData<GetTaskMangerProjectDto>()
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
                    response = _taskMangerProjectService.GetTaskMangerProject(TaskMangerProjectID);
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

        [HttpPost("EditTaskMangerProject")]
        public BaseResponseWithId<long> EditTaskMangerProject([FromForm] EditTaskMangerProjectDto Dto)
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

            #region Validaton
            if (Dto.Id == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project ID";
                response.Errors.Add(error);
                return response;
            }
            DateTime startDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.StartDate, out startDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid Start Date:";
                response.Errors.Add(err);
                return response;
            }
            DateTime endDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.StartDate, out endDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid End Date:";
                response.Errors.Add(err);
                return response;
            }
            if (string.IsNullOrWhiteSpace(Dto.ProjectName))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project Name";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.ClientID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project Client";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.Budget == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project Budget";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.CurrencyID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project Currency ID";
                response.Errors.Add(error);
                return response;
            }
            //if (Dto.ContactPersonID == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //if (Dto.ProjectContactPersonCountryId == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person Country Id";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //if (Dto.ProjectContactPersonGovernorateID == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person Government Id";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //if (string.IsNullOrWhiteSpace(Dto.ProjectContactPersonAddress))
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person Address";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //if (string.IsNullOrWhiteSpace(Dto.ProjectContactPersonName))
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person Name";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //if (string.IsNullOrWhiteSpace(Dto.ProjectContactPersonMob))
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "you Must Enter Project Contact Person Mobile";
            //    response.Errors.Add(error);
            //    return response;
            //}
            #endregion

            try
            {
                if (response.Result)
                {
                    var TaskMangerProject = _taskMangerProjectService.EditTaskMangerProject(Dto, validation.userID, validation.CompanyName);
                    if (!TaskMangerProject.Result)
                    {
                        response.Errors.AddRange(TaskMangerProject.Errors);
                        response.Result = false;
                        return response;
                    }
                    response.ID = TaskMangerProject.ID;
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

        [HttpGet("GetTaskMangerProjectsCards")]
        public async Task<BaseResponseWithDataAndHeader<List<GetTaskMangerProjectCardsDto>>> GetTaskMangrProjectCards(TaskMangerProjectsFilters filters)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetTaskMangerProjectCardsDto>>();
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
                    response = await _taskMangerProjectService.GetTaskMangerProjectCards(filters, validation.userID);
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

        //-----------------------------------------TaskListGropedByWorkFlow-------------------------------------
        [HttpGet("GetTaskGroupedByWorkFlowList")]   //has service
        public BaseResponseWithData<TaskWorkFlosGroups> GetTaskGroupedByWorkFlowList([FromHeader] bool NotActive, [FromHeader] long ProjectID, [FromHeader] long ProjectSprintID, [FromHeader]bool IsArchived, [FromHeader] int CurrentPage, [FromHeader] int NumberOfItemsPerPage)
        {
            BaseResponseWithData<TaskWorkFlosGroups> response = new BaseResponseWithData<TaskWorkFlosGroups>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _taskService.GetTaskGroupedByWorkFlowList(NotActive, ProjectID, ProjectSprintID, IsArchived, validation.userID,CurrentPage, NumberOfItemsPerPage);
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

        //-----------------------------------------project Assign to user (with services)---------------------------------------
        [HttpPost("AddUsersToProject")]
        public BaseResponseWithId<long> AddUsersToProject([FromBody] AddUsersToProjectDto Dto)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (Dto.projectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project ID";
                response.Errors.Add(error);
                return response;
            }
            int AdminCounter = 0;
            foreach (var userId in Dto.AdminUsersList)
            {
                if (userId == 0 || userId < 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = $"you Must Enter a valid User ID at Admin Number : {AdminCounter + 1}";
                    response.Errors.Add(error);
                    return response;
                }
                AdminCounter++;
            }
            int mangerCounter = 0;
            foreach (var userId in Dto.AdminUsersList)
            {
                if (userId == 0 || userId < 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = $"you Must Enter a valid User ID at Manger Number : {mangerCounter + 1}";
                    response.Errors.Add(error);
                    return response;
                }
                mangerCounter++;
            }
            int userCounter = 0;
            foreach (var userId in Dto.NormalUsersList)
            {
                if (userId == 0 || userId < 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = $"you Must Enter a valid User ID at User Number : {userCounter + 1}";
                    response.Errors.Add(error);
                    return response;
                }
                userCounter++;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var assignUsersToProect = _taskMangerProjectService.AddUsersToProject(Dto, validation.userID);
                    if (!assignUsersToProect.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(assignUsersToProect.Errors);
                    }
                    response.ID = Dto.projectID;
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

        [HttpGet("GetUsersAssignToProject")]
        public BaseResponseWithData<GetUsersToProjectDto> GetUsersAssignToProject([FromHeader] long projectID)
        {
            BaseResponseWithData<GetUsersToProjectDto> response = new BaseResponseWithData<GetUsersToProjectDto>();
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
                    var users = _taskMangerProjectService.GetUsersOfProjects(projectID);
                    if (!users.Result)
                    {
                        response.Errors.AddRange(users.Errors);
                        return response;
                    }
                    response.Data = users.Data;
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

        [HttpPost("EditUsersAssignToProject")]
        public BaseResponseWithId<long> EditUsersAssignToProject([FromBody] EditUsersAssignToProjectDto Dto)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion


            #region validation
            if (Dto.ProjectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project ID";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.AdminUsersList != null && Dto.AdminUsersList.Count() > 0)
            {

                int AdminCounter = 0;
                foreach (var user in Dto.AdminUsersList)
                {
                    if (user.UserID == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = $"you Must Enter a valid User ID at Admin Number : {AdminCounter + 1}";
                        response.Errors.Add(error);
                        return response;
                    }
                    AdminCounter++;
                }
            }

            if (Dto.ManagerUsersList != null && Dto.ManagerUsersList.Count() > 0)
            {
                int mangerCounter = 0;
                foreach (var user in Dto.ManagerUsersList)
                {
                    if (user.UserID == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = $"you Must Enter a valid User ID at Manger Number : {mangerCounter + 1}";
                        response.Errors.Add(error);
                        return response;
                    }
                    mangerCounter++;
                }
            }

            if (Dto.NormalUsersList != null && Dto.NormalUsersList.Count() > 0)
            {
                int userCounter = 0;
                foreach (var user in Dto.NormalUsersList)
                {
                    if (user.UserID == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = $"you Must Enter a valid User ID at User Number : {userCounter + 1}";
                        response.Errors.Add(error);
                        return response;
                    }
                    userCounter++;
                }

            }
            #endregion


            try
            {
                if (response.Result)
                {
                    var editUser = _taskMangerProjectService.EditUsersOfProject(Dto, validation.userID);
                    if (!editUser.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(editUser.Errors);
                        return response;
                    }
                    response.ID = editUser.ID;
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

        [HttpGet("GetNormalUsersAssignToProject")]
        public BaseResponseWithData<List<UserWithJobTitleDDL>> GetAllNormslUsersOfProjectDDl([FromHeader] long projectId)                     //list of All Normal users
        {
            BaseResponseWithData<List<UserWithJobTitleDDL>> response = new BaseResponseWithData<List<UserWithJobTitleDDL>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (projectId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter Project ID";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                if (response.Result)
                {
                    var users = _taskMangerProjectService.GetAllNormslUsersOfProjectDDl(projectId);
                    if (!users.Result)
                    {
                        response.Errors.AddRange(users.Errors);
                        return response;
                    }
                    response.Data = users.Data;
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

        //--------------------------------------------------TaskUnitRateServices (with services)-----------------------------------
        [HttpPost("AddTaskUnitRateService")]
        public BaseResponseWithId<long> AddTaskUnitRateService([FromBody] AddTaskUnitRateServiceDto Dto)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region Validation
            if (string.IsNullOrEmpty(Dto.ServiceName))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a valid Service Name";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.Rate == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a valid Rate";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.Quantity == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a valid Quantity";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.TaskID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a valid Task ID";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.UOMID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a valid UOM ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var newTaskUnitRateService = _taskService.AddTaskUnitRateService(Dto, validation.userID);
                    if (!newTaskUnitRateService.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(newTaskUnitRateService.Errors);
                    }
                    response.ID = newTaskUnitRateService.ID;
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

        [HttpPost("EditTaskUnitRateService")]
        public BaseResponseWithId<long> EditTaskUnitRateService([FromBody] EditTaskUnitRateServiceDto Dto)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region Validation
            if (Dto.Id == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a Task Unit Rate Service ID";
                response.Errors.Add(error);
                return response;
            }
            if (string.IsNullOrEmpty(Dto.ServiceName))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a valid Service Name";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.Rate == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a valid Rate";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.Quantity == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a valid Quantity";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.TaskID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a valid Task ID";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.UOMID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter a valid UOM ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var newTaskUnitRateService = _taskService.EditTaskUnitRateService(Dto, validation.userID);
                    if (!newTaskUnitRateService.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(newTaskUnitRateService.Errors);
                    }
                    response.ID = newTaskUnitRateService.ID;
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

        [HttpGet("GetTaskUnitRateServiceList")]
        public BaseResponseWithData<List<GetTaskUnitRateServiceDto>> GetTaskUnitRateServiceList([FromHeader] long TaskID)
        {
            var response = new BaseResponseWithData<List<GetTaskUnitRateServiceDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;

            #region Validation
            if (TaskID == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "The TaskId filed is required";
                response.Errors.Add(err);
                return response;
            }
            #endregion
            try
            {
                if (response.Result)
                {
                    var taskExpensis = _taskService.GetTaskUnitRateServiceList(TaskID);
                    if (!taskExpensis.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskExpensis.Errors);
                        return response;
                    }
                    response.Data = taskExpensis.Data;
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

        [HttpGet("GetTaskUnitRateServiceByID")]
        public BaseResponseWithData<GetTaskUnitRateServiceDto> GetTaskUnitRateServiceByID([FromHeader] long TaskUnitRateServiceID)
        {
            var response = new BaseResponseWithData<GetTaskUnitRateServiceDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;

            #region Validation
            if (TaskUnitRateServiceID == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "The TaskUnitRateServiceID filed is required";
                response.Errors.Add(err);
                return response;
            }
            #endregion
            try
            {
                if (response.Result)
                {
                    var taskExpensis = _taskService.GetTaskUnitRateServiceByID(TaskUnitRateServiceID);
                    if (!taskExpensis.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskExpensis.Errors);
                        return response;
                    }
                    response.Data = taskExpensis.Data;
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

        [HttpGet("GetInventoryUOMDDL")]
        public BaseResponseWithData<List<GetInventoryUOMDDL>> GetInventoryUOMDDL()
        {
            var response = new BaseResponseWithData<List<GetInventoryUOMDDL>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;

            try
            {
                if (response.Result)
                {
                    var inventoryUOMList = _taskService.GetInventoryUOMDDL();
                    if (!inventoryUOMList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(inventoryUOMList.Errors);
                    }
                    response = inventoryUOMList;
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

        [HttpGet("GetTaskUnitRateServiceListByProjectID")]
        public BaseResponseWithData<List<GetTaskUnitRateServiceDto>> GetTaskUnitRateServiceListByProjectID([FromHeader]long ProjectID)
        {
            var response = new BaseResponseWithData<List<GetTaskUnitRateServiceDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;

            #region Validation
            if (ProjectID == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "The projectID filed is required";
                response.Errors.Add(err);
                return response;
            }
            #endregion
            try
            {
                if (response.Result)
                {
                    var taskExpensis = _taskService.GetTaskUnitRateServiceListByProjectID(ProjectID);
                    if (!taskExpensis.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskExpensis.Errors);
                        return response;
                    }
                    response.Data = taskExpensis.Data;
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

        [HttpPost("DeleteTaskUnitRateService")]
        public BaseResponseWithId<long> DeleteTaskUnitRateService([FromHeader] long TaskId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = _taskService.DeleteTaskUnitRateService(TaskId);
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

        //-----------------------------------------APIs with Services-------------------------------------------------
        [HttpGet("GetTaskTypeNameListTest")]
        public async Task<GetTaskTypeNameResponse> GetTaskTypeNameListTest()
        {
            var response = new GetTaskTypeNameResponse()
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
                    var TaskCategory = await _taskService.GetTaskTypeNameList();
                    if (TaskCategory.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(TaskCategory.Errors);
                    }
                    response = TaskCategory;
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

        [HttpGet("GetTaskCategoryDDLTest")]
        public async Task<GetTaskCategoryDDLResponse> GetTaskCategoryDDLTest()
        {
            var response = new GetTaskCategoryDDLResponse()
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
                    var TaskCategory = await _taskService.GetTaskCategoryDDL();
                    if (TaskCategory.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(TaskCategory.Errors);
                    }
                    response = TaskCategory;
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

        [HttpPost("UpdateTaskFlagsOwnerRecieverTest")]
        public async Task<BaseResponseWithId<long>> UpdateTaskFlagsOwnerRecieverTest(GetTaskDetailsData request)
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
                    var task = await _taskService.UpdateTaskFlagsOwnerReciever(request);
                    if (task.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(task.Errors);
                    }
                    response = task;
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

        [HttpPost("AddDeleteTaskUserGroupTest")]
        public async Task<ActionResult<BaseResponseWithId<long>>> AddDeleteTaskUserGroupTest(AddDeleteTaskUserGroupRequest request)
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
                    var taskUserGroup = await _taskService.AddDeleteTaskUserGroup(request);
                    if (taskUserGroup.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskUserGroup.Errors);
                    }
                    response = taskUserGroup;
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

        [HttpPost("AddTaskUserReplyTest")]
        public async Task<ActionResult<BaseResponseWithId<long>>> AddTaskUserReplyTest(GetTaskReplysData request)
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
                    var taskUserReply = await _taskService.AddTaskUserReply(request, validation.userID, validation.CompanyName);
                    if (taskUserReply.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskUserReply.Errors);
                    }
                    response = taskUserReply;
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

        [HttpPost("EditTaskTest")]
        public async Task<BaseResponseWithId<long>> EditTaskTest(AddTaskObjData request)
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
                    var task = await _taskService.EditTask(request, validation.userID, validation.CompanyName);
                    if (task.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(task.Errors);
                    }
                    response = task;
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

        [HttpGet("GetTaskRequirmentsListTest")]
        public async Task<BaseResponseWithData<List<RequirementModel>>> GetTaskRequirmentsListTest([FromHeader] long TaskId)
        {
            var response = new BaseResponseWithData<List<RequirementModel>>()
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
                    var taskRequirements = await _taskService.GetTaskRequirmentsList(TaskId);
                    if (taskRequirements.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskRequirements.Errors);
                    }
                    response = taskRequirements;
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

        [HttpPost("AddTaskRequirements")]
        public async Task<BaseResponseWithId<long>> AddTaskRequirementsTest(TaskRequirementModel request)
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
                    var taskRequirements = await _taskService.AddTaskRequirements(request,  validation.userID);
                    if (taskRequirements.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskRequirements.Errors);
                    }
                    response = taskRequirements;
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

        [HttpPost("AddTaskTest")]
        public async Task<BaseResponseWithId<long>> AddTaskTest([FromBody]AddTaskObjData request)
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
                    var taskReply =await _taskService.AddTask(request,validation.userID, validation.CompanyName);
                    if (taskReply.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskReply.Errors);
                    }
                    response = taskReply;
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

        [HttpGet("GetTaskReplyTest")]
        public GetTaskReplyResponse GetTaskReplytest([FromHeader] GetTaskReplyHeader header)
        {
            var response = new GetTaskReplyResponse()
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
                    var taskReply = _taskService.GetTaskReply(header);
                    if (!taskReply.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskReply.Errors);
                    }
                    response = taskReply;
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

        [HttpGet("GetTaskDetails")]
        public GetTaskUserAndGroupResponse GetTaskDetailsTest([FromHeader] long TaskID = 0)
        {
            GetTaskUserAndGroupResponse response = new GetTaskUserAndGroupResponse();
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
                    var task = _taskService.GetTaskDetails(validation.userID,TaskID);
                    if (!task.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(task.Errors);
                    }
                    response = task;
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

        [HttpGet("GetTaskGroupedByWorkFlowListTest")]
        public BaseResponseWithData<TaskWorkFlosGroups> GetTaskWithService([FromHeader]long ProjectID, [FromHeader]long ProjectSprintID, [FromHeader]bool NotActive, [FromHeader]bool IsArchived, [FromHeader] int CurrentPage, [FromHeader] int NumberOfItemsPerPage)
        {
            var response = new BaseResponseWithData<TaskWorkFlosGroups>()
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
                    var task = _taskService.GetTaskGroupedByWorkFlowList(NotActive, ProjectID, ProjectSprintID, IsArchived, validation.userID, CurrentPage, NumberOfItemsPerPage);
                    if (!task.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(task.Errors);
                    }
                    response = task;
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

        [HttpPost("EditTaskUserReply")]
        public BaseResponseWithId<long> EditTaskUserReply([FromForm]EditTaskUserReplyDto dto)
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

            #region validation
            if (dto.ID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Task User Reply ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var taskUserReply = _taskService.EditTaskUserReply(dto, validation.CompanyName);
                    if(!taskUserReply.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskUserReply.Errors);
                        return response;
                    }
                    response.ID = taskUserReply.ID;
                }
                return response;
            }
            catch(Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpPost("DeleteTaskUserReply")]
        public BaseResponseWithId<long> DeleteEditTaskUserReply([FromForm]long Id)
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

            #region validation
            if (Id == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Task User Reply ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var taskUserReply = _taskService.DeleteEditTaskUserReply(Id);
                    if (!taskUserReply.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskUserReply.Errors);
                        return response;
                    }
                    response.ID = taskUserReply.ID;
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

        [HttpPost("DeleteTaskExpensis")]
        public BaseResponseWithId<long> DeleteTaskExpensis([FromForm]long Id)
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

            #region validation
            if (Id == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Task Expensis ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var taskExpensis = _taskExpensisService.DeleteTaskExpensis(Id);
                    if (!taskExpensis.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(taskExpensis.Errors);
                        return response;
                    }
                    response.ID = taskExpensis.ID;
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

        [HttpPost("DeleteTask")]
        public BaseResponseWithId<long> DeleteTask([FromHeader]long TaskId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = _taskService.DeleteTask(TaskId);
                }
                return response;
            }
            catch(Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetTasksListReportExcell")]
        public BaseResponseWithData<string> GetTasksListReportExcell([FromHeader]GetTasksListReportFilters filters)
        {
            var response = new BaseResponseWithData<string>()
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
                    var tasksList = _taskService.GetTasksListReportExcell(filters, validation.CompanyName);
                    if (!tasksList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(tasksList.Errors);
                        return response;
                    }
                    response = tasksList;
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

        [HttpGet("GetTaskProgressReport")]
        public BaseResponseWithData<string> GetTaskProgressReport([FromHeader] long ProjectId, [FromHeader] long? TaskId, [FromHeader] long? InvoiceNumber, [FromHeader] string Type, [FromHeader] string UserName, [FromHeader] DateTime? From, [FromHeader] DateTime? To)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
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
                    response = _taskService.GetTaskProgressReport(ProjectId, TaskId, InvoiceNumber, Type,UserName, From,To,validation.CompanyName);
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


        /*[HttpPost("AddTaskBrowserTabs")]
        public async Task<BaseResponse> AddTaskBrowserTabs([FromBody] AddTaskBrowserTabsDtoList dto)
        {
            BaseResponse response = new BaseResponse()
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
                    var newTaskExpensis =await _taskService.AddTaskBrowserTabs(dto);
                    if (!newTaskExpensis.Result)
                    {
                        response.Errors.AddRange(newTaskExpensis.Errors);
                        response.Result = false;
                        return response;
                    }
                    //response.ID = newTaskExpensis.ID;
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

        [HttpPost("AddTaskApplicationOpen")]
        public async Task<BaseResponse> AddTaskApplicationOpen([FromBody]AddTaskApplicationOpenList dto)
        {
            BaseResponse response = new BaseResponse()
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
                    var newTaskExpensis =await _taskService.AddTaskApplicationOpen(dto);
                    if (!newTaskExpensis.Result)
                    {
                        response.Errors.AddRange(newTaskExpensis.Errors);
                        response.Result = false;
                        return response;
                    }
                    //response.ID = newTaskExpensis.ID;
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

        [HttpPost("AddTaskScreenShot")]
        public async Task<BaseResponse> AddTaskScreenShot([FromForm]AddTaskScreenShotList dto)
        {
            BaseResponse response = new BaseResponse()
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
                    var newTaskExpensis = await _taskService.AddTaskScreenShot(dto, validation.CompanyName);
                    if (!newTaskExpensis.Result)
                    {
                        response.Errors.AddRange(newTaskExpensis.Errors);
                        response.Result = false;
                        return response;
                    }
                    //response.ID = newTaskExpensis.ID;
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

        [HttpGet("GetTaskBrowserTabs")]
        public async Task<BaseResponseWithData<GetTaskBrowserTabsDtoList>> GetTaskBrowserTabs([FromHeader]long? UserID, [FromHeader]long? TaskID)
        {
            var response = new BaseResponseWithData<GetTaskBrowserTabsDtoList>()
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
                    var tabs = await _taskService.GetTaskBrowserTabs(UserID, TaskID);
                    if (!tabs.Result)
                    {
                        response.Errors.AddRange(tabs.Errors);
                        response.Result = false;
                        return response;
                    }
                    response = tabs;
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

        [HttpGet("GetTaskAppsOpen")]
        public async Task<BaseResponseWithData<GetTaskAppsOpenList>> GetTaskAppsOpen([FromHeader] long? UserID, [FromHeader] long? TaskID)
        {
            var response = new BaseResponseWithData<GetTaskAppsOpenList>()
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
                    var apps = await _taskService.GetTaskAppsOpen(UserID, TaskID);
                    if (!apps.Result)
                    {
                        response.Errors.AddRange(apps.Errors);
                        response.Result = false;
                        return response;
                    }
                    response = apps;
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

        [HttpGet("GetTaskScreenShot")]
        public async Task<BaseResponseWithData<GetTaskScreenShotDtoList>> GetTaskScreenShot([FromHeader]long? UserID, [FromHeader]long? TaskID)
        {
            var response = new BaseResponseWithData<GetTaskScreenShotDtoList>()
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
                    var apps = await _taskService.GetTaskScreenShot(UserID, TaskID);
                    if (!apps.Result)
                    {
                        response.Errors.AddRange(apps.Errors);
                        response.Result = false;
                        return response;
                    }
                    response = apps;
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
        }*/

        [HttpPost("AddTaskMonitoring")]
        public async Task<BaseResponse> AddTaskMonitoring([FromForm] AddTaskMonitorDtoList dto)
        {
            BaseResponse response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _taskService.AddTaskMonitor(dto,validation.CompanyName);
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




        [HttpGet("GetTaskMonitoringByUser")]
        public async Task<BaseResponseWithData<GetTaskMonitorByUser>> GetTaskMonitoringList(GetTaskMonitorFilters filters)
        {
            BaseResponseWithData<GetTaskMonitorByUser> response = new BaseResponseWithData<GetTaskMonitorByUser>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _taskService.GetTaskMonitoringListByUser(filters);
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

        [HttpGet("GetTaskMonitoringByTask")]
        public async Task<BaseResponseWithData<GetTaskMonitorByTaskGroup>> GetTaskMonitoringListByTask([FromHeader] GetTaskMonitorFilters filters)
        {
            BaseResponseWithData<GetTaskMonitorByTaskGroup> response = new BaseResponseWithData<GetTaskMonitorByTaskGroup>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _taskService.GetTaskMonitoringListByTask(filters);
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


        [HttpGet("GetTasksOfUserInDay")]
        public async Task<BaseResponseWithData<List<GetTasksOfUser>>> GetTasksOfUserInDay([FromHeader] DateTime Day, [FromHeader] long UserID)
        {
            BaseResponseWithData<List<GetTasksOfUser>> response = new BaseResponseWithData<List<GetTasksOfUser>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _taskService.GetTasksOfUserInDay(Day, UserID);
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

        [HttpGet("GetTaskTypePermissionList")]
        public GetTaskTypePermissionResponse GetTaskTypePermissionList()
        {
            GetTaskTypePermissionResponse response = new GetTaskTypePermissionResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = _taskService.GetTaskTypePermissionList();
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

        [HttpGet("GetTaskDetailsPerID")]
        public async Task<GetTaskDetailsResponse> GetTaskDetailsPerID([FromHeader] long TaskID)
        {
            GetTaskDetailsResponse response = new GetTaskDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _taskService.GetTaskDetailsPerID(TaskID);
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

        [HttpPost("UpdateTaskReadStatus")]
        public async Task<BaseResponseWithId<long>> UpdateTaskReadStatus(AddTaskObjData request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _taskService.UpdateTaskReadStatus(request);
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

        [HttpPost("AddTaskDetails")]
        public async Task<BaseResponseWithId<long>> AddTaskDetails(GetTaskDetailsData request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _taskService.AddTaskDetails(request);
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

        [HttpPost("AddTaskFlagsPerUser")]
        public async Task<BaseResponseWithId<long>> AddTaskFlagsPerUser(GetTaskDetailsData request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _taskService.Validation = validation;
                    response = await _taskService.AddTaskFlagsPerUser(request);
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

    public class WriteLogFile
    {
        public static bool WriteLog(string strFileName, string strMessage)
        {
            try
            {
                string logDirectory = "/logs";
                string logFileName = "applog.txt";
                string logFilePath = Path.Combine(logDirectory, logFileName);
                FileStream objFilestream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter(objFilestream);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
