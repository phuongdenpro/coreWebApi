using coreWebApi.Models;
using coreWebApi.Repositories.Interfaces;
using DemoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace coreWebApi.Repositories
{
    public class ProductRepository : IProductRepository
    {

        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void Delete(Product product)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }

        public List<Product> GetAll()
        {
            return _context.Products.ToList();
        }

        public async Task<Product?> GetById(int id)
        {
            return _context.Products.Find(id);
        }

        public async Task<Product?> GetByName(string name)
        {
            return await _context.Products.FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task Update(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}
