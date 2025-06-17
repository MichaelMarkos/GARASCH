using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectManagement
{
    public class GetProjectProgressDto
    {
        public long Id { get; set; }
        public string ProjectName { get; set; }
        public int ProgressTypeID { get; set; }
        public string ProgressTypeName { get; set; }
        public int DeliveryTypeID { get; set; }
        public string DeliveryTypeName { get; set; }
        public int ProgressStatusID { get; set; }
        public string ProgressStatusName { get; set; }
        public string Date { get;set; }
        public string AttachmentPath { get;set; }
        public decimal RelatedCollectedPercent { get; set; }
        public string Comment { get; set; }
        public List<GetProgressUsers> ProgressUsers { get; set; } = [];
    }
}
