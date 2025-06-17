using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.InternalTicket;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGaras.Infrastructure.Models.SalesOffer.InternalTicket;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.AccountAndFinance;

namespace NewGarasAPI.Controllers.Offer
{
    [Route("Offer/[controller]")]
    [ApiController]
    public class InternalTicketController : ControllerBase
    {
        private readonly IInternalTicketService _internalTicketService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public InternalTicketController(IInternalTicketService internalTicketService, ITenantService tenantService)
        {
            _internalTicketService = internalTicketService;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
        }

        [HttpGet("GetTopDoctors")]
        public BaseResponseWithData<List<TopData>> GetTopDoctors([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] string CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To)
        {
            BaseResponseWithData<List<TopData>> response = new BaseResponseWithData<List<TopData>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _internalTicketService.GetTopDoctors(year, month, day, CreatorId, From, To);

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



        [HttpGet("GetTopDepartmentsOrCategories")]
        public BaseResponseWithData<List<TopData>> GetTopDepartmentsOrCategories([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] string type)
        {
            BaseResponseWithData<List<TopData>> response = new BaseResponseWithData<List<TopData>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _internalTicketService.GetTopDepartmentsOrCategories(year, month, day, type);

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

        [HttpGet("GetInternalTicketDashboard")]
        public BaseResponseWithData<InternalTicketDashboardSummary> GetInternalTicketDashboard([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] DateTime? From, [FromHeader] DateTime? To)
        {
            BaseResponseWithData<InternalTicketDashboardSummary> response = new BaseResponseWithData<InternalTicketDashboardSummary>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _internalTicketService.GetInternalTicketDashboard(year, month, day, From, To);

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

        [HttpGet("GetInternalTicketsByTeams")]
        public BaseResponseWithData<GetInternalTicketsByDepartmentResponse> GetInternalTicketsByTeams([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] string CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To)
        {
            BaseResponseWithData<GetInternalTicketsByDepartmentResponse> response = new BaseResponseWithData<GetInternalTicketsByDepartmentResponse>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _internalTicketService.GetInternalTicketsByTeams(year, month, day, CreatorId, From, To);

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

        [HttpGet("GetInternalTicketsByCategories")]
        public BaseResponseWithData<GetInternalTicketsByCategoryResponse> GetInternalTicketsByCategories([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] string CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To)
        {
            BaseResponseWithData<GetInternalTicketsByCategoryResponse> response = new BaseResponseWithData<GetInternalTicketsByCategoryResponse>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _internalTicketService.GetInternalTicketsByCategories(year, month, day, CreatorId, From, To);

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
        [HttpGet("GetInternalTicketsByItemCategories")]
        public BaseResponseWithData<GetInternalTicketsByItemCategoryResponse> GetInternalTicketsByItemCategories([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] string CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To)
        {
            BaseResponseWithData<GetInternalTicketsByItemCategoryResponse> response = new BaseResponseWithData<GetInternalTicketsByItemCategoryResponse>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _internalTicketService.GetInternalTicketsByItemCategories(year, month, day, CreatorId, From, To);

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


        [HttpPost("AddNewSalesOfferForInternalTicket")]
        public BaseResponseWithId<long> AddNewSalesOfferForInternalTicket([FromBody] AddNewSalesOfferForInternalTicketRequest request)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var uploadedInventoryItems = _internalTicketService.AddNewSalesOfferForInternalTicket(request, validation.CompanyName, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(uploadedInventoryItems.Errors);
                        return response;
                    }

                    response = uploadedInventoryItems;
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





        [HttpGet("GetSalesOfferListForInternalTicket")]
        public GetSalesOfferListForInternalTicketResponse GetSalesOfferListForInternalTicket([FromHeader] GetSalesOfferInternalTicketListFilters filters)
        {
            GetSalesOfferListForInternalTicketResponse response = new GetSalesOfferListForInternalTicketResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _internalTicketService.GetSalesOfferListForInternalTicket(filters, filters.OfferStatus);

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

        [HttpGet("GetSalesOfferDetailsForInternalTicket")]
        public GetSalesOfferDetailsForInternalTicketResponse GetSalesOfferDetailsForInternalTicket([FromHeader] long SalesOfferId)
        {
            GetSalesOfferDetailsForInternalTicketResponse response = new GetSalesOfferDetailsForInternalTicketResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _internalTicketService.GetSalesOfferDetailsForInternalTicket(SalesOfferId);

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




        [HttpGet("GetSalesOfferTicketsForStorePDF")]
        public async Task<BaseResponseWithData<string>> GetSalesOfferTicketsForStoreForAllUsersPDF(InternalticketheaderPdf header)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
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
                    Response = await _internalTicketService.GetSalesOfferTicketsForStoreForAllUsersPDF(header, validation.userID);
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }

        }

        [HttpGet("GetSalesOfferTicketsForStore")]
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
