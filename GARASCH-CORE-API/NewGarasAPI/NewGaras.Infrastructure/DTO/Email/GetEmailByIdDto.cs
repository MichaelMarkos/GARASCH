using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Email
{
    public class GetEmailByIdDto
    {
        public long ID { get; set; }
        public string EmailId { get; set; }
        public string EmailBody { get; set; }
        public string EmailSubject { get; set; }
        public string EmailSender { get; set; }
        public string UserId { get; set; }
        public bool HasAttatchment { get; set; }
        public List<string> AttachmentList { get; set; }
        public List<string> EmailCcList { get; set; }
    }
}
