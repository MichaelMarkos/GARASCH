using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ChurchAndPriest
{
    public class GetPriestsListDTO
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public long ChurchID { get; set; }
        public string ChurchName { get; set; }
        public int? EparchyID { get; set; }
        public string EparchyName { get; set; }
        public long CreatedBy { get; set; }
        public string creatorName { get; set; }
    }
}
