using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using Microsoft.Extensions.Hosting;
using NewGaras.Domain.Services;
using NewGaras.Domain.Models;
using Azure;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.SalesOffer.Filters;
using NewGarasAPI.Models.Account;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SalesOfferController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly ISalesOfferService _salesOfferService;
        private readonly IInternalTicketService _internalTicketService;

        public SalesOfferController(IUnitOfWork unitOfWork, IWebHostEnvironment host, ITenantService tenantService, IInventoryItemMatrialAddingAndExternalOrderService inventoryItemMatrialAddingAndExternalOrderService, IInventoryItemService inventoryItemService, ISalesOfferService salesOfferService, IInternalTicketService internalTicketService)
        {
            _host = host;
            _unitOfWork = unitOfWork;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _salesOfferService = salesOfferService;
            _internalTicketService = internalTicketService;
        }

        [HttpGet("GetSalesOfferDueClientPOS")]          //duplicate
        public BaseResponseWithData<string> GetSalesOfferDueClientPOS([FromHeader]string DateFrom, [FromHeader]string DateTo)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _salesOfferService.GetSalesOfferDueClientPOS(DateFrom, DateTo,validation.CompanyName);
                    if(data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("GetSalesOfferDueForStore")]           //duplicate
        public BaseResponseWithData<string> GetSalesOfferDueForStore([FromHeader] string ForDate, [FromHeader] bool type)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _salesOfferService.GetSalesOfferDueForStore(ForDate, validation.CompanyName, type);
                    if (data != null)
                    {
                        response = data;
                    }
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

        
        [HttpGet("GetPrOfferItemHistory")]          //duplicate(X)
        public async Task<GetPrOfferItemHistoryResponse> GetPrOfferItemHistory([FromHeader]long? InventoryItemId)
        {
            var response = new GetPrOfferItemHistoryResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = await _salesOfferService.GetPrOfferItemHistory(InventoryItemId);
                    if (data != null)
                    {
                        response = data;
                    }
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

       
        [HttpGet("GetSalesPersonsClientsDetailsExcel")]       //duplicate(X)
        public async Task<BaseMessageResponse> GetSalesPersonsClientsDetailsExcel([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] bool WithProjectExtraModifications)
        {
            var response = new BaseMessageResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _salesOfferService.Validation = validation;
                    var data = await _salesOfferService.GetSalesPersonsClientsDetailsExcel(Month, Year, BranchId, WithProjectExtraModifications);
                    if (data != null)
                    {
                        response = data;
                    }
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
        
        /*[HttpGet("GetSalesOfferTicketsForStore")]       //duplicate
        public BaseResponseWithData<string> GetSalesOfferTicketsForStore([FromHeader] string from, [FromHeader]string to, [FromHeader]string UserId)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _internalTicketService.GetSalesOfferTicketsForStore(from, to, UserId, validation.CompanyName, validation.userID);
                    if (data != null)
                    {
                        response = data;
                    }
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
        }*/

        [HttpGet("GetTotalAmountForEachCategory")] //(x)
        public BaseResponseWithData<string> GetTotalAmountForEachCategory(GetTotalAmountForEachCategoryFilters filters, string CompName)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _salesOfferService.GetTotalAmountForEachCategory(filters, validation.CompanyName);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("GetSalesOfferTicketsForStore")]   //duplicate(X)
        public BaseResponseWithData<string> GetSalesOfferTicketsForStore([FromHeader] string from, [FromHeader] string to, [FromHeader] string UserId)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _internalTicketService.GetSalesOfferTicketsForStore(from, to, UserId, validation.CompanyName, validation.userID);
                    if (data != null)
                    {
                        response = data;
                    }
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
