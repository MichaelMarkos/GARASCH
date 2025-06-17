using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.CRM.UsedInResponse
{
    public class ReportSalesAndCRMStatisctsPerDate
    {
        public string CreationDate { get; set; }
        public int DatePerType { get; set; }
        public int CountOfSales { get; set; }
        public int CountOfCRM { get; set; }
    }
}
