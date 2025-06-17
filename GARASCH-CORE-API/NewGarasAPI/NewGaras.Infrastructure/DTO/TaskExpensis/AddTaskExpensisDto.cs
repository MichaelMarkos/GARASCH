using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskExpensis
{
    public class AddTaskExpensisDto
    {
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public string Curruncy { get; set; }

        public IFormFile? Attachment { get; set; }

        public long TaskId { get; set; }

        public long ExpensisTypeId { get; set; }

        public bool Billable {  get; set; } 
    }
}
