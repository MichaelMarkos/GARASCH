using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class AddAttachment
    {

        public int Id { get; set; }

        public IFormFile? Content { get; set; }


    }
}
