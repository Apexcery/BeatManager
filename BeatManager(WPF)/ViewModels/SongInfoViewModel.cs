using System;
using System.Collections.Generic;

namespace BeatManager_WPF_.ViewModels
{
    public class SongInfoViewModel
    {
        public string FullImagePath { get; set; }
        public string SongName { get; set; }
        public string Artist { get; set; }
        public string Mapper { get; set; }
        public List<Difficulty> Difficulties { get; set; }
        public double BPM { get; set; }

        public string FullSongDir { get; set; }
        public DateTime DateAcquired { get; set; }
        public string Hash { get; set; }

        public class Difficulty
        {
            public int Rank { get; set; }
            public string Name { get; set; }
        }
    }
}
