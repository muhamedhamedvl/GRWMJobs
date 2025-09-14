using GRWMJobs.DAL.Data;
using GRWMJobs.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace GRWMJobs.PL.Middleware
{
    public class UserSessionTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        public UserSessionTrackingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, AppDbContext db)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    var sessionKey = context.Session.Id;
                    if (string.IsNullOrEmpty(sessionKey))
                    {
                        await context.Session.LoadAsync();
                        sessionKey = context.Session.Id;
                    }

                    var session = await db.UserSessions.FirstOrDefaultAsync(s => s.UserId == userId && s.SessionKey == sessionKey);
                    var now = DateTime.UtcNow;
                    if (session == null)
                    {
                        session = new UserSession { UserId = userId, SessionKey = sessionKey, StartedAt = now, LastSeenAt = now, TotalSeconds = 0 };
                        db.UserSessions.Add(session);
                    }
                    else
                    {
                        var delta = (long)(now - session.LastSeenAt).TotalSeconds;
                        if (delta > 0 && delta < 600) // cap idle gaps; ignore if too long
                        {
                            session.TotalSeconds += delta;
                        }
                        session.LastSeenAt = now;
                        db.UserSessions.Update(session);
                    }
                    await db.SaveChangesAsync();
                }
            }

            await _next(context);
        }
    }
}


