using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.ProjectSprint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface ISprintService
    {
        public Task<BaseResponseWithId<long>> AddProjectSprint(AddProjectSprintDto sprintDto, long creatorID);
        public BaseResponseWithData<GetProjectsprintDto> GetProjectSprintByID(long ID);
        public BaseResponseWithData<List<GetProjectsprintDto>> GetProjectSprintList(long ProjectId);
        public BaseResponseWithId<long> EditProjectSprint(EditProjectSprint sprintDto, long editor);
        public BaseResponseWithId<long> DeleteProjectSprint(long Id);
    }
}
