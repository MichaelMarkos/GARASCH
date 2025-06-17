using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.TaskMangerProject.Filters
{
    public class TaskMangerProjectsFilters
    {
        [FromHeader]
        public string? ProjectName { get; set; }
        [FromHeader]
        public long? CreatorID { get; set; }
        [FromHeader]
        public bool? closed { get; set; }
        [FromHeader]
        public bool NotActive { get; set; } = false;
        [FromHeader]
        public bool IsArchived { get; set; } = false;
        [FromHeader]
        public int CurrentPage { get; set; }
        [FromHeader]
        public int numberOfItemsPerPage { get; set; }

    }
}
