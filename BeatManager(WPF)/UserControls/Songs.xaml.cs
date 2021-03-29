using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.ViewModels;
using MaterialDesignThemes.Wpf;
using MoreLinq;
using Newtonsoft.Json;

namespace BeatManager_WPF_.UserControls
{
    public partial class Songs : UserControl
    {
        private readonly Config _config;
        public ObservableCollection<SongTile> Items { get; set; } = new ObservableCollection<SongTile>();

        public Songs(Config config)
        {
            _config = config;

            InitializeComponent();

            LoadSongs();

            this.Loaded += SetGridMaxHeight;
        }

        private void SetGridMaxHeight(object sender, RoutedEventArgs e)
        {
            GridLocalSongs.MaxHeight = GetMaxGridHeight();
        }

        private async void LoadSongs(string searchQuery = null)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                LocalSongsProgressBar.Visibility = Visibility.Visible;
                TxtNumLocalSongsFound.Visibility = Visibility.Hidden;
                GridLocalSongs.Visibility = Visibility.Hidden;
            });

            Items.Clear();

            var rootDir = _config.BeatSaberLocation;

            var songDirs = Directory.GetDirectories($"{rootDir}/Beat Saber_Data/CustomLevels");

            var allLocalSongs = new List<SongInfoViewModel>();
            var filteredSongs = new List<SongInfoViewModel>();
            var numSongs = 0;

            foreach (var songDir in songDirs)
            {
                var files = Directory.GetFiles(songDir);
                
                var infoFilePath = files.FirstOrDefault(x =>
                    x.EndsWith("info.dat", StringComparison.InvariantCultureIgnoreCase));
                if (string.IsNullOrEmpty(infoFilePath))
                    continue;

                var songInfo = JsonConvert.DeserializeObject<SongInfo>(File.ReadAllText(infoFilePath));

                var songInfoViewModel = new SongInfoViewModel
                {
                    SongName = songInfo.SongName,
                    Artist = songInfo.SongAuthorName,
                    Mapper = songInfo.LevelAuthorName,
                    FullImagePath = $"{songDir}/{songInfo.CoverImageFilename}",
                    Difficulties = songInfo.DifficultyBeatmapSets
                        .First(x => x.BeatmapCharacteristicName.Equals("Standard")).DifficultyBeatmaps
                        .Select(x => x.Difficulty).ToList(),
                    BPM = songInfo.BeatsPerMinute,

                    FullSongDir = songDir
                };

                allLocalSongs.Add(songInfoViewModel);
            }

            if (string.IsNullOrEmpty(searchQuery))
            {
                filteredSongs = allLocalSongs.OrderBy(x => x.SongName).DistinctBy(x => new { x.SongName, x.Artist, x.Mapper }).ToList();

                numSongs = allLocalSongs.Count;

                filteredSongs = filteredSongs.Take(25).ToList();
            }
            else
            {
                filteredSongs = allLocalSongs.Where(x => x.SongName.ToLower().Contains(searchQuery.ToLower())).ToList();
                filteredSongs = filteredSongs.Concat(allLocalSongs.Where(x => x.Artist.ToLower().Contains(searchQuery.ToLower()))).ToList();
                filteredSongs = filteredSongs.Concat(allLocalSongs.Where(x => x.Mapper.ToLower().Contains(searchQuery.ToLower()))).ToList();

                filteredSongs = filteredSongs
                    .DistinctBy(x => new {x.SongName, x.Artist, x.Mapper})
                    .ToList();

                numSongs = filteredSongs.Count;

                filteredSongs = filteredSongs.Take(25).ToList();
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (var song in filteredSongs)
                {
                    var songInfoPanel = GenerateSongInfoPanel(song);
                    Items.Add(songInfoPanel);
                }

                TxtNumLocalSongsFound.Text = $"{numSongs} Songs Found";

                LocalSongsProgressBar.Visibility = Visibility.Hidden;
                TxtNumLocalSongsFound.Visibility = Visibility.Visible;
                GridLocalSongs.Visibility = Visibility.Visible;
            });
        }

        private SongTile GenerateSongInfoPanel(SongInfoViewModel song)
        {
            var tile = new SongTile
            {
                SongTileImage =
                {
                    Source = new BitmapImage(new Uri(song.FullImagePath))
                },
                SongTileName =
                {
                    Text = song.SongName
                },
                SongTileArtist =
                {
                    Text = song.Artist
                },
                ToolTip = song.SongName
            };
            
            return tile;
        }

        private double GetMaxGridHeight()
        {
            var mainWindow = Application.Current.MainWindow;
            var tabHeader = LocalTabHeader;
            var filterPanel = LocalSongsButtonFilterPanel;
            var numLocalSongsText = TxtNumLocalSongsFound;

            var maxHeight =
                mainWindow.ActualHeight -
                (double) Application.Current.Resources["TopBarHeight"] -
                tabHeader.ActualHeight -
                tabHeader.Margin.Top -
                tabHeader.Margin.Bottom -
                UI.Padding.Top -
                UI.Padding.Bottom -
                numLocalSongsText.ActualHeight -
                numLocalSongsText.Margin.Top -
                numLocalSongsText.Margin.Bottom -
                filterPanel.ActualHeight -
                filterPanel.Margin.Top -
                filterPanel.Margin.Bottom -
                5; // Minus an extra 5 for slight bottom margin.

            return maxHeight;
        }

        private void BtnLocalSongsSearch_OnClick(object sender, RoutedEventArgs e)
        {
            var searchQuery = TxtLocalSongsSearch.Text;
            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadSongs();
                return;
            }

            LoadSongs(searchQuery);
        }
    }
}
