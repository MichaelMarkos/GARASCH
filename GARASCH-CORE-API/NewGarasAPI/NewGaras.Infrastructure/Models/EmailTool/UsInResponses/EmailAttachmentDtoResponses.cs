using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool.UsInResponses
{
    public class EmailAttachmentDtoResponses
    {
        public string AttachmentID { get; set; }
        public string MediaType { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string FilePath { get; set; }
        
    }
}
