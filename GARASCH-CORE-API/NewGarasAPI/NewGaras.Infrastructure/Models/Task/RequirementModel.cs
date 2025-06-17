using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task
{
    public class RequirementModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Percentage { get; set; }
        public bool IsFinish { get; set; }
        public string CreationDate { get; set; }

        public bool Active { get; set; } = true;
    }
}
