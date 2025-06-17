using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.Hotel;


namespace NewGaras.Infrastructure.Interfaces.Hotel
{
    public interface ILabortoryService
    {
        public Task<BaseResponseWithDataPaging<List<LaboratoryMessagesReport>>> GetLaboratoryMessagePagingList(LaboratoryHeader filters);
        public Task<BaseResponseWithData<string>> MessageReportExcell(LaboratoryHeader filters);


    }
}
