using NewGaras.Infrastructure.Models.TaskMangerProject.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskMangerProject
{
    public class EditUsersAssignToProjectDto
    {
        public long ProjectID { get; set; }
        public List<EditUsersAssignToProject> AdminUsersList { get; set; }
        public List<EditUsersAssignToProject> ManagerUsersList { get; set; }
        public List<EditUsersAssignToProject> NormalUsersList { get; set; }
    }
}
