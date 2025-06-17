

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class SpecialOfferFlag
    {
        public DateTime? StartingDate { get; set; }
        public DateTime? EndingDate { get; set; }
        public byte SpecialOfferFlags { get; set; } = 0;
    }
}
