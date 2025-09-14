using GRWM.BLL.Services.IServices;
using GRWMJobs.DAL.Models;
using GRWMJobs.DAL.Repositores.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRWM.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepo;

        public UserService(IGenericRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepo.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepo.GetByIdAsync(id);
        }

        public async Task AddUserAsync(User user)
        {
            await _userRepo.AddAsync(user);
            await _userRepo.SaveAsync();
        }
    }
}


