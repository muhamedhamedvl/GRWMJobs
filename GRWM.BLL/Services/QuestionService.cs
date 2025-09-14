using GRWM.BLL.Services.IServices;
using GRWMJobs.DAL.Models;
using GRWMJobs.DAL.Repositores.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRWM.BLL.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IGenericRepository<Question> _questionRepo;

        public QuestionService(IGenericRepository<Question> questionRepo)
        {
            _questionRepo = questionRepo;
        }

        public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
        {
            return await _questionRepo.GetAllAsync();
        }

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            return await _questionRepo.GetByIdAsync(id);
        }

        public async Task AddQuestionAsync(Question question)
        {
            await _questionRepo.AddAsync(question);
            await _questionRepo.SaveAsync();
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            _questionRepo.Update(question);
            await _questionRepo.SaveAsync();
        }

        public async Task DeleteQuestionAsync(int id)
        {
            var question = await _questionRepo.GetByIdAsync(id);
            if (question != null)
            {
                _questionRepo.Delete(question);
                await _questionRepo.SaveAsync();
            }
        }
    }
}
