using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.PoInvoice;
using NewGaras.Infrastructure.Models.PurchesRequest.Filters;
using NewGaras.Infrastructure.Models.PurchesRequest.Responses;
using NewGarasAPI.Models.Account;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PoInvoiceController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        static string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IPoInvoiceService _poInvoiceService;

        public PoInvoiceController(IWebHostEnvironment host, ITenantService tenantService, IPoInvoiceService poInvoiceService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _host = host;
            _poInvoiceService = poInvoiceService;
        }



        [HttpGet("GetPoInvoiceData")]
        public async Task<GetPoInvoiceDataResponse> GetPoInvoiceData([FromHeader]long POID)
        {

            GetPoInvoiceDataResponse response = new GetPoInvoiceDataResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data =await _poInvoiceService.GetPoInvoiceData(POID);
                if (!response.Result)
                {
                    response.Result = false;
                    response.Errors.AddRange(data.Errors);
                    return response;
                }
                response = data;
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

        [HttpPost("AddEditPoInvoice")]
        public async Task<BaseResponseWithID> AddEditPoInvoice([FromBody]AddEditPoInvoice RequestData)
        {

            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data = await _poInvoiceService.AddEditPoInvoice(RequestData, validation.userID);
                if (!response.Result)
                {
                    response.Result = false;
                    response.Errors.AddRange(data.Errors);
                    return response;
                }
                response = data;
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

        [HttpPost("AddDirectPoInvoice")]
        public async Task<BaseResponseWithID> AddDirectPoInvoice([FromBody]AddDirectPoInvoiceRequest RequestData)
        {

            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data = await _poInvoiceService.AddDirectPoInvoice(RequestData, validation.userID);
                if (!response.Result)
                {
                    response.Result = false;
                    response.Errors.AddRange(data.Errors);
                    return response;
                }
                response = data;
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

        [HttpPost("AddNewPurchasePOExtraaFees")]
        public async Task<BaseResponseWithID> AddNewPurchasePOExtraaFees([FromBody]AddNewPurchasePOInvoiceExtraFeesRequest RequestData)
        {

            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data = await _poInvoiceService.AddNewPurchasePOExtraaFees(RequestData, validation.userID);
                if (!response.Result)
                {
                    response.Result = false;
                    response.Errors.AddRange(data.Errors);
                    return response;
                }
                response = data;
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


        [HttpGet("PurchasePOInvoicePDF")]
        public async Task<BaseMessageResponse> PurchasePOInvoicePDF([FromHeader] long POID, [FromHeader] bool? GeneratePDF)
        {
            BaseMessageResponse response = new BaseMessageResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _poInvoiceService.PurchasePOInvoicePDF(validation.CompanyName,POID, GeneratePDF);
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

        [HttpGet("GetInvoiceDetailsPDF")]
        public async Task<BaseMessageResponse> GetInvoiceDetailsPDF([FromHeader] long SalesOfferId)
        {
            BaseMessageResponse response = new BaseMessageResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _poInvoiceService.Validation = validation;
                    response = await _poInvoiceService.GetInvoiceDetailsPDF(SalesOfferId);
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
