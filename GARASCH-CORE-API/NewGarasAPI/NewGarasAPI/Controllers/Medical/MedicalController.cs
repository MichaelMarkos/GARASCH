using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services.Medical;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Medical.DoctorSchedule;
using NewGaras.Infrastructure.DTO.Medical.MedicalFinance;
using NewGaras.Infrastructure.DTO.Medical.MedicalReservation;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs;
using NewGaras.Infrastructure.Models.InternalTicket;
using NewGaras.Infrastructure.Models.Medical;
using NewGaras.Infrastructure.Models.Medical.Filters;
using NewGaras.Infrastructure.Models.SalesOffer.InternalTicket;
using NewGarasAPI.Models.User;

namespace NewGarasAPI.Controllers.Medical
{
    [Route("Medical")]
    [ApiController]
    public class MedicalController : ControllerBase
    {
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMedicalService _medicalService;
        private readonly IDoctorScheduleService _doctorScheduleService;
        private readonly IReservationService _reservationService;
        private readonly IMedicalFinancialService _medicalFinancialService;

        public MedicalController(ITenantService tenantService, IMedicalService medicalService, IDoctorScheduleService doctorScheduleService, IReservationService reservationService, IMedicalFinancialService medicalFinancialService)
        {
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _medicalService = medicalService;
            _doctorScheduleService = doctorScheduleService;
            _reservationService = reservationService;
            _medicalFinancialService = medicalFinancialService;
        }

        [HttpPost("AddNewPatient")]
        public BaseResponseWithId<long> AddNewPatient(NewPatientDto patient)
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
                    _medicalService.Validation = validation;
                    Response = _medicalService.AddNewPatient(patient);
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

        [HttpPost("EditPatient")]
        public BaseResponseWithId<long> EditPatient(NewPatientDto patient)
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
                    _medicalService.Validation = validation;
                    Response = _medicalService.EditPatient(patient);
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

        [HttpGet("GetPatient")]
        public BaseResponseWithData<GetPatientModel> GetPatient([FromHeader] long PatientId)
        {
            BaseResponseWithData<GetPatientModel> Response = new BaseResponseWithData<GetPatientModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _medicalService.Validation = validation;
                    Response = _medicalService.GetPatient(PatientId);
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

        [HttpGet("GetPatientsList")]
        public BaseResponseWithData<GetPatientsListModel> GetPatientsList([FromHeader] string SearchKey, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10)
        {
            BaseResponseWithData<GetPatientsListModel> Response = new BaseResponseWithData<GetPatientsListModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _medicalService.Validation = validation;
                    Response = _medicalService.GetPatientsList(SearchKey, CurrentPage, NumberOfItemsPerPage);
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

        [HttpPost("AddDoctorSchedule")]
        public BaseResponseWithId<long> AddDoctorSchedule([FromBody] DoctorScheduleDTO dto)
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
                    _doctorScheduleService.Validation = validation;
                    Response = _doctorScheduleService.AddDoctorSchedule(dto);
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

        [HttpPost("EditDoctorSchedule")]
        public BaseResponseWithId<long> EditDoctorSchedule([FromBody] EditDoctorScheduleDTO dto)
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
                    _doctorScheduleService.Validation = validation;
                    Response = _doctorScheduleService.EditDoctorSchedule(dto);

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

        [HttpPost("AddDoctorSchedulestatus")]
        public BaseResponseWithId<long> AddDoctorSchedulestatus([FromForm] string DoctorScheduleStatusType)
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
                    _doctorScheduleService.Validation = validation;
                    Response = _doctorScheduleService.AddDoctorSchedulestatus(DoctorScheduleStatusType);

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

        [HttpGet("GetDoctorSchedulestatus")]
        public SelectDDLResponse GetDoctorSchedulestatus()
        {
            var Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _medicalService.Validation = validation;
                    Response = _doctorScheduleService.GetDoctorSchedulestatus();
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

        [HttpGet("GetDoctorScheduleList")]
        public BaseResponseWithDataAndHeader<GetDoctorScheduleList> GetDoctorScheduleList(GetDoctorScheduleListFilters filters)
        {
            var Response = new BaseResponseWithDataAndHeader<GetDoctorScheduleList>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _doctorScheduleService.Validation = validation;
                    Response = _doctorScheduleService.GetDoctorScheduleList(filters);
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

        [HttpPost("AddMedicalExaminationOffer")]
        public BaseResponseWithId<long> AddMedicalExaminationOffer([FromBody] AddMedicalExaminationOfferDTO offer)
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
                    _reservationService.Validation = validation;
                    Response = _reservationService.AddMedicalExaminationOffer(offer);

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

        [HttpPost("EditMedicalExaminationOffer")]
        public BaseResponseWithId<long> EditMedicalExaminationOffer([FromBody] EditMedicalExaminationOfferDTO offer)
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
                    _reservationService.Validation = validation;
                    Response = _reservationService.EditMedicalExaminationOffer(offer);

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

        [HttpGet("GetMedicalExaminationOfferList")]
        public BaseResponseWithData<List<GetMedicalExaminationOfferList>> GetMedicalExaminationOfferList()
        {
            var Response = new BaseResponseWithData<List<GetMedicalExaminationOfferList>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _medicalService.Validation = validation;
                    Response = _reservationService.GetMedicalExaminationOfferList();
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

        [HttpPost("AddMedicalReservation")]
        public BaseResponseWithId<long> AddMedicalReservation([FromBody] AddMedicalReservationDTO data)
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
                    _reservationService.Validation = validation;
                    Response = _reservationService.AddMedicalReservation(data);

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

        [HttpGet("GetMedicalReservationList")]
        public BaseResponseWithData<GetMedicalReservationList> GetMedicalReservationList([FromHeader] GetGetMedicalReservationFilters filters)
        {
            var Response = new BaseResponseWithData<GetMedicalReservationList>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _medicalService.Validation = validation;
                    Response = _reservationService.GetMedicalReservationList(filters);
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

        [HttpGet("GetMedicalReservationById")]
        public BaseResponseWithData<GetMedicalReservationDTO> GetMedicalReservationById([FromHeader]long Id)
        {
            var Response = new BaseResponseWithData<GetMedicalReservationDTO>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _medicalService.Validation = validation;
                    Response = _reservationService.GetMedicalReservationById(Id);
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

        [HttpPost("AddExaminationAndExaminationPrices")]
        public BaseResponseWithId<long> AddExaminationAndExaminationPrices([FromBody] AddExaminationAndExaminationPricesDTO data)
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
                    _doctorScheduleService.Validation = validation;
                    Response = _doctorScheduleService.AddExaminationAndExaminationPrices(data);

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

        [HttpPost("AddMedicalOpenBalance")]
        public BaseResponseWithId<long> AddMedicalOpenBalance(AddOpeningMedicalFinancialDTO dto)
        {
            var Response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    _medicalFinancialService.Validation = validation;
                    Response = _medicalFinancialService.AddMedicalOpenBalance(dto, validation.userID);

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

        [HttpPost("AddMedicalClosingBalance")]
        public BaseResponseWithId<long> AddMedicalClosingBalance([FromBody] AddClosingMedicalFinancialDTO dto)
        {
            var Response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    _medicalFinancialService.Validation = validation;
                    Response = _medicalFinancialService.AddMedicalClosingBalance(dto);

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

        [HttpGet("GetPosNumberDDL")]
        public BaseResponseWithData<List<SelectDDL>> GetPosNumerDDL()
        {
            var Response = new BaseResponseWithData<List<SelectDDL>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    _medicalFinancialService.Validation = validation;
                    Response = _medicalFinancialService.GetPosNumerDDL();

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

        [HttpGet("GetAllMedicalDailyTreasuryBalance")]
        public BaseResponseWithData<List<MedicalDailyTreasuryBalanceDto>> GetAllMedicalDailyTreasuryBalance([FromHeader]int PosNumberId, [FromHeader]long CreatedById,[FromHeader] string Type ,[FromHeader]DateTime? From, [FromHeader]DateTime? To, [FromHeader]bool? IsOpeningBalance)
        {
            var Response = new BaseResponseWithData<List<MedicalDailyTreasuryBalanceDto>>
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    _medicalFinancialService.Validation = validation;
                    Response = _medicalFinancialService.GetAllMedicalDailyTreasuryBalance(PosNumberId, CreatedById, Type, From, To, IsOpeningBalance);

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

        [HttpGet("GetRoomsList")]
        public BaseResponseWithData<List<long?>> GetRoomsList()
        {
            var Response = new BaseResponseWithData<List<long?>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _medicalService.Validation = validation;
                    Response = _doctorScheduleService.GetRoomsList();
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

        [HttpGet("GetDoctorScheduleListGroupedByDoctorName")]
        public BaseResponseWithData<GetDoctorScheduleGroupByDocNameList> GetDoctorScheduleListGroupByDoctorName(GetDoctorScheduleListGroupByDoctorNameFilters filters)
        {
            var Response = new BaseResponseWithData<GetDoctorScheduleGroupByDocNameList>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _doctorScheduleService.Validation = validation;
                    Response = _doctorScheduleService.GetDoctorScheduleListGroupByDoctorName(filters);
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

        [HttpGet("GetPercentageTypeListForDoctorSchedule")]
        public SelectDDLResponse GetPercentageTypeListForDoctorSchedule()
        {
            var Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _doctorScheduleService.Validation = validation;
                    Response = _doctorScheduleService.GetPercentageTypeListForDoctorSchedule();
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


        [HttpGet("GetSpecialityListForDoctorSchedule")]
        public SelectDDLResponse GetSpecialityListForDoctorSchedule([FromHeader] long? DoctorID)
        {
            var Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _doctorScheduleService.Validation = validation;
                    Response = _doctorScheduleService.GetSpecialityListForDoctorSchedule(DoctorID);
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

        [HttpGet("GetWeekDaysListForDoctorSchedule")]
        public SelectDDLResponse GetWeekDaysListForDoctorSchedule()
        {
            var Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _doctorScheduleService.Validation = validation;
                    Response = _doctorScheduleService.GetWeekDaysListForDoctorSchedule();
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

        [HttpPost("CancelDoctorSchedule")]
        public BaseResponseWithId<long> CancelDoctorSchedule(CancelDoctorScheduleDTO data)
        {
            var Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _doctorScheduleService.Validation = validation;
                    Response = _doctorScheduleService.CancelDoctorSchedule(data, validation.userID);
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

        [HttpPost("MoveReservationsToAnotherDoctor")]
        public BaseResponseWithId<long> MoveReservationsToAnotherDoctor(MoveReservationsToAnotherDoctorDTO data)
        {
            var Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _reservationService.Validation = validation;
                    Response = _reservationService.MoveReservationsToAnotherDoctor(data, validation.userID);
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

        [HttpPost("AddClientPatientInfo")]
        public BaseResponseWithId<long> AddClientPatientInfo(AddClientPatientInfoDTO data)
        {
            var Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _reservationService.Validation = validation;
                    Response = _reservationService.AddClientPatientInfo(data, validation.userID);
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

        [HttpGet("GetClientPatientInfo")]
        public BaseResponseWithData<GetClientPatientInfoDTO> GetClientPatientInfo([FromHeader]long ClientID)
        {
            BaseResponseWithData<GetClientPatientInfoDTO> Response = new BaseResponseWithData<GetClientPatientInfoDTO>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    _reservationService.Validation = validation;
                    Response = _reservationService.GetClientPatientInfo(ClientID);

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

        [HttpPost("EditClientPatientInfo")]
        public BaseResponseWithId<long> EditClientPatientInfo(EditClientPatientInfoDTO data)
        {
            var Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _reservationService.Validation = validation;
                    Response = _reservationService.EditClientPatientInfo(data, validation.userID);
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

        [HttpGet("GetPaymentMethodDDL")]
        public SelectDDLResponse GetPaymentMethodDDL()
        {
            var Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _reservationService.Validation = validation;
                    Response = _reservationService.GetPaymentMethods();
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

        [HttpGet("GetListOfSerialReserved")]
        public BaseResponseWithData<List<int>> GetListOfSerialReserved(GetListOfSerialReservedFilters filters)
        {
            var Response = new BaseResponseWithData<List<int>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    _reservationService.Validation = validation;
                    Response = _reservationService.GetListOfSerialReserved(filters);

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

        [HttpGet("GetPatientTypeDDl")]
        public SelectDDLResponse GetPatientTypeDDl()
        {
            var Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _reservationService.Validation = validation;
                    Response = _reservationService.GetPatientTypeDDl();
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


        [HttpGet("GetTopDoctors")]
        public BaseResponseWithData<List<TopData>> GetTopDoctors([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] string CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To, 
            [FromHeader] string OfferType, [FromHeader] string OfferTypeReturn)
        {
            BaseResponseWithData<List<TopData>> response = new BaseResponseWithData<List<TopData>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _medicalService.GetTopDoctors(year, month, day, CreatorId, From, To,OfferType,OfferTypeReturn);

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

        [HttpGet("GetTopDepartmentsOrCategories")]
        public BaseResponseWithData<List<TopData>> GetTopDepartmentsOrCategories([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] string type, [FromHeader] string OfferType, [FromHeader] string OfferTypeReturn)
        {
            BaseResponseWithData<List<TopData>> response = new BaseResponseWithData<List<TopData>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _medicalService.GetTopDepartmentsOrCategories(year, month, day, type,OfferType,OfferTypeReturn);

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

        [HttpGet("GetMedicalDashboard")]
        public BaseResponseWithData<InternalTicketDashboardSummary> GetMedicalDashboard([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] DateTime? From, [FromHeader] DateTime? To,
            [FromHeader] string OfferType, [FromHeader] string OfferTypeReturn)
        {
            BaseResponseWithData<InternalTicketDashboardSummary> response = new BaseResponseWithData<InternalTicketDashboardSummary>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _medicalService.GetMedicalDashboard(year, month, day, From, To,OfferType,OfferTypeReturn);

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

        [HttpGet("GetMedicalByTeams")]
        public BaseResponseWithData<GetInternalTicketsByDepartmentResponse> GetMedicalByTeams([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] string CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To,
            [FromHeader] string OfferType, [FromHeader] string OfferTypeReturn)
        {
            BaseResponseWithData<GetInternalTicketsByDepartmentResponse> response = new BaseResponseWithData<GetInternalTicketsByDepartmentResponse>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _medicalService.GetMedicalByTeams(year, month, day, CreatorId, From, To, OfferType, OfferTypeReturn);

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

        [HttpGet("GetMedicalByCategories")]
        public BaseResponseWithData<GetInternalTicketsByCategoryResponse> GetMedicalByCategories([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] string CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To, 
            [FromHeader] string OfferType, [FromHeader] string OfferTypeReturn)
        {
            BaseResponseWithData<GetInternalTicketsByCategoryResponse> response = new BaseResponseWithData<GetInternalTicketsByCategoryResponse>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _medicalService.GetMedicalByCategories(year, month, day, CreatorId, From, To, OfferType, OfferTypeReturn);

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
        [HttpGet("GetMedicalByItemCategories")]
        public BaseResponseWithData<GetInternalTicketsByItemCategoryResponse> GetMedicalByItemCategories([FromHeader] int year, [FromHeader] int month, [FromHeader] int day, [FromHeader] string CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To,
            [FromHeader] string OfferType, [FromHeader] string OfferTypeReturn)
        {
            BaseResponseWithData<GetInternalTicketsByItemCategoryResponse> response = new BaseResponseWithData<GetInternalTicketsByItemCategoryResponse>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _medicalService.GetMedicalByItemCategories(year, month, day, CreatorId, From, To, OfferType, OfferTypeReturn);

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

        [HttpGet("GetSalesOfferListForMedical")]
        public GetSalesOfferListForInternalTicketResponse GetSalesOfferListForMedical([FromHeader] GetSalesOfferInternalTicketListFilters filters)
        {
            GetSalesOfferListForInternalTicketResponse response = new GetSalesOfferListForInternalTicketResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _medicalService.GetSalesOfferListForMedical(filters, filters.OfferStatus);

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

        [HttpGet("GetSalesOfferForStoreExcel")]
        public BaseResponseWithData<string> GetSalesOfferForStoreExcel([FromHeader] string from, [FromHeader] string to, [FromHeader] string UserId, [FromHeader] string OfferType, [FromHeader] string OfferTypeReturn)
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
                    var data = _medicalService.GetSalesOfferForStoreExcel(from, to, UserId, validation.CompanyName, OfferType,OfferTypeReturn ,validation.userID);
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

        [HttpGet("GetSalesOfferForStoreForAllUsersPDF")]
        public async Task<BaseResponseWithData<string>> GetSalesOfferForStoreForAllUsersPDF(InternalticketheaderPdf header)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _medicalService.GetSalesOfferForStoreForAllUsersPDF(header, validation.userID);
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

        [HttpPost("MoveReservationListToAnotherSchedule")]
        public BaseResponseWithId<long> MoveReservationListToAnotherSchedule(MoveReservationListToAnotherSchedule dto) 
        {
            var Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _reservationService.Validation = validation;
                    Response = _reservationService.MoveReservationListToAnotherSchedule(dto);
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

        [HttpPost("CreateDoctorUser")]
        public async Task<BaseResponseWithId<long>> CreateDoctorUser([FromForm] AddDoctorUserDTO NewHrUser)
        {
            var Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    _reservationService.Validation = validation;
                    Response = await _medicalService.CreateDoctorUserAsync(NewHrUser, validation.userID, validation.CompanyName);
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

        [HttpPost("AddReservationForMultiTickests")]
        public BaseResponseWithId<long> AddReservationForMultiTickests([FromBody] AddMedicalReservationDTO data, [FromHeader]int NumberOfTickets)
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
                    _reservationService.Validation = validation;
                    for(int i = 0; i < NumberOfTickets;i++)
                    {
                        Response = _reservationService.AddMedicalReservation(data);

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

        [HttpGet("GetDoctorSchedulePerWeek")]
        public BaseResponseWithData<List<GetDoctorSchedulePerWeekDTO>> GetDoctorSchedulePerWeek(GetDoctorSchedulePerWeekFilters filters)
        {
            BaseResponseWithData<List<GetDoctorSchedulePerWeekDTO>> Response = new BaseResponseWithData<List<GetDoctorSchedulePerWeekDTO>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _doctorScheduleService.GetDoctorSchedulePerWeek(filters);
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

        [HttpGet("GetMedicalDailyTreasuryBalance")]
        public BaseResponseWithData<MedicalDailyTreasuryBalanceDto> GetMedicalDailyTreasuryBalance([FromHeader] int PosNumberId, [FromHeader] bool IsOpeningBalance, string Type,[FromHeader] long? CreatedById )
        {
            var Response = new BaseResponseWithData<MedicalDailyTreasuryBalanceDto>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    _medicalFinancialService.Validation = validation;
                    Response = _medicalFinancialService.GetMedicalDailyTreasuryBalance(PosNumberId, IsOpeningBalance,Type, CreatedById);

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
