using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Team
{
    public class TeamDto
    {
        public long? Id { get; set; }

        public string Name { get; set; }

        public int DepartmentId { get; set; }

        public bool? Active { get; set; }

        public List<long> HrUsersIds { get; set; } = [];
    }
}
