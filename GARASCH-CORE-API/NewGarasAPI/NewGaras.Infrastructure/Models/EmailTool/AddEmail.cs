using NewGaras.Infrastructure.Models.TaskMangerProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool
{
    public class AddEmail
    {
        [FromForm]
        public string EmailID { get; set; }
        [FromForm]
        public string EmailBody { get; set; }
        [FromForm]
        public string EmailSubject { get; set; }
        [FromForm]
        public string EmailSender { get; set; }
        //[FromForm]
        //public long UserID { get; set; }
        [FromForm]
        public List<AddAttachment> AttachmentList { get; set;}
        [FromForm]
        public List<AddEmailCc> EmailCcList { get; set; }
    }
}
