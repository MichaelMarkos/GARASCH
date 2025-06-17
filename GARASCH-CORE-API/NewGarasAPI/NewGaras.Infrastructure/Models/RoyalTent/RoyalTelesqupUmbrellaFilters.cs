using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.RoyalTent
{
    public class RoyalTelesqupUmbrellaFilters
    {
        [FromHeader]
        public string Sales {  get; set; }
        [FromHeader]
        public string Size {  get; set; }
        [FromHeader]
        public string Paint {  get; set; }
        [FromHeader]
        public string Cloth {  get; set; }
        [FromHeader]
        public string Fronton {  get; set; }
    }
}
