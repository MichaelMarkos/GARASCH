

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class AddListofReservationDto
    {
        public int RoomId { get; set; }
        public int? MealId { get; set; }
        public int NumbersofAud { get; set; }
        public int? NumbersofChildern { get; set; }
        public int RoomRate { get; set; }
        public long InventoryItemId { get; set; }
        public List<int>? Years { get; set; }
    }
}
