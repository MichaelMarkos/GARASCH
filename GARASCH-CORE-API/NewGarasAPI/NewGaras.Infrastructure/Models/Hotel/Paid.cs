

namespace NewGaras.Infrastructure.Models.Hotel
{
    public class Paid
    {
       
        public int Id { get; set; }

        public DateTime InvoiceDate { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreateDate { get; set; }

        public long CreateBy { get; set; }

       
        public string? Serial { get; set; }

        public bool IsClosed { get; set; }

        public long ClientId { get; set; }

        public int ReservationId { get; set; }

        public int InvoiceTypeId { get; set; }

        public int CurrencyId { get; set; } = 5;

       
    }
}
