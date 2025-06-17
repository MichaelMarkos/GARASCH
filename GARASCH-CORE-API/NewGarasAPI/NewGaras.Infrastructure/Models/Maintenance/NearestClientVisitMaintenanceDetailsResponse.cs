using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class NearestClientVisitMaintenanceDetailsResponse
    {
        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
        public int Count { get; set; }
        public List<NearestClientVisitMaintenanceDetails> NearestClientVisitMaintenanceDetailsList { get; set; }
    }
}
