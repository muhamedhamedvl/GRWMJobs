using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRWMJobs.DAL.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImagePath { get; set; }

        public ICollection<Category>? Categories { get; set; }
        public ICollection<Question>? Questions { get; set; }
    }
}
