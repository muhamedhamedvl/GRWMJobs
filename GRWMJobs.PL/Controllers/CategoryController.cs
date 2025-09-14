using GRWM.BLL.Services.IServices;
using GRWMJobs.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace GRWMJobs.PL.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService;
        private readonly ISubcategoryService _subcategoryService;

        public CategoryController(ICategoryService categoryService, IQuestionService questionService, IAnswerService answerService, ISubcategoryService subcategoryService)
        {
            _categoryService = categoryService;
            _questionService = questionService;
            _answerService = answerService;
            _subcategoryService = subcategoryService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            if (!(User?.Identity?.IsAuthenticated == true))
            {
                ViewBag.Category = category;
                return View(Enumerable.Empty<Question>());
            }
            var questions = (await _questionService.GetAllQuestionsAsync()).ToList();
            var filtered = questions.Where(q => q.CategoryId == id && q.SubcategoryId == null).ToList();
            var qids = filtered.Select(q => q.Id).ToList();
            if (qids.Count > 0)
            {
                var images = ((GRWMJobs.DAL.Data.AppDbContext)HttpContext.RequestServices.GetService(typeof(GRWMJobs.DAL.Data.AppDbContext))!).Images
                    .Where(i => i.QuestionId != null && qids.Contains(i.QuestionId.Value))
                    .ToList();
                foreach (var q in filtered)
                {
                    q.Images = images.Where(i => i.QuestionId == q.Id).OrderBy(i => i.Order).ToList();
                }
            }
            var subcategories = await _subcategoryService.GetSubcategoriesByCategoryAsync(id);

            foreach (var sub in subcategories)
            {
                sub.QuestionCount = questions.Count(q => q.SubcategoryId == sub.Id);
            }

            ViewBag.Subcategories = subcategories;
            ViewBag.Category = category;
            return View(filtered);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public IActionResult Create(int trackId)
        {
            ViewBag.TrackId = trackId;
            return View();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Create(Category category, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(category);
            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(((IWebHostEnvironment)HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment))!).WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"c_{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }
                category.ImagePath = $"/uploads/{fileName}";
            }
            await _categoryService.AddCategoryAsync(category);
            return RedirectToAction("Details", "Track", new { id = category.TrackId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Edit(Category category, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(category);

            var existing = await _categoryService.GetCategoryByIdAsync(category.Id);
            if (existing == null) return NotFound();

            existing.Name = category.Name;
            existing.TrackId = category.TrackId;

            if (image != null && image.Length > 0)
            {
                var env = (IWebHostEnvironment)HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment))!;
                var uploads = Path.Combine(env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"c_{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }
                existing.ImagePath = $"/uploads/{fileName}";
            }

            await _categoryService.UpdateCategoryAsync(existing);
            return RedirectToAction("Details", "Track", new { id = existing.TrackId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int trackId)
        {
            var questions = (await _questionService.GetAllQuestionsAsync()).Where(q => q.CategoryId == id).ToList();
            foreach (var q in questions)
            {
                var answers = await _answerService.GetAnswersByQuestionAsync(q.Id);
                foreach (var a in answers)
                {
                    await _answerService.DeleteAnswerAsync(a.Id);
                }
                await _questionService.DeleteQuestionAsync(q.Id);
            }

            await _categoryService.DeleteCategoryAsync(id);
            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction("Details", "Track", new { id = trackId });
        }
    }
}


