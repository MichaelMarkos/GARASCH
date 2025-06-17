using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.BL
{
    public class InvoiceBL
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public InvoiceBL(ITenantService tenantService)
        {
            _Context = new GarasTestContext(tenantService);
        }

        public List<InvoiceHeaderModel> oracleGetInvoiceHeader(long InvoiceID)
        {

            try
            {
                List<InvoiceHeaderModel> result = new List<InvoiceHeaderModel>();
                var invID = new SqlParameter("invID", System.Data.SqlDbType.BigInt);
                invID.Value = InvoiceID;
                object[] param = new object[] { invID };

                var InvoiceHeaderDB = _Context.Database.SqlQueryRaw<STP_EINVOICE_GetInvoiceHeader_Result>("Exec STP_EINVOICE_GetInvoiceHeader @invID", param).AsEnumerable().FirstOrDefault();
                if (InvoiceHeaderDB != null)
                {
                    result.Add(new InvoiceHeaderModel(trx_date: InvoiceHeaderDB.InvoiceDate, trx_number: InvoiceHeaderDB.Serial, trxType: InvoiceHeaderDB.TrxType, salesOfferId: InvoiceHeaderDB.SalesOfferId));
                }
                return result;

            }
            catch (Exception)
            {
                return null;
            }
        }
        public List<InvoiceLineModel> oracleGetInvoiceLine(long SalesOfferId)
        {

            try
            {
                List<InvoiceLineModel> result = new List<InvoiceLineModel>();
                var OfferId = new SqlParameter("SalesOfferId", System.Data.SqlDbType.BigInt);
                OfferId.Value = SalesOfferId;
                object[] param = new object[] { OfferId };

                var InvoiceLineDB = _Context.Database.SqlQueryRaw<STP_EINVOICE_GetInvoiceLines_Result>("Exec STP_EINVOICE_GetInvoiceLines @SalesOfferId", param).AsEnumerable().ToList();

                foreach (var item in InvoiceLineDB)
                {
                    result.Add(new InvoiceLineModel(quantity: item.Quantity,
                        itemPrice: item.ItemPrice,
                        discountPercentage: item.DiscountPercentage ?? 0, salesTotal: item.salesTotal, discountValue: item.DiscountValue, netTotal: item.netTotal, itemTypeEINVOICE: item.ItemTypeEINVOICE, internalCode: item.internalCode, itemCode: item.ItemCode, uOM_CODE: item.UOM_CODE, salesOfferProductID: item.SalesOfferProductID, itemID: item.itemID));
                }
                return result;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<InvoiceLineTaxModel> oracleGetInvoiceLineTax(long SalesOfferId, long SalesOfferProductID)
        {
            List<InvoiceLineTaxModel> result = new List<InvoiceLineTaxModel>();
            try
            {
                var OfferId = new SqlParameter("SalesOfferId", System.Data.SqlDbType.BigInt);
                OfferId.Value = SalesOfferId;
                var OfferProductId = new SqlParameter("SalesOfferProductID", System.Data.SqlDbType.BigInt);
                OfferProductId.Value = SalesOfferProductID;

                object[] param = new object[] { OfferId, OfferProductId };

                var LineTaxDB = _Context.Database.SqlQueryRaw<STP_EINVOICE_GetInvoiceLineTax_Result>("Exec STP_EINVOICE_GetInvoiceLineTax @SalesOfferId, @SalesOfferProductID", param).AsEnumerable().ToList();

                foreach (var item in LineTaxDB)
                {
                    result.Add(new InvoiceLineTaxModel(item.TaxName, item.TaxPercentage, item.TaxType, item.TaxValue, item.SubTaxName, item.isPercentage));
                }

            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }

        public void saveInvoiceUUID(long InvoiceID, string uuid, string eInvoiceStatus, DateTime? eInvoiceAcceptDate)
        {

            try
            {
                var invID = new SqlParameter("invID", System.Data.SqlDbType.BigInt);
                invID.Value = InvoiceID;

                var eInvoiceId = new SqlParameter("eInvoiceId", System.Data.SqlDbType.NVarChar);
                eInvoiceId.Value = uuid;

                var Status = new SqlParameter("eInvoiceStatus", System.Data.SqlDbType.NVarChar);
                Status.Value = eInvoiceStatus;

                var AcceptDate = new SqlParameter("eInvoiceAcceptDate", System.Data.SqlDbType.DateTime);
                AcceptDate.Value = eInvoiceAcceptDate;

                object[] param = new object[] { invID, eInvoiceId, Status, AcceptDate };

                _Context.Database.SqlQueryRaw<STP_EINVOICE_GetInvoiceLineTax_Result>("Exec STP_EINVOICE_SaveInvoiceUUID @invID, @eInvoiceId, @eInvoiceStatus, @eInvoiceAcceptDate", param);
            }
            catch (Exception)
            {
                return;
            }
        }

        public string getUUID(long InvoiceID)
        {
            string uuid = "";
            try
            {
                var invID = new SqlParameter("invID", System.Data.SqlDbType.BigInt);
                invID.Value = InvoiceID;

                object[] param = new object[] { invID };

                uuid = _Context.Database.SqlQueryRaw<string>("Exec STP_EINVOICE_GetInvoiceUUID @invID", param).AsEnumerable().FirstOrDefault();

            }
            catch (Exception)
            {
                return uuid;
            }


            return uuid;
        }
    }
}
