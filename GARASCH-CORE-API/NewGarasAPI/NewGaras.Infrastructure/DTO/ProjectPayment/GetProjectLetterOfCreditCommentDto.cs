using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ProjectPayment
{
    public class GetProjectLetterOfCreditCommentDto
    {
        public long ID { get; set; }
        public long ProjectLetterOfCreditID { get; set; }
        public string Comment { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedByImgPath { get; set; }
    }
}
