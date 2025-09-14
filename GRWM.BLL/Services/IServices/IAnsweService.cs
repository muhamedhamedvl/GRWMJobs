using GRWMJobs.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRWM.BLL.Services.IServices
{
    public interface IAnswerService
    {
        Task<IEnumerable<Answer>> GetAnswersByQuestionAsync(int questionId);
        Task<Answer?> GetAnswerByIdAsync(int id);
        Task AddAnswerAsync(Answer answer);
        Task UpdateAnswerAsync(Answer answer);
        Task DeleteAnswerAsync(int id);
    }
}


