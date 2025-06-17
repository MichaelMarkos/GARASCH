using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.DTO.Salary;
using NewGaras.Domain.Interfaces.ServicesInterfaces;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Contract;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;

namespace NewGarasAPI.Controllers.HR
{
    [Route("HR/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public ContractController(IContractService contractService,ITenantService tenantService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _contractService = contractService;
            _helper = new Helper.Helper();
        }
        [HttpPost("AddContract")]
        public BaseResponseWithId<long> AddContract([FromForm] AddContractDto contractDto)
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
                    response = _contractService.AddContract(contractDto, validation.userID);
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

        [HttpPost("EditContract")]
        public BaseResponseWithId<long> EditContract([FromForm] AddContractDto contractDto)
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

                    response = _contractService.EditContract(contractDto, validation.userID);
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
        [HttpGet("GetContract")]
        public BaseResponseWithData<GetContractDto> GetContract([FromHeader] long HrUserId)
        {
            BaseResponseWithData<GetContractDto> response = new BaseResponseWithData<GetContractDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _contractService.GetContract(HrUserId);

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
