
using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class ReservationRoomsDto
    {
        public int Id { get; set; }
        public DateTime? reservationDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool Confirmation { get; set; }
        public decimal TotalCost { get; set; }
        public decimal? TotalPaid { get; set; }

        public bool IsFinished { get; set; }
        public List<RoomModel> Rooms { get; set; }
       // public List<int> Rate { get; set; } 

    }
}
