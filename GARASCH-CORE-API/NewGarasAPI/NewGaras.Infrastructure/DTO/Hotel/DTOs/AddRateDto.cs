

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class AddRateDto
    {
        public int? Id { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
        public byte SpecialOfferFlag { get; set; }
        public int? RoomTypeId { get; set; }
        public int? BuildingId { get; set; }
        public int? RoomViewId { get; set; }
        public bool IsDefault { get; set; } 
        public bool IsActive { get; set; } 
        public int RoomsId { get; set; }
        public int RoomRate { get; set; }
    }
}
