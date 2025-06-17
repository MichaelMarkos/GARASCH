using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.BL
{
    public class CustomerBL
    {

        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public CustomerBL(ITenantService tenantService)
        {
            _Context = new GarasTestContext(tenantService);
        }

        public List<CustomerModel> oracleGetCustomer(long cusID)
        {

            try
            {
                List<CustomerModel> result = new List<CustomerModel>();
                var CustomerId = new SqlParameter("cusID", System.Data.SqlDbType.BigInt);
                CustomerId.Value = cusID;
                object[] param = new object[] { CustomerId };

                var CustomerDB = _Context.Database.SqlQueryRaw<STP_EINVOICE_GETCustomer_Result>("Exec STP_EINVOICE_GETCustomer @cusID", param).AsEnumerable().FirstOrDefault();
                if (CustomerDB != null)
                {
                    result.Add(new CustomerModel(
                    CustomerDB.ACCOUNT_NUMBER.ToString(),
                                                    CustomerDB.party_name.ToString(),
                                                    CustomerDB.COUNTRY.ToString(),
                                                    CustomerDB.street.ToString(),
                                                    CustomerDB.regionCity.ToString(),
                                                    CustomerDB.governate.ToString(),
                                                    CustomerDB.buildingNumber.ToString(),
                                                    CustomerDB.postalCode.ToString(),
                                                    CustomerDB.floor.ToString(),
                                                    CustomerDB.room.ToString(),
                                                    CustomerDB.landmark.ToString(),
                                                    CustomerDB.additionalInformation.ToString(),
                                                    CustomerDB.Cus_type.ToString(),
                                                    CustomerDB.TaxpayerCode
                                                 ));
                }

                return result;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
