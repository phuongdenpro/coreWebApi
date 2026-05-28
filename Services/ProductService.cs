using coreWebApi.Repositories.Interfaces;
using coreWebApi.Services.Interfaces;
using DemoApi.Models;

namespace coreWebApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        public void Create(Product product)
        {
            _repo.Add(product);
        }

        public async Task Delete(int id)
        {
            var product = await _repo.GetById(id);
            if (product != null)
                _repo.Delete(product);
        }

        public List<Product> GetAll()
        {
            return _repo.GetAll();
        }

        public async Task<Product?> GetById(int id)
        {
            return await _repo.GetById(id);
        }

        public async Task<Product?> GetByName(string name)
        {
            return await _repo.GetByName(name);
        }

        public void Update(Product product)
        {
            _repo.Update(product);
        }
    }
}
