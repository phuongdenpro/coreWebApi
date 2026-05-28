using coreWebApi.Models;
using DemoApi.Models;

namespace coreWebApi.Repositories.Interfaces
{
    public interface IProductRepository
    {
        List<Product> GetAll();
        Task<Product?> GetById(int id);
        Task<Product?> GetByName(string name);
        void Add(Product product);
        Task Update(Product product);
        void Delete(Product product);
    }
}
