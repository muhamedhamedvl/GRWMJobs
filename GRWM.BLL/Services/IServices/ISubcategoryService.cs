using GRWMJobs.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRWM.BLL.Services.IServices
{
    public interface ISubcategoryService
    {
        Task<IEnumerable<Subcategory>> GetSubcategoriesByCategoryAsync(int categoryId);
        Task<Subcategory?> GetSubcategoryByIdAsync(int id);
        Task AddSubcategoryAsync(Subcategory subcategory);
        Task UpdateSubcategoryAsync(Subcategory subcategory);
        Task DeleteSubcategoryAsync(int id);
    }
}


