

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class RateDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartingDate { get; set; }
        public DateTime? EndingDate { get; set; }
        public int RoomOfferRate { get; set; }
        public int? RoomTypeId { get; set; }
        public int? BuildingId { get; set; }
        public int? RoomViewId { get; set; }
        public byte SpecialOfferFlag { get; set; }
    }
}
