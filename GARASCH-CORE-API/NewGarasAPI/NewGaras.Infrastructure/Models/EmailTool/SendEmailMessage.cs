using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool
{
    public class SendEmailMessage
    {
        //public string UserID { get; set; }
        [FromForm]
        public List<string> To { get; set; }
        [FromForm]
        public List<string> Cc { get; set; }
        [FromForm]
        public string Subject { get; set; }
        [FromForm]
        public string Body { get; set; }
        [FromForm]
        public List<AddAttachment> AttachmentsList { get; set; }
    }
}
