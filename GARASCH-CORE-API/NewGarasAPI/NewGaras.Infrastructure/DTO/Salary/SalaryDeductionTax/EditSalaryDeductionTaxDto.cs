using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Salary.SalaryDeductionTax
{
    public class EditSalaryDeductionTaxDto
    {
        public long Id { get; set; }
        public long SalaryId { get; set; }
        public string TaxName { get; set; }

        public decimal Percentage { get; set; }

        public decimal Amount { get; set; }


        public int? DeductionTypeId { get; set; }

        public long? SalaryTaxId { get; set; }
        public bool ActiveSalaryDeductionTax { get; set; }
    }
}
