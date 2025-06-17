using EInvoice.App_code.Oracle.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.BL
{
    public class IssuerBL
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public IssuerBL(ITenantService tenantService)
        {
            _Context = new GarasTestContext(tenantService);
        }

        public IssuerModel GetProfileInfo(long ClientAddressID)
        {
            var cltAddressID = new SqlParameter("cltAddressID", System.Data.SqlDbType.BigInt);
            cltAddressID.Value = ClientAddressID;
            object[] param = new object[] { cltAddressID };

            var IssuerDB = _Context.Database.SqlQueryRaw<STP_EINVOICE_GETIssuerProfileData_Result>("Exec STP_EINVOICE_GETIssuerProfileData @cltAddressID", param).AsEnumerable().FirstOrDefault();

            IssuerModel prof = new IssuerModel(companyName: IssuerDB.companyName,
                                                companyNameAR: IssuerDB.companyNameAR,
                                                regNum: IssuerDB.regNum,
                                                type: IssuerDB.type,
                                                branchID: IssuerDB.branchID.ToString(),
                                                country: IssuerDB.country,
                                                governate: IssuerDB.governate,
                                                regionCity: IssuerDB.regionCity,
                                                buildNum: IssuerDB.buildNum,
                                                postalCode: IssuerDB.postalCode.ToString(),
                                                floor: IssuerDB.floor,
                                                room: IssuerDB.room.ToString(),
                                                landmark: IssuerDB.landmark,
                                                addInfo: IssuerDB.addInfo,
                                                street: IssuerDB.street,
                                                activityCode1: IssuerDB.activityCode1.ToString(),
                                                activityCode2: "",
                                                activityCode3: "",
                                                activiyDesc1: IssuerDB.activiyDesc1,
                                                activiyDesc2: "",
                                                activiyDesc3: ""
);


            return prof;
        }
    }
}
