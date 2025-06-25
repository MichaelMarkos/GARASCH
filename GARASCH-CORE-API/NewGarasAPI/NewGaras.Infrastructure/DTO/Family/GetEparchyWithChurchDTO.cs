using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family
{
    public class GetEparchyWithChurchDTO
    {
        public int ID { get; set; }
        public string EparchyName { get; set; }
        public int NumberOfChurchs { get; set; }
    }
}
