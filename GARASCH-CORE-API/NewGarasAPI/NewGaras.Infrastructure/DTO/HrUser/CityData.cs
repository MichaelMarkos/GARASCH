using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class CityData
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public bool Active { get; set; }

        public string GovernorateID { get; set; }
    }
}
