using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.Inventory.Requests;

namespace NewGarasAPI.Controllers.Inventory
{
    [Route("Inventory/[controller]")]
    [ApiController]
    public class InventoryMaterialReleaseController : ControllerBase
    {
        private readonly IInventoryMateriaReleaseService _inventoryMateriaReleaseService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        public InventoryMaterialReleaseController(IInventoryMateriaReleaseService inventoryMateriaReleaseService, ITenantService tenantService)
        {
            _inventoryMateriaReleaseService = inventoryMateriaReleaseService;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
        }
        [HttpGet("GetInventoryItemMatrialReleaseList")]
        public InventoryItemMatrialReleaseResponse GetInventoryItemMatrialReleaseList(GetInventoryItemMatrialReleaseListFilters filters)
        {
            InventoryItemMatrialReleaseResponse response = new InventoryItemMatrialReleaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _inventoryMateriaReleaseService.GetInventoryItemMatrialReleaseList(filters);

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

        [HttpGet("GetInventoryItemMatrialReleaseListPaging")]
        public InventoryItemMatrialReleasePagingResponse GetInventoryItemMatrialReleaseListPaging(GetInventoryItemMatrialReleaseListPagingFilters filters)
        {
            InventoryItemMatrialReleasePagingResponse response = new InventoryItemMatrialReleasePagingResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _inventoryMateriaReleaseService.GetInventoryItemMatrialReleaseListPaging(filters);

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

        [HttpGet("GetInventoryItemMatrialReleasetInfo")]
        public InventoryItemMatrialReleaseInfoResponse GetInventoryItemMatrialReleasetInfo([FromHeader] long MatrialReleaseOrderID)
        {
            InventoryItemMatrialReleaseInfoResponse response = new InventoryItemMatrialReleaseInfoResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _inventoryMateriaReleaseService.GetInventoryItemMatrialReleasetInfo(MatrialReleaseOrderID);

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

        [HttpGet("GetInventoryStoreItemBatchWithExpDate")]
        public GetInventoryStoreItemBatchWithExpDateResponse GetInventoryStoreItemBatchWithExpDate(GetInventoryStoreItemBatchWithExpDateFilters filters)
        {
            GetInventoryStoreItemBatchWithExpDateResponse response = new GetInventoryStoreItemBatchWithExpDateResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _inventoryMateriaReleaseService.Validation = validation;
                    response = _inventoryMateriaReleaseService.GetInventoryStoreItemBatchWithExpDate(filters);

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

        [HttpPost("AddInventoryItemMatrialRelease")]
        public BaseResponseWithId<long> AddInventoryItemMatrialRelease(AddInventoryItemMatrialReleaseRequest request)
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
                    _inventoryMateriaReleaseService.Validation = validation;
                    response = _inventoryMateriaReleaseService.AddInventoryItemMatrialRelease(request,validation.userID);

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

        [HttpPost("AddInventoryItemMatrialReleasePrintInfo")]
        public async Task<BaseResponseWithId<long>> AddInventoryItemMatrialReleasePrintInfo(AddInventoryItemMatrialReleasePrintInfoRequest request)
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
                    response = await _inventoryMateriaReleaseService.AddInventoryItemMatrialReleasePrintInfo(request, validation.userID);

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

        [HttpGet("GetInventoryItemMatrialReleasetPrintInfo")]
        public InventoryMatrialReleasePrintInfoResponse GetInventoryItemMatrialReleasetPrintInfo([FromHeader] long MatrialReleaseOrderID)
        {
            InventoryMatrialReleasePrintInfoResponse response = new InventoryMatrialReleasePrintInfoResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _inventoryMateriaReleaseService.GetInventoryItemMatrialReleasetPrintInfo(MatrialReleaseOrderID);

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

        [HttpGet("GetMatrialReleaseShippingAddressContact")]
        public async Task<GetMatrialReleaseShippingAddressContactResponse> GetMatrialReleaseShippingAddressContact([FromHeader] long MatrialReleaseID)
        {
            GetMatrialReleaseShippingAddressContactResponse response = new GetMatrialReleaseShippingAddressContactResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _inventoryMateriaReleaseService.GetMatrialReleaseShippingAddressContact(MatrialReleaseID);

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

        [HttpPost("MatrialReleasePDFReport")]
        public async Task<BaseMessageResponse> MatrialReleasePDFReport(MatrialReleasePDFFilters filters, GetMatrialReleaseDataResponse request)
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
                    _inventoryMateriaReleaseService.Validation = validation;
                    response = await _inventoryMateriaReleaseService.MatrialReleasePDFReport(filters,request);

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

        [HttpGet("GetInventoryItemForCreateMatrialRelease")]
        public InventoryItemMatrialReleaseInfoResponse GetInventoryItemForCreateMatrialRelease([FromHeader] long MatrialRequestID)
        {
            var response = new InventoryItemMatrialReleaseInfoResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                
                if (response.Result)
                {
                    response = _inventoryMateriaReleaseService.GetInventoryItemForCreateMatrialRelease(MatrialRequestID, validation.userID);
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

        [HttpGet("GetMatrialReleaseItemList")]
        public MatrialReleaseDDLResponse GetMatrialReleaseItemList(GetMatrialReleaseItemListFilters filters)
        {
            var response = new MatrialReleaseDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = _inventoryMateriaReleaseService.GetMatrialReleaseItemList(filters);
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
