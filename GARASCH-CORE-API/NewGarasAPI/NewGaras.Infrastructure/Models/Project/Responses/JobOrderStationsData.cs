using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.Responses
{
    public class JobOrderStationsData
    {
        public int projectId { get; set; }

        public int[] StationsIdsList { get; set; }
        public int StationWorkOrdersCount { get; set; }




    }
}
