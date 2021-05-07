using System.Threading.Tasks;
using BeatManager.Models.BeatSaverAPI;
using BeatManager.Models.BeatSaverAPI.Responses;

namespace BeatManager.Interfaces
{
    public interface IBeatSaverAPI
    {
        public Task<Maps> GetMaps(MapsSortOption sortOption, int page = 1);
        public Task<Maps> SearchMaps(string searchQuery, int page = 1);
        public Task<Map> GetByHash(string hash);
        public Task<bool> DownloadMap(string directDownloadUri, string hash);
    }
}
