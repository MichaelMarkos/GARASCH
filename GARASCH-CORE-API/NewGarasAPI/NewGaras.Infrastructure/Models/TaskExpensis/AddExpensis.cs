using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.TaskExpensis
{
    public class AddExpensis
    {
        public long ID { get; set; }
        public decimal TotalExpensis { get; set; }
        public decimal? Budget { get; set; }
    }
}
