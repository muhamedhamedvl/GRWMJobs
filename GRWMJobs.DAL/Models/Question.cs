using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRWMJobs.DAL.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? Hint { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ImagePath { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int TrackId { get; set; }
        public Track? Track { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int? SubcategoryId { get; set; }
        public Subcategory? Subcategory { get; set; }

        public ICollection<Answer>? Answers { get; set; }
        public ICollection<Image>? Images { get; set; }
    }
}
