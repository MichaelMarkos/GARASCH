using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class ClientMaintenanceDetailsResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public int MaintenanceNo { get; set; }
        public int ValidContractNo { get; set; }
        public int ExpiredContractNo { get; set; }
        public string LastVisitDate { get; set; }
        public long LastVisitId { get; set; }
        public long? LastMaintenanceForID { get; set; }
        public decimal? ClientSatisfactionRate { get; set; }
    }
}
