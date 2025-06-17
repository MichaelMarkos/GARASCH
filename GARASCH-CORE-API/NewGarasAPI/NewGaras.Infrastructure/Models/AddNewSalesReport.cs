using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class AddNewSalesReport
    {
        public long? Id { get; set; }
        public string UserId { get; set; }
        public string ReportDate { get; set; }
        public string ModifiedBy { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string ReviewComment { get; set; }
        public long? ReviewedBy { get; set; }
        public bool IsReviewed { get; set; }
        public float? Review {  get; set; }
        public List<SalesReportExpense> SalesReportExpenses { get; set; }
        public List<SalesReportLine> ReportLinesList { get; set; }
    }
}
