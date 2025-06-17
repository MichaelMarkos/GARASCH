using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetPosClosingDay
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }

        public int StoreId { get; set; }

        public long UserId { get; set; }

        public decimal SalesCount { get; set; }

        public decimal SalesAmount { get; set; }

        public decimal SalesReturnCount { get; set; }

        public decimal SalesReturnAmount { get; set; }

        public decimal NetSalesCount { get; set; }

        public decimal NetSalesAmount { get; set; }

        public string Notes { get; set; }

        public decimal? ClosingDayAmount { get; set; }

        public long? JournalEntryId { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public long CreatedBy { get; set; }

        public long ModifiedBy { get; set; }


        public decimal HospitalityAmount { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal CashAmount { get; set; }

    }
}
