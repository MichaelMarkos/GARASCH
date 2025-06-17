using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.OverTimeAndDeductionRate;
using NewGaras.Infrastructure.DTO.VacationOverTimeAndDeductionRates;
using NewGaras.Infrastructure.Helper.TenantService;

namespace NewGarasAPI.Controllers.HR
{
    [Route("HR/[controller]")]
    [ApiController]
    public class VacationOverTimeAndDeductionController : ControllerBase
    {
        private readonly IVacationOverTimeAndDeductionRate _VOAndDService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public VacationOverTimeAndDeductionController(IVacationOverTimeAndDeductionRate VOAndDService, ITenantService tenantService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _VOAndDService = VOAndDService;
            _helper = new Helper.Helper();
        }
        [HttpPost("AddVacationOverTimeAndDeductionRate")]
        public BaseResponseWithData<List<long>> AddVacationOverTimeAndDeductionRate([FromForm] List<VacationOverTimeDeductionRateDto> dtos)
        {
            BaseResponseWithData<List<long>> Response = new BaseResponseWithData<List<long>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _VOAndDService.AddVacationOverTimeAndDeductionRateList(dtos, validation.userID);
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

        [HttpPost("UpdateVacationOverTimeAndDeductionRate")]
        public BaseResponseWithId<long> UpdateOverTimeAndDeductionRate([FromForm] VacationOverTimeDeductionRateDto dto)
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
                    Response = _VOAndDService.UpdateVacationOverTimeAndDeductionRate(dto, validation.userID);
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

        [HttpGet("GetVacationOverTimeAndDeductionRates")]
        public BaseResponseWithData<List<VacationOverTimeDeductionRateDto>> GetVacationOverTimeAndDeductionRates([FromHeader] long VacationDayId)
        {
            BaseResponseWithData<List<VacationOverTimeDeductionRateDto>> Response = new BaseResponseWithData<List<VacationOverTimeDeductionRateDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _VOAndDService.GetVacationOverTimeAndDeductionRate(VacationDayId);
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

        [HttpPost("DeleteVacationOverTimeAndDeductionRate")]
        public BaseResponseWithId<long> DeleteVacationOverTimeAndDeductionRate([FromHeader] long RateId)
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
                    Response = _VOAndDService.DeleteVacationOverTimeAndDeductionRate(RateId);
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
