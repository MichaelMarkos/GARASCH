using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Models.InventoryStoreReports;
using NewGaras.Infrastructure.Models;

namespace NewGarasAPI.Controllers.Inventory
{
    [Route("Inventory/[controller]")]
    [ApiController]
    public class InventoryStoreReportsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IInventoryStoreReportsService _inventoryStoreReportsService;
        public InventoryStoreReportsController(IUnitOfWork unitOfWork, IWebHostEnvironment host, ITenantService tenantService, IInventoryItemMatrialAddingAndExternalOrderService inventoryItemMatrialAddingAndExternalOrderService, IInventoryStoreReportsService inventoryStoreReportsService)
        {
            _host = host;
            _unitOfWork = unitOfWork;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _inventoryStoreReportsService = inventoryStoreReportsService;

        }

        [HttpGet("GetInventoryStoreReportList")]
        public async Task<GetInventoryStoreReportResponse> GetInventoryStoreReportList() 
        {
            var response = new GetInventoryStoreReportResponse()
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
                    var req =await _inventoryStoreReportsService.GetInventoryStoreReportList();
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

        [HttpGet("GetInventoryStoreReportItemList")]
        public async Task<GetInventoryStoreReportItemResponse> GetInventoryStoreReportItemList([FromHeader]long ReportID)
        {
            var response = new GetInventoryStoreReportItemResponse()
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
                    var req =await _inventoryStoreReportsService.GetInventoryStoreReportItemList(ReportID);
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

        [HttpPost("AddInventoryStoreReport")]
        public async Task<BaseResponseWithID> AddInventoryStoreReport([FromForm]AddInventoryStoreReportRequest data)
        {
            var response = new BaseResponseWithID()
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
                    var req = await _inventoryStoreReportsService.AddInventoryStoreReport(data, validation.userID, validation.CompanyName);
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

        [HttpPost("UpdateInventoryStoreReportItem")]
        public async Task<BaseResponse> UpdateInventoryStoreReportItem([FromBody]UpdateInventoryStoreReportItemRequest data)
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
                    var req = await _inventoryStoreReportsService.UpdateInventoryStoreReportItem(data, validation.userID);
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

        [HttpPost("UpdateReportToInvStoreItem")]
        public async Task<BaseResponse> UpdateReportToInvStoreItem([FromBody]UpdateInventoryStoreReportItemRequest data)
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
                    var req = await _inventoryStoreReportsService.UpdateReportToInvStoreItem(data, validation.userID);
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

        [HttpPost("UpdateInventoryStoreReportOneItem")]
        public async Task<BaseResponse> UpdateInventoryStoreReportOneItem(UpdateInventoryStoreReportOneItemRequest data)
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
                    var req = await _inventoryStoreReportsService.UpdateInventoryStoreReportOneItem(data, validation.userID);
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
