using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetSalesReportsListStatisticsResponse
    {
        public string ReviewAvg {  get; set; }
        public int CreatedReport {  get; set; }
        public int WorkingDays { get; set; }
        public int ForSalesPersonsCount { get; set; }

        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
