using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Enums;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.Models.SongFilterModels;
using BeatManager_WPF_.ViewModels;
using MoreLinq;
using Newtonsoft.Json;

namespace BeatManager_WPF_.UserControls.SongsTabs
{
    public partial class LocalSongs : UserControl, INotifyPropertyChanged
    {
        private readonly Config _config;

        public ObservableCollection<SongTile> Items { get; set; } = new ObservableCollection<SongTile>();

        public LocalSongsFilter Filter = new LocalSongsFilter();

        public int CurrentPageNum = 1;
        public int MaxPageNum = 1;
        public int NumOnPage = 25;

        private bool _hasPreviousPage;
        public bool HasPreviousPage
        {
            get
            {
                _hasPreviousPage = CurrentPageNum > 1;

                return _hasPreviousPage;
            }
            set
            {
                _hasPreviousPage = value;
                this.OnPropertyChanged("HasPreviousPage");
            }
        }

        private bool _hasNextPage;
        public bool HasNextPage
        {
            get
            {
                _hasNextPage = CurrentPageNum < MaxPageNum;

                return _hasNextPage;
            }
            set
            {
                _hasNextPage = value;
                this.OnPropertyChanged("HasNextPage");
            }
        }

        public LocalSongs(Config config)
        {
            _config = config;

            InitializeComponent();
            this.DataContext = this;

            var difficultyFilterButtons = DifficultyFilters;
            foreach (Button button in difficultyFilterButtons.Children)
            {
                var difficulty = button.Name[(button.Name.IndexOf('_') + 1)..];
            
                Enum.TryParse(difficulty, true, out DifficultiesEnum difficultyEnum);
            
                button.Click += (o, args) => DifficultyFilter_OnClick(o, args, difficultyEnum);
            }
            
            var bpmFilterButtons = BPMFilters;
            foreach (Button button in bpmFilterButtons.Children)
            {
                var range = button.Name[(button.Name.IndexOf('_') + 1)..].Split('_');
                
                var lowerRange = int.Parse(range[0]);
                
                var hasUpperRange = range.Length > 1;
                var upperRange = hasUpperRange ? int.Parse(range[1]) : int.MaxValue;
            
                var actualRange = new Range(new Index(lowerRange), new Index(upperRange));
            
                button.Click += (o, args) => BPMFilter_OnClick(o, args, actualRange);
            }
            
            var sortFilterButtons = SortFilters;
            foreach (Button button in sortFilterButtons.Children)
            {
                var sortOption = button.Name[(button.Name.IndexOf('_') + 1)..];
                
                Enum.TryParse(sortOption, true, out LocalSongsFilter.SortFilter.SortOptions sortOptionEnum);
            
                button.Click += (o, args) => SortFilter_OnClick(o, args, sortOptionEnum, button);
            }

            Task task = Task.Run(LoadSongs);

            this.Loaded += SetGridMaxHeight;
        }

        private void SetGridMaxHeight(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow;
            var tabHeader = (TabItem)this.Parent;
            var filterPanel = FilterPanel;
            var pagePanel = PagePanel;

            var maxHeight =
                mainWindow.ActualHeight -
                (double)Application.Current.Resources["TopBarHeight"] -
                tabHeader.ActualHeight -
                tabHeader.Margin.Top -
                tabHeader.Margin.Bottom -
                UI.Padding.Top -
                UI.Padding.Bottom -
                filterPanel.ActualHeight -
                filterPanel.Margin.Top -
                filterPanel.Margin.Bottom -
                pagePanel.ActualHeight -
                pagePanel.Margin.Top -
                pagePanel.Margin.Bottom -
                5; // Minus an extra 5 for slight margin at bottom of screen.

            GridSongs.MaxHeight = maxHeight;
        }

        private void LoadSongs()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                ProgressBar.Visibility = Visibility.Visible;
                PageButtons.Visibility = Visibility.Hidden;
                GridSongs.Visibility = Visibility.Hidden;
            });

            if (Items.Count > 0)
            {
                Trace.WriteLine("--=[ Clearing Local Items ]=--\n");
                Items.Clear();
            }

            Trace.WriteLine($"--=[ Local Search Query: {Filter.SearchQuery} ]=--");
            Trace.WriteLine($"--=[ Local Difficulty Filter: {Filter.Difficulty} ]=--");
            Trace.WriteLine($"--=[ Local BPM Range Filter: {Filter.BpmRange?.Start}-{Filter.BpmRange?.End} ]=--");
            Trace.WriteLine($"--=[ Local Sorting By: {Filter.Sort?.Option?.ToString()} ({Filter.Sort?.Direction?.ToString()}) ]=--");

            var rootDir = _config.BeatSaberLocation;

            var songDirs = Directory.GetDirectories($"{rootDir}/Beat Saber_Data/CustomLevels");

            var allSongs = new List<SongInfoViewModel>();

            foreach (var songDir in songDirs)
            {
                var files = Directory.GetFiles(songDir);

                var infoFilePath = files.FirstOrDefault(x =>
                    x.EndsWith("info.dat", StringComparison.InvariantCultureIgnoreCase));
                if (string.IsNullOrEmpty(infoFilePath))
                    continue;

                var songInfo = JsonConvert.DeserializeObject<SongInfo>(File.ReadAllText(infoFilePath));
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

                allSongs.Add(songInfoViewModel);
            }

            var filteredSongs = allSongs;

            if (!string.IsNullOrEmpty(Filter.SearchQuery))
            {
                var tempList = filteredSongs;

                filteredSongs = tempList.Where(x => x.SongName.ToLower().Contains(Filter.SearchQuery.ToLower())).ToList();
                filteredSongs = filteredSongs.Concat(tempList.Where(x => x.Artist.ToLower().Contains(Filter.SearchQuery.ToLower()))).ToList();
                filteredSongs = filteredSongs.Concat(tempList.Where(x => x.Mapper.ToLower().Contains(Filter.SearchQuery.ToLower()))).ToList();
            }

            if (Filter.Difficulty != null)
            {
                filteredSongs = filteredSongs.Where(x => x.Difficulties.Select(z => z.Name.ToLower()).Contains(Filter.Difficulty.ToString()?.ToLower())).ToList();
            }

            if (Filter.BpmRange != null)
            {
                filteredSongs = filteredSongs.Where(x => x.BPM >= Filter.BpmRange.Value.Start.Value && x.BPM <= Filter.BpmRange.Value.End.Value).ToList();
            }

            filteredSongs = filteredSongs.DistinctBy(x => new { x.SongName, x.Artist, x.Mapper }).ToList();

            if (Filter.Sort?.Option != null && Filter.Sort.Direction != null)
            {
                switch (Filter.Sort.Option)
                {
                    case LocalSongsFilter.SortFilter.SortOptions.Name:
                        switch (Filter.Sort.Direction)
                        {
                            case LocalSongsFilter.SortFilter.SortDirection.Ascending:
                                filteredSongs = filteredSongs.OrderBy(x => x.SongName).ToList();
                                break;
                            case LocalSongsFilter.SortFilter.SortDirection.Descending:
                                filteredSongs = filteredSongs.OrderByDescending(x => x.SongName).ToList();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(LocalSongsFilter.SortFilter.SortDirection));
                        }
                        break;
                    case LocalSongsFilter.SortFilter.SortOptions.Artist:
                        switch (Filter.Sort.Direction)
                        {
                            case LocalSongsFilter.SortFilter.SortDirection.Ascending:
                                filteredSongs = filteredSongs.OrderBy(x => x.Artist).ToList();
                                break;
                            case LocalSongsFilter.SortFilter.SortDirection.Descending:
                                filteredSongs = filteredSongs.OrderByDescending(x => x.Artist).ToList();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(LocalSongsFilter.SortFilter.SortDirection));
                        }
                        break;
                    case LocalSongsFilter.SortFilter.SortOptions.Difficulty:
                        switch (Filter.Sort.Direction)
                        {
                            case LocalSongsFilter.SortFilter.SortDirection.Ascending:
                                filteredSongs = filteredSongs.OrderBy(x => x.Difficulties.Select(z => z.Rank).Min()).ToList();
                                break;
                            case LocalSongsFilter.SortFilter.SortDirection.Descending:
                                filteredSongs = filteredSongs.OrderByDescending(x => x.Difficulties.Select(z => z.Rank).Min()).ToList();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(LocalSongsFilter.SortFilter.SortDirection));
                        }
                        break;
                    case LocalSongsFilter.SortFilter.SortOptions.BPM:
                        switch (Filter.Sort.Direction)
                        {
                            case LocalSongsFilter.SortFilter.SortDirection.Ascending:
                                filteredSongs = filteredSongs.OrderBy(x => x.BPM).ToList();
                                break;
                            case LocalSongsFilter.SortFilter.SortDirection.Descending:
                                filteredSongs = filteredSongs.OrderByDescending(x => x.BPM).ToList();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(LocalSongsFilter.SortFilter.SortDirection));
                        }
                        break;
                    case LocalSongsFilter.SortFilter.SortOptions.Date:
                        switch (Filter.Sort.Direction)
                        {
                            case LocalSongsFilter.SortFilter.SortDirection.Ascending:
                                filteredSongs = filteredSongs.OrderBy(x => x.DateAcquired).ToList();
                                break;
                            case LocalSongsFilter.SortFilter.SortDirection.Descending:
                                filteredSongs = filteredSongs.OrderByDescending(x => x.DateAcquired).ToList();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(LocalSongsFilter.SortFilter.SortDirection));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(LocalSongsFilter.SortFilter.SortOptions));
                }
            }

            var numSongs = filteredSongs.Count;

            var pageResult = (double)numSongs / NumOnPage;
            MaxPageNum = (int)Math.Ceiling(pageResult);

            var toSkip = CurrentPageNum > 1;
            filteredSongs = filteredSongs.Skip(toSkip ? (CurrentPageNum - 1) * NumOnPage : 0).Take(NumOnPage).ToList();

            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (var song in filteredSongs)
                {
                    var songInfoPanel = GenerateSongInfoPanel(song);
                    Items.Add(songInfoPanel);
                }

                TxtCurrentPage.Text = $"Page {CurrentPageNum} / {MaxPageNum}";

                var lowerBound = ((NumOnPage * CurrentPageNum) - NumOnPage) + 1;
                var upperBound = new[] { NumOnPage * CurrentPageNum, numSongs }.Min();
                TxtCurrentCount.Text = $"({lowerBound} to {upperBound}) out of {numSongs}";

                ProgressBar.Visibility = Visibility.Collapsed;
                PageButtons.Visibility = Visibility.Visible;
                GridSongs.Visibility = Visibility.Visible;

                this.OnPropertyChanged("HasPreviousPage");
                this.OnPropertyChanged("HasNextPage");
            });
        }

        private SongTile GenerateSongInfoPanel(SongInfoViewModel song)
        {
            var tile = new SongTile();

            tile.Dispatcher.Invoke(() =>
            {
                tile = new SongTile
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
            });

            return tile;
        }

        private void DifficultyFilter_OnClick(object sender, RoutedEventArgs e, DifficultiesEnum? difficulty)
        {
            Filter.Difficulty = difficulty;
            LoadSongs();
        }

        private void BPMFilter_OnClick(object sender, RoutedEventArgs args, in Range actualRange)
        {
            Filter.BpmRange = actualRange;
            LoadSongs();
        }

        private void SortFilter_OnClick(object sender, RoutedEventArgs args, LocalSongsFilter.SortFilter.SortOptions sortOptionEnum, Button buttonClicked)
        {
            RemoveSymbolFromSortButtons();
            
            if (Filter.Sort.Option == sortOptionEnum)
            {
                if (Filter.Sort.Direction == LocalSongsFilter.SortFilter.SortDirection.Ascending)
                {
                    Filter.Sort.Direction = LocalSongsFilter.SortFilter.SortDirection.Descending;
                    buttonClicked.Content = buttonClicked.Tag + " ▼";
                }
                else
                {
                    Filter.Sort.Direction = LocalSongsFilter.SortFilter.SortDirection.Ascending;
                    buttonClicked.Content = buttonClicked.Tag + " ▲";
                }
            }
            else
            {
                Filter.Sort.Direction = LocalSongsFilter.SortFilter.SortDirection.Ascending;
                buttonClicked.Content = buttonClicked.Tag + " ▲";
            }
            
            Filter.Sort.Option = sortOptionEnum;
            
            LoadSongs();
        }

        private void RemoveSymbolFromSortButtons()
        {
            var sortFilterButtons = SortFilters;
            foreach (Button button in sortFilterButtons.Children)
            {
                button.Content = button.Tag;
            }
        }

        private void PageButtonFirst_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum <= 1)
                return;

            CurrentPageNum = 1;
            LoadSongs();
        }

        private void PageButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum <= 1)
                return;

            CurrentPageNum--;
            LoadSongs();
        }

        private void PageButtonForward_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum >= MaxPageNum)
                return;

            CurrentPageNum++;
            LoadSongs();
        }

        private void PageButtonLast_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum >= MaxPageNum)
                return;

            CurrentPageNum = MaxPageNum;
            LoadSongs();
        }

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            var query = TxtSearch.Text;
            Filter.SearchQuery = query;
            CurrentPageNum = 1;

            LoadSongs();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
