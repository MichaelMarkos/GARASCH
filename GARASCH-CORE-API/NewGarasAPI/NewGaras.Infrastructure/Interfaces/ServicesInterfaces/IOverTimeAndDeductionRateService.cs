using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.OverTimeAndDeductionRate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IOverTimeAndDeductionRateService
    {
        public BaseResponseWithId<long> AddOverTimeAndDeductionRate(OverTimeDeductionRateDto dto,long creator);

        public BaseResponseWithId<long> UpdateOverTimeAndDeductionRate(OverTimeDeductionRateDto dto, long creator);

        public BaseResponseWithData<List<OverTimeDeductionRateDto>> GetOverTimeAndDeductionRate([FromHeader] int branchId);
        public BaseResponseWithId<long> DeleteOverTimeAndDeductionRate(long RateId);
    }
}
