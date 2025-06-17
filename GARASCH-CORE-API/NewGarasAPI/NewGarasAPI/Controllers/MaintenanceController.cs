using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Domain.Models.TaskManager;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Maintenance;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.TaskManager;
using NewGarasAPI.Models.User;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly string key;
        private readonly ITenantService _tenantService;
        private readonly IWebHostEnvironment _host;
        private readonly IMapper _mapper;
        private readonly IMaintenanceAndService _maintenanceAndService;
        
        public MaintenanceController(IWebHostEnvironment host, IMapper mapper,ITenantService tenantService,IMaintenanceAndService maintenanceAndService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            key = "SalesGarasPass";
            _helper = new Helper.Helper();
            _host = host;
            _mapper = mapper;
            _maintenanceAndService = maintenanceAndService;
        }

        [HttpGet("GetMaintenanceByDay")]
        public async Task<VisitsScheduleMaintenanceByDayResponse> GetMaintenanceByDay([FromHeader] GetMaintenanceByDayFilters filters)
        {
            VisitsScheduleMaintenanceByDayResponse Response = new VisitsScheduleMaintenanceByDayResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _maintenanceAndService.GetMaintenanceByDay(filters, validation.CompanyName);
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

        [HttpGet("GetMaintenanceByArea")]
        public async Task<VisitsScheduleMaintenanceByAreaResponse> GetMaintenanceByArea(GetMaintenanceByAreaFilters filters)
        {
            VisitsScheduleMaintenanceByAreaResponse Response = new VisitsScheduleMaintenanceByAreaResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _maintenanceAndService.GetMaintenanceByArea(filters);
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
            
        [HttpGet("GetMaintenanceByMonth")]
        public async Task<VisitsScheduleMaintenanceByYearResponse> GetMaintenanceByMonth([FromHeader] string SearchKey, [FromHeader] int Year = 0)
        {
            VisitsScheduleMaintenanceByYearResponse Response = new VisitsScheduleMaintenanceByYearResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _maintenanceAndService.GetMaintenanceByMonth(SearchKey,Year);
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

        [HttpGet("GetMaintenanceByID")]
        public async Task<GetMaintenanceForByIDResponse> GetMaintenanceByID([FromHeader] int ID = 0)
        {
            GetMaintenanceForByIDResponse Response = new GetMaintenanceForByIDResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _maintenanceAndService.GetMaintenanceByID(validation.CompanyName, ID);
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

        [HttpGet("GetMaintenanceByClient")]
        public async Task<GetMaintenanceForByClientResponse> GetMaintenanceByClient([FromHeader] int ClientID = 0)
        {
            GetMaintenanceForByClientResponse Response = new GetMaintenanceForByClientResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _maintenanceAndService.GetMaintenanceByClient(ClientID);
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

        [HttpPost("AddMaintenanceFor")]
        public async Task<BaseResponseWithID> AddMaintenanceFor(MaintenanceForData request)
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
                    Response = await _maintenanceAndService.AddMaintenanceFor(request,validation.userID);
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

        [HttpPost("DeleteMaintenanceFor")]
        public async Task<BaseResponseWithID> DeleteMaintenanceFor(DeleteMaintenanceRequest request)
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
                    Response = await _maintenanceAndService.DeleteMaintenanceFor(request);
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

        [HttpGet("GetMaintenanceForDetailsList")]
        public async Task<MaintenanceForDetailsResponse> GetMaintenanceForDetailsList(MaintenanceDetailsListCallFilters filters)
        {
            MaintenanceForDetailsResponse Response = new MaintenanceForDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _maintenanceAndService.GetMaintenanceForDetailsList(filters,validation.CompanyName);
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

        [HttpGet("GetManagementOfMaintenanceOrderByID")]
        public async Task<GetManagementOfMaintenanceOrder> GetManagementOfMaintenanceOrderByID([FromHeader]int MaintenanceForID)
        {
            GetManagementOfMaintenanceOrder Response = new GetManagementOfMaintenanceOrder();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var data = await _maintenanceAndService.GetManagementOfMaintenanceOrderByID(MaintenanceForID);
                    if (!Response.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(data.Errors);
                        return Response;
                    }
                    Response = data;
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

        [HttpPost("AddManagementOfMaintenanceOrder")]
        public async Task<BaseResponseWithID> AddManagementOfMaintenanceOrder([FromForm]ManagementOfMaintenanceOrderData dto)
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
                    var data = await _maintenanceAndService.AddManagementOfMaintenanceOrder(dto, validation.userID, validation.CompanyName);
                    if(!Response.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(data.Errors);
                        return Response;
                    }
                    Response = data;
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


        [HttpGet("MaintenanceDetailsListCallExcel")]
        public async Task<BaseMessageResponse> MaintenanceDetailsListCallExcel(MaintenanceDetailsListCallFilters filters)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.MaintenanceDetailsListCallExcel(filters, validation.CompanyName);
                    }
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

        [HttpGet("MaintenanceContractDetailsListExcel")]
        public async Task<BaseResponseWithData<string>> MaintenanceContractDetailsListExcel(MaintenanceContractDetailsListFilters filters, [FromHeader]string CompanyName)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.MaintenanceContractDetailsListExcel(filters, validation.CompanyName);
                    }
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


        [HttpGet("MaintenanceDetailsListCall")]
        public async Task<MaintenanceForDetailsResponse> MaintenanceDetailsListCall(MaintenanceDetailsListCallFilters filters)
        {
            MaintenanceForDetailsResponse Response = new MaintenanceForDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.MaintenanceDetailsListCall(filters, validation.CompanyName);
                    }
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


        [HttpGet("GetMaintenanceList")]
        public MaintenanceProductResponse GetMaintenanceList([FromHeader] string Serial)
        {
            MaintenanceProductResponse Response = new MaintenanceProductResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.GetMaintenanceList(Serial);
                    }
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


        [HttpGet("GetVisitsScheduleOfMaintenanceByID")]
        public async Task<GetVisitsScheduleOfMaintenanceResponse> GetVisitsScheduleOfMaintenanceByID([FromHeader] int ManagementMaintenanceOrderID)
        {
            GetVisitsScheduleOfMaintenanceResponse Response = new GetVisitsScheduleOfMaintenanceResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response =await _maintenanceAndService.GetVisitsScheduleOfMaintenanceByID(ManagementMaintenanceOrderID);
                    }
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


        [HttpGet("GetPreviousVisitsList")]
        public async Task<GetVisitsScheduleOfMaintenanceResponse> GetPreviousVisitsList([FromHeader] int MaintenanceForID)
        {
            GetVisitsScheduleOfMaintenanceResponse Response = new GetVisitsScheduleOfMaintenanceResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.GetPreviousVisitsList(MaintenanceForID);
                    }
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

        [HttpGet("GetVisitsReportDetailsList")]
        public async Task<GetVisitsMaintenanceReportDetailsResponse> GetVisitsReportDetailsList([FromHeader] long MaintVisitID)
        {
            GetVisitsMaintenanceReportDetailsResponse Response = new GetVisitsMaintenanceReportDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.GetVisitsReportDetailsList(MaintVisitID);
                    }
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


        [HttpGet("GetMaintenanceReportExpensesDetailsList")]
        public async Task<GetMaintenanceReportExpensesDetailsResponse> GetMaintenanceReportExpensesDetailsList([FromHeader] long MaintenanceReportId)
        {
            GetMaintenanceReportExpensesDetailsResponse Response = new GetMaintenanceReportExpensesDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.GetMaintenanceReportExpensesDetailsList(MaintenanceReportId);
                    }
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


        [HttpGet("GetMaintenanceVisitWithoutContract")]
        public async Task<GetMaintenanceVisitsWithoutContractResponse> GetMaintenanceVisitWithoutContract([FromHeader] long MaintenanceVisitId)
        {
            GetMaintenanceVisitsWithoutContractResponse Response = new GetMaintenanceVisitsWithoutContractResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.GetMaintenanceVisitWithoutContract(MaintenanceVisitId);
                    }
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


        [HttpGet("VisitsScheduleOfMaintenanceWithoutContract")]
        public async Task<VisitsScheduleOfMaintenanceWithoutContractResponse> VisitsScheduleOfMaintenanceWithoutContract([FromHeader] long ClientID)
        {
            VisitsScheduleOfMaintenanceWithoutContractResponse Response = new VisitsScheduleOfMaintenanceWithoutContractResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.VisitsScheduleOfMaintenanceWithoutContract(ClientID);
                    }
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


        [HttpGet("GetVisitsScheduleOfMaintenanceWithoutContractByMaintenanceForID")]
        public async Task<GetVisitsScheduleOfMaintenanceResponse> GetVisitsScheduleOfMaintenanceWithoutContractByMaintenanceForID([FromHeader] int MaintenanceForID)
        {
            GetVisitsScheduleOfMaintenanceResponse Response = new GetVisitsScheduleOfMaintenanceResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.GetVisitsScheduleOfMaintenanceWithoutContractByMaintenanceForID(MaintenanceForID);
                    }
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


        [HttpGet("GetAllMaintenanceTypes")]
        public GetAllMaintenanceTypesResponse GetAllMaintenanceTypes([FromHeader] int VehicleModelId, [FromHeader] int RateId, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10)
        {
            GetAllMaintenanceTypesResponse Response = new GetAllMaintenanceTypesResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.GetAllMaintenanceTypes(VehicleModelId,RateId,CurrentPage,NumberOfItemsPerPage);
                    }
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


        [HttpGet("GetMaintenanceTypeItemData")]
        public GetMaintenanceTypeItemResponse GetMaintenanceTypeItemData([FromHeader] int MaintenanceTypeItemId)
        {
            GetMaintenanceTypeItemResponse Response = new GetMaintenanceTypeItemResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.GetMaintenanceTypeItemData(MaintenanceTypeItemId);
                    }
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


        [HttpGet("GetMaintenanceBrandsList")]
        public async Task<SelectDDLResponse> GetMaintenanceBrandsList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response =await _maintenanceAndService.GetMaintenanceBrandsList();
                    }
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


        [HttpGet("GetWorkerstatistics")]
        public async Task<WorkerStatisticsResponse> GetWorkerstatistics([FromHeader] long AssignToID, [FromHeader] DateTime? VisitDateFrom, [FromHeader] DateTime? VisitDateTo)
        {
            WorkerStatisticsResponse Response = new WorkerStatisticsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.GetWorkerstatistics(AssignToID, VisitDateFrom, VisitDateTo);
                    }
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

        [HttpGet("GetClientMaintenanceDetails")]
        public async Task<ClientMaintenanceDetailsResponse> GetClientMaintenanceDetails([FromHeader] long ClientID)
        {
            ClientMaintenanceDetailsResponse Response = new ClientMaintenanceDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.GetClientMaintenanceDetails(ClientID);
                    }
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


        [HttpGet("GetNearestClientVisitMaintenanceDetails")]
        public NearestClientVisitMaintenanceDetailsResponse GetNearestClientVisitMaintenanceDetails(NearestClientVisitFilters filters)
        {
            NearestClientVisitMaintenanceDetailsResponse Response = new NearestClientVisitMaintenanceDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response =  _maintenanceAndService.GetNearestClientVisitMaintenanceDetails(filters);
                    }
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


        [HttpGet("GetMaintenanceCategoryList")]
        public async Task<SelectDDLResponse> GetMaintenanceCategpryList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.GetMaintenanceCategpryList();
                    }
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


        [HttpPost("AddVisitsScheduleOfMaintenance")]
        public async Task<BaseResponseWithId<long>> AddVisitsScheduleOfMaintenance(AddVisitsScheduleOfMaintenanceRequest request)
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
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.AddVisitsScheduleOfMaintenance(request,validation.userID);
                    }
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


        [HttpPost("DeleteVisitsScheduleOfMaintenance")]
        public async Task<BaseResponse> DeleteVisitsScheduleOfMaintenance(DeleteVisitScheduleOfMaintenanceRequest request)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.DeleteVisitsScheduleOfMaintenance(request);
                    }
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


        [HttpPost("AddVisitsReportDetailsList")]
        public async Task<BaseResponseWithId<long>> AddVisitsReportDetailsList([FromForm]AddVisitsMaintenanceReportDetailsData request)
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
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.AddVisitsReportDetailsList(request,validation.userID,validation.CompanyName);
                    }
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


        [HttpPost("AddMaintenanceReportExpenses")]
        public async Task<BaseResponseWithId<long>> AddMaintenanceReportExpenses([FromForm]AddMaintenanceReportExpensesRequest request)
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
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.AddMaintenanceReportExpenses(request, validation.userID, validation.CompanyName);
                    }
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


        [HttpPost("AddEditMaintenanceVisitsWithoutContract")]
        public async Task<BaseResponseWithId<long>> AddEditMaintenanceVisitsWithoutContract([FromForm] AddVisitsScheduleOfMaintenance request)
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
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.AddEditMaintenanceVisitsWithoutContract(request, validation.userID, validation.CompanyName);
                    }
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

        [HttpPost("UpdateReminderDateVisitOfMaintenance")]
        public async Task<BaseResponseWithId<long>> UpdateReminderDateVisitOfMaintenance(UpdateReminderDateVisitOfMaintenanceRequest request)
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
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.UpdateReminderDateVisitOfMaintenance(request);
                    }
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

        [HttpPost("AddEditMaintenanceType")]
        public BaseResponseWithID AddEditMaintenanceType(AddEditMaintenanceTypeRequest RequestData)
        {
            var Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.AddEditMaintenanceType(RequestData, validation.userID);
                    }
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

        [HttpGet("ViewAllMaintenanceTypes")]
        public ViewAllMaintenanceTypesResponse ViewAllMaintenanceTypes()
        {
            var Response = new ViewAllMaintenanceTypesResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.ViewAllMaintenanceTypes();
                    }
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

        [HttpGet("GetClientsCardsDataResponse")]
        public async Task<GetMaintenanceClientsCardsData> GetClientsCardsDataResponse(GetClientsCardsDataResponseFilters filters)
        {
            var Response = new GetMaintenanceClientsCardsData();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = await _maintenanceAndService.GetClientsCardsDataResponse(filters);
                    }
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

        [HttpPost("DeleteMaintenanceVisit")]
        public BaseResponse DeleteMaintenanceVisit([FromHeader] long VisitId)
        {
            var Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.DeleteMaintenanceVisit(VisitId);
                    }
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);

                return Response;
            }
        }

        [HttpGet("GetVisitScheduleReport")]
        public BaseResponseWithData<string> GetVisitScheduleReport(GetVisitScheduleReportFilters filters)
        {
            var Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        _maintenanceAndService.Validation = validation;
                        Response = _maintenanceAndService.GetVisitScheduleReport(filters);
                    }
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);

                return Response;
            }
        }
        [HttpPost("DeleteManagementOfMaintenance")]
        public BaseResponse DeleteManagementOfMaintenancee([FromHeader] long ContractId, [FromHeader] bool DeleteContract)
        {
            var Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        _maintenanceAndService.Validation = validation;
                        Response = _maintenanceAndService.DeleteManagementOfMaintenancee(ContractId,DeleteContract);
                    }
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);

                return Response;
            }
        }

        [HttpPost("AddSalesOfferForMAintenance")]
        public BaseResponseWithId<long> AddSalesOfferForMAintenance(MaintenanceOfferDTO dto)
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
                    _maintenanceAndService.Validation = validation;
                    var data = _maintenanceAndService.AddSalesOfferForMAintenance(dto);
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

        [HttpPost("UpdateSalesOfferForMAintenance")]
        public BaseResponseWithId<long> UpdateSalesOfferForMAintenance(MaintenanceOfferDTO dto)
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
                    _maintenanceAndService.Validation = validation;
                    var data = _maintenanceAndService.UpdateSalesOfferForMAintenance(dto);
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

        [HttpGet("ProductFabricatorDDL")]
        public BaseResponseWithData<List<SelectDDL>> ProductFabricatorDDL()
        {
            var Response = new BaseResponseWithData<List<SelectDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.ProductFabricatorDDL();
                    }
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

        [HttpGet("GetSalesOfferOfMaintenance")]
        public BaseResponseWithData<MaintenanceOfferDTO> GetSalesOfferOfMaintenance([FromHeader] long OfferId)
        {
            var Response = new BaseResponseWithData<MaintenanceOfferDTO>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.GetSalesOfferOfMaintenance(OfferId);
                    }
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

        [HttpPost("AddSalesOfferProductListForMaintenance")]
        public BaseResponseWithId<long> AddSalesOfferProductList([FromBody] AddSalesOfferProductListForMainenance salesOfferProducts)
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
                    var data = _maintenanceAndService.AddSalesOfferProductList(salesOfferProducts, validation.userID);
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
        [HttpGet("GetSalesOfferOfMaintenanceList")]
        public BaseResponseWithDataAndHeader<List<MaintenanceOfferCardDTO>> GetSalesOfferOfMaintenanceList(GetSalesOfferOfMaintenanceFilters filters)
        {
            var Response = new BaseResponseWithDataAndHeader<List<MaintenanceOfferCardDTO>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.GetSalesOfferOfMaintenanceList(filters);
                    }
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


        [HttpPost("EditSalesOfferProductListForMaintenance")]
        public BaseResponseWithId<long> EditSalesOfferProductList(EditSalesOfferProductListForMainenance salesOfferProducts)
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
                    var data = _maintenanceAndService.EditSalesOfferProductList(salesOfferProducts, validation.userID);
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

        [HttpGet("GetMaintenanceNameList")]
        public BaseResponseWithData<List<string>> GetMaintenanceNameList()
        {
            var Response = new BaseResponseWithData<List<string>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.GetMaintenanceNameList();
                    }
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

        [HttpGet("GetOfferStatusSummary")]
        public BaseResponseWithData<GetOfferStatusSummaryModdel> GetOfferStatusSummary(GetSalesOfferOfMaintenanceFilters filters)
        {
            var Response = new BaseResponseWithData<GetOfferStatusSummaryModdel>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.GetOfferStatusSummary(filters);
                    }
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

        [HttpPost("AddSalesOfferMaintenanceVisits")]
        public BaseResponseWithId<long> AddSalesOfferMaintenanceVisits(AddSalesOfferMaintenanceVisitsDTO dto)
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
                    var data = _maintenanceAndService.AddSalesOfferMaintenanceVisits(dto, validation.userID);
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

        [HttpGet("GetVisitsDatesOfScheduleOfMaintenance")]
        public BaseResponseWithData<List<string>> GetVisitsDatesOfScheduleOfMaintenance([FromHeader]long offerID)
        {
            var Response = new BaseResponseWithData<List<string>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        Response = _maintenanceAndService.GetVisitsDatesOfScheduleOfMaintenance(offerID);
                    }
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
