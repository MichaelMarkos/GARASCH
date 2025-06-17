using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical
{
    public class GetPatientModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string NationalId { get; set; }
        public string Mobile { get; set; }
        public long SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
    }
}
