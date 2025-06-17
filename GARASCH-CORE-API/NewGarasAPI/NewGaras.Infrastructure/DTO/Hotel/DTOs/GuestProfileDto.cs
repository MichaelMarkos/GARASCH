

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class GuestProfileDto
    {
        //public long? Id { get; set; }
        public long? ClientId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string? Email { get; set; }
        public int? NationalityId { get; set; }
        public long CreatedBy { get; set; } 
        public DateTime CreationDate { get; set; }
        public long SalesPersonId { get; set; } = 1;
        //public DateTime? FirstContractDate { get; set; }= DateTime.Now.Date;
        public DateTime? FirstContractDate { get; set; }
        public int FollowUpPeriod { get; set; } = 4;
        public int? MaritalStatusId { get; set; }                      ////// andro
        public string? Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string? Mobile { get; set; }
        public List<int>? languageeId { get; set; }
        // public List<AddressDto>? addressDtos { get; set; }
        // public List<ClientinformatinDto>? ClientinformatinDtos { get; set; }


    }
}
