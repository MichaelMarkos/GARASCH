using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Salary.SalaryTax
{
    public class AddSalaryTaxDto
    {
        public decimal Percentage { get; set; }
        public long Min { get; set; }
        public long Max { get; set; }
        public bool? Active { get; set; }
        public int? BranchId { get; set; }

        //public int TaxTypeId { get; set; }
        public string TaxTypeName { get; set; }

        public int SalaryTypeId { get; set; }
    }
}
