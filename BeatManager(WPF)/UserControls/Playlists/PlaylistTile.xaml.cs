using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Models;

namespace BeatManager_WPF_.UserControls.Playlists
{
    public partial class PlaylistTile : UserControl
    {
        private readonly Config _config;
        private readonly Playlist _playlist;

        public PlaylistTile(Config config, Playlist playlist)
        {
            _config = config;
            _playlist = playlist;

            InitializeComponent();

            this.Loaded += LoadContent;
        }

        private void LoadContent(object sender, RoutedEventArgs e)
        {
            var base64 = _playlist.Image.Substring(_playlist.Image.IndexOf(',') + 1);
            var byteBuffer = Convert.FromBase64String(base64);
            var stream = new MemoryStream(byteBuffer, 0, byteBuffer.Length);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            PlaylistTileImage.Source = image;
            PlaylistTileName.Text = Regex.Replace(_playlist.PlaylistTitle, @"\r\n?|\n", " ");
            PlaylistTileAuthor.Text = Regex.Replace(_playlist.PlaylistAuthor, @"\r\n?|\n", " ");
            ToolTip = Regex.Replace(_playlist.PlaylistTitle, @"\r\n?|\n", " ");
        }

        private void PlaylistTile_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var playlistDetails = new PlaylistDetails(_config, _playlist);

            var windowContent = ((MainWindow) Application.Current.MainWindow)?.WindowContent;
            if (windowContent == null)
                return;

            windowContent.Children.Clear();
            windowContent.Children.Add(playlistDetails);
        }
    }
}
