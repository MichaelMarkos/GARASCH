using NewGaras.Infrastructure.Models.Task.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task
{
    public class GetTaskAppsOpenList
    {
        public List<GetTaskAppsOpenDto> AppsList { get; set; }
    }
}
