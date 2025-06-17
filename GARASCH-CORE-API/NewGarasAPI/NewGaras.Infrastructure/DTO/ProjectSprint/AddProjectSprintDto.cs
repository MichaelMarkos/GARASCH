using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ProjectSprint
{
    public class AddProjectSprintDto
    {
        public long projectID { get; set; }

        public List<AddProjectSprintsList> sprintsList { get; set; }

        
    }
}
