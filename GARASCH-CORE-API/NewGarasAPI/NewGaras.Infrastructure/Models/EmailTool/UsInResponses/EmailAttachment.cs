using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool.UsInResponses
{
    public class EmailAttachment
    {
        public string OdataType { get; set; }
        public string OdataMediaContentType { get; set; }
        public string Id { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public int Size { get; set; }
        public bool IsInline { get; set; }
        public string ContentId { get; set; }
        public string ContentLocation { get; set; }
        public string ContentBytes { get; set; } // This is Base64 encoded
    }
}
