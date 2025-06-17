using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.RoyalTent
{
    public class BaseMessageExcelResponse
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public List<Error> Errors { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal MeterPrice { get; set; }
    }
}
