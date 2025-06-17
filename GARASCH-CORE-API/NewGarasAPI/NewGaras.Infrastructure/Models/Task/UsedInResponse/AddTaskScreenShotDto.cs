using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task.UsedInResponse
{
    public class AddTaskScreenShotDto
    {
        public long TaskID { get; set; }
        public long UserID { get; set; }
        public string CreationDateTime { get; set; }
        public IFormFile Img { get; set; }
    }
}
