using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Salary.SalaryAllownces
{
    public class AddSalaryAllowncesDto
    {
        public long SalaryID { get; set; }

        public int AllowncesTypeID { get; set; }

        public decimal Amount { get; set; }
        public decimal percentage { get; set; }

    }
}
