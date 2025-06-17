using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.BranchSetting;
using NewGaras.Infrastructure.DTO.OverTimeAndDeductionRate;
using NewGaras.Infrastructure.Helper.TenantService;

namespace NewGarasAPI.Controllers.HR
{
    [Route("HR/[controller]")]
    [ApiController]
    public class OverTimeAndDeductionRateController : ControllerBase
    {
        private readonly IOverTimeAndDeductionRateService _OAndDService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public OverTimeAndDeductionRateController(IOverTimeAndDeductionRateService OAndDService, ITenantService tenantService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _OAndDService = OAndDService;
            _helper = new Helper.Helper();
        }
        [HttpPost("AddOverTimeAndDeductionRate")]
        public BaseResponseWithId<long> AddOverTimeAndDeductionRate([FromForm] OverTimeDeductionRateDto dto)
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
                    Response = _OAndDService.AddOverTimeAndDeductionRate(dto, validation.userID);
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
        [HttpPost("UpdateOverTimeAndDeductionRate")]
        public BaseResponseWithId<long> UpdateOverTimeAndDeductionRate([FromForm] OverTimeDeductionRateDto dto)
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
                    Response = _OAndDService.UpdateOverTimeAndDeductionRate(dto, validation.userID);
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

        [HttpPost("DeleteOverTimeAndDeductionRate")]
        public BaseResponseWithId<long> DeleteOverTimeAndDeductionRate([FromHeader] long RateId)
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
                    Response = _OAndDService.DeleteOverTimeAndDeductionRate(RateId);
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

        [HttpGet("GetOverTimeAndDeductionRates")]
        public BaseResponseWithData<List<OverTimeDeductionRateDto>> GetOverTimeAndDeductionRates([FromHeader] int branchId)
        {
            BaseResponseWithData<List<OverTimeDeductionRateDto>> Response = new BaseResponseWithData<List<OverTimeDeductionRateDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _OAndDService.GetOverTimeAndDeductionRate(branchId);
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
