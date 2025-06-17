using NewGaras.Infrastructure.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskMangerProject
{
    public class GetUsersToProjectDto
    {
        public long projectID { get; set; }
        public List<UserWithJobTitleDDL> AdminUsersList { get; set; }
        public List<UserWithJobTitleDDL> ManagerUsersList { get; set; }
        public List<UserWithJobTitleDDL> NormalUsersList { get; set; }
    }
}
