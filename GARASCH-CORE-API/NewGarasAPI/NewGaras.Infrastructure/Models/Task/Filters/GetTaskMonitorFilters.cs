using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task.Filters
{
    public class GetTaskMonitorFilters
    {
        [FromHeader]
        public long? UserId { get; set; }
        [FromHeader]
        public long? TaskId { get; set; }
        [FromHeader]
        public long? ProjectId { get; set; }
        [FromHeader]
        public DateTime? From { get; set;}
        [FromHeader]
        public DateTime? To { get; set;}
    }
}
