using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.HR
{
    public class GetHrUserAddressDto
    {
        public long? ID { get; set; }
        public long HrUserID { get; set; }
        public string HrUserName { get; set; }
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public int GovernorateID { get; set; }
        public string GovernorateName { get; set; }
        public int? CityID { get; set; }
        public string CityName { get; set; }
        public long? DistrictID { get; set; }
        public string DistrictName { get; set; }
        public long? AreaID { get; set; }
        public string AreaName { get; set; }
        public bool Active { get; set; }
        public string Address { get; set; } = string.Empty;
        public string ZipCode { get; set; }

        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }

        public long? GeographicalNameID { get; set; }
        public string GeographicalName { get; set; }
        public string Description { get; set; }
        public string Street { get; set; }

        public int? HouseNumber { get; set; }
        public int? FloorNumber { get; set; }
        public int? ApartmentNumber { get; set; }
    }
}
