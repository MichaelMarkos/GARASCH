using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.MedicalFinance
{
    public class AddMedicalFinancialDTO
    {
        public long ID { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal? ClosingBalance { get; set; }
        public decimal? TotalReceipts { get; set;}
        public decimal? ReservationAmount { get; set; }
        public decimal? Difference { get; set; }

    }
}
