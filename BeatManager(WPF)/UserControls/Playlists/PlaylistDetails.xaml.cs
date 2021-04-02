using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Enums;
using BeatManager_WPF_.Models;
using Newtonsoft.Json;

namespace BeatManager_WPF_.UserControls.Playlists
{
    public partial class PlaylistDetails : UserControl
    {
        private readonly Config _config;
        private readonly Playlist _playlist;

        public PlaylistDetails(Config config, Playlist playlist)
        {
            _config = config;
            _playlist = playlist;

            InitializeComponent();

            this.Loaded += LoadContent;
        }

        private void LoadContent(object sender, RoutedEventArgs e)
        {
            var base64 = _playlist.Image[(_playlist.Image.IndexOf(',') + 1)..];
            var byteBuffer = Convert.FromBase64String(base64);
            var stream = new MemoryStream(byteBuffer, 0, byteBuffer.Length);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            ImgPlaylist.Source = image;
            TxtName.Text = _playlist.PlaylistTitle;
            TxtAuthor.Text = _playlist.PlaylistAuthor;
            TxtDesc.Text = _playlist.PlaylistDescription;
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            byte[] bytes = null;
            var encoder = new PngBitmapEncoder();

            if (ImgPlaylist.Source is BitmapSource bitmapSource)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using var stream = new MemoryStream();
                encoder.Save(stream);
                bytes = stream.ToArray();
            }

            if (bytes == null)
            {
                MainWindow.ShowNotification("Failed to save playlist (Failed to encode image).", NotificationSeverityEnum.Error);
                return;
            }

            var base64Image = $"data:image/png;base64,{Convert.ToBase64String(bytes)}";

            var imageString = base64Image != _playlist.Image ? base64Image : _playlist.Image;

            var titleString = "";
            if (!string.IsNullOrEmpty(TxtName.Text) && TxtName.Text != _playlist.PlaylistTitle)
            {
                titleString = TxtName.Text;
            }
            else
            {
                titleString = string.IsNullOrEmpty(_playlist.PlaylistTitle) ? $"New Playlist - {DateTime.UtcNow}" : _playlist.PlaylistTitle;
            }

            var authorString = "";
            if (!string.IsNullOrEmpty(TxtAuthor.Text) && TxtAuthor.Text != _playlist.PlaylistAuthor)
            {
                authorString = TxtAuthor.Text;
            }
            else
            {
                if (!string.IsNullOrEmpty(_playlist.PlaylistAuthor))
                {
                    authorString = _playlist.PlaylistTitle;
                }
            }

            var descString = "";
            if (!string.IsNullOrEmpty(TxtDesc.Text) && TxtDesc.Text != _playlist.PlaylistDescription)
            {
                descString = TxtDesc.Text;
            }
            else
            {
                if (!string.IsNullOrEmpty(_playlist.PlaylistDescription))
                {
                    descString = _playlist.PlaylistDescription;
                }
            }
            
            var editedPlaylist = new Playlist
            {
                Image = imageString,
                PlaylistTitle = titleString.Trim(),
                PlaylistAuthor = authorString.Trim(),
                PlaylistDescription = descString.Trim(),
                Songs = _playlist.Songs
            };

            if (!string.IsNullOrEmpty(_playlist.FullPath))
            {
                File.Delete(_playlist.FullPath);
                Globals.Playlists.Remove(Globals.Playlists.FirstOrDefault(x => x.FullPath.Equals(_playlist.FullPath)));
            }

            var saveLoc = _config.BeatSaberLocation + "/Playlists";
            if (!Directory.Exists(saveLoc))
                Directory.CreateDirectory(saveLoc);

            editedPlaylist.FullPath = Regex.Replace($"{saveLoc}/{editedPlaylist.PlaylistTitle}.json", @"\r\n?|\n", " ");

            File.WriteAllText(editedPlaylist.FullPath, JsonConvert.SerializeObject(editedPlaylist));
            Globals.Playlists.Add(editedPlaylist);
        }
    }
}
