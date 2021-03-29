﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public LocalSongsFilter LocalFilter = new LocalSongsFilter();

        public Songs(Config config)
        {
            _config = config;

            InitializeComponent();

            Task.Run(() => LoadSongs(new LocalSongsFilter()));

            var stackPanel = LocalSongsDifficultyFilters;
            foreach (Button button in stackPanel.Children)
            {
                var difficulty = button.Name.Substring(button.Name.IndexOf('_') + 1);

                Enum.TryParse(difficulty, true, out DifficultyFilter difficultyEnum);

                button.Click += (o, args) => LocalSongsDifficultyFilter_OnClick(o, args, difficultyEnum);
            }

            this.Loaded += SetGridMaxHeight;
        }

        private void SetGridMaxHeight(object sender, RoutedEventArgs e)
        {
            GridLocalSongs.MaxHeight = CalculateMaxGridHeight();
        }

        private async Task LoadSongs(LocalSongsFilter filter)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                LocalSongsProgressBar.Visibility = Visibility.Visible;
                TxtNumLocalSongsFound.Visibility = Visibility.Hidden;
                GridLocalSongs.Visibility = Visibility.Hidden;
            });

            if (Items.Count > 0)
            {
                Trace.WriteLine("--=[ Clearing Items ]=--");
                Items.Clear();
            }

            Trace.WriteLine($"--=[ Search Query: {filter.SearchQuery} ]=--");
            Trace.WriteLine($"--=[ Difficulty Filter: {filter.Difficulty} ]=--");

            var rootDir = _config.BeatSaberLocation;

            var songDirs = Directory.GetDirectories($"{rootDir}/Beat Saber_Data/CustomLevels");

            var allLocalSongs = new List<SongInfoViewModel>();

            foreach (var songDir in songDirs)
            {
                var files = Directory.GetFiles(songDir);
                
                var infoFilePath = files.FirstOrDefault(x =>
                    x.EndsWith("info.dat", StringComparison.InvariantCultureIgnoreCase));
                if (string.IsNullOrEmpty(infoFilePath))
                    continue;

                var songInfo = JsonConvert.DeserializeObject<SongInfo>(await File.ReadAllTextAsync(infoFilePath));

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

            var filteredSongs = allLocalSongs;

            if (filter.Difficulty != null)
            {
                filteredSongs = filteredSongs.Where(x => x.Difficulties.Select(z => z.ToLower()).Contains(filter.Difficulty.ToString()?.ToLower())).ToList();
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                var tempList = filteredSongs;

                filteredSongs = tempList.Where(x => x.SongName.ToLower().Contains(filter.SearchQuery.ToLower())).ToList();
                filteredSongs = filteredSongs.Concat(tempList.Where(x => x.Artist.ToLower().Contains(filter.SearchQuery.ToLower()))).ToList();
                filteredSongs = filteredSongs.Concat(tempList.Where(x => x.Mapper.ToLower().Contains(filter.SearchQuery.ToLower()))).ToList();
            }

            filteredSongs = filteredSongs.OrderBy(x => x.SongName).DistinctBy(x => new { x.SongName, x.Artist, x.Mapper }).ToList();

            var numSongs = filteredSongs.Count;

            filteredSongs = filteredSongs.Take(25).ToList();

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

        private double CalculateMaxGridHeight()
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
                LocalFilter.SearchQuery = null;
                LoadSongs(LocalFilter);
                return;
            }

            LocalFilter.SearchQuery = searchQuery;
            LoadSongs(LocalFilter);
        }

        private void LocalSongsDifficultyFilter_OnClick(object sender, RoutedEventArgs e, DifficultyFilter? difficulty)
        {
            LocalFilter.Difficulty = difficulty;
            LoadSongs(LocalFilter);
        }
    }
}
