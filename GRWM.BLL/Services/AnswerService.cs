using GRWM.BLL.Services.IServices;
using GRWMJobs.DAL.Models;
using GRWMJobs.DAL.Repositores.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRWM.BLL.Services
{
    public class AnswerService : IAnswerService
    {
        private readonly IGenericRepository<Answer> _answerRepo;

        public AnswerService(IGenericRepository<Answer> answerRepo)
        {
            _answerRepo = answerRepo;
        }

        public async Task<IEnumerable<Answer>> GetAnswersByQuestionAsync(int questionId)
        {
            var all = await _answerRepo.GetAllAsync();
            return all.Where(a => a.QuestionId == questionId);
        }

        public async Task<Answer?> GetAnswerByIdAsync(int id)
        {
            return await _answerRepo.GetByIdAsync(id);
        }

        public async Task AddAnswerAsync(Answer answer)
        {
            await _answerRepo.AddAsync(answer);
            await _answerRepo.SaveAsync();
        }

        public async Task UpdateAnswerAsync(Answer answer)
        {
            _answerRepo.Update(answer);
            await _answerRepo.SaveAsync();
        }

        public async Task DeleteAnswerAsync(int id)
        {
            var answer = await _answerRepo.GetByIdAsync(id);
            if (answer != null)
            {
                _answerRepo.Delete(answer);
                await _answerRepo.SaveAsync();
            }
        }
    }
}


