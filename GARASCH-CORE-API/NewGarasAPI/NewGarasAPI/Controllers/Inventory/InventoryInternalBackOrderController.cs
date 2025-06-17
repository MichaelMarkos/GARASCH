using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;

namespace NewGarasAPI.Controllers.Inventory
{
    [Route("Inventory/[controller]")]
    [ApiController]
    public class InventoryInternalBackOrderController : ControllerBase
    {
        private readonly IInventoryInternalBackOrderService _internalBackOrderService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        public InventoryInternalBackOrderController(IInventoryInternalBackOrderService internalBackOrderService, ITenantService tenantService)
        {
            _internalBackOrderService = internalBackOrderService;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
        }

        [HttpGet("GetInventoryIntenralBackOrdertInfo")]
        public InventoryInternalBackOrderItemInfoResponse GetInventoryIntenralBackOrdertInfo([FromHeader] long MatrialInternalBackOrderID)
        {
            InventoryInternalBackOrderItemInfoResponse response = new InventoryInternalBackOrderItemInfoResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _internalBackOrderService.GetInventoryIntenralBackOrdertInfo(MatrialInternalBackOrderID);

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


        [HttpGet("GetInventoryInternalBackOrderItemList")]
        public InventoryInternalBackOrderItemResponse GetInventoryInternalBackOrderItemList(GetInventoryInternalBackOrderFilters filters)
        {
            InventoryInternalBackOrderItemResponse response = new InventoryInternalBackOrderItemResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _internalBackOrderService.GetInventoryInternalBackOrderItemList(filters);

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


        [HttpPost("AddInventoryInternalBackOrder")]
        public BaseResponseWithId<long> AddInventoryInternalBackOrder(AddInventoryInternalBackOrderRequest request)
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
                    response = _internalBackOrderService.AddInventoryInternalBackOrder(request,validation.userID);

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
