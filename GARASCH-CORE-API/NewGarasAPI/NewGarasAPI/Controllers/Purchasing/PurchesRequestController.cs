using Azure;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.Client;
using NewGaras.Infrastructure.Models.PurchesRequest;
using NewGaras.Infrastructure.Models.PurchesRequest.Filters;
using NewGaras.Infrastructure.Models.PurchesRequest.Responses;
using NewGarasAPI.Models.User;

namespace NewGarasAPI.Controllers
{
    [Route("Purchasing/[controller]")]
    [ApiController]
    public class PurchesRequestController : Controller
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        static string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IPurchesRequestService _purchesRequestService;

        public PurchesRequestController(IWebHostEnvironment host, ITenantService tenantService, IPurchesRequestService purchesRequestService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _host = host;
            _purchesRequestService = purchesRequestService;
        }

        [HttpGet("GetManageAssignedPRItems")]
        public ManageAssignedPRItemsResponse GetManageAssignedPRItems([FromHeader]ManageAssignedPRItemsFilters filters)
        {
            ManageAssignedPRItemsResponse response = new ManageAssignedPRItemsResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data = _purchesRequestService.GetManageAssignedPRItems(filters, validation.CompanyName);
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

        [HttpGet("GetPurchasingPersonsList")]
        public UserDDLResponse GetPurchasingPersonsList([FromHeader]string SearchKey)
        {
            UserDDLResponse response = new UserDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data = _purchesRequestService.GetPurchasingPersonsList(SearchKey, validation.CompanyName);
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

        [HttpPost("AddMatrialDirectPR")]
        public BaseResponseWithID AddMatrialDirectPR([FromBody]AddMatrialDirectPRRequest newData)
        {
            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data = _purchesRequestService.AddMatrialDirectPR(newData, validation.userID);
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

        [HttpPost("RemoveAssignedToPRItem")]
        public BaseResponseWithID RemoveAssignedToPRItem([FromBody] RemoveAssignedToPRItemsRRequest newData)
        {
            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchesRequestService.RemoveAssignedToPRItem(newData);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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
        [HttpGet("GetSelectPRItemsForAssign")]
        public SelectPRItemsForAssignResponse GetSelectPRItemsForAssign([FromHeader]long? InventoryItemID)
        {
            SelectPRItemsForAssignResponse response = new SelectPRItemsForAssignResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchesRequestService.GetSelectPRItemsForAssign(InventoryItemID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpPost("AddAssignPRItem")]
        public BaseResponseWithID AddAssignPRItem(AssignPRItemRequest NewData)
        {
            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data = _purchesRequestService.AddAssignPRItem(NewData);
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

        [HttpGet("GetSelectPRItemsForAddPO")]
        public SelectPRItemsForAddPOResponse GetSelectPRItemsForAddPO([FromHeader]long? InventoryItemID)
        {
            SelectPRItemsForAddPOResponse response = new SelectPRItemsForAddPOResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchesRequestService.GetSelectPRItemsForAddPO(InventoryItemID, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpPost("AddPurchaseOrder")]
        public async Task<BaseResponseWithID> AddPurchaseOrder([FromBody]AddPurchaseOrderRequest newData)
        {
            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data =await _purchesRequestService.AddPurchaseOrder(newData, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpGet("GetMatrialPurchaseRequestList")]
        public InventoryMatrialPurchaseRequestResponse GetMatrialPurchaseRequestList([FromHeader]InventoryMatrialPurchaseFilters filters)
        {
            InventoryMatrialPurchaseRequestResponse response = new InventoryMatrialPurchaseRequestResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchesRequestService.GetMatrialPurchaseRequestList(filters);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpGet("GetMatrialPurchaseRequestListForWeb")]
        public InventoryMatrialPurchaseRequestResponse2 GetMatrialPurchaseRequestListForWeb([FromHeader]InventoryMatrialPurchaseFilters filters)
        {
            InventoryMatrialPurchaseRequestResponse2 response = new InventoryMatrialPurchaseRequestResponse2();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchesRequestService.GetMatrialPurchaseRequestListForWeb(filters);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpGet("GetPurchaseRequestWithItemsInfo")]
        public PurchaseRequestWithItemsInfoResponse GetPurchaseRequestWithItemsInfo([FromHeader]long PurchaseRequestID)
        {
            PurchaseRequestWithItemsInfoResponse response = new PurchaseRequestWithItemsInfoResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchesRequestService.GetPurchaseRequestWithItemsInfo(PurchaseRequestID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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


        [HttpGet("GetMangePurchasePOList")]
        public GetPurchasePOResponse GetMangePurchasePOList([FromHeader]long? InventoryItemID, [FromHeader]long? CreatorUserID, [FromHeader]string RequestDatestr, [FromHeader] bool? WithJE)
        {
            GetPurchasePOResponse response = new GetPurchasePOResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchesRequestService.GetMangePurchasePOList(InventoryItemID, CreatorUserID, RequestDatestr, WithJE);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpGet("GetMangePurchasePOWebList")]
        public GetPurchasePOWebResponse GetMangePurchasePOWebList([FromHeader]GetMangePurchasePOWebListFilters filters)
        {
            GetPurchasePOWebResponse response = new GetPurchasePOWebResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchesRequestService.GetMangePurchasePOWebList(filters);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpGet("ViewPurchaseOrder")]
        public async Task<ViewPurchaseOrderResponse> ViewPurchaseOrder([FromHeader]long? PoId, [FromHeader]string SupplierInvoiceSerial)
        {

            ViewPurchaseOrderResponse response = new ViewPurchaseOrderResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data =await _purchesRequestService.ViewPurchaseOrder(PoId, SupplierInvoiceSerial);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpGet("GetPurchaseItemList")]
        public async Task<GetPurchaseItemListResponse> GetPurchaseItemList([FromHeader]string SupplierInvoiceSerial)
        {
            GetPurchaseItemListResponse response = new GetPurchaseItemListResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = await _purchesRequestService.GetPurchaseItemList(SupplierInvoiceSerial);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
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

        [HttpPost("ManagePurchaseRequestItem")]
        public async Task<BaseResponseWithID> ManagePurchaseRequestItem([FromBody]ManagePurchaseRequestItemRequest RequestData)
        {
            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data =await _purchesRequestService.ManagePurchaseRequestItem(RequestData, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(data.Errors);
                        return response;
                    }
                    response = data;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);
                return response;
            }
        }

    }
}
