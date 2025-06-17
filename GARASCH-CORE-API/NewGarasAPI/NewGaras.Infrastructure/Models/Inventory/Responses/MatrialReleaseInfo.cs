using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class MatrialReleaseInfo
    {
        public long Id { get; set; }
        public string  CreationDate { get; set; }
        public List<MatrialReleaseItem> MatrialReleaseItemList { get; set; }
    }
}
