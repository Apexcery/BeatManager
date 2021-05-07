using System;
using System.IO;
using System.Net;
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

        public async Task<Map> GetByHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return null;

            var response = await _client.GetAsync($"maps/by-hash/{hash}").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return null;

            var map = JsonConvert.DeserializeObject<Map>(await response.Content.ReadAsStringAsync());

            return map;
        }

        public async Task<bool> DownloadMap(string directDownloadUri, string hash)
        {
            if (string.IsNullOrEmpty(directDownloadUri))
                return false;

            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BeatManager";

            var uri = new Uri(@"https://beatsaver.com" + directDownloadUri);
            var client = new WebClient();
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0");
            client.Headers.Add("Accept-Language", "en-GB,en-US;q=0.7,en;q=0.3");
            client.Headers.Add("Accept", "application/json,text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DownloadFile(uri, $"{appDataFolder}/data/{hash}.zip");

            return true;
        }
    }
}
