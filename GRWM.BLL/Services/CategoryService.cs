using GRWM.BLL.Services.IServices;
using GRWMJobs.DAL.Models;
using GRWMJobs.DAL.Repositores.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRWM.BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepo;

        public CategoryService(IGenericRepository<Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<IEnumerable<Category>> GetCategoriesByTrackAsync(int trackId)
        {
            var all = await _categoryRepo.GetAllAsync();
            return all.Where(c => c.TrackId == trackId);
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepo.GetByIdAsync(id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _categoryRepo.AddAsync(category);
            await _categoryRepo.SaveAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _categoryRepo.Update(category);
            await _categoryRepo.SaveAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category != null)
            {
                _categoryRepo.Delete(category);
                await _categoryRepo.SaveAsync();
            }
        }
    }
}


