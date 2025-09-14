using System.Collections.Generic;

namespace GRWMJobs.DAL.Models
{
    public class Subcategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImagePath { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<Question>? Questions { get; set; }

        public int QuestionCount { get; set; }
    }
}


