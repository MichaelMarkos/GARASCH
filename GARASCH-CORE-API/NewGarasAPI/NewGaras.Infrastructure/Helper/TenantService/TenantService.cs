using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Helper.TenantService
{
    public class TenantService : ITenantService
    {
        private readonly TenantSettings _tenantSettings;
        private HttpContext _httpContext;
        private Tenant _currentTenant;
        public TenantService(IOptions<TenantSettings> tenantSettings, IHttpContextAccessor contextAccessor)
        {
            _tenantSettings = tenantSettings.Value;
            _httpContext = contextAccessor.HttpContext;
            if (_httpContext != null)
            {

                if (_httpContext.Request.Headers.TryGetValue("CompanyName", out var tenantId))
                {
                    SetTenant(tenantId!);
                }
                else
                {
                    SetTenant("GARASTest");
                    //throw new Exception("Invalid Company!");
                }
            }
        }

        public bool SetTenant(string tenantId)
        {
            _currentTenant = _tenantSettings.Tenants.Where(a => a.TID.ToLower() == tenantId.ToLower()).FirstOrDefault();
            if (_currentTenant == null) return false; //throw new Exception("Invalid Tenant!");
            if (string.IsNullOrEmpty(_currentTenant.ConnectionString))
            {
                SetDefaultConnectionStringToCurrentTenant();
            }
            return true;
        }
        //public ApplicationDbContext CreateDbContext(ITenantService _tenantService)
        //{
        //    // Apply your multi-tenancy strategy here to determine the appropriate database connection details for the given tenant
        //    string connectionString = GetConnectionString();

        //    // Create and configure the database options
        //    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        //    optionsBuilder.UseSqlServer(connectionString);

        //    // Create the database context using the options
        //    return new ApplicationDbContext(optionsBuilder.Options, _tenantService);
        //}
        private void SetDefaultConnectionStringToCurrentTenant()
        {
            _currentTenant.ConnectionString = _tenantSettings.Defaults.ConnectionString;
        }

        public string GetConnectionString()
        {
            if (_currentTenant.TID != null)
            {
                return _tenantSettings.Tenants.Where(a => a.TID.ToLower() == _currentTenant.TID.ToLower()).FirstOrDefault()?.ConnectionString;
            }
            return _currentTenant?.ConnectionString;
        }

        public string GetDatabaseProvider()
        {
            return _tenantSettings.Defaults?.DBProvider;
        }

        public Tenant GetTenant()
        {
            return _currentTenant;
        }



    }
}
