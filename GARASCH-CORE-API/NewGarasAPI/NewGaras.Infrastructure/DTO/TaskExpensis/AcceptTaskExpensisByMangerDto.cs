using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskExpensis
{
    public class AcceptTaskExpensisByMangerDto
    {
        public long ExpsensisID { get; set; }
        public bool Approved { get; set; }
    }
}
