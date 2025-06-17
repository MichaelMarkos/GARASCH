using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task
{
    public class GetTaskBrowserTabsDto
    {
        public long TaskID { get; set; }
        public string taskName { get; set; }
        public long UserID { get; set; }
        public string UserName { get; set; }
        public string CreationDateTime { get; set; }
        public string TabName { get; set; }
    }
}
