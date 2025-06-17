

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class ReservationRoomsByIDDto
    {
        public int Id { get; set; }
        public DateTime? reservationDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long ClientId { get; set; }
        public string? ClientName { get; set; }
        public string Provider { get; set; }
        public bool Confirmation { get; set; }
        public virtual List<AddlistreservationByIDDto>? Reservationdetails { get; set; } 
    }
}
