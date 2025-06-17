using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.TaskMangerProject
{
    public class TaskMangerAttachment
    {
        public int? ID { get; set; }
        public bool Active { get; set; }
        public IFormFile FileContent { get; set; }
    }
}
