using System;
using System.Collections.Generic;

namespace BeatManager.ViewModels
{
    public class SongInfoViewModel
    {
        public string FullImagePath { get; set; }
        public string SongName { get; set; }
        public string Artist { get; set; }
        public string Mapper { get; set; }
        public List<Difficulty> Difficulties { get; set; } = new List<Difficulty>();
        public double BPM { get; set; }
        public string Hash { get; set; }

        public class Difficulty
        {
            public int Rank { get; set; }
            public string Name { get; set; }
        }
    }

    public class LocalSongInfoViewModel : SongInfoViewModel
    {
        public string FullSongDir { get; set; }
        public DateTime DateAcquired { get; set; }
    }

    public class OnlineSongInfoViewModel : SongInfoViewModel
    {
        public string DownloadPath { get; set; }
        public int Downloads { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
    }

    public class SongDetailsViewModel
    {
        public string FullImagePath { get; set; }
        public string SongName { get; set; }
        public string Artist { get; set; }
        public string Mapper { get; set; }
        public string Description { get; set; }
        public List<Difficulty> Difficulties { get; set; } = new List<Difficulty>();
        public double BPM { get; set; }
        public string Hash { get; set; }
        public string DownloadPath { get; set; }
        public int Downloads { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }

        public class Difficulty
        {
            public int Rank { get; set; }
            public string Name { get; set; }
        }
    }
}
