using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.Responses
{
    public class WorkShopStationData
    {
        public long ID { get; set; }
        public string StationName { get; set; }
        public string Location { get; set; }

        public int BranchID { get; set; }

        public int TeamID { get; set; }



    }
}
