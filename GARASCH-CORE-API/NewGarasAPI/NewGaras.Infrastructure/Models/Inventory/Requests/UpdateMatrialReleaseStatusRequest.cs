using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class UpdateMatrialReleaseStatusRequest
    {
        public long MatrialReleaseId { get; set; }
        public string Status { get; set; }
        public string StatusComment { get; set; }
    }
}
