using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.Responses
{
    public class ProjectContactPersonData
    {
        public int? ID { get; set; }
        public int ProjectID { get; set; }
        public int CountryID { get; set; }
        public int GovernorateID { get; set; }
        public int? AreaID { get; set; }

        public string Address { get; set; }
        public string ProjectContactPersonName { get; set; }
        public string ProjectContactPersonMobile { get; set; }
        public bool Active { get; set; }




    }
}
