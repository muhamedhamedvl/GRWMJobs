using GRWMJobs.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRWM.BLL.Services.IServices
{
    public interface IQuestionService
    {
        Task<IEnumerable<Question>> GetAllQuestionsAsync();
        Task<Question?> GetQuestionByIdAsync(int id);
        Task AddQuestionAsync(Question question);
        Task UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(int id);
    }
}
