using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services.Medical;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Medical.DoctorRooms;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs;

namespace NewGarasAPI.Controllers.Medical
{
    [Route("Medical/[controller]")]
    [ApiController]
    public class DoctorRoomController : ControllerBase
    {
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IDoctorRoomService _doctorRoomService;
        public DoctorRoomController(ITenantService tenantService, IDoctorRoomService doctorRoomService)
        {
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _doctorRoomService = doctorRoomService;
        }
        [HttpPost("AddDoctorRoom")]
        public BaseResponseWithId<long> AddDoctorRoom(DoctorRoomDto dto)
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
                    _doctorRoomService.Validation = validation;
                    Response = _doctorRoomService.AddDoctorRoom(dto);
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


        [HttpPost("UpdateDoctorRoom")]
        public BaseResponseWithId<long> UpdateDoctorRoom(DoctorRoomDto dto)
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
                    _doctorRoomService.Validation = validation;
                    Response = _doctorRoomService.UpdateDoctorRoom(dto);
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


        [HttpGet("GetDoctorRoomById")]
        public BaseResponseWithData<DoctorRoomDto> GetDoctorRoomById([FromHeader] long RoomId)
        {
            BaseResponseWithData<DoctorRoomDto> Response = new BaseResponseWithData<DoctorRoomDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _doctorRoomService.Validation = validation;
                    Response = _doctorRoomService.GetDoctorRoomById(RoomId);
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


        [HttpGet("GetDoctorRoomList")]
        public BaseResponseWithDataAndHeader<GetDoctorRooms> GetDoctorRoomList(GetDoctorRoomListFilters filters)
        {
            BaseResponseWithDataAndHeader<GetDoctorRooms> Response = new BaseResponseWithDataAndHeader<GetDoctorRooms>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _doctorRoomService.Validation = validation;
                    Response = _doctorRoomService.GetDoctorRoomList(filters);
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

        [HttpGet("GetRoomsWithSchedule")]
        public BaseResponseWithDataAndHeader<RoomsWithSchedule> GetRoomsWithSchedule(GetRoomsWithScheduleFilters filters)
        {
            BaseResponseWithDataAndHeader<RoomsWithSchedule> Response = new BaseResponseWithDataAndHeader<RoomsWithSchedule>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _doctorRoomService.Validation = validation;
                    Response = _doctorRoomService.GetRoomsWithSchedule(filters);
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
