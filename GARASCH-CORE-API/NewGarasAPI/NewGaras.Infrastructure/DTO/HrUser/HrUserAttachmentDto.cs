using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class HrUserAttachmentDto
    {
        public long? ID { get; set; }
        public long HrUserID { get; set; }
        public long AttachmentTypeID { get; set; }
        public bool Active { get; set; }

        public string AttachmentNumber { get; set; }
        public IFormFile AttachmentFile { get; set; }
    }
}
