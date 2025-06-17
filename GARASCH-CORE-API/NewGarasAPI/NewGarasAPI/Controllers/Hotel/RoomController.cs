
using AutoMapper;
using NewGaras.Domain.Mappers.Hotel;
using NewGaras.Domain.Models;
using NewGaras.Domain.Services.Hotel;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Hotel.DTOs;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure.Models;


namespace GarasAPP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IRateRepository _rateRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public RoomController(IRoomRepository roomRepository , IMapper mapper , IUnitOfWork unitOfWork , IRateRepository rateRepository , ITenantService tenantService)
        {
            _roomRepository=roomRepository;
            _mapper=mapper;
            _unitOfWork=unitOfWork;
            _rateRepository=rateRepository;
            _helper=new Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoomsAsync([FromHeader] int PageNo = 1 , [FromHeader] int NoOfItems = 20)
        {
            _rateRepository.DailyUpdate();
            _unitOfWork.Complete();
            BaseResponseWithDataAndHeader<List<GetRoomDto>> Response = new BaseResponseWithDataAndHeader<List<GetRoomDto>>();
            Response.Data=new List<GetRoomDto>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var allRoomsDB = (await _roomRepository.FindAllAsync(
                    r => r.Id > 0, new string[] { "RoomType", "Building", "RoomView" })).AsQueryable();

                    var rommPagedList = PagedList<Room>.Create(allRoomsDB,PageNo, NoOfItems);

                    var allRooms = rommPagedList.Select(x => x.ToGetRoomDto()).ToList();

                    Response.PaginationHeader=new PaginationHeader
                    {
                        CurrentPage=PageNo ,
                        TotalPages=rommPagedList.TotalPages ,
                        ItemsPerPage=NoOfItems ,
                        TotalItems=rommPagedList.TotalCount
                    };


                    Response.Data=_roomRepository.GetAllWithFacilities(allRooms);

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


    

        [HttpGet("room")]
        public async Task<IActionResult> GetRoomAsync([FromHeader] int roomId)
        {
            _rateRepository.DailyUpdate();
            _unitOfWork.Complete();
            BaseResponseWithData<GetRoomDto> Response = new BaseResponseWithData<GetRoomDto>();
            Response.Data=new GetRoomDto();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var tempData =(await _roomRepository.FindAsync(
                    r => r.Id == roomId, new string[] { "RoomType", "Building", "RoomView" }));
                    var tempdata2 = tempData.ToGetRoomDto();
                    Response.Data=_roomRepository.GetWithFacilities(tempdata2);
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

        [HttpGet("searchInRooms")]
        public async Task<IActionResult> SearchInRoomsAsync([FromHeader] FindRoomDto findRoomDto)
        {
            _rateRepository.DailyUpdate();
            BaseResponseWithData<List<GetRoomDto>> Response = new BaseResponseWithData<List<GetRoomDto>>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response=await _roomRepository.FindRoomAsync(findRoomDto);
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

        [HttpPost]
        public IActionResult AddRoom(RoomDto createRoomDto)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponseWithData<Room> Response = new BaseResponseWithData<Room>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var newRoom = _roomRepository.Add(_mapper.Map<Room>(createRoomDto));
                    _unitOfWork.Complete();
                    if(createRoomDto.FacilitiesIds!=null)
                    {
                        var result = _roomRepository.AddFacilities(newRoom.Id, createRoomDto.FacilitiesIds);
                        Response.Result=result.Result;
                        Response.Errors=result.Errors;
                    }
                    _rateRepository.Add(new Rate
                    {
                        RoomRate=(int)createRoomDto.Rate ,
                        RoomId=newRoom.Id ,
                        Currency="EGY" ,
                        IsDefault=true ,
                        IsActive=true ,
                        RoomTypeId=createRoomDto.RoomTypeId ,
                        RoomViewId=createRoomDto.RoomViewId ,
                        BuildingId=createRoomDto.BuildingId
                    });
                    _unitOfWork.Complete();
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

        [HttpPost("remove-room")]
        public IActionResult RemoveRoom([FromHeader] int roomId)
        {
            var response = new BaseResponseWithData<Room>
            {
                Data = new Room(),
                Errors = new List<Error>(),
                Result = false
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors=validation.errors;
                response.Result=validation.result;

                if(response.Result)
                {
                    var room = _roomRepository.GetById(roomId);
                    if(room!=null)
                    {
                        var roomReservation  = _Context.RoomsReservations.FirstOrDefault(r => r.RoomId==roomId);
                        if(roomReservation != null)
                        {
                            response.Errors.Add(new Error { ErrorCode="Err11" , ErrorMSG="Room  found in reservation in system" });
                            return Ok(response);
                        }
                        _roomRepository.Delete(room);
                        _unitOfWork.Complete();
                        response.Result=true;
                    }
                    else
                    {
                        response.Errors.Add(new Error { ErrorCode="Err11" , ErrorMSG="Room not found." });
                        return NotFound(response);
                    }
                }

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


        //[HttpPut]
        //public IActionResult UpdateRoom([FromHeader] int roomId,[FromBody] RoomDto updateRoomDto)
        //{
        //    BaseResponseWithData<Room> Response = new BaseResponseWithData<Room>();
        //    Response.Data = new Room();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    if (updateRoomDto.Id != roomId)
        //        return BadRequest(ModelState);

        //    try
        //    {

        //        var updatedRoom = _roomRepository.Update(_mapper.Map<Room>(updateRoomDto));
        //        Response = _roomRepository.AddFacilities(updatedRoom.Id, updateRoomDto.FacilitiesIds, true);
        //        _unitOfWork.Complete();
        //        Response.Result = true;
        //        Response.Data = updatedRoom;
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
        //public async Task<IActionResult> UpdateRoom([FromHeader] int roomId , [FromBody] UpdataRoomDto updateRoomDto)
        //{
        //    BaseResponseWithData<Room> Response = new BaseResponseWithData<Room>();
        //    Response.Data=new Room();
        //    Response.Errors=new List<Error>();
        //    Response.Result=false;

        //    //if(!ModelState.IsValid)
        //    //    return BadRequest(ModelState);
        //    if(updateRoomDto.Id!=roomId)
        //        return BadRequest(ModelState);

        //    try
        //    {
        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //        Response.Errors=validation.errors;
        //        Response.Result=validation.result;
        //        if(Response.Result)
        //        {
        //            var updatedRoom =  _roomRepository.Update(_mapper.Map<Room>(updateRoomDto));
        //            Response=_roomRepository.AddFacilities(updatedRoom.Id , updateRoomDto.FacilitiesIds , true);
        //            _unitOfWork.Complete();
        //            Response.Result=true;
        //            Response.Data=updatedRoom;
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

        [HttpPost("update-room")]
        public async Task<IActionResult> UpdateRoom([FromHeader] int roomId , [FromBody] UpdataRoomDto updateRoomDto)
        {
            var response = new BaseResponseWithData<Room>
            {
                Data = new Room(),
                Errors = new List<Error>(),
                Result = false
            };



            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors=validation.errors;
                response.Result=validation.result;

                if(response.Result)
                {
                    var room = _roomRepository.GetById(roomId);
                    if(room!=null)

                        room.Name=updateRoomDto.Name;
                    room.RoomViewId=updateRoomDto.RoomViewId;
                    room.RoomTypeId=updateRoomDto.RoomTypeId;
                    room.BuildingId=updateRoomDto.BuildingId;
                    room.Description=updateRoomDto.Description;
                    room.Capacity=updateRoomDto.capacity;
                    var updatedRoom = _roomRepository.Update(room);
                    var oldRate = _Context.Rates.FirstOrDefault(r=>r.RoomId == roomId);
                    if(oldRate!=null)
                    {
                        oldRate.RoomTypeId=updateRoomDto.RoomTypeId;
                        oldRate.BuildingId=updateRoomDto.BuildingId;
                        oldRate.RoomViewId=updateRoomDto.RoomViewId;
                        oldRate.RoomRate= (int)updateRoomDto.Rate;
                        _Context.Rates.Update(oldRate);
                        _Context.SaveChanges();
                    }

                    response=_roomRepository.AddFacilities(updatedRoom.Id , updateRoomDto.FacilitiesIds , true);

                    _unitOfWork.Complete();
                    response.Result=true;
                    response.Data=null;

                }
                else
                {
                    response.Errors.Add(new Error { ErrorCode="Err11" , ErrorMSG="Room not found." });
                    return Ok(response);
                }
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
