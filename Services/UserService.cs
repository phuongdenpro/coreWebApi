using System.Threading.Tasks;
using coreWebApi.Models;
using coreWebApi.Repositories.Interfaces;
using coreWebApi.Services.Interfaces;

namespace coreWebApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public List<User> GetAll()
        {
            return _repo.GetAll();
        }

        public async Task<User?> GetById(int id)
        {
            return await _repo.GetById(id);
        }

        public void Create(User user)
        {
            _repo.Add(user);
        }

        public void Update(User user)
        {
            _repo.Update(user);
        }

        public async Task Delete(int id)
        {
            var user = await _repo.GetById(id);
            if (user != null)
                _repo.Delete(user);
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _repo.GetByEmail(email);
        }
    }
}
