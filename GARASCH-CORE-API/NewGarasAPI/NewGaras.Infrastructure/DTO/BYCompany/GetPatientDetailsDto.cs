using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.BYCompany
{
    public class GetPatientDetailsDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Photo { get; set; }
        public string Email { get; set; }

        public string Mobile { get; set; }
        public long userId { get; set; }
        public List<InsuranceDto> Insurance { get; set; }

        public int? countryid { get; set; }
        public string countryName { get; set; }

        public int? cityid { get; set; }
        public string cityName { get; set; }

        public long? AreaId { get; set; }
        public string AreaName { get; set; }

        public string Address { get; set; }
        public string Description { get; set; }
        public DateTime? CreationDate { get; set; }
    }
}
