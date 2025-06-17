using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class AddInventoryItemContentDto
    {

        public long InventoryItemId { get; set; }

        public string ChapterName { get; set; }
        public long? ParentContentId { get; set; }

        //public int DataLevel { get; set; }
        public string Description { get; set; }

    }
}
