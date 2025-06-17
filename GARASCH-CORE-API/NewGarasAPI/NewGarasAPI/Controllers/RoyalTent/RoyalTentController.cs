using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.RoyalTent;
using NewGaras.Infrastructure.Models.SalesOffer;

namespace NewGarasAPI.Controllers.RoyalTent
{
    [Route("[controller]")]
    [ApiController]
    public class RoyalTentController : ControllerBase
    {
        private readonly IRoyalTentService _royalTentService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public RoyalTentController(IRoyalTentService royalTentService, ITenantService tenantService)
        {
            _royalTentService = royalTentService;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
        }

        [HttpGet("RoyalTelesqupUmbrellaExcel")]
        public async Task<BaseMessageExcelResponse> RoyalTelesqupUmbrellaExcel(RoyalTelesqupUmbrellaFilters filters)
        {
            BaseMessageExcelResponse response = new BaseMessageExcelResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _royalTentService.Validation = validation;
                    response = await _royalTentService.RoyalTelesqupUmbrellaExcel(filters);

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


        [HttpGet("MainVariablesRoyalTentExcel")]
        public async Task<BaseMessageMainVariablesExcelResponse> MainVariablesRoyalTentExcel(MainVariablesRoyalTentFilters filters)
        {
            BaseMessageMainVariablesExcelResponse response = new BaseMessageMainVariablesExcelResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _royalTentService.Validation = validation;
                    response = await _royalTentService.MainVariablesRoyalTentExcel(filters);

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


        [HttpPost("UpdateRoyalTentExcel")]
        public BaseResponse UpdateRoyalTentExcel([FromForm]Stream stream)
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
                    _royalTentService.Validation = validation;
                    response = _royalTentService.UpdateRoyalTentExcel(stream);

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
    }
}
