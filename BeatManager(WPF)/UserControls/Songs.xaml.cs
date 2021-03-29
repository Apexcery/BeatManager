using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.ViewModels;
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

            var difficultyFilterButtons = LocalSongsDifficultyFilters;
            foreach (Button button in difficultyFilterButtons.Children)
            {
                var difficulty = button.Name[(button.Name.IndexOf('_') + 1)..];

                Enum.TryParse(difficulty, true, out LocalSongsFilter.DifficultyFilter difficultyEnum);

                button.Click += (o, args) => LocalSongsDifficultyFilter_OnClick(o, args, difficultyEnum);
            }

            var bpmFilterButtons = LocalSongsBPMFilters;
            foreach (Button button in bpmFilterButtons.Children)
            {
                var range = button.Name[(button.Name.IndexOf('_') + 1)..].Split('_');
                
                var lowerRange = int.Parse(range[0]);
                
                var hasUpperRange = range.Length > 1;
                var upperRange = hasUpperRange ? int.Parse(range[1]) : int.MaxValue;

                var actualRange = new Range(new Index(lowerRange), new Index(upperRange));

                button.Click += (o, args) => LocalSongsBPMFilter_OnClick(o, args, actualRange);
            }

            var sortFilterButtons = LocalSongsSortFilters;
            foreach (Button button in sortFilterButtons.Children)
            {
                var sortOption = button.Name[(button.Name.IndexOf('_') + 1)..];
                
                Enum.TryParse(sortOption, true, out LocalSongsFilter.SortFilter.SortOptions sortOptionEnum);

                button.Click += (o, args) => LocalSongsSortFilter_OnClick(o, args, sortOptionEnum, button);
            }

            SetDefaultSort();

            Task.Run(() => LoadSongs(LocalFilter));

            this.Loaded += SetGridMaxHeight;
        }

        private void SetDefaultSort()
        {
            var sortByNameButton = LocalSongsSortFilter_Name;
            sortByNameButton.Content = sortByNameButton.Tag + " ▲";

            LocalFilter.Sort = new LocalSongsFilter.SortFilter
            {
                Direction = LocalSongsFilter.SortFilter.SortDirection.Ascending,
                Option = LocalSongsFilter.SortFilter.SortOptions.Name
            };
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
                Trace.WriteLine("--=[ Clearing Items ]=--\n");
                Items.Clear();
            }

            Trace.WriteLine($"--=[ Search Query: {filter.SearchQuery} ]=--");
            Trace.WriteLine($"--=[ Difficulty Filter: {filter.Difficulty} ]=--");
            Trace.WriteLine($"--=[ BPM Range Filter: {filter.BpmRange?.Start}-{filter.BpmRange?.End} ]=--");
            Trace.WriteLine($"--=[ Sorting By: {filter.Sort?.Option?.ToString()}-{filter.Sort?.Direction?.ToString()} ]=--");

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
                if (songInfo == null)
                    continue;

                var songInfoViewModel = new SongInfoViewModel
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
                    DateAcquired = File.GetCreationTimeUtc(infoFilePath)
                };

                allLocalSongs.Add(songInfoViewModel);
            }

            var filteredSongs = allLocalSongs;

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                var tempList = filteredSongs;

                filteredSongs = tempList.Where(x => x.SongName.ToLower().Contains(filter.SearchQuery.ToLower())).ToList();
                filteredSongs = filteredSongs.Concat(tempList.Where(x => x.Artist.ToLower().Contains(filter.SearchQuery.ToLower()))).ToList();
                filteredSongs = filteredSongs.Concat(tempList.Where(x => x.Mapper.ToLower().Contains(filter.SearchQuery.ToLower()))).ToList();
            }

            if (filter.Difficulty != null)
            {
                filteredSongs = filteredSongs.Where(x => x.Difficulties.Select(z => z.Name.ToLower()).Contains(filter.Difficulty.ToString()?.ToLower())).ToList();
            }

            if (filter.BpmRange != null)
            {
                filteredSongs = filteredSongs.Where(x => x.BPM >= filter.BpmRange.Value.Start.Value && x.BPM <= filter.BpmRange.Value.End.Value).ToList();
            }

            filteredSongs = filteredSongs.DistinctBy(x => new { x.SongName, x.Artist, x.Mapper }).ToList();

            if (filter.Sort?.Option != null && filter.Sort.Direction != null)
            {
                switch (filter.Sort.Option)
                {
                    case LocalSongsFilter.SortFilter.SortOptions.Name:
                        switch (filter.Sort.Direction)
                        {
                            case LocalSongsFilter.SortFilter.SortDirection.Ascending:
                                filteredSongs = filteredSongs.OrderBy(x => x.SongName).ToList();
                                break;
                            case LocalSongsFilter.SortFilter.SortDirection.Descending:
                                filteredSongs = filteredSongs.OrderByDescending(x => x.SongName).ToList();
                                break;
                        }
                        break;
                    case LocalSongsFilter.SortFilter.SortOptions.Artist:
                        switch (filter.Sort.Direction)
                        {
                            case LocalSongsFilter.SortFilter.SortDirection.Ascending:
                                filteredSongs = filteredSongs.OrderBy(x => x.Artist).ToList();
                                break;
                            case LocalSongsFilter.SortFilter.SortDirection.Descending:
                                filteredSongs = filteredSongs.OrderByDescending(x => x.Artist).ToList();
                                break;
                        }
                        break;
                    case LocalSongsFilter.SortFilter.SortOptions.Difficulty:
                        switch (filter.Sort.Direction)
                        {
                            case LocalSongsFilter.SortFilter.SortDirection.Ascending:
                                filteredSongs = filteredSongs.OrderBy(x => x.Difficulties.Select(z => z.Rank).Min()).ToList();
                                break;
                            case LocalSongsFilter.SortFilter.SortDirection.Descending:
                                filteredSongs = filteredSongs.OrderByDescending(x => x.Difficulties.Select(z => z.Rank).Min()).ToList();
                                break;
                        }
                        break;
                    case LocalSongsFilter.SortFilter.SortOptions.BPM:
                        switch (filter.Sort.Direction)
                        {
                            case LocalSongsFilter.SortFilter.SortDirection.Ascending:
                                filteredSongs = filteredSongs.OrderBy(x => x.BPM).ToList();
                                break;
                            case LocalSongsFilter.SortFilter.SortDirection.Descending:
                                filteredSongs = filteredSongs.OrderByDescending(x => x.BPM).ToList();
                                break;
                        }
                        break;
                    case LocalSongsFilter.SortFilter.SortOptions.Date:
                        switch (filter.Sort.Direction)
                        {
                            case LocalSongsFilter.SortFilter.SortDirection.Ascending:
                                filteredSongs = filteredSongs.OrderBy(x => x.DateAcquired).ToList();
                                break;
                            case LocalSongsFilter.SortFilter.SortDirection.Descending:
                                filteredSongs = filteredSongs.OrderByDescending(x => x.DateAcquired).ToList();
                                break;
                        }
                        break;
                }
            }

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

            LocalFilter.SearchQuery = searchQuery;
            LoadSongs(LocalFilter);
        }

        private void LocalSongsDifficultyFilter_OnClick(object sender, RoutedEventArgs e, LocalSongsFilter.DifficultyFilter? difficulty)
        {
            LocalFilter.Difficulty = difficulty;
            LoadSongs(LocalFilter);
        }

        private void LocalSongsBPMFilter_OnClick(object sender, RoutedEventArgs args, in Range actualRange)
        {
            LocalFilter.BpmRange = actualRange;
            LoadSongs(LocalFilter);
        }

        private void LocalSongsSortFilter_OnClick(object sender, RoutedEventArgs args, LocalSongsFilter.SortFilter.SortOptions sortOptionEnum, Button buttonClicked)
        {
            RemoveSymbolFromSortButtons();

            if (LocalFilter.Sort?.Option == sortOptionEnum)
            {
                if (LocalFilter.Sort.Direction == LocalSongsFilter.SortFilter.SortDirection.Ascending)
                {
                    LocalFilter.Sort.Direction = LocalSongsFilter.SortFilter.SortDirection.Descending;
                    buttonClicked.Content = buttonClicked.Tag + " ▼";
                }
                else
                {
                    LocalFilter.Sort.Direction = LocalSongsFilter.SortFilter.SortDirection.Ascending;
                    buttonClicked.Content = buttonClicked.Tag + " ▲";
                }
            }
            else
            {
                LocalFilter.Sort.Direction = LocalSongsFilter.SortFilter.SortDirection.Ascending;
                buttonClicked.Content = buttonClicked.Tag + " ▲";
            }

            LocalFilter.Sort.Option = sortOptionEnum;

            LoadSongs(LocalFilter);
        }

        private void RemoveSymbolFromSortButtons()
        {
            var sortFilterButtons = LocalSongsSortFilters;
            foreach (Button button in sortFilterButtons.Children)
            {
                button.Content = button.Tag;
            }
        }
    }
}
