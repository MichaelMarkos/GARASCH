using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.Vehicle;
using NewGaras.Infrastructure.Models.Vehicle.filters;
using NewGarasAPI.Models.Admin;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IVehicleService _vehicleService;
        public VehicleController(IWebHostEnvironment host, ITenantService tenantService, IVehicleService vehicleService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            key = "SalesGarasPass";
            _helper = new Helper.Helper();
            _host = host;
            _vehicleService = vehicleService;
        }

        [HttpGet("GetVehicleModel")]        
        public async Task<GetVehiclePerBrandResponse> GetVehicleModel([FromHeader] long VehicleBrandId)
        {
            GetVehiclePerBrandResponse response = new GetVehiclePerBrandResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _vehicleService.GetVehicleModel(VehicleBrandId);
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

        [HttpPost("AddEditVehicleModel")]
        public BaseResponseWithId<long> AddEditVehicleModel(VehiclePerBrandData request)
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
                    response = _vehicleService.AddEditVehicleModel(request,validation.userID);
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

        [HttpGet("GetVehiclePerCategory")]
        public async Task<GetVehiclePerCategoryResponse> GetVehiclePerCategory()
        {
            GetVehiclePerCategoryResponse response = new GetVehiclePerCategoryResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _vehicleService.GetVehiclePerCategory();
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


        [HttpPost("AddEditVehiclePerCategory")]
        public BaseResponseWithId<long> AddEditVehiclePerCategory(VehiclePerCategoryData request)
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
                    response = _vehicleService.AddEditVehiclePerCategory(request, validation.userID);
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

        [HttpGet("GetVehicleBrandPerModel")]
        public async Task<GetVehicleBrandPerModelResponse> GetVehicleBrandPerModel()
        {
            GetVehicleBrandPerModelResponse response = new GetVehicleBrandPerModelResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _vehicleService.GetVehicleBrandPerModel();
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

        [HttpPost("AddEditVehicleBrand")]
        public BaseResponseWithId<long> AddEditVehicleBrand(VehicleBrandData request)
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
                    response = _vehicleService.AddEditVehicleBrand(request, validation.userID);
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

        [HttpGet("GetVehicleBrandType")]
        public SelectDDLResponse GetVehicleBrandType()
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vehicleService.GetVehicleBrandType();
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

        [HttpGet("GetVehicleBodyType")]
        public SelectDDLResponse GetVehicleBodyType([FromHeader] long VehicleModelId)
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vehicleService.GetVehicleBodyType(VehicleModelId);
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

        [HttpGet("GetVehicleColorList")]
        public SelectDDLResponse GetVehicleColorList()
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vehicleService.GetVehicleColorList();
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

        [HttpGet("GetVehicleTransmissionList")]
        public SelectDDLResponse GetVehicleTransmissionList()
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vehicleService.GetVehicleTransmissionList();
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

        [HttpGet("GetVehicleWheelsDriveList")]
        public SelectDDLResponse GetVehicleWheelsDriveList()
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _vehicleService.GetVehicleWheelsDriveList();
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

        [HttpGet("GetVehicleServiceCategoryDDL")]
        public async Task<GetVehicleCategoryResponse> GetVehicleServiceCategoryDDL()
        {
            GetVehicleCategoryResponse response = new GetVehicleCategoryResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _vehicleService.GetVehicleServiceCategoryDDL();
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

        [HttpPost("AddNewClientVehicle")]
        public BaseResponseWithID AddNewClientVehicle([FromBody]AddNewVehicle RequestData)
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
                    response = _vehicleService.AddNewClientVehicle(RequestData, validation.userID);
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

        [HttpPost("AddNextVehicleMaintenanceInCurrentOpenJobOrder")]
        public BaseResponseWithID AddNextVehicleMaintenanceInCurrentOpenJobOrder([FromBody]AddNewVehicleMaitenanceJobOrder RequestData)
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
                    response = _vehicleService.AddNextVehicleMaintenanceInCurrentOpenJobOrder(RequestData, validation.userID);
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

        [HttpGet("GetClientVehiclesDataResponse")]
        public GetClinetVehiclesDataForGet GetClientVehiclesDataResponse([FromHeader]GetClientVehiclesDataResponseFilters filters)
        {
            var response = new GetClinetVehiclesDataForGet();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response =  _vehicleService.GetClientVehiclesDataResponse(filters, validation.CompanyName);
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

        [HttpGet("GetVehicleMaintenanceJobOrderHistory")]
        public async Task<GetVehicleMaintenanceJobOrderHistoryResponse> GetVehicleMaintenanceJobOrderHistory([FromHeader]GetVehicleMaintenanceJobOrderHistoryFilters filters)
        {
            var response = new GetVehicleMaintenanceJobOrderHistoryResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response =await _vehicleService.GetVehicleMaintenanceJobOrderHistory(filters);
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

        [HttpGet("ViewVehicleMaintenanceTypes")]
        public ViewVehicleMaintenanceTypesResponse ViewVehicleMaintenanceTypes([FromHeader]long? ClientVehicleId)
        {
            var response = new ViewVehicleMaintenanceTypesResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response =  _vehicleService.ViewVehicleMaintenanceTypes(ClientVehicleId);
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


    }
}
