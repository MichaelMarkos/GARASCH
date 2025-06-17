using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.Shift;
using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IShiftService
    {
        public BaseResponseWithId<long> AddShift(AddShiftDto shiftDto,long creator);

        public BaseResponseWithId<long> UpdateShift(List<AddShiftDto> shiftDto, long creator);

        public BaseResponseWithId AddListOfShifts(AddListOfShiftsDto dto, long creator);

        public Task<BaseResponseWithData<List<GetBranchScheduls>>> GetShifts(int branchId);

        public BaseResponseWithData<BranchScheduleDto> GetShift(long shiftId);

        public bool CheckShiftOverlapping(AddShiftDto shiftDto);
        public bool CheckShiftListOverlapping(List<AddShiftDto> shiftDto);

        public Tuple<int, int> CalculateNOBranchWorkingDay(int BranchId);

    }
}
