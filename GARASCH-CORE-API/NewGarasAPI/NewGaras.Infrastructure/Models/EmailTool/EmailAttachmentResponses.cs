using NewGaras.Infrastructure.Models.EmailTool.UsInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool
{
    public class EmailAttachmentResponse
    {
        public string OdataContext { get; set; }
        public List<EmailAttachment> value { get; set; }
    }
}
