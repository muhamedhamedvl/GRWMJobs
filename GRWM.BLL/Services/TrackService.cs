using GRWM.BLL.Services.IServices;
using GRWMJobs.DAL.Models;
using GRWMJobs.DAL.Repositores.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GRWM.BLL.Services
{
    public class TrackService : ITrackService
    {
        private readonly IGenericRepository<Track> _trackRepo;

        public TrackService(IGenericRepository<Track> trackRepo)
        {
            _trackRepo = trackRepo;
        }

        public async Task<IEnumerable<Track>> GetAllTracksAsync()
        {
            return await _trackRepo.GetAllAsync();
        }

        public async Task<Track?> GetTrackByIdAsync(int id)
        {
            return await _trackRepo.GetByIdAsync(id);
        }

        public async Task AddTrackAsync(Track track)
        {
            await _trackRepo.AddAsync(track);
            await _trackRepo.SaveAsync();
        }

        public async Task UpdateTrackAsync(Track track)
        {
            _trackRepo.Update(track);
            await _trackRepo.SaveAsync();
        }

        public async Task DeleteTrackAsync(int id)
        {
            var track = await _trackRepo.GetByIdAsync(id);
            if (track != null)
            {
                _trackRepo.Delete(track);
                await _trackRepo.SaveAsync();
            }
        }
    }
}


