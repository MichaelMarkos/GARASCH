using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.ProjectManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.ProjectManagment
{
    public interface IProjectProgressService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithId<int> AddNewProgressType([FromBody] ProgressTypeDto dto, long creator);
        public BaseResponseWithId<int> UpdateProgressType([FromBody] ProgressTypeDto dto,long creator);

        public BaseResponseWithData<ProgressTypeDto> GetProgressTypeById([FromHeader] int progressId);
        public BaseResponseWithData<List<ProgressTypeDto>> GetProgressTypeList();

        public BaseResponseWithData<List<DeliveryTypeDto>> GetDeliveryTypeList();
        public BaseResponseWithData<List<ProgressStatusDto>> GetProgressStatusList();

        public BaseResponseWithId<long> AddNewProjectProgress([FromForm] ProjectProgressDto dto, long creator, string CompanyName);
        public BaseResponseWithId<long> UpdateProjectProgress([FromForm] ProjectProgressDto dto, long creator, string CompanyName);
        public BaseResponseWithData<GetProjectProgressDto> GetProjectProgressById([FromHeader] long ProjectProgressId);
        public BaseResponseWithData<List<GetProjectProgressDto>> GetProjectProgressList([FromHeader] long projectId);
        public BaseResponseWithId<long> AddProgressTypeForProject([FromHeader] long ProjectId);
    }
}
