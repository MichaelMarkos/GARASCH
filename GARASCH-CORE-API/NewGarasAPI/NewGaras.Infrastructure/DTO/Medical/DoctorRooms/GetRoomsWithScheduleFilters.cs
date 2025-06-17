using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.DoctorRooms
{
    public class GetRoomsWithScheduleFilters
    {
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public DateTime Day {  get; set; }
        [FromHeader]
        public bool SortByDoctornameAsce { get; set; }
        [FromHeader]
        public bool SortByDoctornameDesc { get; set; }
        [FromHeader]
        public int? BranchID { get; set; } 
    }
}
