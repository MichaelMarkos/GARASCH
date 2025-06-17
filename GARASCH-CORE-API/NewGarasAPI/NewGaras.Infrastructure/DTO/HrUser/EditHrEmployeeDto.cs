using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class EditHrEmployeeDto : HrUserDto
    {
        public long HrUserId { get; set; }
        public string systemEmail { get; set; }
        public string password { get; set; }
        public string confirmPassword { get; set; }

        public bool IsUser { get; set; }
    }
}
