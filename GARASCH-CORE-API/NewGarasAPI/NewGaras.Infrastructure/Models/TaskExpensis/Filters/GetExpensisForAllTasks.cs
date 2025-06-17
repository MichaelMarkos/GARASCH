using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.TaskExpensis.Filters
{
    public class GetExpensisForAllTasks
    {
        [FromHeader]
        public long ProjectID { get; set; }
        [FromHeader]
        public bool GroupByDate { get; set; }
        [FromHeader]
        public bool GroupByTask { get; set; }
        [FromHeader]
        public bool GroupByUser { get; set; }
        [FromHeader]
        public int ItemPerPage { get; set; } = 10;
        [FromHeader]
        public int CurrrenPage { get; set; } = 1;
    }
}
