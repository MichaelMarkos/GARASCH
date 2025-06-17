using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Log;
using NewGaras.Infrastructure.DTO.VacationType;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Models.HR;
using System.Net;

namespace NewGarasAPI.Controllers.HR
{
    [Route("HR/[controller]")]
    [ApiController]
    public class VacationTypeController : ControllerBase
    {
        private readonly IVacationTypeService _vacationTypeService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public VacationTypeController(IVacationTypeService vacationTypeService, ITenantService tenantService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _vacationTypeService = vacationTypeService;
            _helper = new Helper.Helper();
        }
        [HttpPost("AddVacationType")]
        public BaseResponseWithId<int> AddVacationType([FromForm] ContractLeaveSettingDto dto)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vacationTypeService.AddVacationType(dto, validation.userID);
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

        [HttpPost("EditVacationType")]
        public BaseResponseWithId<int> EditVacationType([FromForm] ContractLeaveSettingDto dto)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vacationTypeService.EditVacationType(dto, validation.userID);
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

        [HttpGet("GetVacationTypesList")]
        public BaseResponseWithData<List<ContractLeaveSettingDto>> GetVacationTypesList()
        {
            BaseResponseWithData<List<ContractLeaveSettingDto>> response = new BaseResponseWithData<List<ContractLeaveSettingDto>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vacationTypeService.GetVacationTypesList();
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


        [HttpPost("EditVacationTypesForUser")]
        public BaseResponseWithData<List<long>> EditVacationTypesForUser([FromForm] List<VacationTypesForUser> dto)
        {
            BaseResponseWithData<List<long>> response = new BaseResponseWithData<List<long>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vacationTypeService.EditVacationTypesForUser(dto, validation.userID);
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

        [HttpGet("GetVacationTypesForUser")]
        public BaseResponseWithData<List<VacationTypesForUser>> GetVacationTypesForUser([FromHeader] long HrUserId)
        {
            BaseResponseWithData<List<VacationTypesForUser>> response = new BaseResponseWithData<List<VacationTypesForUser>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vacationTypeService.GetVacationTypesForUser(HrUserId);
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

        [HttpGet("GetDDLVacationTypesForUser")]
        public BaseResponseWithData<List<DDlVacationTypesForUser>> GetDDLVacationTypesForUser([FromHeader] long HrUserId)
        {
            BaseResponseWithData<List<DDlVacationTypesForUser>> response = new BaseResponseWithData<List<DDlVacationTypesForUser>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vacationTypeService.GetDDlVacationTypesForUser(HrUserId);
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

        [HttpGet("GetVacationType")]

        public BaseResponseWithData<ContractLeaveSettingDto> GetVacationType([FromHeader] int VacationTypeId)
        {
            BaseResponseWithData<ContractLeaveSettingDto> response = new BaseResponseWithData<ContractLeaveSettingDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vacationTypeService.GetVacationType(VacationTypeId);
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


        [HttpPost("DeleteLeaveType")]
        public BaseResponseWithId<int> DeleteLeaveType([FromHeader] int LeaveTypeId)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vacationTypeService.DeleteLeaveType(LeaveTypeId);
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

        [HttpPost("ArchiveLeaveType")]
        public BaseResponseWithId<int> ArchiveLeaveType([FromHeader] int LeaveTypeId, [FromHeader] bool Archive)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vacationTypeService.ArchiveLeaveType(LeaveTypeId, Archive, validation.userID);
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
