using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model;
using NewGarasAPI.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.BL
{
    public class ItemBL
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public ItemBL(ITenantService tenantService)
        {
            _Context = new GarasTestContext(tenantService);
        }

        public List<ItemModel> oracleGetItem(long ItemID)
        {
            try
            {

                List<ItemModel> result = new List<ItemModel>();

                var item = new SqlParameter("itmID", System.Data.SqlDbType.BigInt);
                item.Value = ItemID;
                object[] param = new object[] { item };

                var ItemObjDB = _Context.Database.SqlQueryRaw<STP_EINVOICE_GETItem_Result>("Exec STP_EINVOICE_GETItem @itmID", param).AsEnumerable().FirstOrDefault();

                if (ItemObjDB != null)
                {
                    result.Add(new ItemModel(ItemObjDB.ItemTypeEINVOICE,
                                                ItemObjDB.internalCode,
                                                ItemObjDB.ItemCode,
                                                ItemObjDB.UOM_CODE,
                                                ItemObjDB.codeName,
                                                 ItemObjDB.codeDescription ?? ""));


                }
                return result;

            }
            catch (Exception)
            {
                return null;
            }
        }


        public string GetItemOfferPricingComment(long SaelesOfferProductID)
        {
            string Comment = "";

            if (SaelesOfferProductID != 0)
            {
                var OfferProductID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
                OfferProductID.Value = SaelesOfferProductID;
                object[] param = new object[] { OfferProductID };

                var SalesOfferProductDB = _Context.Database.SqlQueryRaw<proc_SalesOfferProductLoadByPrimaryKey_Result>("Exec proc_SalesOfferProductLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();

                if (SalesOfferProductDB != null)
                {
                    Comment = SalesOfferProductDB.ItemPricingComment;
                }
            }
            return Comment;
        }
    }
}
