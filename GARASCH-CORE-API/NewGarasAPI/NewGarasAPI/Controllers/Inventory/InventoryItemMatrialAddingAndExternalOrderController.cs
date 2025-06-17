using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.Responses;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.InventoryItemMatrialAddingAndExternalOrder.Filters;
using NewGaras.Infrastructure.DTO.InventoryItemMatrialAddingAndExternalOrder;
using DocumentFormat.OpenXml.Spreadsheet;
using NewGaras.Infrastructure.Models.Inventory.Requests;

namespace NewGarasAPI.Controllers.Inventory
{
    [Route("Inventory/[controller]")]
    [ApiController]
    public class InventoryItemMatrialAddingAndExternalOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IInventoryItemMatrialAddingAndExternalOrderService _inventoryItemMatrialAddingAndExternalOrderService;

        public InventoryItemMatrialAddingAndExternalOrderController(IUnitOfWork unitOfWork, IWebHostEnvironment host, ITenantService tenantService, IInventoryItemMatrialAddingAndExternalOrderService inventoryItemMatrialAddingAndExternalOrderService)
        {
            _host = host;
            _unitOfWork = unitOfWork;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _inventoryItemMatrialAddingAndExternalOrderService = inventoryItemMatrialAddingAndExternalOrderService;
        }


        [HttpGet("GetInventoryItemSupplierMatrialAddingAndExternalOrderInfo")]
        public InventoryItemSupplierMatrialAddingOrderInfoResponse GetInventoryItemSupplierMatrialAddingAndExternalOrderInfo([FromHeader] long MatrialAddingOrderID)
        {
            InventoryItemSupplierMatrialAddingOrderInfoResponse Response = new InventoryItemSupplierMatrialAddingOrderInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            #region validation
            if (MatrialAddingOrderID == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err109";
                error.ErrorMSG = "Invalid Matrial Adding Order ID";
                Response.Errors.Add(error);
                return Response;
            }
            #endregion

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var inventoryItemOpeningBalance = _inventoryItemMatrialAddingAndExternalOrderService.GetInventoryItemSupplierMatrialAddingAndExternalOrderInfo(MatrialAddingOrderID, validation.userID, validation.CompanyName);
                    if (!inventoryItemOpeningBalance.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(inventoryItemOpeningBalance.Errors);
                        return Response;
                    }
                    Response = inventoryItemOpeningBalance;
                    return Response;
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);

                return Response;
            }
        }


        [HttpGet("GetInventoryItemMatrialAddingAndExternalOrderList")]
        public InventoryItemMatrialAddingOrder GetInventoryItemMatrialAddingAndExternalOrderList([FromHeader] GetInventoryItemMatrialAddingAndExternalOrderListFilters filters)
        {
            InventoryItemMatrialAddingOrder Response = new InventoryItemMatrialAddingOrder();
            Response.Result = true;
            Response.Errors = new List<Error>();



            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var inventoryItemOpeningBalance = _inventoryItemMatrialAddingAndExternalOrderService.GetInventoryItemMatrialAddingAndExternalOrderList(filters, validation.userID, validation.CompanyName);
                    if (!inventoryItemOpeningBalance.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(inventoryItemOpeningBalance.Errors);
                        return Response;
                    }
                    Response = inventoryItemOpeningBalance;
                    return Response;
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);

                return Response;
            }
        }

        [HttpPost("AddInventoryAddingAndExternalBackOrder")]
        public BaseResponseWithID AddInventoryAddingAndExternalBackOrder([FromBody] AddSupplierAndStoreWithMatrialAddingAndExternalBackOrderRequest dto)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();



            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var inventoryAddingAndExternalBackOrder = _inventoryItemMatrialAddingAndExternalOrderService.AddInventoryAddingAndExternalBackOrder(dto, validation.userID, validation.CompanyName);
                    if (!inventoryAddingAndExternalBackOrder.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(inventoryAddingAndExternalBackOrder.Errors);
                        return Response;
                    }
                    Response = inventoryAddingAndExternalBackOrder;
                    return Response;
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);

                return Response;
            }
        }


        [HttpPost("ReverseInventoryAddingOrder")]
        public BaseResponseWithId<long> ReverseInventoryAddingOrder(ReverseInventoryAddingOrderRequest request)
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
                    Response = _inventoryItemMatrialAddingAndExternalOrderService.ReverseInventoryAddingOrder(request, validation.userID, validation.CompanyName);  
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);

                return Response;
            }
        }

        [HttpPost("AddPOToInventoryStoreItem")]
        public async Task<BaseResponseWithID> AddPOToInventoryStoreItem([FromBody] AddPOToInventoryStoreItemRequest dto)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();



            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var inventoryAddingAndExternalBackOrder = await _inventoryItemMatrialAddingAndExternalOrderService.AddPOToInventoryStoreItem(dto, validation.userID, validation.CompanyName);
                    if (!inventoryAddingAndExternalBackOrder.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(inventoryAddingAndExternalBackOrder.Errors);
                        return Response;
                    }
                    Response = inventoryAddingAndExternalBackOrder;
                    return Response;
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);

                return Response;
            }
        }
    }
}
