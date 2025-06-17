

using NewGaras.Domain.Interfaces.Repositories;
using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Interfaces.Hotel
{
    public interface IAddressRepository : IBaseRepository<ClientAddress, long>
    {
    }
}
