using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class GetPersonStatusListDTO
    {
        public int ID { get; set; }
        public string statusName { get; set; }
        public string Description { get; set; }
    }
}
