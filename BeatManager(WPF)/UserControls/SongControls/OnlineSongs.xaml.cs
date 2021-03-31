using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Interfaces;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.Models.BeatSaverAPI;
using BeatManager_WPF_.ViewModels;

namespace BeatManager_WPF_.UserControls.SongControls
{
    public partial class OnlineSongs : UserControl
    {
        private readonly Config _config;
        private readonly IBeatSaverAPI _beatSaverApi;

        public ObservableCollection<SongTile> Items { get; set; } = new ObservableCollection<SongTile>();

        public LocalSongsFilter Filter = new LocalSongsFilter();

        public int CurrentPageNum = 1;
        public int MaxPageNum = 1;
        public int NumOnPage = 25;

        public OnlineSongs(Config config, IBeatSaverAPI beatSaverApi)
        {
            _config = config;
            _beatSaverApi = beatSaverApi;

            InitializeComponent();

            var difficultyFilterButtons = DifficultyFilters;
            foreach (Button button in difficultyFilterButtons.Children)
            {
                var difficulty = button.Name[(button.Name.IndexOf('_') + 1)..];

                Enum.TryParse(difficulty, true, out LocalSongsFilter.DifficultyFilter difficultyEnum);

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

            Task task = Task.Run((Action) LoadSongs);

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
                Trace.WriteLine("--=[ Clearing Online Items ]=--\n");
                Items.Clear();
            }

            Trace.WriteLine($"--=[ Online Search Query: {Filter.SearchQuery} ]=--");
            Trace.WriteLine($"--=[ Online Difficulty Filter: {Filter.Difficulty} ]=--");
            Trace.WriteLine($"--=[ Online BPM Range Filter: {Filter.BpmRange?.Start}-{Filter.BpmRange?.End} ]=--");
            Trace.WriteLine($"--=[ Online Sorting By: {Filter.Sort?.Option?.ToString()}-{Filter.Sort?.Direction?.ToString()} ]=--");


            var songs = _beatSaverApi.GetMaps(MapsSortOption.Rating).Result;

            var allOnlineSongs = new List<SongInfoViewModel>();

            foreach (var song in songs.Songs)
            {
                allOnlineSongs.Add(new SongInfoViewModel
                {
                    SongName = song.Name,
                    Artist = song.Metadata.SongAuthorName,
                    Mapper = song.Metadata.LevelAuthorName,
                    FullImagePath = $"https://beatsaver.com{song.CoverURL}",
                    BPM = song.Metadata.Bpm
                });
            }

            var numSongs = allOnlineSongs.Count;

            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (var song in allOnlineSongs)
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

        private void DifficultyFilter_OnClick(object sender, RoutedEventArgs e, LocalSongsFilter.DifficultyFilter? difficulty)
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
            RemoveSymbolFromLocalSortButtons();

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

        private void RemoveSymbolFromLocalSortButtons()
        {
            var sortFilterButtons = SortFilters;
            foreach (Button button in sortFilterButtons.Children)
            {
                button.Content = button.Tag;
            }
        }

        private void PageButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum > 1)
                CurrentPageNum--;
            else
                return;

            LoadSongs();

            if (CurrentPageNum == 1)
                PageButtonBack.IsEnabled = false;

            if (CurrentPageNum < MaxPageNum)
                PageButtonForward.IsEnabled = true;
        }

        private void PageButtonForward_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum < MaxPageNum)
                CurrentPageNum++;
            else
                return;

            LoadSongs();

            if (CurrentPageNum == MaxPageNum)
                PageButtonForward.IsEnabled = false;

            if (CurrentPageNum > 1)
                PageButtonBack.IsEnabled = true;
        }
    }
}
