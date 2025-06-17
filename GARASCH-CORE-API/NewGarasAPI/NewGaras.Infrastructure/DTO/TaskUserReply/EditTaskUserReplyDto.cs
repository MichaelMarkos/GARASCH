using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskUserReply
{
    public class EditTaskUserReplyDto
    {
        public long ID { get; set; }
        public string CommentReply { get; set; }
        public IFormFile ReplyAttachment { get; set; }
    }
}
