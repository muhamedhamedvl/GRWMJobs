using GRWM.BLL.Services.IServices;
using GRWMJobs.DAL.Data;
using GRWMJobs.DAL.Models;
using GRWMJobs.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace GRWMJobs.PL.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService;
        private readonly ICategoryService _categoryService;
        private readonly ISubcategoryService _subcategoryService;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _db;

        public QuestionController(IQuestionService questionService, IAnswerService answerService, ICategoryService categoryService, ISubcategoryService subcategoryService, IWebHostEnvironment env, AppDbContext db)
        {
            _questionService = questionService;
            _answerService = answerService;
            _categoryService = categoryService;
            _subcategoryService = subcategoryService;
            _env = env;
            _db = db;
        }

        public async Task<IActionResult> Index(int trackId, string? search, int page = 1, int pageSize = 10)
        {
            var questions = await _questionService.GetAllQuestionsAsync();
            var query = questions.Where(q => q.TrackId == trackId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                query = query.Where(q =>
                    (q.Title != null && q.Title.ToLower().Contains(s)) ||
                    (q.Content != null && q.Content.ToLower().Contains(s)) ||
                    (q.Hint != null && q.Hint.ToLower().Contains(s))
                );
            }

            var total = query.Count();
            var items = query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new QuestionListViewModel
            {
                Questions = items,
                TrackId = trackId,
                Search = search,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };

            return View(vm);
        }

        public async Task<IActionResult> Answers(int questionId)
        {
            var answers = (await _answerService.GetAnswersByQuestionAsync(questionId)).ToList();
            var answerIds = answers.Select(a => a.Id).ToList();
            if (answerIds.Count > 0)
            {
                var images = _db.Images.Where(i => i.AnswerId != null && answerIds.Contains(i.AnswerId.Value)).ToList();
                foreach (var a in answers)
                {
                    a.Images = images.Where(i => i.AnswerId == a.Id).OrderBy(i => i.Order).ToList();
                }
            }
            var question = await _questionService.GetQuestionByIdAsync(questionId);
            ViewBag.CategoryId = question?.CategoryId;
            ViewBag.SubcategoryId = question?.SubcategoryId;
            return PartialView("_Answers", answers);
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Create(int trackId = 0, int categoryId = 0, int? subcategoryId = null)
        {
            if (subcategoryId.HasValue && subcategoryId.Value > 0)
            {
                var sub = await _subcategoryService.GetSubcategoryByIdAsync(subcategoryId.Value);
                if (sub != null)
                {
                    categoryId = sub.CategoryId;
                }
            }
            if (categoryId > 0 && trackId == 0)
            {
                var cat = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (cat != null) trackId = cat.TrackId;
            }

            ViewBag.TrackId = trackId;
            ViewBag.CategoryId = categoryId;
            ViewBag.SubcategoryId = subcategoryId;
            return View();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Create(Question question, IFormFile? image, IFormFile[]? images)
        {
            var currentUserIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                question.UserId = currentUserId;
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.TrackId = question.TrackId;
                ViewBag.CategoryId = question.CategoryId;
                ViewBag.SubcategoryId = question.SubcategoryId;
                return View(question);
            }

            if (question.SubcategoryId.HasValue && question.SubcategoryId.Value > 0)
            {
                var sub = await _subcategoryService.GetSubcategoryByIdAsync(question.SubcategoryId.Value);
                if (sub != null)
                {
                    question.CategoryId = question.CategoryId == 0 ? sub.CategoryId : question.CategoryId;
                    var cat = await _categoryService.GetCategoryByIdAsync(sub.CategoryId);
                    if (cat != null) question.TrackId = question.TrackId == 0 ? cat.TrackId : question.TrackId;
                }
            }
            else if (question.CategoryId > 0 && question.TrackId == 0)
            {
                var cat = await _categoryService.GetCategoryByIdAsync(question.CategoryId);
                if (cat != null) question.TrackId = cat.TrackId;
            }

            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"q_{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }
                question.ImagePath = $"/uploads/{fileName}";
            }
            await _questionService.AddQuestionAsync(question);

            if (images != null && images.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var toAdd = new List<Image>();
                foreach (var img in images.Where(f => f != null && f.Length > 0))
                {
                    var fileName = $"q_{Guid.NewGuid():N}{Path.GetExtension(img.FileName)}";
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await img.CopyToAsync(stream);
                    }
                    toAdd.Add(new Image
                    {
                        FileName = fileName,
                        FilePath = $"/uploads/{fileName}",
                        QuestionId = question.Id,
                        UserId = currentUserId
                    });
                }
                if (toAdd.Count > 0)
                {
                    _db.Images.AddRange(toAdd);
                    await _db.SaveChangesAsync();
                }
            }
            TempData["Success"] = "Question created successfully.";
            if (question.SubcategoryId.HasValue)
            {
                return RedirectToAction("Details", "Subcategory", new { id = question.SubcategoryId.Value });
            }
            return RedirectToAction("Details", "Category", new { id = question.CategoryId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var q = (await _questionService.GetAllQuestionsAsync()).FirstOrDefault(x => x.Id == id);
            if (q == null) return NotFound();
            // Attach existing images for display
            q.Images = _db.Images.Where(i => i.QuestionId == id).OrderBy(i => i.Order).ToList();
            return View(q);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Edit(Question question, IFormFile? image, IFormFile[]? images)
        {
            if (!ModelState.IsValid) return View(question);


            var existing = await _questionService.GetQuestionByIdAsync(question.Id);
            if (existing == null) return NotFound();


            existing.Title = question.Title;
            existing.Content = question.Content;
            existing.Hint = question.Hint;

            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"q_{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }
                existing.ImagePath = $"/uploads/{fileName}";
            }

            await _questionService.UpdateQuestionAsync(existing);


            if (images != null && images.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var editUserIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var editUserId = int.TryParse(editUserIdClaim, out var userId) ? userId : existing.UserId;
                var toAdd = new List<Image>();
                foreach (var img in images.Where(f => f != null && f.Length > 0))
                {
                    var fileName = $"q_{Guid.NewGuid():N}{Path.GetExtension(img.FileName)}";
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await img.CopyToAsync(stream);
                    }
                    toAdd.Add(new Image
                    {
                        FileName = fileName,
                        FilePath = $"/uploads/{fileName}",
                        QuestionId = existing.Id,
                        UserId = editUserId
                    });
                }
                if (toAdd.Count > 0)
                {
                    _db.Images.AddRange(toAdd);
                    await _db.SaveChangesAsync();
                }
            }
            TempData["Success"] = "Question updated successfully.";
            return RedirectToAction("Details", "Category", new { id = existing.CategoryId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id, int questionId)
        {
            var img = await _db.Images.FindAsync(id);
            if (img == null || img.QuestionId != questionId)
            {
                return NotFound();
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(img.FilePath))
                {
                    var relative = img.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var fullPath = Path.Combine(_env.WebRootPath, relative);
                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                }
            }
            catch { }

            _db.Images.Remove(img);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Image deleted.";
            return RedirectToAction(nameof(Edit), new { id = questionId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id, int categoryId, int? subcategoryId)
        {
            var q = (await _questionService.GetAllQuestionsAsync()).FirstOrDefault(x => x.Id == id);
            if (q == null) return NotFound();
            ViewBag.CategoryId = categoryId;
            ViewBag.SubcategoryId = subcategoryId;
            return View(q);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int categoryId, int? subcategoryId)
        {
            var answers = await _answerService.GetAnswersByQuestionAsync(id);
            foreach (var a in answers)
            {
                await _answerService.DeleteAnswerAsync(a.Id);
            }
            await _questionService.DeleteQuestionAsync(id);
            TempData["Success"] = "Question deleted successfully.";
            if (subcategoryId.HasValue)
            {
                return RedirectToAction("Details", "Subcategory", new { id = subcategoryId.Value });
            }
            return RedirectToAction("Details", "Category", new { id = categoryId });
        }
    }
}
