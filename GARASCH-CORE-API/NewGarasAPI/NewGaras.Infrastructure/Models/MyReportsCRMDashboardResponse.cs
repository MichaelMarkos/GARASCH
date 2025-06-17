using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class MyReportsCRMDashboardResponse
    {
        public int TotalCRMReportsCount { get; set; }

        public int TotalCRMSentMeetingsCount { get; set; }

        public int TotalCRMSentPhoneCount { get; set; }
        public int TotalCRMSentWhatsappCount { get; set; }
        public int TotalCRMSentEmailCount { get; set; }
        public int TotalCRMSentOtherCount { get; set; }

        public int TotalCRMRecievedMeetingsCount { get; set; }

        public int TotalCRMRecievedPhoneCount { get; set; }
        public int TotalCRMRecievedWhatsappCount { get; set; }
        public int TotalCRMRecievedEmailCount { get; set; }
        public int TotalCRMRecievedOtherCount { get; set; }

        public string CrmCustomerSatisfactionPercentage { get; set; }

        public int TotalSalesReportsCount { get; set; }

        public int TotalSalesMeetingsCount { get; set; }

        public int TotalSalesPhoneCount { get; set; }
        public int TotalSalesWhatsappCount { get; set; }
        public int TotalSalesEmailCount { get; set; }
        public int TotalSalesOtherCount { get; set; }
        public string SalesReportsReviewAvg { get; set; }

        public string SalesCustomerSatisfactionPercentage { get; set; }

        public string TotalCustomerSatisfactionPercentage { get; set; }

        public List<ReportReason> CrmReportReasons { get; set; }
        public List<ReportReason> SalesReportReasons { get; set; }
        public List<ReportReason> TotalReportReasons { get; set; }

        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
