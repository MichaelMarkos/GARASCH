using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ChurchAndPriest
{
    public class GetChurchesListDTO
    {
        public long ID { get; set; }
        public string ChurchName { get; set; }
        public int? EparchyID { get; set; }
        public string EparchyName { get; set; }
    }
}
