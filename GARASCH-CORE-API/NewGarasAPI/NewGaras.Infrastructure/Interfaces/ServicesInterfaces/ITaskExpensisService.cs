using NewGaras.Domain.DTO.Salary;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.TaskExpensis;
using NewGaras.Infrastructure.Models.TaskExpensis;
using NewGaras.Infrastructure.Models.TaskExpensis.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface ITaskExpensisService
    {
        public BaseResponseWithData<AddExpensis> AddTaskExpensis(AddTaskExpensisDto Dto, long Creator,  string CompName);

        public BaseResponseWithData<AddExpensis> EditTaskExpensis(EditTaskExpensisDto Dto, long Creator, string CompName);

        public BaseResponseWithData<List<GetTaskExpensisDto>> GetTaskExpensisList(long TaskId);

        public BaseResponseWithData<GetTaskExpensisDto> GetTaskExpensisByID(long Id);
        public BaseResponseWithId<long> AcceptTaskExpensisByManger(long ExpensisId, bool Approved, long manger);
        public BaseResponseWithData<GetExpensisForAllTasksDto> GetExpensisForAllTasks(GetExpensisForAllTasks filters, string companyName);
        public BaseResponseWithId<long> DeleteTaskExpensis(long Id);
    }
}
