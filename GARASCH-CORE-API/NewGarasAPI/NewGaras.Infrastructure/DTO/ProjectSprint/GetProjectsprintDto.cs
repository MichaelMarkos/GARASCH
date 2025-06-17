using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ProjectSprint
{
    public class GetProjectsprintDto
    {
        public long Id { get; set; }
        public string name { get; set; }
        public long projectID { get; set; }
        public int orderNo { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
