using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.UsedInResponses
{
    public class TeamUsersSelectResponseData
    {
        public long ID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public int? JobTitleID { get; set; }

        public string DepartmentName { get; set; }


    }
}
