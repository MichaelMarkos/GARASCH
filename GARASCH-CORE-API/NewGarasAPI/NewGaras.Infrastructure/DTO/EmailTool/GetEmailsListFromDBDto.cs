using NewGaras.Infrastructure.Models.EmailTool.UsInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.EmailTool
{
    public class GetEmailsListFromDBDto
    {
        public long ID { get; set; }
        public string EmailId { get; set; }
        public string EmailBody { get; set; }
        public string EmailSubject { get; set; }
        public string EmailSender { get; set; }
        public string EmailSenderName { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public bool HasAttatchment { get; set; }
        public string RecivedDate { get; set; }
        public int EmailTypeID { get; set; }
        public string EmailTypeName { get; set; }
        public string EmailComment { get; set; }
        public List<string> AttachmentList { get; set; }
        public List<string> EmailCcList { get; set; }
        public List<EmailCateoryListData> EmailCateoryList { get; set; }
    }
}
