using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;

namespace NewGarasAPI.Controllers.Inventory
{
    [Route("Inventory/[controller]")]
    [ApiController]
    public class InventoryTransferOrderController : ControllerBase
    {
        private readonly IInventoryTransferOrderService _inventoryTransferOrderService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        public InventoryTransferOrderController(IInventoryTransferOrderService inventoryTransferOrderService, ITenantService tenantService)
        {
            _inventoryTransferOrderService = inventoryTransferOrderService;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
        }

        [HttpPost("AddInvnetoryInternalTransferOrder")]
        public BaseResponseWithId<long> AddInvnetoryInternalTransferOrder(AddInventoryInternalTransferOrderRequest request)
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
                    response = _inventoryTransferOrderService.AddInvnetoryInternalTransferOrder(request, validation.userID, validation.CompanyName);

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
        [HttpPost("AddInvnetoryInternalTransferOrder2")]
        public async Task<BaseResponseWithId<long>> AddInvnetoryInternalTransferOrder2(AddInventoryInternalTransferOrderRequest request)
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
                    response = await _inventoryTransferOrderService.AddInvnetoryInternalTransferOrder2(request, validation.userID, validation.CompanyName);

                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);

                return response;
            }
        }
        [HttpPost("AddInvnetoryInternalTransferOrderPOS")]
        public async Task<BaseResponseWithId<long>> AddInvnetoryInternalTransferOrderPOS(AddInventoryInternalTransferOrderRequest request, long creator, string compName)
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
                    response = await _inventoryTransferOrderService.AddInvnetoryInternalTransferOrderPOS(request, validation.userID, validation.CompanyName);

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
        [HttpPost("AddInvnetoryInternalTransferOrderItemsPOS")]
        public async Task<BaseResponseWithId<long>> AddInvnetoryInternalTransferOrderItemsPOS(AddInventoryInternalTransferOrderItemsRequest request, long creator, string compName)
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
                    response = await _inventoryTransferOrderService.AddInvnetoryInternalTransferOrderItemsPOS(request, validation.userID, validation.CompanyName);

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


        [HttpPost("ReverseInvnetoryInternalTransferOrder")]
        public BaseResponseWithId<long> ReverseInvnetoryInternalTransferOrder(ReverseInvnetoryInternalTransferOrder request)
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
                    response = _inventoryTransferOrderService.ReverseInvnetoryInternalTransferOrder(request, validation.userID, validation.CompanyName);
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


        [HttpGet("GetInventoryInternalTransferItemList")]
        public InventoryInternalTransferOrderResponse GetInventoryInternalTransferItemList(GetInventoryInternalTransferFilters filters)
        {
            InventoryInternalTransferOrderResponse response = new InventoryInternalTransferOrderResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _inventoryTransferOrderService.GetInventoryInternalTransferItemList(filters, validation.userID, validation.CompanyName);

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


        [HttpGet("GetInventoryIntenralTransferItemInfo")]
        public InventoryInternalTransferOrderItemInfoResponse GetInventoryIntenralTransferItemInfo([FromHeader] long InternalTransferOrderID)
        {
            InventoryInternalTransferOrderItemInfoResponse response = new InventoryInternalTransferOrderItemInfoResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _inventoryTransferOrderService.GetInventoryIntenralTransferItemInfo(InternalTransferOrderID, validation.userID, validation.CompanyName);

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

        [HttpGet("GetInventoryInternalTransferItemListForWeb")]
        public BaseResponseWithDataAndHeader<List<InventoryInternalTransferOrderInfo>> GetInventoryInternalTransferItemListForWeb([FromHeader] GetInventoryInternalTransferForWebFilters filtes)
        {
            var response = new BaseResponseWithDataAndHeader<List<InventoryInternalTransferOrderInfo>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _inventoryTransferOrderService.GetInventoryInternalTransferItemListForWeb(filtes, validation.userID, validation.CompanyName);

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
