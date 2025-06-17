using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.DTO.Salary;
using NewGaras.Domain.Interfaces.ServicesInterfaces;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Shift;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;

namespace NewGarasAPI.Controllers
{
    [Route("HR/[controller]")]
    [ApiController]
    public class BranchScheduleController : ControllerBase
    {
        private readonly IShiftService _shiftService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public BranchScheduleController(IShiftService shiftService,ITenantService tenantService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _shiftService = shiftService;
            _helper = new Helper.Helper();
        }
        [HttpPost("AddShift")]
        public BaseResponseWithId<long> AddShift([FromForm] AddShiftDto shiftDto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _shiftService.AddShift(shiftDto, validation.userID);
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
        [HttpPost("UpdateShift")]
        public BaseResponseWithId<long> UpdateShift([FromForm] List<AddShiftDto> shiftDtos)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _shiftService.UpdateShift(shiftDtos, validation.userID);
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
        [HttpPost("AddListOfShifts")]
        public BaseResponseWithId AddListOfShifts([FromForm] AddListOfShiftsDto dto)
        {
            BaseResponseWithId response = new BaseResponseWithId();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = _shiftService.AddListOfShifts(dto, validation.userID);

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

        [HttpGet("GetShifts")]
        public async Task<BaseResponseWithData<List<GetBranchScheduls>>> GetShifts([FromHeader] int branchId)
        {
            BaseResponseWithData<List<GetBranchScheduls>> response = new BaseResponseWithData<List<GetBranchScheduls>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _shiftService.GetShifts(branchId);
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
        [HttpGet("GetShiftById")]
        public BaseResponseWithData<BranchScheduleDto> GetShiftById([FromHeader] long shiftId)
        {
            BaseResponseWithData<BranchScheduleDto> response = new BaseResponseWithData<BranchScheduleDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _shiftService.GetShift(shiftId);
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
