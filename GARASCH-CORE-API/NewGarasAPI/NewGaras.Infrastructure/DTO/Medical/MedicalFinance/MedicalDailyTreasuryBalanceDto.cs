using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Medical.MedicalFinance
{
    public class MedicalDailyTreasuryBalanceDto
    {
        public long Id { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalReceipts { get; set; }
        public decimal ReservationAmount { get; set; }
        public decimal Difference { get; set; }
        public bool IsOpeningBalance { get; set; }
        public long CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string ClosingDate { get; set; }
        public long ModifiedBy { get; set; }
        public long? ReceivedFrom { get; set; }
        public int? PosNumberId { get; set; }

        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
        public string ReceivedFromName { get; set; }
        public string PosNumberName { get; set; }
        public string Type { get; set; }
    }
}
