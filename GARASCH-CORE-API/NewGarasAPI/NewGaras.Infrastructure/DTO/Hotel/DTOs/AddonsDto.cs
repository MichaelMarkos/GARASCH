

namespace NewGaras.Infrastructure.DTO.Hotel.DTOs
{
    public class AddonsDto
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreateDate { get; set; }

        public long CreateBy { get; set; }


        public bool IsClosed { get; set; }

        public long ClientId { get; set; }

        public int ReservationId { get; set; }

        public double Quantity { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public long InventoryItemId { get; set; }
        public string ItemPricingComment { get; set; }

       
    }
}
