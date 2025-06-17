

using NewGaras.Infrastructure.DTO.Hotel.DTOs;
using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class RoomReservationDto
    {
        public RoomDto2 Room { get; set; }
        public List<ReservationGridDto> reservations { get; set; }
        public List<RateDto> rate { get; set; }
        //public byte? SpecialOfferFlag { get; set; }
        //public DateTime? StartingDate { get; set; }
        //public DateTime? EndingDate { get; set; }

    }
}
