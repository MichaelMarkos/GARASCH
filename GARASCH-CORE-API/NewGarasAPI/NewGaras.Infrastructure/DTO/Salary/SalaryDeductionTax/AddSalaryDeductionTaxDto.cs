using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Salary.SalaryDeductionTax
{
    public class AddSalaryDeductionTaxDto
    {
        public long SalaryId { get; set; }
        public string TaxName { get; set; }

        public decimal Percentage { get; set; }

        public decimal Amount { get; set; }

        public bool Active { get; set; }

        public int? DeductionTypeId { get; set; }

        public long? SalaryTaxId { get; set; }
    }
}
