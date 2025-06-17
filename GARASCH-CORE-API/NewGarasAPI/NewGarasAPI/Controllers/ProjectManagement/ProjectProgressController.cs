using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services;
using NewGaras.Domain.Services.ProjectManagment;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.ProjectManagment;
using NewGaras.Infrastructure.Models.ProjectManagement;

namespace NewGarasAPI.Controllers.ProjectManagement
{
    [Route("[controller]")]
    [ApiController]
    public class ProjectProgressController : ControllerBase
    {
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IProjectProgressService _projectProgressService;
        public ProjectProgressController(IMapper mapper, IWebHostEnvironment host,ITenantService tenantService,IProjectProgressService projectProgressService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            _mapper = mapper;
            _host = host;
            _projectProgressService = projectProgressService;
        }
        [HttpPost("AddNewProgressType")]
        public BaseResponseWithId<int> AddNewProgressType([FromBody] ProgressTypeDto dto)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectProgressService.AddNewProgressType(dto, validation.userID);
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

        [HttpPost("UpdateProgressType")]
        public BaseResponseWithId<int> UpdateProgressType([FromBody] ProgressTypeDto dto)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectProgressService.UpdateProgressType(dto, validation.userID); 
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

        [HttpGet("GetProgressTypeById")]
        public BaseResponseWithData<ProgressTypeDto> GetProgressTypeById([FromHeader] int progressId)
        {
            BaseResponseWithData<ProgressTypeDto> Response = new BaseResponseWithData<ProgressTypeDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                   Response = _projectProgressService.GetProgressTypeById(progressId);

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

        [HttpGet("GetProgressTypeList")]
        public BaseResponseWithData<List<ProgressTypeDto>> GetProgressTypeList()
        {
            BaseResponseWithData<List<ProgressTypeDto>> Response = new BaseResponseWithData<List<ProgressTypeDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectProgressService.GetProgressTypeList();

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

        [HttpGet("GetDeliveryTypeList")]
        public BaseResponseWithData<List<DeliveryTypeDto>> GetDeliveryTypeList()
        {
            BaseResponseWithData<List<DeliveryTypeDto>> Response = new BaseResponseWithData<List<DeliveryTypeDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                   Response = _projectProgressService.GetDeliveryTypeList();

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

        [HttpGet("GetProgressStatusList")]
        public BaseResponseWithData<List<ProgressStatusDto>> GetProgressStatusList()
        {
            BaseResponseWithData<List<ProgressStatusDto>> Response = new BaseResponseWithData<List<ProgressStatusDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                   Response = _projectProgressService.GetProgressStatusList();

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

        [HttpPost("AddNew")]
        public BaseResponseWithId<long> AddNewProjectProgress([FromForm] ProjectProgressDto dto)
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
                    _projectProgressService.Validation = validation;
                    Response = _projectProgressService.AddNewProjectProgress(dto,validation.userID,validation.CompanyName);
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

        [HttpPost("Update")]
        public BaseResponseWithId<long> UpdateProjectProgress([FromForm] ProjectProgressDto dto)
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
                    _projectProgressService.Validation = validation;
                    Response = _projectProgressService.UpdateProjectProgress(dto, validation.userID, validation.CompanyName);
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

        [HttpGet("GetById")]
        public BaseResponseWithData<GetProjectProgressDto> GetProjectProgressById([FromHeader] long ProjectProgressId)
        {
            BaseResponseWithData<GetProjectProgressDto> Response = new BaseResponseWithData<GetProjectProgressDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectProgressService.GetProjectProgressById(ProjectProgressId);
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

        [HttpGet("GetAll")]
        public BaseResponseWithData<List<GetProjectProgressDto>> GetProjectProgressList([FromHeader]long projectId)
        {
            BaseResponseWithData<List<GetProjectProgressDto>> Response = new BaseResponseWithData<List<GetProjectProgressDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            var list = new List<GetProjectProgressDto>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                #region validation
                var project = _Context.Projects.Where(a => a.Id == projectId).FirstOrDefault();
                if(project == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Project with this Id";
                    Response.Errors.Add(error);
                    return Response;
                }
                #endregion

                if (Response.Result)
                {
                    Response = _projectProgressService.GetProjectProgressList(projectId);
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

        [HttpPost("AddProgressTypeForProject")]
        public BaseResponseWithId<long> AddProgressTypeForProject([FromHeader] long ProjectId)
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
                    Response = _projectProgressService.AddProgressTypeForProject(ProjectId);
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
