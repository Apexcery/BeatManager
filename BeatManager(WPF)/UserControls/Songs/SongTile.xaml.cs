using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Enums;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.ViewModels;
using FontAwesome.WPF;
using Newtonsoft.Json;
using Color = System.Drawing.Color;

namespace BeatManager_WPF_.UserControls.Songs
{
    public partial class SongTile : UserControl
    {
        private readonly SongInfoViewModel _songInfo;
        private readonly Action _refreshSongs;

        private readonly BitmapImage _image;

        public SongTile(SongInfoViewModel songInfo, bool isLocal, Action refreshSongs)
        {
            _songInfo = songInfo;
            _refreshSongs = refreshSongs;

            InitializeComponent();

            if (isLocal)
            {
                _image = new BitmapImage();
                var stream = File.OpenRead(_songInfo.FullImagePath);

                _image.BeginInit();
                _image.CacheOption = BitmapCacheOption.OnLoad;
                _image.StreamSource = stream;
                _image.EndInit();
                stream.Close();
                stream.Dispose();
            }
            else
            {
                _image = new BitmapImage(new Uri(_songInfo.FullImagePath));
            }

            SongTileImage.Source = _image;
            SongTileName.Text = _songInfo.SongName;
            SongTileArtist.Text = _songInfo.Artist;
            SongTileBPM.Content = (int)_songInfo.BPM;
            ToolTip = _songInfo.SongName;

            var grid = SongTileOptionsPanel.Children.OfType<Grid>().First();
            if (isLocal)
            {
                var playlistButton = CreateButtonForOptionsPanel("BtnSongTileOptionsPlaylist", "Add/Remove from playlists", "MaterialDesignRaisedButton", FontAwesomeIcon.List);
                playlistButton.Click += BtnSongTileOptionsPlaylist_OnClick;
                Grid.SetColumn(playlistButton, 0);
                grid.Children.Add(playlistButton);

                var deleteButton = CreateButtonForOptionsPanel("BtnSongTileOptionsDelete", "Delete song from library", "MaterialDesignRaisedAccentButton", FontAwesomeIcon.Trash);
                deleteButton.Click += BtnSongTileOptionsDelete_OnClick;
                Grid.SetColumn(deleteButton, 1);
                grid.Children.Add(deleteButton);
            }
            else
            {
                var playlistButton = CreateButtonForOptionsPanel("BtnSongTileOptionsPlaylist", "Add/Remove from playlists", "MaterialDesignRaisedButton", FontAwesomeIcon.List);
                playlistButton.Click += BtnSongTileOptionsPlaylist_OnClick;
                Grid.SetColumn(playlistButton, 0);
                grid.Children.Add(playlistButton);
            }
        }

        private Button CreateButtonForOptionsPanel(string buttonName, string tooltip, string style, FontAwesomeIcon icon)
        {
            var button = new Button
            {
                Name = buttonName,
                Width = 50,
                Height = 50,
                Margin = new Thickness(0),
                ToolTip = tooltip
            };
            button.SetResourceReference(StyleProperty, style);

            button.Content = new ImageAwesome
            {
                Icon = icon,
                Style = new Style
                {
                    Triggers =
                    {
                        new DataTrigger
                        {
                            Binding = new Binding
                            {
                                Source = button,
                                Path = new PropertyPath("IsMouseOver")
                            },
                            Value = true,
                            Setters =
                            {
                                new Setter
                                {
                                    Property = ImageAwesome.ForegroundProperty,
                                    Value = new SolidColorBrush(Colors.White)
                                }
                            }
                        },
                        new DataTrigger
                        {
                            Binding = new Binding
                            {
                                Source = button,
                                Path = new PropertyPath("IsMouseOver")
                            },
                            Value = false,
                            Setters =
                            {
                                new Setter
                                {
                                    Property = ImageAwesome.ForegroundProperty,
                                    Value = new SolidColorBrush(Colors.Black)
                                }
                            }
                        }
                    }
                }
            };

            return button;
        }

        private void BtnSongTileOptionsPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void BtnSongTileOptionsDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var songDirectory = _songInfo.FullSongDir;
            if (MessageBox.Show("Are you sure you want to delete this song?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            var songToRemove = Globals.LocalSongs.FirstOrDefault(x => x.FullSongDir == _songInfo.FullSongDir);
            if (songToRemove != null)
            {
                try
                {
                    var infoFilePath = Directory.GetFiles(_songInfo.FullSongDir).FirstOrDefault(x => x.EndsWith("info.dat", StringComparison.InvariantCultureIgnoreCase));
                    var stringToHash = File.ReadAllText(infoFilePath);
                    var songInfo = JsonConvert.DeserializeObject<SongInfo>(stringToHash);
                    foreach (var diffSet in songInfo.DifficultyBeatmapSets)
                    {
                        foreach (var diff in diffSet.DifficultyBeatmaps)
                        {
                            var diffPath = $"{_songInfo.FullSongDir}/{diff.BeatmapFilename}";
                            stringToHash += File.ReadAllText(diffPath);
                        }
                    }
                    var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
                    var hashString = string.Concat(hash.Select(b => b.ToString("x2")));

                    for (var i = Globals.Playlists.Count; i --> 0;)
                    {
                        var playlist = Globals.Playlists[i];
                        var allSongsInPlaylist = playlist.Songs.Select(x => x.Hash).ToList();
                        if (allSongsInPlaylist.Contains(hashString))
                        {
                            allSongsInPlaylist.Remove(hashString);
                            playlist.Songs = allSongsInPlaylist.Select(x => new Playlist.Song { Hash = x }).ToList();
                        }

                        Globals.Playlists[i] = playlist;
                        File.WriteAllText(playlist.FullPath, JsonConvert.SerializeObject(playlist));
                    }

                    Globals.LocalSongs.Remove(songToRemove);

                    _refreshSongs();

                    Directory.Delete(songDirectory, true);

                    MainWindow.ShowNotification("Song successfully deleted.", NotificationSeverityEnum.Success);
                }
                catch (Exception ex)
                {
                    MainWindow.ShowNotification("Song failed to be deleted.", NotificationSeverityEnum.Error);
                }
            }
        }
    }
}
