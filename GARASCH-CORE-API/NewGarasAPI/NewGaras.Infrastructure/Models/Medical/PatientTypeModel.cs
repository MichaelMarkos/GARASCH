using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical
{
    public class PatientTypeModel
    {
        public long? Id { get; set; }

        public string Type { get; set; }

        public decimal Percentage { get; set; }

        public bool Active { get; set; }
    }
}
