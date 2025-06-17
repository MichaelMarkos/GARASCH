using NewGaras.Infrastructure.DTO.TaskExpensis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.TaskExpensis
{
    public class GroupedExpensis
    {
        public string Key { get; set; }
        public List<GetTaskExpensisDto> ExpensisList { get; set; }
    }
}
