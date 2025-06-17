using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class UpdateInventoryItemContentDto
    {
        public long Id { get; set; } 
        public string ChapterName { get; set; }
        public string Description { get; set; }

    }
}
