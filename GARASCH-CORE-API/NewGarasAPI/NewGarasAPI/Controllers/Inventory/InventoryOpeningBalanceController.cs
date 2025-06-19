using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using NewGaras.Domain.Services;
using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.Responses;
using NewGaras.Domain.Models;
using Azure;
using NewGaras.Infrastructure.Models.InventoryItem;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance;

namespace NewGarasAPI.Controllers.Inventory
{
    [Route("[controller]")]
    [ApiController]
    public class InventoryOpeningBalanceController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IInventoryOpeningBalanceService _inventoryOpeningBalanceService;

        public InventoryOpeningBalanceController(IUnitOfWork unitOfWork, IWebHostEnvironment host, ITenantService tenantService, IInventoryOpeningBalanceService inventoryOpeningBalanceService)
        {
            _host = host;
            _unitOfWork = unitOfWork;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _inventoryOpeningBalanceService = inventoryOpeningBalanceService;
        }


        [HttpPost("AddInventoryItemOpeningBalance")]
        public BaseResponseWithId<long> AddInventoryItemOpeningBalance([FromBody] AddInventoryItemOpeningBalanceRequest dto)
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
                    var inventoryItemOpeningBalance = _inventoryOpeningBalanceService.AddInventoryItemOpeningBalance(dto, validation.userID);
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

        [HttpPost("AddInventoryItemOpeningBalanceForLIBMARK")]
        public BaseResponseWithId<long> AddInventoryItemOpeningBalanceForLIBMARK([FromBody] AddInventoryItemOpeningBalanceRequest dto)
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
                    var inventoryItemOpeningBalance = _inventoryOpeningBalanceService.AddInventoryItemOpeningBalanceForLIBMARK(dto, validation.userID);
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


        [HttpPost("AddInventoryItemOpeningBalancePOS")]
        public BaseResponseWithId<long> AddInventoryItemOpeningBalancePOS([FromBody] AddInventoryItemOpeningBalanceRequest dto)
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
                    var inventoryItemOpeningBalance = _inventoryOpeningBalanceService.AddInventoryItemOpeningBalancePOS(dto, validation.userID);
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

        [HttpGet("GetInventoryItemOpeningBalancePOSTemplete")]
        public BaseResponseWithData<string> GetInventoryItemOpeningBalancePOSTemplete()
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
                    var templete = _inventoryOpeningBalanceService.GetInventoryItemOpeningBalancePOSTemplete(validation.CompanyName);
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

                return response;
            }
        }

        [HttpPost("AddInventoryItemOpeningBalancePOSByExcelSheet")]
        public BaseResponseWithMessage<string> UploadInventoryItemExcel([FromForm] AddAttachment dto)
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
            var fileExtension = Path.GetExtension(dto.Content.FileName);
            var allowedExtensions = new List<string>() { ".xlsx", ".xml" };
            if (!allowedExtensions.Contains(fileExtension))
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
                    var uploadedInventoryItems = _inventoryOpeningBalanceService.UploadInventoryItemOpeningBalancePOSExcel(dto, validation.userID);
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

        [HttpPost("EditInventoryItemCost")]
        public BaseResponseWithId<long> UpdateInventoryItemCost([FromBody] UpdateInventoryItemCost Data)
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
                    var inventoryItemOpeningBalance = _inventoryOpeningBalanceService.UpdateInventoryItemCost(Data.inventoryStoreItemID, Data.Cost);
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

    }
}
