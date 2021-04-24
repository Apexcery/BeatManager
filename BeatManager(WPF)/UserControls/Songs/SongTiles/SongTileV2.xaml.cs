using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Enums;
using BeatManager_WPF_.Interfaces;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.ViewModels;
using MaterialDesignThemes.Wpf;
using MoreLinq;
using Newtonsoft.Json;
using Color = System.Windows.Media.Color;

namespace BeatManager_WPF_.UserControls.Songs.SongTiles
{
    public partial class SongTileV2 : UserControl
    {

        private readonly Action _refreshSongs;
        private readonly Config _config;
        private readonly IBeatSaverAPI? _beatSaverAPI;
        private readonly LocalSongInfoViewModel? _localSongInfo;
        private readonly OnlineSongInfoViewModel? _onlineSongInfo;
        private readonly ObservableCollection<SongTileV2> _songTiles;

        public static readonly char[] IllegalCharacters = new char[]
        {
            '<', '>', ':', '/', '\\', '|', '?', '*', '"',
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
            '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000d',
            '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016',
            '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001f',
        };

        public SongTileV2(Action refreshSongs, Config config, IBeatSaverAPI? beatSaverAPI, ObservableCollection<SongTileV2> songTiles, LocalSongInfoViewModel? localSongInfo = null, OnlineSongInfoViewModel? onlineSongInfo = null)
        {
            if (localSongInfo == null && onlineSongInfo == null)
                throw new ArgumentException("At least one of the filters in the song tile must be non-null.");
            if (localSongInfo != null && onlineSongInfo != null)
                throw new ArgumentException("Only one of the song info types should be set.");

            _refreshSongs = refreshSongs;
            _config = config;
            _beatSaverAPI = beatSaverAPI;
            _localSongInfo = localSongInfo;
            _onlineSongInfo = onlineSongInfo;
            _songTiles = songTiles;

            InitializeComponent();

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

            SongTileImage.Source = image;
            SongTileImageBlurred.Source = image;
            SongTileName.Text = _localSongInfo?.SongName ?? _onlineSongInfo!.SongName;
            SongTileArtist.Text = _localSongInfo?.Artist ?? _onlineSongInfo!.Artist;
            SongTileBPM.Content = _localSongInfo != null ? (int)_localSongInfo.BPM : (int)_onlineSongInfo!.BPM;
            ToolTip = _localSongInfo?.SongName ?? _onlineSongInfo!.SongName;
        }

        private void SongTileFlipper_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var tile in _songTiles)
            {
                if (tile == this)
                    continue;
                if (tile.SongTileFlipper.IsFlipped)
                    Flipper.FlipCommand.Execute(null, tile.SongTileFlipper);
            }

            Flipper.FlipCommand.Execute(null, SongTileFlipper);
        }

        private void FrontContentGrid_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var duration = new TimeSpan(0, 0, 0, 0, 100);

            var imageBlurAnimation = new DoubleAnimation(0, 5, duration);
            SongTileImage.Effect = new BlurEffect { Radius = 0 };
            SongTileImage.Effect.BeginAnimation(BlurEffect.RadiusProperty, imageBlurAnimation);

            var opacityAnimation = new DoubleAnimation(0.6, 1, duration);
            
            SongTileDetailBox.BeginAnimation(OpacityProperty, opacityAnimation);

            var bpmBoxBackgroundColorAnimation = new ColorAnimation(Color.FromRgb(0, 0, 0), Color.FromRgb(33, 148, 243), duration);
            var bpmBoxBorderColorAnimation = new ColorAnimation(Color.FromRgb(33, 148, 243), Color.FromRgb(0, 0, 0), duration);
            SongTileBPMBox.Background = new SolidColorBrush();
            SongTileBPMBox.Background.BeginAnimation(SolidColorBrush.ColorProperty, bpmBoxBackgroundColorAnimation);
            SongTileBPMBox.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, bpmBoxBorderColorAnimation);
            SongTileBPMBox.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        private void FrontContentGrid_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var duration = new TimeSpan(0, 0, 0, 0, 500);

            var imageBlurAnimation = new DoubleAnimation(5, 0, duration);
            SongTileImage.Effect = new BlurEffect { Radius = 0 };
            SongTileImage.Effect.BeginAnimation(BlurEffect.RadiusProperty, imageBlurAnimation);

            var opacityAnimation = new DoubleAnimation(1, 0.6, duration);
            SongTileDetailBox.BeginAnimation(OpacityProperty, opacityAnimation);

            var bpmBoxBackgroundColorAnimation = new ColorAnimation(Color.FromRgb(33, 148, 243), Color.FromRgb(0, 0, 0), duration);
            var bpmBoxBorderColorAnimation = new ColorAnimation(Color.FromRgb(0, 0, 0), Color.FromRgb(33, 148, 243), duration);
            SongTileBPMBox.Background = new SolidColorBrush();
            SongTileBPMBox.Background.BeginAnimation(SolidColorBrush.ColorProperty, bpmBoxBackgroundColorAnimation);
            SongTileBPMBox.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, bpmBoxBorderColorAnimation);
            SongTileBPMBox.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        private void BtnSongTilePlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow == null)
                return;

            Application.Current.MainWindow.Effect = new BlurEffect
            {
                Radius = 10
            };

            var popup = new AddRemoveFromPlaylist(_config, _localSongInfo, _onlineSongInfo)
            {
                Owner = Application.Current.MainWindow
            };
            popup.ShowDialog();

            Application.Current.MainWindow.Effect = null;
        }

        private void BtnSongTileDelete_OnClick(object sender, RoutedEventArgs e) //TODO: Remove from all playlists if exists.
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

                    for (var i = Globals.Playlists.Count; i-- > 0;)
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

        private void BtnSongTileDownload_OnClick(object sender, RoutedEventArgs e) //TODO: Download song on a separate thread and show spinner on single song card.
        {
            if (_beatSaverAPI == null || _onlineSongInfo == null)
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
