using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.CRM;
using NewGaras.Infrastructure.Models.SalesOffer.Filters;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGarasAPI.Models.User;
using NewGaras.Infrastructure.Models.CRM.Filters;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CRMController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly string key;
        private readonly IWebHostEnvironment _host;
        private readonly ICrmService _crmService;
        private readonly ITenantService _tenantService;

        public CRMController(IWebHostEnvironment host, ITenantService tenantService, ICrmService crmService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            key = "SalesGarasPass";
            _helper = new Helper.Helper();
            _host = host;
            _crmService = crmService;
        }

        [HttpGet("GetMyClientsDetailsCRM")]
        public async Task<MyClientsCRMDashboardResponse> GetMyClientsDetailsCRM(GetMyClientsDetailsCRMFilters filters)
        {
            MyClientsCRMDashboardResponse response = new MyClientsCRMDashboardResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = await _crmService.GetMyClientsDetailsCRM(filters);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetSalesPersonsClientsDetails")]
        public async Task<SalesPersonsClientsDetailsResponse> GetSalesPersonsClientsDetails(GetMyClientsDetailsCRMFilters filters)
        {
            SalesPersonsClientsDetailsResponse response = new SalesPersonsClientsDetailsResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = await _crmService.GetSalesPersonsClientsDetails(filters);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("SalesPersonClientsList")]
        public async Task<SalesPersonClientsListResponse> SalesPersonClientsList([FromHeader] long SalesPersonId, [FromHeader] string SupportedBy, [FromHeader] int Month, [FromHeader] int Year)
        {
            SalesPersonClientsListResponse response = new SalesPersonClientsListResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = await _crmService.SalesPersonClientsList(SalesPersonId, SupportedBy, Month, Year);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("GetProductsSalesPersons")]
        public async Task<SalesPersonsProductResponse> GetProductsSalesPersons([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] long ProductId)
        {
            SalesPersonsProductResponse response = new SalesPersonsProductResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = await _crmService.GetProductsSalesPersons(Month, Year, BranchId, ProductId);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("GetMyOffersDetailsCRM")]
        public async Task<MyOffersCRMDashboardResponse> GetMyOffersDetailsCRM([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] long SalesPersonId)
        {
            MyOffersCRMDashboardResponse response = new MyOffersCRMDashboardResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = await _crmService.GetMyOffersDetailsCRM(Month, Year, BranchId, SalesPersonId);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("GetSalesPersonOffersDetails")]
        public async Task<SalesPersonsOffersDetailsResponse> GetSalesPersonOffersDetails([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] long SalesPersonId)
        {
            SalesPersonsOffersDetailsResponse response = new SalesPersonsOffersDetailsResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = await _crmService.GetSalesPersonOffersDetails(Month, Year, BranchId, SalesPersonId);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("GetMyReportsDetailsCRM")]
        public async Task<MyReportsCRMDashboardResponse> GetMyReportsDetailsCRM(GetMyReportsDetailsCRMFilters filters)
        {
            MyReportsCRMDashboardResponse response = new MyReportsCRMDashboardResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = await _crmService.GetMyReportsDetailsCRM(filters);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("SalesAndCRMReportsDetails")]
        public ClientsSalesAndCrmReportsDetailsResponse SalesAndCRMReportsDetails(SalesAndCRMReportsFilters filters)
        {
            ClientsSalesAndCrmReportsDetailsResponse response = new ClientsSalesAndCrmReportsDetailsResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.SalesAndCRMReportsDetails(filters);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("CRMReportsDetails")]
        public ClientsCrmReportsDetailsResponse CRMReportsDetails(CRMReportsDetailsFilters filters)
        {
            ClientsCrmReportsDetailsResponse response = new ClientsCrmReportsDetailsResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.CRMReportsDetails(filters);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("GetSalesAndCRMAddNewReportScreenData")]
        public SalesAndCRMAddNewReportScreenData GetSalesAndCRMAddNewReportScreenData()
        {
            SalesAndCRMAddNewReportScreenData response = new SalesAndCRMAddNewReportScreenData()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.GetSalesAndCRMAddNewReportScreenData();
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("GetCrmContactTypesDDL")]
        public BaseResponseWithData<List<SelectDDL>> GetCrmContactTypesDDL()
        {
            BaseResponseWithData<List<SelectDDL>> response = new BaseResponseWithData<List<SelectDDL>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.GetCrmContactTypesDDL();
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetCrmRecievedTypesDDL")]
        public BaseResponseWithData<List<SelectDDL>> GetCrmRecievedTypesDDL()
        {
            BaseResponseWithData<List<SelectDDL>> response = new BaseResponseWithData<List<SelectDDL>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.GetCrmRecievedTypesDDL();
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetDailyReportThroughDDL")]
        public BaseResponseWithData<List<SelectDDL>> GetDailyReportThroughDDL()
        {
            BaseResponseWithData<List<SelectDDL>> response = new BaseResponseWithData<List<SelectDDL>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.GetDailyReportThroughDDL();
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GeCrmReportReasonsDDL")]
        public BaseResponseWithData<List<SelectDDL>> GeCrmReportReasonsDDL()
        {
            BaseResponseWithData<List<SelectDDL>> response = new BaseResponseWithData<List<SelectDDL>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.GeCrmReportReasonsDDL();
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpPost("AddNewCRMReport")]
        public BaseResponseWithId<long> AddNewCRMReport(AddNewCRMReport request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.AddNewCRMReport(request,validation.userID);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("AddNewSalesReport")]
        public BaseResponseWithId<long> AddNewSalesReport([FromForm]AddNewSalesReport request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.AddNewSalesReport(request, validation.userID,validation.CompanyName);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("GetSalesReportList")]
        public GetSalesReportsListResponse GetSalesReportList(GetSalesReportFilters filters)
        {
            GetSalesReportsListResponse response = new GetSalesReportsListResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.GetSalesReportList(filters);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("GetSalesReportListStatistics")]
        public GetSalesReportsListStatisticsResponse GetSalesReportListStatistics(GetSalesReportFilters filters)
        {
            GetSalesReportsListStatisticsResponse response = new GetSalesReportsListStatisticsResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.GetSalesReportListStatistics(filters);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpGet("GetSalesReportLinesList")]
        public GetSalesReportLinesListResponse GetSalesReportLinesList([FromHeader] long SalesReportId)
        {
            GetSalesReportLinesListResponse response = new GetSalesReportLinesListResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.GetSalesReportLinesList(SalesReportId);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetMapSalesReportLinesList")]
        public async Task<MapSaleReportLinesResponse> GetMapSalesReportLinesList([FromHeader] long SalesReportId, [FromHeader] long SalesPersonId, [FromHeader] int BranchId, [FromHeader] DateTime? CreationFrom, [FromHeader] DateTime? CreationTo)
        {
            MapSaleReportLinesResponse response = new MapSaleReportLinesResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = await _crmService.GetMapSalesReportLinesList(SalesReportId, SalesPersonId, BranchId, CreationFrom, CreationTo);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpPost("EditSalesReportExpenses")]
        public BaseResponseWithId<long> EditSalesReportExpenses([FromForm]SalesReportLineExpense request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.EditSalesReportExpenses(request, validation.userID, validation.CompanyName);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("DeleteSalesReportExpenses")]
        public BaseResponse DeleteSalesReportExpenses([FromHeader] long SalesReportExpenseId)
        {
            BaseResponse response = new BaseResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.DeleteSalesReportExpenses(SalesReportExpenseId);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("DeleteSalesReportLines")]
        public BaseResponse DeleteSalesReportLines([FromHeader] long SalesReportlineId)
        {
            BaseResponse response = new BaseResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = _crmService.DeleteSalesReportLines(SalesReportlineId);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetCRMReportReasonsList")]
        public async Task<GetCRMReportReasonsResponse> GetCRMReportReasonsList()
        {
            GetCRMReportReasonsResponse response = new GetCRMReportReasonsResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    response = await _crmService.GetCRMReportReasonsList();
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpPost("AddEditCRMReportReasons")]
        public BaseResponseWithId<long> AddEditCRMReportReasons(GetCRMReportReasonsResponseVM request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                _crmService.Validation = validation;
                if (response.Result)
                {
                    response = _crmService.AddEditCRMReportReasons(request);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }


        [HttpPost("CRMReportEditReminderStatus")]
        public BaseResponse CRMReportEditReminderStatus([FromHeader] long CrmReportId, [FromHeader] bool Status)
        {
            BaseResponse response = new BaseResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                _crmService.Validation = validation;
                if (response.Result)
                {
                    response = _crmService.CRMReportEditReminderStatus(CrmReportId, Status);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetCRMReportLineStatisticsPerDate")]
        public GetReportStatiscsGroupbyDateResponse GetCRMReportLineStatisticsPerDate([FromHeader]GetCRMReportLineStatisticsPerDateFilter filters)
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
                    var data = _crmService.GetCRMReportLineStatisticsPerDate(filters);
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

        [HttpGet("GetSalesAndCRMReportLineStatisticsPerDate")]
        public GetSalesAndCRMReportStatiscsGroupbyDateResponse GetSalesAndCRMReportLineStatisticsPerDate([FromHeader]GetSalesAndCRMReportLineStatisticsPerDateFilters filters)
        {
            var response = new GetSalesAndCRMReportStatiscsGroupbyDateResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _crmService.GetSalesAndCRMReportLineStatisticsPerDate(filters);
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
