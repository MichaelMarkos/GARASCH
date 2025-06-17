using NewGarasAPI.Models.Admin;
using NewGarasAPI.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Admin
{
    public class GetAddEmployeeScreenDataResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public List<JobTitleData> JobTitelList { get; set; }
        public List<BranchData> BranchList { get; set; }
        public List<DepartmentData> DepartmentList { get; set; }
        public List<GenderData> GenderList { get; set; }
    }
}
