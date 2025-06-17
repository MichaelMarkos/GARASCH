using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.DTO.Medical.DoctorRooms;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical
{
    public interface IDoctorRoomService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithId<long> AddDoctorRoom(DoctorRoomDto dto);
        public BaseResponseWithId<long> UpdateDoctorRoom(DoctorRoomDto dto);

        public BaseResponseWithDataAndHeader<GetDoctorRooms> GetDoctorRoomList(GetDoctorRoomListFilters filters);

        public BaseResponseWithData<DoctorRoomDto> GetDoctorRoomById([FromHeader] long RoomId);

        public BaseResponseWithDataAndHeader<RoomsWithSchedule> GetRoomsWithSchedule(GetRoomsWithScheduleFilters filters);
    }
}
