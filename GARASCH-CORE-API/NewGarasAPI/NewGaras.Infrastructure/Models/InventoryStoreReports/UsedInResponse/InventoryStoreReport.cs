using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryStoreReports.UsedInResponse
{
    public class InventoryStoreReport
    {
        public long ID { get; set; }
        public string ReportSubject { get; set; }
        public string ReportDesc { get; set; }
        public long StoreID { get; set; }
        public string StoreName { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string Status { get; set; }
        public string Approval { get; set; }
        public string OperationType { get; set; }
        public bool IsFinished { get; set; }
        public List<Attachment> AttachemtsList { get; set; }
    }
}
