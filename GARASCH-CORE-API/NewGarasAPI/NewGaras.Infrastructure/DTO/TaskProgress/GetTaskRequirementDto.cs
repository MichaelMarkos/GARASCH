using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskProgress
{
    public class GetTaskRequirementDto
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public decimal Percentage { get; set; }
        public bool IsFinished { get; set; }
    }
}
