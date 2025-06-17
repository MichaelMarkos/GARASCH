using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ProjectSprint
{
    public class EditProjectSprint
    {
        
        public long ProjectId { get; set; }

        public List<EditProjectSprintsList> sprintsList { get; set; }
    }
}
