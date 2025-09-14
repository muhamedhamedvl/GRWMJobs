using System;

namespace GRWMJobs.DAL.Models
{
    public class UserSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public string SessionKey { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
        public long TotalSeconds { get; set; } = 0; 
    }
}


