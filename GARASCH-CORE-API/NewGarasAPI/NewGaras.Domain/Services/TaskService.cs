using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NewGaras.Domain.Consts;
using NewGaras.Domain.Models;
using NewGaras.Domain.Models.TaskManager;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.TaskExpensis;
using NewGaras.Infrastructure.DTO.TaskUnitRateService;
using NewGaras.Infrastructure.DTO.TaskUserReply;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Task;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.TaskManager;
using NewGarasAPI.Models.User;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Task = NewGaras.Infrastructure.Entities.Task;
using NewGaras.Infrastructure.Models.Task.Filters;
using System.Drawing;
using System.Reflection.Metadata;
using DocumentFormat.OpenXml.Spreadsheet;
using Color = System.Drawing.Color;
using Azure;
using DocumentFormat.OpenXml.Office2010.Excel;
using NewGaras.Infrastructure.Models.Task.UsedInResponse;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using NewGaras.Infrastructure.DTO.Task;
using System.IO;
using System.Net;

namespace NewGaras.Domain.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
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
        //private readonly IAttendanceService _attendanceService;
        public TaskService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            //_attendanceService = attendanceService;
        }

        //public GetTaskResponse GetTask(GetTaskHeader header, long userID)
        //{
        //    GetTaskResponse response = new GetTaskResponse();
        //    response.Result = true;
        //    response.Errors = new List<Error>();

        //    try
        //    {

        //        var GetTaskList = new List<TasksData>();
        //        //var tasks =new List<GetTaskIndex>().AsQueryable();
        //        if (response.Result)
        //        {
        //            //WriteLogFile.WriteLog("ConsoleLog.log", string.Format("{0} @ {1}", "Log is Created at", DateTime.Now));

        //            //level_1
        //            var tasksIDList = new List<long>();
        //            long UserId = userID;
        //            var ListUserPM = new List<long>();
        //            ListUserPM = _Context.Tasks.Include(a => a.Project.ProjectAssignUsers).SelectMany(a => a.Project.ProjectAssignUsers.Where(x => x.RoleId == 145 && a.Active).Select(a => a.UserId)).ToList();
        //            if (ListUserPM.Contains(userID))
        //            {
        //                var assignUsersIds = _unitOfWork.ProjectAssignUsers.FindAll(a => a.UserId == userID).Select(b => b.ProjectId).ToList();
        //                var projects = _unitOfWork.Projects.FindAll(a => assignUsersIds.Contains(a.Id)).Select(b => b.Id).ToList();
        //                tasksIDList.AddRange(_Context.Tasks.Include(a => a.Project.ProjectAssignUsers.Where(x => x.UserId == userID)).Select(a => a.Id).ToList());
        //                var test = _unitOfWork.Tasks.FindAll(a => projects.Contains(a.ProjectId??0), new[] { "Project" }).Select(a => a.Id);
        //            }
        //            if (header.ToUserID != 0)
        //            {
        //                UserId = header.ToUserID;
        //            }

        //            tasksIDList.AddRange(_unitOfWork.TaskPermissions.FindAll(x => x.UserGroupId == UserId && x.IsGroup == false || x.IsGroup && _unitOfWork.GroupUsers.FindAll(a => a.GroupId == x.UserGroupId).Select(c => c.UserId).Contains(UserId)).Select(x => x.TaskId).ToList());

        //            tasksIDList = tasksIDList.Distinct().ToList();
        //            //level_2
        //            //var FinalTasksQuery = _Context.Tasks.Where(x => tasksIDList.Contains(x.Id)).AsQueryable();

        //            Expression<Func<Task, bool>> criteria = (a => true); 

        //            criteria = a =>
        //            (
        //            (tasksIDList.Contains(a.Id)) &&
        //            (header.NotActive != null ? a.Active == false : a.Active == true) &&
        //            (header.IsArchived == true ? a.IsArchived == true : (a.IsArchived == false || a.IsArchived == null))
        //            );
        //            var FinalTasksQuery = _unitOfWork.Tasks.FindAll(criteria, new[] { "CreatedByNavigation" });

        //            //if (header.NotActive == true)
        //            //{
        //            //    FinalTasksQuery = FinalTasksQuery.Where(x => x.Active == false).AsQueryable();
        //            //}
        //            //else
        //            //{
        //            //    FinalTasksQuery = FinalTasksQuery.Where(x => x.Active == true).AsQueryable();
        //            //}

        //            //if (header.IsArchived)
        //            //{
        //            //    FinalTasksQuery = FinalTasksQuery.Where(x => x.IsArchived == true).AsQueryable();
        //            //}
        //            var FinalTasks = FinalTasksQuery;
        //            //level_3

        //            var replies = _unitOfWork.TaskUserReplies.FindAll(x => tasksIDList.Contains(x.TaskId)).ToList();
        //            var details = _unitOfWork.TaskDetails.FindAll(x => tasksIDList.Contains(x.TaskId)).ToList();
        //            var types = _unitOfWork.TaskTypes.GetAll();
        //            var taskCount = _unitOfWork.Tasks.GetAll();
        //            var receivers = _unitOfWork.TaskFlagsOwnerRecievers.FindAll(x => tasksIDList.Contains(x.TaskId)).ToList();
        //            var tasks = FinalTasks.Select(a => new GetTaskIndex
        //            {
        //                ID = a.Id,
        //                Active = a.Active,
        //                IsFinished = replies.Where(x => x.TaskId == a.Id).Select(x => x.IsFinished).FirstOrDefault(),
        //                NeedApproval = details.Where(x => x.TaskId == a.Id).Select(x => x.NeedApproval).FirstOrDefault(),
        //                IsTaskOwner = a.CreatedBy == userID,
        //                Piriority = details.Where(x => x.TaskId == a.Id).Select(x => x.Priority).FirstOrDefault(),
        //                Status = details.Where(x => x.TaskId == a.Id).Select(x => x.Status).FirstOrDefault(),
        //                Name = a.Name,
        //                Description = a.Name,
        //                ExpireDate = a.ExpireDate?.ToString(),
        //                CreationDate = a.CreationDate.ToString(),
        //                CreatedBy = a.CreatedBy,
        //                CreatoreName = a.CreatedByNavigation.FirstName + a.CreatedByNavigation?.MiddleName + " " + a.CreatedByNavigation.LastName,
        //                TaskCategory = a.TaskCategory,
        //                /* GroupReply = a.GroupReply,
        //                TaskPage = a.TaskPage,
        //                TaskUser = a.TaskUser,
        //                TaskTypeName = types.Where(x => x.Id == a.TaskTypeId).Select(x => x.Name).FirstOrDefault(),*/
        //                TaskSubject = a.TaskSubject,
        //                //BranchName = a.BranchId != null? Common.GetBranchName((int)a.BranchId, _Context):null,
        //                CreatorPhoto = a.CreatedByNavigation.PhotoUrl != null ? Globals.baseURL + a.CreatedByNavigation.PhotoUrl : null,
        //                TaskStatus = "",
        //                //TaskCount = 0,
        //                RejectedNo = replies.Where(x => x.TaskId == a.Id && x.Approval == false).Count(),
        //                //TaskCreatorCount = taskCount.Where(x => x.CreatedBy == a.CreatedBy).Count(),
        //                TaskTypeID = a.TaskTypeId,
        //                ProjectID = a.ProjectId,
        //                ProjectSprintID = a.ProjectSprintId,
        //                ProjectWorkFlowID = a.ProjectWorkFlowId,
        //                //taskAttachements = details.Where(x => x.TaskId == a.Id).Select(x => x.CreatorAttachement).FirstOrDefault() != null ? Globals.baseURL + details.Where(x => x.TaskId == a.Id).Select(x => x.CreatorAttachement).FirstOrDefault()?.TrimStart('~') : null,
        //                TaskUserGroupList = receivers.Where(x => x.TaskId == a.Id && x.UserId == userID).Select(x => new TaskUserGroupData()
        //                {
        //                    TaskID = x.TaskId,
        //                    UserGroupID = x.UserId,
        //                    Flag = x.Flag,
        //                    Read = x.Read,
        //                    Star = x.Star
        //                }).ToList()
        //            }).AsQueryable();


        //            if (header.TaskID != 0)
        //            {
        //                tasks = tasks.Where(a => a.ID == header.TaskID).AsQueryable();
        //            }
        //            if (header.TaskTypeID != 0)
        //            {
        //                tasks = tasks.Where(a => a.TaskTypeID == header.TaskTypeID).AsQueryable();
        //            }
        //            if (!string.IsNullOrWhiteSpace(header.Status))
        //            {
        //                tasks = tasks.Where(a => a.Status != null && a.Status == header.Status).AsQueryable();
        //            }
        //            if (!string.IsNullOrWhiteSpace(header.TaskCategory))
        //            {
        //                tasks = tasks.Where(a => a.TaskCategory == header.TaskCategory).AsQueryable();
        //            }
        //            if (!string.IsNullOrWhiteSpace(header.PriorityFilter))
        //            {
        //                tasks = tasks.Where(a => a.Piriority.Trim().ToLower() == header.PriorityFilter.Trim().ToLower()).AsQueryable();
        //            }
        //            if (!string.IsNullOrWhiteSpace(header.TaskName))
        //            {
        //                tasks = tasks.Where(a => a.Name.ToLower().Trim().Contains(header.TaskName.ToLower().Trim())).AsQueryable();
        //            }
        //            if (header.NeedApproval != null)
        //            {
        //                tasks = tasks.Where(a => a.NeedApproval == header.NeedApproval).AsQueryable();
        //            }
        //            if (header.IsFinished != null)
        //            {
        //                tasks = tasks.Where(a => a.IsFinished == header.IsFinished).AsQueryable();
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
        //            if (DateFrom != DateTime.MinValue)
        //            {
        //                tasks = tasks.Where(x => DateTime.Parse(x.CreationDate).Date >= DateFrom.Date).AsQueryable();

        //            }
        //            if (DateTo != DateTime.MinValue)
        //            {
        //                tasks = tasks.Where(x => DateTime.Parse(x.CreationDate).Date <= DateTo.Date).AsQueryable();

        //            }
        //            if (!string.IsNullOrWhiteSpace(header.SearchKey))
        //            {
        //                tasks = tasks.Where(x => x.TaskSubject.Contains(header.SearchKey) || x.Description.Contains(header.SearchKey) || x.Name.Contains(header.SearchKey));
        //            }
        //            if (header.Delayed)
        //            {
        //                tasks = tasks.Where(a => a.Status != null && (a.Status.Trim() == "Waiting Approval" || a.Status.Trim() == "Open") &&
        //                (a.ExpireDate != null ? DateTime.Parse(a.ExpireDate).Date < DateTime.Now.Date : false)).AsQueryable();
        //            }
        //            if (header.Read != null)
        //            {
        //                tasks = tasks.Where(a => a.TaskUserGroupList.Any(z => z.Read == header.Read));
        //            }
        //            if (header.Flag != null)
        //            {
        //                tasks = tasks.Where(a => a.TaskUserGroupList.Any(z => z.Flag == header.Flag));
        //            }
        //            if (header.Star != null)
        //            {
        //                tasks = tasks.Where(a => a.TaskUserGroupList.Any(z => z.Star == header.Star));
        //            }

        //            var taskList = tasks.ToList();
        //            foreach (var a in taskList)
        //            {
        //                var LastTaskStatus = _unitOfWork.TaskUserReplies.FindAll(x => x.TaskId == a.ID).OrderBy(x => x.CreationDate).LastOrDefault();
        //                if (LastTaskStatus != null)
        //                {
        //                    if (LastTaskStatus.IsFinished == null && LastTaskStatus.Approval == null)
        //                    {
        //                        a.TaskStatus = "Need Approval";
        //                    }
        //                    else if (LastTaskStatus.IsFinished == true)
        //                    {
        //                        a.TaskStatus = "Waiting Approval";
        //                    }
        //                    else if (LastTaskStatus.Approval != null && LastTaskStatus.Approval == false)
        //                    {
        //                        a.TaskStatus = "Rejected";
        //                    }
        //                    else if (LastTaskStatus.Approval != null && LastTaskStatus.Approval == true)
        //                    {
        //                        a.TaskStatus = "Approved";
        //                    }
        //                }

        //            }
        //            var taskListPaged = PagedList<GetTaskIndex>.Create(taskList.OrderByDescending(a => DateTime.Parse(a.CreationDate)).AsQueryable(), header.CurrentPage, header.NumberOfItemsPerPage);
        //            response.TasksList = taskListPaged;
        //            response.TaskCount = taskList.Count();
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


        public GetTaskUserAndGroupResponse GetTaskDetails(long UserID, long TaskID = 0)
        {
            GetTaskUserAndGroupResponse response = new GetTaskUserAndGroupResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                if (response.Result)
                {

                    var Task = _unitOfWork.Tasks.FindAll(a => a.Id == TaskID, new[] { "Project.ProjectAssignUsers", "CreatedByNavigation", "Branch", "Project.ProjectAssignUsers.User" }).FirstOrDefault(); //Include(a => a.Project.ProjectAssignUsers).ThenInclude(a => a.User).Where(a => a.Id == TaskID).FirstOrDefault();
                    if (Task == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Task is not found";
                        response.Errors.Add(error);
                        return response;
                    }
                    var Progresses = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.TaskId == Task.Id).ToList();
                    var expenses = _unitOfWork.TaskExpensis.FindAll(a => a.TaskId == Task.Id).ToList();
                    if (Task != null)
                    {
                        var replies = _unitOfWork.TaskUserReplies.FindAll(x => x.TaskId == TaskID).ToList();
                        var details = _unitOfWork.TaskDetails.FindAll(x => x.TaskId == TaskID).ToList();
                        var types = _unitOfWork.TaskTypes.GetAll();
                        var taskCount = _unitOfWork.Tasks.GetAll();   //to be Edited
                        var receivers = _unitOfWork.TaskFlagsOwnerRecievers.FindAll(x => x.TaskId == TaskID).ToList();
                        var CreatorAttachement = details.Where(x => x.TaskId == Task.Id).Select(x => x.CreatorAttachement).FirstOrDefault();
                        var TaskData = new GetTaskData
                        {
                            ID = Task.Id,
                            Active = Task.Active,
                            ProjectId = Task.ProjectId,
                            ProjectSprintId = Task.ProjectSprintId,
                            ProjectWorkFlowId = Task.ProjectWorkFlowId,
                            IsFinished = replies.Where(x => x.TaskId == Task.Id).Select(x => x.IsFinished).FirstOrDefault(),
                            NeedApproval = details.Where(x => x.TaskId == Task.Id).Select(x => x.NeedApproval).FirstOrDefault(),
                            IsTaskOwner = Task.CreatedBy == UserID,
                            Piriority = details.Where(x => x.TaskId == Task.Id).Select(x => x.Priority).FirstOrDefault(),
                            Status = details.Where(x => x.TaskId == Task.Id).Select(x => x.Status).FirstOrDefault(),
                            Name = Task.Name,
                            Description = Task.Description,
                            ExpireDate = Task.ExpireDate?.ToString(),
                            CreationDate = Task.CreationDate.ToString(),
                            CreatedBy = Task.CreatedBy,
                            CreatoreName = Task.CreatedByNavigation.FirstName + " " + Task.CreatedByNavigation.MiddleName + " " + Task.CreatedByNavigation.LastName,
                            TaskCategory = Task.TaskCategory,
                            GroupReply = Task.GroupReply,
                            TaskPage = Task.TaskPage,
                            TaskUser = Task.TaskUser,
                            TaskTypeName = types.Where(x => x.Id == Task.TaskTypeId).Select(x => x.Name).FirstOrDefault(),
                            TaskSubject = Task.TaskSubject,
                            BranchName = Task.BranchId != null ? Task?.Branch?.Name : null,
                            CreatorPhoto = Task.CreatedByNavigation.PhotoUrl != null ? Globals.baseURL + Task.CreatedByNavigation.PhotoUrl : null,
                            TaskStatus = "",
                            TaskCount = 0,
                            RejectedNo = replies.Where(x => x.TaskId == Task.Id && x.Approval == false).Count(),
                            TaskCreatorCount = taskCount.Where(x => x.CreatedBy == Task.CreatedBy).Count(),
                            TaskTypeID = Task.TaskTypeId,
                            taskAttachements = CreatorAttachement != null ? Globals.baseURL + CreatorAttachement?.TrimStart('~') : null,
                            taskAttachementName = "",
                            TaskUserGroupList = receivers.Where(x => x.TaskId == Task.Id && x.UserId == UserID).Select(x => new TaskUserGroupData()
                            {
                                TaskID = x.TaskId,
                                UserGroupID = x.UserId,
                                Flag = x.Flag,
                                Read = x.Read,
                                Star = x.Star
                            }).ToList(),
                            ScreenMonitoring = details.Where(x => x.TaskId == Task.Id).Select(x => x.ScreenMonitoring).FirstOrDefault(),
                            AllowTime = details.Where(x => x.TaskId == Task.Id).Select(x => x.AllowTime).FirstOrDefault(),
                            ProjectBudget = details.Where(x => x.TaskId == Task.Id).Select(x => x.ProjectBudget).FirstOrDefault(),
                            Currency = details.Where(x => x.TaskId == Task.Id).Select(x => x.Currency).FirstOrDefault(),
                            Weight = details.Where(x => x.TaskId == Task.Id).Select(x => x.Weight).FirstOrDefault(),
                            ProjectManagers = Task.Project?.ProjectAssignUsers.Where(a => a.RoleId == 146).Select(a => new ProjectManagerAndAdminModel() { Id = a.User.Id, Name = a.User.FirstName + " " + a.User.LastName }).ToList(),
                            ProjectAdmins = Task.Project?.ProjectAssignUsers.Where(a => a.RoleId == 145).Select(a => new ProjectManagerAndAdminModel() { Id = a.User.Id, Name = a.User.FirstName + " " + a.User.LastName }).ToList(),
                            ApprovedProgressTotalHours = Progresses.Where(a => a.WorkingHoursApproval == true).Select(a => a.TotalHours).Sum(),
                            PendingProgressTotalHours = Progresses.Where(a => a.WorkingHoursApproval == null).Select(a => a.TotalHours).Sum(),
                            RejectedProgressTotalHours = Progresses.Where(a => a.WorkingHoursApproval == false).Select(a => a.TotalHours).Sum(),
                            ApprovedTaskExpenses = expenses.Where(a => a.Approved == true).Select(a => a.Amount).Sum(),
                            PendingTaskExpenses = expenses.Where(a => a.Approved == null).Select(a => a.Amount).Sum(),
                            RejectedTaskExpenses = expenses.Where(a => a.Approved == false).Select(a => a.Amount).Sum(),
                            CheckIn = Progresses.Where(a => a.CheckOutTime == null && a.Date.Date == DateTime.Now.Date).OrderBy(a => a.Id).LastOrDefault()?.CheckInTime,
                            UnitRateService = Task?.Project?.UnitRateService,
                            taskAttachementExtension = CreatorAttachement != null ? Path.GetExtension(Globals.baseURL + CreatorAttachement?.TrimStart('~')) : null,
                        };
                        response.TaskData = TaskData;
                        var taskPermissions = _unitOfWork.TaskPermissions.FindAll(x => x.TaskId == TaskID).ToList();

                        var Users = taskPermissions.Where(a => a.IsGroup == false).ToList();
                        var usersIDs = taskPermissions.Where(a => a.IsGroup == false).Select(a => a.UserGroupId).ToList();
                        var usersData = _unitOfWork.Users.FindAll(a => usersIDs.Contains(a.Id)).ToList();

                        var groups = taskPermissions.Where(a => a.IsGroup == true).ToList();
                        var groupsIDs = taskPermissions.Where(a => a.IsGroup == true).Select(a => a.Id).ToList();
                        var groupsData = _unitOfWork.Groups.FindAll(a => groupsIDs.Contains(a.Id)).ToList();

                        var usersFinalList = Users.Select(a => new TaskUserData()
                        {
                            UserID = a.UserGroupId,
                            UserName = usersData.Where(x => x.Id == a.UserGroupId).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault(),
                            UserPhoto = usersData.Where(x => x.Id == a.UserGroupId).Select(a => a.PhotoUrl).FirstOrDefault(),
                            IsCreator = _unitOfWork.Tasks.FindAll(x => x.Id == a.TaskId).FirstOrDefault()?.CreatedBy == a.UserGroupId,
                            Flag = receivers.Where(x => x.TaskId == TaskID && x.UserId == a.UserGroupId).Select(x => x.Flag ?? false).FirstOrDefault()
                        }).ToList();
                        response.TaskUsers = usersFinalList;

                        var groupsFinalList = groups.Select(a => new TaskGroupData()
                        {
                            GroupID = a.UserGroupId,
                            GroupName = groupsData.Where(x => x.Id == a.UserGroupId).Select(b => b.Name).FirstOrDefault(),
                            IsGroup = true,
                        }).ToList();
                        var groupsFinalListIDs = groups.Select(a => a.Id).ToList();

                        var groupUsers = _unitOfWork.GroupUsers.FindAll(a => groupsFinalListIDs.Contains(a.GroupId));
                        foreach (var group in groupsFinalList)
                        {
                            var users = groupUsers.Where(a => a.GroupId == group.GroupID).ToList();
                            group.TaskGroupUsersList = users.Select(a => new TaskUserData()
                            {
                                UserID = (long)a.UserId,
                                UserName = _unitOfWork.Users.FindAll(x => x.Id == a.UserId).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault(),
                                UserPhoto = _unitOfWork.Users.FindAll(x => x.Id == a.UserId).Select(a => a.PhotoUrl).FirstOrDefault(),
                                IsCreator = _unitOfWork.Tasks.FindAll(a => a.Id == TaskID).FirstOrDefault()?.CreatedBy == a.UserId,
                                Flag = receivers.Where(x => x.TaskId == TaskID && x.UserId == a.UserId).Select(x => x.Flag ?? false).FirstOrDefault()
                            }).ToList();
                        }
                        response.TaskGroups = groupsFinalList;

                        var groupUsersOnly = groupsFinalList.SelectMany(a => a.TaskGroupUsersList).ToList();
                        List<TaskUserData> UsersOnly = new List<TaskUserData>();
                        UsersOnly.AddRange(groupUsersOnly);
                        UsersOnly.AddRange(usersFinalList);
                        response.TaskUsersAndGroups = UsersOnly.Distinct().ToList();
                        /*bool IsTaskCreator = false;
                        var TaskCreatorCheck = _Context.Tasks.Where(x => x.CreatedBy == validation.userID && x.Id == TaskID).FirstOrDefault();

                        if (TaskCreatorCheck != null)
                        {
                            IsTaskCreator = true;
                        }

                            var TaskUsersAndGroupsList = _Context.TaskPermissions.Where(x => x.TaskId == TaskID).ToList();

                            if (TaskUsersAndGroupsList != null)
                            {
                                foreach (var item in TaskUsersAndGroupsList)
                                {
                                    if (item.UserGroupId != 0 && item.IsGroup == true)
                                    {
                                        var TaskGroupDetailsObj = new TaskGroupData();
                                        TaskGroupDetailsObj.GroupID = item.UserGroupId;
                                        TaskGroupDetailsObj.GroupName = Common.GetGroupName(item.UserGroupId,_Context);

                                        var UsersOfGroupList = _Context.GroupUsers.Where(x => x.GroupId == item.UserGroupId).ToList();
                                        if (UsersOfGroupList != null && UsersOfGroupList.Count > 0)
                                        {
                                            var GrpUsrs = new List<TaskUserData>();

                                            foreach (var UserOfGroup in UsersOfGroupList)
                                            {
                                                var UsersGroupData = new TaskUserData();
                                                UsersGroupData.UserID = UserOfGroup.UserId;
                                                UsersGroupData.UserName = Common.GetUserName(UserOfGroup.UserId,_Context);
                                                UsersGroupData.UserPhoto = Globals.baseURL + Common.GetUserPhoto(UserOfGroup.UserId, _Context);
                                                    *//*"/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(UsersGroupData.UserID.ToString(), key)) + "&type=photo&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();*//*


                                                GrpUsrs.Add(UsersGroupData);



                                            }
                                            TaskGroupDetailsObj.TaskGroupUsersList = GrpUsrs;
                                            TaskGroupDataList.Add(TaskGroupDetailsObj);
                                        }



                                    }

                                    if (item.UserGroupId != 0 && item.IsGroup == false)
                                    {
                                        var TaskUserDetailsObj = new TaskUserData();

                                        TaskUserDetailsObj.UserID = item.UserGroupId;
                                        TaskUserDetailsObj.UserName = Common.GetUserName(item.UserGroupId,_Context);
                                        TaskUserDetailsObj.UserPhoto = Globals.baseURL + Common.GetUserPhoto(item.UserGroupId, _Context);
                                           *//* "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(item.UserGroupId.ToString(), key)) + "&type=photo&CompName=" + Request.Headers["CompanyName"].ToString().ToLower(); ;*//*
                                        TaskUserDataList.Add(TaskUserDetailsObj);
                                    }

                                }

                            }





                        response.GetTaskGroupList = TaskGroupDataList;
                        response.GetTaskUserList = TaskUserDataList;
                        response.IsTaskCreator = IsTaskCreator;*/
                        response.IsTaskCreator = UserID == Task.CreatedBy;

                    }

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

        public GetTaskReplyResponse GetTaskReply([FromHeader] GetTaskReplyHeader header)
        {
            var response = new GetTaskReplyResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                #region
                //var task = _unitOfWork.Tasks.FindAll(a => a.Id == header.TaskID).FirstOrDefault();
                //if (task == null)
                //{
                //    response.Result = false;
                //    Error err = new Error();
                //    err.ErrorCode = "Err101";
                //    err.errorMSG = "No Task with this ID";
                //    response.Errors.Add(err);
                //    return response;
                //}
                #endregion

                var GetTaskReplyDataList = new List<GetTaskReplysData>();

                if (response.Result)
                {
                    /*int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 10;
                    if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }



                    long TaskID = 0;
                    if (!string.IsNullOrEmpty(headers["TaskID"]) && long.TryParse(headers["TaskID"], out TaskID))
                    {
                        long.TryParse(headers["TaskID"], out TaskID);
                    }
                    string CommentSearch = null;
                    if (!string.IsNullOrEmpty(headers["CommentSearch"]))
                    {
                        CommentSearch = headers["CommentSearch"];
                    }*/

                    if (response.Result)
                    {
                        Expression<Func<TaskUserReply, bool>> creatria = (a => true);

                        creatria = a =>
                        (
                        (header.TaskID != 0 ? a.TaskId == header.TaskID : true) &&
                        (header.CommentSearch != null ? a.CommentReply.Contains(header.CommentSearch) : true)
                        );


                        var GetTaskUserReplyList = _unitOfWork.TaskUserReplies.FindAllPaging(creatria, header.CurrentPage, header.NumberOfItemsPerPage, new[] { "CreatedByNavigation" },
                                                        orderBy: a => a.Id, orderByDirection: ApplicationConsts.OrderByDescending);

                        //var GetTaskUserReplyList = PagedList<TaskUserReply>.Create(GetTaskUserReplyDB.OrderByDescending(x => x.Id), header.CurrentPage, header.NumberOfItemsPerPage);

                        response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = header.CurrentPage,
                            TotalPages = GetTaskUserReplyList.TotalPages,
                            ItemsPerPage = header.NumberOfItemsPerPage,
                            TotalItems = GetTaskUserReplyList.TotalCount
                        };


                        if (GetTaskUserReplyList != null)
                        {
                            var TaskStatus = _unitOfWork.TaskDetails.FindAll(x => x.TaskId == header.TaskID).FirstOrDefault();

                            foreach (var GetTaskUserReplyOBJ in GetTaskUserReplyList)
                            {

                                var GetEmployeeResponse = new GetTaskReplysData();

                                GetEmployeeResponse.ID = (int)GetTaskUserReplyOBJ.Id;
                                GetEmployeeResponse.Active = GetTaskUserReplyOBJ.Active;
                                GetEmployeeResponse.Approval = GetTaskUserReplyOBJ.Approval;
                                GetEmployeeResponse.ApprovalComment = GetTaskUserReplyOBJ.ApprovalComment;
                                GetEmployeeResponse.CommentReply = GetTaskUserReplyOBJ.CommentReply;
                                if (TaskStatus != null)
                                {

                                    GetEmployeeResponse.TaskStatus = TaskStatus.Status;
                                }
                                GetEmployeeResponse.CreatedBy = GetTaskUserReplyOBJ.CreatedBy;
                                GetEmployeeResponse.CreatorName = GetTaskUserReplyOBJ.CreatedByNavigation.FirstName + " " + GetTaskUserReplyOBJ.CreatedByNavigation.LastName;
                                if (GetTaskUserReplyOBJ.CreatorAttach != null)
                                {
                                    GetEmployeeResponse.CreatorAttach = Globals.baseURL + GetTaskUserReplyOBJ.CreatorAttach;
                                }
                                if (GetTaskUserReplyOBJ.CreatedByNavigation.PhotoUrl != null)
                                {
                                    GetEmployeeResponse.CreatorPhoto = Globals.baseURL + GetTaskUserReplyOBJ.CreatedByNavigation.PhotoUrl;
                                    /*"/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(GetTaskUserReplyOBJ.CreatedBy.ToString(), key)) + "&type=photo&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();*/
                                }
                                GetEmployeeResponse.CreationDate = GetTaskUserReplyOBJ.CreationDate.ToString();
                                if (GetTaskUserReplyOBJ.ReplyAttach != null)
                                {
                                    GetEmployeeResponse.ReplyAttach = Globals.baseURL + GetTaskUserReplyOBJ.ReplyAttach;

                                }
                                GetEmployeeResponse.IsFinished = GetTaskUserReplyOBJ.IsFinished;
                                GetEmployeeResponse.RecieverUserID = GetTaskUserReplyOBJ.RecieverUserId;
                                GetEmployeeResponse.TaskID = GetTaskUserReplyOBJ.TaskId;








                                GetTaskReplyDataList.Add(GetEmployeeResponse);
                            }



                        }

                    }

                    response.GetTaskReplyList = GetTaskReplyDataList;

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

        public async Task<BaseResponseWithId<long>> AddTask(AddTaskObjData request, long UserID, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var AddTaskOBJ = new AddTaskObjData();



                if (Response.Result)
                {
                    if (request.Name == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Task Name.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (request.TaskUser == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Task user Name.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (request.TaskTypeID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Task Type ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    #region check Data in the DB
                    if (request.ProjectID != null)
                    {
                        var project = _unitOfWork.Projects.FindAll(a => a.Id == request.ProjectID).FirstOrDefault();
                        if (project == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "No Project with this ID.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    if (request.ProjectSprintId != null)
                    {
                        var project = _unitOfWork.ProjectSprints.FindAll(a => a.Id == request.ProjectSprintId).FirstOrDefault();
                        if (project == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "No Project Sprint with this ID.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    if (request.ProjectWorkflowId != null)
                    {
                        var project = _unitOfWork.ProjectWorkFlows.FindAll(a => a.Id == request.ProjectWorkflowId).FirstOrDefault();
                        if (project == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "No Project Work Flow with this ID.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    #endregion

                    if (Response.Result)
                    {

                        // Insert
                        //ObjectParameter TaskID = new ObjectParameter("ID", typeof(int));

                        var BranchID = _unitOfWork.Users.FindAll(x => x.Id == UserID).Select(x => x.BranchId).FirstOrDefault();






                        var TaskDB = new NewGaras.Infrastructure.Entities.Task();

                        TaskDB.Name = request.Name;
                        TaskDB.Description = request.Description;
                        TaskDB.Active = request.Active;
                        TaskDB.BranchId = request.BranchID;
                        if (!string.IsNullOrEmpty(request.ExpireDate))
                        {
                            TaskDB.ExpireDate = DateTime.Parse(request.ExpireDate);
                        }
                        TaskDB.TaskUser = request.TaskUser;
                        TaskDB.TaskTypeId = request.TaskTypeID;
                        TaskDB.RefrenceNumber = "";
                        TaskDB.TaskCategory = request.TaskCategory;
                        TaskDB.TaskSubject = request.TaskSubject;
                        TaskDB.TaskPage = request.TaskPage;
                        TaskDB.GroupReply = request.GroupReply;
                        TaskDB.CreationDate = DateTime.Now;
                        TaskDB.CreatedBy = UserID;
                        TaskDB.ModifiedDate = DateTime.Now;
                        TaskDB.ModifiedBy = UserID;
                        if (request.ProjectID != null) TaskDB.ProjectId = request.ProjectID;
                        if (request.ProjectSprintId != null) TaskDB.ProjectSprintId = request.ProjectSprintId;
                        if (request.ProjectWorkflowId != null) TaskDB.ProjectWorkFlowId = request.ProjectWorkflowId;



                        _unitOfWork.Tasks.Add(TaskDB);
                        var Res = _unitOfWork.Complete();


                        if (Res > 0)
                        {

                            Response.ID = TaskDB.Id;
                        }
                        #region comments
                        //var TaskIDInsertID = long.Parse(TaskID.Value.ToString());
                        //List<long> IDSGroupForFlagsList = new List<long>();
                        //if (TaskDB.ID > 0)
                        //{

                        //    if (Request.AddGroupIDsData.Count() > 0)
                        //    {



                        //        foreach (var TaskGroup in Request.AddGroupIDsData)
                        //        {
                        //            var GroupUsers = _Context.Group_User.Where(x => x.GroupID == TaskGroup.GroupID).Select(x => x.UserID).ToList();

                        //            foreach (var UsersIDs in GroupUsers)
                        //            {

                        //                ObjectParameter TaskFlagsID = new ObjectParameter("ID", typeof(int));
                        //                var TaskFlagsIDInsertion = _Context.proc_TaskFlagsOwnerRecieverInsert(
                        //                  TaskFlagsID,
                        //                  TaskDB.ID,
                        //                  UsersIDs,
                        //                  false,
                        //                  Request.Flag,
                        //                  false

                        //             );
                        //            }




                        //                IDSGroupForFlagsList.Add(TaskGroup.GroupID);
                        //        }
                        //    }

                        //}
                        //else
                        //{
                        //    Response.Result = false;
                        //    Error error = new Error();
                        //    error.ErrorCode = "Err25";
                        //    error.ErrorMSG = "Faild To Insert this Task Flags !!";
                        //    Response.Errors.Add(error);
                        //}


                        //if (Request.AddUserIDData != null)
                        //{
                        //    if (Request.AddUserIDData.Count() > 0)
                        //    {


                        //        foreach (var TaskUser in Request.AddUserIDData)
                        //        {
                        //            if (!Common.CheckUserIsGroupUser(TaskUser.UserID, IDSGroupForFlagsList))
                        //            {
                        //                ObjectParameter TasUserID = new ObjectParameter("ID", typeof(long));
                        //                var TasUserIDInsertion = _Context.proc_TaskFlagsOwnerRecieverInsert(TasUserID,
                        //              TaskDB.ID,
                        //              TaskUser.UserID,
                        //              false,
                        //              Request.Flag,
                        //              false

                        //                 );




                        //            }
                        //        }
                        //    }
                        //}








                        #endregion





                        if (TaskDB.Id > 0)
                        {
                            //ObjectParameter TaskIDEncrement = new ObjectParameter("ID", typeof(int));
                            string FilePath = null;
                            //var TaskIDInsertID = long.Parse(TaskID.Value.ToString());
                            //Response.ID = TaskIDInsertID;
                            if (request.taskAttachements != null)
                            {
                                FilePath = await Common.SaveFileAsync("Attachments/" + CompanyName + "/Tasks" + "/", request.taskAttachements.FileContent, request.taskAttachements.FileName, request.taskAttachements.FileExtension, _host);

                            }
                            // Insert
                            var TaskDetailsInsertion = _unitOfWork.TaskDetails.Add(new TaskDetail()
                            {
                                TaskId = TaskDB.Id,
                                Status = request.Status,
                                Priority = request.Priority?.Trim(),
                                NeedApproval = request.NeedApproval,
                                CreatorAttachement = FilePath
                            ,
                                ScreenMonitoring = request.ScreenMonitoring,
                                AllowTime = request.AllowTime,
                                ProjectBudget = (long)request.ProjectBudget,
                                Currency = request.Currency,
                                Weight = request.Weight
                            });
                            _unitOfWork.Complete();

                        }

                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Insert this Task Details !!";
                            Response.Errors.Add(error);
                        }









                        List<long> IDSGroupList = new List<long>();
                        if (TaskDB.Id > 0)
                        {
                            var TaskIDReturn = TaskDB.Id;

                            Response.ID = TaskIDReturn;

                            if (request.AddGroupIDsData != null)
                            {
                                if (request.AddGroupIDsData.Count() > 0)
                                {
                                    foreach (var TaskGroup in request.AddGroupIDsData)
                                    {
                                        //ObjectParameter TaskGroupID = new ObjectParameter("ID", typeof(long));
                                        var TaskGroupInsertion = _unitOfWork.TaskPermissions.Add(new TaskPermission()
                                        {
                                            TaskId = TaskDB.Id,
                                            UserGroupId = TaskGroup.GroupID,
                                            PermissionLevelId = TaskGroup.PermissionLevelID,
                                            IsGroup = true
                                        });
                                        _unitOfWork.Complete();
                                        //   var FlagCheck = _Context.TaskFlagsOwnerRecievers.Where(x => x.TaskID == TaskDB.ID && x.UserID == TaskGroup.GroupID).FirstOrDefault();


                                        var GroupUser = _unitOfWork.GroupUsers.FindAll(x => x.GroupId == TaskGroup.GroupID).Select(x => x.UserId).ToList();

                                        foreach (var userID in GroupUser)
                                        {
                                            if (_unitOfWork.TaskFlagsOwnerRecievers.FindAll(a => a.TaskId == TaskDB.Id && a.UserId == userID).FirstOrDefault() == null)
                                            {
                                                var TaskUserFlags = new TaskFlagsOwnerReciever();
                                                TaskUserFlags.TaskId = TaskDB.Id;
                                                TaskUserFlags.UserId = (long)userID;
                                                TaskUserFlags.Read = false;
                                                TaskUserFlags.Flag = false;
                                                TaskUserFlags.Star = false;


                                                _unitOfWork.TaskFlagsOwnerRecievers.Add(TaskUserFlags);
                                                _unitOfWork.Complete();
                                            }

                                        }



                                        IDSGroupList.Add(TaskGroup.GroupID);
                                    }
                                }
                            }
                            _unitOfWork.Complete();

                            if (request.AddUserIDData != null)
                            {
                                if (request.AddUserIDData.Count() > 0)
                                {
                                    foreach (var TaskUser in request.AddUserIDData)
                                    {
                                        //if (!Common.CheckUserIsGroupUser(TaskUser.UserID, IDSGroupList, _Context))  //for periti group only because there is no group users in periti
                                        /*if(_unitOfWork.GroupUsers.FindAll(x => x.UserId == TaskUser.UserID && IDSGroupList.Contains(x.GroupId)).FirstOrDefault() != null)
                                        {*/
                                        if (_unitOfWork.TaskPermissions.FindAll(x => x.TaskId == TaskIDReturn && x.UserGroupId == UserID).FirstOrDefault() == null)
                                        {
                                            var TasUserIDInsertion = _unitOfWork.TaskPermissions.Add(new TaskPermission()
                                            {
                                                TaskId = TaskDB.Id,
                                                UserGroupId = TaskUser.UserID,
                                                PermissionLevelId = TaskUser.PermissionLevelID,
                                                IsGroup = false
                                            });
                                            _unitOfWork.Complete();
                                        }
                                        var FlagCheck = _unitOfWork.TaskFlagsOwnerRecievers.FindAll(x => x.TaskId == TaskDB.Id && x.UserId == TaskUser.UserID).FirstOrDefault();
                                        if (FlagCheck == null)
                                        {
                                            var TaskUserFlags = new TaskFlagsOwnerReciever();
                                            TaskUserFlags.TaskId = TaskDB.Id;
                                            TaskUserFlags.UserId = TaskUser.UserID;
                                            TaskUserFlags.Read = false;
                                            TaskUserFlags.Flag = false;
                                            TaskUserFlags.Star = false;

                                            _unitOfWork.TaskFlagsOwnerRecievers.Add(TaskUserFlags);
                                            _unitOfWork.Complete();
                                        }

                                        //}
                                    }
                                }
                            }
                            _unitOfWork.Complete();

                            //if (!Common.CheckUserIsGroupUser((int)validation.userID, IDSGroupList, _Context))
                            if (_unitOfWork.GroupUsers.FindAll(x => x.UserId == UserID && IDSGroupList.Contains(x.GroupId)).FirstOrDefault() != null)
                            {
                                if (_unitOfWork.TaskPermissions.FindAll(x => x.TaskId == TaskIDReturn && x.UserGroupId == UserID).FirstOrDefault() == null)
                                {
                                    //ObjectParameter TasUserID = new ObjectParameter("ID", typeof(long));
                                    /*var TasUserIDInsertion = _Context.proc_TaskPermissionInsert(TasUserID,
                                    TaskDB.ID,
                                    TaskUser.UserID,
                                    TaskUser.PermissionLevelID,
                                    false

                                     );*/
                                    var TasUserIDInsertion = _unitOfWork.TaskPermissions.Add(new TaskPermission()
                                    {
                                        TaskId = TaskDB.Id,
                                        UserGroupId = UserID,
                                        PermissionLevelId = 6,
                                        IsGroup = false
                                    });
                                }
                                var FlagCheck = _unitOfWork.TaskFlagsOwnerRecievers.FindAll(x => x.TaskId == TaskDB.Id && x.UserId == UserID).FirstOrDefault();
                                if (FlagCheck == null)
                                {
                                    var TaskUserFlags = new TaskFlagsOwnerReciever();
                                    TaskUserFlags.TaskId = TaskDB.Id;
                                    TaskUserFlags.UserId = UserID;
                                    TaskUserFlags.Read = false;
                                    TaskUserFlags.Flag = false;
                                    TaskUserFlags.Star = false;

                                    _unitOfWork.TaskFlagsOwnerRecievers.Add(TaskUserFlags);
                                    _unitOfWork.Complete();
                                }

                            }


                            _unitOfWork.Complete();


                        }

                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Insert this Task !!";
                            Response.Errors.Add(error);
                        }






                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Faild To Insert this Task !!";
                        Response.Errors.Add(error);
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

        public async Task<BaseResponseWithId<long>> AddTaskRequirements(TaskRequirementModel request, long UserID)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var TaskDetails = new GetTaskDetailsData();


                if (Response.Result)
                {

                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.TaskId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "TaskId is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var CheckTask = _unitOfWork.Tasks.FindAll(x => x.Id == request.TaskId, includes: new[] { "WorkingHourseTrackings" }).FirstOrDefault();
                    if (CheckTask == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This Task is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.RequirementList == null || request.RequirementList.Count() <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "must be insert at least one";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var CheckTotalPercent = request.RequirementList.Where(x => x.Active == true).Select(x => x.Percentage).Sum();
                    if (CheckTotalPercent != 100)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "must be Total Percent equal 100%";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {
                        var progress = CheckTask.WorkingHourseTrackings.Select(a => a.ProgressRate).Max();
                        var TaskRequirmentList = new List<TaskRequirement>();
                        var count = (decimal)0;
                        foreach (var item in request.RequirementList)
                        {
                            if (item.Id != 0)
                            {
                                var Requirment = _unitOfWork.TaskRequirements.GetById(item.Id);
                                if (Requirment != null && item.Active)
                                {
                                    Requirment.TaskId = request.TaskId;
                                    Requirment.Name = item.Name;
                                    Requirment.Percentage = item.Percentage;
                                    count += Requirment.Percentage;
                                    if (count <= progress)
                                    {
                                        Requirment.IsFinished = true;
                                    }
                                    else
                                    {
                                        Requirment.IsFinished = false;
                                    }
                                    Requirment.Active = true;
                                    Requirment.ModifiedBy = UserID;
                                    Requirment.ModifiedDate = DateTime.Now;
                                    _unitOfWork.TaskRequirements.Update(Requirment);
                                    //_unitOfWork.Complete();
                                }
                                else if (Requirment != null && !item.Active)
                                {
                                    _unitOfWork.TaskRequirements.Delete(Requirment);
                                    //_unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = $"Requirment at index {request.RequirementList.IndexOf(item)} is not found";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {

                                var Requirment = new TaskRequirement();
                                Requirment.TaskId = request.TaskId;
                                Requirment.Name = item.Name;
                                Requirment.Percentage = item.Percentage;
                                count += Requirment.Percentage;
                                if (count <= progress)
                                {
                                    Requirment.IsFinished = true;
                                }
                                else
                                {
                                    Requirment.IsFinished = false;
                                }
                                Requirment.Active = true;
                                Requirment.CreatedBy = UserID;
                                Requirment.ModifiedBy = UserID;
                                Requirment.CreationDate = DateTime.Now;
                                Requirment.ModifiedDate = DateTime.Now;
                                //TaskRequirmentList.Add(Requirment);
                                await _unitOfWork.TaskRequirements.AddAsync(Requirment);
                                //_unitOfWork.Complete();

                            }

                        }
                        //_Context.TaskRequirements.AddRange(TaskRequirmentList);

                        _unitOfWork.Complete();
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err25";
                    error.ErrorMSG = "Faild To Insert this Task !!";
                    Response.Errors.Add(error);
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

        public async Task<BaseResponseWithData<List<RequirementModel>>> GetTaskRequirmentsList(long TaskId)
        {
            BaseResponseWithData<List<RequirementModel>> response = new BaseResponseWithData<List<RequirementModel>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var GetTaskCategoryDDLList = new List<RequirementModel>();

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
                    var TaskRequirementsList = await _unitOfWork.TaskRequirements.FindAllAsync(x => x.TaskId == TaskId);


                    var TaskRequirementsListDB = TaskRequirementsList.Select(item => new RequirementModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Percentage = item.Percentage,
                        IsFinish = item.IsFinished,
                        CreationDate = item.CreationDate.ToShortDateString(),
                    }).ToList();


                    response.Data = TaskRequirementsListDB;
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

        public async Task<BaseResponseWithId<long>> EditTask(AddTaskObjData request, long UserId, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.ID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Task ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {

                            var UpdateTaskDB = _unitOfWork.Tasks.FindAll(x => x.Id == request.ID).FirstOrDefault();
                            var UpdateTaskDetailsDB = _unitOfWork.TaskDetails.FindAll(x => x.TaskId == request.ID).FirstOrDefault();

                            if (UpdateTaskDB != null)
                            {
                                // Update

                                if (UpdateTaskDB != null)
                                {
                                    UpdateTaskDB.Name = request.Name;
                                    UpdateTaskDB.Description = request.Description;
                                    UpdateTaskDB.Active = request.Active;
                                    UpdateTaskDB.BranchId = request.BranchID;
                                    if (!string.IsNullOrWhiteSpace(request.ExpireDate))
                                    {
                                        UpdateTaskDB.ExpireDate = DateTime.Parse(request.ExpireDate);

                                    }
                                    UpdateTaskDB.TaskUser = request.TaskUser;
                                    UpdateTaskDB.BranchId = request.TaskTypeID;
                                    UpdateTaskDB.RefrenceNumber = "";
                                    UpdateTaskDB.TaskCategory = request.TaskCategory;
                                    UpdateTaskDB.TaskSubject = request.TaskSubject;
                                    UpdateTaskDB.TaskPage = request.TaskPage;
                                    UpdateTaskDB.GroupReply = request.GroupReply;
                                    if (request.DeleteAttachment == true && UpdateTaskDetailsDB.CreatorAttachement != null)
                                    {
                                        var oldpath = Path.Combine(_host.WebRootPath + '/' + UpdateTaskDetailsDB.CreatorAttachement);
                                        if (System.IO.File.Exists(oldpath))
                                        {
                                            System.IO.File.Delete(oldpath);
                                            UpdateTaskDetailsDB.CreatorAttachement = null;
                                        }
                                    }

                                    if (request.taskAttachements != null)
                                    {
                                        if (UpdateTaskDetailsDB.CreatorAttachement != null)
                                        {
                                            var oldpath = Path.Combine(_host.WebRootPath + '/' + UpdateTaskDetailsDB.CreatorAttachement);
                                            if (System.IO.File.Exists(oldpath))
                                            {
                                                System.IO.File.Delete(oldpath);
                                                UpdateTaskDetailsDB.CreatorAttachement = null;
                                            }
                                        }
                                        //ObjectParameter TaskIDEncrement = new ObjectParameter("ID", typeof(int));
                                        //var CompanyName = Request.Headers["CompanyName"].ToString().ToLower();
                                        var FilePath = await Common.SaveFileAsync("Attachments/" + CompanyName + "/Tasks" + "/", request.taskAttachements.FileContent, request.taskAttachements.FileName, request.taskAttachements.FileExtension, _host);

                                        // Insert

                                        UpdateTaskDetailsDB.CreatorAttachement = FilePath;

                                    }

                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Task!";
                                    Response.Errors.Add(error);
                                }

                                if (UpdateTaskDetailsDB != null)
                                {
                                    UpdateTaskDetailsDB.NeedApproval = request.NeedApproval;
                                    UpdateTaskDetailsDB.Priority = request.Priority?.Trim();
                                    //   UpdateTaskDetailsDB.CreatorAttachement = Request.CreatorAttachement;
                                    if (request.ScreenMonitoring != null)
                                    {
                                        UpdateTaskDetailsDB.ScreenMonitoring = request.ScreenMonitoring;
                                    }
                                    if (request.AllowTime != null)
                                    {
                                        UpdateTaskDetailsDB.AllowTime = request.AllowTime;
                                    }
                                    if (request.ProjectBudget != null)
                                    {
                                        UpdateTaskDetailsDB.ProjectBudget = (long)request.ProjectBudget;
                                    }
                                    if (request.Currency != null)
                                    {
                                        UpdateTaskDetailsDB.Currency = request.Currency;
                                    }
                                    if (request.Weight != null)
                                    {
                                        UpdateTaskDetailsDB.Weight = request.Weight;
                                    }
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Task Details!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Task Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                            Response.ID = request.ID;


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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithId<long>> AddTaskUserReply(GetTaskReplysData request, long UserID, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var TaskDetails = new GetTaskDetailsData();


                if (Response.Result)
                {

                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    //check sent data
                    if (request.TaskID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Task ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    var IsFinishedCount = _unitOfWork.TaskUserReplies.FindAll(x => x.TaskId == request.TaskID && x.IsFinished == true).FirstOrDefault();
                    var IsTaskNeedAproval = _unitOfWork.TaskDetails.FindAll(x => x.TaskId == request.TaskID).Select(x => x.NeedApproval).FirstOrDefault();


                    if (IsTaskNeedAproval == false && request.Approval != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This Task Doesn't need Aprroval.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {

                        // Insert


                        //ObjectParameter ID = new ObjectParameter("ID", typeof(long));

                        var TaskUserReplyDB = new TaskUserReply();



                        if (IsTaskNeedAproval == true)
                        {
                            TaskUserReplyDB.Approval = request.Approval;
                            TaskUserReplyDB.ApprovalComment = request.ApprovalComment;


                        }
                        TaskUserReplyDB.Active = request.Active;

                        //if (Request.ApprovalComment != null)
                        //{
                        TaskUserReplyDB.CommentReply = request.CommentReply;
                        if (TaskUserReplyDB.CreatedBy == TaskUserReplyDB.RecieverUserId && IsFinishedCount == null)
                        {
                            TaskUserReplyDB.IsFinished = request.IsFinished;
                        }
                        else
                        {
                            TaskUserReplyDB.IsFinished = null;
                        }
                        //}


                        TaskUserReplyDB.CreatedBy = UserID;
                        TaskUserReplyDB.CreationDate = DateTime.Now;

                        //var CompanyName = Request.Headers["CompanyName"].ToString().ToLower();
                        //var CreatorFilePath = Common.SaveFile("/Attachments/" + CompanyName +"/", Request.CreatorFileContent, Request.CreatorFileName, Request.CreatorFileExtension);

                        //TaskUserReplyDB.CreatorAttach = CreatorFilePath;


                        TaskUserReplyDB.RecieverUserId = request.RecieverUserID;

                        TaskUserReplyDB.ModifiedBy = UserID;
                        TaskUserReplyDB.ModifiedDate = DateTime.Now;


                        if (!string.IsNullOrEmpty(request.ReplyFileContent) & !string.IsNullOrEmpty(request.ReplyFileName) & !string.IsNullOrEmpty(request.ReplyFileExtension) & request.ReplyFileContent != " " && request.ReplyFileExtension != " " && request.ReplyFileName != " ")
                        {
                            var ReplyFilePath = Common.SaveFile("Attachments/" + CompanyName + "/", request.ReplyFileContent, request.ReplyFileName, request.ReplyFileExtension, _host);
                            TaskUserReplyDB.ReplyAttach = ReplyFilePath;
                        }



                        TaskUserReplyDB.TaskId = request.TaskID;

                        _unitOfWork.TaskUserReplies.Add(TaskUserReplyDB);
                        var Res = _unitOfWork.Complete();

                        var TaskDetailsDB = _unitOfWork.TaskDetails.FindAll(x => x.TaskId == request.TaskID).FirstOrDefault();
                        var TaskUserReplyHasFinished = _unitOfWork.TaskUserReplies.FindAll(x => x.TaskId == request.TaskID && x.IsFinished == true).FirstOrDefault();
                        //if (Request.TaskStatus != null)
                        //{
                        //    if (TaskDetailsDB != null)
                        //    {

                        //        TaskDetailsDB.Status = Request.TaskStatus;
                        //    }
                        //}

                        if (IsTaskNeedAproval == true && request.TaskStatus == null)
                        {
                            if (request.Approval == true)
                            {


                                if (TaskDetailsDB != null)
                                {
                                    TaskDetailsDB.Status = "Closed";


                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Task Status!!";
                                    Response.Errors.Add(error);
                                }

                            }
                            else if (request.Approval == null && request.TaskStatus == null && TaskUserReplyHasFinished != null)
                            {
                                if (TaskDetailsDB != null)
                                {
                                    TaskDetailsDB.Status = "Waiting Approval";
                                }
                            }
                            else if (request.Approval == null && request.TaskStatus == null)
                            {
                                if (TaskDetailsDB != null)
                                {
                                    TaskDetailsDB.Status = "Open";
                                }
                            }
                            else if (request.Approval == false)
                            {
                                if (TaskUserReplyHasFinished != null)
                                {
                                    TaskUserReplyHasFinished.IsFinished = false;
                                }
                                TaskDetailsDB.Status = "Open";
                            }
                        }
                        else
                        {
                            if (TaskDetailsDB != null)
                            {
                                if (request.TaskStatus != null)
                                {

                                    TaskDetailsDB.Status = request.TaskStatus;
                                }
                            }
                        }

                        _unitOfWork.Complete();

                        if (Res > 0)
                        {
                            Response.ID = TaskUserReplyDB.Id;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Insert this Task Reply !!";
                            Response.Errors.Add(error);
                        }


                        //}
                        //else
                        //{
                        //    Response.Result = false;
                        //    Error error = new Error();
                        //    error.ErrorCode = "Err25";
                        //    error.ErrorMSG = "Faild To Insert this Task Reply Because This Task Doesn't need approval !!";
                        //    Response.Errors.Add(error);
                        //}



                    }








                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err25";
                    error.ErrorMSG = "Faild To Insert this Task !!";
                    Response.Errors.Add(error);
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

        public async Task<BaseResponseWithId<long>> AddDeleteTaskUserGroup(AddDeleteTaskUserGroupRequest request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (request.DeleteAllButOne == true)
                    {
                        if (request.RemainUserGroupId == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Please Insert Remained User Group Id ";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (request.TaskId == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Please Insert Task Id ";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    if (Response.Result)
                    {
                        if (request.DeleteAllButOne == true)
                        {
                            var TaskDb = (await _unitOfWork.Tasks.FindAllAsync(x => x.Id == request.TaskId)).FirstOrDefault();

                            var TaskPermisions = await _unitOfWork.TaskPermissions.FindAllAsync(x => x.TaskId == request.TaskId);
                            var TaskPermisionsDb = TaskPermisions.ToList();

                            if (TaskPermisionsDb != null && TaskPermisionsDb.Any())
                            {

                                var TaskUserDeleted = TaskPermisionsDb.Where(x => x.UserGroupId != request.RemainUserGroupId && x.UserGroupId != TaskDb.CreatedBy && !x.IsGroup).ToList();

                                if (TaskUserDeleted != null && TaskUserDeleted.Any())
                                {
                                    _unitOfWork.TaskPermissions.DeleteRange(TaskUserDeleted);
                                }

                                var TaskGroupsDeleted = TaskPermisionsDb.Where(x => x.IsGroup).ToList();

                                if (TaskGroupsDeleted != null && TaskGroupsDeleted.Any())
                                {
                                    _unitOfWork.TaskPermissions.DeleteRange(TaskGroupsDeleted);
                                }

                                if (!TaskPermisionsDb.Any(a => a.UserGroupId != request.RemainUserGroupId && !a.IsGroup))
                                {
                                    TaskPermission taskPermission = new TaskPermission
                                    {
                                        IsGroup = false,
                                        PermissionLevelId = 6,
                                        TaskId = (long)request.TaskId,
                                        UserGroupId = (long)request.RemainUserGroupId
                                    };
                                    _unitOfWork.TaskPermissions.Add(taskPermission);
                                }
                                if (!TaskPermisionsDb.Any(a => a.UserGroupId != TaskDb.CreatedBy && !a.IsGroup))
                                {
                                    TaskPermission taskPermission = new TaskPermission
                                    {
                                        IsGroup = false,
                                        PermissionLevelId = 6,
                                        TaskId = (long)request.TaskId,
                                        UserGroupId = TaskDb.CreatedBy
                                    };
                                    _unitOfWork.TaskPermissions.Add(taskPermission);
                                }

                                var TaskFlags = await _unitOfWork.TaskFlagsOwnerRecievers.FindAllAsync(x => x.TaskId == request.TaskId && x.UserId != request.RemainUserGroupId && x.UserId != TaskDb.CreatedBy);

                                if (TaskFlags != null && TaskFlags.Any())
                                {
                                    _unitOfWork.TaskFlagsOwnerRecievers.DeleteRange(TaskFlags);
                                }

                                _unitOfWork.Complete();
                            }

                        }
                        else
                        {
                            // Modification by michael markos 2023-8-31
                            foreach (var item in request.TaskUserGroupList)
                            {
                                if (item.IsDeleted == true)
                                {
                                    var TaskUserGroupsDelted = (await _unitOfWork.TaskPermissions
                                        .FindAllAsync(x => x.TaskId == item.TaskID && x.IsGroup == item.IsGroup && x.UserGroupId == item.UserGroupID)).FirstOrDefault();

                                    if (TaskUserGroupsDelted != null)
                                    {

                                        _unitOfWork.TaskPermissions.Delete(TaskUserGroupsDelted);
                                        _unitOfWork.Complete();


                                        var TaskFlagID = (await _unitOfWork.TaskFlagsOwnerRecievers.FindAllAsync(x => x.TaskId == item.TaskID && x.UserId == item.UserGroupID)).FirstOrDefault();
                                        _unitOfWork.TaskFlagsOwnerRecievers.Delete(TaskFlagID);
                                        _unitOfWork.Complete();
                                    }
                                }
                                else
                                {
                                    //ObjectParameter ID = new ObjectParameter("ID", typeof(int));

                                    var TaskUserGroup = _unitOfWork.TaskPermissions.Add(new TaskPermission()
                                    {
                                        TaskId = item.TaskID,
                                        UserGroupId = item.UserGroupID,
                                        PermissionLevelId = 6,
                                        IsGroup = item.IsGroup
                                    });
                                    _unitOfWork.Complete();
                                    //ObjectParameter TaskFlagID = new ObjectParameter("ID", typeof(int));

                                    var TaskFlagOrReciever = _unitOfWork.TaskFlagsOwnerRecievers.Add(new TaskFlagsOwnerReciever()
                                    {
                                        TaskId = item.TaskID,
                                        UserId = item.UserGroupID,
                                        Read = false,
                                        Flag = item.flag,
                                        Star = false
                                    });
                                    _unitOfWork.Complete();
                                }
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

        public async Task<BaseResponseWithId<long>> UpdateTaskFlagsOwnerReciever(GetTaskDetailsData request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var TaskDetails = new GetTaskDetailsData();
                var GetTaskDetailsListList = new List<GetTaskDetailsList>();




                if (Response.Result)
                {

                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    //check sent data
                    if (request.TaskID == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Task ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.UserID == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid User ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }



                    if (Response.Result)
                    {

                        var TaskFlagsID = _unitOfWork.TaskFlagsOwnerRecievers.FindAll(x => x.TaskId == request.TaskID && x.UserId == request.UserID).FirstOrDefault();
                        if (TaskFlagsID != null)
                        {

                            /*var TaskIDUpdate = _Context.proc_TaskFlagsOwnerRecieverUpdate(TaskFlagsID.ID,
                                                                                                   Request.TaskID,
                                                                                                   Request.UserID,
                                                                                                   Request.Read,
                                                                                                   Request.Flag,
                                                                                                   Request.Star
                                                                                           );*/
                            TaskFlagsID.TaskId = request.TaskID;
                            TaskFlagsID.UserId = request.UserID;
                            TaskFlagsID.Read = request.Read;
                            TaskFlagsID.Flag = request.Flag;
                            TaskFlagsID.Star = request.Star;
                            _unitOfWork.Complete();
                            Response.ID = TaskFlagsID.Id;

                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Task ID Doesn't Exist !!";
                            Response.Errors.Add(error);
                        }

                    }








                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err25";
                    error.ErrorMSG = "Faild To Insert this Task !!";
                    Response.Errors.Add(error);
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

        public async Task<GetTaskCategoryDDLResponse> GetTaskCategoryDDL()
        {
            GetTaskCategoryDDLResponse response = new GetTaskCategoryDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetTaskCategoryDDLList = new List<TaskCategoryDDLData>();




                if (response.Result)
                {

                    var GetTaskCategoryDDLListDB = await _unitOfWork.TaskCategories.GetAllAsync();

                    if (GetTaskCategoryDDLListDB != null)
                    {

                        foreach (var GetTaskCategoryDDLOBJ in GetTaskCategoryDDLListDB)
                        {
                            var GetTaskCategoryDDLResponse = new TaskCategoryDDLData();



                            GetTaskCategoryDDLResponse.ID = (int)GetTaskCategoryDDLOBJ.Id;

                            GetTaskCategoryDDLResponse.Name = GetTaskCategoryDDLOBJ.Name;






                            GetTaskCategoryDDLList.Add(GetTaskCategoryDDLResponse);
                        }



                    }

                }


                response.TaskCategoryDDLlist = GetTaskCategoryDDLList;
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

        public async Task<GetTaskTypeNameResponse> GetTaskTypeNameList()
        {
            GetTaskTypeNameResponse response = new GetTaskTypeNameResponse();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {


                var TaskTypeResponseList = new List<TaskTypeData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var TasktypeDB = _unitOfWork.TaskTypes.GetAll();


                        if (TasktypeDB != null && TasktypeDB.Count() > 0)
                        {

                            foreach (var TasktypeDBOBJ in TasktypeDB)
                            {
                                var TasktypeDBOBJResponse = new TaskTypeData();

                                TasktypeDBOBJResponse.ID = TasktypeDBOBJ.Id;

                                TasktypeDBOBJResponse.TaskTypeName = TasktypeDBOBJ.Name;




                                TaskTypeResponseList.Add(TasktypeDBOBJResponse);
                            }



                        }

                    }

                }
                response.TaskTypeList = TaskTypeResponseList;
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

        //public async Task<GetTaskListsByStatusResponse> GetTaskListsByStatus(GetTaskHeader header)
        //{
        //    GetTaskListsByStatusResponse response = new GetTaskListsByStatusResponse();
        //    response.Result = true;
        //    response.Errors = new List<Error>();
        //    try
        //    {
        //        if (response.Result)
        //        {
        //            var ReceivedTasks = new List<GetTaskIndex>();
        //            var OpenTasks = new List<GetTaskIndex>();
        //            var WaitingTasks = new List<GetTaskIndex>();
        //            var ClosedTasks = new List<GetTaskIndex>();
        //            header.Status = "Received";
        //            ReceivedTasks = GetTask(header).Value.TasksList;
        //            header.Status = "Open";
        //            OpenTasks = GetTask(header).Value.TasksList;
        //            header.Status = "Waiting Approval";
        //            WaitingTasks = GetTask(header).Value.TasksList;
        //            header.Status = "Closed";
        //            ClosedTasks = GetTask(header).Value.TasksList;



        //            response.OpenTasksList = OpenTasks;
        //            response.OpenCount = OpenTasks.Count();
        //            response.ReceivedTasksList = ReceivedTasks;
        //            response.ReceivedCount = ReceivedTasks.Count();
        //            response.WaitingTasksList = WaitingTasks;
        //            response.WaitingCount = WaitingTasks.Count();
        //            response.ClosedTasksList = ClosedTasks;
        //            response.ClosedCount = ClosedTasks.Count();
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.InnerException.Message;
        //        response.Errors.Add(error);

        //        return response;
        //    }
        //}

        //-----------------------------------------GetTaskGroupedByWorkFlowList API-------------------------
        public BaseResponseWithData<TaskWorkFlosGroups> GetTaskGroupedByWorkFlowList(bool NotActive, long ProjectID, long ProjectSprintID, bool IsArchived, long UserID, int CurrentPage, int NumberOfItemsPerPage)
        {
            BaseResponseWithData<TaskWorkFlosGroups> response = new BaseResponseWithData<TaskWorkFlosGroups>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                var GetTaskList = new List<TasksData>();
                //var tasks =new List<GetTaskIndex>().AsQueryable();
                if (response.Result)
                {
                    Expression<Func<Task, bool>> criteria = (a => true);
                    criteria = a =>
                    (
                    (ProjectID != 0 ? a.ProjectId == ProjectID : true) &&
                    (ProjectSprintID != 0 ? a.ProjectSprintId == ProjectSprintID : true) &&
                    (NotActive == true ? a.Active == false : a.Active == true) &&
                    (IsArchived == true ? a.IsArchived == true : (a.IsArchived == false || a.IsArchived == null))
                    );



                    var TasksList = _unitOfWork.Tasks.FindAll(criteria, new[] { "CreatedByNavigation" });

                    var FinalTasks = TasksList.ToList();

                    //level_3

                    var replies = _unitOfWork.TaskUserReplies.GetAll();
                    var details = _unitOfWork.TaskDetails.GetAll();
                    var types = _unitOfWork.TaskTypes.GetAll();
                    //var taskCount = _unitOfWork.Tasks.GetAll();
                    var receivers = _unitOfWork.TaskFlagsOwnerRecievers.GetAll();
                    var tasks = FinalTasks.Select(a => new GetTaskIndex
                    {
                        ID = a.Id,
                        Active = a.Active,
                        IsFinished = replies.Where(x => x.TaskId == a.Id).Select(x => x.IsFinished).FirstOrDefault(),
                        NeedApproval = details.Where(x => x.TaskId == a.Id).Select(x => x.NeedApproval).FirstOrDefault(),
                        IsTaskOwner = a.CreatedBy == UserID,
                        Piriority = details.Where(x => x.TaskId == a.Id).Select(x => x.Priority).FirstOrDefault(),
                        Status = details.Where(x => x.TaskId == a.Id).Select(x => x.Status).FirstOrDefault(),
                        Name = a.Name,
                        Description = a.Name,
                        ExpireDate = a.ExpireDate != null ? a.ExpireDate.ToString() : null,
                        CreationDate = a.CreationDate.ToString(),
                        CreatedBy = a.CreatedBy,
                        CreatoreName = a.CreatedByNavigation.FirstName + " " + a.CreatedByNavigation.MiddleName + " " + a.CreatedByNavigation.LastName,
                        TaskCategory = a.TaskCategory,
                        /* GroupReply = a.GroupReply,
                        TaskPage = a.TaskPage,
                        TaskUser = a.TaskUser,
                        TaskTypeName = types.Where(x => x.Id == a.TaskTypeId).Select(x => x.Name).FirstOrDefault(),*/
                        TaskSubject = a.TaskSubject,
                        //BranchName = a.BranchId != null? Common.GetBranchName((int)a.BranchId, _Context):null,
                        CreatorPhoto = a.CreatedByNavigation.PhotoUrl != null ? Globals.baseURL + a.CreatedByNavigation.PhotoUrl : null,
                        TaskStatus = "",
                        //TaskCount = 0,
                        RejectedNo = replies.Where(x => x.TaskId == a.Id && x.Approval == false).Count(),
                        //TaskCreatorCount = taskCount.Where(x => x.CreatedBy == a.CreatedBy).Count(),
                        TaskTypeID = a.TaskTypeId,
                        ProjectID = a.ProjectId,
                        ProjectSprintID = a.ProjectSprintId,
                        ProjectWorkFlowID = a.ProjectWorkFlowId,
                        //taskAttachements = details.Where(x => x.TaskId == a.Id).Select(x => x.CreatorAttachement).FirstOrDefault() != null ? Globals.baseURL + details.Where(x => x.TaskId == a.Id).Select(x => x.CreatorAttachement).FirstOrDefault()?.TrimStart('~') : null,
                        TaskUserGroupList = receivers.Where(x => x.TaskId == a.Id && x.UserId == UserID).Select(x => new TaskUserGroupData()
                        {
                            TaskID = x.TaskId,
                            UserGroupID = x.UserId,
                            Flag = x.Flag,
                            Read = x.Read,
                            Star = x.Star
                        }).ToList()
                    }).AsQueryable();

                    var taskList = tasks.ToList();
                    foreach (var a in taskList)
                    {
                        var LastTaskStatus = _unitOfWork.TaskUserReplies.FindAll(x => x.TaskId == a.ID).OrderBy(x => x.CreationDate).LastOrDefault();
                        if (LastTaskStatus != null)
                        {
                            if (LastTaskStatus.IsFinished == null && LastTaskStatus.Approval == null)
                            {
                                a.TaskStatus = "Need Approval";
                            }
                            else if (LastTaskStatus.IsFinished == true)
                            {
                                a.TaskStatus = "Waiting Approval";
                            }
                            else if (LastTaskStatus.Approval != null && LastTaskStatus.Approval == false)
                            {
                                a.TaskStatus = "Rejected";
                            }
                            else if (LastTaskStatus.Approval != null && LastTaskStatus.Approval == true)
                            {
                                a.TaskStatus = "Approved";
                            }
                        }

                    }
                    var groupedTaskList = taskList.GroupBy(a => a.ProjectWorkFlowID).ToList();
                    //var taskListPaged = PagedList<GetTaskIndex>.Create(taskList.OrderByDescending(a => DateTime.Parse(a.CreationDate)).AsQueryable(), CurrentPage, NumberOfItemsPerPage);
                    //response.TasksList = taskListPaged;
                    //response.TaskCount = taskList.Count();
                    //response.Data = groupedTaskList;
                    var workFlowList = _unitOfWork.ProjectWorkFlows.FindAll(a => a.ProjectId == ProjectID).ToList();
                    var TaskGroupingByWorkFlow = new List<TaskGroupingByWorkFlow>();
                    int TaskCount = 0;
                    foreach (var group in groupedTaskList)
                    {
                        var taskGroup = new TaskGroupingByWorkFlow();
                        taskGroup.ProjectWorkFlowID = (long)group.Key;
                        taskGroup.WorkFlowName = workFlowList.Where(a => a.Id == taskGroup.ProjectWorkFlowID).Select(a => a.WorkFlowName).FirstOrDefault();
                        TaskCount += group.Count();
                        var newtasksList = new List<GetTaskIndex>();
                        foreach (var task in group)
                        {
                            var taskResponse = new GetTaskIndex()
                            {
                                ID = task.ID,
                                Active = task.Active,
                                IsFinished = replies.Where(x => x.TaskId == task.ID).Select(x => x.IsFinished).FirstOrDefault(),
                                NeedApproval = details.Where(x => x.TaskId == task.ID).Select(x => x.NeedApproval).FirstOrDefault(),
                                IsTaskOwner = task.CreatedBy == UserID,
                                Piriority = details.Where(x => x.TaskId == task.ID).Select(x => x.Priority).FirstOrDefault(),
                                Status = details.Where(x => x.TaskId == task.ID).Select(x => x.Status).FirstOrDefault(),
                                Name = task.Name,
                                Description = task.Description,
                                ExpireDate = task.ExpireDate?.ToString(),
                                CreationDate = task.CreationDate.ToString(),
                                CreatedBy = task.CreatedBy,
                                CreatoreName = task.CreatoreName,
                                TaskCategory = task.TaskCategory,
                                /* GroupReply = a.GroupReply,
                                TaskPage = a.TaskPage,
                                TaskUser = a.TaskUser,
                                TaskTypeName = types.Where(x => x.Id == a.TaskTypeId).Select(x => x.Name).FirstOrDefault(),*/
                                TaskSubject = task.TaskSubject,
                                //BranchName = a.BranchId != null? Common.GetBranchName((int)a.BranchId, _Context):null,
                                CreatorPhoto = task.CreatorPhoto,
                                TaskStatus = "",
                                //TaskCount = 0,
                                RejectedNo = replies.Where(x => x.TaskId == task.ID && x.Approval == false).Count(),
                                //TaskCreatorCount = taskCount.Where(x => x.CreatedBy == a.CreatedBy).Count(),
                                TaskTypeID = task.TaskTypeID,
                                ProjectID = task.ProjectID,
                                ProjectSprintID = task.ProjectSprintID,
                                ProjectWorkFlowID = task.ProjectWorkFlowID,
                                //taskAttachements = details.Where(x => x.TaskId == a.Id).Select(x => x.CreatorAttachement).FirstOrDefault() != null ? Globals.baseURL + details.Where(x => x.TaskId == a.Id).Select(x => x.CreatorAttachement).FirstOrDefault()?.TrimStart('~') : null,
                                TaskUserGroupList = receivers.Where(x => x.TaskId == task.ID && x.UserId == UserID).Select(x => new TaskUserGroupData()
                                {
                                    TaskID = x.TaskId,
                                    UserGroupID = x.UserId,
                                    Flag = x.Flag,
                                    Read = x.Read,
                                    Star = x.Star
                                }).ToList()
                            };
                            newtasksList.Add(taskResponse);
                        }
                        taskGroup.TaskList = newtasksList.OrderByDescending(a => a.ID).ToList();
                        TaskGroupingByWorkFlow.Add(taskGroup);
                    }

                    var tempObj = new TaskWorkFlosGroups();
                    tempObj.groupList = TaskGroupingByWorkFlow;
                    tempObj.CountOfTasks = TaskCount;
                    response.Data = tempObj;
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

        public BaseResponseWithId<long> DeleteTask(long TaskId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                if (TaskId == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err101";
                    err.errorMSG = "Task Id Is Required";
                    response.Errors.Add(err);
                    return response;
                }
                var task = _unitOfWork.Tasks.GetById(TaskId);
                if (task == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err102";
                    err.errorMSG = "Task not found";
                    response.Errors.Add(err);
                    return response;
                }
                _unitOfWork.Tasks.Delete(task);
                _unitOfWork.Complete();

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "Err10";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }
        //--------------------------------------------------------------------------------------------------
        public BaseResponseWithId<long> AddTaskUnitRateService(AddTaskUnitRateServiceDto Dto, long Creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                #region Validation
                var task = _unitOfWork.Tasks.GetById(Dto.TaskID);
                if (task == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Task with this Id";
                    response.Errors.Add(error);
                    return response;
                }

                var inventoryUOM = _unitOfWork.InventoryUoms.GetById(Dto.UOMID);
                if (inventoryUOM == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No UOM with this Id";
                    response.Errors.Add(error);
                    return response;
                }
                if (task.ProjectId == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This Task has no project can not add Task Unit Rate Service to it";
                    response.Errors.Add(error);
                    return response;
                }
                var project = _unitOfWork.Projects.GetById(task.ProjectId ?? 0);
                if (project.UnitRateService == null || project.UnitRateService == false)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "The parent project Unit Rate Service flag is not checked";
                    response.Errors.Add(error);
                    return response;
                }
                #endregion

                var newTaskUnitRate = new TaskUnitRateService();
                newTaskUnitRate.ServiceName = Dto.ServiceName;
                newTaskUnitRate.Rate = Dto.Rate;
                newTaskUnitRate.Quantity = Dto.Quantity;
                newTaskUnitRate.Total = Dto.Rate * Dto.Quantity;
                newTaskUnitRate.TaskId = Dto.TaskID;
                newTaskUnitRate.Uomid = Dto.UOMID;
                newTaskUnitRate.CreationDate = DateTime.Now;
                newTaskUnitRate.CreatedBy = Creator;
                newTaskUnitRate.ModifiedDate = DateTime.Now;
                newTaskUnitRate.ModifiedBy = Creator;

                _unitOfWork.TaskUnitRateServices.Add(newTaskUnitRate);
                _unitOfWork.Complete();

                response.ID = newTaskUnitRate.Id;
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

        public BaseResponseWithId<long> EditTaskUnitRateService(EditTaskUnitRateServiceDto Dto, long Editor)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                #region Validation
                var taskUnitRateService = _unitOfWork.TaskUnitRateServices.GetById(Dto.Id);
                if (taskUnitRateService == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Task Unit Rate Service with this Id";
                    response.Errors.Add(error);
                    return response;
                }
                var task = _unitOfWork.Tasks.GetById(Dto.TaskID);
                if (task == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Task with this Id";
                    response.Errors.Add(error);
                    return response;
                }

                var inventoryUOM = _unitOfWork.InventoryUoms.GetById(Dto.UOMID);
                if (inventoryUOM == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No UOM with this Id";
                    response.Errors.Add(error);
                    return response;
                }
                if (task.ProjectId == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This Task has no project can not add Task Unit Rate Service to it";
                    response.Errors.Add(error);
                    return response;
                }
                var project = _unitOfWork.Projects.GetById(task.ProjectId ?? 0);
                if (project.UnitRateService == null || project.UnitRateService == false)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "The parent project Unit Rate Service flag is not checked";
                    response.Errors.Add(error);
                    return response;
                }
                #endregion

                taskUnitRateService.ServiceName = Dto.ServiceName;
                taskUnitRateService.Rate = Dto.Rate;
                taskUnitRateService.Quantity = Dto.Quantity;
                taskUnitRateService.Total = Dto.Rate * Dto.Quantity;
                taskUnitRateService.TaskId = Dto.TaskID;
                taskUnitRateService.Uomid = Dto.UOMID;
                taskUnitRateService.ModifiedBy = Editor;
                taskUnitRateService.ModifiedDate = DateTime.Now;

                _unitOfWork.Complete();
                response.ID = Dto.Id;
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

        public BaseResponseWithData<List<GetTaskUnitRateServiceDto>> GetTaskUnitRateServiceList(long TaskId)
        {
            BaseResponseWithData<List<GetTaskUnitRateServiceDto>> response = new BaseResponseWithData<List<GetTaskUnitRateServiceDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var task = _unitOfWork.Tasks.Find(a => a.Id == TaskId);
                if (task == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Task not Found, please Enter a valid Task ID";
                    response.Errors.Add(error);
                    return response;
                }

                var tastUnitRateService = _unitOfWork.TaskUnitRateServices.FindAll((a => a.TaskId == TaskId), new[] { "CreatedByNavigation", "Uom" });
                tastUnitRateService = tastUnitRateService.OrderByDescending(a => a.CreationDate).ToList();
                var tastUnitRateServiceData = _mapper.Map<List<GetTaskUnitRateServiceDto>>(tastUnitRateService);


                response.Data = tastUnitRateServiceData;
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

        public BaseResponseWithData<GetTaskUnitRateServiceDto> GetTaskUnitRateServiceByID(long TaskUnitRateServiceID)
        {
            BaseResponseWithData<GetTaskUnitRateServiceDto> response = new BaseResponseWithData<GetTaskUnitRateServiceDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var taskUnitRateServices = _unitOfWork.TaskUnitRateServices.FindAll((a => a.Id == TaskUnitRateServiceID), new[] { "CreatedByNavigation", "Uom" }).FirstOrDefault();
                if (taskUnitRateServices == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Task Unit Rate Services not Found, please Enter a valid Task Unit Rate Services ID";
                    response.Errors.Add(error);
                    return response;
                }

                //var tastUnitRateService = _unitOfWork.TaskUnitRateServices.FindAll((a => a.TaskId == TaskId));
                //tastUnitRateService = tastUnitRateService.OrderByDescending(a => a.CreationDate).ToList();
                var taskExpensisData = _mapper.Map<GetTaskUnitRateServiceDto>(taskUnitRateServices);


                response.Data = taskExpensisData;
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

        public BaseResponseWithData<List<GetInventoryUOMDDL>> GetInventoryUOMDDL()
        {
            BaseResponseWithData<List<GetInventoryUOMDDL>> Response = new BaseResponseWithData<List<GetInventoryUOMDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {


                if (Response.Result)
                {
                    var InventoryUOMList = _unitOfWork.InventoryUoms.GetAll();
                    InventoryUOMList = InventoryUOMList.OrderBy(a => a.Name);
                    var InventoryUOMDDL = new List<GetInventoryUOMDDL>();
                    foreach (var inventory in InventoryUOMList)
                    {
                        var DDLObj = new GetInventoryUOMDDL();
                        DDLObj.ID = inventory.Id;
                        DDLObj.Name = inventory.Name.Trim();
                        DDLObj.ShortName = inventory.ShortName.Trim();

                        InventoryUOMDDL.Add(DDLObj);
                    }
                    Response.Data = InventoryUOMDDL;

                }

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

        public BaseResponseWithData<List<TaskDDL>> GetTaskDDL(long ProjectId)
        {
            BaseResponseWithData<List<TaskDDL>> Response = new BaseResponseWithData<List<TaskDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var TaskDDLQuerable = _unitOfWork.Tasks.GetAsQueryable();
                    if (ProjectId != 0)
                    {
                        TaskDDLQuerable = TaskDDLQuerable.Where(x => x.ProjectId == ProjectId);
                    }
                    var TaskDDLList = TaskDDLQuerable.Select(a => new TaskDDL { ID = a.Id, Name = a.Name }).ToList();
                    Response.Data = TaskDDLList;
                }
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


        public BaseResponseWithData<List<GetTaskUnitRateServiceDto>> GetTaskUnitRateServiceListByProjectID(long ProjectID)
        {
            BaseResponseWithData<List<GetTaskUnitRateServiceDto>> response = new BaseResponseWithData<List<GetTaskUnitRateServiceDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var project = _unitOfWork.Projects.GetById(ProjectID);
                if (project == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "project not Found, please Enter a valid project ID";
                    response.Errors.Add(error);
                    return response;
                }
                var task = _unitOfWork.Tasks.FindAll(a => a.ProjectId == ProjectID);

                var tasksIds = task.Select(a => a.Id).ToList();

                var tastUnitRateService = _unitOfWork.TaskUnitRateServices.FindAll((a => tasksIds.Contains(a.TaskId)), new[] { "CreatedByNavigation", "Uom", "CreatedByNavigation.JobTitle" });
                tastUnitRateService = tastUnitRateService.OrderByDescending(a => a.CreationDate).ToList();
                var taskUnitRateServiceData = _mapper.Map<List<GetTaskUnitRateServiceDto>>(tastUnitRateService);

                foreach (var taskUnitRateService in taskUnitRateServiceData)
                {
                    var item = _unitOfWork.ProjectInvoiceItems.FindAll(a => a.ItemId == taskUnitRateService.Id && a.Type == "TaskUnitRateServices").FirstOrDefault();
                    if (item != null)
                    {
                        taskUnitRateService.IsInvoiced = true;
                        taskUnitRateService.InvoiceId = item.Id;
                    }
                }

                response.Data = taskUnitRateServiceData;
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

        public BaseResponseWithId<long> DeleteTaskUnitRateService(long Id)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var TaskUserReply = _unitOfWork.TaskUnitRateServices.GetById(Id);
            if (TaskUserReply == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No task unit Rate Service with this ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {

                _unitOfWork.TaskUnitRateServices.Delete(TaskUserReply);
                _unitOfWork.Complete();

                response.ID = TaskUserReply.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        //-------------------------------------------------------taskUser Reply-----------------------------------------------
        public BaseResponseWithId<long> EditTaskUserReply(EditTaskUserReplyDto dto, string companyaName)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region check in DB
            var taskUserReply = _unitOfWork.TaskUserReplies.GetById(dto.ID);
            if (taskUserReply == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "Err-01";
                err.ErrorMSG = "No Task user reply with this ID";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                taskUserReply.CommentReply = dto.CommentReply;
                if (dto.ReplyAttachment != null)
                {
                    if (taskUserReply.ReplyAttach != null)
                    {
                        string FilePath = Path.Combine(_host.WebRootPath, taskUserReply.ReplyAttach);
                        if (System.IO.File.Exists(FilePath))
                        {
                            System.IO.File.Delete(FilePath);
                        }
                    }

                    var fileExtension = dto.ReplyAttachment.FileName.Split('.').Last();
                    var virtualPath = $"Attachments\\{companyaName}\\TaskUserReply\\{taskUserReply.Id}\\";
                    var FileName = System.IO.Path.GetFileNameWithoutExtension(dto.ReplyAttachment.FileName.Trim().Replace(" ", ""));
                    var AttachPath = Common.SaveFileIFF(virtualPath, dto.ReplyAttachment, FileName, fileExtension, _host);

                    taskUserReply.ReplyAttach = AttachPath;
                }
                _unitOfWork.Complete();

                response.ID = taskUserReply.Id;
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

        public BaseResponseWithId<long> DeleteEditTaskUserReply(long Id)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var TaskUserReply = _unitOfWork.TaskUserReplies.GetById(Id);
            if (TaskUserReply == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No task user Reply with this ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {

                _unitOfWork.TaskUserReplies.Delete(TaskUserReply);
                _unitOfWork.Complete();

                response.ID = TaskUserReply.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }
        //--------------------------------------------------------------------------------------------------------------------
        public BaseResponseWithData<string> GetTaskProgressReport([FromHeader] long ProjectId, [FromHeader] long? TaskId, [FromHeader] long? InvoiceNumber, [FromHeader] string Type, [FromHeader] string UserName, [FromHeader] DateTime? From, [FromHeader] DateTime? To, string CompanyName)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (ProjectId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Project Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var Project = _unitOfWork.Projects.GetById(ProjectId);
                if (Project == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Project not found";
                    Response.Errors.Add(error);
                    return Response;
                }

                var Progress = _unitOfWork.ProjectInvoiceItems.FindAllQueryable(a => a.ProjectInvoice.ProjectId == ProjectId, includes: new[] { "ProjectInvoice.Project.Tasks", "ProjectInvoice.Project.SalesOffer" }).AsQueryable();

                if (TaskId != null)
                {
                    Progress = Progress.Where(a => a.ProjectInvoice.Project.Tasks.Select(a => a.Id).ToList().Contains((long)TaskId)).AsQueryable();
                }
                if (InvoiceNumber != null)
                {
                    Progress = Progress.Where(a => a.ProjectInvoice.Id == InvoiceNumber).AsQueryable();
                }
                if (!string.IsNullOrEmpty(Type))
                {
                    Progress = Progress.Where(a => a.Type == Type).AsQueryable();
                }
                if (!string.IsNullOrEmpty(UserName))
                {
                    Progress = Progress.Where(a => a.UserName == UserName).AsQueryable();
                }
                if (From != null)
                {
                    Progress = Progress.Where(a => a.CreationDate >= From).AsQueryable();
                }
                if (To != null)
                {
                    Progress = Progress.Where(a => a.CreationDate <= To).AsQueryable();
                }
                var ProgressList = Progress.ToList();
                var package = new ExcelPackage();
                var dt = new System.Data.DataTable("Grid");
                dt.Columns.AddRange(new DataColumn[12] {
                new DataColumn("Project Name"),
                new DataColumn("Task Name"),
                new DataColumn("Type"),
                new DataColumn("Invoice Serial"),
                new DataColumn("Name"),
                new DataColumn("Quantity"),
                new DataColumn("Unit Used"),
                new DataColumn("Price"),
                new DataColumn("Total"),
                new DataColumn("Date"),
                new DataColumn("Creator Name"),
                new DataColumn("Comment"),
                });

                var itemIds = ProgressList.Select(a => a.ItemId).ToList();
                var expenses = _unitOfWork.TaskExpensis.FindAll(a => itemIds.Contains(a.Id), includes: new[] { "ExpensisType" }).ToList();
                var wtps = _unitOfWork.WorkingHoursTrackings.FindAll(a => itemIds.Contains(a.Id)).ToList();
                var UniteRates = _unitOfWork.TaskUnitRateServices.FindAll(a => itemIds.Contains(a.Id), includes: new[] { "Uom" }).ToList();
                foreach (var item in ProgressList)
                {
                    if (item.Type == "TaskExpenses")
                    {
                        dt.Rows.Add(
                        item.ProjectInvoice.Project.SalesOffer.ProjectName,
                        item.ProjectInvoice.Project.Tasks.FirstOrDefault()?.Name,
                        item.Type,
                        item.ProjectInvoice.InvoiceSerial,
                        expenses.Where(a => a.Id == item.ItemId).FirstOrDefault()?.ExpensisType?.ExpensisTypeName,
                        1,
                        "N/A",
                        item.Quantity,
                        item.Quantity,
                        item.CreationDate.ToShortDateString(),
                        item.UserName,
                        expenses.Where(a => a.Id == item.Id).FirstOrDefault()?.Note
                        );
                    }
                    else if (item.Type == "WTP")
                    {
                        dt.Rows.Add(
                        item.ProjectInvoice.Project.SalesOffer.ProjectName,
                        item.ProjectInvoice.Project.Tasks.FirstOrDefault()?.Name,
                        item.Type,
                        item.ProjectInvoice.InvoiceSerial,
                        Decimal.Round(wtps.FirstOrDefault(a => a.Id == item.ItemId)?.ProgressRate ?? 0, 2, MidpointRounding.AwayFromZero)
                         + "% Progress",
                        item.Quantity,
                        "Working Hours",
                        item.Rate,
                        item.Total,
                        item.CreationDate.ToShortDateString(),
                        item.UserName,
                        wtps.Where(a => a.Id == item.ItemId).FirstOrDefault()?.ProgressNote
                        );
                    }
                    else if (item.Type == "TaskUnitRateServices")
                    {
                        dt.Rows.Add(
                        item.ProjectInvoice.Project.SalesOffer.ProjectName,
                        item.ProjectInvoice.Project.Tasks.FirstOrDefault()?.Name,
                        item.Type,
                        item.ProjectInvoice.InvoiceSerial,
                        item.Name,
                        item.Quantity,
                        UniteRates.Where(a => a.Id == item.ItemId).FirstOrDefault()?.Uom?.Name,
                        item.Rate,
                        item.Total,
                        item.CreationDate.ToShortDateString(),
                        item.UserName,
                        ""
                        );
                    }
                    else if (item.Type == "discount")
                    {
                        dt.Rows.Add(
                        item.ProjectInvoice.Project.SalesOffer.ProjectName,
                        item.ProjectInvoice.Project.Tasks.FirstOrDefault()?.Name,
                        item.Type,
                        item.ProjectInvoice.InvoiceSerial,
                        item.Name,
                        1,
                        "N/A",
                        item.Total * -1,
                        item.Total * -1,
                        item.CreationDate.ToShortDateString(),
                        item.CreationDate.ToShortDateString(),
                        item.UserName,
                        ""
                        );
                    }
                }
                var workSheet = package.Workbook.Worksheets.Add("TaskProgress");
                workSheet.DefaultRowHeight = 12;
                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[1, 1, 1, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[1, 1, 1, 12].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 174, 81));
                ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);
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
                var newpath = $"Attachments\\{CompanyName}\\TaskProgressReports";
                var savedPath = System.IO.Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                {
                    File.Delete(savedPath);
                }
                Directory.CreateDirectory(savedPath);
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\TaskProgressReport_{date}.xlsx";
                package.SaveAs(excelPath);
                package.Dispose();
                Response.Data = Globals.baseURL + '\\' + newpath + $"\\TaskProgressReport_{date}.xlsx";
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }

        //----------------------------------------------------Excell APIs-----------------------------------------------------
        public BaseResponseWithData<string> GetTasksListReportExcell(GetTasksListReportFilters filters, string CompName)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region validation in DB
            if (filters.Priorty != null)
            {
                var branch = _unitOfWork.Prioritys.GetById(filters.Priorty ?? 0);
                if (branch == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Priorty is not found";
                    response.Errors.Add(error);
                    return response;
                }

            }

            if (filters.ProjectID != null)
            {
                var branch = _unitOfWork.Projects.GetById(filters.ProjectID ?? 0);
                if (branch == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Project is not found";
                    response.Errors.Add(error);
                    return response;
                }

            }
            if (filters.UserID != null)
            {
                var branch = _unitOfWork.Users.GetById(filters.UserID ?? 0);
                if (branch == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "User is not found";
                    response.Errors.Add(error);
                    return response;
                }

            }
            if (filters.TaskTypeID != null)
            {
                var branch = _unitOfWork.TaskTypes.GetById(filters.TaskTypeID ?? 0);
                if (branch == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Task Type is not found";
                    response.Errors.Add(error);
                    return response;
                }

            }

            #endregion

            #region Date Validation
            DateTime startDate = DateTime.Now;
            if (!string.IsNullOrEmpty(filters.DateFrom))
            {
                if (!DateTime.TryParse(filters.DateFrom, out startDate))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Enter a valid DateFrom";
                    response.Errors.Add(error);
                    return response;
                }
            }

            DateTime endDate = DateTime.Now;
            if (!string.IsNullOrEmpty(filters.DateTo))
            {
                if (!DateTime.TryParse(filters.DateTo, out endDate))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Enter a valid DateFrom";
                    response.Errors.Add(error);
                    return response;
                }
            }
            #endregion

            try
            {

                var tasksQueryable = _unitOfWork.Tasks.FindAllQueryable(a => true, new[] { "Project", "TaskType", "CreatedByNavigation", "TaskDetails", "Project.Priorty", "TaskExpensis", "Project.SalesOffer", "WorkingHourseTrackings", "TaskRequirements" });

                if (filters.ProjectID != null)
                {
                    tasksQueryable = tasksQueryable.Where(a => a.ProjectId == filters.ProjectID);
                }
                if (filters.Priorty != null)
                {
                    tasksQueryable = tasksQueryable.Where(a => a.Project.PriortyId == filters.Priorty);
                }
                if (filters.UserID != null)
                {
                    tasksQueryable = tasksQueryable.Where(a => a.CreatedBy == filters.UserID);
                }
                if (filters.Priorty != null)
                {
                    tasksQueryable = tasksQueryable.Where(a => a.Project.Priorty.Id == filters.Priorty);
                }
                if (!string.IsNullOrEmpty(filters.Status))
                {
                    tasksQueryable = tasksQueryable.Where(a => a.TaskDetails.FirstOrDefault().Status.Contains(filters.Status));
                }
                if (!string.IsNullOrEmpty(filters.DateFrom))
                {
                    tasksQueryable = tasksQueryable.Where(a => a.CreationDate.Date >= startDate);
                }
                if (!string.IsNullOrEmpty(filters.DateTo))
                {
                    tasksQueryable = tasksQueryable.Where(a => a.CreationDate.Date <= startDate);
                }
                if (!string.IsNullOrEmpty(filters.TaskCategory))
                {
                    tasksQueryable = tasksQueryable.Where(a => a.TaskCategory.Contains(filters.TaskCategory));
                }
                if (filters.Budget != null)
                {
                    tasksQueryable = tasksQueryable.Where(a => a.Project.Budget <= filters.Budget);
                }

                var tasksList = tasksQueryable.ToList();
                ExcelPackage excel = new ExcelPackage();

                var sheet = excel.Workbook.Worksheets.Add($"taskssListReport");

                sheet.Cells[1, 1].Value = "Task Name";
                sheet.Cells[1, 2].Value = "ProjectName";
                sheet.Cells[1, 3].Value = "Subject";
                sheet.Cells[1, 4].Value = "Priorty";
                sheet.Cells[1, 5].Value = "Status";
                sheet.Cells[1, 6].Value = "Progress";
                sheet.Cells[1, 7].Value = "Description";
                sheet.Cells[1, 8].Value = "Budget";
                sheet.Cells[1, 9].Value = "Expensis";
                sheet.Cells[1, 10].Value = "Start Date";
                sheet.Cells[1, 11].Value = "End Date";
                sheet.Cells[1, 12].Value = "Task Users";
                //---------------------------------------styling---------------------------------------------
                sheet.DefaultRowHeight = 12;
                sheet.Row(1).Height = 20;
                sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(1).Style.Font.Bold = true;
                sheet.Cells[1, 1, 1, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, 12].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 174, 81));
                sheet.Cells[1, 1, 1, 12].Style.Font.Color.SetColor(Color.White);


                int rowCount = 2;
                foreach (var task in tasksList)
                {
                    //decimal countOFprogress = project.WorkingHourseTrackings.Count();
                    //var totSumOfProgress = project.WorkingHourseTrackings.Sum(a => a.ProgressRate);
                    //decimal totalProgress = 0.0m;
                    //if (countOFprogress != 0 && totSumOfProgress != 0)
                    //{
                    //    totalProgress = totSumOfProgress ?? 0 / countOFprogress;

                    //}

                    //var totalWorkkingHours = project.WorkingHourseTrackings.Sum(a => a.TotalHours);

                    //var test = project.Tasks.SelectMany(a => a.TaskExpensis.Select(b => b.Amount)).Sum();

                    sheet.Cells[rowCount, 1].Value = task.Name;
                    if (task.Project != null)
                    {

                        sheet.Cells[rowCount, 2].Value = task.Project.SalesOffer.ProjectName;
                        sheet.Cells[rowCount, 4].Value = task.Project.Priorty.Name;
                    }
                    sheet.Cells[rowCount, 3].Value = task.TaskSubject;
                    sheet.Cells[rowCount, 5].Value = task.TaskDetails.FirstOrDefault().Status;

                    if (task.WorkingHourseTrackings.OrderByDescending(a => a.CreationDate).FirstOrDefault()?.ProgressRate != null)
                    {
                        sheet.Cells[rowCount, 6].Value = task.WorkingHourseTrackings.LastOrDefault().ProgressRate;
                    }


                    sheet.Cells[rowCount, 7].Value = task.Description;
                    sheet.Cells[rowCount, 8].Value = task.TaskDetails.FirstOrDefault().ProjectBudget;
                    sheet.Cells[rowCount, 9].Value = task.TaskExpensis.Sum(a => a.Amount);
                    sheet.Cells[rowCount, 10].Value = task.CreationDate.ToString();
                    sheet.Cells[rowCount, 11].Value = task.ExpireDate.ToString();
                    sheet.Cells[rowCount, 12].Value = task.CreatedByNavigation.FirstName + " " + task.CreatedByNavigation.LastName;

                    rowCount++;
                }

                for (int i = 1; i <= 12; i++)
                {
                    sheet.Column(i).AutoFit();
                }
                sheet.Column(3).Width = 45;
                sheet.Column(3).Style.WrapText = true;
                sheet.Column(7).Width = 45;
                sheet.Column(7).Style.WrapText = true;
                for (int i = 1; i <= tasksList.Count() + 1; i++)
                {
                    sheet.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                var path = $"Attachments\\{CompName}\\tasksListReport";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var currentDate = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\tasksListReport_{currentDate}.xlsx";

                if (Directory.Exists(savedPath))
                {
                    // Get all files in the directory
                    string[] files = Directory.GetFiles(savedPath);

                    // Iterate through each file and delete it
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }

                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                var fullPath = Globals.baseURL + "\\" + path + $"\\tasksListReport_{currentDate}.xlsx";

                response.Data = fullPath;

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

        //----------------------------------------------------periti APP-----------------------------------------------------

        /*public async  Task<BaseResponse> AddTaskBrowserTabs(AddTaskBrowserTabsDtoList dto)
        {
            var response = new BaseResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region check in DB
            var tasksIds = dto.TaskBrowserTabsList.Select(a => a.TaskID).ToList();
            var usersIds = dto.TaskBrowserTabsList.Select(a => a.UserID).ToList();

            var tasksData =await _unitOfWork.Tasks.FindAllAsync(a => tasksIds.Contains(a.Id));
            var usersData =await _unitOfWork.Users.FindAllAsync(a => usersIds.Contains(a.Id));

            int count = 1;
            foreach (var item in dto.TaskBrowserTabsList)
            {

                var task = tasksData.Where(a => a.Id == item.TaskID).FirstOrDefault();
                if(task == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no task with this ID at object number {count}";
                    response.Errors.Add(err);
                    return response;
                }

                var user = usersData.Where(a => a.Id == item.UserID).FirstOrDefault();
                if (user == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no User with this ID at object number {count}";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion

                #region date validation
                DateTime RequestDate = DateTime.Now;
                if (!string.IsNullOrEmpty(item.CreationDateTime))
                {
                    if (!DateTime.TryParse(item.CreationDateTime, out RequestDate))
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-02";
                        error.ErrorMSG = $"please Enter a valid Request Date at object number {count}";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                count++;
            }
            #endregion

            try
            {
                var listOTaskBrowserTabs = new List<TaskBrowserTab>();
                foreach (var tab in dto.TaskBrowserTabsList)
                {
                    var date = DateTime.Parse(tab.CreationDateTime);

                    var newTab = new TaskBrowserTab();
                    newTab.TaskId = tab.TaskID;
                    newTab.UserId = tab.UserID;
                    newTab.CreationDate = date;
                    newTab.TabName = tab.TabName;

                    listOTaskBrowserTabs.Add(newTab);
                }

                var savedTabs = _unitOfWork.TaskBrowserTabs.AddRange(listOTaskBrowserTabs);
                _unitOfWork.Complete();

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

        public async Task<BaseResponse> AddTaskApplicationOpen(AddTaskApplicationOpenList dto)
        {
            var response = new BaseResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            var tasksIds = dto.TaskApplicationOpen.Select(a => a.TaskID).ToList();
            var usersIds = dto.TaskApplicationOpen.Select(a => a.UserID).ToList();

            var tasksData =await _unitOfWork.Tasks.FindAllAsync(a => tasksIds.Contains(a.Id));
            var usersData =await _unitOfWork.Users.FindAllAsync(a => usersIds.Contains(a.Id));

            int count = 1;
            foreach (var app in dto.TaskApplicationOpen)
            {
                #region check in DB
                var task = tasksData.Where(a => a.Id == app.TaskID).FirstOrDefault();
                if (task == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no task with this ID at object number {count}";
                    response.Errors.Add(err);
                    return response;
                }

                var user = usersData.Where(a => a.Id == app.UserID).FirstOrDefault();
                if (user == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no User with this ID at object number {count}";
                    response.Errors.Add(err);
                    return response;
                }

                #endregion
                #region date validation
                DateTime RequestDate = DateTime.Now;
                if (!string.IsNullOrEmpty(app.CreationDateTime))
                {
                    if (!DateTime.TryParse(app.CreationDateTime, out RequestDate))
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-02";
                        error.ErrorMSG = $"please Enter a valid Request Date at obiect number {count}";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                #endregion
                count++;
            }


            try
            {
                var listOTaskBrowserTabs = new List<TaskApplicationOpen>();

                foreach (var app in dto.TaskApplicationOpen)
                {
                    var date = DateTime.Parse(app.CreationDateTime);

                    var newTab = new TaskApplicationOpen();
                    newTab.TaskId = app.TaskID;
                    newTab.UserId = app.UserID;
                    newTab.CreationDate = date;
                    newTab.AppName = app.AppName;

                    listOTaskBrowserTabs.Add(newTab);
                }

                var savedTabs = _unitOfWork.TaskApplicationsOpen.AddRange(listOTaskBrowserTabs);
                _unitOfWork.Complete();

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

        public async Task<BaseResponse> AddTaskScreenShot(AddTaskScreenShotList dto, string CompName)
        {
            var response = new BaseResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };


            var tasksIds = dto.ScreenShotsList.Select(a => a.TaskID).ToList();
            var usersIds = dto.ScreenShotsList.Select(a => a.UserID).ToList();

            var tasksData = await _unitOfWork.Tasks.FindAllAsync(a => tasksIds.Contains(a.Id));
            var usersData = await _unitOfWork.Users.FindAllAsync(a => usersIds.Contains(a.Id));

            int count = 1;
            foreach (var app in dto.ScreenShotsList)
            {
                #region check in DB
                var task = tasksData.Where(a => a.Id == app.TaskID).FirstOrDefault();
                if (task == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no task with this ID at object number {count}";
                    response.Errors.Add(err);
                    return response;
                }

                var user = usersData.Where(a => a.Id == app.UserID).FirstOrDefault();
                if (user == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no User with this ID at object number {count}";
                    response.Errors.Add(err);
                    return response;
                }

                #endregion
                #region date validation
                DateTime RequestDate = DateTime.Now;
                if (!string.IsNullOrEmpty(app.CreationDateTime))
                {
                    if (!DateTime.TryParse(app.CreationDateTime, out RequestDate))
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-02";
                        error.ErrorMSG = $"please Enter a valid Request Date at obiect number {count}";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                #endregion
                count++;
            }


            try
            {
                var listOTaskBrowserTabs = new List<TaskScreenShot>();

                foreach (var app in dto.ScreenShotsList)
                {
                    var date = DateTime.Parse(app.CreationDateTime);

                    var newTab = new TaskScreenShot();
                    newTab.TaskId = app.TaskID;
                    newTab.UserId = app.UserID;
                    newTab.CreationDate = date;

                    var saveDate = DateTime.Now.ToString("yyyyMMddHHmmssff");
                    var fileExtension = app.Img.FileName.Split('.').Last();
                    var virtualPath = $"Attachments\\{CompName}\\TaskScreenShots\\{app.TaskID}\\";
                    var FileName = System.IO.Path.GetFileNameWithoutExtension(app.Img.FileName.Trim().Replace(" ", ""));
                    var AttachPath = Common.SaveFileIFF(virtualPath, app.Img, FileName, fileExtension, _host);


                    newTab.ImgPath = AttachPath;

                    listOTaskBrowserTabs.Add(newTab);
                }

                var savedTabs = _unitOfWork.TaskscreenShots.AddRange(listOTaskBrowserTabs);
                _unitOfWork.Complete();

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

        public async Task<BaseResponse> AddTaskMonitor(AddTaskMonitorDtoList dto, string companyName)
        {
            var response = new BaseResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region check in DB
            var tasksIds = dto.TaskMonitorList.Select(a => a.TaskID).ToList();
            var usersIds = dto.TaskMonitorList.Select(a => a.UserID).ToList();

            var tasksData = await _unitOfWork.Tasks.FindAllAsync(a => tasksIds.Contains(a.Id));
            var usersData = await _unitOfWork.Users.FindAllAsync(a => usersIds.Contains(a.Id));

            int count = 1;
            foreach (var item in dto.TaskMonitorList)
            {

                var task = tasksData.Where(a => a.Id == item.TaskID).FirstOrDefault();
                if (task == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no task with this ID at object number {count}";
                    response.Errors.Add(err);
                    return response;
                }

                var user = usersData.Where(a => a.Id == item.UserID).FirstOrDefault();
                if (user == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no User with this ID at object number {count}";
                    response.Errors.Add(err);
                    return response;
                }
                count++;
            }
            #endregion

            try
            {
                var listOTaskMonitoring = new List<TaskUserMonitor>();
                foreach (var Monitor in dto.TaskMonitorList)
                {
                    var TaskMonitor = new TaskUserMonitor();
                    TaskMonitor.TaskId = Monitor.TaskID;
                    TaskMonitor.UserId = Monitor.UserID;
                    TaskMonitor.CreationDate = Monitor.CreationDateTime;
                    TaskMonitor.TabName = Monitor.TabName;
                    TaskMonitor.AppName = Monitor.AppName;
                    if (Monitor.Img != null)
                    {
                        var fileExtension = Monitor.Img.FileName.Split('.').Last();
                        var virtualPath = $"Attachments\\{companyName}\\TaskMonitor\\{Monitor.TaskID}\\";
                        var FileName = System.IO.Path.GetFileNameWithoutExtension(Monitor.Img.FileName.Trim().Replace(" ", ""));
                        var AttachPath = Common.SaveFileIFF(virtualPath, Monitor.Img, FileName, fileExtension, _host);
                        TaskMonitor.ImgPath = AttachPath;

                        var SmallAttachPath = Common.SaveSmallFileIff(virtualPath, Monitor.Img, FileName + "_Small", fileExtension, _host, 200, 300);
                        TaskMonitor.SmallImgPath = SmallAttachPath;
                    }

                    listOTaskMonitoring.Add(TaskMonitor);
                }
                var savedTabs = _unitOfWork.TaskUserMonitors.AddRange(listOTaskMonitoring);
                _unitOfWork.Complete();

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

        public async Task<BaseResponseWithData<GetTaskMonitorByUser>> GetTaskMonitoringListByUser([FromHeader] GetTaskMonitorFilters filters)
        {
            var response = new BaseResponseWithData<GetTaskMonitorByUser>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            response.Data = new GetTaskMonitorByUser();
            try
            {
                var Monitoring = _unitOfWork.TaskUserMonitors.FindAllQueryable(a => true, includes: new[] { "Task.Project.SalesOffer", "User" }).AsQueryable();

                if (filters.UserId != null)
                {
                    Monitoring = Monitoring.Where(a => a.UserId == filters.UserId).AsQueryable();
                }
                if (filters.TaskId != null)
                {
                    Monitoring = Monitoring.Where(a => a.TaskId == filters.TaskId).AsQueryable();
                }
                if (filters.ProjectId != null)
                {
                    Monitoring = Monitoring.Where(a => a.Task.ProjectId == filters.ProjectId).AsQueryable();
                }
                if (filters.From != null)
                {
                    Monitoring = Monitoring.Where(a => a.CreationDate >= filters.From).AsQueryable();
                }
                if (filters.To != null)
                {
                    Monitoring = Monitoring.Where(a => a.CreationDate <= filters.To).AsQueryable();
                }
                var byUsers = Monitoring.GroupBy(a => a.User.FirstName + " " + a.User.LastName).ToList();

                var list = new List<GetTaskMonitorData>();

                foreach (var user in byUsers)
                {
                    var item = new GetTaskMonitorData()
                    {
                        UserName = user.Key,
                        UserImg = user.FirstOrDefault()?.User?.PhotoUrl != null ? Globals.baseURL + user.FirstOrDefault()?.User?.PhotoUrl : null,
                        MonitoredTasks = user.GroupBy(a => a.Task.Name).Select(a => new MonitoredTasks()
                        {
                            TaskName = a.Key,
                            MonitorData = a.ToList().Select(x => new GetTaskMonitor()
                            {
                                AppName = x.AppName,
                                ImgPath = x.ImgPath != null ? Globals.baseURL + x.ImgPath : null,
                                SmallImgPath = x.SmallImgPath != null ? Globals.baseURL + x.SmallImgPath : null,
                                TabName = x.TabName,
                                ProjectName = x.Task?.Project?.SalesOffer?.ProjectName,
                                CreationDateTime = x.CreationDate.ToString()
                            }).ToList()
                        }).ToList(),
                    };
                    list.Add(item);
                }
                response.Data.UsersCount = Monitoring.ToList().DistinctBy(a => a.UserId).Count();
                response.Data.TaskCount = Monitoring.ToList().DistinctBy(a => a.TaskId).Count();
                response.Data.MonitorsData = list;
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


        public async Task<BaseResponseWithData<GetTaskMonitorByTaskGroup>> GetTaskMonitoringListByTask([FromHeader] GetTaskMonitorFilters filters)
        {
            var response = new BaseResponseWithData<GetTaskMonitorByTaskGroup>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            response.Data = new GetTaskMonitorByTaskGroup();
            try
            {
                var Monitoring = _unitOfWork.TaskUserMonitors.FindAllQueryable(a => true, includes: new[] { "Task.Project.SalesOffer", "User" }).AsQueryable();

                if (filters.UserId != null)
                {
                    Monitoring = Monitoring.Where(a => a.UserId == filters.UserId).AsQueryable();
                }
                if (filters.TaskId != null)
                {
                    Monitoring = Monitoring.Where(a => a.TaskId == filters.TaskId).AsQueryable();
                }
                if (filters.ProjectId != null)
                {
                    Monitoring = Monitoring.Where(a => a.Task.ProjectId == filters.ProjectId).AsQueryable();
                }
                if (filters.From != null)
                {
                    Monitoring = Monitoring.Where(a => a.CreationDate >= filters.From).AsQueryable();
                }
                if (filters.To != null)
                {
                    Monitoring = Monitoring.Where(a => a.CreationDate <= filters.To).AsQueryable();
                }
                var byTasks = Monitoring.GroupBy(a => a.Task.Name).ToList();

                var list = new List<GetTaskMonitorByTask>();

                foreach (var task in byTasks)
                {
                    var item = new GetTaskMonitorByTask()
                    {
                        TaskName = task.Key,
                        MonitoredUsersList = task.GroupBy(a => a.User.FirstName + " " + a.User.LastName).Select(a => new MonitoredUsers()
                        {
                            UserName = a.Key,
                            UserImg = a.FirstOrDefault()?.User?.PhotoUrl != null ? Globals.baseURL + a.FirstOrDefault()?.User?.PhotoUrl : null,
                            MonitorData = a.ToList().Select(x => new GetTaskMonitor()
                            {
                                AppName = x.AppName,
                                ImgPath = x.ImgPath != null ? Globals.baseURL + x.ImgPath : null,
                                SmallImgPath = x.SmallImgPath != null ? Globals.baseURL + x.SmallImgPath : null,
                                TabName = x.TabName,
                                ProjectName = x.Task?.Project?.SalesOffer?.ProjectName,
                                CreationDateTime = x.CreationDate.ToString()
                            }).ToList()
                        }).ToList(),
                    };
                    list.Add(item);
                }
                response.Data.UsersCount = Monitoring.ToList().DistinctBy(a => a.UserId).Count();
                response.Data.TaskCount = Monitoring.ToList().DistinctBy(a => a.TaskId).Count();
                response.Data.MonitorsData = list;
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

        public async Task<BaseResponseWithData<GetTaskBrowserTabsDtoList>> GetTaskBrowserTabs(long? UserID, long? TaskID)
        {
            var response = new BaseResponseWithData<GetTaskBrowserTabsDtoList>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region check DB
            if (UserID != null)
            {
                var user = _unitOfWork.Users.Find(a => a.Id == UserID);
                if (user == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no user with this ID ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if (TaskID != null)
            {
                var task = _unitOfWork.Tasks.Find(a => a.Id == TaskID);
                if (task == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no task with this ID ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            #endregion

            try
            {
                var tabs = _unitOfWork.TaskBrowserTabs.FindAllQueryable(a => true, new[] { "User", "Task" });
                if (UserID != null)
                {
                    tabs = tabs.Where(a => a.UserId == UserID);
                }
                if (TaskID != null)
                {
                    tabs = tabs.Where(a => a.TaskId == TaskID);
                }

                var tabsList = tabs.ToList();

                var data = new List<GetTaskBrowserTabsDto>();

                foreach (var tab in tabsList)
                {
                    var tabData = new GetTaskBrowserTabsDto();

                    tabData.UserID = tab.UserId;
                    tabData.UserName = tab.User.FirstName + " " + tab.User.LastName;
                    tabData.TaskID = tab.TaskId;
                    tabData.taskName = tab.Task.Name;
                    tabData.CreationDateTime = tab.CreationDate.ToString("yyyy-MM-dd HH:mm:ss");
                    tabData.TabName = tab.TabName;

                    data.Add(tabData);
                }
                var dtoData = new GetTaskBrowserTabsDtoList();
                dtoData.TabsList = data;
                response.Data = dtoData;
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

        public async Task<BaseResponseWithData<GetTaskAppsOpenList>> GetTaskAppsOpen(long? UserID, long? TaskID)
        {
            var response = new BaseResponseWithData<GetTaskAppsOpenList>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region check DB
            if (UserID != null)
            {
                var user = _unitOfWork.Users.Find(a => a.Id == UserID);
                if (user == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no user with this ID ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if (TaskID != null)
            {
                var task = _unitOfWork.Tasks.Find(a => a.Id == TaskID);
                if (task == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no task with this ID ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            #endregion

            try
            {
                var tabs = _unitOfWork.TaskApplicationsOpen.FindAllQueryable(a => true, new[] { "User", "Task" });
                if (UserID != null)
                {
                    tabs = tabs.Where(a => a.UserId == UserID);
                }
                if (TaskID != null)
                {
                    tabs = tabs.Where(a => a.TaskId == TaskID);
                }

                var tabsList = tabs.ToList();

                var data = new List<GetTaskAppsOpenDto>();

                foreach (var app in tabsList)
                {
                    var tabData = new GetTaskAppsOpenDto();

                    tabData.UserID = app.UserId;
                    tabData.UserName = app.User.FirstName + " " + app.User.LastName;
                    tabData.TaskID = app.TaskId;
                    tabData.taskName = app.Task.Name;
                    tabData.CreationDateTime = app.CreationDate.ToString("yyyy-MM-dd HH:mm:ss");
                    tabData.AppName = app.AppName;

                    data.Add(tabData);
                }
                var dtoData = new GetTaskAppsOpenList();
                dtoData.AppsList = data;
                response.Data = dtoData;
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

        public async Task<BaseResponseWithData<GetTaskScreenShotDtoList>> GetTaskScreenShot(long? UserID, long? TaskID)
        {
            var response = new BaseResponseWithData<GetTaskScreenShotDtoList>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region check DB
            if (UserID != null)
            {
                var user = _unitOfWork.Users.Find(a => a.Id == UserID);
                if (user == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no user with this ID ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if (TaskID != null)
            {
                var task = _unitOfWork.Tasks.Find(a => a.Id == UserID);
                if (task == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"There is no task with this ID ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            #endregion

            try
            {
                var tabs = _unitOfWork.TaskscreenShots.FindAllQueryable(a => true, new[] { "User", "Task" });
                if (UserID != null)
                {
                    tabs = tabs.Where(a => a.UserId == UserID);
                }
                if (TaskID != null)
                {
                    tabs = tabs.Where(a => a.TaskId == TaskID);
                }

                var tabsList = tabs.ToList();

                var data = new List<GetTaskScreenShotDto>();

                foreach (var screenShot in tabsList)
                {
                    var screenShotData = new GetTaskScreenShotDto();

                    screenShotData.UserID = screenShot.UserId;
                    screenShotData.UserName = screenShot.User.FirstName + " " + screenShot.User.LastName;
                    screenShotData.TaskID = screenShot.TaskId;
                    screenShotData.taskName = screenShot.Task.Name;
                    screenShotData.CreationDateTime = screenShot.CreationDate.ToString("yyyy-MM-dd HH:mm:ss");
                    screenShotData.ImgPath = Globals.baseURL + screenShot.ImgPath;

                    data.Add(screenShotData);
                }
                var dtoData = new GetTaskScreenShotDtoList();
                dtoData.ScreenShots = data;
                response.Data = dtoData;
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

        public async Task<BaseResponseWithData<List<GetTasksOfUser>>> GetTasksOfUserInDay([FromHeader] DateTime Day, [FromHeader] long UserID)
        {
            var response = new BaseResponseWithData<List<GetTasksOfUser>>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                var workinghours = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.HrUser.UserId == UserID && a.Date.Date == Day.Date && a.TaskId != null, includes: new[] { "Task", "HrUser" }).ToList();

                var list = workinghours.Select(a => new GetTasksOfUser()
                {
                    TaskID = a.TaskId ?? 0,
                    TaskName = a.Task.Name
                }).ToList();

                response.Data = list.DistinctBy(a => a.TaskID).ToList();
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

        public GetTaskResponse GetTask([FromHeader] GetTaskHeader header, long userID)
        {
            GetTaskResponse response = new GetTaskResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var GetTaskList = new List<TasksData>();
                //var tasks =new List<GetTaskIndex>().AsQueryable();
                if (response.Result)
                {
                    //level_1
                    var tasksIDList = new List<long>();
                    long UserId = userID;
                    var ListUserPM = new List<long>();
                    ListUserPM = _unitOfWork.Tasks.FindAll(a => a.Project != null, includes: new[] { "Project.ProjectAssignUsers" }).SelectMany(a => a.Project?.ProjectAssignUsers.Where(x => x.RoleId == 145 && a.Active).Select(a => a.UserId)).ToList();
                    if (ListUserPM.Contains(userID))
                    {
                        var ids = _unitOfWork.Tasks.FindAll(a => a.Project.ProjectAssignUsers.Any(x => x.UserId == userID), includes: new[] { "Project.ProjectAssignUsers" }).Select(a => a.Id);

                        tasksIDList.AddRange(ids);
                    }
                    if (header.ToUserID != 0)
                    {
                        UserId = header.ToUserID;
                    }
                    var groupUsers = _unitOfWork.GroupUsers.GetAll();
                    var relevantGroupUsers = groupUsers.Where(a => a.UserId == UserId).Select(a => a.GroupId).ToList().ToList();
                    var nextIds = _unitOfWork.TaskPermissions
                        .FindAll(x =>
                            (x.UserGroupId == UserId && !x.IsGroup) ||
                            (x.IsGroup && relevantGroupUsers.Contains(x.UserGroupId))
                        )
                        .Select(x => x.TaskId)
                        .ToList();

                    tasksIDList.AddRange(nextIds);

                    tasksIDList = tasksIDList.Distinct().ToList();
                    //level_2
                    var FinalTasksQuery = _unitOfWork.Tasks.FindAllQueryable(x => tasksIDList.Contains(x.Id), includes: new[] { "WorkingHourseTrackings", "CreatedByNavigation" }).AsQueryable();

                    if (header.NotActive == true)
                    {
                        FinalTasksQuery = FinalTasksQuery.Where(x => x.Active == false).AsQueryable();
                    }
                    else
                    {
                        FinalTasksQuery = FinalTasksQuery.Where(x => x.Active == true).AsQueryable();
                    }

                    if (header.IsArchived)
                    {
                        FinalTasksQuery = FinalTasksQuery.Where(x => x.IsArchived == true).AsQueryable();
                    }
                    var FinalTasks = FinalTasksQuery.ToList();
                    //level_3

                    var replies = _unitOfWork.TaskUserReplies.FindAll(x => tasksIDList.Contains(x.TaskId)).ToList();
                    var details = _unitOfWork.TaskDetails.FindAll(x => tasksIDList.Contains(x.TaskId)).ToList();
                    var types = _unitOfWork.TaskTypes.GetAll();
                    var taskCount = _unitOfWork.Tasks.GetAll();
                    var receivers = _unitOfWork.TaskFlagsOwnerRecievers.FindAll(x => tasksIDList.Contains(x.TaskId)).ToList();
                    var tasks = FinalTasks.Select(a => new GetTaskIndex
                    {
                        ID = a.Id,
                        Active = a.Active,
                        IsFinished = replies.Where(x => x.TaskId == a.Id).Select(x => x.IsFinished).FirstOrDefault(),
                        NeedApproval = details.Where(x => x.TaskId == a.Id).Select(x => x.NeedApproval).FirstOrDefault(),
                        IsTaskOwner = a.CreatedBy == userID,
                        Piriority = details.Where(x => x.TaskId == a.Id).Select(x => x.Priority).FirstOrDefault(),
                        Status = details.Where(x => x.TaskId == a.Id).Select(x => x.Status).FirstOrDefault(),
                        Name = a.Name,
                        Description = a.Name,
                        ExpireDate = a.ExpireDate?.ToString(),
                        CreationDate = a.CreationDate.ToString(),
                        CreatedBy = a.CreatedBy,
                        CreatoreName = a.CreatedByNavigation.FirstName + " " + a.CreatedByNavigation.LastName,
                        TaskCategory = a.TaskCategory,
                        /* GroupReply = a.GroupReply,
                        TaskPage = a.TaskPage,
                        TaskUser = a.TaskUser,
                        TaskTypeName = types.Where(x => x.Id == a.TaskTypeId).Select(x => x.Name).FirstOrDefault(),*/
                        TaskSubject = a.TaskSubject,
                        //BranchName = a.BranchId != null? Common.GetBranchName((int)a.BranchId, _Context):null,
                        CreatorPhoto = a.CreatedByNavigation.PhotoUrl != null ? Globals.baseURL + a.CreatedByNavigation.PhotoUrl : null,
                        TaskStatus = "",
                        //TaskCount = 0,
                        RejectedNo = replies.Where(x => x.TaskId == a.Id && x.Approval == false).Count(),
                        //TaskCreatorCount = taskCount.Where(x => x.CreatedBy == a.CreatedBy).Count(),
                        TaskTypeID = a.TaskTypeId,
                        ProjectID = a.ProjectId,
                        ProjectSprintID = a.ProjectSprintId,
                        ProjectWorkFlowID = a.ProjectWorkFlowId,
                        //taskAttachements = details.Where(x => x.TaskId == a.Id).Select(x => x.CreatorAttachement).FirstOrDefault() != null ? Globals.baseURL + details.Where(x => x.TaskId == a.Id).Select(x => x.CreatorAttachement).FirstOrDefault()?.TrimStart('~') : null,
                        TaskUserGroupList = receivers.Where(x => x.TaskId == a.Id && x.UserId == userID).Select(x => new TaskUserGroupData()
                        {
                            TaskID = x.TaskId,
                            UserGroupID = x.UserId,
                            Flag = x.Flag,
                            Read = x.Read,
                            Star = x.Star
                        }).ToList(),
                        LastPrgress = a.WorkingHourseTrackings.OrderBy(a => a.CreationDate).LastOrDefault()?.ProgressRate ?? 0
                    }).AsQueryable();


                    if (header.TaskID != 0)
                    {
                        tasks = tasks.Where(a => a.ID == header.TaskID).AsQueryable();
                    }
                    if (header.TaskTypeID != 0)
                    {
                        tasks = tasks.Where(a => a.TaskTypeID == header.TaskTypeID).AsQueryable();
                    }
                    if (!string.IsNullOrWhiteSpace(header.Status))
                    {
                        tasks = tasks.Where(a => a.Status != null && a.Status == header.Status).AsQueryable();
                    }
                    if (!string.IsNullOrWhiteSpace(header.TaskCategory))
                    {
                        tasks = tasks.Where(a => a.TaskCategory == header.TaskCategory).AsQueryable();
                    }
                    if (!string.IsNullOrWhiteSpace(header.PriorityFilter))
                    {
                        tasks = tasks.Where(a => a.Piriority.Trim().ToLower() == header.PriorityFilter.Trim().ToLower()).AsQueryable();
                    }
                    if (!string.IsNullOrWhiteSpace(header.TaskName))
                    {
                        tasks = tasks.Where(a => a.Name.ToLower().Trim().Contains(header.TaskName.ToLower().Trim())).AsQueryable();
                    }
                    if (header.NeedApproval != null)
                    {
                        tasks = tasks.Where(a => a.NeedApproval == header.NeedApproval).AsQueryable();
                    }
                    if (header.IsFinished != null)
                    {
                        tasks = tasks.Where(a => a.IsFinished == header.IsFinished).AsQueryable();
                    }
                    DateTime DateFrom = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(header.DateFrom) && DateTime.TryParse(header.DateFrom, out DateFrom))
                    {
                        DateFrom = DateTime.Parse(header.DateFrom);
                    }

                    DateTime DateTo = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(header.DateTo) && DateTime.TryParse(header.DateTo, out DateTo))
                    {
                        DateTo = DateTime.Parse(header.DateTo);
                    }
                    if (DateFrom != DateTime.MinValue)
                    {
                        tasks = tasks.Where(x => DateTime.Parse(x.CreationDate).Date >= DateFrom.Date).AsQueryable();

                    }
                    if (DateTo != DateTime.MinValue)
                    {
                        tasks = tasks.Where(x => DateTime.Parse(x.CreationDate).Date <= DateTo.Date).AsQueryable();

                    }
                    if (!string.IsNullOrWhiteSpace(header.SearchKey))
                    {
                        tasks = tasks.Where(x => x.TaskSubject.Contains(header.SearchKey) || x.Description.Contains(header.SearchKey) || x.Name.Contains(header.SearchKey));
                    }
                    if (header.Delayed)
                    {
                        tasks = tasks.Where(a => a.Status != null && (a.Status.Trim() == "Waiting Approval" || a.Status.Trim() == "Open") &&
                        (a.ExpireDate != null ? DateTime.Parse(a.ExpireDate).Date < DateTime.Now.Date : false)).AsQueryable();
                    }
                    if (header.Read != null)
                    {
                        tasks = tasks.Where(a => a.TaskUserGroupList.Any(z => z.Read == header.Read));
                    }
                    if (header.Flag != null)
                    {
                        tasks = tasks.Where(a => a.TaskUserGroupList.Any(z => z.Flag == header.Flag));
                    }
                    if (header.Star != null)
                    {
                        tasks = tasks.Where(a => a.TaskUserGroupList.Any(z => z.Star == header.Star));
                    }

                    var taskList = tasks.ToList();
                    foreach (var a in taskList)
                    {
                        var LastTaskStatus = _unitOfWork.TaskUserReplies.FindAll(x => x.TaskId == a.ID).OrderBy(x => x.CreationDate).LastOrDefault();
                        if (LastTaskStatus != null)
                        {
                            if (LastTaskStatus.IsFinished == null && LastTaskStatus.Approval == null)
                            {
                                a.TaskStatus = "Need Approval";
                            }
                            else if (LastTaskStatus.IsFinished == true)
                            {
                                a.TaskStatus = "Waiting Approval";
                            }
                            else if (LastTaskStatus.Approval != null && LastTaskStatus.Approval == false)
                            {
                                a.TaskStatus = "Rejected";
                            }
                            else if (LastTaskStatus.Approval != null && LastTaskStatus.Approval == true)
                            {
                                a.TaskStatus = "Approved";
                            }
                        }

                    }
                    var taskListPaged = PagedList<GetTaskIndex>.Create(taskList.OrderByDescending(a => DateTime.Parse(a.CreationDate)).AsQueryable(), header.CurrentPage, header.NumberOfItemsPerPage);
                    response.TasksList = taskListPaged;
                    response.TaskCount = taskList.Count();
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

        public GetTaskTypePermissionResponse GetTaskTypePermissionList()
        {
            GetTaskTypePermissionResponse response = new GetTaskTypePermissionResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var TaskPermissionResponseList = new List<TaskPermissionData>();
                if (response.Result)
                {
                    if (response.Result)
                    {
                        var TaskPermissionDB = _unitOfWork.PermissionLevels.GetAll();


                        if (TaskPermissionDB != null && TaskPermissionDB.Count() > 0)
                        {

                            foreach (var TaskPermissionDBOBJ in TaskPermissionDB)
                            {
                                var TaskPermissionDBOBJResponse = new TaskPermissionData();

                                TaskPermissionDBOBJResponse.ID = TaskPermissionDBOBJ.Id;

                                TaskPermissionDBOBJResponse.TaskPermissionName = TaskPermissionDBOBJ.Name;




                                TaskPermissionResponseList.Add(TaskPermissionDBOBJResponse);
                            }



                        }

                    }

                }
                response.TaskPermissionList = TaskPermissionResponseList;
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

        public async Task<GetTaskDetailsResponse> GetTaskDetailsPerID([FromHeader] long TaskID)
        {
            GetTaskDetailsResponse response = new GetTaskDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var TaskDetails = new GetTaskDetailsData();
                var GetTaskDetailsListList = new List<GetTaskDetailsList>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var TaskDetailsDB = (await _unitOfWork.TaskDetails.FindAllAsync(x => x.TaskId == TaskID)).FirstOrDefault();
                        var TaskFlagsOwnerAndRecieversDB = (await _unitOfWork.TaskFlagsOwnerRecievers.FindAllAsync(x => x.TaskId == TaskID)).FirstOrDefault();
                        var TaskUserReplyDB = (await _unitOfWork.TaskUserReplies.FindAllAsync(x => x.TaskId == TaskID)).ToList();


                        if (TaskDetailsDB != null)
                        {
                            if (TaskFlagsOwnerAndRecieversDB != null)
                            {






                                TaskDetails.TaskID = TaskDetailsDB.TaskId;

                                TaskDetails.NeedApproval = (bool)TaskDetailsDB.NeedApproval;

                                TaskDetails.Priority = TaskDetailsDB.Priority?.Trim();

                                TaskDetails.Status = TaskDetailsDB.Status;

                                TaskDetails.CreatorAttachement = TaskDetailsDB.CreatorAttachement!=null? Globals.baseURL+"/"+ TaskDetailsDB.CreatorAttachement:null;
                                TaskDetails.Flag = (bool)TaskFlagsOwnerAndRecieversDB.Flag;
                                TaskDetails.Star = (bool)TaskFlagsOwnerAndRecieversDB.Star;
                                TaskDetails.Read = (bool)TaskFlagsOwnerAndRecieversDB.Read;





                                foreach (var TaskUserReplyDBOBJ in TaskUserReplyDB)
                                {
                                    var TaskUserReplyDBResponse = new GetTaskDetailsList();



                                    TaskUserReplyDBResponse.TaskID = TaskUserReplyDBOBJ.TaskId;

                                    TaskUserReplyDBResponse.ReplyAttach = TaskUserReplyDBOBJ.ReplyAttach;
                                    TaskUserReplyDBResponse.RecieverUserID = TaskUserReplyDBOBJ.RecieverUserId;
                                    TaskUserReplyDBResponse.IsFinished = (bool)TaskUserReplyDBOBJ.IsFinished;
                                    TaskUserReplyDBResponse.CreatorAttach = TaskUserReplyDBOBJ.CreatorAttach;
                                    TaskUserReplyDBResponse.CommentReply = TaskUserReplyDBOBJ.CommentReply;
                                    TaskUserReplyDBResponse.ApprovalComment = TaskUserReplyDBOBJ.ApprovalComment;
                                    TaskUserReplyDBResponse.Approval = (bool)TaskUserReplyDBOBJ.Approval;







                                    GetTaskDetailsListList.Add(TaskUserReplyDBResponse);
                                }
                                TaskDetails.GetTaskDetailsList = GetTaskDetailsListList;


                                response.GetTaskDetailsList = TaskDetails;
                            }
                        }

                    }

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

        public async Task<BaseResponseWithId<long>> UpdateTaskReadStatus(AddTaskObjData Request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (string.IsNullOrEmpty(Request.ID.ToString()))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Task ID Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        if (Request.ID != 0)
                        {
                            var TaskDBUpdateDB = (await _unitOfWork.TaskFlagsOwnerRecievers.FindAllAsync(x => x.TaskId == Request.ID)).FirstOrDefault();
                            if (TaskDBUpdateDB != null)
                            {
                                // Update

                                TaskDBUpdateDB.Read = true;
                                _unitOfWork.TaskFlagsOwnerRecievers.Update(TaskDBUpdateDB);
                                _unitOfWork.Complete();

                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Task Doesn't Exist!!";
                                Response.Errors.Add(error);
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

        public async Task<BaseResponseWithId<long>> AddTaskDetails(GetTaskDetailsData Request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var TaskDetails = new GetTaskDetailsData();
                var GetTaskDetailsListList = new List<GetTaskDetailsList>();
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    //check sent data
                    if (Request.TaskID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Task ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {
                        var details = new TaskDetail()
                        {
                            TaskId = Request.TaskID,
                            Status = Request.Status,
                            Priority = Request.Priority?.Trim(),
                            NeedApproval = Request.NeedApproval,
                            CreatorAttachement = Request.CreatorAttachement
                        };
                        _unitOfWork.TaskDetails.Add(details);
                        var TaskIDInsertion = _unitOfWork.Complete();



                        if (TaskIDInsertion > 0)
                        {
                            Response.ID = details.Id;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Insert this Task Details !!";
                            Response.Errors.Add(error);
                        }
                    }








                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err25";
                    error.ErrorMSG = "Faild To Insert this Task !!";
                    Response.Errors.Add(error);
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

        public async Task<BaseResponseWithId<long>> AddTaskFlagsPerUser(GetTaskDetailsData Request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var TaskDetails = new GetTaskDetailsData();
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    //check sent data
                    if (Request.TaskID == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Task ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }




                    if (Response.Result)
                    {

                        var TaskUserFlagsAlreadyFound = (await _unitOfWork.TaskFlagsOwnerRecievers.FindAllAsync(x => x.TaskId == Request.TaskID && x.UserId == validation.userID)).FirstOrDefault();
                        if (TaskUserFlagsAlreadyFound == null)
                        {
                            var TaskUserDetailsDB = new TaskFlagsOwnerReciever();

                            TaskUserDetailsDB.TaskId = Request.TaskID;
                            TaskUserDetailsDB.UserId = validation.userID;
                            TaskUserDetailsDB.Read = Request.Read;
                            TaskUserDetailsDB.Star = Request.Star;
                            TaskUserDetailsDB.Flag = Request.Flag;
                            _unitOfWork.TaskFlagsOwnerRecievers.Add(TaskUserDetailsDB);
                            var Res =_unitOfWork.Complete();
                            if (Res > 0)
                            {
                                Response.ID = TaskUserDetailsDB.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Task Flags !!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This User Has a previous Record with task flags !!";
                            Response.Errors.Add(error);
                        }
                        //}
                        //else
                        //{
                        //    Response.Result = false;
                        //    Error error = new Error();
                        //    error.ErrorCode = "Err25";
                        //    error.ErrorMSG = "This User ID isn't assigned to this TaskID !!";
                        //    Response.Errors.Add(error);
                        //}


                    }








                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err25";
                    error.ErrorMSG = "Faild To Insert this Task !!";
                    Response.Errors.Add(error);
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
    }
}
