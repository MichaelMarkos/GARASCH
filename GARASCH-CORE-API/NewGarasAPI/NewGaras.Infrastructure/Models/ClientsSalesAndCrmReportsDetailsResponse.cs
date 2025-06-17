using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class ClientsSalesAndCrmReportsDetailsResponse
    {
        public long FilteredClientId { get; set; }
        public string FilteredClientName { get; set; }
        public string FilteredStartDate { get; set; }
        public string FilteredEndDate { get; set; }
        public int FilteredBeforeDays { get; set; }
        public long FilteredSalesPersonId { get; set; }
        public long FilteredReportCreator {  get; set; }
        public int FilteredBranchId {  get; set; }

        public List<SalesAndCRMByDay> CrmAndSalesByDayReports { get; set; }

        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
