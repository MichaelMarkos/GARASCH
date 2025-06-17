using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskUnitRateService
{
    public class EditTaskUnitRateServiceDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public decimal Rate { get; set; }
        public decimal Quantity { get; set; }
        public long TaskID { get; set; }
        public int UOMID { get; set; }
    }
}
