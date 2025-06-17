using NewGaras.Infrastructure.DTO.Salary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Contract
{
    public class GetContractWithSalaryDto
    {
        public GetContractDto Contract { get; set; }
        public GetSalaryDto Salary { get; set; }
    }
}
