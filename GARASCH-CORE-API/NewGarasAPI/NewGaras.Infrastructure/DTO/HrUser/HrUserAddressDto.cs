using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class HrUserAddressDto
    {
        public long ID { get; set; }
        public long HrUserID { get; set; }
        public int CountryID { get; set; }
        public int GovernorateID { get; set; }

        public int? CityID { get; set; }
        public long? DistrictID { get; set; }
        public long? AreaID { get; set; }

        public string Address { get; set; } = string.Empty;
        public string ZipCode { get; set; }

        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }

        public long? GeographicalNameID { get; set; }
        public string? Description { get; set; }
        public string? Street { get; set; }

        public int? HouseNumber { get; set; }
        public int? FloorNumber { get; set; }
        public int? ApartmentNumber { get; set; }
    }
}
