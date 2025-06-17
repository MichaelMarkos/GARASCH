using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Contract;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Maintenance;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGaras.Infrastructure.Models.SalesOffer.Filters;
using NewGarasAPI.Models.Account;

namespace NewGarasAPI.Controllers.Offer
{
    [Route("Offer/[controller]")]
    [ApiController]
    public class SalesOfferController : ControllerBase
    {
        private readonly ISalesOfferService _salesOfferService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IInternalTicketService _internalTicketService;
        private readonly IMedicalService _medicalService;
        public SalesOfferController(ISalesOfferService salesOfferService, ITenantService tenantService, IInternalTicketService internalTicketService,IMedicalService medicalService)
        {
            _salesOfferService = salesOfferService;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _internalTicketService = internalTicketService;
            _medicalService = medicalService;
        }

        [HttpGet("GetSalesOfferReport")]
        public BaseResponseWithData<string> GetSalesOfferReport([FromHeader] SalesOfferReportFilter filter)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.SalesOfferReport(filter, validation.CompanyName);

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

        [HttpGet("GetClientOrderRate")]
        public BaseResponseWithData<string> GetClientOrdeRate(SalesOfferReportFilter filters)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.GetClientOrdeRate(filters, validation.CompanyName);

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

        [HttpGet("GetSalesOfferVehicleReport")]
        public BaseResponseWithData<string> GetSalesOfferVehicleReport([FromHeader] SalesOfferReportFilter filter)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.SalesOfferReportForVehicle(filter, validation.CompanyName);

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

        [HttpGet("GetSalesOfferItemsCategoryReport")]
        public BaseResponseWithData<string> SalesOfferItemsCategoryReport([FromHeader] int Year, [FromHeader] int Month, [FromHeader] long SalesPersonId)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.SalesOfferItemsCategoryReport(validation.CompanyName,Year,Month,SalesPersonId);

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

        [HttpGet("ValidateProductsPrices")]
        public BaseResponseWithData<bool> ValidateProductsPrices(List<OfferProductValidation> OfferProducts)
        {
            BaseResponseWithData<bool> response = new BaseResponseWithData<bool>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.ValidateProductsPrices(OfferProducts);

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

        [HttpPost("UpdateProductsPricesInDB")]
        public BaseResponseWithData<bool> UpdateProductsPricesInDB([FromHeader] long OfferID)
        {
            BaseResponseWithData<bool> response = new BaseResponseWithData<bool>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.UpdateProductsPricesInDB(OfferID);

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

        [HttpGet("GetClientFromSalesOfferReport")]
        public BaseResponseWithData<string> GetClientFromSalesOfferReport([FromHeader] AccountSalesOfferReportFilter filters)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.GetClientFromSalesOfferReport(filters, validation.CompanyName);

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
        [HttpGet("GetAllClientsAccumulativeByMonths")]
        public BaseResponseWithData<string> GetAllClientsAccumulativeByMonths(AccountSalesOfferReportFilter filters)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _salesOfferService.Validation = validation;
                    response = _salesOfferService.GetAllClientsAccumulativeByMonths(filters);

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

        [HttpGet("GetAllSuppliersAccumulativeByMonths")]
        public BaseResponseWithData<string> GetAllSuppliersAccumulativeByMonths(AccountSalesOfferReportFilter filters)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _salesOfferService.Validation = validation;
                    response = _salesOfferService.GetAllSuppliersAccumulativeByMonths(filters);

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

        [HttpGet("GetSupplierFromSalesOfferReport")]
        public BaseResponseWithData<string> GetSupplierFromSalesOfferReport([FromHeader] AccountSalesOfferReportFilter filters)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.GetSupplierFromSalesOfferReport(filters, validation.CompanyName);

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

        [HttpGet("GetAccountAmountByAdvancedType")]
        public BaseResponseWithData<string> GetAccountAmountByAdvancedType([FromHeader] int Year, [FromHeader] bool ByEntryDate, [FromHeader] long AccountCategoryId, [FromHeader] long? AdvancedTypeId, [FromHeader] int? BranchId)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.GetAccountAmountByAdvancedType(validation.CompanyName, Year, ByEntryDate, AccountCategoryId, AdvancedTypeId, BranchId);

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


        [HttpGet("GetAccountAmountByCategoryName")]
        public BaseResponseWithData<string> GetAccountAmountByCategoryName([FromHeader] int Year, [FromHeader] bool ByEntryDate, [FromHeader] int? BranchId, [FromHeader] string AccountIds)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.GetAccountAmountByCategoryName(validation.CompanyName, Year, ByEntryDate, BranchId, AccountIds);

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

        [HttpGet("GetSalesOfferList")]
        public GetSalesOfferListResponse GetSalesOfferList([FromHeader] GetSalesOfferListFilters filters)
        {
            GetSalesOfferListResponse response = new GetSalesOfferListResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.GetSalesOfferList(filters, filters.OfferStatus);

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

        [HttpGet("GetSalesOfferDetails")]
        public GetSalesOfferDetailsResponse GetSalesOfferDetails([FromHeader] long SalesOfferId)
        {
            GetSalesOfferDetailsResponse response = new GetSalesOfferDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.GetSalesOfferDetails(SalesOfferId);

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

        [HttpGet("GetSalesOfferProductsDetails")]
        public GetSalesOfferProductsDetailsResponse GetSalesOfferProductsDetails([FromHeader] long SalesOfferId)
        {
            GetSalesOfferProductsDetailsResponse response = new GetSalesOfferProductsDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.GetSalesOfferProductsDetails(SalesOfferId);

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

        [HttpGet("GetOfferInventoryItemInfo")]
        public GetOfferInventoryItemResponse GetOfferInventoryItemInfo([FromHeader] long InventoryItemId)
        {
            GetOfferInventoryItemResponse response = new GetOfferInventoryItemResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.GetOfferInventoryItemInfo(InventoryItemId);

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

        [HttpGet("GetBOMInventoryItemsInfo")]
        public GetBOMInventoryItemsResponse GetBOMInventoryItemsInfo([FromHeader] long BOMId)
        {
            GetBOMInventoryItemsResponse response = new GetBOMInventoryItemsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _salesOfferService.GetBOMInventoryItemsInfo(BOMId);

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


        [HttpGet("GetSalesOfferItemSellingHistory")]
        public GetSalesOfferItemSellingHistoryResponse GetSalesOfferItemSellingHistory([FromHeader] long InventoryItemId)
        {
            {
                GetSalesOfferItemSellingHistoryResponse response = new GetSalesOfferItemSellingHistoryResponse();
                response.Result = true;
                response.Errors = new List<Error>();
                try
                {
                    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                    response.Errors = validation.errors;
                    response.Result = validation.result;
                    if (response.Result)
                    {
                        response = _salesOfferService.GetSalesOfferItemSellingHistory(InventoryItemId);

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

        [HttpGet("GetOfferInventoryItemsList")]
        public GetOfferInventoryItemsListResponse GetOfferInventoryItemsList([FromHeader]GetOfferInventoryItemsListFilters filters)
        {
            {
                GetOfferInventoryItemsListResponse response = new GetOfferInventoryItemsListResponse();
                response.Result = true;
                response.Errors = new List<Error>();
                try
                {
                    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                    response.Errors = validation.errors;
                    response.Result = validation.result;
                    if (response.Result)
                    {
                        response = _salesOfferService.GetOfferInventoryItemsList(filters);

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
        [HttpGet("GetOfferExtraCostTypesList")]
        public GetOfferExtraCostTypesListResponse GetOfferExtraCostTypesList()
        {
            {
                GetOfferExtraCostTypesListResponse response = new GetOfferExtraCostTypesListResponse();
                response.Result = true;
                response.Errors = new List<Error>();
                try
                {
                    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                    response.Errors = validation.errors;
                    response.Result = validation.result;
                    if (response.Result)
                    {
                        response = _salesOfferService.GetOfferExtraCostTypesList();

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

        [HttpGet("GetOfferTaxTypesList")]
        public GetOfferTaxListResponse GetOfferTaxTypesList()
        {
            {
                GetOfferTaxListResponse response = new GetOfferTaxListResponse();
                response.Result = true;
                response.Errors = new List<Error>();
                try
                {
                    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                    response.Errors = validation.errors;
                    response.Result = validation.result;
                    if (response.Result)
                    {
                        response = _salesOfferService.GetOfferTaxTypesList();

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

        [HttpGet("GetOfferTermsAndConditionsList")]
        public GetOfferTermsAndConditionsListResponse GetOfferTermsAndConditionsList()
        {
            {
                GetOfferTermsAndConditionsListResponse response = new GetOfferTermsAndConditionsListResponse();
                response.Result = true;
                response.Errors = new List<Error>();
                try
                {
                    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                    response.Errors = validation.errors;
                    response.Result = validation.result;
                    if (response.Result)
                    {
                        response = _salesOfferService.GetOfferTermsAndConditionsList();

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


        [HttpGet("GetSalesOfferListDDLData")]
        public async Task<GetSalesOfferListDDLResponse> GetSalesOfferListDDLData([FromHeader]long ClientId, [FromHeader]string SearchKey, [FromHeader]bool? StatusIsOpenFilter)
        {
            {
                GetSalesOfferListDDLResponse response = new GetSalesOfferListDDLResponse();
                response.Result = true;
                response.Errors = new List<Error>();
                try
                {
                    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                    response.Errors = validation.errors;
                    response.Result = validation.result;
                    if (response.Result)
                    {
                        response = await _salesOfferService.GetSalesOfferListDDLData(ClientId, SearchKey, StatusIsOpenFilter);

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

        [HttpGet("GetSalesOfferProductListDDLData")]
        public GetSalesOfferProductListDDLResponse GetSalesOfferProductListDDLData([FromHeader]long SalesOfferId)
        {
            {
                GetSalesOfferProductListDDLResponse response = new GetSalesOfferProductListDDLResponse();
                response.Result = true;
                response.Errors = new List<Error>();
                try
                {
                    HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                    response.Errors = validation.errors;
                    response.Result = validation.result;
                    if (response.Result)
                    {
                        response =  _salesOfferService.GetSalesOfferProductListDDLData(SalesOfferId);

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

        [HttpGet("RejectedOfferScreenData")]
        public async Task<RejectedOfferScreenDataResponse> RejectedOfferScreenData([FromHeader]long? POID, [FromHeader]long? PRID)
        {
            var response = new RejectedOfferScreenDataResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = await _salesOfferService.RejectedOfferScreenData(POID, PRID);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpPost("AddNewSalesOffer")]
        public BaseResponseWithId<long> AddNewSalesOffer(AddNewSalesOfferData request)
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
                    var data = _salesOfferService.AddNewSalesOffer(request, validation.userID,validation.CompanyName);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpPost("AddNewSalesOfferForInternalTicket")]
        public BaseResponseWithId<long> AddNewSalesOfferForInternalTicket([FromBody] AddNewSalesOfferForInternalTicketRequest request)
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
                    var uploadedInventoryItems = _salesOfferService.AddNewSalesOfferForInternalTicket(request, validation.CompanyName, validation.userID);
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


        [HttpPost("ClosingSalesOffer")]
        public BaseResponseWithId<long> ClosingSalesOffer(ClosingSalesOfferData request)
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
                    _salesOfferService.Validation = validation; 
                    var data = _salesOfferService.ClosingSalesOffer(request, validation.CompanyName);
                    if (data != null)
                    {
                        response = data;
                    }
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

        //Not Used
        [HttpGet("GetSalesOffersDashboard")]
        public GetSalesOfferDashboardResponse GetSalesOffersDashboard()
        {
            var response = new GetSalesOfferDashboardResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _salesOfferService.GetSalesOffersDashboard();
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("GetSalesOfferInternalApproval")]
        public GetSalesOfferInternalApprovalResponse GetSalesOfferInternalApproval([FromHeader] long SalesOfferId)
        {
            var response = new GetSalesOfferInternalApprovalResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _salesOfferService.GetSalesOfferInternalApproval(SalesOfferId);
                    if (data != null)
                    {
                        response = data;
                    }
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


        [HttpPost("AddSalesOfferPricingDetails")]
        public BaseResponseWithId<long> AddSalesOfferPricingDetails(AddNewSalesPricingDetailsData request)
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
                    var data = _salesOfferService.AddSalesOfferPricingDetails(request);
                    if (data != null)
                    {
                        response = data;
                    }
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


        [HttpPost("AddSalesOfferInternalApproval")]
        public BaseResponseWithId<long> AddSalesOfferInternalApproval(AddNewSalesOfferInternalApprovalData request)
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
                    _salesOfferService.Validation = validation;
                    var data = _salesOfferService.AddSalesOfferInternalApproval(request);
                    if (data != null)
                    {
                        response = data;
                    }
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


        [HttpPost("RejectClosedSalesOffer")]
        public BaseResponseWithId<long> RejectClosedSalesOffer(RejectClosedSalesOfferData request)
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
                    var data = _salesOfferService.RejectClosedSalesOffer(request,validation.userID);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("GetSalesPersonSalesOfferList")]
        public GetSalesPersonSalesOfferListResponse GetSalesPersonSalesOfferList(GetSalesPersonSalesOfferListFilters filters, [FromHeader]string OfferStatusParam)
        {
            var response = new GetSalesPersonSalesOfferListResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _salesOfferService.GetSalesPersonSalesOfferList(filters, OfferStatusParam);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("GetTargetOfLast5Years")]
        public async Task<GetTargetOfLast5YearsResponse> GetTargetOfLast5Years()
        {
            GetTargetOfLast5YearsResponse response = new GetTargetOfLast5YearsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _salesOfferService.GetTargetOfLast5Years();
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        [HttpPost("TargetNextYearDetails")]
        public BaseResponseWithId<long> TargetNextYearDetails(TargetNextYearDetailsResponse request)
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
                    var data = _salesOfferService.TargetNextYearDetails(request, validation.userID);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("SalesOfferAchievedTarget")]
        public async Task<SalesOfferAchievedTargetResponse> SalesOfferAchievedTarget(SalesOfferAchievedTargetFilters filters)
        {
            SalesOfferAchievedTargetResponse response = new SalesOfferAchievedTargetResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _salesOfferService.SalesOfferAchievedTarget(filters);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        [HttpGet("GetSalesOfferDetailsPDF")]
        public async Task<BaseMessageResponse> GetSalesOfferDetailsPDF([FromHeader] long SalesOfferId)
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
                    _salesOfferService.Validation = validation;
                    response = await _salesOfferService.GetSalesOfferDetailsPDF(SalesOfferId);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        [HttpGet("GetInvoiceDetailsPDF")]
        public async Task<BaseMessageResponse> GetInvoiceDetailsPDF([FromHeader] long SalesOfferId)
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
                    _salesOfferService.Validation = validation;
                    response = await _salesOfferService.GetInvoiceDetailsPDF(SalesOfferId);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        [HttpGet("GetSalesOfferListDDLForRelease")]
        public async Task<GetSalesOfferListDDLForReleaseResponse> GetSalesOfferListDDLForRelease([FromHeader]long ClientId, [FromHeader] string SearchKey, [FromHeader] bool? StatusIsOpenFilter, [FromHeader]int CurrentPage, [FromHeader]int NumberOfItemsPerPage)
        {
            var  response = new GetSalesOfferListDDLForReleaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _salesOfferService.Validation = validation;
                    response = await _salesOfferService.GetSalesOfferListDDLForRelease(ClientId, SearchKey, StatusIsOpenFilter, CurrentPage, NumberOfItemsPerPage);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        [HttpGet("GetRejectedOffer")]
        public async Task<GetRejectedOfferResponse> GetRejectedOffer([FromHeader] GetRejectedOfferFilters filters)
        {
            var response = new GetRejectedOfferResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = await _salesOfferService.GetRejectedOffer(filters);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpPost("AddEditRejectedSupplierOffer")]
        public async Task<BaseResponseWithID> AddEditRejectedSupplierOffer([FromBody] AddEditSupplierOfferResponse RequestData)
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
                    var data = await _salesOfferService.AddEditRejectedSupplierOffer(RequestData, validation.userID);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("GetSalesReportLineStatisticsPerDate")]
        public GetReportStatiscsGroupbyDateResponse GetSalesReportLineStatisticsPerDate([FromHeader] GetSalesReportLineStatisticsPerDateFilters filters)
        {
            var response = new GetReportStatiscsGroupbyDateResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _salesOfferService.GetSalesReportLineStatisticsPerDate(filters);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("SalesReportsDetails")]
        public ClientsSalesReportsDetailsResponse SalesReportsDetails([FromHeader] SalesReportsDetailsFilters filters)
        {
            var response = new ClientsSalesReportsDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _salesOfferService.SalesReportsDetails(filters);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("GetPrOfferItemHistory")]
        public async Task<GetPrOfferItemHistoryResponse> GetPrOfferItemHistory([FromHeader] long? InventoryItemId)
        {
            var response = new GetPrOfferItemHistoryResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = await _salesOfferService.GetPrOfferItemHistory(InventoryItemId);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("GetSalesPersonsClientsDetailsExcel")]
        public async Task<BaseMessageResponse> GetSalesPersonsClientsDetailsExcel([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] bool WithProjectExtraModifications)
        {
            var response = new BaseMessageResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _salesOfferService.Validation = validation;
                    var data = await _salesOfferService.GetSalesPersonsClientsDetailsExcel(Month, Year, BranchId, WithProjectExtraModifications);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("GetSalesOfferDueClientPOS")]
        public BaseResponseWithData<string> GetSalesOfferDueClientPOS([FromHeader] string DateFrom, [FromHeader] string DateTo)
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
                    var data = _salesOfferService.GetSalesOfferDueClientPOS(DateFrom, DateTo, validation.CompanyName);
                    if (data != null)
                    {
                        response = data;
                    }
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

        [HttpGet("GetSalesOfferDueForStore")]
        public BaseResponseWithData<string> GetSalesOfferDueForStore([FromHeader] string ForDate, [FromHeader] bool type)
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
                    var data = _salesOfferService.GetSalesOfferDueForStore(ForDate, validation.CompanyName, type);
                    if (data != null)
                    {
                        response = data;
                    }
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

        //[HttpGet("GetSalesOfferTicketsForStore")]
        //public BaseResponseWithData<string> GetSalesOfferTicketsForStore([FromHeader] string from, [FromHeader] string to, [FromHeader] string OfferType, [FromHeader] string OfferTypeReturn,[FromHeader] string UserId)
        //{
        //    var response = new BaseResponseWithData<string>();
        //    response.Result = true;
        //    response.Errors = new List<Error>();

        //    try
        //    {

        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //        response.Errors = validation.errors;
        //        response.Result = validation.result;

        //        if (response.Result)
        //        {
        //            var data = _medicalService.GetSalesOfferForStoreExcel(from, to, UserId, validation.CompanyName,OfferType,OfferTypeReturn, validation.userID);
        //            if (data != null)
        //            {
        //                response = data;
        //            }
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
        //        response.Errors.Add(error);

        //        return response;
        //    }
        //}

        [HttpGet("GetSalesOfferTicketsForStore")]
        public BaseResponseWithData<string> GetSalesOfferTicketsForStore([FromHeader] string from, [FromHeader] string to, [FromHeader] string UserId)
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
                    var data = _internalTicketService.GetSalesOfferTicketsForStore(from, to, UserId, validation.CompanyName, validation.userID);
                    if (data != null)
                    {
                        response = data;
                    }
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


        [HttpGet("GetTotalAmountForEachCategory")]
        public BaseResponseWithData<string> GetTotalAmountForEachCategory(GetTotalAmountForEachCategoryFilters filters, string CompName)
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
                    var data = _salesOfferService.GetTotalAmountForEachCategory(filters, validation.CompanyName);
                    if (data != null)
                    {
                        response = data;
                    }
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


        [HttpGet("SalesOfferExcel")]
        public async Task<BaseMessageResponse> SalesOfferExcel(SalesOfferExcelfilters filter)
        {
            var response = new BaseMessageResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    _salesOfferService.Validation = validation;
                    var data = await _salesOfferService.SalesOfferExcel(filter);
                    if (data != null)
                    {
                        response = data;
                    }
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
