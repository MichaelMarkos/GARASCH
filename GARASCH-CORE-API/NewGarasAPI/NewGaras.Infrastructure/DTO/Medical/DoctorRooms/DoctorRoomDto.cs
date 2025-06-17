using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.DoctorRooms
{
    public class DoctorRoomDto
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public bool Active {  get; set; }
        public int? BranchID { get; set; }
    }
}
