using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool
{
    public class AddListOfEmail
    {
        [FromForm]
        public List<AddEmail> Emails { get; set; }
    }
}
