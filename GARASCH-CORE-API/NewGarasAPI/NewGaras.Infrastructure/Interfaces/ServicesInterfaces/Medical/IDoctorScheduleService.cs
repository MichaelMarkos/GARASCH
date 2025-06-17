using NewGaras.Infrastructure.DTO.Medical.DoctorSchedule;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Medical;
using NewGaras.Infrastructure.Models.Medical.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical
{
    public interface IDoctorScheduleService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithId<long> AddDoctorSchedulestatus(string DoctorScheduleStatusType);

        public SelectDDLResponse GetDoctorSchedulestatus();
        public BaseResponseWithId<long> AddDoctorSchedule(DoctorScheduleDTO dto);

        public BaseResponseWithId<long> EditDoctorSchedule(EditDoctorScheduleDTO dto);

        public BaseResponseWithDataAndHeader<GetDoctorScheduleList> GetDoctorScheduleList(GetDoctorScheduleListFilters filters);

        public BaseResponseWithId<long> AddExaminationAndExaminationPrices(AddExaminationAndExaminationPricesDTO dto);
        public BaseResponseWithData<List<long?>> GetRoomsList();
        public BaseResponseWithData<GetDoctorScheduleGroupByDocNameList> GetDoctorScheduleListGroupByDoctorName(GetDoctorScheduleListGroupByDoctorNameFilters filters);
        public SelectDDLResponse GetPercentageTypeListForDoctorSchedule();
        public SelectDDLResponse GetSpecialityListForDoctorSchedule(long? DoctorID);
        public SelectDDLResponse GetWeekDaysListForDoctorSchedule();
        public BaseResponseWithId<long> CancelDoctorSchedule(CancelDoctorScheduleDTO data ,long userID);
        public BaseResponseWithData<List<GetDoctorSchedulePerWeekDTO>> GetDoctorSchedulePerWeek(GetDoctorSchedulePerWeekFilters filters);
    }
}
