using GRWMJobs.DAL.Data;
using GRWMJobs.DAL.Models;
using GRWM.BLL.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GRWMJobs.PL.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IServices.IQuestionService _questionService;
        private readonly IServices.IAnswerService _answerService;
        public UserController(AppDbContext context, GRWM.BLL.Services.IServices.IQuestionService questionService, GRWM.BLL.Services.IServices.IAnswerService answerService)
        {
            _context = context;
            _questionService = questionService;
            _answerService = answerService;
        }

        public async Task<IActionResult> Index()
        {
            var users = _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var sessions = _context.UserSessions.Where(s => s.UserId == id);
            _context.UserSessions.RemoveRange(sessions);

            var allAnswers = await _answerService.GetAnswersByQuestionAsync(-1); 

            var userAnswers = _context.Answers.Where(a => a.UserId == id).ToList();
            foreach (var a in userAnswers)
            {
                await _answerService.DeleteAnswerAsync(a.Id);
            }


            var userQuestions = _context.Questions.Where(q => q.UserId == id).ToList();
            foreach (var q in userQuestions)
            {

                var qAnswers = _context.Answers.Where(a => a.QuestionId == q.Id).ToList();
                foreach (var a in qAnswers)
                {
                    await _answerService.DeleteAnswerAsync(a.Id);
                }
                await _questionService.DeleteQuestionAsync(q.Id);
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}


