using NewGaras.Domain.DTO.HrUser;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.JobTitle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IJobTitleService
    {
        public Task<BaseResponseWithData<GetJobTilteDto>> GetById(long Id);
        public Task<BaseResponseWithData<List<GetAllJobTilteDto>>> GetAll();

        public Task<BaseResponseWithId<long>> AddJobTilte(AddJobTitleDto NewJobTitle, long UserId);

        public Task<BaseResponseWithId<long>> EditJobTitle(EditJobTitleDto newJobTitle, long UserId);

        public BaseResponseWithId<long> DeleteJobTitle(long JobTitleId);
        public BaseResponseWithId<long> ArchiveJobTitle(long JobTitleId, bool Archive, long creator);
    }
}
