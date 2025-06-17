

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class AddlistreservationByIDDto
    {
        public string? RoomName { get; set; }
        public int? RoomId { get; set; }
        public string? MealName { get; set; }
        public int? NumAdults { get; set; }
        public int? NumChildern { get; set; }
        public List<int>? YearsofChildern { get; set; }
    }
}
