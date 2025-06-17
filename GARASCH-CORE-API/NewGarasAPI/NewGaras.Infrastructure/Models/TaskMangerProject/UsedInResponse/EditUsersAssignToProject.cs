using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.TaskMangerProject.UsedInResponse
{
    public class EditUsersAssignToProject
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public bool Active { get; set; }
    }
}
