using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class AddNewCRMReport
    {
        public long? Id { get; set; }
        public string CrmUserId { get; set; }
        public int BranchId { get; set; }
        public string ReportDate { get; set; }
        public long ClientId { get; set; }
        public long ClientContactPersonId { get; set; }
        public bool IsNew { get; set; }
        public int? CrmContactTypeId { get; set; }
        public string OtherContactName { get; set; }
        public int? CrmRecievedTypeId { get; set; }
        public string OtherRecievedName { get; set; }
        public int? CrmReportReasonId { get; set; }
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public long? ModifiedBy { get; set; }
        public decimal? CustomerSatisfaction { get; set; }
        public long? DailyReportId { get; set; }
        public long? DailyReportLineId { get; set; }
        public long? RelatedToInventoryItemId { get; set; }
        public long? RelatedToSalesOfferProductId { get; set; }
        public long? RelatedToSalesOfferId { get; set; }
        public string Hint { get; set; }
        public string ReminderDate { get; set; }
        public bool ReminderIsClosed { get; set; }
    }
}
