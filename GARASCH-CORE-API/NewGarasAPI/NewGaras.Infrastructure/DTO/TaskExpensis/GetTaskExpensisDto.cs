using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskExpensis
{
    public class GetTaskExpensisDto
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public string Curruncy { get; set; }

        public string Imgpath { get; set; }

        public long TaskId { get; set; }
        public string TaskName { get; set; }
        //public string Date { get; set; }
        public long? ExpensisTypeId { get; set; }
        public string ExpensisTypeName { get; set; }

        public long CreatedBy { get; set; }
        public string UserName { get; set; }
        public string UserImgPath { get; set; }
        public string CreationDate { get; set; }
        public string ProjectName { get; set; }
        public bool? Approved { get; set; }
        public long? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public string ApprovedByImg { get; set; }

        public bool Billable { get; set; }

        public bool IsInvoiced { get; set; } = false;
        public long InvoiceId { get; set; } = 0;
        public int? JobTitleID { get; set; }
        public string  JobTitleName { get; set; }
    }
}
