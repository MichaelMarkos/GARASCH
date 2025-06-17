using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.JobTitle;
using NewGaras.Domain.Models;
using NewGaras.Domain.DTO.HrUser;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGarasAPI.Models.HR;

namespace NewGarasAPI.Controllers.HrControllers
{
    [Route("[controller]")]
    [ApiController]
    public class JobTitleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        static string key;
        private GarasTestContext _Context;
        private readonly IMapper _mapper;
        private readonly ILogService _logService;
        private readonly IJobTitleService _jobTitleService;
        private readonly IHrUserService _hrUserService;
        private readonly ITenantService _tenantService;
        private readonly IAdminService _adminService;
        public JobTitleController(IUnitOfWork unitOfWork, IWebHostEnvironment host, IMapper mapper, ILogService logService, IJobTitleService jobTitleService,IHrUserService hrUserService,ITenantService tenantService, IAdminService adminService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _logService = logService;
            _jobTitleService = jobTitleService;
            _hrUserService = hrUserService;
            _adminService = adminService;
        }

        [HttpGet("GetJobTitleById")]
        public async Task<BaseResponseWithData<GetJobTilteDto>> GetJobTitleById([FromHeader]long JobTitleId)
        {
            var response = new BaseResponseWithData<GetJobTilteDto>();
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
                    var data = await _jobTitleService.GetById(JobTitleId);
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

        [HttpGet("GetAllJobtitles")]
        public async Task<BaseResponseWithData<List<GetAllJobTilteDto>>> GetAllJobtitles()
        {
            var response = new BaseResponseWithData<List<GetAllJobTilteDto>>();
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
                    response = await _jobTitleService.GetAll();

                    var HrUsers = await _hrUserService.GetAllUsersWithJobTitle();
                    foreach(var JobTitle in response.Data)
                    {
                        JobTitle.TotalHrUserNumber = HrUsers.Data.Where(a => a.JobTitleId == JobTitle.Id).Count();
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

        [HttpPost("AddJobTitle")]
        public async Task<BaseResponseWithId<long>> AddJobTitle([FromForm]AddJobTitleDto NewJobTitle)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
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
                    response = await _jobTitleService.AddJobTilte(NewJobTitle, validation.userID);
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

        [HttpPost("EditJobTitle")]
        public async Task<BaseResponseWithId<long>> EditJobTitle([FromForm] EditJobTitleDto NewJobTitle)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
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
                    response = await _jobTitleService.EditJobTitle(NewJobTitle, validation.userID);
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

        [HttpPost("DeleteJobTitle")]
        public BaseResponseWithId<long> DeleteJobTitle([FromHeader]long JobTitleId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
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
                    response = _jobTitleService.DeleteJobTitle(JobTitleId);
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

        [HttpPost("ArchiveJobTitle")]
        public BaseResponseWithId<long> ArchiveJobTitle([FromHeader]long JobTitleId, [FromHeader]bool Archive)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
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
                    response = _jobTitleService.ArchiveJobTitle(JobTitleId,Archive,validation.userID);
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

        [HttpGet("GetJobTitle")]
        public GetJobTitleResponse GetJobTitle()
        {
            GetJobTitleResponse response = new GetJobTitleResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {


                    response = _adminService.GetJobTitle();



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

    }
}
