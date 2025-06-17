using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.MedicalFinance
{
    public class AddClosingMedicalFinancialDTO
    {
        public long ID { get; set; }
        public decimal? ClosingBalance { get; set; }
        public decimal? TotalReceipts { get; set; }

    }
}
