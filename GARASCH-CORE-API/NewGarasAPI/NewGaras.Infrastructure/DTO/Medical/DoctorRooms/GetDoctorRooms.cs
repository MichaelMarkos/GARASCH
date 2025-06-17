using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.DTO.Medical.DoctorRooms
{
    public class GetDoctorRooms
    {
        public List<DoctorRoomDto> DoctorRoomsList { get; set; }
    }
}
