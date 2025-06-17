

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class AddressDto
    {
        public long? ClientId { get; set; }
        public int CountryId { get; set; }
        public int GovernorateId { get; set; }
        public int AreaId { get; set; }
        public string Address { get; set; }
        public string? BuildingNumber { get; set; }
        public string? Floor { get; set; }
        public string? Description { get; set; }
        //public long CreatedBy { get; set; } = 1;
        //public DateTime CreationDate { get; set; } 
        public bool? Active { get; set; }


    }
}
