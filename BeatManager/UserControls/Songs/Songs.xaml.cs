using System.Collections.Generic;
using System.Windows.Controls;
using BeatManager.Interfaces;
using BeatManager.Models;
using BeatManager.UserControls.Songs.SongsTabs;

namespace BeatManager.UserControls.Songs
{
    public partial class Songs : UserControl
    {
        private readonly Config _config;
        private readonly IBeatSaverAPI _beatSaverApi;
        private readonly List<Playlist> _playlists;

        public Songs(Config config, IBeatSaverAPI beatSaverApi)
        {
            _config = config;
            _beatSaverApi = beatSaverApi;
            _playlists = SongData.Playlists;

            InitializeComponent();

            var localSongControl = new LocalSongs(_config);
            LocalTabHeader.Content = localSongControl;

            var onlineSongControl = new OnlineSongs(_config, _beatSaverApi);
            OnlineTabHeader.Content = onlineSongControl;
        }
    }
}
