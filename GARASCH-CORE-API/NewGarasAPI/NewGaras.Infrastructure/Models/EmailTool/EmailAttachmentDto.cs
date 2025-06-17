using NewGaras.Infrastructure.Models.EmailTool.UsInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool
{
    public class EmailAttachmentDto
    {
        public string UserID { get; set; }
        public string MessageID { get; set; }
        public List<EmailAttachmentDtoResponses> AttachList { get; set; }
    }
}
