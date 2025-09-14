using GRWMJobs.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRWM.BLL.Services.IServices
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task AddUserAsync(User user);
    }
}


