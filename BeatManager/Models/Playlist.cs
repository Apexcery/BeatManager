using System.Collections.Generic;
using Newtonsoft.Json;

namespace BeatManager.Models
{
    public class Playlist
    {
        [JsonProperty("playlistTitle")]
        public string PlaylistTitle { get; set; }

        [JsonProperty("playlistAuthor")]
        public string PlaylistAuthor { get; set; }

        [JsonProperty("playlistDescription")]
        public string PlaylistDescription { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("songs")]
        public List<Song> Songs { get; set; }

        [JsonIgnore]
        public string FullPath { get; set; }

        public class Song
        {
            [JsonProperty("hash")]
            public string Hash { get; set; }
        }
    }
}
