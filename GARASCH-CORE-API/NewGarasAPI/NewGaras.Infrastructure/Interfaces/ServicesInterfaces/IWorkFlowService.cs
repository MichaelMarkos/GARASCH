using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IWorkFlowService
    {
        public Task<BaseResponseWithId<long>> AddWorkFlow([FromForm] AddWorkFlowDto Dto, long creatorID);
        public BaseResponseWithData<GetWorkFlowDto> GetWorKFlowByID(long ID);
        public BaseResponseWithId<long> EditWorkFlow(EditWorkFlowDto Dto, long editor);
        public BaseResponseWithData<List<GetWorkFlowDto>> GetProjectWorkFlowList(long ProjectId);
    }
}
