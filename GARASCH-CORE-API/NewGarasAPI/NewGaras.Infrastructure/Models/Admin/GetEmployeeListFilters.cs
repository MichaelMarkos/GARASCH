using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Admin
{
    public class GetEmployeeListFilters
    {
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public long BranchID { get; set; }
        [FromHeader]
        public long DepartmentID { get; set; }
        [FromHeader]
        public long UserID { get; set; }
        [FromHeader]
        public bool isExpired { get; set; }
        [FromHeader]
        public DateTime expiredIn { get; set; } = DateTime.Now;
    }
}
