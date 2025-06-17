using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface INotificationSubscriptionService
    {
        public Task<NewGaras.Domain.Models.BaseResponseWithId<long>> CreateSubscriptionAsync(string UserEmail);

       // public Task RenewSubscriptionAsync(string subscriptionId);
    }
}
