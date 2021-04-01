using System.Threading.Tasks;
using BeatManager_WPF_.Models.BeatSaverAPI;
using BeatManager_WPF_.Models.BeatSaverAPI.Responses;

namespace BeatManager_WPF_.Interfaces
{
    public interface IBeatSaverAPI
    {
        public Task<Maps> GetMaps(MapsSortOption sortOption, int page = 1);
        public Task<Maps> SearchMaps(string searchQuery, int page = 1);
    }
}
