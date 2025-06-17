using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectManagement
{
    public class ProjectProgressDto
    {
        public long? Id { get; set; }
        public long ProjectId { get; set; }
        public int ProgressTypeId { get; set; }
        public int DeliveryTypeId { get; set; }
        public int ProgressStatusId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public string Comment { get; set; }
        public IFormFile Attachment { get; set; } = null;

        public bool Active { get; set; } = true;

        public bool DeleteAttachment { get; set; } = false;

        public List<ProgressUsers> ProgressUsers { get; set; } = [];
    }
}
