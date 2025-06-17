using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ProjectPayment
{
    public class GetProjectPaymentTermsDto
    {
        public long ID { get; set; }
        public long ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int PaymentTermID { get; set; }
        public string PaymentTermName { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Amount { get; set; }
        public int CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public string DueDate { get; set; }
        public decimal Collected { get; set; }
        public string CollectionDate { get; set; }
        public decimal Remain { get; set; }
        public long DailyJournalEntryID { get; set; }
    }
}
