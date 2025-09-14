using GRWM.BLL.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GRWMJobs.PL.Controllers
{
    public class TrackController : Controller
    {
        private readonly ITrackService _trackService;
        private readonly ICategoryService _categoryService;
        private readonly IQuestionService _questionService;
        private readonly IAnswerService _answerService;

        private readonly IWebHostEnvironment _env;
        public TrackController(ITrackService trackService, ICategoryService categoryService, IQuestionService questionService, IAnswerService answerService, IWebHostEnvironment env)
        {
            _trackService = trackService;
            _categoryService = categoryService;
            _questionService = questionService;
            _answerService = answerService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var tracks = await _trackService.GetAllTracksAsync();
            return View(tracks);
        }

        public async Task<IActionResult> Details(int id)
        {
            var track = await _trackService.GetTrackByIdAsync(id);
            if (track == null) return NotFound();
            var categories = await _categoryService.GetCategoriesByTrackAsync(id);
            ViewBag.Track = track;
            return View(categories);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Create(Track track, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(track);
            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"t_{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }
                track.ImagePath = $"/uploads/{fileName}";
            }
            await _trackService.AddTrackAsync(track);
            TempData["Success"] = "Track created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var track = await _trackService.GetTrackByIdAsync(id);
            if (track == null) return NotFound();
            return View(track);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Edit(Track track, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(track);

            var existing = await _trackService.GetTrackByIdAsync(track.Id);
            if (existing == null) return NotFound();

            existing.Name = track.Name;

            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"t_{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }
                existing.ImagePath = $"/uploads/{fileName}";
            }

            await _trackService.UpdateTrackAsync(existing);
            TempData["Success"] = "Track updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var track = await _trackService.GetTrackByIdAsync(id);
            if (track == null) return NotFound();
            return View(track);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var questions = (await _questionService.GetAllQuestionsAsync()).Where(q => q.TrackId == id).ToList();
            foreach (var q in questions)
            {
                var answers = await _answerService.GetAnswersByQuestionAsync(q.Id);
                foreach (var a in answers)
                {
                    await _answerService.DeleteAnswerAsync(a.Id);
                }
                await _questionService.DeleteQuestionAsync(q.Id);
            }

            var categories = await _categoryService.GetCategoriesByTrackAsync(id);
            foreach (var c in categories)
            {
                await _categoryService.DeleteCategoryAsync(c.Id);
            }

            await _trackService.DeleteTrackAsync(id);
            TempData["Success"] = "Track deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}


