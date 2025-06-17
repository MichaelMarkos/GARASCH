using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool.UsInResponses
{
    public class EmailMessage
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string BodyPreview { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public DateTime ReceivedDateTime { get; set; }
        public DateTime SentDateTime { get; set; }
        public bool HasAttachments { get; set; }
        public string InternetMessageId { get; set; }
        public string Importance { get; set; }
        public string ParentFolderId { get; set; }
        public string ConversationId { get; set; }
        public string WebLink { get; set; }
        public InferenceClassification InferenceClassification { get; set; }
        public EmailBody Body { get; set; }
        public EmailAddressContainer Sender { get; set; }
        public EmailAddressContainer From { get; set; }
        public List<EmailAddressContainer> ToRecipients { get; set; }
        public List<EmailAddressContainer> CcRecipients { get; set; }
        public List<EmailAddressContainer> BccRecipients { get; set; }
        public Flag Flag { get; set; }
    }
}
