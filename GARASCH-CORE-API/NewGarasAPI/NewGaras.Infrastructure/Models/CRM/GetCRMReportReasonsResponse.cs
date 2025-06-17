using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.CRM
{
    public class GetCRMReportReasonsResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public List<GetCRMReportReasonsResponseVM> CRMReportReasonsList { get; set; }
    }
}
