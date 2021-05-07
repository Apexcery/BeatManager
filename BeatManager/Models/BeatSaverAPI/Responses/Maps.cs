using System.Collections.Generic;
using Newtonsoft.Json;

namespace BeatManager.Models.BeatSaverAPI.Responses
{
    public class Maps
    {
        [JsonProperty("docs")]
        public List<Map> Songs { get; set; }

        [JsonProperty("totalDocs")]
        public int TotalSongs { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }

        [JsonProperty("prevPage")]
        public int? PrevPage { get; set; }

        [JsonProperty("nextPage")]
        public int? NextPage { get; set; }
    }
}
