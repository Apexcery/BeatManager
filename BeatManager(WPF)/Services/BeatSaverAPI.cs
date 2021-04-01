using System.Net.Http;
using System.Threading.Tasks;
using BeatManager_WPF_.Interfaces;
using BeatManager_WPF_.Models.BeatSaverAPI;
using BeatManager_WPF_.Models.BeatSaverAPI.Responses;
using Newtonsoft.Json;

namespace BeatManager_WPF_.Services
{
    public class BeatSaverAPI : IBeatSaverAPI
    {
        private readonly HttpClient _client;

        public BeatSaverAPI(HttpClient client)
        {
            _client = client;
        }

        public async Task<Maps> GetMaps(MapsSortOption sortOption, int page = 1)
        {
            var response = await _client.GetAsync($"maps/{sortOption}/{page}").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return null;

            var maps = JsonConvert.DeserializeObject<Maps>(await response.Content.ReadAsStringAsync());

            return maps;
        }

        public async Task<Maps> SearchMaps(string searchQuery, int page = 1)
        {
            if (string.IsNullOrEmpty(searchQuery))
                return null;

            var response = await _client.GetAsync($"search/text/{page}?q={searchQuery}").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return null;

            var maps = JsonConvert.DeserializeObject<Maps>(await response.Content.ReadAsStringAsync());

            return maps;
        }
    }
}
