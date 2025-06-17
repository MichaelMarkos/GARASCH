using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.BYCompany;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Models.HR;

namespace NewGarasAPI.Controllers.PatientForBY
{
    [Route("[controller]")]
    [ApiController]
    public class PatientController : Controller
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IPatientService _patientService;
        public PatientController(IWebHostEnvironment host,ITenantService tenantService, IPatientService patientService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            key = "SalesGarasPass";
            _helper = new Helper.Helper();
            _host = host;
            _patientService = patientService;
        }


        [HttpPost("AddNewPatient")]
        public ActionResult<BaseResponseWithID> AddNewPatient([FromForm] PatientDto request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _patientService.AddNewPatient(request, validation.userID, validation.CompanyName);
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

        [HttpPost("EditPatient")]
        public ActionResult<BaseResponseWithID> EditPatient([FromForm] PatientDto request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _patientService.EditPatient(request, validation.userID, validation.CompanyName);
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

        [HttpGet("GetPatients")]
        public ActionResult<BaseResponseWithDataAndHeader<List<GetPatientDto>>> GetPatients([FromHeader] string firstname, [FromHeader] string lastname, [FromHeader] DateTime? DOB, [FromHeader] string phone, [FromHeader] string IncuranceNo, [FromHeader] bool GetAll, [FromHeader] int CurrentPage=1, [FromHeader]int PageSize = 1000)
        {
            BaseResponseWithDataAndHeader<List<GetPatientDto>> Response = new BaseResponseWithDataAndHeader<List<GetPatientDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _patientService.GetPatients(firstname, lastname, DOB, phone, IncuranceNo, GetAll, CurrentPage, PageSize);
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

        [HttpGet("GetPatientById")]
        public ActionResult<BaseResponseWithData<GetPatientDetailsDto>> GetPatientById([FromHeader] long patientId)
        {
            BaseResponseWithData<GetPatientDetailsDto> Response = new BaseResponseWithData<GetPatientDetailsDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _patientService.GetPatientById(patientId);
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
        [HttpGet("GetUserInsurances")]
        public ActionResult<BaseResponseWithData<List<InsuranceDto>>> GetUserInsurances([FromHeader] long patientId)
        {
            BaseResponseWithData<List<InsuranceDto>> Response = new BaseResponseWithData<List<InsuranceDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                var ins = new List<InsuranceDto>();
                if (Response.Result)
                {
                   Response = _patientService.GetUserInsurances(patientId);
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

        [HttpGet("GetUserPatients")]
        public ActionResult<BaseResponseWithData<List<GetPatientsDDl>>> GetUserPatients()
        {
            BaseResponseWithData<List<GetPatientsDDl>> Response = new BaseResponseWithData<List<GetPatientsDDl>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                var pat = new List<GetPatientsDDl>();
                if (Response.Result)
                {
                    Response = _patientService.GetUserPatients();
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
