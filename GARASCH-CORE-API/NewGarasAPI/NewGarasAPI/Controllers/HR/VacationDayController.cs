using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.VacationDay;
using NewGaras.Infrastructure.Helper.TenantService;

namespace NewGarasAPI.Controllers.HR
{
    [Route("HR/[controller]")]
    [ApiController]
    public class VacationDayController : ControllerBase
    {
        private readonly IVacationDayService _vacationDayService;
        private readonly IVacationOverTimeAndDeductionRate _vacationOverTimeAndDeductionRate;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public VacationDayController(IVacationDayService vacationDayService, IVacationOverTimeAndDeductionRate vacationOverTimeAndDeductionRate, ITenantService tenantService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _vacationDayService = vacationDayService;
            _vacationOverTimeAndDeductionRate = vacationOverTimeAndDeductionRate;
            _helper = new Helper.Helper();
        }


        [HttpPost("UpdateVacationDay")]
        public BaseResponseWithId<long> UpdateVacationDay([FromForm] AddVacationDayDto dto)
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
                    Response = _vacationDayService.UpdateVacationDay(dto, validation.userID);
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
