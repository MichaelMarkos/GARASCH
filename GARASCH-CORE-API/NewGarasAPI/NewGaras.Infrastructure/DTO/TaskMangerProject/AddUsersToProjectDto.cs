using NewGaras.Infrastructure.Models.TaskMangerProject.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskMangerProject
{
    public class AddUsersToProjectDto
    {
        public long projectID { get; set; }
        public List<long> AdminUsersList { get; set; }
        public List<long> ManagerUsersList { get; set; }
        public List<long> NormalUsersList { get; set; }

    }
}
