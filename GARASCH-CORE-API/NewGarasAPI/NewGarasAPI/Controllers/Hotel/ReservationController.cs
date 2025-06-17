
using AutoMapper;
using Azure;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO;
using NewGaras.Infrastructure.DTO.Hotel.DTOs;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Hotel.DTOs;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Hotel;
using System.Security.Claims;

namespace GarasAPP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationRepository _ReservationRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IRateRepository _rateRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public ReservationController(IReservationRepository ReservationRepository , IUnitOfWork unitOfWork
            , IMapper mapper , IRoomRepository roomRepository , IRateRepository rateRepository , IClientRepository clientRepository , ITenantService tenantService)
        {
            _ReservationRepository=ReservationRepository;
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _roomRepository=roomRepository;
            _rateRepository=rateRepository;
            _clientRepository=clientRepository;
            _helper=new Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
        }


        //[HttpGet]
        //public async Task<IActionResult> GetAllReservations()
        //{
        //    BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
        //    Response.Data = new List<Reservation>();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;

        //    try
        //    {
        //        Response.Data = (List<Reservation>)await _ReservationRepository.GetAllAsync();
        //        Response.Result = true;
        //        return Ok(Response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return BadRequest(Response);
        //    }
        //}
        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            BaseResponseWithData<List<ReservationRoomsDto>> Response = new BaseResponseWithData<List<ReservationRoomsDto>>();
            Response.Data=new List<ReservationRoomsDto>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    //var tempData = _mapper.Map<List<ReservationRoomsDto>>(await _ReservationRepository.GetAllAsync());
                    var tempData = _mapper.Map<List<ReservationRoomsDto>>(_Context.Reservations.Where(x=>x.Confirmation == true));
                    Response.Data=_ReservationRepository.GetAllWithRooms(tempData);
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [HttpGet("reservation")]
        public async Task<IActionResult> GetReservationAsync([FromHeader] int reservationId)
        {
            BaseResponseWithData<ReservationRoomsDto> Response = new BaseResponseWithData<ReservationRoomsDto>();
            Response.Data=new ReservationRoomsDto();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var tempdata = _mapper.Map<ReservationRoomsDto>(await _ReservationRepository.GetByIdAsync(reservationId));
                    Response.Data=_ReservationRepository.GetWithRoom(tempdata);
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }
        [HttpGet("reservationWithMealandChildern")]
        public async Task<IActionResult> GetReservationWithChildernandMealTypeAsync([FromHeader] int reservationId)
        {
            BaseResponseWithData<ReservationRoomsByIDDto> Response = new BaseResponseWithData<ReservationRoomsByIDDto>();
            Response.Data=new ReservationRoomsByIDDto();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var tempdata = _mapper.Map<ReservationRoomsByIDDto>(await _ReservationRepository.GetByIdAsync(reservationId));
                    tempdata.ClientName=_clientRepository.Find(x => x.Id==tempdata.ClientId).Name;
                    Response.Data=_ReservationRepository.GetWithRoomandmealandchildern(tempdata);
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }
        //[HttpGet("reservation")]
        //public async Task<IActionResult> GetReservationAsync([FromHeader] int reservationId)
        //{
        //    BaseResponseWithData<Reservation> Response = new BaseResponseWithData<Reservation>();
        //    Response.Data = new Reservation();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;

        //    try
        //    {
        //        Response.Data = await _ReservationRepository.GetByIdAsync(reservationId);
        //        Response.Result = true;
        //        return Ok(Response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return BadRequest(Response);
        //    }
        //}
        [HttpGet("room")]
        public async Task<IActionResult> GetRoomReservation([FromHeader] DurationDto2? durationDto , [FromHeader] int roomId)
        {
            BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
            Response.Data=new List<Reservation>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    if(_roomRepository.GetById(roomId)==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="This Room is not valid";
                        Response.Errors.Add(error);
                        return BadRequest(Response);
                    }
                    //var ttt = _mapper.Map<List<ReservationRoomsDto>>(await _ReservationRepository.GetAllAsync());
                    //var test = (_ReservationRepository.GetRoomReservations(durationDto, roomId));
                    //var tempdata = _mapper.Map<List<ReservationRoomsDto>>( test); 

                    Response=_ReservationRepository.AllReservedRooms(durationDto , roomId);

                    //var tempdate =  (_ReservationRepository.GetRoomReservations(durationDto, roomId));

                    //var temp  =  _ReservationRepository.GetAllWithRooms(_ReservationRepository.GetRoomReservations(durationDto,roomId));
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        //[HttpGet("room")]
        //public async Task<IActionResult> GetRoomReservation([FromHeader] int roomId, DurationDto? durationDto)
        //{
        //    BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
        //    Response.Data = new List<Reservation>();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;

        //    try
        //    {
        //        if (_roomRepository.GetById(roomId) == null)
        //        {
        //            Response.Result = false;
        //            Response.Errors.Add(new Error { code = "E-2", message = "This Room is not valid" });
        //            return BadRequest(Response);
        //        }
        //        Response = _ReservationRepository.GetRoomReservations(durationDto, roomId);
        //        return Ok(Response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return BadRequest(Response);
        //    }
        //}

        [HttpGet("rooms")]
        public async Task<IActionResult> GetAllRoomReservation([FromHeader] DurationDto2? durationDto)
        {
            BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
            Response.Data=new List<Reservation>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response=_ReservationRepository.AllReservedRooms(durationDto);
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        //[HttpGet("rooms")]
        //public async Task<IActionResult> GetAllRoomReservation(DurationDto? durationDto)
        //{
        //    BaseResponseWithData<List<Reservation>> Response = new BaseResponseWithData<List<Reservation>>();
        //    Response.Data = new List<Reservation>();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;

        //    try
        //    {
        //        Response = _ReservationRepository.GetRoomReservations(durationDto);
        //        return Ok(Response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return BadRequest(Response);
        //    }
        //}

        [HttpGet("grid")]
        public async Task<IActionResult> GetRoomReservationGrid([FromHeader] DurationDto2 durationDto , [FromHeader] int? roomTypeId = 0 , [FromHeader] int? roomviewId = 0 , [FromHeader] int? roombuildingId = 0)
        {

            BaseResponseWithData<List<RoomReservationDto>> Response = new BaseResponseWithData<List<RoomReservationDto>>();
            // _rateRepository.DailyUpdate();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response=_ReservationRepository.GetRoomReservationsGrid(durationDto , roomTypeId , roomviewId , roombuildingId);
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [HttpGet("filterRoomNotReservation")]
        public async Task<IActionResult> FilterRoomswithViewBuildingTypeReservation([FromHeader] DurationDto2 durationDto , [FromHeader] int? roomTypeId = 0 , [FromHeader] int? roomviewId = 0 , [FromHeader] int? roombuildingId = 0)
        {

            BaseResponseWithData<List<Room>> Response = new BaseResponseWithData<List<Room>>();
            // _rateRepository.DailyUpdate();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response=_ReservationRepository.GetRoomsNotReservationsGrid(durationDto , roomTypeId , roomviewId , roombuildingId);
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }
        // addreservation
        //[HttpPost]
        //public IActionResult AddReservation(ReservationDto Dto)
        //{
        //    //if(!ModelState.IsValid)
        //    //    return BadRequest(ModelState);
        //    BaseResponseWithData<Reservation> Response = new BaseResponseWithData<Reservation>();
        //    Response.Data=new Reservation();
        //    Response.Errors=new List<Error>();
        //    Response.Result=false;
        //    var duration = new DurationDto
        //    {
        //        StartDate = Dto.FromDate,
        //        EndDate = Dto.ToDate
        //    };

        //    try
        //    {
        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //        Response.Errors=validation.errors;
        //        Response.Result=validation.result;
        //        if(Response.Result)
        //        {
        //            for(int i = 0 ;Dto.ListRooms.Count>i ;i++)
        //            {

        //                if((_ReservationRepository.GetRoomReservationsfortesting(duration , Dto.ListRooms [i].RoomId).Data.Any()))
        //                {
        //                    Error error = new Error();
        //                    error.ErrorCode="Err10";
        //                    error.ErrorMSG=$"This room {Dto.ListRooms [i].RoomId} is already reserved in those days";
        //                    Response.Errors.Add(error);
        //                    return BadRequest(Response);
        //                }

        //            }
        //            Dto.Provider=validation.userID.ToString();
        //            var tempdata = _ReservationRepository.Add(_mapper.Map<Reservation>(Dto));
        //            _unitOfWork.Complete();
        //            var newReservation = _ReservationRepository.AddRooms(tempdata.Id,Dto.ListRooms);
        //            _unitOfWork.Complete();
        //            if(Dto.TotalPaid>0)
        //            {
        //                var NewReservation = new ReservationInvoice
        //                {
        //                    ClientId = Dto.ClientId,
        //                    ReservationId = tempdata.Id,
        //                    InvoiceDate = DateTime.Now,
        //                    Amount =  Dto.TotalPaid ,
        //                    CreateDate = DateTime.Now,
        //                    CreateBy = validation.userID ,
        //                    Serial = Dto.InvoiceSerial,
        //                    InvoiceTypeId =(int) Dto.InvoiceTypeId,
        //                    CurrencyId = Dto.CurrencyId ?? 1
        //                };
        //                _unitOfWork.ReservationInvoice.Add(NewReservation);
        //                _unitOfWork.Complete();

        //            }

        //            Response=newReservation;
        //            Response.Result=true;
        //        }
        //        return Ok(Response);
        //    }
        //    catch(Exception ex)
        //    {
        //        Response.Result=false;
        //        Error error = new Error();
        //        error.ErrorCode="Err10";
        //        error.ErrorMSG=ex.InnerException.Message;
        //        Response.Errors.Add(error);
        //        return BadRequest(Response);
        //    }
        //}

        //[HttpPost]
        //public IActionResult AddReservation(ReservationDto reservationDto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    BaseResponseWithData<Reservation> Response = new BaseResponseWithData<Reservation>();
        //    Response.Data = new Reservation();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;
        //    var duration = new DurationDto
        //    {
        //        StartDate = reservationDto.FromDate,
        //        EndDate = reservationDto.ToDate
        //    };

        //    try
        //    {


        //            foreach (var roomId in reservationDto.roomIds)
        //            {
        //                if ((_ReservationRepository.GetRoomReservationsfortesting(duration, roomId).Data.Any()))
        //                {
        //                    Response.Errors.Add(new Error { code = "E-2", message = $"This room {roomId} is already reserved in those days" });
        //                    return BadRequest(Response);
        //                }
        //            }

        //        var tempdata = _ReservationRepository.Add(_mapper.Map<Reservation>(reservationDto));
        //        _unitOfWork.Complete();
        //        //var newReservation = _ReservationRepository.AddRooms(tempdata.Id,reservationDto.RoomIds);
        //        _unitOfWork.Complete();
        //        // Response = newReservation;
        //        Response.Result = true;
        //        return Ok(Response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return BadRequest(Response);
        //    }
        //}


        [HttpPost]
        public IActionResult AddReservation(ReservationDto Dto)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponseWithData<Reservation> Response = new BaseResponseWithData<Reservation>();
            Response.Data=new Reservation();
            Response.Errors=new List<Error>();
            Response.Result=false;
            var duration = new DurationDto
            {
                StartDate = Dto.FromDate,
                EndDate = Dto.ToDate
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    for(int i = 0 ;Dto.ListRooms.Count>i ;i++)
                    {

                        if((_ReservationRepository.GetRoomReservationsfortesting(duration , Dto.ListRooms [i].RoomId).Data.Any()))
                        {
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG=$"This room {Dto.ListRooms [i].RoomId} is already reserved in those days";
                            Response.Errors.Add(error);
                            return BadRequest(Response);
                        }

                    }
                    Dto.Provider=validation.userID.ToString();
                    long createdBy = validation.userID;
                    Dto.OfferType="Reservation";
                    Dto.OfferId=(_ReservationRepository.GetOfferId(Dto , createdBy)).Data;
                    var  tempdata = _ReservationRepository.Add(_mapper.Map<Reservation>(Dto));

                    _unitOfWork.Complete();
                    var newReservation = _ReservationRepository.AddRooms(tempdata.Id,Dto.ListRooms);
                    _unitOfWork.Complete();
                    if(Dto.TotalPaid>0)
                    {
                        var NewReservation = new ReservationInvoice
                        {
                            ClientId = Dto.ClientId,
                            ReservationId = tempdata.Id,
                            InvoiceDate = DateTime.Now,
                            Amount =  Dto.TotalPaid ,
                            CreateDate = DateTime.Now,
                            CreateBy = validation.userID ,
                            Serial = Dto.InvoiceSerial,
                            InvoiceTypeId =(int) Dto.InvoiceTypeId,
                            CurrencyId = Dto.CurrencyId ?? 1
                        };
                        _unitOfWork.ReservationInvoice.Add(NewReservation);
                        _unitOfWork.Complete();

                    }

                    Response=newReservation;
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [HttpPost("Remove")]
        public IActionResult RemoveReservation([FromHeader] int reservationId , [FromHeader] string returnValue)
        {
            BaseResponseWithData<Reservation> Response = new BaseResponseWithData<Reservation>();
            Response.Data=new Reservation();
            Response.Errors=new List<Error>();
            Response.Result=false;

            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    // var reservationRomm = _Context.RoomsReservations.Remove(_Context.RoomsReservations.FirstOrDefault(r=>r.ReservationId == reservationId));
                    //_ReservationRepository.Delete(_ReservationRepository.GetById(reservationId));

                    var reservation  = _ReservationRepository.GetById(reservationId);
                    if(reservation ==null)
                    {
                        Response.Errors.Add(new Error { ErrorCode="Err11" , ErrorMSG="reservation not found." });
                        return NotFound(Response);
                    }
                    if(string.IsNullOrEmpty(returnValue))
                    {
                        reservation.Confirmation=false;
                        _Context.Reservations.Update(reservation);
                    }
                    else
                    {
                        var oferReservation = _Context.SalesOffers.FirstOrDefault(x=>x.Id == reservation.OfferId);
                    }


                    _unitOfWork.Complete();
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        //[HttpPut]
        //public IActionResult UpdateReservation([FromHeader] int reservationId, [FromBody] Reservation reservation)
        //{
        //    BaseResponseWithData<Reservation> Response = new BaseResponseWithData<Reservation>();
        //    Response.Data = new Reservation();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    if (reservation.Id != reservationId)
        //        return BadRequest(ModelState);

        //    try
        //    {

        //        var updatedReservation = _ReservationRepository.Update(reservation);
        //        _unitOfWork.Complete();
        //        Response.Result = true;
        //        Response.Data = updatedReservation;
        //        return Ok(Response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return BadRequest(Response);
        //    }
        //}

        //[HttpPut]
        //public IActionResult UpdateReservation([FromHeader] int reservationId, [FromBody] ReservationUpdataDto reservationUpdata)
        //{
        //    BaseResponseWithData<Reservation> Response = new BaseResponseWithData<Reservation>();
        //    Response.Data = new Reservation();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    if (reservationUpdata.Id != reservationId)
        //        return BadRequest(ModelState);
        //    var duration = new DurationDto
        //    {
        //        StartDate = reservationUpdata.FromDate,
        //        EndDate = reservationUpdata.ToDate
        //    };

        //    try
        //    {

        //        if (_ReservationRepository.GetRoomReservations(duration, reservationUpdata.RoomId).Data.Any())
        //        {
        //            Response.Errors.Add(new Error { code = "E-2", message = "This room is already reserved in those days" });
        //            return BadRequest(Response);
        //        }

        //        var updatedReservation = _ReservationRepository.Update(_mapper.Map<Reservation>(reservationUpdata));
        //        _unitOfWork.Complete();
        //        Response.Result = true;
        //        Response.Data = updatedReservation;
        //        return Ok(Response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Response.Errors.Add(new Error { code = "E-1", message = ex.InnerException != null ? ex.InnerException?.Message : ex.Message });
        //        return BadRequest(Response);
        //    }
        //}

        [HttpPost("update")]
        public IActionResult UpdateReservation([FromHeader] int reservationId , [FromBody] ReservationDto Dto)
        {
            BaseResponseWithData<Reservation> Response = new BaseResponseWithData<Reservation>();
            Response.Data=new Reservation();
            Response.Errors=new List<Error>();
            Response.Result=false;

            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            if(Dto.Id!=reservationId)
                return BadRequest(ModelState);
            var duration = new DurationDto
            {
                StartDate = Dto.FromDate,
                EndDate = Dto.ToDate
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    for(int i = 0 ;Dto.ListRooms.Count>i ;i++)
                    {

                        if((_ReservationRepository.GetRoomReservationsfortesting(duration , Dto.ListRooms [i].RoomId , Dto.Id).Data.Any()))
                        {
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG=$"This room {Dto.ListRooms [i].RoomId} is already reserved in those days";
                            Response.Errors.Add(error);
                            return BadRequest(Response);
                        }

                    }
                    var updatedReservation = _ReservationRepository.Update(_mapper.Map<Reservation>(Dto));
                    _unitOfWork.Complete();
                    var newReservation = _ReservationRepository.AddRooms(updatedReservation.Id, Dto.ListRooms, true);
                    _unitOfWork.Complete();
                    Response.Result=true;
                    Response.Data=updatedReservation;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }


        [HttpGet("getHeaderNumbers")]
        public async Task<IActionResult> getHeaderNumbers([FromHeader] DurationDto2 durationDto)
        {

            BaseResponseWithData<HeaderNumbersReservations> Response = new BaseResponseWithData<HeaderNumbersReservations>();
            // _rateRepository.DailyUpdate();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var headerNumber = new HeaderNumbersReservations ();
                    var test =_ReservationRepository.AllReservedRooms(durationDto);
                    headerNumber.ReservationRoom=_ReservationRepository.GetAllRoomsReservationWithDuration(durationDto).Data;
                    var roomTypeId = 0;
                    var roomviewId = 0;
                    var roombuildingId = 0;
                    var test1 =_ReservationRepository.GetRoomsNotReservationsGrid(durationDto ,roomTypeId ,roomviewId ,roombuildingId);
                    headerNumber.AvailableRoom=test1.Data.Count();
                    var allreservations =_unitOfWork.Reservations.FindAll(x=>x.Confirmation).ToList();
                    var test2 = allreservations.Intersect(test.Data).ToList();
                    headerNumber.Confirmation=test2.Count();

                    Response.Data=headerNumber;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }
        // addreservation

        [HttpPost("AddInvoice")]
        public IActionResult AddInvoice(ReservationInvoice Dto)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponse Response = new BaseResponse();
            Response.Errors=new List<Error>();
            Response.Result=false;


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    Dto.CreateBy=validation.userID;

                    var newReservation =_ReservationRepository.AddInvoice(Dto);

                    Response=newReservation;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }


        [HttpPost("AddAddons")]
        public IActionResult AddAddons(AddonsDto Dto)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponse Response = new BaseResponse();
            Response.Errors=new List<Error>();
            Response.Result=false;


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    Dto.CreateBy=validation.userID;

                    var newReservation =_ReservationRepository.AddAddons(Dto);

                    Response=newReservation;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }



        [HttpGet("getAllInvoice")]
        public async Task<IActionResult> getAllInvoice([FromHeader] int reservationId)
        {
            var response = new BaseResponseWithData<List<ReservationInvoiceDto>>
            {
                Data = new List<ReservationInvoiceDto>(),
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors=validation.errors;
                response.Result=validation.result;

                if(!response.Result)
                    return Ok(response);

                var allInvoices = _Context.ReservationInvoices
            .Where(x => x.ReservationId == reservationId)
            .ToList();

                var allUsers = _Context.Users.ToList();
                var allClients = _Context.Clients.ToList();
                var allInvoiceTypes = _Context.InvoiceTypes.ToList();
                var allCurrencies = _Context.Currencies.ToList();

                var invoiceDtos = allInvoices.Select(a => new ReservationInvoiceDto
                {
                    Id = a.Id,
                    InvoiceDate = a.InvoiceDate.ToString("yyyy-MM-dd") ?? "",
                    Amount = a.Amount,
                    CreateDate = a.CreateDate.ToString("yyyy-MM-dd") ?? "",
                    CreateBy = allUsers.FirstOrDefault(u => u.Id == a.CreateBy)?.FirstName ?? "",
                    Serial = a.Serial,
                    ClientName = allClients.FirstOrDefault(c => c.Id == a.ClientId)?.Name ?? "",
                    InvoiceTypeName = allInvoiceTypes.FirstOrDefault(i => i.Id == a.InvoiceTypeId)?.Name ?? "",
                    CurrencyName = allCurrencies.FirstOrDefault(c => c.Id == a.CurrencyId)?.Name ?? " "
                }).ToList();

                response.Data=invoiceDtos;
                return Ok(response);
            }
            catch(Exception ex)
            {
                response.Result=false;
                response.Errors.Add(new Error
                {
                    ErrorCode="Err10" ,
                    ErrorMSG=ex.InnerException?.Message??ex.Message
                });

                return BadRequest(response);
            }
        }

    }
}