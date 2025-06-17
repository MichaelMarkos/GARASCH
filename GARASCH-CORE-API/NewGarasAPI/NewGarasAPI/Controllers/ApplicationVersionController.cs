using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Client.Filters;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApplicationVersionController : ControllerBase
    {
        private readonly IApplicationVersionService _applicationVersion;
        private Helper.Helper _helper;
        private readonly ITenantService _tenantService;
        private GarasTestContext _Context;
        public ApplicationVersionController(IApplicationVersionService applicationVersion, ITenantService tenantService)
        {
            _applicationVersion = applicationVersion;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
        }

        [HttpGet("GetApplicationVersion")]
        public BaseResponseWithData<GetAppsVersionModel> GetApplicationVersion([FromHeader] string CompanyName)
        {
            BaseResponseWithData<GetAppsVersionModel> Response = new BaseResponseWithData<GetAppsVersionModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    Response = _applicationVersion.GetApplicationVersion(CompanyName);
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

        [HttpPost("UpdateApplicationVersion")]
        public BaseResponse UpdateApplicationVersion(GetAppsVersionModel newVersion)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                #region Validation
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                #endregion
                if (Response.Result)
                {
                    _applicationVersion.Validation = validation;
                    Response = _applicationVersion.UpdateApplicationVersion(newVersion);
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
