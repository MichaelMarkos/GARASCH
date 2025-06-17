using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.Branch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IBranchService
    {
        public BaseResponseWithId<long> AddBranch(AddBranchDto branchDto, long creator);
        public BaseResponseWithId<long> EditBranch(AddBranchDto branchDto, long creator);

        public BaseResponseWithData<List<GetBranchDto>> GetAllBranches();

        public BaseResponseWithData<GetBranchDto> GetBranch(int BranchId);

        public BaseResponseWithId<long> DeleteBranch(int BranchId);

        public BaseResponseWithId<long> DeleteBranchByInclude(int BranchId);
        public BaseResponseWithId<long> ArchiveBranch(int BranchId, bool Archive, long creator);

        public Task<SelectDDLResponse> GetBranchesList();
    }
}
