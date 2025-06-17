using AutoMapper;
using NewGaras.Infrastructure;
using System.Linq.Expressions;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Repositories;
using NewGaras.Domain.Models;


namespace NewGaras.Domain.Services.Hotel
{
    public class RoomRepository : BaseRepository<Room, int>, IRoomRepository
    {
        protected GarasTestContext _context;
        protected IUnitOfWork _unitOfWork;
        protected IMapper _mapper;
        public RoomRepository(GarasTestContext context, IMapper mapper, IUnitOfWork unitOfWork) : base(context)
        {
            _context = context;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public BaseResponseWithData<Room> AddFacilities(int id, List<int>? facilities, bool updateFacility = false)
        {
            BaseResponseWithData<Room> Response = new BaseResponseWithData<Room>();
            Response.Data = new Room();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {
                if (id == 0 || facilities is null || facilities.Contains(0))
                {
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid room facilities";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (updateFacility == true)
                {
                    foreach (var roomFacility in _context.RoomFacilities.Where(x => x.RoomId == id))
                    {
                        _context.RoomFacilities.Remove(roomFacility);
                    }

                }
                foreach (var facility in facilities)
                {
                    _context.RoomFacilities.Add(new RoomFacility { RoomId = id, FacilityId = facility });
                }
                Response.Result = true;
                Response.Data = null;
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithData<List<GetRoomDto>>> FindRoomAsync(FindRoomDto findRoomDto)
        {
            BaseResponseWithData<List<GetRoomDto>> Response = new BaseResponseWithData<List<GetRoomDto>>();
            Response.Data = new List<GetRoomDto>();
            Response.Errors = new List<Error>();
            Response.Result = false;
            try
            {
                if (findRoomDto == null)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid search keys";
                    Response.Errors.Add(error);
                    return Response;
                }

                Expression<Func<Room, bool>> filter = (x =>
                                    (findRoomDto.Name == null || x.Name.Contains(findRoomDto.Name))
                                  && (findRoomDto.RoomType == null || x.RoomType.Type.Contains(findRoomDto.RoomType))
                                  && (findRoomDto.RoomView == null || x.RoomView.View.Contains(findRoomDto.RoomView))
                                  && (findRoomDto.Building == null || x.Building.BuildingName.Contains(findRoomDto.Building))
                );

                var tempRooms = _mapper.Map<List<GetRoomDto>>(FindAll(filter, new string[] { "RoomType", "Building", "RoomView" }));

                //var reservedRooms = new List<Room>();
                //if (findRoomDto.FilterByReserved)
                //{
                //    if (findRoomDto.FromDate != null || findRoomDto.ToDate != null)
                //    {
                //        reservedRooms = _context.Reservations
                //            .Where(x => x.FromDate < findRoomDto.FromDate && x.ToDate > findRoomDto.ToDate)
                //            .Select(x => x.Room).ToList();
                //        if (findRoomDto.Reserved) tempRooms = reservedRooms;
                //        else tempRooms = tempRooms.Except(reservedRooms).ToList();
                //    }
                //}

                tempRooms = GetAllWithFacilities(tempRooms);
                if (findRoomDto.Facilties != null)
                {
                    foreach (var facilityId in findRoomDto.Facilties)
                    {
                        var facility = _context.Facilities.FirstOrDefault(f => f.Id == facilityId);
                        tempRooms = tempRooms.Where(x => x.Facilities.Contains(facility)).ToList();
                    }
                }

                Response.Data = tempRooms;
                Response.Result = false;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }

        }

        public List<GetRoomDto> GetAllWithFacilities(List<GetRoomDto> roomDtos)
        {
            for (int i = 0; i < roomDtos.Count; i++)
            {
                roomDtos[i] = GetWithFacilities(roomDtos[i]);
            }
            return roomDtos;
        }

        public GetRoomDto GetWithFacilities(GetRoomDto roomDto)
        {

            roomDto.Facilities = _context.RoomFacilities.Where(x => x.RoomId == roomDto.Id)
                    .Select(x => x.Facility).ToList();
            roomDto.FacilityName = _context.RoomFacilities.Where(x => x.RoomId == roomDto.Id)
                  .Select(x => x.Facility.FacilityName).ToList();
            //roomDto.FacilitiesIds = roomDto.Facilities.Select(x => x.Id).ToList();
            var RoomDB = _context.Rates.Where(r => r.IsActive && r.RoomId == roomDto.Id).FirstOrDefault();
            if (RoomDB != null)
            {
                roomDto.Rate = RoomDB.RoomRate;

            }
            return roomDto;
        }
    }
}
