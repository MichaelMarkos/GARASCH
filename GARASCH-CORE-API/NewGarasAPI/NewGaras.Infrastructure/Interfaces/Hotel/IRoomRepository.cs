


using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Interfaces.Hotel
{
    public interface IRoomRepository : IBaseRepository<Room , int>
    {
        BaseResponseWithData<Room> AddFacilities(int id , List<int>? facilities , bool updateFacility = false);
        Task<BaseResponseWithData<List<GetRoomDto>>> FindRoomAsync(FindRoomDto findRoomDto);
        List<GetRoomDto> GetAllWithFacilities(List<GetRoomDto> roomDtos);
        GetRoomDto GetWithFacilities(GetRoomDto roomDto);
    }
}
