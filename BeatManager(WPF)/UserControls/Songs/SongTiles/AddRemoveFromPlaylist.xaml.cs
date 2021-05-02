using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.UserControls.Playlists;
using BeatManager_WPF_.UserControls.Songs.SongTiles;
using BeatManager_WPF_.ViewModels;
using MoreLinq;

namespace BeatManager_WPF_.UserControls.Songs
{
    public partial class AddRemoveFromPlaylist : Window
    {
        public ObservableCollection<AddRemovePlaylistTile> Playlists { get; set; } = new ObservableCollection<AddRemovePlaylistTile>();

        private readonly Config _config;
        private readonly LocalSongInfoViewModel? _localSongInfo;
        private readonly OnlineSongInfoViewModel? _onlineSongInfo;

        public AddRemoveFromPlaylist(Config config, LocalSongInfoViewModel? localSongInfo = null, OnlineSongInfoViewModel? onlineSongInfo = null)
        {
            if (localSongInfo == null && onlineSongInfo == null)
                throw new ArgumentException("At least one of the filters in the song tile must be non-null.");
            if (localSongInfo != null && onlineSongInfo != null)
                throw new ArgumentException("Only one of the song info types should be set.");

            _config = config;
            _localSongInfo = localSongInfo;
            _onlineSongInfo = onlineSongInfo;

            InitializeComponent();

            LoadContent();
        }

        private void LoadContent()
        {
            SongName.Text = _localSongInfo?.SongName ?? _onlineSongInfo!.SongName;

            BitmapImage image;
            if (_localSongInfo != null)
            {
                image = new BitmapImage();
                var stream = File.OpenRead(_localSongInfo.FullImagePath);

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                stream.Close();
                stream.Dispose();
            }
            else
            {
                image = new BitmapImage(new Uri(_onlineSongInfo!.FullImagePath));
            }
            SongImage.Source = image;

            SongData.Playlists.OrderBy(x => x.PlaylistTitle).ForEach(p =>
            {
                var playlistTile = new AddRemovePlaylistTile(_config, p, _localSongInfo?.Hash ?? _onlineSongInfo!.Hash);
                Playlists.Add(playlistTile);
            });
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
