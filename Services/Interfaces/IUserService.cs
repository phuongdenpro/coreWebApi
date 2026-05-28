using coreWebApi.Models;

namespace coreWebApi.Services.Interfaces
{
    public interface IUserService
    {
        List<User> GetAll();
        Task<User?> GetById(int id);
        Task<User?> GetByEmail(string email);
        void Create(User user);
        void Update(User user);
        Task Delete(int id);
    }
}
