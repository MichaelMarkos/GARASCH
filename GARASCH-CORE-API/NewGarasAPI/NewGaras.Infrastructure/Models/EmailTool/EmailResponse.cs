using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool
{
    public class EmailResponse
    {
        public string OdataContext { get; set; }
        public List<NewGaras.Infrastructure.Models.EmailTool.UsInResponses.EmailMessage> Value { get; set; }
    }
}
