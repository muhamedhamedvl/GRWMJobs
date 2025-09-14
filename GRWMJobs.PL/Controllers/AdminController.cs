using GRWMJobs.DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GRWMJobs.PL.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Dashboard()
        {
            var users = _db.Users.ToList();
            var sessions = _db.UserSessions.ToList();

            var userRows = users.Select(u => new
            {
                User = u,
                TotalSeconds = sessions.Where(s => s.UserId == u.Id).Sum(s => s.TotalSeconds),
                LastSeen = sessions.Where(s => s.UserId == u.Id).OrderByDescending(s => s.LastSeenAt).FirstOrDefault()?.LastSeenAt
            }).OrderByDescending(x => x.TotalSeconds).ToList();

            ViewBag.UserRows = userRows;
            ViewBag.TotalUsers = users.Count;
            
            return View();
        }
    }
}


