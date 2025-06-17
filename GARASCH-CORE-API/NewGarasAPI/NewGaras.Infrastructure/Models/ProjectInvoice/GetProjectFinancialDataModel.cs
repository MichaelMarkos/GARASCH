using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectInvoice
{
    public class GetProjectFinancialDataModel
    {
        public decimal ProjectBudget { get; set; } = 0;

        public decimal TotalTasksBudget { get; set; } = 0;

        public decimal ExpensisTotalAmount { get; set; } = 0;
        public decimal InvoicesCollected { get; set; } = 0;
        public decimal InvoicesRemain { get; set; } = 0;
        public decimal InvoicesAmount { get; set; } = 0;
        public decimal InvoicesAmountExtra { get; set; } = 0;
        public decimal InvoicesAmountBasic { get; set; } = 0;
        public decimal GrossProfit { get; set; } = 0;

        public decimal WorkingHours { get; set; } = 0;

        public decimal DirectExpenses { get; set; } = 0;

        public decimal UnitRateService { get; set; } = 0;
        public decimal RemainingTobeCollected { get; set; } = 0;
        public decimal RemainingTobeInvoiced { get; set; } = 0;
    }
}
