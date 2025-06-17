using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class ClientAddressData
    {
        public long? ID { get; set; }
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public int GovernorateID { get; set; }
        public string GovernorateName { get; set; }
        public string Address { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string BuildingNumber { get; set; }
        public string Floor { get; set; }
        public string Description { get; set; }
        public long? AreaID { get; set; }
        public string AreaName { get; set; }
        public bool Active { get; set; }
        public decimal? latitude { get; set; }
        public decimal? longtitud { get; set; }
    }
}
