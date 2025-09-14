using GRWM.BLL.Services.IServices;
using GRWMJobs.DAL.Models;
using GRWMJobs.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GRWMJobs.PL.Controllers
{
    public class SubcategoryController : Controller
    {
        private readonly ISubcategoryService _subcategoryService;
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService;
        private readonly IWebHostEnvironment _env;

        public SubcategoryController(ISubcategoryService subcategoryService, IQuestionService questionService, IAnswerService answerService, IWebHostEnvironment env)
        {
            _subcategoryService = subcategoryService;
            _questionService = questionService;
            _answerService = answerService;
            _env = env;
        }

        public async Task<IActionResult> Details(int id, string? search, int page = 1, int pageSize = 10)
        {
            var sub = await _subcategoryService.GetSubcategoryByIdAsync(id);
            if (sub == null) return NotFound();

            var allQuestions = (await _questionService.GetAllQuestionsAsync()).ToList();
            var query = allQuestions.Where(q => q.SubcategoryId == id);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim().ToLowerInvariant();
                query = query.Where(q =>
                    (q.Title != null && q.Title.ToLower().Contains(searchTerm)) ||
                    (q.Content != null && q.Content.ToLower().Contains(searchTerm)) ||
                    (q.Hint != null && q.Hint.ToLower().Contains(searchTerm))
                );
            }

            var totalCount = query.Count();
            var questions = query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            var qids = questions.Select(q => q.Id).ToList();
            if (qids.Count > 0)
            {
                var images = ((GRWMJobs.DAL.Data.AppDbContext)HttpContext.RequestServices.GetService(typeof(GRWMJobs.DAL.Data.AppDbContext))!).Images
                    .Where(i => i.QuestionId != null && qids.Contains(i.QuestionId.Value))
                    .ToList();
                foreach (var q in questions)
                {
                    q.Images = images.Where(i => i.QuestionId == q.Id).OrderBy(i => i.Order).ToList();
                }
            }

            var viewModel = new SubcategoryQuestionsViewModel
            {
                Questions = questions,
                Subcategory = sub,
                Search = search,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public IActionResult Create(int categoryId)
        {
            return View(new Subcategory { CategoryId = categoryId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subcategory subcategory, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(subcategory);
            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"sc_{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }
                subcategory.ImagePath = $"/uploads/{fileName}";
            }
            await _subcategoryService.AddSubcategoryAsync(subcategory);
            TempData["Success"] = "Subcategory created successfully.";
            return RedirectToAction("Details", "Category", new { id = subcategory.CategoryId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sub = await _subcategoryService.GetSubcategoryByIdAsync(id);
            if (sub == null) return NotFound();
            return View(sub);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Subcategory subcategory, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(subcategory);

            var existing = await _subcategoryService.GetSubcategoryByIdAsync(subcategory.Id);
            if (existing == null) return NotFound();

            existing.Name = subcategory.Name;
            existing.CategoryId = subcategory.CategoryId;

            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"sc_{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }
                existing.ImagePath = $"/uploads/{fileName}";
            }
            await _subcategoryService.UpdateSubcategoryAsync(existing);
            TempData["Success"] = "Subcategory updated successfully.";
            return RedirectToAction("Details", "Category", new { id = existing.CategoryId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var sub = await _subcategoryService.GetSubcategoryByIdAsync(id);
            if (sub == null) return NotFound();
            return View(sub);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sub = await _subcategoryService.GetSubcategoryByIdAsync(id);
            if (sub == null) return NotFound();
            var questions = (await _questionService.GetAllQuestionsAsync()).Where(q => q.SubcategoryId == id).ToList();
            foreach (var q in questions)
            {
                var answers = await _answerService.GetAnswersByQuestionAsync(q.Id);
                foreach (var a in answers)
                {
                    await _answerService.DeleteAnswerAsync(a.Id);
                }
                await _questionService.DeleteQuestionAsync(q.Id);
            }

            await _subcategoryService.DeleteSubcategoryAsync(id);
            TempData["Success"] = "Subcategory deleted successfully.";
            return RedirectToAction("Details", "Category", new { id = sub.CategoryId });
        }
    }
}


