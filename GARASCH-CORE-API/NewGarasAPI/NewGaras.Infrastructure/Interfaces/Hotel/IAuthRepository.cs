


using NewGaras.Domain.Interfaces.Repositories;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.Hotel.DTOs.Auth;
using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Interfaces.Hotel
{
    public interface IAuthRepository : IBaseRepository<User , long>
    {
        Task<BaseResponseWithData<AuthModel>> GetTokenAsync(LoginRequestModel model);

        Task<BaseResponseWithData<AuthModel>> GetUserDataAsync(string ApplicationUserId);
        // Task<string> AddRoleAsync(AddRoleModel model);
        Task<BaseResponseWithData<Role>> AddRoleName(Role model);
        Task<BaseResponseWithData<AddRoleModel>> AddRoleforUser(AddRoleModel model);
        Task<BaseResponseWithData<AuthModel>> Register(UserDto model);

        Task<BaseResponseWithData<List<RoleViewModel>>> GetRoleList();

    }
}
