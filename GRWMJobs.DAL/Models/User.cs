using System.ComponentModel.DataAnnotations;

namespace GRWMJobs.DAL.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        public Role Role { get; set; } = Role.User;

        public ICollection<Question>? Questions { get; set; }
        public ICollection<Answer>? Answers { get; set; }
    }
}
