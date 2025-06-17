

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class ReservationUpdataDto
    {
        public int Id { get; set; }
        public DateTime? reservationDate { get; set; } = DateTime.Now;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<int> RoomIds { get; set; }
        public long ClientId { get; set; }
        public string Provider { get; set; }
        public bool Confirmation { get; set; }
    }
}
