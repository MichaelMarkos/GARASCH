using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class MyClientsCRMDashboardResponse
    {
        public int OldClientsCount { get; set; }
        public int OldClientsRFQCount { get; set; }
        public List<ClientsStatistics> OldClientsSupportedByList { get; set; }
        public int OldExpiredClients {  get; set; }
        public int OldWillExpiredClients { get; set; }

        public int NewClientsCount { get; set; }
        public int NewClientsRFQCount { get; set; }
        public int NewClientsCountLastYear { get; set; }
        public List<ClientsStatistics> NewClientsSupportedByList { get; set; }
        public string NewClientsState { get; set; } //(Up, Down)
        public int NewExpiredClients {  get; set; }
        public int NewWillExpiredClients { get; set; }

        public int TotalClientsCount { get; set; }
        public int TotalClientsRFQCount { get; set; }

        public AchievedTarget AchievedTarget { get; set; }
        public EngagingRate EngagingRate { get; set; }
        public AcquisitionRate AcquisitionRate { get; set; }

        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
