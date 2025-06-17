using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InternalTicket
{
    public class InternalticketheaderPdf
    {
        
        [FromHeader]
        public string FromDate { set; get; } = new DateTime(DateTime.Now.Year , 1 , 1).ToShortDateString();
        [FromHeader]
        public string ToDate { set; get; } = new DateTime(DateTime.Now.Year+1 , 1 , 1).ToShortDateString();
        [FromHeader]
        public string CompanyName { set; get; }

        [FromHeader]
        public string UserID { set; get; }
        [FromHeader]
        public long createdBy { set; get; }
        [FromHeader]
        public string OfferType { set; get; }
        [FromHeader]
        public string OfferTypeReturn { set; get; }
    }
}
