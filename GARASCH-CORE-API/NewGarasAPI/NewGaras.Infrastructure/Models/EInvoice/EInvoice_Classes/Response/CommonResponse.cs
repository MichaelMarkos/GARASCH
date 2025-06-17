using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Response
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class FreezeStatus
    {
        public bool frozen { get; set; }
        public string type { get; set; }
        public string actionDate { get; set; }
        public string auCode { get; set; }
        public string auName { get; set; }
    }

    public class Metadata
    {
        public int totalPages { get; set; }
        public int totalCount { get; set; }
    }
}
