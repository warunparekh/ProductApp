using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductApp.Data;
using ProductApp.Models;

namespace ProductApp.Repositories
{
    public class ProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) => _db = db;

        public Task<List<Product>> GetAllAsync()
            => _db.Products
                  .Include(p => p.Category)
                  .ToListAsync();

        public Task<Product> GetByIdAsync(int id)
            => _db.Products
                  .Include(p => p.Category)
                  .FirstOrDefaultAsync(p => p.ProductId == id);

        public async Task AddAsync(Product p)
        {
            _db.Products.Add(p);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product p)
        {
            _db.Products.Update(p);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p != null)
            {
                _db.Products.Remove(p);
                await _db.SaveChangesAsync();
            }
        }
    }
}
