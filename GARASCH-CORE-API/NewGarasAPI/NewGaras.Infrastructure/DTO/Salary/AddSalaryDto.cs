using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.DTO.Salary
{
    public class AddSalaryDto
    {
        public long? Id { get; set; }
        public long ContractId { get; set; }
        public int CurrencyId { get; set; }

        public decimal TotalGross { get; set; }

        public decimal TotalNet { get; set; }

        public decimal TotalIncome { get; set; }

        public decimal MultiplyingFactor { get; set; }

        public int PaymentMethodId { get; set; }
        public int PaymentStrategyId { get; set; }
        public long? HrUserId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal VariantSalary { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string Reason { get; set; }
    }
}
