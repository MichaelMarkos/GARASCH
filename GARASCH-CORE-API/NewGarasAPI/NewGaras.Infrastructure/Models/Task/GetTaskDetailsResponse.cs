using NewGarasAPI.Models.TaskManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task
{
    public class GetTaskDetailsResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public GetTaskDetailsData GetTaskDetailsList {  get; set; }
    }
}
