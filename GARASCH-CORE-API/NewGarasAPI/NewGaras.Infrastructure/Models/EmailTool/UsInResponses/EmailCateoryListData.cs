using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool.UsInResponses
{
    public class EmailCateoryListData
    {
        public long Id { get; set; }
        public long EmailId { get; set; }
        public long CategoryTypeID { get; set; }
        public string CategoryTypeName { get; set; }
        public long TypeID { get; set; }
        public string TypeName { get; set; }
        public long CreatedBy { get; set; }
        public string CreatorName { get; set; }
        public string CreationDate { get; set;}
        public long? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModificationDate { get; set; }

    }
}
