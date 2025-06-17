using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.MedicalFinance
{
    public class AddOpeningMedicalFinancialDTO
    {
        public decimal OpeningBalance { get; set; }
        public int PosNumberId { get; set; }
        public string Type { get; set; }
    }
}
