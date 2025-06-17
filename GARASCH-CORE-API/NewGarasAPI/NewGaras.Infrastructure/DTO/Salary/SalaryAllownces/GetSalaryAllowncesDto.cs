using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Salary.SalaryAllownces
{
    public class GetSalaryAllowncesDto
    {
        public int Id { get; set; }
        public long SalaryID { get; set; }

        public int AllowncesTypeID { get; set; }
        public string AllowncesTypeName { get; set; }

        public decimal Amount { get; set; }
        public decimal percentage { get; set; }
    }
}
