using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    public class AddMatrialDirectPRResponse
    {
        public bool Result;
        public long PRID;
        // Using for Create PO 
        public long MRItemID;
        public long PRItemID;
        public List<Error> Errors;
    }
}
