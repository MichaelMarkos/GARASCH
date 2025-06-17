using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.ItemsPricing;

namespace NewGarasAPI.Controllers.Inventory
{
    [Route("Inventory/[controller]")]
    [ApiController]
    public class ItemsPricingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IItemsPricingService _itemsPricingService;
        public ItemsPricingController(IUnitOfWork unitOfWork, IWebHostEnvironment host, ITenantService tenantService, IInventoryItemMatrialAddingAndExternalOrderService inventoryItemMatrialAddingAndExternalOrderService, IItemsPricingService itemsPricingService)
        {
            _host = host;
            _unitOfWork = unitOfWork;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _itemsPricingService = itemsPricingService;

        }

        [HttpPost("ManageInventoryStoreItemPricinig")]
        public BaseResponse ManageInventoryStoreItemPricinig([FromBody]AddOneInventoryStoreItemPricing data)
        {
            var response = new BaseResponse()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var req = _itemsPricingService.ManageInventoryStoreItemPricinig(data, validation.userID);
                    if(!req.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(req.Errors);
                        return response;
                    }
                    response = req;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                var err = new Error();
                err.ErrorCode = "Err-02";
                err.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpPost("AddInventoryStoreItemPricinig")]
        public BaseResponse AddInventoryStoreItemPricinig(AddInventoryStoreItemPricing data)
        {
            var response = new BaseResponse()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var req = _itemsPricingService.AddInventoryStoreItemPricinig(data);
                    if (!req.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(req.Errors);
                        return response;
                    }
                    response = req;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                var err = new Error();
                err.ErrorCode = "Err-02";
                err.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetInventoryItemPriceHistoryList")]
        public PurchasePoInventoryItemPriceListResponse GetInventoryItemPriceHistoryList([FromHeader]long InventoryItemId)
        {
            var response = new PurchasePoInventoryItemPriceListResponse()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var req = _itemsPricingService.GetInventoryItemPriceHistoryList(InventoryItemId);
                    if (!req.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(req.Errors);
                        return response;
                    }
                    response = req;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                var err = new Error();
                err.ErrorCode = "Err-02";
                err.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(err);
                return response;
            }
        }
    }
}
