using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.BYCompany;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany;

namespace NewGarasAPI.Controllers.BYCompany
{
    [Route("[controller]")]
    [ApiController]
    public class InsuranceCompanyNamesController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly ITenantService _tenantService;
        private readonly IInsuranceCompanyNamesService _companyNamesService;

        public InsuranceCompanyNamesController(IWebHostEnvironment host, IMapper mapper,ITenantService tenantService,IInsuranceCompanyNamesService companyNamesService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            _companyNamesService = companyNamesService;
        }
        [HttpGet("GetInsuranceCompanies")]
        public BaseResponseWithData<List<InsuranceCompanyNameDto>> GetInsuranceCompanies()
        {
            BaseResponseWithData<List<InsuranceCompanyNameDto>> Response = new BaseResponseWithData<List<InsuranceCompanyNameDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _companyNamesService.GetInsuranceCompanies();
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
        [HttpPost("AddInsuranceCompany")]
        public BaseResponseWithId<long> AddInsuranceCompany(InsuranceCompanyNameDto dto)
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
                    Response = _companyNamesService.AddInsuranceCompany(dto, validation.userID);
                }
                
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
    }
}
