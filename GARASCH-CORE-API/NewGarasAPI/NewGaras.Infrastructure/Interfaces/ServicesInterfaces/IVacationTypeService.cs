using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.VacationType;
using NewGaras.Infrastructure.Models.HR;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IVacationTypeService
    {
        public BaseResponseWithId<int> AddVacationType([FromForm]ContractLeaveSettingDto dto,long creator);
        public BaseResponseWithId<int> EditVacationType([FromForm]ContractLeaveSettingDto dto,long creator);

        public BaseResponseWithData<List<ContractLeaveSettingDto>> GetVacationTypesList();
        public BaseResponseWithData<List<long>> EditVacationTypesForUser(List<VacationTypesForUser> list,long creator);
        public BaseResponseWithData<List<VacationTypesForUser>> GetVacationTypesForUser(long HrUserId);
        public BaseResponseWithData<List<DDlVacationTypesForUser>> GetDDlVacationTypesForUser(long HrUserId);
        public BaseResponseWithData<ContractLeaveSettingDto> GetVacationType(int VacationTypeId);
        public BaseResponseWithId<int> DeleteLeaveType(int LeaveTypeId);
        public BaseResponseWithId<int> ArchiveLeaveType(int LeaveTypeId, bool Archive, long creator);
    }
}
