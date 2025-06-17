using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace NewGaras.Domain.Services.ContractExtensionJob
{
    public class ContractExtension : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IContractService _contractService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ITenantService _tenantService;
        private GarasTestContext _Context;
        public ContractExtension(IServiceScopeFactory serviceScopeFactory,ITenantService tenantService)

        {
            _serviceScopeFactory = serviceScopeFactory;
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            DateTime nextMidnight = new DateTime(now.Year, now.Month, now.Day, 13, 27, 0).AddDays(0);
            TimeSpan timeUntilMidnight = nextMidnight - now;

            _timer = new Timer(ExtendContracts, null, timeUntilMidnight, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private void ExtendContracts(object state)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var test = _tenantService.SetTenant("periti");
                if(test == null)
                {
                    var text = "no";
                }
                 var jobLogic = scope.ServiceProvider.GetRequiredService<IContractService>();
                jobLogic.ExtendContracts(_tenantService);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
