using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskUnitRateService
{
    public class GetTaskUnitRateServiceDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public decimal Rate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        public long TaskID { get; set; }
        public string CreationDate { get; set; }
        public int UOMID { get; set; }
        public string UOMName { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string CreatorImgPath { get; set; }
        public bool IsInvoiced { get; set; } = false;
        public long InvoiceId { get; set; } = 0;
        public int JobtitleID { get; set; }
        public string JobtitleName { get; set; }

    }
}
