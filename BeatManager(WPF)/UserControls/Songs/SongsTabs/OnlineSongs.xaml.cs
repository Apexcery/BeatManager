using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Interfaces;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.Models.BeatSaverAPI;
using BeatManager_WPF_.Models.BeatSaverAPI.Responses;
using BeatManager_WPF_.Models.SongFilterModels;
using BeatManager_WPF_.ViewModels;

namespace BeatManager_WPF_.UserControls.Songs.SongsTabs
{
    public partial class OnlineSongs : UserControl, INotifyPropertyChanged
    {
        private readonly Config _config;
        private readonly IBeatSaverAPI _beatSaverApi;

        public ObservableCollection<SongTileV2> Items { get; set; } = new ObservableCollection<SongTileV2>();

        public OnlineSongsFilter Filter = new OnlineSongsFilter();

        public int CurrentPageNum = 0;
        public int MaxPageNum = 1;
        public int NumOnPage = 25;

        private bool _hasPreviousPage;
        public bool HasPreviousPage
        {
            get
            {
                _hasPreviousPage = CurrentPageNum > 0;

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

        public bool IsLocal => false;
        public bool IsOnline => true;

        public OnlineSongs(Config config, IBeatSaverAPI beatSaverApi)
        {
            _config = config;
            _beatSaverApi = beatSaverApi;

            InitializeComponent();
            this.DataContext = this;

            var sortFilterButtons = SortFilters;
            foreach (Button button in sortFilterButtons.Children)
            {
                var sortOption = button.Name[(button.Name.IndexOf('_') + 1)..];

                Enum.TryParse(sortOption, true, out MapsSortOption sortOptionEnum);

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
                Trace.WriteLine("--=[ Clearing Online Items ]=--\n");
                Items.Clear();
            }

            Trace.WriteLine($"--=[ Online Search Query: {Filter.SearchQuery} ]=--");
            // Trace.WriteLine($"--=[ Online Difficulty Filter: {Filter.Difficulty} ]=--");
            // Trace.WriteLine($"--=[ Online BPM Range Filter: {Filter.BpmRange?.Start}-{Filter.BpmRange?.End} ]=--");
            Trace.WriteLine($"--=[ Online Sorting By: {Filter.Sort.Option} (Descending) ]=--");

            Maps songs;

            if (string.IsNullOrEmpty(Filter.SearchQuery))
            {
                songs = _beatSaverApi.GetMaps(Filter.Sort.Option, CurrentPageNum).Result;
            }
            else
            {
                RemoveSymbolFromSortButtons(); // Limitation on beatsaver doesn't allow searching and sorting at the same time.
                songs = _beatSaverApi.SearchMaps(Filter.SearchQuery, CurrentPageNum).Result;
            }

            var allOnlineSongs = new List<OnlineSongInfoViewModel>();

            foreach (var song in songs.Songs)
            {
                var songInfoViewModel = new OnlineSongInfoViewModel
                {
                    SongName = song.Name,
                    Artist = song.Metadata.SongAuthorName,
                    Mapper = song.Metadata.LevelAuthorName,
                    FullImagePath = $"https://beatsaver.com{song.CoverURL}",
                    BPM = song.Metadata.Bpm,
                    Hash = song.Hash,
                    DownloadPath = $@"https://beatsaver.com{song.DirectDownload}"
                };

                if (song.Metadata.Difficulties.Easy)
                    songInfoViewModel.Difficulties.Add(new SongInfoViewModel.Difficulty{Name = "Easy", Rank = 1});
                if (song.Metadata.Difficulties.Normal)
                    songInfoViewModel.Difficulties.Add(new SongInfoViewModel.Difficulty{Name = "Normal", Rank = 3});
                if (song.Metadata.Difficulties.Hard)
                    songInfoViewModel.Difficulties.Add(new SongInfoViewModel.Difficulty{Name = "Hard", Rank = 5});
                if (song.Metadata.Difficulties.Expert)
                    songInfoViewModel.Difficulties.Add(new SongInfoViewModel.Difficulty{Name = "Expert", Rank = 7});
                if (song.Metadata.Difficulties.ExpertPlus)
                    songInfoViewModel.Difficulties.Add(new SongInfoViewModel.Difficulty{Name = "ExpertPlus", Rank = 9});

                allOnlineSongs.Add(songInfoViewModel);
            }

            var numSongs = songs.TotalSongs;

            var pageResult = (double)numSongs / NumOnPage;
            MaxPageNum = ((int) Math.Ceiling(pageResult)) - 1;

            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (var song in allOnlineSongs)
                {
                    // var songInfoPanel = new SongTile(false, LoadSongs, _config, _beatSaverApi, onlineSongInfo: song);
                    var songInfoPanelV2 = new SongTileV2(LoadSongs, _config, _beatSaverApi, onlineSongInfo: song);
                    Items.Add(songInfoPanelV2);
                }

                TxtCurrentPage.Text = $"Page {CurrentPageNum + 1} / {MaxPageNum + 1}";

                var lowerBound = (CurrentPageNum * NumOnPage) + 1;
                var upperBound = Math.Min(NumOnPage * (CurrentPageNum + 1), numSongs);
                TxtCurrentCount.Text = $"({lowerBound} to {upperBound}) out of {numSongs}";

                ProgressBar.Visibility = Visibility.Collapsed;
                PageButtons.Visibility = Visibility.Visible;
                GridSongs.Visibility = Visibility.Visible;

                this.OnPropertyChanged("HasPreviousPage");
                this.OnPropertyChanged("HasNextPage");
            });
        }

        private void SortFilter_OnClick(object sender, RoutedEventArgs args, MapsSortOption sortOptionEnum, Button buttonClicked)
        {
            RemoveSymbolFromSortButtons();

            CurrentPageNum = 0;

            buttonClicked.Content = buttonClicked.Tag + " ▼";

            Filter.Sort.Option = sortOptionEnum;
            Filter.SearchQuery = "";
            TxtSearch.Text = "";

            LoadSongs();
        }

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveSymbolFromSortButtons();

            var query = TxtSearch.Text;
            
            Filter.SearchQuery = query;
            
            CurrentPageNum = 0;

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
            if (CurrentPageNum <= 0)
                return;

            CurrentPageNum = 0;
            LoadSongs();
        }

        private void PageButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum <= 0)
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void FilterPanel_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }
}
