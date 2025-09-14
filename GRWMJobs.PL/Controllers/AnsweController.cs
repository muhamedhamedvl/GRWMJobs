using GRWMJobs.DAL.Data;
using GRWMJobs.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GRWMJobs.PL.Controllers
{
    public class AnswerController : Controller
    {
        private readonly IAnswerService _answerService;
        private readonly IQuestionService _questionService;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;
        public AnswerController(IAnswerService answerService, IQuestionService questionService, IWebHostEnvironment env, AppDbContext context)
        {
            _answerService = answerService;
            _questionService = questionService;
            _env = env;
            _context = context;
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Create(int questionId)
        {
            var q = (await _questionService.GetAllQuestionsAsync()).FirstOrDefault(x => x.Id == questionId);
            if (q == null) return NotFound();
            ViewBag.Question = q;
            return View(new Answer { QuestionId = questionId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Answer answer, IFormFile? image, IFormFile[]? images)
        {
            var currentUserIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                answer.UserId = currentUserId;
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
            if (!ModelState.IsValid)
            {
                return View(answer);
            }
            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"a_{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }
                answer.ImagePath = $"/uploads/{fileName}";
            }
            await _answerService.AddAnswerAsync(answer);
            // Save multiple images
            if (images != null && images.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var toAdd = new List<GRWMJobs.DAL.Models.Image>();
                foreach (var img in images.Where(f => f != null && f.Length > 0))
                {
                    var fileName = $"a_{Guid.NewGuid():N}{Path.GetExtension(img.FileName)}";
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await img.CopyToAsync(stream);
                    }
                    toAdd.Add(new GRWMJobs.DAL.Models.Image
                    {
                        FileName = fileName,
                        FilePath = $"/uploads/{fileName}",
                        AnswerId = answer.Id,
                        UserId = currentUserId
                    });
                }
                if (toAdd.Count > 0)
                {
                    _context.Images.AddRange(toAdd);
                    await _context.SaveChangesAsync();
                }
            }
            TempData["Success"] = "Answer created successfully.";
            var q = await _questionService.GetQuestionByIdAsync(answer.QuestionId);
            if (q?.SubcategoryId != null)
            {
                return RedirectToAction("Details", "Subcategory", new { id = q.SubcategoryId });
            }
            return RedirectToAction("Details", "Category", new { id = q?.CategoryId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var answer = await _answerService.GetAnswerByIdAsync(id);
            if (answer == null) return NotFound();
            var q = await _questionService.GetQuestionByIdAsync(answer.QuestionId);
            ViewBag.Question = q;
            // Attach existing images for display
            var imgs = _context.Images.Where(i => i.AnswerId == id).OrderBy(i => i.Order).ToList();
            answer.Images = imgs;
            return View(answer);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Answer answer, int categoryId, int? subcategoryId, IFormFile? image, IFormFile[]? images)
        {
            var existing = await _answerService.GetAnswerByIdAsync(answer.Id);
            if (existing == null) return NotFound();

            existing.Content = answer.Content;
            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"a_{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }
                existing.ImagePath = $"/uploads/{fileName}";
            }

            await _answerService.UpdateAnswerAsync(existing);
            // Append additional images
            if (images != null && images.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var editUserIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var editUserId = int.TryParse(editUserIdClaim, out var userId) ? userId : existing.UserId;
                var toAdd = new List<GRWMJobs.DAL.Models.Image>();
                foreach (var img in images.Where(f => f != null && f.Length > 0))
                {
                    var fileName = $"a_{Guid.NewGuid():N}{Path.GetExtension(img.FileName)}";
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await img.CopyToAsync(stream);
                    }
                    toAdd.Add(new GRWMJobs.DAL.Models.Image
                    {
                        FileName = fileName,
                        FilePath = $"/uploads/{fileName}",
                        AnswerId = existing.Id,
                        UserId = editUserId
                    });
                }
                if (toAdd.Count > 0)
                {
                    _context.Images.AddRange(toAdd);
                    await _context.SaveChangesAsync();
                }
            }
            TempData["Success"] = "Answer updated successfully.";
            if (subcategoryId.HasValue)
            {
                return RedirectToAction("Details", "Subcategory", new { id = subcategoryId.Value });
            }
            return RedirectToAction("Details", "Category", new { id = categoryId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id, int answerId)
        {
            var img = await _context.Images.FindAsync(id);
            if (img == null || img.AnswerId != answerId)
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

            _context.Images.Remove(img);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Image deleted.";
            return RedirectToAction(nameof(Edit), new { id = answerId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id, int categoryId, int? subcategoryId)
        {
            var answer = await _answerService.GetAnswerByIdAsync(id);
            if (answer == null) return NotFound();
            ViewBag.CategoryId = categoryId;
            ViewBag.SubcategoryId = subcategoryId;
            var q = await _questionService.GetQuestionByIdAsync(answer.QuestionId);
            ViewBag.Question = q;
            return View(answer);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int categoryId, int? subcategoryId)
        {
            await _answerService.DeleteAnswerAsync(id);
            TempData["Success"] = "Answer deleted successfully.";
            if (subcategoryId.HasValue)
            {
                return RedirectToAction("Details", "Subcategory", new { id = subcategoryId.Value });
            }
            return RedirectToAction("Details", "Category", new { id = categoryId });
        }
    }
}
