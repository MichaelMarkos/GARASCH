using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.Department;
using NewGaras.Infrastructure.DTO.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IDepartmentService
    {
        public BaseResponseWithId<int> AddDepartmnet(AddDepartmentDto departmentDto,long creator);

        public BaseResponseWithData<GetDepartmentDto> GetDepartment(long DepartmentId);

        public BaseResponseWithId<int> EditDepartmnet(AddDepartmentDto departmentDto, long creator);

        public BaseResponseWithData<List<GetDepartmentDto>> GetBranchDepartments(int BranchId);

        public BaseResponseWithId<long> EditTeam(TeamDto teamDto,long creator);
        public BaseResponseWithId<long> DeleteDepartment(int DepartmentId);
        public BaseResponseWithId<long> ArchiveDepartment(int DepartmentId, bool Archive, long creator);
    }
}
