using NewGaras.Infrastructure.DTO.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Department
{
    public class GetDepartmentDto
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public int BranchId { get; set; }

        public bool? Active { get; set; }

        public List<GetTeamDto> Teams { get; set; }
    }
}
