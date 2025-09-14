using GRWM.BLL.Services.IServices;
using GRWMJobs.DAL.Models;
using GRWMJobs.DAL.Repositores.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRWM.BLL.Services
{
    public class SubcategoryService : ISubcategoryService
    {
        private readonly IGenericRepository<Subcategory> _repo;
        public SubcategoryService(IGenericRepository<Subcategory> repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Subcategory>> GetSubcategoriesByCategoryAsync(int categoryId)
        {
            var all = await _repo.GetAllAsync();
            return all.Where(s => s.CategoryId == categoryId);
        }

        public async Task<Subcategory?> GetSubcategoryByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task AddSubcategoryAsync(Subcategory subcategory)
        {
            await _repo.AddAsync(subcategory);
            await _repo.SaveAsync();
        }

        public async Task UpdateSubcategoryAsync(Subcategory subcategory)
        {
            _repo.Update(subcategory);
            await _repo.SaveAsync();
        }

        public async Task DeleteSubcategoryAsync(int id)
        {
            var s = await _repo.GetByIdAsync(id);
            if (s != null)
            {
                _repo.Delete(s);
                await _repo.SaveAsync();
            }
        }
    }
}


