using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.VacationDay;
using NewGaras.Infrastructure.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IVacationDayService
    {
        public BaseResponseWithId<long> AddVacationDay([FromForm] AddVacationDayDto dto,long creator);

        public BaseResponseWithId<long> UpdateVacationDay([FromForm] AddVacationDayDto dto,long creator);

        public BaseResponseWithData<GetVacationDayDto> GetVacationDay([FromHeader] long VacationDayId);

        public BaseResponseWithData<List<GetVacationDayDto>> GetVacationDayList([FromHeader] int branchId);

        public BaseResponseWithData<List<GetGroupedVacationDays>> GetVacationDaysTree(int branchId);

        public BaseResponseWithId<long> ArchiveVacationDay(long VacationdayId, bool Archive, long creator);

        public List<DateTime> GetDatesOfDayInMonth(int year, int month, DayOfWeek targetDay);

        public BaseResponseWithData<GetHolidaysOfMonthModel> GetHolidaysOfMonth([FromHeader] int BranchId, [FromHeader] int year, [FromHeader] int Month);

    }
}
