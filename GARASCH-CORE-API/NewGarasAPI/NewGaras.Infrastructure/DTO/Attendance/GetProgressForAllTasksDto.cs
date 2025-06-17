using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class GetProgressForAllTasksDto
    {
        public string projectName {  get; set; }

        public string TaskName { get; set; }

        public string UserName { get; set; }

        public long HrUserId { get; set; }

        public string Date {  get; set; }

        public string ProgressNote { get; set; }

        public decimal TotalHours { get; set; }

        public decimal? ProgressPercent { get; set; }

        public TimeOnly CheckIn { get; set; }

        public TimeOnly CheckOut { get; set; }

        public decimal Cost { get; set; }

        public decimal WorkingHourRate { get; set; }

        public long? Id { get; set; }

        public string JobTitle { get; set; }

        public bool IsInvoiced { get; set; } = true;
        public long InvoiceId { get; set; } = 0;
    }
}
