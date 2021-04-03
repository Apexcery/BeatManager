using System.Collections.Generic;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.ViewModels;

namespace BeatManager_WPF_
{
    public static class Globals
    {
        public static List<Playlist> Playlists = new List<Playlist>();
        public static List<SongInfoViewModel> LocalSongs = new List<SongInfoViewModel>();
    }
}
