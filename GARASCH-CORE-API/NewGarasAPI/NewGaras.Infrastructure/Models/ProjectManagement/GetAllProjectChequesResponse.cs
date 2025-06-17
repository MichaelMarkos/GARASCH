using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectManagement
{
    public class GetAllProjectChequesResponse
    {
        public List<ChequeStatusSummary> NotRecieved { get; set; }
        public List<ChequeStatusSummary> Recieved { get; set; }
        public List<ChequeStatusSummary> UnderCollection { get; set; }
        public List<ChequeStatusSummary> Refused { get; set; }
        public List<ChequeStatusSummary> Collected { get; set; }
        public List<GetProjectChequeDto> Cheques { get; set; }
        
        public string ReportPath { get; set; }
    }
}
