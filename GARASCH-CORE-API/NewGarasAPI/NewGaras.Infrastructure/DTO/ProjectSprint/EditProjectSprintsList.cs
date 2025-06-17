using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ProjectSprint
{
    public class EditProjectSprintsList
    {
        public long? ID{ get; set; }
        public string name { get; set; }
        public int orderNo { get; set; }
        public string stratDate { get; set; }
        public string EndDate { get; set; }
        public bool Active { get; set; }
    }
}

