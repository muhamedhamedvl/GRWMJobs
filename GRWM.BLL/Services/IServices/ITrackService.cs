using GRWMJobs.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRWM.BLL.Services.IServices
{
    public interface ITrackService
    {
        Task<IEnumerable<Track>> GetAllTracksAsync();
        Task<Track?> GetTrackByIdAsync(int id);
        Task AddTrackAsync(Track track);
        Task UpdateTrackAsync(Track track);
        Task DeleteTrackAsync(int id);
    }
}


