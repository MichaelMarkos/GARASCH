using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Support
{
    public class AddSupportRequestDto
    {
        public bool ForAppSupport { get; set; }             //true -> app support , false -> IT Support
        public bool ForSoftwareSupport { get; set; }        //only available when the ForAppSupport is False (IT support)
        public string Module { get; set; }
        public string Priority { get; set; }
        public string Name { get; set; }
        public string Descriptions { get; set; }
    }
}
