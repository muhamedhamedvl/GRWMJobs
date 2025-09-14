using GRWMJobs.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRWM.BLL.Services.IServices
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetCategoriesByTrackAsync(int trackId);
        Task<Category?> GetCategoryByIdAsync(int id);
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
    }
}


