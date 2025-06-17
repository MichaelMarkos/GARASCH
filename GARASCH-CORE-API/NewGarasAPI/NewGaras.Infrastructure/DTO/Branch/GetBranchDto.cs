using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Branch
{
    public class GetBranchDto
    {
        public int? Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool? Active { get; set; }

        public bool IsMain { get; set; } = false;

        public int GovernorateId { get; set; }
        public string Governorate { get; set; }
        public int? Building { get; set; }

        public int? Floor { get; set; }
        public string Mobile { get; set; }

        public long? AreaId { get; set; }
        public string Area { get; set; }
        public int CountryId { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Description { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
        public int DepartmentNum { get; set; }

        public decimal? Diameter { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
