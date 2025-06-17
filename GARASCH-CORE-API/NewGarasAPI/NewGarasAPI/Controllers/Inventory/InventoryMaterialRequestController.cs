using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.ProjectManagment;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;

namespace NewGarasAPI.Controllers.Inventory
{
    [Route("Inventory/[controller]")]
    [ApiController]
    public class InventoryMaterialRequestController : ControllerBase
    {
        private readonly IInventoryMaterialRequestService _materialRequestService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public InventoryMaterialRequestController(IInventoryMaterialRequestService materialRequestService, ITenantService tenantService)
        {
            _materialRequestService = materialRequestService;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
        }

        [HttpPost("AddInventoryStoreWithMatrialRequest")]
        public BaseResponseWithId<long> AddInventoryStoreWithMatrialRequest(AddInventoryStoreWithMatrialRequest request)
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
                    response = _materialRequestService.AddInventoryStoreWithMatrialRequest(request,validation.userID);

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

        [HttpGet("GetInventoryItemMatrialRequestList")]
        public InventoryItemMatrialRequest GetInventoryItemMatrialRequestList([FromHeader]GetInventoryItemMatrialFilters filters)
        {
            InventoryItemMatrialRequest response = new InventoryItemMatrialRequest();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _materialRequestService.GetInventoryItemMatrialRequestList(filters);

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

        [HttpGet("GetInventoryMatrialRequestListPaging")]
        public async Task<InventoryItemMatrialRequestPagingResponse> GetInventoryMatrialRequestListPaging(InventoryItemMatrialPagingFilters filters) { 
            InventoryItemMatrialRequestPagingResponse response = new InventoryItemMatrialRequestPagingResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _materialRequestService.GetInventoryMatrialRequestListPaging(filters,validation.userID);

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

        [HttpGet("GetInventoryItemMatrialRequestInfo")]
        public InventoryItemMatrialRequestInfoResponse GetInventoryItemMatrialRequestInfo([FromHeader] long MatrialRequestOrderID)
        {
            InventoryItemMatrialRequestInfoResponse response = new InventoryItemMatrialRequestInfoResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _materialRequestService.GetInventoryItemMatrialRequestInfo(MatrialRequestOrderID, validation.userID);

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
