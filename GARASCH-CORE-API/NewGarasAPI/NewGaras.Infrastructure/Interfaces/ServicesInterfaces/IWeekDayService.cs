using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Models;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IWeekDayService
    {
        public BaseResponseWithData<List<WeekDayDto>> GetWeekDays([FromHeader] int BranchId);

        public BaseResponse UpdateWeekDays(UpdateWeekDaysModel Dto);
    }
}
