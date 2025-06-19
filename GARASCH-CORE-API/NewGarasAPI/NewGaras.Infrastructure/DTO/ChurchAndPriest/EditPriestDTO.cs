using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ChurchAndPriest
{
    public class EditPriestDTO
    {
        public long ID { get; set; }
        public string PriestName { get; set; }
        public long? ChurchID { get; }
    }
}
