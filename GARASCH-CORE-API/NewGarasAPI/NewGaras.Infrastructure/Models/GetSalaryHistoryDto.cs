using NewGaras.Infrastructure.DTO.Salary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetSalaryHistoryDto
    {
        public long HrUserId { get; set; }
        public string HrUserName { get; set; }
        public List<GetSalaryDto> SalaryHistory { get; set; } = new List<GetSalaryDto>();
    }
}
