using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRWMJobs.DAL.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImagePath { get; set; }

        public int TrackId { get; set; }
        public Track? Track { get; set; }

        public ICollection<Question>? Questions { get; set; }
    }
}
