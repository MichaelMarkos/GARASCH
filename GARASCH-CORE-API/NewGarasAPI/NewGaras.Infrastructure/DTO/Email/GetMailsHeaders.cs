using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Email
{
    public class GetMailsHeaders
    {
        [FromHeader]
        public long? ID { get; set; }
        [FromHeader]
        public string EmailId { get; set; }
        [FromHeader]
        public string EmailBody { get; set; }
        [FromHeader]
        public string EmailSubject { get; set; }
        [FromHeader]
        public string EmailSender { get; set; }
        [FromHeader]
        public long? UserId { get; set; }
        [FromHeader]
        public int numOfItemsPerPage { get; set; }
        [FromHeader]
        public int currentPage { get; set; }
    }
}
