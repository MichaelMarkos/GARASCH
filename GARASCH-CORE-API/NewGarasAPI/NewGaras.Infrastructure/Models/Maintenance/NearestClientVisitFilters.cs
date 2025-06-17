using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class NearestClientVisitFilters
    {
        [FromHeader]
        public long VisitMaintinanceID { get; set; }
        [FromHeader]
        public long AssignTo {  get; set; }
        [FromHeader]
        public decimal Latitude { get; set; }
        [FromHeader]
        public decimal Longitude { get; set; }
        [FromHeader]
        public decimal Radius { get; set; }
    }
}
