using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Enums;
using BeatManager_WPF_.Interfaces;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.ViewModels;
using FontAwesome.WPF;
using Newtonsoft.Json;

namespace BeatManager_WPF_.UserControls.Songs
{
    public partial class SongTile : UserControl
    {
        private readonly LocalSongInfoViewModel _localSongInfo;
        private readonly OnlineSongInfoViewModel _onlineSongInfo;
        private readonly Action _refreshSongs;
        private readonly Config _config;
        private readonly IBeatSaverAPI _beatSaverAPI;

        public static readonly char[] IllegalCharacters = new char[]
        {
            '<', '>', ':', '/', '\\', '|', '?', '*', '"',
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
            '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000d',
            '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016',
            '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001f',
        };

        private readonly BitmapImage _image;

        public SongTile(bool isLocal, Action refreshSongs, Config config, IBeatSaverAPI beatSaverAPI, LocalSongInfoViewModel localSongInfo = null, OnlineSongInfoViewModel onlineSongInfo = null)
        {
            _localSongInfo = localSongInfo;
            _onlineSongInfo = onlineSongInfo;
            _refreshSongs = refreshSongs;
            _config = config;
            _beatSaverAPI = beatSaverAPI;

            InitializeComponent();

            if (isLocal)
            {
                _image = new BitmapImage();
                var stream = File.OpenRead(_localSongInfo.FullImagePath);

                _image.BeginInit();
                _image.CacheOption = BitmapCacheOption.OnLoad;
                _image.StreamSource = stream;
                _image.EndInit();
                stream.Close();
                stream.Dispose();
            }
            else
            {
                _image = new BitmapImage(new Uri(_onlineSongInfo.FullImagePath));
            }

            SongTileImage.Source = _image;
            SongTileName.Text = _localSongInfo?.SongName ?? _onlineSongInfo.SongName;
            SongTileArtist.Text = _localSongInfo?.Artist ?? _onlineSongInfo.Artist;
            SongTileBPM.Content = _localSongInfo != null ? (int) _localSongInfo.BPM : (int) _onlineSongInfo.BPM;
            ToolTip = _localSongInfo?.SongName ?? _onlineSongInfo.SongName;

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

                var downloadButton = CreateButtonForOptionsPanel("BtnSongTileOptionsDownload", "Download song to library", "MaterialDesignRaisedButton", FontAwesomeIcon.Download);
                downloadButton.Click += BtnSongTileOptionsDownload_OnClick;
                Grid.SetColumn(downloadButton, 1);
                grid.Children.Add(downloadButton);
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
            var songDirectory = _localSongInfo.FullSongDir;
            if (MessageBox.Show("Are you sure you want to delete this song?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            var songToRemove = Globals.LocalSongs.FirstOrDefault(x => x.FullSongDir == _localSongInfo.FullSongDir);
            if (songToRemove != null)
            {
                try
                {
                    var infoFilePath = Directory.GetFiles(_localSongInfo.FullSongDir).FirstOrDefault(x => x.EndsWith("info.dat", StringComparison.InvariantCultureIgnoreCase));
                    var stringToHash = File.ReadAllText(infoFilePath);
                    var songInfo = JsonConvert.DeserializeObject<SongInfo>(stringToHash);
                    foreach (var diffSet in songInfo.DifficultyBeatmapSets)
                    {
                        foreach (var diff in diffSet.DifficultyBeatmaps)
                        {
                            var diffPath = $"{_localSongInfo.FullSongDir}/{diff.BeatmapFilename}";
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

        private void BtnSongTileOptionsDownload_OnClick(object sender, RoutedEventArgs e) //TODO: Download song on a separate thread and show spinner on single song card.
        {
            if(_beatSaverAPI == null || _onlineSongInfo == null)
                return;

            var song = _beatSaverAPI.GetByHash(_onlineSongInfo.Hash).Result;

            var songDirName = string.Concat(($"{song.Key} ({song.Metadata.SongName} - {song.Metadata.LevelAuthorName})").Split(IllegalCharacters));
            if (Directory.Exists($"{_config.BeatSaberLocation}/Beat Saber_Data/CustomLevels/{songDirName}"))
            {
                MainWindow.ShowNotification("Song already exists.", NotificationSeverityEnum.Warning);
                return;
            }

            _beatSaverAPI.DownloadMap(song.DirectDownload, song.Hash).Wait();
            var zip = ZipFile.OpenRead($"./data/{song.Hash}.zip");

            var songDir = $"{_config.BeatSaberLocation}/Beat Saber_Data/CustomLevels/{songDirName}";
            zip.ExtractToDirectory(songDir);

            var files = Directory.GetFiles(songDir);

            var infoFilePath = files.FirstOrDefault(x =>
                x.EndsWith("info.dat", StringComparison.InvariantCultureIgnoreCase));
            if (string.IsNullOrEmpty(infoFilePath))
                return;

            var songInfo = JsonConvert.DeserializeObject<SongInfo>(File.ReadAllText(infoFilePath));
            if (songInfo == null)
                return;

            var stringToHash = File.ReadAllText(infoFilePath);
            foreach (var diffSet in songInfo.DifficultyBeatmapSets)
            {
                foreach (var diff in diffSet.DifficultyBeatmaps)
                {
                    var diffPath = $"{songDir}/{diff.BeatmapFilename}";
                    stringToHash += File.ReadAllText(diffPath);
                }
            }
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
            var hashString = string.Concat(hash.Select(b => b.ToString("x2")));

            var songInfoViewModel = new LocalSongInfoViewModel
            {
                SongName = songInfo.SongName,
                Artist = songInfo.SongAuthorName,
                Mapper = songInfo.LevelAuthorName,
                FullImagePath = $"{songDir}/{songInfo.CoverImageFilename}",
                Difficulties = songInfo.DifficultyBeatmapSets.SelectMany(x => x.DifficultyBeatmaps).Select(x => new SongInfoViewModel.Difficulty
                {
                    Rank = x.DifficultyRank,
                    Name = x.Difficulty
                }).ToList(),
                BPM = songInfo.BeatsPerMinute,

                FullSongDir = songDir,
                DateAcquired = File.GetCreationTimeUtc(infoFilePath),
                Hash = hashString
            };

            Globals.LocalSongs.Add(songInfoViewModel);

            MainWindow.ShowNotification($"Song '{songInfoViewModel.SongName}' downloaded successfully!", NotificationSeverityEnum.Success);
        }
    }
}
