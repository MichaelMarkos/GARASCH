


using NewGaras.Infrastructure.DTO.Hotel.Validators;

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class NewRateDto
    {
        [FeatureDate(ErrorMessage = "Start date mast starts from tommorrow")]
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
        public byte SpecialOfferFlag { get; set; }
        public int? RoomTypeId { get; set; }
        public int? BuildingId { get; set; }
        public int? RoomViewId { get; set; }
        public List<int> RoomsIds { get; set; }
        public List<int> RoomsRates { get; set; }
    }
}
