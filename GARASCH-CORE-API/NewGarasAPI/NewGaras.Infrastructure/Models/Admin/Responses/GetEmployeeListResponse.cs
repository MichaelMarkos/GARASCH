using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Admin.Responses
{
    public class GetEmployeeListResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public List<EmployeeBasicInfo> EmployeeInfoList { get; set; }
        public PaginationHeader PaginationHeader { get; set; }
    }
}
