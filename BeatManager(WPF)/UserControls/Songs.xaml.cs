using System.Windows.Controls;
using BeatManager_WPF_.Interfaces;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.UserControls.SongControls;

namespace BeatManager_WPF_.UserControls
{
    public partial class Songs : UserControl
    {
        private readonly Config _config;
        private readonly IBeatSaverAPI _beatSaverApi;

        public Songs(Config config, IBeatSaverAPI beatSaverApi)
        {
            _config = config;
            _beatSaverApi = beatSaverApi;

            InitializeComponent();

            var localSongControl = new LocalSongs(_config);
            LocalTabHeader.Content = localSongControl;

            var onlineSongControl = new OnlineSongs(_config, _beatSaverApi);
            OnlineTabHeader.Content = onlineSongControl;
        }
    }
}
