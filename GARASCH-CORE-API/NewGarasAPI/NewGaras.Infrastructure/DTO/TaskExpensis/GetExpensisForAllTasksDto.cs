using NewGaras.Infrastructure.Models.TaskExpensis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskExpensis
{
    public class GetExpensisForAllTasksDto
    {
        public List<GetTaskExpensisDto> ExpensisList { get; set; }

        public List<GroupedExpensis> GroupedExpenses { get; set; }

        public string FilePath { get; set; }
    }
}
