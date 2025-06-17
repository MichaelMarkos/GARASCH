


using NewGaras.Infrastructure.DTO.Hotel.DTOs;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;

namespace NewGaras.Infrastructure.Interfaces.Hotel
{
    public interface IReservationRepository : IBaseRepository<Reservation , int>
    {
        BaseResponseWithData<List<Reservation>> GetRoomReservations(DurationDto? durationDto , int roomId = 0);
        BaseResponseWithData<List<Reservation>> AllReservedRooms(DurationDto2? durationDto , int roomId = 0);
        public BaseResponseWithData<List<RoomReservationDto>> GetRoomReservationsGrid(DurationDto2 durationDto , int? roomTypeId , int? roomviewId , int? roombuildingId);
        public BaseResponseWithData<List<ReservationRoomsDto>> GetAllRoomReservations(DurationDto? durationDto , List<int> roomIds);
        public List<ReservationRoomsDto> GetAllWithRooms(List<ReservationRoomsDto> roomDtos);
        public ReservationRoomsDto GetWithRoom(ReservationRoomsDto roomDto);
        public BaseResponseWithData<List<ReservationRoomsDto>> GetAllRoomReservationsForAdding(DurationDto? durationDto , List<int> roomIds);
        public BaseResponseWithData<Reservation> AddRooms(int id , List<AddListofReservationDto> addlist , bool updateFacility = false);
        public BaseResponseWithData<List<Reservation>> NotGetRoomReservations(DurationDto? durationDto , List<int> roomIds , int Id);
        public BaseResponseWithData<List<Reservation>> GetRoomReservationsfortesting(DurationDto? durationDto , int roomId = 0 , int reservationId = 0);
        public ReservationRoomsByIDDto GetWithRoomandmealandchildern(ReservationRoomsByIDDto roomDto);
        public BaseResponseWithData<List<Room>> GetRoomsNotReservationsGrid(DurationDto2 durationDto , int? roomTypeId , int? roomviewId , int? roombuildingId);

        public BaseResponseWithData<int> GetAllRoomsReservationWithDuration(DurationDto2 durationDto);

        public BaseResponseWithData<long> GetOfferId(ReservationDto dto , long createdBy);

        public BaseResponse AddInvoice(ReservationInvoice dto);
        public BaseResponse AddAddons(AddonsDto dto);

    }
}
