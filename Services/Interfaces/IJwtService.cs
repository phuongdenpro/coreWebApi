using coreWebApi.Models;

namespace coreWebApi.Services.Interfaces
{
    public interface IJwtService
    {
        string CreateAccessToken(User user, IConfiguration config);
    }
}
