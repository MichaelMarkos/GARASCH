using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.BYCompany
{
    public class PatientDto
    {
        public long? PatientId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public IFormFile? Photo { get; set; }
        public string Email { get; set; }

        public string Mobile { get; set; }
        public bool? Active { get; set; }
        public List<InsuranceDto> Insurances { get; set; }
        public int? CityId { get; set; }
        public int? CountryId { get; set; }
        public long? AreaId { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public DateTime? CreationDate { get; set; }


    }
}
