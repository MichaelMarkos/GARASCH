

using NewGaras.Infrastructure.DTO.Hotel.Validators;

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class ReservationDto
    {
        public int Id { get; set; }
        [FeatureDate]
        public DateTime FromDate { get; set; }
        [FeatureDate]
        public DateTime ToDate { get; set; }
        public List<AddListofReservationDto> ListRooms { get; set; }
        public long ClientId { get; set; }
        public string Provider { get; set; }
        public string OfferType { get; set; }
        public bool Confirmation { get; set; }

        public decimal TotalCost { get; set; } 

        public decimal TotalPaid { get; set; } = 0;
        public int? InvoiceTypeId { get; set; }
        public int? CurrencyId { get; set; }
        public string? InvoiceSerial { get; set; }
        public long OfferId { get; set; }

    }
}
