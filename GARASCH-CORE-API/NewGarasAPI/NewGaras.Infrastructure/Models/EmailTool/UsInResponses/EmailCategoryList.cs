using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool.UsInResponses
{
    public class EmailCategoryList
    {
        [FromBody]
        public long EmailID { get; set; }

        [FromBody]
        public string Comment { get; set; }
        [FromBody]
        public List<AddCategoryList> CategoryList { get; set; }
    }
}
