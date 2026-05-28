using coreWebApi.Models;

namespace coreWebApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        List<User> GetAll();
        Task<User?> GetById(int id);
        Task<User?> GetByEmail(string email);
        void Add(User user);
        Task Update(User user);
        void Delete(User user);
    }
}
