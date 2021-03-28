using System.Collections.Generic;

namespace BeatManager_WPF_.ViewModels
{
    public class SongInfoViewModel
    {
        public string FullImagePath { get; set; }
        public string SongName { get; set; }
        public string Artist { get; set; }
        public string Mapper { get; set; }
        public List<string> Difficulties { get; set; }
        public double BPM { get; set; }

        public string FullSongDir { get; set; }
    }
}
