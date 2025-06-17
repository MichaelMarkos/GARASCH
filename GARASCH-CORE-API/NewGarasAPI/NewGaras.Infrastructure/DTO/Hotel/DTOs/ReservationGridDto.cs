

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class ReservationGridDto
    {
        public int Id { get; set; }
        public DateTime? reservationDate { get; set; } = DateTime.Now;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long ClientId { get; set; }
        public string Provider { get; set; }
        public bool Confirmation { get; set; }
        public string ClientName { get; set; }
    }
}
