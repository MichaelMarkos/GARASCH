using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryItemMatrialReleasePagingResponse
    {
        public List<InventoryMatrialReleaseInfoForPaging> MatrialReleaseList { get; set; }
        public PaginationHeader PaginationHeader { get; set; } = new PaginationHeader();
        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
