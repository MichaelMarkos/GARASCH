using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetMaintenanceByDayFilters
    {
        [FromHeader]
        public bool IsOpen { get; set; } = false;
        [FromHeader]
        public bool IsDelayed { get; set; } = false;
        [FromHeader]
        public int? Year { get; set; } = 0;
        [FromHeader]
        public int? Month { get; set; } = 0;
        [FromHeader]
        public int? Day { get; set; } = 0;
        [FromHeader]
        public long? AssignToID { get; set; } = 0;
        [FromHeader]
        public string SearchKey { get; set; }
    }
}
