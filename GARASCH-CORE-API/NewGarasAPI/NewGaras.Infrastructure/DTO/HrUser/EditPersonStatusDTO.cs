using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class EditPersonStatusDTO
    {
        public int Id { get; set; }
        public string statusName { get; set; }
        public string Description { get; set; }
    }
}
