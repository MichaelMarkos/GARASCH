using NewGaras.Infrastructure.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class ClientAttachmentsData
    {
        [FromForm]
        public long ClientId { get; set; }
        [FromForm]
        public string CreatedBy { get; set; }

        public UploadAttachment TaxCardAttachment { get; set; }

        public UploadAttachment CommercialRecordAttachment { get; set; }

        public List<UploadAttachment> LicenseAttachements { get; set; }

        public List<UploadAttachment> BussinessCardsAttachments { get; set; }

        public List<UploadAttachment> BrochuresAttachments { get; set; }

        public List<UploadAttachment> OtherAttachments { get; set; }
    }
}
