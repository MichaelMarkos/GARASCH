using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.UsedInResponses
{
    public class ReportStatisctsPerDate
    {
        public string CreationDate { get; set; }
        public int DatePerType { get; set; }
        public int Count { get; set; }
        public List<ReportStatisctsPerDate> ReportLinesPerDateList { get; set; }
    }
}
