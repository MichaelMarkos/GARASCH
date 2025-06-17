using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Salary.AllowncesType
{
    public class AddAllowanceTypeDto
    {
        public string Type { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Amount { get; set; }
        public int CurrencyID { get; set; }
        public int SalaryTypeID { get; set; }
        
    }
}
