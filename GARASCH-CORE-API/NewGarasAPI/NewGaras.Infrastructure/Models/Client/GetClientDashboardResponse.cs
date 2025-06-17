using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class GetClientDashboardResponse
    {
        public ClientDashboard ClientDashboard { get; set; }

        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}
