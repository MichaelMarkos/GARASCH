using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets
{
    public class AddSalesTarget
    {
        public int? Id { get; set; }
        public int Year { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public decimal Target {  get; set; }
        public int CurrencyId { get; set; }
        public bool CanEdit { get; set; }
        public bool? Active { get; set; }
    }
}
