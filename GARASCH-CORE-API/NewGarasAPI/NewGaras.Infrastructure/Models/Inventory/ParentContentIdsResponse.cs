using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class ParentContentIdsResponse
    {
        public List<ParentContentInfo> ParentContentInfos { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class ParentContentInfo
    {
        public long ParentContentId { get; set; }
        public string ChapterName { get; set; }
    }
}
