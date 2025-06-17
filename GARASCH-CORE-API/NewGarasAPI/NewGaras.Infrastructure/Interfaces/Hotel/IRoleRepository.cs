

using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Interfaces.Hotel
{
    public interface IRoleRepository : IBaseRepository<Role , long>
    {
        bool HasRole(string role , string id);
    }
}
