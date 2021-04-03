using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.ViewModels;

namespace BeatManager_WPF_.UserControls.Songs
{
    public partial class SongRowTile : UserControl
    {
        private readonly SongInfoViewModel _songInfo;
        private readonly Playlist? _playlist;

        public SongRowTile(SongInfoViewModel songInfo, Playlist? playlist)
        {
            _songInfo = songInfo;
            _playlist = playlist;

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
    }
}
