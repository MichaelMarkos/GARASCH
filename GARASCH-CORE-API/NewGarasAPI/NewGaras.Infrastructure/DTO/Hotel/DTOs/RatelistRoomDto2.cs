
namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class RatelistRoomDto2
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int? Roomcapacity { get; set; }
        public List<DateRoomRate> dateroomrate { get; set; }
    }
    public class DateRoomRate 
    {
        public DateTime date { get; set; }
        public int RoomRate { get; set; }
    }
}
