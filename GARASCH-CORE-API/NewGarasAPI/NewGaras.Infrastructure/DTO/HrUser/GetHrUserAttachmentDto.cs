using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class GetHrUserAttachmentDto
    {
        public long? ID { get; set; }
        public long HrUserID { get; set; }
        public string HrUserName { get; set; }
        public long AttachmentTypeID { get; set; }
        public string AttachmentTypeName { get; set; }
        public string AttachmentNumber { get; set; }

        public string AttachmentPath { get; set; }
    }
}
