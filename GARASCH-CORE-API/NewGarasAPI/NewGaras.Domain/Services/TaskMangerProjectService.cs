using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NewGaras.Domain.DTO.HrUser;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Attendance;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.DTO.ProjectLetterOfCredit;
using NewGaras.Infrastructure.DTO.ProjectPayment;
using NewGaras.Infrastructure.DTO.ProjectSprint;
using NewGaras.Infrastructure.DTO.TaskMangerProject;
using NewGaras.Infrastructure.DTO.TaskUnitRateService;
using NewGaras.Infrastructure.DTO.User;
using NewGaras.Infrastructure.DTO.WorkFlow;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.ProjectLetterOfCredit;
using NewGaras.Infrastructure.Models.TaskManager;
using NewGaras.Infrastructure.Models.TaskMangerProject;
using NewGaras.Infrastructure.Models.TaskMangerProject.Filters;
using NewGarasAPI.Models.Project.UsedInResponses;
using NewGarasAPI.Models.User;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = NewGarasAPI.Models.Common.Error;
using Font = System.Drawing.Font;
using System.Drawing;
using System.Web;
using NewGaras.Infrastructure.Models.ProjectPayment;
using DocumentFormat.OpenXml.Spreadsheet;
using Color = System.Drawing.Color;

namespace NewGaras.Domain.Services
{
    public class TaskMangerProjectService : ITaskMangerProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly ITaskService _taskService;
        private readonly ISprintService _sprintService;
        //private readonly IAttendanceService _attendanceService;
        public TaskMangerProjectService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, ITaskService taskService, ISprintService sprintService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _taskService = taskService;
            _sprintService = sprintService;
            //_attendanceService = attendanceService;
        }

        public BaseResponseWithId<long> AddTaskMangerProject(AddTaskMangerProjectDto Dto, long Creator, string CompName)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region Date Validation
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
            if (!DateTime.TryParse(Dto.EndDate, out endDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid End Date:";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            #region Check availablty in DB
            var client = _unitOfWork.Clients.GetById(Dto.ClientID);
            if (client == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Client with this Id";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.CurrencyID != null)
            {
                var currency = _unitOfWork.Currencies.GetById(Dto.CurrencyID ?? 0);
                if (currency == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No currency with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }

            //var contactPerson = _unitOfWork.ProjectContactPersons.GetById(Dto.ContactPersonID);
            //if (contactPerson == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Project Contact Persons with this Id";
            //    response.Errors.Add(error);
            //    return response;
            //}

            if (Dto.PriorityID != null)
            {
                var priorty = _unitOfWork.Prioritys.GetById((int)Dto.PriorityID);
                if (priorty == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No priorty with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }
            if (Dto.Budget != null)
            {
                if (Dto.CurrencyID == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "There Is Budget added You must Enter Currency ID";
                    response.Errors.Add(error);
                    return response;
                }
                var currency = _unitOfWork.Currencies.GetById((int)Dto.CurrencyID);
                if (currency == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Currency with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }
            if (Dto.ProjectContactPersonCountryId != null)
            {

                var country = _unitOfWork.Country.GetById((int)Dto.ProjectContactPersonCountryId);
                if (country == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Country with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }
            if (Dto.ProjectContactPersonGovernorateID != null)
            {

                var Governorate = _unitOfWork.Governorates.GetById((int)Dto.ProjectContactPersonGovernorateID);
                if (Governorate == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Government with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }
            if (Dto.ProjectContactPersonAreaId != null)
            {
                var Area = _unitOfWork.Areas.GetById(Dto.ProjectContactPersonAreaId ?? 0);
                if (Area == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Area with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }
            #endregion

            #region Who can create project
            var userCanAdd = _unitOfWork.UserRoles.FindAll(a => a.UserId == Creator && (a.RoleId == 145 || a.RoleId == 146 || a.RoleId == 1)).FirstOrDefault();
            if (userCanAdd == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "You are not authorized to Add a new project";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                //-----------------------------Add Data To Salesoffer table----------------------------------
                int branchIdToBeAdded = 0;
                var BranchFromDatabase = _unitOfWork.Users.GetById(Creator).BranchId;
                if (BranchFromDatabase == null)
                {
                    var mainbranch = _unitOfWork.Branches.Find(a => a.IsMain == true);
                    branchIdToBeAdded = mainbranch.Id;
                }

                var newSalesoffer = new SalesOffer();
                newSalesoffer.StartDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                newSalesoffer.EndDate = DateOnly.FromDateTime(endDate);
                newSalesoffer.SalesPersonId = Creator;
                newSalesoffer.CreatedBy = Creator;
                newSalesoffer.CreationDate = DateTime.Now;
                newSalesoffer.ModifiedBy = Creator;
                newSalesoffer.Modified = DateTime.Now;
                newSalesoffer.Active = true;
                newSalesoffer.Completed = true;
                newSalesoffer.BranchId = Dto.BranchId != 0 ? Dto.BranchId : BranchFromDatabase != null ? BranchFromDatabase ?? 0 : branchIdToBeAdded;
                newSalesoffer.OfferType = "Task Manger";
                newSalesoffer.ProjectLocation = Dto.ProjectLocation;
                newSalesoffer.ProjectName = Dto.ProjectName;
                newSalesoffer.ClientId = Dto.ClientID;

                var salesOfferSaved = _unitOfWork.SalesOffers.Add(newSalesoffer);
                _unitOfWork.Complete();

                //-----------------------------Add Data To Project table----------------------------------
                var newProject = new Project();
                newProject.SalesOfferId = salesOfferSaved.Id;
                newProject.Closed = false;
                newProject.Revision = 0;        //in edit the number will rise up 
                newProject.StartDate = startDate;
                newProject.EndDate = endDate;
                newProject.CreatedBy = Creator;
                newProject.CreationDate = DateTime.Now;
                newProject.BranchId = Dto.BranchId != 0 ? Dto.BranchId : BranchFromDatabase != null ? BranchFromDatabase ?? 0 : branchIdToBeAdded;
                newProject.Active = true;
                newProject.ProjectDescription = Dto.ProjectDescription;
                newProject.Billable = Dto.Billable;
                newProject.TimeTracking = Dto.TimeTracking;
                newProject.PriortyId = Dto.PriorityID != null ? Dto.PriorityID : null;
                newProject.Active = Dto.Active ?? true;
                newProject.CostTypeId = Dto.costTypeID;
                if (Dto.Budget != null)
                {
                    newProject.Budget = Dto.Budget;
                    newProject.CurrencyId = Dto.CurrencyID;
                }
                newProject.FromTaskManger = true;

                var projectSaved = _unitOfWork.Projects.Add(newProject);
                _unitOfWork.Complete();

                //-----------------------------Add Data To Project table----------------------------------
                //var newContactPersonList = new List<ProjectContactPerson>();


                //-----------------------------Add Data To Project Attachment table----------------------------------
                var newProjectAttachmentList = new List<ProjectAttachment>();

                //if (Dto.AttachmentList != null)
                //{
                //    foreach (var attachment in Dto.AttachmentList)
                //    {
                //        var projectAttachment = new ProjectAttachment();

                //        var fileExtension = attachment.FileName.Split('.').Last();
                //        var virtualPath = $"Attachments\\{CompName}\\TaskManger\\{projectSaved.Id}\\";
                //        var FileName = System.IO.Path.GetFileNameWithoutExtension(attachment.FileName.Trim().Replace(" ", ""));
                //        var AttachPath = Common.SaveFileIFF(virtualPath, attachment, FileName, fileExtension, _host);

                //        projectAttachment.ProjectId = projectSaved.Id;
                //        projectAttachment.AttachmentPath = AttachPath;
                //        projectAttachment.CreatedBy = Creator;
                //        projectAttachment.CreationDate = DateTime.Now;
                //        projectAttachment.FileName = FileName;
                //        projectAttachment.FileExtenssion = fileExtension;

                //        newProjectAttachmentList.Add(projectAttachment);
                //    }

                //    var ListOfAtachmentAdded = _unitOfWork.ProjectAttachments.AddRange(newProjectAttachmentList);
                //    _unitOfWork.Complete();
                //}


                if (Dto.AttachmentListTest != null)
                {
                    foreach (var attachment in Dto.AttachmentListTest)
                    {
                        var projectAttachment = new ProjectAttachment();

                        var fileExtension = attachment.Content.FileName.Split('.').Last();
                        var virtualPath = $"Attachments\\{CompName}\\TaskManger\\{projectSaved.Id}\\";
                        var FileName = System.IO.Path.GetFileNameWithoutExtension(attachment.Content.FileName.Trim().Replace(" ", ""));
                        var AttachPath = Common.SaveFileIFF(virtualPath, attachment.Content, FileName, fileExtension, _host);

                        projectAttachment.ProjectId = projectSaved.Id;
                        projectAttachment.AttachmentPath = AttachPath;
                        projectAttachment.CreatedBy = Creator;
                        projectAttachment.CreationDate = DateTime.Now;
                        projectAttachment.FileName = FileName;
                        projectAttachment.FileExtenssion = fileExtension;

                        newProjectAttachmentList.Add(projectAttachment);
                    }

                    var ListOfAtachmentAdded = _unitOfWork.ProjectAttachments.AddRange(newProjectAttachmentList);
                    _unitOfWork.Complete();
                }

                //------------------------------Add Data for project Contact table-----------------------------
                if (
                    Dto.ProjectContactPersonCountryId != null && Dto.ProjectContactPersonCountryId != 0 &&
                    Dto.ProjectContactPersonGovernorateID != null && Dto.ProjectContactPersonGovernorateID != 0 &&
                    !string.IsNullOrWhiteSpace(Dto.ProjectContactPersonName) &&
                    !string.IsNullOrWhiteSpace(Dto.ProjectContactPersonMob)
                    )
                {
                    Dto.ProjectContactPersonAddress = string.IsNullOrWhiteSpace(Dto.ProjectContactPersonAddress) ? " " : Dto.ProjectContactPersonAddress;

                    var newContactPerson = new ProjectContactPerson();
                    newContactPerson.ProjectId = newProject.Id;
                    newContactPerson.CountryId = (int)Dto.ProjectContactPersonCountryId;
                    newContactPerson.GovernorateId = (int)Dto.ProjectContactPersonGovernorateID;
                    newContactPerson.AreaId = Dto.ProjectContactPersonAreaId;
                    newContactPerson.Address = Dto.ProjectContactPersonAddress;
                    newContactPerson.ProjectContactPersonEmail = Dto.ProjectContactPersonEmail;
                    newContactPerson.ProjectContactPersonHomeNum = Dto.ProjectContactPersonHome;
                    newContactPerson.ProjectContactPersonMobile = Dto.ProjectContactPersonMob;
                    newContactPerson.ProjectContactPersonName = Dto.ProjectContactPersonName;
                    newContactPerson.Active = true;
                    newContactPerson.CreatedBy = Creator;
                    newContactPerson.CreationDate = DateTime.Now;

                    var ContactPerson = _unitOfWork.ProjectContactPersons.Add(newContactPerson);
                    _unitOfWork.Complete();

                }

                //----------------------------------Add Data to ProjectAssignUser (add Creator as Project Admin)-------
                var NewProjectAssignUser = new ProjectAssignUser();
                NewProjectAssignUser.ProjectId = newProject.Id;
                NewProjectAssignUser.UserId = Creator;
                NewProjectAssignUser.RoleId = 145;
                NewProjectAssignUser.RoleName = "Task Project Admin";
                NewProjectAssignUser.Active = true;
                NewProjectAssignUser.CreationBy = Creator;
                NewProjectAssignUser.CreationDate = DateTime.Now;
                NewProjectAssignUser.ModifiedBy = Creator;
                NewProjectAssignUser.ModificationDate = DateTime.Now;
                _unitOfWork.ProjectAssignUsers.Add(NewProjectAssignUser);
                _unitOfWork.Complete();

                response.ID = newProject.Id;
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

        public BaseResponseWithData<GetTaskMangerProjectDto> GetTaskMangerProject(long TaskMangerProjectID)
        {
            var response = new BaseResponseWithData<GetTaskMangerProjectDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            //#region user Auth
            //HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            //response.Errors = validation.errors;
            //response.Result = validation.result;
            //#endregion


            try
            {
                if (response.Result)
                {
                    var TaskMangerProject = _unitOfWork.Projects.FindAll((a => a.Id == TaskMangerProjectID), new[] { "CostType", "Branch", "Priorty", "Tasks", "Tasks.TaskExpensis", "Tasks.TaskDetails", "WorkingHourseTrackings", "ProjectAssignUsers", "CreatedByNavigation", "SalesOffer.Client.SalesPerson" }).FirstOrDefault();
                    if (TaskMangerProject == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "This Task Manger Project Id is not found ";
                        response.Errors.Add(err);
                        return response;
                    }

                    var SalesOffer = _unitOfWork.SalesOffers.FindAll((a => a.Id == TaskMangerProject.SalesOfferId)).FirstOrDefault();
                    if (SalesOffer == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "This SalesOffer Id is not found ";
                        response.Errors.Add(err);
                        return response;
                    }

                    var ProjectData = _mapper.Map<GetTaskMangerProjectDto>(TaskMangerProject);
                    ProjectData.ClientSalesPersonID = TaskMangerProject.SalesOffer?.Client?.SalesPersonId;
                    ProjectData.ClientSalesPersonName = TaskMangerProject.SalesOffer?.Client?.SalesPerson?.FirstName + TaskMangerProject.SalesOffer?.Client?.SalesPerson?.MiddleName + TaskMangerProject.SalesOffer?.Client?.SalesPerson?.LastName;
                    ProjectData.ProjectCreatorID = TaskMangerProject.CreatedBy;
                    ProjectData.ProjectCreatorName = TaskMangerProject.CreatedByNavigation.FirstName + " " + TaskMangerProject.CreatedByNavigation.MiddleName ?? "" + " " + TaskMangerProject.CreatedByNavigation.LastName;
                    ProjectData.CostTypeName = TaskMangerProject.CostType?.CostingTypeName;
                    ProjectData.BranchName = TaskMangerProject.Branch.Name;
                    ProjectData.BranchID = TaskMangerProject.Branch.Id;
                    ProjectData.PriortyName = TaskMangerProject.Priorty?.Name;
                    ProjectData.NumOfOpenTask = TaskMangerProject.Tasks.SelectMany(x => x.TaskDetails).Where(wh => wh.Status == "Open").Count();
                    ProjectData.NumOfClosedTask = TaskMangerProject.Tasks.SelectMany(x => x.TaskDetails).Where(wh => wh.Status == "Closed").Count();
                    //-------------------------calculate total Expensis---------------------
                    var Expensis = TaskMangerProject.Tasks.SelectMany(y => y.TaskExpensis).Where(y => y.Approved == true).Sum(s => s.Amount);
                    var Costs = GetCostsForAllTask(TaskMangerProjectID);

                    ProjectData.Expenses = Expensis + Costs.Data;
                    //---------------------End calculate total Expensis----------------------


                    //-------------calculate total progress-------------
                    var workingHourseTracking = TaskMangerProject.WorkingHourseTrackings.ToList();
                    var totalNumberOfTasksForProject = workingHourseTracking.Where(a => a.ProjectId == TaskMangerProjectID && a.ApprovedBy != null).Select(a => a.TaskId).Distinct().Count();

                    var totalProgressNumber = workingHourseTracking.Where(a => a.ProjectId == TaskMangerProjectID && a.ProgressRate > 0 && a.ApprovedBy != null)
                                        .GroupBy(a => a.TaskId).Select(a => a.OrderByDescending(a => a.Id).FirstOrDefault()).Sum(x => x.ProgressRate);

                    var totalNumberOfTasksHaveProgress = workingHourseTracking.Where(a => a.ProjectId == TaskMangerProjectID && a.ProgressRate > 0 && a.ApprovedBy != null).Select(a => a.TaskId).Distinct().Count();

                    if (totalNumberOfTasksHaveProgress > 0)
                    {
                        ProjectData.ProgressPercentage = totalProgressNumber / totalNumberOfTasksHaveProgress;
                        ProjectData.ProgressComment = $"{totalNumberOfTasksHaveProgress} from {totalNumberOfTasksForProject}";
                    }
                    //-------------------------------------------------

                    ProjectData.NumberOfUsers = TaskMangerProject.ProjectAssignUsers.Where(a => a.RoleId == null).Count();
                    ProjectData.workingHours = TaskMangerProject.WorkingHourseTrackings.Select(a => a.TotalHours).Sum();

                    // Total Budget from Project Tasks List 

                    var ProjectTasksList = _unitOfWork.Tasks.FindAll((a => a.ProjectId == TaskMangerProjectID), new[] { "TaskDetails" });
                    if (ProjectTasksList.Count() > 0)
                    {
                        ProjectData.TotalSumTaskBudget = ProjectTasksList.Select(x => x.TaskDetails.Sum(a => a.ProjectBudget)).Sum();
                    }

                    var contactPerson = _unitOfWork.ProjectContactPersons.FindAll((a => a.ProjectId == TaskMangerProjectID), new[] { "Governorate", "Area", "Country" }).FirstOrDefault();
                    if (contactPerson != null)
                    {
                        ProjectData.ContactPersonID = contactPerson.Id;
                        ProjectData.ContactPersonEmail = contactPerson.ProjectContactPersonEmail;
                        ProjectData.ContactpersonMobile = contactPerson.ProjectContactPersonMobile;
                        ProjectData.ContcatPersonHome = contactPerson.ProjectContactPersonHomeNum;
                        ProjectData.ContcatPersonName = contactPerson.ProjectContactPersonName;
                        ProjectData.ContcatPersonAddress = contactPerson.Address;
                        ProjectData.CountryID = contactPerson.CountryId;
                        if (contactPerson.CountryId > 0) ProjectData.CountryName = contactPerson.Country.Name;

                        ProjectData.GovernorateID = contactPerson.GovernorateId;
                        if (contactPerson.GovernorateId > 0) ProjectData.GovernorateName = contactPerson.Governorate.Name;

                        ProjectData.AreaID = contactPerson.AreaId;
                        if (contactPerson.AreaId > 0) ProjectData.AreaName = contactPerson.Area.Name;
                    }

                    var projectAttachments = _unitOfWork.ProjectAttachments.FindAll((a => a.ProjectId == TaskMangerProjectID)).ToList();
                    //if(projectAttachments != null)
                    //{
                    //    projectAttachments.ForEach(a => a.AttachmentPath = Globals.baseURL + a.AttachmentPath);
                    //    ProjectData.AttachmentList = projectAttachments.Select(a =>a.AttachmentPath).ToList();

                    //}

                    //-------------------------------------------------------
                    if (projectAttachments != null)
                    {
                        projectAttachments.ForEach(a => a.AttachmentPath = Globals.baseURL + a.AttachmentPath);
                        var attachListWithIDs = new List<AttachmentList>();
                        foreach (var attach in projectAttachments)
                        {
                            var attachWithID = new AttachmentList();
                            attachWithID.Id = attach.Id;
                            attachWithID.AttachmentPath = attach.AttachmentPath;
                            attachWithID.AttachmentName = attach.FileName + "." + attach.FileExtenssion.Trim();
                            attachListWithIDs.Add(attachWithID);
                        }
                        ProjectData.AttachmentList = attachListWithIDs;
                    }
                    //-------------------------------------------------------
                    ProjectData.ProjectName = SalesOffer.ProjectName;
                    ProjectData.ProjectLocation = SalesOffer.ProjectLocation;

                    var Currency = _unitOfWork.Currencies.FindAll((a => a.Id == ProjectData.CurrencyID)).FirstOrDefault();
                    if (Currency != null)
                    {
                        ProjectData.CurrencyName = Currency.Name;
                    }

                    ProjectData.ClientID = SalesOffer.ClientId;
                    var client = _unitOfWork.Clients.FindAll((a => a.Id == SalesOffer.ClientId)).FirstOrDefault();
                    if (client != null)
                    {
                        ProjectData.ClientName = client.Name;
                    }



                    //--------------------------------Project Sprints------------------------------------------------------
                    var projectSprintsList = _unitOfWork.ProjectSprints.FindAll((a => a.ProjectId == TaskMangerProjectID)).ToList();

                    var projectSprintsListDto = _mapper.Map<List<GetProjectsprintDto>>(projectSprintsList);
                    ProjectData.ProjectSprints = projectSprintsListDto;

                    //--------------------------------project work Flow------------------------------------------------------
                    var projectWorkFlowList = _unitOfWork.Workflows.FindAll((a => a.ProjectId == TaskMangerProjectID)).ToList();

                    var projectWorkflowListDto = _mapper.Map<List<GetWorkFlowDto>>(projectWorkFlowList);
                    ProjectData.ProjectWorkFlows = projectWorkflowListDto;

                    response.Data = ProjectData;
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

        public BaseResponseWithId<long> EditTaskMangerProject(EditTaskMangerProjectDto Dto, long Creator, string CompName)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };



            #region Check availablty in DB
            var ProjectTaskManger = _unitOfWork.Projects.FindAll((a => a.Id == Dto.Id), new[] { "SalesOffer" }).FirstOrDefault();
            if (ProjectTaskManger == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Task Manger Project with this Id";
                response.Errors.Add(error);
                return response;
            }
            var client = _unitOfWork.Clients.GetById(Dto.ClientID);
            if (client == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Client with this Id";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.CurrencyID != null)
            {
                var currency = _unitOfWork.Currencies.GetById(Dto.CurrencyID ?? 0);
                if (currency == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No currency with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }

            //var contactPerson = _unitOfWork.ProjectContactPersons.GetById(Dto.ContactPersonID);
            //if (contactPerson == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Project Contact Persons with this Id";
            //    response.Errors.Add(error);
            //    return response;
            //}

            if (Dto.PriorityID != null)
            {
                var priorty = _unitOfWork.Prioritys.GetById((int)Dto.PriorityID);
                if (priorty == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No priorty with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }
            if (Dto.Budget != null)
            {
                if (Dto.CurrencyID == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "There Is Budget added You must Enter Currency ID";
                    response.Errors.Add(error);
                    return response;
                }
                var currency = _unitOfWork.Currencies.GetById((int)Dto.CurrencyID);
                if (currency == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Currency with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }
            //var country = _unitOfWork.Country.GetById(Dto.ProjectContactPersonCountryId);
            //if (country == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Country with this Id";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //var Governorate = _unitOfWork.Governorates.GetById(Dto.ProjectContactPersonGovernorateID);
            //if (Governorate == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Government with this Id";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //if (Dto.ProjectContactPersonAreaId != null)
            //{
            //    var Area = _unitOfWork.Areas.GetById(Dto.ProjectContactPersonAreaId ?? 0);
            //    if (Area == null)
            //    {
            //        response.Result = false;
            //        Error error = new Error();
            //        error.ErrorCode = "Err101";
            //        error.ErrorMSG = "No Area with this Id";
            //        response.Errors.Add(error);
            //        return response;
            //    }
            //}
            if (Dto.CostTypeID != null)
            {
                var costType = _unitOfWork.ProjectCostTypes.GetById(Dto.CostTypeID ?? 0);
                if (costType == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No costType with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }
            #endregion

            #region Date Validation
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
            if (!DateTime.TryParse(Dto.EndDate, out endDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid End Date:";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            #region role validation
            var userIsAdminOrManger = _unitOfWork.ProjectAssignUsers.FindAll(a => a.ProjectId == Dto.Id && (a.RoleId == 145 || a.RoleId == 1)).FirstOrDefault();
            var superAdmin = _unitOfWork.UserRoles.FindAll(a => a.UserId == Creator && a.RoleId == 1).FirstOrDefault();
            if (userIsAdminOrManger == null && superAdmin == null)                                   //Admin of project and super Admin only can Edit
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "You are not authorized to edit this project";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {

                if (response.Result)
                {
                    //-----------------------------Add Data To Salesoffer table----------------------------------
                    int branchIdToBeAdded = 0;
                    var BranchFromDatabase = _unitOfWork.Users.GetById(Creator).BranchId;
                    if (BranchFromDatabase == null)
                    {
                        var mainbranch = _unitOfWork.Branches.Find(a => a.IsMain == true);
                        branchIdToBeAdded = mainbranch.Id;
                    }

                    ProjectTaskManger.SalesOffer.BranchId = BranchFromDatabase != null ? BranchFromDatabase ?? 0 : branchIdToBeAdded;
                    ProjectTaskManger.SalesOffer.ProjectLocation = Dto.ProjectLocation;
                    ProjectTaskManger.SalesOffer.ProjectName = Dto.ProjectName;
                    ProjectTaskManger.SalesOffer.ClientId = Dto.ClientID;

                    //var salesOfferSaved = _unitOfWork.SalesOffers.Add(newSalesoffer);
                    _unitOfWork.Complete();


                    //-----------------------------Add Data To Project table----------------------------------
                    //var newProject = new Project();

                    ProjectTaskManger.Closed = Dto.Closed;
                    ProjectTaskManger.Revision = Dto.Revision;
                    if (!string.IsNullOrWhiteSpace(Dto.StartDate))
                    {
                        ProjectTaskManger.StartDate = startDate;
                    }
                    if (!string.IsNullOrWhiteSpace(Dto.EndDate))
                    {
                        ProjectTaskManger.EndDate = endDate;
                    }
                    ProjectTaskManger.BranchId = BranchFromDatabase != null ? BranchFromDatabase ?? 0 : branchIdToBeAdded;
                    ProjectTaskManger.ProjectDescription = Dto.ProjectDescription;
                    ProjectTaskManger.Billable = Dto.Billable;
                    ProjectTaskManger.TimeTracking = Dto.TimeTracking;
                    if (Dto.PriorityID != null) ProjectTaskManger.PriortyId = Dto.PriorityID;
                    ProjectTaskManger.Active = Dto.Active;
                    if (Dto.Budget != null)
                    {
                        ProjectTaskManger.Budget = Dto.Budget;
                        ProjectTaskManger.CurrencyId = Dto.CurrencyID;
                    }
                    if (Dto.CostTypeID != null && Dto.CostTypeID != 0)
                    {
                        ProjectTaskManger.CostTypeId = Dto.CostTypeID;
                    }
                    ProjectTaskManger.ModifiedBy = Creator;
                    ProjectTaskManger.ModifiedDate = DateTime.Now;

                    //var projectSaved = _unitOfWork.Projects.Add(newProject);
                    _unitOfWork.Complete();

                    //-----------------------------Add Data To Project Attachment table----------------------------------
                    var newProjectAttachmentList = new List<ProjectAttachment>();

                    if (Dto.AttachmentList != null)
                    {
                        foreach (var attachment in Dto.AttachmentList)
                        {
                            if (attachment.ID != null)
                            {
                                if (attachment.ID != null && attachment.Active == false)
                                {
                                    // Delete
                                    var AttachmentDb = _unitOfWork.ProjectAttachments.FindAll((a => a.Id == attachment.ID)).FirstOrDefault();
                                    if (AttachmentDb != null)
                                    {
                                        string path = Path.Combine(_host.WebRootPath, AttachmentDb.AttachmentPath);
                                        if (System.IO.File.Exists(path))
                                        {
                                            System.IO.File.Delete(path);
                                        }
                                        _unitOfWork.ProjectAttachments.Delete(AttachmentDb);
                                        _unitOfWork.Complete();
                                    }

                                }
                                if (attachment.ID != null && attachment.Active == true)//id : 5 , Active : true
                                {
                                    var AttachmentDb = _unitOfWork.ProjectAttachments.FindAll((a => a.Id == attachment.ID)).FirstOrDefault();
                                    if (AttachmentDb != null)
                                    {
                                        string path = Path.Combine(_host.WebRootPath, AttachmentDb.AttachmentPath);
                                        if (System.IO.File.Exists(path))
                                        {
                                            System.IO.File.Delete(path);
                                        }
                                        var fileExtension = attachment.FileContent.FileName.Split('.').Last();
                                        var virtualPath = $"Attachments\\{CompName}\\TaskManger\\{Dto.Id}\\";
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(attachment.FileContent.FileName.Trim().Replace(" ", ""));
                                        var AttachPath = Common.SaveFileIFF(virtualPath, attachment.FileContent, FileName, fileExtension, _host);

                                        AttachmentDb.AttachmentPath = AttachPath;
                                        AttachmentDb.FileName = FileName;
                                        AttachmentDb.FileExtenssion = fileExtension;
                                        //_unitOfWork.ProjectAttachments.Delete(AttachmentDb);
                                        _unitOfWork.Complete();
                                    }
                                }
                            }
                            else
                            {
                                var projectAttachment = new ProjectAttachment();

                                var fileExtension = attachment.FileContent.FileName.Split('.').Last();
                                var virtualPath = $"Attachments\\{CompName}\\TaskManger\\{Dto.Id}\\";
                                var FileName = System.IO.Path.GetFileNameWithoutExtension(attachment.FileContent.FileName.Trim().Replace(" ", ""));
                                var AttachPath = Common.SaveFileIFF(virtualPath, attachment.FileContent, FileName, fileExtension, _host);

                                projectAttachment.ProjectId = Dto.Id;
                                projectAttachment.AttachmentPath = AttachPath;
                                projectAttachment.CreatedBy = Creator;
                                projectAttachment.CreationDate = DateTime.Now;
                                projectAttachment.FileName = FileName;
                                projectAttachment.FileExtenssion = fileExtension;

                                newProjectAttachmentList.Add(projectAttachment);
                            }

                        }

                        var ListOfAtachmentAdded = _unitOfWork.ProjectAttachments.AddRange(newProjectAttachmentList);
                        _unitOfWork.Complete();

                    }
                    //------------------------------Add Data for project Contact table-----------------------------

                    if (
                         Dto.ProjectContactPersonCountryId != 0 &&
                         Dto.ProjectContactPersonGovernorateID != 0 &&
                        !string.IsNullOrWhiteSpace(Dto.ProjectContactPersonName) &&
                        !string.IsNullOrWhiteSpace(Dto.ProjectContactPersonMob)
                        )
                    {
                        Dto.ProjectContactPersonAddress = string.IsNullOrWhiteSpace(Dto.ProjectContactPersonAddress) ? " " : Dto.ProjectContactPersonAddress;

                        var ContactPerson = _unitOfWork.ProjectContactPersons.FindAll((a => a.ProjectId == Dto.Id)).FirstOrDefault();
                        if (ContactPerson != null)
                        {
                            ContactPerson.CountryId = Dto.ProjectContactPersonCountryId;
                            ContactPerson.GovernorateId = Dto.ProjectContactPersonGovernorateID;
                            ContactPerson.AreaId = Dto.ProjectContactPersonAreaId;
                            ContactPerson.Address = Dto.ProjectContactPersonAddress;
                            ContactPerson.ProjectContactPersonEmail = Dto.ProjectContactPersonEmail;
                            ContactPerson.ProjectContactPersonHomeNum = Dto.ProjectContactPersonHome;
                            ContactPerson.ProjectContactPersonMobile = Dto.ProjectContactPersonMob;
                            ContactPerson.ProjectContactPersonName = Dto.ProjectContactPersonName;
                            ContactPerson.Modified = DateTime.Now;
                            ContactPerson.ModifiedBy = Creator;
                        }
                        else
                        {
                            Dto.ProjectContactPersonAddress = string.IsNullOrWhiteSpace(Dto.ProjectContactPersonAddress) ? " " : Dto.ProjectContactPersonAddress;

                            var newContactPerson = new ProjectContactPerson();
                            newContactPerson.ProjectId = Dto.Id;
                            newContactPerson.CountryId = (int)Dto.ProjectContactPersonCountryId;
                            newContactPerson.GovernorateId = (int)Dto.ProjectContactPersonGovernorateID;
                            newContactPerson.AreaId = Dto.ProjectContactPersonAreaId;
                            newContactPerson.Address = Dto.ProjectContactPersonAddress;
                            newContactPerson.ProjectContactPersonEmail = Dto.ProjectContactPersonEmail;
                            newContactPerson.ProjectContactPersonHomeNum = Dto.ProjectContactPersonHome;
                            newContactPerson.ProjectContactPersonMobile = Dto.ProjectContactPersonMob;
                            newContactPerson.ProjectContactPersonName = Dto.ProjectContactPersonName;
                            newContactPerson.Active = true;
                            newContactPerson.CreatedBy = Creator;
                            newContactPerson.CreationDate = DateTime.Now;

                            var newProjectContactPerson = _unitOfWork.ProjectContactPersons.Add(newContactPerson);
                            _unitOfWork.Complete();
                        }


                        //var ContactPerson = _unitOfWork.ProjectContactPersons.Add(newContactPerson);
                        _unitOfWork.Complete();
                    }

                    response.ID = ProjectTaskManger.Id;
                }

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

        public async Task<BaseResponseWithDataAndHeader<List<GetTaskMangerProjectCardsDto>>> GetTaskMangerProjectCards(TaskMangerProjectsFilters filters, long UserID)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetTaskMangerProjectCardsDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            var projectAdminOrMangers = _unitOfWork.ProjectAssignUsers.FindAll(a => a.UserId == UserID && (a.RoleId == 145 || a.RoleId == 146 || a.RoleId == 1));
            var projectsIds = projectAdminOrMangers.Select(a => a.ProjectId).ToList();
            //var projectCreatosIds = _unitOfWork.Projects.FindAll(a => a.CreatedBy == UserID);
            var adminsOrMangersForAllprojects = _unitOfWork.UserRoles.FindAll(a => a.RoleId == 145 || a.RoleId == 1);
            var moreIds = adminsOrMangersForAllprojects.Where(a => a.UserId != null).Select(a => a.UserId);
            //projectsIds = projectsIds.AddRange(moreIds);

            //foreach (var userrole in adminsOrMangersForAllprojects)
            //{
            //    if(userrole.UserId != null)
            //    {
            //        //projectsIds.Add(userrole.UserId ?? 0);
            //    }
            //}

            bool userIsProjectAdmin = false;
            if (moreIds.Contains(UserID)) userIsProjectAdmin = true;

            Expression<Func<Project, bool>> criteria = (a => true);
            criteria = a =>
            (
            (!string.IsNullOrEmpty(filters.ProjectName) ? (a.SalesOffer.ProjectName.Contains(filters.ProjectName)) : true) &&
            (filters.CreatorID != null ? a.CreatedBy == filters.CreatorID : true) &&
            (filters.closed != null ? a.Closed == filters.closed : true) &&
            (userIsProjectAdmin == false ? ((a.CreatedBy == UserID) || (projectsIds.Contains(a.Id))) : true) &&
            (filters.NotActive == true ? a.Active == false : a.Active == true) &&
            (filters.IsArchived == true ? a.IsArchived == true : (a.IsArchived == false || a.IsArchived == null))
            );

            try
            {
                if (response.Result)
                {
                    var CurrencyList = _unitOfWork.Currencies.GetAll();
                    //var data = await _unitOfWork.HrUsers.FindAllAsync((a => a.Active == true), new[] { "JobTitle" });
                    var data = _unitOfWork.Projects.FindAllPaging(criteria, filters.CurrentPage, filters.numberOfItemsPerPage, new[] { "SalesOffer", "Tasks", "Tasks.TaskExpensis", "Tasks.TaskDetails", "Priorty" });
                    var projectsIDs = data.Select(x => x.Id).ToList();
                    response.Data = data.Select(x => new GetTaskMangerProjectCardsDto
                    {
                        ID = x.Id,
                        ProjectName = x.SalesOffer.ProjectName,
                        EndDate = x.EndDate.ToShortDateString(),
                        RemainDays = x.EndDate.Subtract(DateTime.Now).Days > 0 ? x.EndDate.Subtract(DateTime.Now).Days : 0,
                        TotalCost = data.Where(proID => proID.Id == x.Id).SelectMany(x => x.Tasks.SelectMany(y => y.TaskExpensis)).Sum(s => s.Amount),
                        NumOfOpenTasks = data.Where(w => w.Id == x.Id).SelectMany(x => x.Tasks.SelectMany(y => y.TaskDetails)).Where(wh => wh.Status == "Open").Count(),
                        NumOfClosedTasks = data.Where(project => project.Id == x.Id).SelectMany(x => x.Tasks.SelectMany(y => y.TaskDetails)).Where(taskDetails => taskDetails.Status == "Closed").Count(),
                        PriortyID = x.PriortyId,
                        PriortyName = x.Priorty != null ? x.Priorty.Name : null,
                        Currency = CurrencyList.Where(a => a.Id == x.CurrencyId).Select(a => a.Name).FirstOrDefault(),
                        Description = x.ProjectDescription,
                        UsersList = GetAllUsersOfProjects(x.Id).Data
                    }).ToList();
                    PaginationHeader paginationHeader = new PaginationHeader();
                    paginationHeader.CurrentPage = filters.CurrentPage;
                    paginationHeader.ItemsPerPage = filters.numberOfItemsPerPage;
                    paginationHeader.TotalPages = data.TotalPages;
                    paginationHeader.TotalItems = data.TotalCount;

                    response.PaginationHeader = paginationHeader;

                    //var tasksOfProjects = data.Where(w => w.Id == x.Id ).SelectMany(x => x.Tasks.SelectMany(y => y.TaskDetails)).Count();

                }
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

        public BaseResponseWithId<long> AddProjectSettings(AddTaskMangerProjectSettingsDto Dto,long UserID)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var project = _unitOfWork.Projects.FindAll((a => a.Id == Dto.ProjectID)).FirstOrDefault();
                if (project == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Project with this Id";
                    response.Errors.Add(error);
                    return response;
                }
                if (Dto.AllowProjectChatting != null) project.AllowProjectChatting = Dto.AllowProjectChatting;
                if (Dto.AllowTaskScreenMonitoring != null) project.AllowTaskScreenMonitoring = Dto.AllowTaskScreenMonitoring;
                if (Dto.TimeTracking != null) project.TimeTracking = Dto.TimeTracking;
                if (Dto.Billable != null) project.Billable = Dto.Billable;
                if (Dto.MoveBySequenceTask != null)
                {
                    project.MoveBySequenceTask = Dto.MoveBySequenceTask;
                    project.MoveBySequenceType = Dto.MoveBySequenceType;
                }
                if (Dto.UnitRateService != null) project.UnitRateService = Dto.UnitRateService;
                if (Dto.Billable == true)
                {
                    project.BillingFactor = Dto.BillingFactor;
                    var billable = _unitOfWork.Billingtypes.FindAll((a => a.Id == Dto.BillingTypeID)).FirstOrDefault();
                    if (billable == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "No Billing Type with this Id";
                        response.Errors.Add(error);
                        return response;
                    }
                    project.BillingTypeId = Dto.BillingTypeID;

                }
                project.ModifiedBy = UserID;
                project.ModifiedDate = DateTime.Now;
                _unitOfWork.Projects.Update(project);
                _unitOfWork.Complete();
                response.ID = project.Id;
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
        public BaseResponseWithId<long> DeleteProjectSettings(long id)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check in DB
            var project = _unitOfWork.Projects.GetById(id);
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Task Manger Project with this Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                project.AllowProjectChatting = null;
                project.AllowTaskScreenMonitoring = null;
                project.Billable = null;
                project.BillingFactor = null;
                project.BillingTypeId = null;
                project.TimeTracking = null;
                project.MoveBySequenceTask = null;
                project.MoveBySequenceType = null;
                project.UnitRateService = null;

                _unitOfWork.Complete();
                response.ID = id;
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

        public BaseResponseWithData<GetTaskMangerProjectSettingsDto> GetProjectsSettings(long ProjectID)
        {
            var response = new BaseResponseWithData<GetTaskMangerProjectSettingsDto>();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {
                if (response.Result)
                {
                    var project = _unitOfWork.Projects.FindAll((a => a.Id == ProjectID), new[] { "BillingType" }).FirstOrDefault();
                    if (project == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "No Project with this Id";
                        response.Errors.Add(error);
                        return response;
                    }
                    var projectSettings = new GetTaskMangerProjectSettingsDto();
                    projectSettings.ProjectID = ProjectID;
                    projectSettings.Billable = project.Billable;
                    projectSettings.BillingFactor = project.BillingFactor;
                    projectSettings.BillingTypeID = project.BillingTypeId;
                    projectSettings.BillingTypeName = project.BillingType != null ? project.BillingType.BillingTypeName : null;
                    projectSettings.AllowProjectChatting = project.AllowProjectChatting;
                    projectSettings.AllowTaskScreenMonitoring = project.AllowTaskScreenMonitoring;
                    projectSettings.MoveBySequenceTask = project.MoveBySequenceTask;
                    projectSettings.TimeTracking = project.TimeTracking;
                    projectSettings.MoveBySequenceType = project.MoveBySequenceType;
                    projectSettings.UnitRateService = project.UnitRateService;

                    response.Data = projectSettings;
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

        public BaseResponseWithData<List<CostTypeDDL>> ProjectCostTypes()
        {
            var response = new BaseResponseWithData<List<CostTypeDDL>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var CostTypeList = _unitOfWork.ProjectCostTypes.GetAll();

                response.Data = CostTypeList.Select(a => new CostTypeDDL
                {
                    ID = a.Id,
                    Name = a.CostingTypeName,
                }).ToList();
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

        public BaseResponseWithData<List<BillingTypeDDL>> BillingTypes()
        {
            var response = new BaseResponseWithData<List<BillingTypeDDL>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var CostTypeList = _unitOfWork.Billingtypes.GetAll();

                response.Data = CostTypeList.Select(a => new BillingTypeDDL
                {
                    ID = a.Id,
                    Name = a.BillingTypeName,
                }).ToList();
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

        //---------------------------------------Projects Users-----------------------------------------------------

        public BaseResponseWithId<long> AddUsersToProject(AddUsersToProjectDto Dto, long Creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region DB check
            var project = _unitOfWork.Projects.FindAll(a => a.Id == Dto.projectID).FirstOrDefault();
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project with This ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion


            #region Who can assign user to project
            var userCanAdd = _unitOfWork.UserRoles.FindAll(a => a.UserId == Creator && (a.RoleId == 145 || a.RoleId == 1)).FirstOrDefault();
            var superAdmin = _unitOfWork.UserRoles.FindAll(a => a.UserId == Creator && a.RoleId == 1).FirstOrDefault();
            if (userCanAdd == null && superAdmin == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "You are not authorized to assign users to This project";
                response.Errors.Add(err);
                return response;
            }
            #endregion


            try
            {

                var AllUserList = new List<long>();
                foreach (var item in Dto.AdminUsersList)
                {
                    AllUserList.Add(item);
                }
                foreach (var item in Dto.ManagerUsersList)
                {
                    AllUserList.Add(item);
                }
                var usersRolesList = _unitOfWork.UserRoles.FindAll((a => AllUserList.Contains(a.UserId ?? 0)));

                if (Dto.AdminUsersList.Count() > 0 && Dto.AdminUsersList != null)
                {

                    foreach (var user in Dto.AdminUsersList)
                    {

                        //var userRole = usersRolesList.Where(a => a.RoleId == 145 && a.UserId == user).FirstOrDefault();
                        //if(userRole == null) 
                        //{
                        //    var NewUserRole = new UserRole();
                        //    NewUserRole.RoleId = 145;           //will be changed to projectAdmin role when add to DB
                        //    NewUserRole.UserId = user;
                        //    NewUserRole.CreatedBy = Creator;
                        //    NewUserRole.CreationDate = DateTime.Now;

                        //    _unitOfWork.UserRoles.Add(NewUserRole);

                        //}
                        var NewProjectAssignUser = new ProjectAssignUser();
                        NewProjectAssignUser.ProjectId = Dto.projectID;
                        NewProjectAssignUser.UserId = user;
                        NewProjectAssignUser.RoleId = 145;
                        NewProjectAssignUser.RoleName = "Task Project Admin";
                        NewProjectAssignUser.Active = true;
                        NewProjectAssignUser.CreationBy = Creator;
                        NewProjectAssignUser.CreationDate = DateTime.Now;
                        NewProjectAssignUser.ModifiedBy = Creator;
                        NewProjectAssignUser.ModificationDate = DateTime.Now;
                        _unitOfWork.ProjectAssignUsers.Add(NewProjectAssignUser);

                    }
                }

                //--------------------------for Project Manger---------------------------
                if (Dto.ManagerUsersList.Count() > 0 && Dto.ManagerUsersList != null)
                {

                    foreach (var user in Dto.ManagerUsersList)
                    {

                        //var userRole = usersRolesList.Where(a => a.RoleId == 146 && a.UserId == user).FirstOrDefault();
                        //if (userRole == null)
                        //{
                        //    var NewUserRole = new UserRole();
                        //    NewUserRole.RoleId = 146;           //will be changed to project Manger role when add to DB
                        //    NewUserRole.UserId = user;
                        //    NewUserRole.CreatedBy = Creator;
                        //    NewUserRole.CreationDate = DateTime.Now;

                        //    _unitOfWork.UserRoles.Add(NewUserRole);

                        //}
                        var NewProjectAssignUser = new ProjectAssignUser();
                        NewProjectAssignUser.ProjectId = Dto.projectID;
                        NewProjectAssignUser.UserId = user;
                        NewProjectAssignUser.RoleId = 146;
                        NewProjectAssignUser.RoleName = "Task Project Manger";
                        NewProjectAssignUser.Active = true;
                        NewProjectAssignUser.CreationBy = Creator;
                        NewProjectAssignUser.CreationDate = DateTime.Now;
                        NewProjectAssignUser.ModifiedBy = Creator;
                        NewProjectAssignUser.ModificationDate = DateTime.Now;

                        _unitOfWork.ProjectAssignUsers.Add(NewProjectAssignUser);
                    }
                }


                //------------------------------------for Normal Users------------------------------------------
                if (Dto.NormalUsersList.Count() > 0 && Dto.NormalUsersList != null)
                {

                    foreach (var user in Dto.NormalUsersList)
                    {

                        var NewProjectAssignUser = new ProjectAssignUser();
                        NewProjectAssignUser.ProjectId = Dto.projectID;
                        NewProjectAssignUser.UserId = user;
                        NewProjectAssignUser.Active = true;
                        NewProjectAssignUser.CreationBy = Creator;
                        NewProjectAssignUser.CreationDate = DateTime.Now;
                        NewProjectAssignUser.ModifiedBy = Creator;
                        NewProjectAssignUser.ModificationDate = DateTime.Now;

                        _unitOfWork.ProjectAssignUsers.Add(NewProjectAssignUser);
                    }
                }

                _unitOfWork.Complete();
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

        public BaseResponseWithData<GetUsersToProjectDto> GetUsersOfProjects(long projectId)
        {
            BaseResponseWithData<GetUsersToProjectDto> response = new BaseResponseWithData<GetUsersToProjectDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check IN DB
            var project = _unitOfWork.Projects.FindAll((a => a.Id == projectId)).FirstOrDefault();
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project with This ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                var projectAssignUserList = _unitOfWork.ProjectAssignUsers.FindAll((a => a.ProjectId == projectId));
                var userIDs = projectAssignUserList.Select(a => a.UserId).ToList();
                var userOfproject = _unitOfWork.Users.FindAll(a => userIDs.Contains(a.Id), new[] { "JobTitle" });

                var adminList = new List<UserWithJobTitleDDL>();
                var mangerList = new List<UserWithJobTitleDDL>();
                var normalList = new List<UserWithJobTitleDDL>();

                var UsersOfProjects = new GetUsersToProjectDto();
                foreach (var projectAssignUser in projectAssignUserList)
                {
                    if (projectAssignUser.RoleId == 145)
                    {
                        var user = userOfproject.Where(a => a.Id == projectAssignUser.UserId).FirstOrDefault();
                        var adminUser = new UserWithJobTitleDDL()
                        {
                            ID = projectAssignUser.Id,
                            UserId = user.Id,
                            FullName = user.FirstName + " " + (user.MiddleName ?? "") + " " + user.LastName,
                            ImgPath = user.PhotoUrl != null ? Globals.baseURL + user.PhotoUrl : null,
                            JobTitleName = user.JobTitle != null ? user.JobTitle.Name : null,
                        };
                        adminList.Add(adminUser);
                    }

                    if (projectAssignUser.RoleId == 146)
                    {
                        var user = userOfproject.Where(a => a.Id == projectAssignUser.UserId).FirstOrDefault();
                        var mangerUser = new UserWithJobTitleDDL()
                        {
                            ID = projectAssignUser.Id,
                            UserId = user.Id,
                            FullName = user.FirstName + " " + (user.MiddleName ?? "") + " " + user.LastName,
                            ImgPath = user.PhotoUrl != null ? Globals.baseURL + user.PhotoUrl : null,
                            JobTitleName = user.JobTitle != null ? user.JobTitle.Name : null,
                        };
                        mangerList.Add(mangerUser);
                    }

                    if (projectAssignUser.RoleId == null)
                    {
                        var user = userOfproject.Where(a => a.Id == projectAssignUser.UserId).FirstOrDefault();
                        var normalUser = new UserWithJobTitleDDL()
                        {
                            ID = projectAssignUser.Id,
                            UserId = user.Id,
                            FullName = user.FirstName + " " + (user.MiddleName ?? "") + " " + user.LastName,
                            ImgPath = user.PhotoUrl != null ? Globals.baseURL + user.PhotoUrl : null,
                            JobTitleName = user.JobTitle != null ? user.JobTitle.Name : null,
                        };
                        normalList.Add(normalUser);
                    }
                }

                UsersOfProjects.AdminUsersList = adminList;
                UsersOfProjects.ManagerUsersList = mangerList;
                UsersOfProjects.NormalUsersList = normalList;

                response.Data = UsersOfProjects;
                response.Data.projectID = project.Id;
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

        public BaseResponseWithId<long> EditUsersOfProject(EditUsersAssignToProjectDto Dto, long Edior)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region Who can assign user to project
            var userCanAdd = _unitOfWork.ProjectAssignUsers.FindAll(a => a.ProjectId == Dto.ProjectID && a.UserId == Edior && (a.RoleId == 145 || a.RoleId == 1)).FirstOrDefault();
            var superAdmin = _unitOfWork.UserRoles.FindAll(a => a.UserId == Edior && a.RoleId == 1).FirstOrDefault();
            if (userCanAdd == null && superAdmin == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "You are not authorized to assign user to This project";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            #region DB check
            var project = _unitOfWork.Projects.FindAll(a => a.Id == Dto.ProjectID).FirstOrDefault();
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project with This ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {

                var AllUserList = new List<long>();
                if (Dto.AdminUsersList != null && Dto.AdminUsersList.Count() > 0)
                {
                    foreach (var item in Dto.AdminUsersList)
                    {
                        AllUserList.Add(item.UserID);
                    }
                }

                if (Dto.ManagerUsersList != null && Dto.ManagerUsersList.Count() > 0)
                {
                    foreach (var item in Dto.ManagerUsersList)
                    {
                        AllUserList.Add(item.UserID);
                    }
                }
                var usersRolesList = _unitOfWork.UserRoles.FindAll((a => AllUserList.Contains(a.UserId ?? 0)));

                var usersOfProject = _unitOfWork.ProjectAssignUsers.FindAll(a => a.ProjectId == Dto.ProjectID);

                if (Dto.AdminUsersList != null && Dto.AdminUsersList.Count() > 0)
                {

                    foreach (var user in Dto.AdminUsersList)
                    {
                        if (user.ID != 0 && user.Active == false)
                        {
                            var deleteUser = usersOfProject.Where(a => a.UserId == user.UserID).FirstOrDefault();
                            if (deleteUser != null)
                            {
                                _unitOfWork.ProjectAssignUsers.Delete(deleteUser);
                            }
                        }
                        if (user.Active == true)
                        {
                            var currentUser = usersOfProject.Where(a => a.UserId == user.UserID && a.RoleId == 145).FirstOrDefault();
                            if (currentUser == null)
                            {
                                //var userRole = usersRolesList.Where(a => a.RoleId == 144 && a.UserId == user.UserID).FirstOrDefault();
                                //if (userRole == null)
                                //{
                                //    var NewUserRole = new UserRole();
                                //    NewUserRole.RoleId = 144;           //will be changed to projectAdmin role when add to DB
                                //    NewUserRole.UserId = user.UserID;
                                //    NewUserRole.CreatedBy = Edior;
                                //    NewUserRole.CreationDate = DateTime.Now;

                                //    _unitOfWork.UserRoles.Add(NewUserRole);

                                //}
                                var NewProjectAssignUser = new ProjectAssignUser();
                                NewProjectAssignUser.ProjectId = Dto.ProjectID;
                                NewProjectAssignUser.UserId = user.UserID;
                                NewProjectAssignUser.RoleId = 145;
                                NewProjectAssignUser.RoleName = "Task Project Admin";
                                NewProjectAssignUser.Active = true;
                                NewProjectAssignUser.CreationBy = Edior;
                                NewProjectAssignUser.CreationDate = DateTime.Now;
                                NewProjectAssignUser.ModifiedBy = Edior;
                                NewProjectAssignUser.ModificationDate = DateTime.Now;
                                _unitOfWork.ProjectAssignUsers.Add(NewProjectAssignUser);

                            }
                        }

                    }
                }
                //_unitOfWork.Complete();

                //--------------------------for Project Manger---------------------------
                if (Dto.ManagerUsersList != null && Dto.ManagerUsersList.Count() > 0)
                {

                    foreach (var user in Dto.ManagerUsersList)
                    {
                        if (user.ID != 0 && user.Active == false)
                        {
                            var deleteUser = usersOfProject.Where(a => a.UserId == user.UserID && a.Id == user.ID).FirstOrDefault();
                            if (deleteUser != null)
                            {
                                _unitOfWork.ProjectAssignUsers.Delete(deleteUser);
                            }
                        }
                        if (user.Active == true)
                        {
                            var currentUser = usersOfProject.Where(a => a.UserId == user.UserID && a.RoleId == 146).FirstOrDefault();
                            if (currentUser == null)
                            {
                                //var userRole = usersRolesList.Where(a => a.RoleId == 145 && a.UserId == user.UserID).FirstOrDefault();
                                //if (userRole == null)
                                //{
                                //    var NewUserRole = new UserRole();
                                //    NewUserRole.RoleId = 145;           //will be changed to projectAdmin role when add to DB
                                //    NewUserRole.UserId = user.UserID;
                                //    NewUserRole.CreatedBy = Edior;
                                //    NewUserRole.CreationDate = DateTime.Now;

                                //    _unitOfWork.UserRoles.Add(NewUserRole);

                                //}
                                var NewProjectAssignUser = new ProjectAssignUser();
                                NewProjectAssignUser.ProjectId = Dto.ProjectID;
                                NewProjectAssignUser.UserId = user.UserID;
                                NewProjectAssignUser.RoleId = 146;
                                NewProjectAssignUser.RoleName = "Task Project Manger";
                                NewProjectAssignUser.Active = true;
                                NewProjectAssignUser.CreationBy = Edior;
                                NewProjectAssignUser.CreationDate = DateTime.Now;
                                NewProjectAssignUser.ModifiedBy = Edior;
                                NewProjectAssignUser.ModificationDate = DateTime.Now;
                                _unitOfWork.ProjectAssignUsers.Add(NewProjectAssignUser);

                            }
                        }

                    }
                }
                //_unitOfWork.Complete();

                //------------------------------------for Normal Users------------------------------------------
                if (Dto.NormalUsersList != null && Dto.NormalUsersList.Count() > 0)
                {

                    foreach (var user in Dto.NormalUsersList)
                    {

                        if (user.UserID != 0 && user.Active == false)
                        {
                            var deleteUser = usersOfProject.Where(a => a.UserId == user.UserID).FirstOrDefault();
                            if (deleteUser != null)
                            {
                                _unitOfWork.ProjectAssignUsers.Delete(deleteUser);
                            }
                        }
                        if (user.Active == true)
                        {
                            var currentUser = usersOfProject.Where(a => a.UserId == user.UserID && a.RoleId == null).FirstOrDefault();
                            if (currentUser == null)
                            {

                                var NewProjectAssignUser = new ProjectAssignUser();
                                NewProjectAssignUser.ProjectId = Dto.ProjectID;
                                NewProjectAssignUser.UserId = user.UserID;
                                NewProjectAssignUser.Active = true;
                                NewProjectAssignUser.CreationBy = Edior;
                                NewProjectAssignUser.CreationDate = DateTime.Now;
                                NewProjectAssignUser.ModifiedBy = Edior;
                                NewProjectAssignUser.ModificationDate = DateTime.Now;
                                _unitOfWork.ProjectAssignUsers.Add(NewProjectAssignUser);

                            }
                        }

                    }
                }

                _unitOfWork.Complete();
                response.ID = Dto.ProjectID;
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

        public BaseResponseWithData<List<long>> GetMangersOfProject(long projectId)
        {
            BaseResponseWithData<List<long>> response = new BaseResponseWithData<List<long>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check IN DB
            var project = _unitOfWork.Projects.FindAll((a => a.Id == projectId)).FirstOrDefault();
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project with This ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                var projectAssignUserList = _unitOfWork.ProjectAssignUsers.FindAll((a => a.ProjectId == projectId));
                var userIDs = projectAssignUserList.Select(a => a.UserId).ToList();
                var userOfproject = _unitOfWork.Users.FindAll(a => userIDs.Contains(a.Id));



                var MangersOfProject = new List<long>();
                foreach (var projectAssignUser in projectAssignUserList)
                {

                    if (projectAssignUser.RoleId == 145)
                    {
                        var user = userOfproject.Where(a => a.Id == projectAssignUser.UserId).FirstOrDefault();
                        var mangerUser = user.Id;
                        MangersOfProject.Add(mangerUser);
                    }
                }


                response.Data = MangersOfProject;
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

        public BaseResponseWithData<List<MangersOfProject>> GetMangersOfProjectByTaskID(long TaskID)
        {
            BaseResponseWithData<List<MangersOfProject>> response = new BaseResponseWithData<List<MangersOfProject>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check IN DB
            var Task = _unitOfWork.Tasks.FindAll((a => a.Id == TaskID), new[] { "Project" }).FirstOrDefault();
            if (Task == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Task with This ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (Task.Project == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This Task has no project ID";
                    response.Errors.Add(error);
                    return response;
                }

                var projectAssignUserList = _unitOfWork.ProjectAssignUsers.FindAll(a => a.ProjectId == Task.Project.Id);
                var userIDs = projectAssignUserList.Select(a => a.UserId).ToList();
                var userOfproject = _unitOfWork.Users.FindAll(a => userIDs.Contains(a.Id));



                var MangersOfProject = new List<MangersOfProject>();
                foreach (var projectAssignUser in projectAssignUserList)
                {

                    if (projectAssignUser.RoleId == 145)
                    {
                        var manger = new MangersOfProject();
                        var user = userOfproject.Where(a => a.Id == projectAssignUser.UserId).FirstOrDefault();
                        manger.Id = user.Id;
                        manger.Email = user.Email;
                        MangersOfProject.Add(manger);
                    }
                }


                response.Data = MangersOfProject;
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

        public BaseResponseWithData<decimal> GetCostsForAllTask(long ProjectID)
        {
            BaseResponseWithData<decimal> Response = new BaseResponseWithData<decimal>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var tasks = new List<GetProgressForAllTasksDto>();
            if (ProjectID == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Project Id Is Required";
                Response.Errors.Add(error);
                return Response;
            }
            var TasksProgress = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.TaskId != null && a.ProjectId != null && ProjectID == a.ProjectId, includes: new[] { "HrUser", "Task", "Project.SalesOffer" });
            tasks = TasksProgress.Select(a => new GetProgressForAllTasksDto()
            {
                //projectName = a.Project?.SalesOffer?.ProjectName,
                //TaskName = a.Task?.Name,
                //UserName = a.HrUser.FirstName + " " + a.HrUser.LastName,
                //Date = a.Date,
                //ProgressNote = a.ProgressNote,
                //ProgressPercent = (decimal)a.ProgressRate,
                //TotalHours = a.TotalHours,
                //CheckIn = a.CheckInTime != null ? (TimeOnly)a.CheckInTime : new TimeOnly(0, 0, 0),
                //CheckOut = a.CheckOutTime != null ? (TimeOnly)a.CheckOutTime : new TimeOnly(0, 0, 0),
                WorkingHourRate = true ? a.TaskRate > 1 ? a.TaskRate : a.WorkingHourRate : 0,
                Cost = 0
            }).ToList();
            tasks = tasks.Select(x => { x.Cost = true ? x.TotalHours * x.WorkingHourRate : 0; return x; }).ToList();

            var costsSum = tasks.Select(a => a.Cost).Sum();
            Response.Data = costsSum;
            return Response;
        }

        public BaseResponseWithData<List<UserWithJobTitleDDL>> GetAllUsersOfProjects(long projectId)                     //list of All users without grouped by role (Admins, mangers, normal)
        {
            BaseResponseWithData<List<UserWithJobTitleDDL>> response = new BaseResponseWithData<List<UserWithJobTitleDDL>>();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var projectAssignUserList = _unitOfWork.ProjectAssignUsers.FindAll((a => a.ProjectId == projectId));
                var userIDs = projectAssignUserList.Select(a => a.UserId).ToList();
                var userOfproject = _unitOfWork.Users.FindAll(a => userIDs.Contains(a.Id), new[] { "JobTitle" });

                var allUsers = new List<UserWithJobTitleDDL>();



                foreach (var projectAssignUser in projectAssignUserList)
                {

                    var user = userOfproject.Where(a => a.Id == projectAssignUser.UserId).FirstOrDefault();
                    var adminUser = new UserWithJobTitleDDL()
                    {
                        ID = projectAssignUser.Id,
                        UserId = user.Id,
                        FullName = user.FirstName + " " + (user.MiddleName ?? "") + " " + user.LastName,
                        ImgPath = user.PhotoUrl != null ? Globals.baseURL + user.PhotoUrl : null,
                        JobTitleName = user.JobTitle != null ? user.JobTitle.Name : null,
                    };
                    allUsers.Add(adminUser);



                }



                response.Data = allUsers;
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

        public BaseResponseWithData<List<UserWithJobTitleDDL>> GetAllNormslUsersOfProjectDDl(long projectId)                     //list of All Normal users
        {
            BaseResponseWithData<List<UserWithJobTitleDDL>> response = new BaseResponseWithData<List<UserWithJobTitleDDL>>();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var projectAssignUserList = _unitOfWork.ProjectAssignUsers.FindAll((a => a.ProjectId == projectId && a.RoleId == null));
                var userIDs = projectAssignUserList.Select(a => a.UserId).ToList();
                var NormalUserOfproject = _unitOfWork.Users.FindAll(a => userIDs.Contains(a.Id), new[] { "JobTitle" });

                var NormalUsers = new List<UserWithJobTitleDDL>();



                foreach (var projectAssignUser in projectAssignUserList)
                {

                    var user = NormalUserOfproject.Where(a => a.Id == projectAssignUser.UserId).FirstOrDefault();
                    var adminUser = new UserWithJobTitleDDL()
                    {
                        ID = projectAssignUser.Id,
                        UserId = user.Id,
                        FullName = user.FirstName + " " + (user.MiddleName ?? "") + " " + user.LastName,
                        ImgPath = user.PhotoUrl != null ? Globals.baseURL + user.PhotoUrl : null,
                        JobTitleName = user.JobTitle != null ? user.JobTitle.Name : null,
                    };
                    NormalUsers.Add(adminUser);



                }



                response.Data = NormalUsers;
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

        //---------------------------------------Project Payments---------------------------------------------------
        public BaseResponseWithId<long> AddProjectPaymentTerms(AddProjectPaymentTermsDto Dto, long UserID)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region DB check
            var project = _unitOfWork.Projects.FindAll(a => a.Id == Dto.ProjectID, new[] { "SalesOffer" }).FirstOrDefault();
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project with This ID";
                response.Errors.Add(error);
                return response;
            }
            var paymentTerm = _unitOfWork.PaymentTerms.GetById(Dto.PaymentTermID);
            if (paymentTerm == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Payment Term with This ID";
                response.Errors.Add(error);
                return response;
            }
            var currency = _unitOfWork.Currencies.GetById(Dto.CurrencyID);
            if (currency == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No currency with This ID";
                response.Errors.Add(error);
                return response;
            }
            //var DailyJournalEntry = _unitOfWork.DailyJournalEntries.GetById(Dto.DailyJournalEntryID);
            //if (DailyJournalEntry == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Daily Journal Entry with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            #endregion

            #region Date Validation
            DateTime DueDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.DueDate, out DueDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid Start Date:";
                response.Errors.Add(err);
                return response;
            }

            DateTime CollectionDate = DateTime.Now; // Default
            if (Dto.CollectionDate != null)
            {
                DateTime.TryParse(Dto.CollectionDate, out CollectionDate);
            }
            //if (!DateTime.TryParse(Dto.CollectionDate, out CollectionDate))
            //{
            //response.Result = false;
            //Error err = new Error();
            //err.ErrorCode = "E-1";
            //err.errorMSG = "please, Enter a valid Collection Date:";
            //response.Errors.Add(err);
            //return response;
            //}
            #endregion

            try
            {
                var terms = _unitOfWork.ProjectPaymentTerms.FindAll(a => a.ProjectId == Dto.ProjectID).ToList();
                var termsSum = terms.Sum(a => a.Percentage);
                if ((termsSum + Dto.Percentage) > 100)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "Percentage values can't exceed 100%";
                    response.Errors.Add(err);
                    return response;
                }
                var salaeOfferTotalAmount = project?.SalesOffer?.FinalOfferPrice ?? 0;

                var newProjectPaymentTerm = new ProjectPaymentTerm();
                newProjectPaymentTerm.ProjectId = Dto.ProjectID;
                newProjectPaymentTerm.PaymentTermId = Dto.PaymentTermID;
                newProjectPaymentTerm.Percentage = (decimal)(Dto.Percentage != null ? Dto.Percentage : (salaeOfferTotalAmount / Dto.Amount) * 100);
                newProjectPaymentTerm.Amount = (decimal)(Dto.Amount != null ? Dto.Amount : (Dto.Percentage / 100) * salaeOfferTotalAmount);
                newProjectPaymentTerm.CurrencyId = Dto.CurrencyID;
                newProjectPaymentTerm.DueDate = DueDate;
                newProjectPaymentTerm.Collected = Dto.Collected;
                newProjectPaymentTerm.CollectionDate = CollectionDate;
                newProjectPaymentTerm.Remain = newProjectPaymentTerm.Amount - newProjectPaymentTerm.Collected;
                newProjectPaymentTerm.CreatedBy = UserID;
                newProjectPaymentTerm.CreationDate = DateTime.Now;
                newProjectPaymentTerm.ModifiedBy = UserID;
                newProjectPaymentTerm.ModificationDate = DateTime.Now;
                newProjectPaymentTerm.Active = true;

                var ProjectPaymentTermDB = _unitOfWork.ProjectPaymentTerms.Add(newProjectPaymentTerm);
                _unitOfWork.Complete();

                //var ProjectPaymentJournalEntry = new ProjectPaymentJournalEntry();
                //ProjectPaymentJournalEntry.ProjectPaymentTermId = ProjectPaymentTermDB.Id;
                //ProjectPaymentJournalEntry.DailyJournalEntryId = Dto.DailyJournalEntryID;

                response.ID = ProjectPaymentTermDB.Id;
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

        public BaseResponseWithData<GetProjectPaymentTerms> GetProjectPaymentTerms(GetprojectPaymentTermsFilters filters)
        {
            var response = new BaseResponseWithData<GetProjectPaymentTerms>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            #region DateValidation

            DateTime DueDateFrom = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(filters.DueDateFrom))
            {
                if (!DateTime.TryParse(filters.DueDateFrom, out DueDateFrom))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid DueDate From ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            DateTime DueDateTo = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(filters.DueDateTo))
            {
                if (!DateTime.TryParse(filters.DueDateTo, out DueDateTo))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid DueDate To ";
                    response.Errors.Add(err);
                    return response;
                }
            }
            #endregion

            try
            {
                Expression<Func<ProjectPaymentTerm, bool>> creatria = (a => true);

                creatria = a =>
                (
                (filters.ProjectID != null ? a.ProjectId == filters.ProjectID : true) &&
                (!string.IsNullOrWhiteSpace(filters.DueDateFrom) == true ? a.DueDate >= DueDateFrom : true) &&
                (!string.IsNullOrWhiteSpace(filters.DueDateTo) == true ? a.DueDate <= DueDateTo : true) &&
                (filters.NumGreaterThanRemain != null ? a.Remain > filters.NumGreaterThanRemain : true) &&
                (filters.IsCollected == true ? a.Remain == 0 : true)
                );

                var projectPaymentTerm = _unitOfWork.ProjectPaymentTerms.FindAllPaging(creatria, filters.CurrentPage, filters.NumberOfItemsPerPage, new[] { "Project.SalesOffer", "PaymentTerm", "Currency" });

                var projectPaymentTermList = new GetProjectPaymentTerms();
                projectPaymentTermList.PaymentTermsList = new List<GetProjectPaymentTermsDto>();
                projectPaymentTermList.pagination = new PaginationHeader();


                var PaginationHeaderl = new PaginationHeader()
                {
                    CurrentPage = filters.CurrentPage,
                    ItemsPerPage = filters.NumberOfItemsPerPage,
                    TotalItems = projectPaymentTerm.TotalCount,
                    TotalPages = projectPaymentTerm.TotalPages
                };
                //var projectPaymentTerm = _Context.ProjectPaymentTerms.Where((a => a.Active == true)).Include(a => a.Project.SalesOffer).Include(a => a.Currency).Include(a => a.PaymentTerm).ToList();
                var projectPaymentTermData = _mapper.Map<List<GetProjectPaymentTermsDto>>(projectPaymentTerm);
                projectPaymentTermList.PaymentTermsList.AddRange(projectPaymentTermData);

                response.Data = projectPaymentTermList;
                response.Data.pagination = PaginationHeaderl;
                response.Data.TotalRemain = projectPaymentTermData.Sum(a => a.Remain);
                response.Data.TotalCollected = projectPaymentTermData.Sum(a => a.Collected);
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

        public BaseResponseWithData<List<GetPaymentTermDDL>> GetPaymentTermsDDL()
        {
            BaseResponseWithData<List<GetPaymentTermDDL>> Response = new BaseResponseWithData<List<GetPaymentTermDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {


                if (Response.Result)
                {
                    var PaymentTermList = _unitOfWork.PaymentTerms.GetAll();
                    PaymentTermList = PaymentTermList.OrderBy(a => a.PaymentTermName);
                    var PaymentTermDDL = new List<GetPaymentTermDDL>();
                    foreach (var PaymentTerm in PaymentTermList)
                    {
                        var DDLObj = new GetPaymentTermDDL();
                        DDLObj.ID = PaymentTerm.Id;
                        DDLObj.Name = PaymentTerm.PaymentTermName.Trim();

                        PaymentTermDDL.Add(DDLObj);
                    }
                    Response.Data = PaymentTermDDL;

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

        public BaseResponseWithId<int> AddPaymentTerms(string paymentTerm)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                if (string.IsNullOrEmpty(paymentTerm))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a payment term name";
                    response.Errors.Add(err);
                    return response;
                }
                var newPaymentTerm = new PaymentTerm();
                newPaymentTerm.PaymentTermName = paymentTerm.Trim();

                _unitOfWork.PaymentTerms.Add(newPaymentTerm);
                _unitOfWork.Complete();

                response.ID = newPaymentTerm.Id;
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

        public BaseResponseWithId<long> EditProjectPaymentTerms(EditProjectPaymentTermsDto Dto, long UserID)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region DB check
            var projectPaymentTerm = _unitOfWork.ProjectPaymentTerms.FindAll(a => a.Id == Dto.ID).FirstOrDefault();
            if (projectPaymentTerm == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No projectPaymentTerm with This ID";
                response.Errors.Add(error);
                return response;
            }
            var project = _unitOfWork.Projects.FindAll(a => a.Id == Dto.ProjectID, new[] { "SalesOffer" }).FirstOrDefault();
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project with This ID";
                response.Errors.Add(error);
                return response;
            }
            var paymentTerm = _unitOfWork.PaymentTerms.GetById(Dto.PaymentTermID);
            if (paymentTerm == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Payment Term with This ID";
                response.Errors.Add(error);
                return response;
            }
            var currency = _unitOfWork.Currencies.GetById(Dto.CurrencyID);
            if (currency == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No currency with This ID";
                response.Errors.Add(error);
                return response;
            }
            //var DailyJournalEntry = _unitOfWork.DailyJournalEntries.GetById(Dto.DailyJournalEntryID);
            //if (DailyJournalEntry == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Daily Journal Entry with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            #endregion

            #region Date Validation
            DateTime DueDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.DueDate, out DueDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid Start Date:";
                response.Errors.Add(err);
                return response;
            }

            DateTime CollectionDate = projectPaymentTerm.CollectionDate; // Default
            if (Dto.CollectionDate != null)
            {
                DateTime.TryParse(Dto.CollectionDate, out CollectionDate);
            }
            //DateTime CollectionDate = DateTime.Now;
            //if (!DateTime.TryParse(Dto.CollectionDate, out CollectionDate))
            //{
            //    //response.Result = false;
            //    //Error err = new Error();
            //    //err.ErrorCode = "E-1";
            //    //err.errorMSG = "please, Enter a valid Collection Date:";
            //    //response.Errors.Add(err);
            //    //return response;
            //}
            #endregion

            try
            {
                var termsSum = _unitOfWork.ProjectPaymentTerms.FindAll(a => a.ProjectId == Dto.ProjectID && a.Id != Dto.ID).Sum(a => a.Percentage);
                //var termsSum = terms.Sum(a => a.Percentage);
                if ((termsSum + Dto.Percentage) > 100)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "Percentage values can't exceed 100%";
                    response.Errors.Add(err);
                    return response;
                }

                if (Dto.Active == false) // Delete this Payment Terms Selected
                {
                    _unitOfWork.ProjectPaymentTerms.Delete(projectPaymentTerm);
                    _unitOfWork.Complete();
                    response.ID = Dto.ID;
                    return response;
                }


                var salaeOfferTotalAmount = project?.SalesOffer?.FinalOfferPrice ?? 0;

                //var newProjectPaymentTerm = new ProjectPaymentTerm();
                projectPaymentTerm.ProjectId = Dto.ProjectID;
                projectPaymentTerm.PaymentTermId = Dto.PaymentTermID;
                projectPaymentTerm.Percentage = (decimal)(Dto.Percentage != null ? Dto.Percentage : (salaeOfferTotalAmount / Dto.Amount) * 100);
                projectPaymentTerm.Amount = (decimal)(Dto.Amount != null ? Dto.Amount : (Dto.Percentage / 100) * salaeOfferTotalAmount);
                projectPaymentTerm.CurrencyId = Dto.CurrencyID;
                projectPaymentTerm.DueDate = DueDate;
                projectPaymentTerm.Collected = Dto.Collected;
                projectPaymentTerm.CollectionDate = CollectionDate;
                projectPaymentTerm.Remain = projectPaymentTerm.Amount - projectPaymentTerm.Collected;
                projectPaymentTerm.ModifiedBy = UserID;
                projectPaymentTerm.ModificationDate = DateTime.Now;
                projectPaymentTerm.Active = true;

                //var ProjectPaymentTermDB = _unitOfWork.ProjectPaymentTerms.Add(newProjectPaymentTerm);
                _unitOfWork.Complete();



                response.ID = projectPaymentTerm.Id;
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

        public BaseResponseWithData<List<CostTypeDDL>> GetPaymentMethodDDl()
        {
            var response = new BaseResponseWithData<List<CostTypeDDL>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    var PaymentTermList = _unitOfWork.PaymentMethods.GetAll();
                    PaymentTermList = PaymentTermList.OrderBy(a => a.Name);
                    var PaymentTermDDL = new List<CostTypeDDL>();
                    foreach (var PaymentTerm in PaymentTermList)
                    {
                        var DDLObj = new CostTypeDDL();
                        DDLObj.ID = PaymentTerm.Id;
                        DDLObj.Name = PaymentTerm.Name.Trim();

                        PaymentTermDDL.Add(DDLObj);
                    }
                    response.Data = PaymentTermDDL;

                }

                return response;

                //if (response.Result)
                //{
                //    var PaymentTermsList = _taskMangerProjectService.GetPaymentTermsDDL();
                //    if (!PaymentTermsList.Result)
                //    {
                //        response.Result = false;
                //        response.Errors.AddRange(PaymentTermsList.Errors);
                //    }
                //    response = PaymentTermsList;
                //}
                //return response;
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

        public BaseResponseWithData<string> GetProjectPaymentTermsReport([FromHeader] GetprojectPaymentTermsFilters filters, string CompanyName)
        {
            var response = new BaseResponseWithData<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            #region DateValidation

            DateTime DueDateFrom = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(filters.DueDateFrom))
            {
                if (!DateTime.TryParse(filters.DueDateFrom, out DueDateFrom))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid DueDate From ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            DateTime DueDateTo = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(filters.DueDateTo))
            {
                if (!DateTime.TryParse(filters.DueDateTo, out DueDateTo))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid DueDate To ";
                    response.Errors.Add(err);
                    return response;
                }
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    Expression<Func<ProjectPaymentTerm, bool>> creatria = (a => true);

                    creatria = a =>
                    (
                    (filters.ProjectID != null ? a.ProjectId == filters.ProjectID : true) &&
                    (!string.IsNullOrWhiteSpace(filters.DueDateFrom) == true ? a.DueDate >= DueDateFrom : true) &&
                    (!string.IsNullOrWhiteSpace(filters.DueDateTo) == true ? a.DueDate <= DueDateTo : true) &&
                    (filters.NumGreaterThanRemain != null ? a.Remain > filters.NumGreaterThanRemain : true) &&
                    (filters.IsCollected == true ? a.Remain == 0 : true)
                    );

                    var projectPaymentTermList = _unitOfWork.ProjectPaymentTerms.FindAllPaging(creatria, filters.CurrentPage, filters.NumberOfItemsPerPage, new[] { "Project.SalesOffer", "PaymentTerm", "Currency" });


                    ExcelPackage excel = new ExcelPackage();
                    var worksheet = excel.Workbook.Worksheets.Add("ProjectPaymentTerm");
                    worksheet.DefaultRowHeight = 12;
                    worksheet.Row(1).Height = 20;
                    worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Row(1).Style.Font.Bold = true;
                    worksheet.Cells[1, 1, 1, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, 1, 1, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CadetBlue);
                    worksheet.Cells[1, 1, 1, 10].Style.Font.Color.SetColor(System.Drawing.Color.White);

                    worksheet.Cells[1, 1].Value = "ID";
                    worksheet.Cells[1, 2].Value = "Project";
                    worksheet.Cells[1, 3].Value = "Payment Term";
                    worksheet.Cells[1, 4].Value = "Percentage";
                    worksheet.Cells[1, 5].Value = "Amount";
                    worksheet.Cells[1, 6].Value = "Currency";
                    worksheet.Cells[1, 7].Value = "Due Date";
                    worksheet.Cells[1, 8].Value = "Collected";
                    worksheet.Cells[1, 9].Value = "CollectionDate";
                    worksheet.Cells[1, 10].Value = "Remain";

                    if (projectPaymentTermList.Count() > 0)
                    {
                        var list = projectPaymentTermList.ToList();
                        int recordIndex = 2;
                        foreach (var item in list)
                        {
                            worksheet.Cells[recordIndex, 1].Value = item.Id;
                            worksheet.Cells[recordIndex, 2].Value = item.Project?.SalesOffer?.ProjectName;
                            worksheet.Cells[recordIndex, 3].Value = item.PaymentTerm.PaymentTermName;
                            worksheet.Cells[recordIndex, 4].Value = item.Percentage;
                            worksheet.Cells[recordIndex, 5].Value = item.Amount;
                            worksheet.Cells[recordIndex, 6].Value = item.Currency.Name;
                            worksheet.Cells[recordIndex, 7].Value = item.DueDate.ToShortDateString();
                            worksheet.Cells[recordIndex, 8].Value = item.Collected;
                            worksheet.Cells[recordIndex, 9].Value = item.CollectionDate.ToShortDateString();
                            worksheet.Cells[recordIndex, 10].Value = item.Remain;
                            worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Row(recordIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            recordIndex++;
                        }
                        worksheet.Column(4).Style.Numberformat.Format = "#0\\.00%";
                        worksheet.Column(5).Style.Numberformat.Format = "#,##0.00";
                        worksheet.Column(7).Style.Numberformat.Format = "yyyy/mm/dd";
                        worksheet.Column(8).Style.Numberformat.Format = "#,##0.00";
                        worksheet.Column(9).Style.Numberformat.Format = "yyyy/mm/dd";
                        worksheet.Column(10).Style.Numberformat.Format = "#,##0.00";

                        worksheet.Column(1).AutoFit();
                        worksheet.Column(2).AutoFit();
                        worksheet.Column(3).AutoFit();
                        worksheet.Column(4).AutoFit();
                        worksheet.Column(5).AutoFit();
                        worksheet.Column(6).AutoFit();
                        worksheet.Column(7).AutoFit();
                        worksheet.Column(8).AutoFit();
                        worksheet.Column(9).AutoFit();
                        worksheet.Column(10).AutoFit();

                        var path = $"Attachments\\{CompanyName}\\ProjectPaymentTerms";
                        var savedPath = Path.Combine(_host.WebRootPath, path);
                        if (System.IO.File.Exists(savedPath))
                            System.IO.File.Delete(savedPath);

                        // Create excel file on physical disk  
                        Directory.CreateDirectory(savedPath);
                        //FileStream objFileStrm = File.Create(savedPath);
                        //objFileStrm.Close();
                        var excelPath = savedPath + $"\\ProjectPaymentTermReport.xlsx";
                        excel.SaveAs(excelPath);
                        // Write content to excel file  
                        //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                        //Close Excel package 
                        excel.Dispose();
                        response.Data = Globals.baseURL + '\\' + path + $"\\ProjectPaymentTermReport.xlsx";
                    }

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

        //---------------------------------------Project Letter of Credit-------------------------------------------
        public BaseResponseWithId<long> AddProjectLetterOfCredit(AddProjectLetterOfCreditDto Dto, long Creator)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            #region DB check
            var project = _unitOfWork.Projects.FindAll(a => a.Id == Dto.ProjectID, new[] { "SalesOffer" }).FirstOrDefault();
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project with This ID";
                response.Errors.Add(error);
                return response;
            }
            var currency = _unitOfWork.Currencies.GetById(Dto.CurrencyID);
            if (currency == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No currency with This ID";
                response.Errors.Add(error);
                return response;
            }
            var letterOfCredit = _unitOfWork.LetterOfCreditTypies.GetById(Dto.LetterOfCreditTypeID);
            if (letterOfCredit == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Letter Of Credit Type with This ID";
                response.Errors.Add(error);
                return response;
            }

            #endregion

            #region Date Validation
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

            DateTime EndDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.EndDate, out EndDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid End Date:";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var newProjectLOC = new ProjectLetterOfCredit();
                newProjectLOC.ProjectId = Dto.ProjectID;
                newProjectLOC.LetterOfCreditTypeId = Dto.LetterOfCreditTypeID;
                newProjectLOC.ReturnedAfter = Dto.ReturnedAfter;
                newProjectLOC.BankName = Dto.bankName;
                newProjectLOC.StartDate = startDate;
                newProjectLOC.Amout = Dto.Amount;
                newProjectLOC.CurrencyId = Dto.CurrencyID;
                newProjectLOC.EndDate = EndDate;
                newProjectLOC.Status = "Not Returned";
                newProjectLOC.CreatedBy = Creator;
                newProjectLOC.CreationDate = DateTime.Now;
                newProjectLOC.ModifiedBy = Creator;
                newProjectLOC.ModificationDate = DateTime.Now;
                newProjectLOC.Active = true;

                _unitOfWork.ProjectLetterOfCredits.Add(newProjectLOC);
                _unitOfWork.Complete();


                response.ID = newProjectLOC.Id;
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

        public BaseResponseWithDataAndHeader<List<GetProjectLetterOfCreditDto>> GetProjectLetterOfCredit([FromHeader] ProjectLetterOfCreditGetModel request)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetProjectLetterOfCreditDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var status = "";
                if (request.Status != null)
                {
                    status = HttpUtility.UrlDecode(request.Status);
                }

                Expression<Func<ProjectLetterOfCredit, bool>> creatria = (a => true);

                creatria = a =>
                (
                (request.ClienId != null ? a.Project.SalesOffer.ClientId == request.ClienId : true) &&
                (request.SalesPersonID != null ? a.Project.SalesOffer.SalesPersonId == request.SalesPersonID : true) &&
                (request.ProjectID != null ? a.ProjectId == request.ProjectID : true) &&
                ((!string.IsNullOrWhiteSpace(status) == true) ? a.Status == status : true) &&
                ((!string.IsNullOrWhiteSpace(request.BankName) == true) ? a.BankName == request.BankName : true) &&
                (request.LetterOfCreditTypeID != null ? a.LetterOfCreditTypeId == request.LetterOfCreditTypeID : true)
                );

                var ProjectLetterOfCreditList = new List<GetProjectLetterOfCreditDto>();

                var ProjectLetterOfCredit = _unitOfWork.ProjectLetterOfCredits.FindAllPaging(creatria, CurrentPage: request.currentPage, request.NumberOfitemsPerPage, new[] { "LetterOfCreditType", "Currency", "Project", "Project.SalesOffer" });
                if (ProjectLetterOfCredit == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "No Project with this Id";
                    response.Errors.Add(error);
                    return response;
                }

                var ProjectLetterOfCreditData = _mapper.Map<List<GetProjectLetterOfCreditDto>>(ProjectLetterOfCredit);
                ProjectLetterOfCreditList.AddRange(ProjectLetterOfCreditData);

                var paging = new PaginationHeader()
                {
                    CurrentPage = request.currentPage,
                    ItemsPerPage = request.currentPage,
                    TotalPages = ProjectLetterOfCredit.TotalPages,
                    TotalItems = ProjectLetterOfCredit.TotalCount
                };

                response.PaginationHeader = paging;
                response.Data = ProjectLetterOfCreditList;
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

        public BaseResponseWithId<long> EditProjectLetterOfCredit(EditProjectLetterOfCreditDto Dto, long Editor)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();


            #region DB check
            var ProjectLOC = _unitOfWork.ProjectLetterOfCredits.GetById(Dto.ID);
            if (ProjectLOC == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project Letter Of Credits with This ID";
                response.Errors.Add(error);
                return response;
            }
            var project = _unitOfWork.Projects.FindAll(a => a.Id == Dto.ProjectID, new[] { "SalesOffer" }).FirstOrDefault();
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project with This ID";
                response.Errors.Add(error);
                return response;
            }
            var currency = _unitOfWork.Currencies.GetById(Dto.CurrencyID);
            if (currency == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No currency with This ID";
                response.Errors.Add(error);
                return response;
            }
            var letterOfCredit = _unitOfWork.LetterOfCreditTypies.GetById(Dto.LetterOfCreditTypeID);
            if (letterOfCredit == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Letter Of Credit Type with This ID";
                response.Errors.Add(error);
                return response;
            }

            #endregion

            #region Date Validation
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

            DateTime EndDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.EndDate, out EndDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid End Date:";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {

                ProjectLOC.ProjectId = Dto.ProjectID;
                ProjectLOC.LetterOfCreditTypeId = Dto.LetterOfCreditTypeID;
                ProjectLOC.ReturnedAfter = Dto.ReturnedAfter;
                ProjectLOC.BankName = Dto.bankName;
                ProjectLOC.StartDate = startDate;
                ProjectLOC.Amout = Dto.Amount;
                ProjectLOC.CurrencyId = Dto.CurrencyID;
                ProjectLOC.EndDate = EndDate;
                ProjectLOC.ModifiedBy = Editor;
                ProjectLOC.ModificationDate = DateTime.Now;
                ProjectLOC.Active = true;

                _unitOfWork.Complete();


                response.ID = ProjectLOC.Id;
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

        public BaseResponseWithId<long> EditProjectLetterOfCreditStatus(long ProjectLetterOfCredit, bool status)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();



            #region Check in DB
            var ProjectLOC = _unitOfWork.ProjectLetterOfCredits.GetById(ProjectLetterOfCredit);
            if (ProjectLOC == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project Letter Of Credits with This ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (status == true) ProjectLOC.Status = "Returned";
                else
                {
                    ProjectLOC.Status = "Not Returned";
                }
                _unitOfWork.Complete();
                response.ID = ProjectLOC.Id;
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

        public BaseResponseWithId<long> AddProjectLetterOfCreditComment(long prjectLetterOfCreditID, string comment, long Creator)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            #region DB check
            var project = _unitOfWork.ProjectLetterOfCredits.FindAll(a => a.Id == prjectLetterOfCreditID).FirstOrDefault();
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project Letter Of Credits with This ID";
                response.Errors.Add(error);
                return response;
            }

            #endregion

            try
            {
                var newComment = new ProjectLetterOfCreditComment();
                newComment.Comment = comment;
                newComment.ProjectLetterOfCreditId = prjectLetterOfCreditID;
                newComment.CreatedBy = Creator;
                newComment.CreationDate = DateTime.Now;
                newComment.ModifiedBy = Creator;
                newComment.ModificationDate = DateTime.Now;
                newComment.Active = true;

                _unitOfWork.ProjectLetterOfCreditComments.Add(newComment);
                _unitOfWork.Complete();


                response.ID = newComment.Id;
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

        public BaseResponseWithId<long> EditProjectLetterOfCreditComment(long CommentID, long prjectLetterOfCreditID, string comment, long Creator)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };


            #region DB check
            var project = _unitOfWork.ProjectLetterOfCredits.FindAll(a => a.Id == prjectLetterOfCreditID).FirstOrDefault();
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project Letter Of Credits with This ID";
                response.Errors.Add(error);
                return response;
            }
            var commentDb = _unitOfWork.ProjectLetterOfCreditComments.GetById(CommentID);
            if (commentDb == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Project Letter Of Credits comment with This ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {

                commentDb.Comment = comment;
                commentDb.ProjectLetterOfCreditId = prjectLetterOfCreditID;
                commentDb.ModifiedBy = Creator;
                commentDb.ModificationDate = DateTime.Now;



                _unitOfWork.Complete();


                response.ID = commentDb.Id;
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

        public BaseResponseWithData<List<GetProjectLetterOfCreditCommentDto>> GetProjectLetterOfCreditComment(long ProjectLetterOfCreditID)
        {
            var response = new BaseResponseWithData<List<GetProjectLetterOfCreditCommentDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    var commentList = _unitOfWork.ProjectLetterOfCreditComments.FindAll(a => a.ProjectLetterOfCreditId == ProjectLetterOfCreditID, new[] { "CreatedByNavigation" });

                    var data = _mapper.Map<List<GetProjectLetterOfCreditCommentDto>>(commentList);
                    response.Data = data;
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

        public BaseResponseWithData<List<CostTypeDDL>> GetLetterOfCreditTypeDDL()
        {
            var response = new BaseResponseWithData<List<CostTypeDDL>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    var LetterOfCreditTypies = _unitOfWork.LetterOfCreditTypies.GetAll();
                    //PaymentTermList = PaymentTermList.OrderBy(a => a.Name);
                    var PaymentTermDDL = new List<CostTypeDDL>();
                    foreach (var PaymentTerm in LetterOfCreditTypies)
                    {
                        var DDLObj = new CostTypeDDL();
                        DDLObj.ID = PaymentTerm.Id;
                        DDLObj.Name = PaymentTerm.LoctypeName.Trim();

                        PaymentTermDDL.Add(DDLObj);
                    }
                    response.Data = PaymentTermDDL;

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

        public BaseResponseWithData<string> GetProjectLetterOfCreditReport([FromHeader] ProjectLetterOfCreditGetModel request, string companyname)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                Expression<Func<ProjectLetterOfCredit, bool>> creatria = (a => true);

                creatria = a =>
                (
                (request.ClienId != null ? a.Project.SalesOffer.ClientId == request.ClienId : true) &&
                (request.SalesPersonID != null ? a.Project.SalesOffer.SalesPersonId == request.SalesPersonID : true) &&
                (request.ProjectID != null ? a.ProjectId == request.ProjectID : true) &&
                ((!string.IsNullOrWhiteSpace(request.Status) == true) ? a.Status == request.Status : true) &&
                ((!string.IsNullOrWhiteSpace(request.BankName) == true) ? a.BankName == request.BankName : true) &&
                (request.LetterOfCreditTypeID != null ? a.LetterOfCreditTypeId == request.LetterOfCreditTypeID : true)
                );

                var ProjectList = _unitOfWork.ProjectLetterOfCredits.FindAll(creatria, includes: new[] { "LetterOfCreditType", "ProjectLetterOfCreditComments", "Currency", "Project", "Project.SalesOffer.Client", "Project.SalesOffer.SalesPerson", "Project.ProjectProgresses" }).ToList();

                /*_Context.ProjectLetterOfCredits.Where(a => ProjectID == null || (a.ProjectId == ProjectID)).Include(a => a.Currency).Include(a => a.LetterOfCreditType).Include(a => a.ProjectLetterOfCreditComments).Include(a => a.Currency).Include(a => a.Project).ThenInclude(a => a.SalesOffer).ThenInclude(a => a.Client).ThenInclude(a => a.SalesPerson).Include(a => a.Project.ProjectProgresses).ToList();*/

                ExcelPackage excel = new ExcelPackage();
                var worksheet = excel.Workbook.Worksheets.Add("ProjectLetterOfCredit");
                worksheet.DefaultRowHeight = 12;
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(2).Style.Font.Bold = true;
                worksheet.View.RightToLeft = true;

                worksheet.Cells[1, 1, 1, 29].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 29].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(224, 105, 32));
                worksheet.Cells[1, 1, 1, 29].Style.Font.Color.SetColor(System.Drawing.Color.White);
                /*worksheet.Column(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Column(1).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(224,105,32));
                worksheet.Column(1).Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(224,105,32));
                worksheet.Column(1).Style.Font.Bold = true;*/

                worksheet.Cells[1, 1].Value = "م";
                worksheet.Cells[1, 2].Value = "المندوب";
                worksheet.Cells[1, 3].Value = "رقم أمر الشغل";
                worksheet.Cells[1, 4].Value = "اسم العميل";
                worksheet.Cells[1, 5].Value = "";
                worksheet.Column(5).Width = 1;
                worksheet.Cells[1, 6].Value = "نوع الخطاب";
                worksheet.Cells[1, 7].Value = "البنك";
                worksheet.Cells[1, 8].Value = "مبلغ الخطاب";
                worksheet.Cells[1, 9].Value = "بداية الخطاب";
                worksheet.Cells[1, 10].Value = "نهاية الخطاب";
                worksheet.Cells[1, 11].Value = "ملاحظات";
                worksheet.Cells[1, 12].Value = "";
                worksheet.Column(12).Width = 1;
                for (int i = 1; i <= 12; i++)
                {
                    worksheet.Cells[1, i, 2, i].Merge = true;
                    worksheet.Cells[1, i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                }
                worksheet.Cells[1, 13].Value = "مراحل التعاقد من حيث رد خطاب الضمان";

                worksheet.Cells[2, 1, 2, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Column(1).Width = 2;
                for (int i = 1; i <= 29; i++)
                {
                    if (i != 4 && i != 5 && i != 11 && i != 12)
                    {
                        worksheet.Column(i).Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Column(i).Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    }
                }
                worksheet.Cells[1, 13, 1, 29].Merge = true;
                worksheet.Cells[2, 1, 2, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(224, 105, 32));
                worksheet.Column(5).Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Column(5).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Column(12).Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Column(12).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 13, 2, 29].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[2, 13, 2, 29].Style.Font.Color.SetColor(System.Drawing.Color.White);
                worksheet.Cells[2, 13, 2, 29].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[2, 13, 2, 29].Style.WrapText = true;
                //worksheet.Row(2).Height = 50;
                worksheet.Row(2).CustomHeight = false;
                worksheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(224, 105, 32));
                worksheet.Cells[2, 13].Value = "امر شغل نهائي";
                worksheet.Cells[2, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(224, 105, 32));
                worksheet.Cells[2, 14].Value = "استلام الدفعه المقدمه";
                worksheet.Cells[2, 15].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 15].Value = "معاينه شركه مارينا";
                worksheet.Cells[2, 16].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 16].Value = "رسومات اعمال مدنيه مكتب فنى";
                worksheet.Cells[2, 17].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 17].Value = "اعمال مدنيه تم ارسالها للعميل";
                worksheet.Cells[2, 18].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 18].Value = "موافقه اعمال مدنيه تم ارسالها للعميل";
                worksheet.Cells[2, 19].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 19].Value = "رسومات التصنيع مكتب فنى";
                worksheet.Cells[2, 20].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 20].Value = "مرحله التصنيع الورش";
                worksheet.Cells[2, 21].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 21].Value = "استلام اعمال مدنيه من العميل";
                worksheet.Cells[2, 22].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 22].Value = "توريد اجزاء اولى";
                worksheet.Cells[2, 23].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(224, 105, 32));
                worksheet.Cells[2, 23].Value = "استلام دفعه توريد اجزاء اولى";
                worksheet.Cells[2, 24].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 24].Value = "تركيب اجزاء اولى";
                worksheet.Cells[2, 25].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 25].Value = "توريد اجزاء تانيه";
                worksheet.Cells[2, 26].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(224, 105, 32));
                worksheet.Cells[2, 26].Value = "استلام دفعه توريد اجزاء تانيه";
                worksheet.Cells[2, 27].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 27].Value = "تركيب اجزاء ثانيه";
                worksheet.Cells[2, 28].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 28].Value = "محضر الاستلام";
                worksheet.Cells[2, 29].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(42, 75, 238));
                worksheet.Cells[2, 29].Value = "ملاحظات محضر الاستلام";

                #region FeedingData
                if (ProjectList.Count > 0)
                {
                    int recordIndex = 3;
                    int index = 1;
                    foreach (var item in ProjectList)
                    {
                        worksheet.Cells[recordIndex, 1].Value = index;
                        worksheet.Cells[recordIndex, 1, recordIndex + 1, 1].Merge = true;
                        worksheet.Cells[recordIndex, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[recordIndex, 2].Value = item.Project.SalesOffer.SalesPerson.FirstName + " "
                            + item.Project.SalesOffer.SalesPerson.LastName;
                        worksheet.Cells[recordIndex, 2, recordIndex + 1, 2].Merge = true;
                        worksheet.Cells[recordIndex, 3].Value = item.Project.SalesOffer.ProjectName + ((item.Project.ProjectSerial != null) ? " - " + item.Project.ProjectSerial : "");
                        worksheet.Cells[recordIndex, 3, recordIndex + 1, 3].Merge = true;
                        worksheet.Cells[recordIndex, 4].Value = item.Project.SalesOffer.Client.Name;
                        worksheet.Cells[recordIndex, 4, recordIndex + 1, 4].Merge = true;
                        worksheet.Cells[recordIndex, 5, recordIndex + 1, 5].Merge = true;
                        worksheet.Cells[recordIndex, 6].Value = item.LetterOfCreditType.LoctypeName;
                        worksheet.Cells[recordIndex, 6, recordIndex + 1, 6].Merge = true;
                        worksheet.Cells[recordIndex, 7].Value = item.BankName;
                        worksheet.Cells[recordIndex, 7, recordIndex + 1, 7].Merge = true;
                        worksheet.Cells[recordIndex, 8].Value = item.Amout + " - " + item.Currency.Name;
                        worksheet.Cells[recordIndex, 8, recordIndex + 1, 8].Merge = true;
                        worksheet.Cells[recordIndex, 9].Value = item.StartDate.ToShortDateString();
                        worksheet.Cells[recordIndex, 9, recordIndex + 1, 9].Merge = true;
                        worksheet.Cells[recordIndex, 10].Value = item.EndDate.ToShortDateString();
                        worksheet.Cells[recordIndex, 10, recordIndex + 1, 10].Merge = true;
                        worksheet.Cells[recordIndex, 11].Value = string.Join(@"\n", item.ProjectLetterOfCreditComments.Select(a => a.Comment));
                        worksheet.Cells[recordIndex, 11, recordIndex + 1, 11].Merge = true;
                        worksheet.Cells[recordIndex, 12, recordIndex + 1, 12].Merge = true;
                        worksheet.Cells[recordIndex, 13].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 13].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 13].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 1).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 13].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 1).FirstOrDefault()?.Date.ToShortDateString() ?? "";
                        worksheet.Cells[recordIndex, 14].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 14].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 14].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 2).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 14].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 2).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 15].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 15].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 15].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 3).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 15].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 3).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 16].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 16].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 16].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 4).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 16].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 4).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 17].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 17].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 17].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 5).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 17].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 5).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 18].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 18].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 18].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 6).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 18].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 6).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 19].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 19].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 19].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 7).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 19].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 7).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 20].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 20].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 20].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 8).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 20].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 8).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 21].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 21].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 21].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 9).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 21].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 9).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 22].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 22].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 22].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 10).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 22].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 10).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 23].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 23].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 23].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 11).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 23].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 11).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 24].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 24].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 24].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 12).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 24].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 12).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 25].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 25].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 25].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 13).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 25].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 13).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 26].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 26].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 26].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 14).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 26].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 14).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 27].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 27].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 27].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 15).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 27].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 15).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 28].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 28].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 28].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 16).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 28].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 16).FirstOrDefault()?.Date.ToShortDateString() ?? "";

                        worksheet.Cells[recordIndex, 29].Style.Font.Charset = 2;
                        worksheet.Cells[recordIndex, 29].Style.Font.SetFromFont("Wingdings", 16);
                        worksheet.Cells[recordIndex, 29].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 17).FirstOrDefault() != null ? "ü" : "û";
                        worksheet.Cells[recordIndex + 1, 29].Value = item.Project.ProjectProgresses.Where(a => a.ProgressTypeId == 17).FirstOrDefault()?.Date.ToShortDateString() ?? "";


                        worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Row(recordIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells[recordIndex, 2, recordIndex + 1, 29].Style.Border.BorderAround(ExcelBorderStyle.Medium);

                        recordIndex++;
                        recordIndex++;
                        index++;
                    }
                    worksheet.Column(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Column(1).Style.Font.Color.SetColor(System.Drawing.Color.White);
                    worksheet.Column(1).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(224, 105, 32));
                    var font = new Font("Calibri", 11);

                    for (int i = 1; i < 13; i++)
                    {
                        if (i != 1 && i != 5 && i != 12)
                        {
                            var maxWidth = worksheet.Cells[GetColumnRange(worksheet, i).Address].Max(c => c.Value != null ? GetTextWidth(c.Value.ToString(), font) : 0);
                            worksheet.Column(i).Width = (double)maxWidth / (double)6.5;
                        }
                        worksheet.Column(i).Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Column(i).Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    }
                    worksheet.Column(5).Style.Border.Left.Style = ExcelBorderStyle.None;
                    worksheet.Column(5).Style.Border.Right.Style = ExcelBorderStyle.None;
                    worksheet.Column(6).Style.Border.Left.Style = ExcelBorderStyle.None;
                    worksheet.Column(12).Style.Border.Left.Style = ExcelBorderStyle.None;
                    worksheet.Column(12).Style.Border.Right.Style = ExcelBorderStyle.None;
                    worksheet.Column(13).Style.Border.Left.Style = ExcelBorderStyle.None;
                }

                #endregion


                var path = $"Attachments\\{companyname}\\ProjectLetterOfCredits";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (System.IO.File.Exists(savedPath))
                    System.IO.File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\ProjectLetterOfCredits_{date}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                response.Data = Globals.baseURL + '\\' + path + $"\\ProjectLetterOfCredits_{date}.xlsx";

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

        public BaseResponseWithId<List<long>> AddProjectLetterOfCreditCommentList(AddLetterOfCreditCommentList comments, long UserID)
        {
            var response = new BaseResponseWithId<List<long>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                int count = 1;
                foreach (var projectComment in comments.CommentList)
                {
                    var project = _unitOfWork.ProjectLetterOfCredits.FindAll(a => a.Id == projectComment.ProjectLetterOfCreditID).FirstOrDefault();
                    if (project == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = $"No Project Letter Of Credits with This ID at index number {count}";
                        response.Errors.Add(error);
                        return response;
                    }

                    count++;
                }

                if (response.Result)
                {
                    var commentList = new List<ProjectLetterOfCreditComment>();

                    foreach (var Comment in comments.CommentList)
                    {
                        var newComment = new ProjectLetterOfCreditComment();
                        newComment.Comment = Comment.comment;
                        newComment.ProjectLetterOfCreditId = Comment.ProjectLetterOfCreditID;
                        newComment.CreatedBy = UserID;
                        newComment.CreationDate = DateTime.Now;
                        newComment.ModifiedBy = UserID;
                        newComment.ModificationDate = DateTime.Now;
                        newComment.Active = true;

                        commentList.Add(newComment);
                    }

                    _unitOfWork.ProjectLetterOfCreditComments.AddRange(commentList);
                    _unitOfWork.Complete();
                    var commentsIDs = new List<long>();
                    commentsIDs.AddRange(commentList.Select(a => a.Id));
                    response.ID = commentsIDs;
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
        private static float GetTextWidth(string text, Font font)
        {
            using (var graphics = Graphics.FromImage(new Bitmap(1, 1)))
            {
                var size = graphics.MeasureString(text, font);
                return size.Width;
            }
        }
        private static ExcelRange GetColumnRange(ExcelWorksheet worksheet, int columnNumber)
        {
            // Get the dimension of the worksheet
            var dimension = worksheet.Dimension;

            // Check if the worksheet is empty
            if (dimension == null)
            {
                return null;
            }

            // Get the range of cells in the specified column
            var startCell = worksheet.Cells[dimension.Start.Row, columnNumber];
            var endCell = worksheet.Cells[dimension.End.Row, columnNumber];

            return worksheet.Cells[startCell.Start.Row, startCell.Start.Column, endCell.End.Row, endCell.End.Column];
        }

        //--------------------------------------Delete APIs---------------------------------------------------------
        public BaseResponseWithId<long> DeleteUsersAssiginToProject(long id)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check in DB
            var project = _unitOfWork.Projects.GetById(id);
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Task Manger Project with this Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                var users = _unitOfWork.ProjectAssignUsers.FindAll(a => a.ProjectId == id);

                _unitOfWork.ProjectAssignUsers.DeleteRange(users);
                _unitOfWork.Complete();
                response.ID = id;
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

        //-------------------------------------Archived APIs--------------------------------------------------------
        public BaseResponseWithId<long> ArchiveProject(long id, bool IsArchived)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check in DB
            var project = _unitOfWork.Projects.GetById(id);
            if (project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Task Manger Project with this Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {

                var tasks = _unitOfWork.Tasks.FindAll(a => a.ProjectId == id);

                foreach (var task in tasks)
                {
                    task.IsArchived = IsArchived;
                }
                project.IsArchived = IsArchived;

                _unitOfWork.Complete();


                response.ID = id;
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

        public BaseResponseWithId<long> DeleteProject([FromHeader] long ProjectId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {

                if (Response.Result)
                {
                    if (ProjectId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "ProjectId Is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var project = _unitOfWork.Projects.FindAll(a => a.Id == ProjectId, includes: new[] { "ProjectSprints", "Tasks", "WorkingHourseTrackings" }).FirstOrDefault();
                    if (project == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "Project not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var tasks = _unitOfWork.Tasks.FindAll(a => a.ProjectId == project.Id).ToList();
                    if (tasks.Count > 0)
                    {
                        for (int i = 0; i < tasks.Count; i++)
                        {
                            var task = tasks[i];
                            _taskService.DeleteTask(task.Id);
                        }
                    }
                    var sprints = _unitOfWork.ProjectSprints.FindAll(a => a.ProjectId == project.Id).ToList();
                    if (sprints.Count > 0)
                    {
                        for (int i = 0; i < sprints.Count; i++)
                        {
                            var sprint = sprints[i];
                            _sprintService.DeleteProjectSprint(sprint.Id);
                        }
                    }
                    var workinghours = _unitOfWork.WorkingHoursTrackings.FindAll(a => a.ProjectId == project.Id).ToList();
                    if (workinghours.Count > 0)
                    {
                        for (int i = 0; i < workinghours.Count; i++)
                        {
                            var workinghour = workinghours[i];
                            workinghour.ProjectId = null;
                            //_Context.WorkingHourseTrackings.Update(workinghour);
                        }
                        _unitOfWork.Complete();
                    }
                    //var projectdb = _Context.Projects.Where(a => a.Id == ProjectId).FirstOrDefault();
                    _unitOfWork.Projects.Delete(project);
                    _unitOfWork.Complete();
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

        //-----------------------------------Excell APIs------------------------------------------------------------
        public BaseResponseWithData<string> GetProjectsListReportExcell(int? BranchId, long? ProjectClientID, string CompName)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region validation
            if (BranchId != null)
            {
                var branch = _unitOfWork.Branches.GetById(BranchId??0);
                if(branch == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Branch ID is not found";
                    response.Errors.Add(error);
                    return response;
                }

            }

            if (ProjectClientID != null)
            {
                var branch = _unitOfWork.Clients.GetById(ProjectClientID ?? 0);
                if (branch == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Client ID is not found";
                    response.Errors.Add(error);
                    return response;
                }

            }
            #endregion

            try
            {

                var projectsQueryable = _unitOfWork.Projects.FindAllQueryable(a => true, new[] { "SalesOffer", "ProjectAssignUsers", "Tasks", "WorkingHourseTrackings", "Tasks.TaskExpensis", "ProjectInvoices", "CreatedByNavigation" } );
                
                if (BranchId != null)
                {
                    projectsQueryable = projectsQueryable.Where(a => a.BranchId == BranchId);
                }
                if(ProjectClientID != null)
                {
                    projectsQueryable = projectsQueryable.Where(a => a.SalesOffer.ClientId == ProjectClientID);
                }
                var projectsList = projectsQueryable.ToList();
                ExcelPackage excel = new ExcelPackage();

                var sheet = excel.Workbook.Worksheets.Add($"ProjectsListReport");

                sheet.Cells[1, 1].Value = "ProjectName";
                sheet.Cells[1, 2].Value = "StartDate";
                sheet.Cells[1, 3].Value = "EndDate";
                sheet.Cells[1, 4].Value = "CreatedBy";
                sheet.Cells[1, 5].Value = "Tasks Count";
                sheet.Cells[1, 6].Value = "totalProgress";
                sheet.Cells[1, 7].Value = "totalWorkkingHours";
                sheet.Cells[1, 8].Value = "Budget";
                sheet.Cells[1, 9].Value = "Total Expensis";
                sheet.Cells[1, 10].Value = "Total Invoices";
                sheet.Cells[1, 11].Value = "Total Collected";

                //---------------------------------------styling---------------------------------------------
                sheet.DefaultRowHeight = 12;
                sheet.Row(1).Height = 20;
                sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(1).Style.Font.Bold = true;
                sheet.Cells[1, 1, 1, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, 11].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 174, 81));
                sheet.Cells[1, 1, 1, 11].Style.Font.Color.SetColor(Color.White);
                

                int rowCount = 2;
                foreach (var project in projectsList)
                {
                    decimal countOFprogress = project.WorkingHourseTrackings.Count();
                    var totSumOfProgress = project.WorkingHourseTrackings.Sum(a => a.ProgressRate);
                    decimal totalProgress = 0.0m;
                    if (countOFprogress != 0 && totSumOfProgress != 0)
                    {
                        totalProgress = (totSumOfProgress??0) / countOFprogress;

                    }

                    var totalWorkkingHours = project.WorkingHourseTrackings.Sum(a => a.TotalHours);

                    //var test = project.Tasks.SelectMany(a => a.TaskExpensis.Select(b => b.Amount)).Sum();

                    sheet.Cells[rowCount, 1].Value = project.SalesOffer.ProjectName;
                    sheet.Cells[rowCount, 2].Value = project.StartDate.ToString();
                    sheet.Cells[rowCount, 3].Value = project.EndDate.ToString();
                    sheet.Cells[rowCount, 4].Value = project?.CreatedByNavigation?.FirstName + " " + project?.CreatedByNavigation?.LastName;
                    sheet.Cells[rowCount, 5].Value = project.Tasks.Where(a => a.ProjectId == project.Id).Count();
                    sheet.Cells[rowCount, 6].Value = Math.Round(totalProgress, 4);
                    sheet.Cells[rowCount, 7].Value = totalWorkkingHours;
                    sheet.Cells[rowCount, 8].Value = project.Budget;
                    sheet.Cells[rowCount, 9].Value = project.Tasks.SelectMany(a => a.TaskExpensis.Select(b => b.Amount)).Sum();
                    sheet.Cells[rowCount, 10].Value = project.ProjectInvoices.Sum(a => a.Amount);
                    sheet.Cells[rowCount, 11].Value = project.ProjectInvoices.Sum(a => a.Collected);

                    rowCount++;
                }

                for (int i = 1; i <= 11; i++)
                {
                    sheet.Column(i).AutoFit();
                }
                for (int i = 1; i <= projectsList.Count()+1; i++)
                {
                    sheet.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                var path = $"Attachments\\{CompName}\\ProjectsListReport";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var currentDate = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\ProjectsListReport_{currentDate}.xlsx";

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
                var fullPath = Globals.baseURL + "\\" + path + $"\\ProjectsListReport_{currentDate}.xlsx";

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

        public BaseResponseWithData<string> GetProjectsGeneralReportExcell(long? ProjectId, string DateFrom, string DateTo, string CompName)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region validation
            if (ProjectId != null)
            {
                var branch = _unitOfWork.Projects.GetById(ProjectId ?? 0);
                if (branch == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Project with this ID is not found";
                    response.Errors.Add(error);
                    return response;
                }

            }

            #endregion

            #region Date Validation
            DateTime startDate = DateTime.Now;
            if (!string.IsNullOrEmpty(DateFrom))
            {
                if (!DateTime.TryParse(DateFrom, out startDate))
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
            if (!string.IsNullOrEmpty(DateTo))
            {
                if (!DateTime.TryParse(DateTo, out endDate))
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

                var projectsQueryable = _unitOfWork.Projects.FindAllQueryable(a => true, new[] { "SalesOffer", "ProjectAssignUsers", "Tasks", "WorkingHourseTrackings", "Tasks.TaskExpensis", "ProjectInvoices", "CreatedByNavigation" , "Tasks.TaskDetails" });

                if (ProjectId != null)
                {
                    projectsQueryable = projectsQueryable.Where(a => a.Id == ProjectId);
                }
                if (!string.IsNullOrEmpty(DateFrom))
                {
                    projectsQueryable = projectsQueryable.Where(a => a.CreationDate.Date >= startDate);
                }
                if (!string.IsNullOrEmpty(DateTo))
                {
                    projectsQueryable = projectsQueryable.Where(a => a.CreationDate.Date <= endDate);
                }
                var projectsList = projectsQueryable.ToList();
                ExcelPackage excel = new ExcelPackage();

                var sheet = excel.Workbook.Worksheets.Add($"ProjectsGeneralReport");

                sheet.Cells[1, 1].Value = "ProjectName";
                sheet.Cells[1, 2].Value = "Project Budget";
                sheet.Cells[1, 3].Value = "Tasks Budget";
                sheet.Cells[1, 4].Value = "Expenses";
                sheet.Cells[1, 5].Value = "Left in budget";
                sheet.Cells[1, 6].Value = "Total Expenses";
                sheet.Cells[1, 7].Value = "Working hours";
                sheet.Cells[1, 8].Value = "Direct Expenses";
                sheet.Cells[1, 9].Value = "Unit rate service";
                sheet.Cells[1, 10].Value = "Total Invoices";
                sheet.Cells[1, 11].Value = "Remaining to be Invoiced";
                sheet.Cells[1, 12].Value = "Collected Invoiced";
                sheet.Cells[1, 13].Value = "Remaining to be collected";

                //---------------------------------------styling---------------------------------------------
                sheet.DefaultRowHeight = 12;
                sheet.Row(1).Height = 20;
                sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(1).Style.Font.Bold = true;
                sheet.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 174, 81));
                sheet.Cells[1, 1, 1, 13].Style.Font.Color.SetColor(Color.White);


                int rowCount = 2;
                foreach (var project in projectsList)
                {
                    decimal countOFprogress = project.WorkingHourseTrackings.Count();
                    var totSumOfProgress = project.WorkingHourseTrackings.Sum(a => a.ProgressRate);
                    decimal totalProgress = 0.0m;
                    if (countOFprogress != 0 && totSumOfProgress != 0)
                    {
                        totalProgress = totSumOfProgress ?? 0 / countOFprogress;

                    }

                    var totalWorkkingHours = project.WorkingHourseTrackings.Sum(a => a.TotalHours);

                    //var test = project.Tasks.SelectMany(a => a.TaskExpensis.Select(b => b.Amount)).Sum();

                    var taskExpensislist = project.Tasks.SelectMany(a => a.TaskExpensis);
                    if (!string.IsNullOrEmpty(DateFrom))
                    {
                        taskExpensislist = taskExpensislist.Where(a => a.CreationDate.Date >= startDate);
                    }
                    if (!string.IsNullOrEmpty(DateTo))
                    {
                        taskExpensislist = taskExpensislist.Where(a => a.CreationDate.Date <= endDate);
                    }

                    var taskExpensis = taskExpensislist.Sum(a => a.Amount);

                    decimal totalWorkinHoursExpensis = 0;
                    decimal totalWorkingHours = 0;

                    var WorkingHourselist = project.WorkingHourseTrackings.ToList();
                    if (!string.IsNullOrEmpty(DateFrom))
                    {
                        WorkingHourselist = WorkingHourselist.Where(a => a.CreationDate.Date >= startDate).ToList();
                    }
                    if (!string.IsNullOrEmpty(DateTo))
                    {
                        WorkingHourselist = WorkingHourselist.Where(a => a.CreationDate.Date <= endDate).ToList();
                    }

                    foreach (var workingHourRec in WorkingHourselist)
                    {
                        var recExpensis = workingHourRec.TotalHours * workingHourRec.WorkingHourRate;
                        totalWorkinHoursExpensis += recExpensis;

                        totalWorkingHours += workingHourRec.TotalHours;
                    }
                    decimal totalUnitRateService = 0;
                    if (project.UnitRateService == true)
                    {
                        totalUnitRateService = project.Tasks.SelectMany(a => a.TaskUnitRateServices.Select(b => b.Total)).Sum();
                    }

                    var totalInvoices = project.ProjectInvoices.Sum(a => a.Amount);
                    var totalCollected = project.ProjectInvoices.Sum(a => a.Collected);

                    sheet.Cells[rowCount, 1].Value = project.SalesOffer.ProjectName;
                    sheet.Cells[rowCount, 2].Value = project.Budget;
                    sheet.Cells[rowCount, 3].Value = project.Tasks.SelectMany(a => a.TaskDetails.Select(b => b.ProjectBudget)).Sum();
                    sheet.Cells[rowCount, 4].Value = taskExpensis + totalWorkinHoursExpensis + totalUnitRateService;
                    sheet.Cells[rowCount, 5].Value = project.Budget - (taskExpensis + totalWorkinHoursExpensis + totalUnitRateService);
                    sheet.Cells[rowCount, 6].Value = taskExpensis + totalWorkinHoursExpensis + totalUnitRateService;       //total expensis
                    sheet.Cells[rowCount, 7].Value = totalWorkingHours;
                    sheet.Cells[rowCount, 8].Value = taskExpensis;
                    
                    sheet.Cells[rowCount, 9].Value = totalUnitRateService;
                    sheet.Cells[rowCount, 10].Value = totalInvoices;
                    sheet.Cells[rowCount, 11].Value = (taskExpensis + totalWorkinHoursExpensis + totalUnitRateService) - totalInvoices;
                    sheet.Cells[rowCount, 12].Value = totalCollected;
                    sheet.Cells[rowCount, 13].Value = totalInvoices - totalCollected;
                    rowCount++;
                }

                for (int i = 1; i <= 13; i++)
                {
                    sheet.Column(i).AutoFit();
                }
                for (int i = 1; i <= projectsList.Count() + 1; i++)
                {
                    sheet.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                var path = $"Attachments\\{CompName}\\ProjectsGeneralReport";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var currentDate = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\ProjectsGeneralReport_{currentDate}.xlsx";

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
                var fullPath = Globals.baseURL + "\\" + path + $"\\ProjectsGeneralReport_{currentDate}.xlsx";

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
    }
}
