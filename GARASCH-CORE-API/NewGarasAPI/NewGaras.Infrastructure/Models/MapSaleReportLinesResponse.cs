using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class MapSaleReportLinesResponse
    {
        public List<MapSalesReportLine> MapSalesReportLines { get; set; }

        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
