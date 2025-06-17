using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.UsedInResponses
{
    public class WorkshopStationsItemResponseData
    {
        public long ID { get; set; }
        public string StationName { get; set; }
        public string Location { get; set; }
        public int? BranchId { get; set; }
        public string BranchName { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public long? TeamId { get; set; }
        public string TeamName { get; set; }
        public long[] TeamUsersIds { get; set; }
        public int StationWorkOrdersCount { get; set; }


    }
}
