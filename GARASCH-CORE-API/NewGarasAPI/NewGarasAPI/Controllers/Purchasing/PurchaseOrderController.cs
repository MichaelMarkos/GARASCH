using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.PurchaseOrder;
using NewGaras.Infrastructure.Models.PurchesRequest;
using NewGaras.Infrastructure.Models.PurchesRequest.Filters;
using NewGaras.Infrastructure.Models.PurchesRequest.Responses;

namespace NewGarasAPI.Controllers.Purchasing
{
    [Route("Purchasing/[controller]")]
    [ApiController]
    public class PurchaseOrderController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        static string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderController(IWebHostEnvironment host, ITenantService tenantService, IPurchaseOrderService purchaseOrderService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _host = host;
            _purchaseOrderService = purchaseOrderService;
        }
        [HttpGet("GetPurchasingAndSuppliersDashboard")]
        public PurchasingAndSuppliersDashboardResponse GetPurchasingAndSuppliersDashboard([FromHeader] long InventoryStoreID)
        {
            PurchasingAndSuppliersDashboardResponse response = new PurchasingAndSuppliersDashboardResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _purchaseOrderService.GetPurchasingAndSuppliersDashboard(InventoryStoreID);
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

        [HttpGet("GetSelectPRItemsForAddPO")]
        public SelectPRItemsForAddPOResponse GetSelectPRItemsForAddPO([FromHeader] long? InventoryItemID)
        {
            var response = new SelectPRItemsForAddPOResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchaseOrderService.GetSelectPRItemsForAddPO(InventoryItemID, validation.userID);
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

        [HttpPost("AddNewPurchaseOrder")]
        public async Task<BaseResponseWithID> AddNewPurchaseOrder([FromBody]AddNewPurchaseOrderRequest NewData)
        {
            var response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data =await _purchaseOrderService.AddNewPurchaseOrder(NewData, validation.userID);
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

        [HttpPost("UpdatePurchaseOrder")]
        public async Task<BaseResponse> UpdatePurchaseOrder(UpdatePurchaseOrderRequest updatedData)
        {
            var response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = await _purchaseOrderService.UpdatePurchaseOrder(updatedData);
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

        [HttpPost("AddImportPoSettings")]
        public BaseResponse AddImportPoSettings(ImportPoSettings newData)
        {
            var response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchaseOrderService.AddImportPoSettings(newData);
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

        [HttpPost("SentToSupplier")]
        public BaseResponse SentToSupplier([FromForm]SentToSupplier sent)
        {
            var response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchaseOrderService.SentToSupplier(sent, validation.userID, validation.CompanyName);
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

        [HttpPost("AddShippmentDocuments")]
        public BaseResponse AddShippmentDocuments([FromForm]AddShippmentDocuments doc)
        {
            var response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchaseOrderService.AddShippmentDocuments(doc, validation.userID, validation.CompanyName);
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


        [HttpPost("AddShippingMethodDetails")]
        public BaseResponse AddShippingMethodDetails([FromBody]ShippingMethodDetails details)
        {
            var response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchaseOrderService.AddShippingMethodDetails(details);
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

        [HttpGet("GetPOApprovalStatus")]
        public ViewPOApprovalStatusResponse GetPOApprovalStatus([FromHeader]long POID)
        {
            var response = new ViewPOApprovalStatusResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data =  _purchaseOrderService.GetPOApprovalStatus(POID);
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

        [HttpPost("SendFinalPOToSelectedSupplier")]
        public async Task<BaseResponseWithID> SendFinalPOToSelectedSupplier([FromBody]SendFinalPOToSelectedSupplierRequest RequestData)
        {
            var response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data =await _purchaseOrderService.SendFinalPOToSelectedSupplier(RequestData, validation.userID);
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

        [HttpPost("ManagePurchaseOrderItem")]
        public async Task<BaseResponse> ManagePurchaseOrderItem(ManagePurchaseOrderItemRequest RequestData)
        {
            var response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = await _purchaseOrderService.ManagePurchaseOrderItem(RequestData, validation.userID);
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

        [HttpPost("ManageAddingOrderItemPO")]
        public async Task<BaseResponse> ManageAddingOrderItemPO(ManageAddingOrderItemPORequest RequestData)
        {
            var response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = await _purchaseOrderService.ManageAddingOrderItemPO(RequestData, validation.userID);
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


        [HttpGet("GetActivePOList")]
        public ActivePODDLResponse GetActivePOList([FromHeader]long InventoryItemID, [FromHeader]long ToSupplierID)
        {
            var response = new ActivePODDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchaseOrderService.GetActivePOList(InventoryItemID, ToSupplierID);
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

        [HttpGet("GetExternalPOList")]
        public ActivePODDLResponse GetExternalPOList([FromHeader]long InventoryItemID, [FromHeader]long ToSupplierID)
        {
            var response = new ActivePODDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    var data = _purchaseOrderService.GetExternalPOList(InventoryItemID, ToSupplierID);
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
    }
}
