using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.DTO.Salary;
using NewGaras.Domain.Interfaces.ServicesInterfaces;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;

namespace NewGarasAPI.Controllers.HR
{
    [Route("HR/[controller]")]
    [ApiController]
    public class WeekDayController : ControllerBase
    {
        private readonly IWeekDayService _weekDayService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public WeekDayController(ISalaryService salaryService, IWeekDayService weekDayService, ITenantService tenantService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _weekDayService = weekDayService;
            _helper = new Helper.Helper();
        }
        [HttpGet("GetWeekDays")]
        public BaseResponseWithData<List<WeekDayDto>> GetWeekDays([FromHeader] int BranchId)
        {
            BaseResponseWithData<List<WeekDayDto>> response = new BaseResponseWithData<List<WeekDayDto>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)

                {
                    response = _weekDayService.GetWeekDays(BranchId);

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

        [HttpPost("UpdateWeekDays")]
        public BaseResponse UpdateWeekDays(UpdateWeekDaysModel Dto)
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
                    response = _weekDayService.UpdateWeekDays(Dto);

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
