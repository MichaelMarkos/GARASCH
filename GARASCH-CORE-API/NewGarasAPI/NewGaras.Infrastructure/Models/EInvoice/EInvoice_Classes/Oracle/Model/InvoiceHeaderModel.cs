using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model
{
    [Serializable]
    public class InvoiceHeaderModel
    {

        DateTime? trx_date;
        string trx_number;
        string trxType;
        long? SalesOfferId;

        public InvoiceHeaderModel(DateTime? trx_date, string trx_number, string trxType, long? salesOfferId)
        {
            Trx_date = trx_date;
            Trx_number = trx_number;
            TrxType = trxType;
            SalesOfferId1 = salesOfferId;
        }

        public DateTime? Trx_date { get => trx_date; set => trx_date = value; }
        public string Trx_number { get => trx_number; set => trx_number = value; }
        public string TrxType { get => trxType; set => trxType = value; }
        public long? SalesOfferId1 { get => SalesOfferId; set => SalesOfferId = value; }
    }
}
