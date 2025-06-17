using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectManagement
{
    public class ProgressTypeDto
    {
        public int? Id { get; set; }
        public string Type { get; set; }
        public decimal ProgressPercentage { get; set; } = 0;

        public bool? Active { get; set; }
    }
}
