using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool
{
    public class GetEmailInfo
    {
        public int NumberOfInbox { get; set; }
        public int NumberOfJunk { get; set; }
        public int NumberOfDraft { get; set; }
        public int NumberOfSent { get; set; }
        public int NumberOfArchive { get; set; }
    }
}
