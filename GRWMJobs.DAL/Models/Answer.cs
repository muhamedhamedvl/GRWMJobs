using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRWMJobs.DAL.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ImagePath { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<Image>? Images { get; set; }
    }
}
