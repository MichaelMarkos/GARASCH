using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services;
using NewGaras.Infrastructure.DTO.WorkFlow;
using NewGaras.Infrastructure.DTO.JobTitle;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using NewGaras.Infrastructure.DTO.ProjectSprint;
using Azure;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics.Metrics;
using NewGarasAPI.Models.Project.UsedInResponses;
using NewGaras.Infrastructure.DTO.TaskMangerProject;
using NewGaras.Infrastructure.Helper.TenantService;

namespace NewGarasAPI.Controllers.TaskMangerProject
{
    [Route("[controller]")]
    [ApiController]
    public class TaskMangerProjectController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        static string key;
        private GarasTestContext _Context;
        private readonly IMapper _mapper;
        private readonly ISprintService _sprintService;
        private readonly IWorkFlowService _workFlowService;
        private readonly ITaskMangerProjectService _taskMangerProjectService;
        private readonly ITenantService _tenantService;
        public TaskMangerProjectController(IUnitOfWork unitOfWork, IWebHostEnvironment host, IMapper mapper,
            IHrUserService hrUserService, IMailService mailService, ISprintService sprintService, IWorkFlowService workFlowService,ITaskMangerProjectService taskMangerProjectService,ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _sprintService = sprintService;
            _workFlowService = workFlowService;
            _taskMangerProjectService = taskMangerProjectService;
        }

        [HttpPost("AddProjectWorkFlow")]
        public async Task<BaseResponseWithId<long>> AddProjectWorkFlow([FromForm]AddWorkFlowDto Dto)
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
            if (Dto.ProjectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "project ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            int counter = 0;
            foreach (var workFlow in Dto.WorkFlowList)
            {
                if (string.IsNullOrEmpty(workFlow.Name))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = $"Name Of project WorkFlow Is Required at workFlow number {counter}";
                    response.Errors.Add(error);
                    return response;
                }
                
                if (workFlow.orderNum == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = $"order Number Is Required at workFlow number {counter}";
                    response.Errors.Add(error);
                    return response;
                }
                counter++;
            }
            
            #endregion

            try
            {
                if (response.Result)
                {
                    response = await _workFlowService.AddWorkFlow(Dto, validation.userID);
                }
                //_logService.AddLog("Add New Employee", "HrUser", "", (int)response.ID, validation.userID);
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

        [HttpGet("GetProjectWorkFlowById")]
        public BaseResponseWithData<GetWorkFlowDto> GetProjectWorkFlowById([FromHeader]long Id)
        {
            var response = new BaseResponseWithData<GetWorkFlowDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if(Id == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please Enter a Work Flow ID";
                response.Errors.Add(err);
                return response;
            }

            try
            {
                if (response.Result)
                {
                    var data = _workFlowService.GetWorKFlowByID(Id);
                    if (data.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpPost("EditProjectWorkFlow")]
        public BaseResponseWithId<long> EditProjectWorkFlow([FromForm]EditWorkFlowDto Dto)
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
            if (Dto.ProjectID == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please Enter a project Sprint ID";
                response.Errors.Add(err);
                return response;
            }


            int counter = 0;
            foreach (var workFlow in Dto.WorkFlowList)
            {
                if (workFlow.Active == true)
                {
                    if (string.IsNullOrEmpty(workFlow.Name))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"please Enter a project sprint name at sprint number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                    if (workFlow.OrderNum == 0)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"please Enter a project sprint order Number at sprint number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }

                }

                counter++;

            }
            #endregion


            try
            {
                if (response.Result)
                {
                    var workFlow = _workFlowService.EditWorkFlow(Dto, validation.userID);
                    if (workFlow.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(workFlow.Errors);
                        return response;
                    }
                    response.ID = workFlow.ID;
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

        [HttpGet("GetProjectWorkFlowList")]
        public BaseResponseWithData<List<GetWorkFlowDto>> GetProjectWorkFlowList([FromHeader] long ProjectID)
        {
            var response = new BaseResponseWithData<List<GetWorkFlowDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if(ProjectID == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please Enter a project ID";
                response.Errors.Add(err);
                return response;
            }

            try
            {
                if (response.Result)
                {
                    var sprintList = _workFlowService.GetProjectWorkFlowList(ProjectID);
                    response.Data = sprintList.Data;
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


        [HttpPost("AddProjectSprint")]
        public async Task<BaseResponseWithId<long>> AddProjectSprint([FromForm] AddProjectSprintDto Dto)
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
            if (Dto.projectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "project ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            var validationCounter = 0;
            foreach (var sprint in Dto.sprintsList)
            {
                if (string.IsNullOrEmpty(sprint.name))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = $"Name Of project sprint Is Required at {validationCounter}";
                    response.Errors.Add(error);
                    return response;
                }
                
                if (sprint.orderNo == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = $"order Number Is Required at {validationCounter}";
                    response.Errors.Add(error);
                    return response;
                }
                if (string.IsNullOrEmpty(sprint.stratDate))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = $"start Date Is Required at {validationCounter}";
                    response.Errors.Add(error);
                    return response;
                }
                if (string.IsNullOrEmpty(sprint.EndDate))
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = $"end Date Is Required at {validationCounter}";
                    response.Errors.Add(error);
                    return response;
                }
                validationCounter++;
            }

            #endregion

            #region Date Validation
            int counter = 0;
            foreach (var sprint in Dto.sprintsList)
            {

                DateTime startDate = DateTime.Now;
                if (!DateTime.TryParse(sprint.stratDate, out startDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid start Date at sprint number {counter}";
                    response.Errors.Add(err);
                    return response;
                }
                DateTime endDate = DateTime.Now;
                if (!DateTime.TryParse(sprint.EndDate, out endDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid end Date at sprint number {counter}";
                    response.Errors.Add(err);
                    return response;
                }
                if (endDate < startDate)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"The End Date cannot be smaller than Start date at sprint number {counter}";
                    response.Errors.Add(err);
                    return response;
                }
                counter++;

            }

            #endregion

            try
            {
                if (response.Result)
                {
                    var sprint = await _sprintService.AddProjectSprint(Dto, validation.userID);
                    if (sprint.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(sprint.Errors);
                        return response;
                    }
                    response.ID = sprint.ID;
                }
                //_logService.AddLog("Add New Employee", "HrUser", "", (int)response.ID, validation.userID);
                
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

        [HttpGet("GetProjectSprintByID")]
        public BaseResponseWithData<GetProjectsprintDto> GetProjectSprintByID([FromHeader]long ID)
        {
            var response = new BaseResponseWithData<GetProjectsprintDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (ID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Name Of project Sprint Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var data = _sprintService.GetProjectSprintByID(ID);
                    if (data.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpGet("GetProjectSprintList")]
        public BaseResponseWithData<List<GetProjectsprintDto>> GetProjectSprintList([FromHeader]long ProjectID)
        {
            var response = new BaseResponseWithData<List<GetProjectsprintDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            if (ProjectID == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please Enter a project ID";
                response.Errors.Add(err);
                return response;
            }

            try
            {
                if (response.Result)
                {
                    var sprintList = _sprintService.GetProjectSprintList(ProjectID);
                    response.Data = sprintList.Data;
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

        [HttpPost("EditProjectSprint")]
        public BaseResponseWithId<long> EditProjectSprint([FromForm] EditProjectSprint sprintDto)
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
            if (sprintDto.ProjectId == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please Enter a project Sprint ID";
                response.Errors.Add(err);
                return response;
            }

            
            int counter = 0;
            foreach (var sprint in sprintDto.sprintsList)
            {
                if(sprint.Active == true)
                {
                    if (string.IsNullOrEmpty(sprint.name))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"please Enter a project sprint name at sprint number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                    if (sprint.orderNo == 0)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"please Enter a project sprint order Number at sprint number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }

                    DateTime startDate = DateTime.Now;
                    if (!DateTime.TryParse(sprint.stratDate, out startDate))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"please, Enter a valid start Date at sprint number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                    DateTime endDate = DateTime.Now;
                    if (!DateTime.TryParse(sprint.EndDate, out endDate))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"please, Enter a valid end Date at sprint number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                    if (endDate < startDate)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"The End Date cannot be smaller than Start date at sprint number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                }
                
                counter++;

            }
            #endregion


            try
            {
                if(response.Result)
                {
                    var sprints = _sprintService.EditProjectSprint(sprintDto, validation.userID);
                    if (sprints.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(sprints.Errors);
                        return response;
                    }
                    response.ID = sprints.ID;
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

        [HttpPost("UpdateTaskMangerProjectSettings")]
        public BaseResponseWithId<long> UpdateTaskMangerProjectSettings([FromBody]AddTaskMangerProjectSettingsDto Dto)
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
            if(Dto.ProjectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Project ID is required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var projectSettings = _taskMangerProjectService.AddProjectSettings(Dto,validation.userID);
                    if (projectSettings.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(projectSettings.Errors);
                        return response;
                    }
                    response.ID = projectSettings.ID;
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

        [HttpGet("GetTaskMangerProjectSettings")]
        public BaseResponseWithData<GetTaskMangerProjectSettingsDto> GetTaskMangerProjectSettings([FromHeader]long ProjectID)
        {
            var response = new BaseResponseWithData<GetTaskMangerProjectSettingsDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (ProjectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "project ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var data = _taskMangerProjectService.GetProjectsSettings(ProjectID);
                    if (data.Result == false)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        //-----------------------------Costing Type---------------------------------------------
        [HttpGet("GetProjectCostTypesDDL")]
        public BaseResponseWithData<List<CostTypeDDL>> GetProjectCostTypesDDL()
        {
            var response = new BaseResponseWithData<List<CostTypeDDL>>();
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
                    var costTypes = _taskMangerProjectService.ProjectCostTypes();
                    response.Data = costTypes.Data;
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

        //-----------------------------Billing Types-------------------------------------------
        [HttpGet("GetBillingTypesDDL")]
        public BaseResponseWithData<List<BillingTypeDDL>> GetBillingTypesDDL()
        {
            var response = new BaseResponseWithData<List<BillingTypeDDL>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if(response.Result)
                {
                    var billingTypes = _taskMangerProjectService.BillingTypes();
                    response.Data = billingTypes.Data;
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

        //-----------------------------Delete APIs----------------------------------------------
        [HttpPost("DeleteProjectSprint")]
        public BaseResponseWithId<long> DeleteProjectSprint([FromForm]long Id)
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
                error.ErrorMSG = "Project Sprint Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var taskExpensis = _sprintService.DeleteProjectSprint(Id);
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

        [HttpPost("DeleteProjectSettings")]
        public BaseResponseWithId<long> DeleteProjectSettings([FromForm]long Id)
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
                error.ErrorMSG = "Project ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var taskExpensis = _taskMangerProjectService.DeleteProjectSettings(Id);
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

        [HttpPost("DeleteUsersAssiginToProject")]
        public BaseResponseWithId<long> DeleteUsersAssiginToProject([FromForm]long Id)
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
                error.ErrorMSG = "Project ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var taskExpensis = _taskMangerProjectService.DeleteUsersAssiginToProject(Id);
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

        //-----------------------------Archived APIs--------------------------------------------
        [HttpPost("ArchiveProject")]
        public BaseResponseWithId<long> ArchiveProject([FromForm]long Id, [FromForm]bool IsArchived)
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
                error.ErrorMSG = "Project ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    var project = _taskMangerProjectService.ArchiveProject(Id, IsArchived);
                    if (!project.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(project.Errors);
                        return response;
                    }
                    response.ID = project.ID;
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

        //----------------------------Excell APIs------------------------------------------------

        [HttpGet("GetProjectsListReportExcell")]
        public BaseResponseWithData<string> GetProjectsListReportExcell([FromHeader]int? BranchId, [FromHeader]long? ProjectClientID)
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
                    var projectsList = _taskMangerProjectService.GetProjectsListReportExcell(BranchId, ProjectClientID, validation.CompanyName);
                    if (!projectsList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(projectsList.Errors);
                        return response;
                    }
                    response = projectsList;
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

        [HttpGet("GetProjectsGeneralReportExcell")]
        public BaseResponseWithData<string> GetProjectsGeneralReportExcell([FromHeader] long? ProjectId, [FromHeader] string DateFrom, [FromHeader] string DateTo)
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
                    var tasksList = _taskMangerProjectService.GetProjectsGeneralReportExcell(ProjectId, DateFrom, DateTo, validation.CompanyName);
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
    }
}
