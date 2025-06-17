using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Admin
{
    public class EmployeeGroupData
    {
        //public int ID { get; set; }
        public int GroupID { get; set; }

        public int UserID { get; set; }


        public List<EditUserGroupData> userGroupData {  get; set; }
    }
}
