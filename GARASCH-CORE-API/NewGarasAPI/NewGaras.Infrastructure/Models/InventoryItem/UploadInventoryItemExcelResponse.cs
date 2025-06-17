using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    public class UploadInventoryItemExcelResponse
    {
        public List<long> ListOfAddedIDs { get; set; }
        public string ErrorFilePath { get; set; }
    }
}
