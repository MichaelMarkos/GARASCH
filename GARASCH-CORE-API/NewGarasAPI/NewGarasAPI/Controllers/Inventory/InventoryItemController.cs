using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.InventoryItem;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Models.Account;
using NewGaras.Infrastructure.Models.User.Filters;
using NewGaras.Infrastructure.Models.User.Response;
using Azure;
using NewGaras.Infrastructure.Models.InventoryItem.Filters;
using NewGaras.Infrastructure.Models.Common;
using NewGarasAPI.Models.Inventory;
using NewGarasAPI.Models.Inventory.Requests;

namespace NewGarasAPI.Controllers.Inventory
{
    [Route("Inventory/[controller]")]
    [ApiController]
    public class InventoryItemController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IInventoryItemService _inventoryItemService;

        public InventoryItemController(IUnitOfWork unitOfWork, IWebHostEnvironment host, ITenantService tenantService, IInventoryItemMatrialAddingAndExternalOrderService inventoryItemMatrialAddingAndExternalOrderService, IInventoryItemService inventoryItemService)
        {
            _host = host;
            _unitOfWork = unitOfWork;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _inventoryItemService = inventoryItemService;
            
        }

        [HttpGet("GetInventoryItemExcelTemplete")]
        public BaseResponseWithData< string> GetInventoryItemExcelTemplete()
        {
            var response = new BaseResponseWithData< string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var templete = _inventoryItemService.GetInventoryItemExcelTemplete(validation.CompanyName);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(templete.Errors);
                        return response;
                    }
                    response.Data = templete.Data;
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

                return  response;
            }
        }

        [HttpPost("AddInventoryItemListByExcelSheet")]
        public BaseResponseWithMessage<string> UploadInventoryItemExcel([FromForm]AddAttachment dto)
        {
            var response = new BaseResponseWithMessage<string>();
            response.Result = true; 
            response.Errors = new List<Error>();

            #region validation

            if (dto.Content == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Please Upload the Templete Excel file";
                response.Errors.Add(error);
                return response;
            }
            var fileExtension = System.IO.Path.GetExtension(dto.Content.FileName);
            var allowedExtensions = new List<string>() { ".xlsx", ".xml" };
            if (!allowedExtensions.Contains(fileExtension) )
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "please upload an valid Excel file";
                response.Errors.Add(error);
                return response;
            }

            #endregion


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var uploadedInventoryItems = _inventoryItemService.UploadInventoryItemExcel(dto, validation.userID);
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

        [HttpPost("EditInventoryStorePerID")]
        public BaseResponseWithId<long> EditInventoryStorePerID([FromBody]EditInventoryStoreData request)
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
                    var result = _inventoryItemService.EditInventoryStorePerID(request, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetAccountAndFinanceInventoryItemStockBalance")]
        public AccountAndFinanceInventoryItemStockBalanceResponse GetAccountAndFinanceInventoryItemStockBalance([FromHeader]long InventoryItemID)
        {
            var response = new AccountAndFinanceInventoryItemStockBalanceResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var result = _inventoryItemService.GetAccountAndFinanceInventoryItemStockBalance(InventoryItemID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }
                    response = result;
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


        [HttpGet("GetAccountAndFinanceInventoryItemStockBalanceHold")]
        public async Task<InventoryItemStockBalanceHoldResponse> GetAccountAndFinanceInventoryItemStockBalanceHold([FromHeader]long InventoryItemID)
        {
            var response = new InventoryItemStockBalanceHoldResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var result = await _inventoryItemService.GetAccountAndFinanceInventoryItemStockBalanceHold(InventoryItemID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }
                    response = result;
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

        [HttpGet("GetInventoryAndStoresDashboard")]
        public async Task<InventoryAndStoresDashboardResponse> GetInventoryAndStoresDashboard([FromHeader] long InventoryStoreID,[FromHeader] DateTime? DateTo)
        {
            var response = new InventoryAndStoresDashboardResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _inventoryItemService.GetInventoryAndStoresDashboard(InventoryStoreID,DateTo);
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

        [HttpGet("GetInventoryStoreItemTotalPricesAndCosts")]
        public InventoryStoreItemTotalPriceResponse GetInventoryStoreItemTotalPricesAndCosts([FromHeader] long InventoryStoreID, [FromHeader] DateTime? DateTo)
        {
            var response = new InventoryStoreItemTotalPriceResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = _inventoryItemService.GetInventoryStoreItemTotalPricesAndCosts(InventoryStoreID, DateTo);
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

        [HttpGet("GetInventoryStoreKeeperDDL")]
        public async Task<GetInventoryStoreKeeperDDLResponse> GetInventoryStoreKeeperDDL()
        {
            var response = new GetInventoryStoreKeeperDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _inventoryItemService.GetInventoryStoreKeeperDDL();

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

        [HttpGet("GetInventoryStoreLocationsDDL")]
        public async Task<GetInventoryStoreLocationsDDLResponse> GetInventoryStoreLocationsDDL()
        {
            var response = new GetInventoryStoreLocationsDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _inventoryItemService.GetInventoryStoreLocationsDDL();

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

        [HttpGet("GetBranchProduct")]
        public async Task<GetBranchProductResponse> GetBranchProduct()
        {
            var response = new GetBranchProductResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    response = await _inventoryItemService.GetBranchProduct();

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

        [HttpGet("GetInventoryItemListDDL")]
        public async Task<SelectDDLResponse> GetInventoryItemListDDL([FromHeader] int InventoryItemCategoryId, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 100)
        {
            var response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = await _inventoryItemService.GetInventoryItemListDDL(InventoryItemCategoryId, CurrentPage,NumberOfItemsPerPage);

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

        [HttpPost("DeleteInventoryStoreKeeper")]
        public BaseResponseWithId<long> DeleteInventoryStoreKeeper(AddInventoryStoreData request)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.DeleteInventoryStoreKeeper(request);

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

        [HttpGet("InventoryStoreItemReportWithoutPrices")]
        public BaseMessageResponse InventoryStoreItemReportWithoutPrices([FromHeader] string FileExtension)
        {
            var response = new BaseMessageResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.InventoryStoreItemReportWithoutPrices(FileExtension);

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

        [HttpGet("InventoryStoreItemReportWithProfitCalc")]
        public BaseMessageResponse InventoryStoreItemReportWithProfitCalc([FromHeader] decimal Profit, [FromHeader] string FileExtension)
        {
            var response = new BaseMessageResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.InventoryStoreItemReportWithProfitCalc(Profit, FileExtension);

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

        [HttpGet("HoldItemsExcel")]
        public async Task<BaseMessageResponse> HoldItemsExcel()
        {
            var response = new BaseMessageResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = await _inventoryItemService.HoldItemsExcel();

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

        [HttpGet("InventoryStoreItemExcelsheet")]
        public BaseMessageResponse InventoryStoreItemExcelsheet([FromHeader] string FileExtension)
        {
            var response = new BaseMessageResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.InventoryStoreItemExcelsheet(FileExtension);

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

        [HttpGet("RemainInventoryItemRequestedQTYReport")]
        public GetRemainInventoryItemRequestedQtyResponse RemainInventoryItemRequestedQTYReport([FromHeader] string FileExtension)
        {
            var response = new GetRemainInventoryItemRequestedQtyResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.RemainInventoryItemRequestedQTYReport(FileExtension);

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

        [HttpGet("InventoryStoreItemMovementReportPDF")]
        public GetInventoryStoreItemMovementReportResponse InventoryStoreItemMovementReportPDF([FromHeader] string FileExtension, [FromHeader] long InventoryItemID)
        {
            var response = new GetInventoryStoreItemMovementReportResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.InventoryStoreItemMovementReportPDF(FileExtension, InventoryItemID);

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

        [HttpGet("GetAccountAndFinanceInventoryStoreItemMovementReportList")]
        public AccountsAndFinanceInventoryStoreItemReportResponse GetInventoryStoreItemMovementReportList([FromHeader]GetAccountAndFinanceInventoryStoreItemMovementReportListFilters filters)
        {
            var response = new AccountsAndFinanceInventoryStoreItemReportResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response =  _inventoryItemService.GetInventoryStoreItemMovementReportList(filters);
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

        [HttpGet("GetInventoryItemSupplierList")]
        public InventoryItemSupplierResponse GetInventoryItemSupplierList([FromHeader] long InventoryItemID, [FromHeader] string SupplierItemSerial, [FromHeader] string OrderType)
        {
            var response = new InventoryItemSupplierResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.GetInventoryItemSupplierList(InventoryItemID, SupplierItemSerial, OrderType);
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

        [HttpGet("GetContractTypeList")]
        public GetContractTypeListResponse GetContractTypeList()
        {
            var response = new GetContractTypeListResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.GetContractTypeList();
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

        [HttpPost("HoldReleaseInventoryMatrialRequest")]
        public async Task<BaseResponse> HoldReleaseInventoryMatrialRequest(AddInventoryStoreWithMatrialRequestt RequestData)
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
                    var result = await _inventoryItemService.HoldReleaseInventoryMatrialRequest(RequestData);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetInventoryItemHoldDetails")]
        public async Task<InventoryItemHoldDetailsResponse> GetInventoryItemHoldDetails([FromHeader]long InventoryItemID)
        {
            var response = new InventoryItemHoldDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response =await _inventoryItemService.GetInventoryItemHoldDetails(InventoryItemID);
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

        [HttpGet("GetAccountAndFinanceInventoryStoreItemReportList2")]
        public AccountsAndFinanceInventoryStoreItemResponse GetAccountAndFinanceInventoryStoreItemReportList2([FromHeader]GetAccountAndFinanceInventoryStoreItemReportListFilters filters)
        {
            var response = new AccountsAndFinanceInventoryStoreItemResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response =  _inventoryItemService.GetAccountAndFinanceInventoryStoreItemReportList(filters);
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

        [HttpGet("GetInventoryStoreList")]
        public InventortyStoreListResponse GetInventoryStoreList([FromHeader] string Type, [FromHeader] long userId)
        {
            var response = new InventortyStoreListResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.GetInventoryStoreList(Type, userId);
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

        [HttpGet("GetInventoryStoreLocationList")]
        public InventortyStoreLocationListResponse GetInventoryStoreLocationList([FromHeader] long InventoryStoreID, [FromHeader] long? InventoryItemID)
        {
            var response = new InventortyStoreLocationListResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.GetInventoryStoreLocationList(InventoryStoreID, InventoryItemID);
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

        [HttpGet("GetInventoryStoreItemFullDataList")]
        public InventortyStoreItemFullDataListResponse GetInventoryStoreItemFullDataList([FromHeader] GetInventoryStoreItemFullDataListFilters filters)
        {
            var response = new InventortyStoreItemFullDataListResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.GetInventoryStoreItemFullDataList(filters);
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

        [HttpGet("GetAccountAndFinanceInventoryStoreItemInfo")]
        public AccountsAndFinanceInventoryItemInfoResponse GetAccountAndFinanceInventoryStoreItemInfo([FromHeader] long InventoryItemID, [FromHeader] string InventoryItemCode)
        {
            var response = new AccountsAndFinanceInventoryItemInfoResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = _inventoryItemService.GetAccountAndFinanceInventoryStoreItemInfo(InventoryItemID, InventoryItemCode);
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

        [HttpGet("GetInventoryItemContentTree")]
        public async Task<GetInventoryItemContentTreeResponse> GetInventoryItemContentTree([FromHeader] long? InventoryItemId)
        {
            var response = new GetInventoryItemContentTreeResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                _inventoryItemService.Validation = validation;
                if (response.Result)
                {
                    response = await _inventoryItemService.GetInventoryItemContentTree(InventoryItemId);
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

        [HttpPost("DeleteInventoryItemContent")]
        public BaseResponseWithId<long> DeleteInventoryItemContent([FromHeader] long InventoryItemContentId)
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
                    var result = _inventoryItemService.DeleteInventoryItemContent(InventoryItemContentId);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpPost("AddInventoryItemContent")]
        public async Task<BaseResponseWithId<long>> AddInventoryItemContent(AddInventoryItemContentDto request)
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
                    _inventoryItemService.Validation = validation;
                    var result = await _inventoryItemService.AddInventoryItemContent(request);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpPost("UpdateInventoryItemContent")]
        public async Task<BaseResponseWithId<long>> UpdateInventoryItemContent(UpdateInventoryItemContentDto request)
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
                    _inventoryItemService.Validation = validation;
                    var result = await _inventoryItemService.UpdateInventoryItemContent(request);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetInventoryItemList")]
        public InventortyItemListResponse GetInventoryItemList(GetInventoryItemListFilters filters)
        {
            var response = new InventortyItemListResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _inventoryItemService.Validation = validation;
                    var result = _inventoryItemService.GetInventoryItemList(filters);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpPost("DeleteInvenotryItem")]
        public BaseResponseWithId<long> DeleteInvenotryItem([FromHeader] long InventoryItemId, [FromHeader] bool Active)
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
                    _inventoryItemService.Validation = validation;
                    var result = _inventoryItemService.DeleteInvenotryItem(InventoryItemId, Active);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetInventoryStoresIncludeLocationsList")]
        public InventortyStoreIncludeLocationListResponse GetInventoryStoresIncludeLocationsListForBY([FromHeader] long userId)
        {
            var response = new InventortyStoreIncludeLocationListResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _inventoryItemService.Validation = validation;
                    var result = _inventoryItemService.GetInventoryStoresIncludeLocationsListForBY(userId);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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


        [HttpGet("GetInventoryStoreItemLowStockList")]
        public InventortyItemLowStockListResponse GetInventoryStoreItemLowStockList([FromHeader] string SearchKey, [FromHeader] long InventoryStoreID, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10)
        {
            var response = new InventortyItemLowStockListResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _inventoryItemService.Validation = validation;
                    var result = _inventoryItemService.GetInventoryStoreItemLowStockList(SearchKey, InventoryStoreID, CurrentPage, NumberOfItemsPerPage);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetInventoryItem")]
        public GetInventoryItemResponse GetInventoryItem([FromHeader] long InventoryItemID)
        {
            var response = new GetInventoryItemResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _inventoryItemService.Validation = validation;
                    var result = _inventoryItemService.GetInventoryItem(InventoryItemID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetInventoryStore")]
        public async Task<GetInventoryStoreResponse> GetInventoryStore()
        {
            var response = new GetInventoryStoreResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _inventoryItemService.Validation = validation;
                    var result = await _inventoryItemService.GetInventoryStore();
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetInventoryStorePerID")]
        public async Task<GetInventoryStorePerIDResponse> GetInventoryStorePerID([FromHeader] long StoreID)
        {
            var response = new GetInventoryStorePerIDResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _inventoryItemService.Validation = validation;
                    var result = await _inventoryItemService.GetInventoryStorePerID(StoreID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpPost("AddNewInventoryItem")]
        public BaseResponseWithId<long> AddNewInventoryItem([FromForm] AddNewInventoryItemRequest request)
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
                    _inventoryItemService.Validation = validation;
                    var result = _inventoryItemService.AddNewInventoryItem(request);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetInventoryItemRejectedOfferSupplierList")]
        public InventoryItemRejectedOfferSupplierResponse GetInventoryItemRejectedOfferSupplierList([FromHeader] long InventoryItemID, [FromHeader] long POID, [FromHeader] long SupplierID)
        {
            var response = new InventoryItemRejectedOfferSupplierResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _inventoryItemService.Validation = validation;
                    var result = _inventoryItemService.GetInventoryItemRejectedOfferSupplierList(InventoryItemID, POID, SupplierID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetRemainInventoryItemRequestedQty")]
        public GetRemainInventoryItemRequestedQtyResponse GetRemainInventoryItemRequestedQty([FromHeader] long InventoryItemId)
        {
            var response = new GetRemainInventoryItemRequestedQtyResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _inventoryItemService.Validation = validation;
                    var result = _inventoryItemService.GetRemainInventoryItemRequestedQty(InventoryItemId);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetAccountAndFinanceInventoryItemMovementListV2")]
        public BaseResponseWithDataAndHeader<AccountAndFinanceInventoryItemMovementResponse> GetAccountAndFinanceInventoryItemMovementListV2(AccountAndFinanceInventoryItemMovementListV2Filters filters)
        {
            var response = new BaseResponseWithDataAndHeader<AccountAndFinanceInventoryItemMovementResponse>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _inventoryItemService.Validation = validation;
                    var result = _inventoryItemService.GetAccountAndFinanceInventoryItemMovementListV2(filters);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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

        [HttpGet("GetInventoryItemLocationList")]
        public SelectDDLResponse GetInventoryItemLocationList([FromHeader] long InventoryItemId, [FromHeader] int StoreId)
        {
            var response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _inventoryItemService.Validation = validation;
                    var result = _inventoryItemService.GetInventoryItemLocationList(InventoryItemId, StoreId);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(result.Errors);
                        return response;
                    }

                    response = result;
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
