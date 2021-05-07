using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BeatManager.Enums;
using BeatManager.Models;
using BeatManager.ViewModels;
using Newtonsoft.Json;

namespace BeatManager.UserControls.Playlists
{
    public partial class PlaylistSongRowTile : UserControl
    {
        private readonly SongInfoViewModel _songInfo;
        private readonly Playlist _playlist;
        private readonly Action _updateList;

        public PlaylistSongRowTile(SongInfoViewModel songInfo, Playlist playlist, Action updateList)
        {
            _songInfo = songInfo;
            _playlist = playlist;
            _updateList = updateList;

            InitializeComponent();

            this.Loaded += LoadContent;
        }

        private void LoadContent(object sender, RoutedEventArgs e)
        {
            LblSongName.Content = _songInfo.SongName;
            LblSongArtist.Content = _songInfo.Artist;
            LblSongMapper.Content = _songInfo.Mapper;

            ImgSong.Source = new BitmapImage(new Uri(_songInfo.FullImagePath));
        }

        private void BtnRemoveFromPlaylist_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var response = MessageBox.Show("Are you sure you want to remove this song?", "Are you sure?", MessageBoxButton.YesNo);
            if (response == MessageBoxResult.No || response == MessageBoxResult.Cancel || response == MessageBoxResult.None)
                return;

            var toRemove = _playlist.Songs.FirstOrDefault(x => x.Hash.Equals(_songInfo.Hash));
            if (toRemove == null)
            {
                MainWindow.ShowNotification("Failed to remove song from playlist.", NotificationSeverityEnum.Error);
                return;
            }

            _playlist.Songs.Remove(toRemove);

            var index = SongData.Playlists.FindIndex(x => x.FullPath == _playlist.FullPath);
            if (index == -1)
            {
                MainWindow.ShowNotification("Failed to remove song from playlist.", NotificationSeverityEnum.Error);
                return;
            }

            SongData.Playlists[index] = _playlist;

            File.WriteAllText(_playlist.FullPath, JsonConvert.SerializeObject(_playlist, Formatting.None));

            _updateList();

            MainWindow.ShowNotification("Successfully removed song from playlist.", NotificationSeverityEnum.Success);
        }
    }
}
