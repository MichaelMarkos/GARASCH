using NewGaras.Infrastructure.Models.EmailTool.UsInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool
{
    public class SingleEmailMessage
    {
        public string OdataContext { get; set; }
        public string OdataEtag { get; set; }
        public string Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public string ChangeKey { get; set; }
        public List<string> Categories { get; set; }
        public DateTime ReceivedDateTime { get; set; }
        public DateTime SentDateTime { get; set; }
        public bool HasAttachments { get; set; }
        public string InternetMessageId { get; set; }
        public string Subject { get; set; }
        public string BodyPreview { get; set; }
        public string Importance { get; set; }
        public string ParentFolderId { get; set; }
        public string ConversationId { get; set; }
        public string ConversationIndex { get; set; }
        public bool? IsDeliveryReceiptRequested { get; set; }
        public bool? IsReadReceiptRequested { get; set; }
        public bool IsRead { get; set; }
        public bool IsDraft { get; set; }
        public string WebLink { get; set; }
        public string InferenceClassification { get; set; }
        public EmailBody Body { get; set; }
        public EmailAddressContainer Sender { get; set; }
        public EmailAddressContainer From { get; set; }
        public List<EmailAddressContainer> ToRecipients { get; set; }
        public List<EmailAddressContainer> CcRecipients { get; set; }
        public List<EmailAddressContainer> BccRecipients { get; set; }
        public List<EmailAddressContainer> ReplyTo { get; set; }
        public EmailFlag Flag { get; set; }
    }
}
