using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class ClientsCrmReportsDetailsResponse
    {
        public long FilteredClientId {  get; set; }
        public string FilteredClientName { get; set; }
        public int FilteredMonth {  get; set; }
        public int FilteredYear { get; set; }
        public long FilteredSalesPersonId { get; set; }
        public int FilteredBranchId { get; set; }
        public long FilteredReportCreator {  get; set; }
        public PaginationHeader PaginationHeader { get; set; }

        public List<CrmSalesClientReport> CrmReports {  get; set; }

        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
