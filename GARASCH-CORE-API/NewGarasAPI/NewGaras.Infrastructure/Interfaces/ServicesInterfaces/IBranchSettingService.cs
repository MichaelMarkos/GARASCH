using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.BranchSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IBranchSettingService
    {
        public BaseResponseWithData<List<long>> AddBranchSetting([FromForm] BranchSettingDto dto, long creator);

        public BaseResponseWithData<List<long>> EditBranchSetting([FromForm] EditBranchSettingDto dto, long creator);

        public BaseResponseWithData<GetBranchSettingDto> GetBranchSetting([FromHeader] int branchId);
        public BaseResponseWithId<long> DeleteBranchSetting(long BranchSettingId);
    }
}
