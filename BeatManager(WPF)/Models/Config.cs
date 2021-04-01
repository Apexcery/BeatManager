using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatManager_WPF_.Models
{
    public class Config
    {
        public string BeatSaberLocation { get; set; } = "";
        public string StartupPage { get; set; } = Page.Songs.ToString();

        public enum Page
        {
            Songs,
            Playlists
        }
    }
}
