using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.SalesTargets;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SalesTargetController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly ISalesTargetsService _salesTargetsService;
        public SalesTargetController(IWebHostEnvironment host, ITenantService tenantService, IPOSService PosService, ISalesTargetsService salesTargetsService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            key = "SalesGarasPass";
            _helper = new Helper.Helper();
            _host = host;
            _salesTargetsService = salesTargetsService;
        }

        [HttpPost("AddEditSalesTarget")]
        public async Task<BaseResponseWithID> AddEditSalesTarget(AddSalesTarget RequestData)
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
                    var salesTarget =await _salesTargetsService.AddEditSalesTarget(RequestData, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(salesTarget.Errors);
                        return response;
                    }

                    response = salesTarget;
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

        [HttpPost("AddEditSalesBranchTarget")]
        public async Task<BaseResponseWithID> AddEditSalesBranchTarget(AddSalesBranchTargetResponse RequestData)
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
                    var salesTarget = await _salesTargetsService.AddEditSalesBranchTarget(RequestData, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(salesTarget.Errors);
                        return response;
                    }

                    response = salesTarget;
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

        [HttpPost("AddEditSalesBranchUserTarget")]
        public async Task<BaseResponseWithID> AddEditSalesBranchUserTarget(AddSalesBranchUserTargetResponse RequestData)
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
                    var salesTarget = await _salesTargetsService.AddEditSalesBranchUserTarget(RequestData, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(salesTarget.Errors);
                        return response;
                    }

                    response = salesTarget;
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
        [HttpPost("AddEditSalesBranchProductTarget")]
        public async Task<BaseResponseWithID> AddEditSalesBranchProductTarget(AddSalesBranchProductTargetResponse RequestData)
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
                    var salesTarget = await _salesTargetsService.AddEditSalesBranchProductTarget(RequestData, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(salesTarget.Errors);
                        return response;
                    }

                    response = salesTarget;
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

        [HttpGet("GetLastSalesTargetList")]
        public async Task<GetSalesTargetListResponse> GetLastSalesTargetList([FromHeader]int? filterYear)
        {
            var response = new GetSalesTargetListResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var salesTarget = await _salesTargetsService.GetLastSalesTargetList(filterYear);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(salesTarget.Errors);
                        return response;
                    }

                    response = salesTarget;
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

        [HttpGet("GetSalesBranchTargetList")]
        public async Task<GetSalesBranchTargetResponse> GetSalesBranchTargetList([FromHeader]int TargetId)
        {
            var response = new GetSalesBranchTargetResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var salesTarget = await _salesTargetsService.GetSalesBranchTargetList(TargetId);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(salesTarget.Errors);
                        return response;
                    }

                    response = salesTarget;
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

        [HttpGet("GetSalesBranchUserTargetList")]
        public async Task<GetSalesBranchUserTargetResponse> GetSalesBranchUserTargetList([FromHeader]int TargetId, [FromHeader]int? BranchId)
        {
            var response = new GetSalesBranchUserTargetResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var salesTarget = await _salesTargetsService.GetSalesBranchUserTargetList(TargetId, BranchId);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(salesTarget.Errors);
                        return response;
                    }

                    response = salesTarget;
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

        [HttpGet("GetSalesBranchProductTargetList")]
        public async Task<GetSalesBranchProductTargetResponse> GetSalesBranchProductTargetList([FromHeader]int TargetId, [FromHeader] int? BranchId)
        {
            var response = new GetSalesBranchProductTargetResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var salesTarget = await _salesTargetsService.GetSalesBranchProductTargetList(TargetId, BranchId);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(salesTarget.Errors);
                        return response;
                    }

                    response = salesTarget;
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

        [HttpGet("GetTopSellingAndProfitProductsList")]
        public async Task<TopSellingAndProfitProductsResponse> GetTopSellingAndProfitProductsList([FromHeader] int? TargetYear, [FromHeader] int? StartYear)
        {
            var response = new TopSellingAndProfitProductsResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var salesTarget = await _salesTargetsService.GetTopSellingAndProfitProductsList(TargetYear, StartYear);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(salesTarget.Errors);
                        return response;
                    }

                    response = salesTarget;
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

        [HttpGet("GetTargetList")]
        public async Task<GetSalesTargetDDLResponse> GetTargetList()
        {
            var response = new GetSalesTargetDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var salesTarget = await _salesTargetsService.GetTargetList();
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(salesTarget.Errors);
                        return response;
                    }

                    response = salesTarget;
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
