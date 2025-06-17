


using NewGaras.Infrastructure.Entities;

namespace NewGaras.Domain.Mappers.Hotel
{
    public static class RoomMapper
    {
        public static GetRoomDto ToGetRoomDto(this Room commentModel)
        {
            return new GetRoomDto
            {
                Id=commentModel.Id ,
                Name=commentModel.Name ,
                Description=commentModel.Description ,
                RoomTypeName=commentModel.RoomType.Type ,
                RoomViewName=commentModel.RoomView.View ,
                BuildingName=commentModel.Building.BuildingName


            };
        }
    }
}
