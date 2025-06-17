

namespace NewGaras.Infrastructure.DTO.Hotel.DTOs
{
    public class GetDetailsClientbyIdViewModel
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Mobile { get; set; }
        public long CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string? MaritalStatus { get; set; }                      ////// andro
        public string? Gender { get; set; }
        public string? DOB { get; set; }
        public List<string>? languages { get; set; }
        public string? Nationality { get; set; }
        public string ImagePath { get; set; }
        public List<AddressDtoById> addressDtoByIds { get; set; }
        public List<GetClientImages> Images { get; set; }

    }
    public class GetClientImages
    {
        public string? TypeImage { get; set; }
        public string? Image { get; set; }
    }
    public class AddressDtoById
    {
        public string Country { get; set; }
        public string Governorate { get; set; }
        public string? Area { get; set; }
        public string Address { get; set; }
        public string? BuildingNumber { get; set; }
        public string? Floor { get; set; }
        public string? Description { get; set; }
        //public long CreatedBy { get; set; } = 1;
        //public DateTime CreationDate { get; set; } 
        public bool? Active { get; set; }


    }
}
