using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IGraphAuthService
    {
       public Task<string> GetAccessTokenAsync();
    }
}
