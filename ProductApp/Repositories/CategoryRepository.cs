using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductApp.Data;
using ProductApp.Models;

namespace ProductApp.Repositories
{
    public class CategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) => _db = db;

        public Task<List<Category>> GetAllAsync()
            => _db.Categories.ToListAsync();

        public Task<Category> GetByIdAsync(int id)
            => _db.Categories.FindAsync(id).AsTask();

        public async Task AddAsync(Category c)
        {
            _db.Categories.Add(c);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category c)
        {
            _db.Categories.Update(c);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c != null)
            {
                _db.Categories.Remove(c);
                await _db.SaveChangesAsync();
            }
        }
    }
}
