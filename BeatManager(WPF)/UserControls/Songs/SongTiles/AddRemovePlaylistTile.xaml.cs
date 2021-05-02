using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Models;
using MaterialDesignThemes.Wpf;

namespace BeatManager_WPF_.UserControls.Songs.SongTiles
{
    public partial class AddRemovePlaylistTile : UserControl, INotifyPropertyChanged
    {
        private readonly Config _config;
        private Playlist _playlist;
        private readonly string _songHash;

        private bool PlaylistContainsSong => _playlist.Songs.Select(x => x.Hash).Contains(_songHash);

        public PackIconKind PackIconKind => PlaylistContainsSong ? PackIconKind.ClearCircle : PackIconKind.TickCircle;
        public string OverlayText => PlaylistContainsSong ? "Remove from Playlist" : "Add to Playlist";
        public string NumSongsInPlaylist => _playlist.Songs.Count.ToString();

        public AddRemovePlaylistTile(Config config, Playlist playlist, string songHash)
        {
            _config = config;
            _playlist = playlist;
            _songHash = songHash;

            InitializeComponent();

            LoadContent();
        }

        private void LoadContent()
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

        private void FrontContentGrid_OnMouseEnter(object sender, MouseEventArgs e)
        {
            TriggerOnHoverAnimations();
        }

        private void TriggerOnHoverAnimations()
        {
            var duration = new TimeSpan(0, 0, 0, 0, 100);

            var imageBlurAnimation = new DoubleAnimation(0, 5, duration);
            PlaylistTileImage.Effect = new BlurEffect { Radius = 0 };
            PlaylistTileImage.Effect.BeginAnimation(BlurEffect.RadiusProperty, imageBlurAnimation);

            var overlayToColor = PlaylistContainsSong ? Color.FromRgb(255, 61, 0) : Color.FromRgb(100, 221, 23);
            var overlayColorAnimation = new ColorAnimation(overlayToColor, duration);
            var overlayOpacityAnimation = new DoubleAnimation(0, 0.9, duration * 2);
            PlaylistTileOverlay.Background.BeginAnimation(SolidColorBrush.ColorProperty, overlayColorAnimation);
            PlaylistTileOverlay.BeginAnimation(OpacityProperty, overlayOpacityAnimation);
        }

        private void FrontContentGrid_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var duration = new TimeSpan(0, 0, 0, 0, 500);

            var imageBlurAnimation = new DoubleAnimation(5, 0, duration);
            PlaylistTileImage.Effect = new BlurEffect { Radius = 0 };
            PlaylistTileImage.Effect.BeginAnimation(BlurEffect.RadiusProperty, imageBlurAnimation);
            
            var overlayOpacityAnimation = new DoubleAnimation(0.8, 0, duration);
            PlaylistTileOverlay.BeginAnimation(OpacityProperty, overlayOpacityAnimation);
        }

        private async void PlaylistTileFlipper_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistContainsSong) // Remove song from playlist.
            {
                var removeSuccess = SongData.RemoveSongFromPlaylist(_playlist, _songHash);
                if (removeSuccess)
                {
                    OnPropertyChanged("PackIconKind");
                    OnPropertyChanged("OverlayText");
                    OnPropertyChanged("NumSongsInPlaylist");
                    TriggerOnHoverAnimations();
                }
                return;
            }

            var addSuccess = SongData.AddSongToPlaylist(_playlist, _songHash);
            if (addSuccess)
            {
                OnPropertyChanged("PackIconKind");
                OnPropertyChanged("OverlayText");
                OnPropertyChanged("NumSongsInPlaylist");
                TriggerOnHoverAnimations();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
