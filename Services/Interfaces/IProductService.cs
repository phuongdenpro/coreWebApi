using coreWebApi.Models;
using DemoApi.Models;

namespace coreWebApi.Services.Interfaces
{
    public interface IProductService
    {
        List<Product> GetAll();
        Task<Product?> GetById(int id);
        Task<Product?> GetByName(string name);
        void Create(Product product);
        void Update(Product product);
        Task Delete(int id);
    }
}
