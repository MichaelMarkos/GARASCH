using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskExpensis
{
    public class EditTaskExpensisDto
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public string Curruncy { get; set; }

        public IFormFile? Attachment { get; set; }
        public bool ActiveAttach { get; set; }
        public long TaskId { get; set; }

        public long ExpensisTypeId { get; set; }

        public bool Billable { get; set; }
    }
}
