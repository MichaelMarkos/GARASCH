using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Admin
{
    public class EmployeeBasicInfo
    {
        public int? ID { get; set; }
        public string IDEnc { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string JobTitleName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string BranchName { get; set; }
        public bool Status { get; set; }
        public string Photo { get; set; }
        public int? JobTitleID { get; set; }
        public int? DepartmentID { get; set; }
    }
}
