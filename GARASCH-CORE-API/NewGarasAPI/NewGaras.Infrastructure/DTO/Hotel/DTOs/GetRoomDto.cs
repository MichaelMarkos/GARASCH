

using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class GetRoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public RoomType? RoomType { get; set; }
        //public Building? Building { get; set; }
        //public RoomView? RoomView { get; set; }
        public string? RoomTypeName { get; set; }
        public string? BuildingName { get; set; }
        public string? RoomViewName { get; set; }
        public List<Facility> Facilities { get; set; }
        public List<string>? FacilityName { get; set; }
        public int Rate { get; set; }
    }
}
