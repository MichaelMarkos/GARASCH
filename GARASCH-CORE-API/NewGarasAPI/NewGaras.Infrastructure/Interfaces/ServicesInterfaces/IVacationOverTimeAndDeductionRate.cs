using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.OverTimeAndDeductionRate;
using NewGaras.Infrastructure.DTO.VacationOverTimeAndDeductionRates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IVacationOverTimeAndDeductionRate
    {
        public BaseResponseWithData<List<long>> AddVacationOverTimeAndDeductionRateList(List<VacationOverTimeDeductionRateDto> dto, long creator);

        public BaseResponseWithId<long> UpdateVacationOverTimeAndDeductionRate(VacationOverTimeDeductionRateDto dto, long creator);

        public BaseResponseWithData<List<VacationOverTimeDeductionRateDto>> GetVacationOverTimeAndDeductionRate([FromHeader] long VacationDayId);

        public BaseResponseWithId<long> DeleteVacationOverTimeAndDeductionRate(long RateId);
    }
}
