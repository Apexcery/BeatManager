using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using BeatManager.Models;
using MaterialDesignThemes.Wpf;

namespace BeatManager.UserControls.Playlists
{
    public partial class PlaylistTile : UserControl
    {
        private readonly Config _config;
        private readonly Playlist _playlist;

        private readonly ObservableCollection<PlaylistTile> _playlistTiles;
        private readonly Action _loadPlaylists;

        public PlaylistTile(Config config, Playlist playlist, ObservableCollection<PlaylistTile> playlistTiles, Action loadPlaylists)
        {
            _config = config;
            _playlist = playlist;

            _playlistTiles = playlistTiles;
            _loadPlaylists = loadPlaylists;

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
            PlaylistTileImageBlurred.Source = image;

            PlaylistTileName.Text = Regex.Replace(_playlist.PlaylistTitle, @"\r\n?|\n", " ");
            PlaylistTileAuthor.Text = Regex.Replace(_playlist.PlaylistAuthor, @"\r\n?|\n", " ");
            PlaylistTileSongCount.Content = _playlist.Songs.Count;
            ToolTip = Regex.Replace(_playlist.PlaylistTitle, @"\r\n?|\n", " ");
        }

        private void PlaylistTileFlipper_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var tile in _playlistTiles)
            {
                if (tile == this)
                    continue;
                if (tile.PlaylistTileFlipper.IsFlipped)
                    Flipper.FlipCommand.Execute(null, tile.PlaylistTileFlipper);
            }

            Flipper.FlipCommand.Execute(null, PlaylistTileFlipper);
        }

        private void FrontContentGrid_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var duration = new TimeSpan(0, 0, 0, 0, 100);

            var imageBlurAnimation = new DoubleAnimation(0, 5, duration);
            PlaylistTileImage.Effect = new BlurEffect { Radius = 0 };
            PlaylistTileImage.Effect.BeginAnimation(BlurEffect.RadiusProperty, imageBlurAnimation);

            var opacityAnimation = new DoubleAnimation(0.6, 1, duration);

            PlaylistTileDetailBox.BeginAnimation(OpacityProperty, opacityAnimation);

            var bpmBoxBackgroundColorAnimation = new ColorAnimation(Color.FromRgb(0, 0, 0), Color.FromRgb(33, 148, 243), duration);
            var bpmBoxBorderColorAnimation = new ColorAnimation(Color.FromRgb(33, 148, 243), Color.FromRgb(0, 0, 0), duration);
            PlaylistTileSongCountBox.Background = new SolidColorBrush();
            PlaylistTileSongCountBox.Background.BeginAnimation(SolidColorBrush.ColorProperty, bpmBoxBackgroundColorAnimation);
            PlaylistTileSongCountBox.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, bpmBoxBorderColorAnimation);
            PlaylistTileSongCountBox.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        private void FrontContentGrid_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var duration = new TimeSpan(0, 0, 0, 0, 500);

            var imageBlurAnimation = new DoubleAnimation(5, 0, duration);
            PlaylistTileImage.Effect = new BlurEffect { Radius = 0 };
            PlaylistTileImage.Effect.BeginAnimation(BlurEffect.RadiusProperty, imageBlurAnimation);

            var opacityAnimation = new DoubleAnimation(1, 0.6, duration);
            PlaylistTileDetailBox.BeginAnimation(OpacityProperty, opacityAnimation);

            var bpmBoxBackgroundColorAnimation = new ColorAnimation(Color.FromRgb(33, 148, 243), Color.FromRgb(0, 0, 0), duration);
            var bpmBoxBorderColorAnimation = new ColorAnimation(Color.FromRgb(0, 0, 0), Color.FromRgb(33, 148, 243), duration);
            PlaylistTileSongCountBox.Background = new SolidColorBrush();
            PlaylistTileSongCountBox.Background.BeginAnimation(SolidColorBrush.ColorProperty, bpmBoxBackgroundColorAnimation);
            PlaylistTileSongCountBox.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, bpmBoxBorderColorAnimation);
            PlaylistTileSongCountBox.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        private void PlaylistTileViewDetails_OnClick(object sender, RoutedEventArgs e)
        {
            var playlistDetails = new PlaylistDetails(_config, _playlist);

            var windowContent = ((MainWindow)Application.Current.MainWindow)?.WindowContent;
            if (windowContent == null)
                return;

            windowContent.Children.Clear();
            windowContent.Children.Add(playlistDetails);
        }

        private void PlaylistTileDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var response = MessageBox.Show("Are you sure you want to delete this playlist?", "Are you sure?", MessageBoxButton.YesNo);
            if (response == MessageBoxResult.Yes)
            {
                var success = SongData.DeletePlaylist(_playlist);
                if (success)
                    _loadPlaylists();
            }
        }
    }
}
