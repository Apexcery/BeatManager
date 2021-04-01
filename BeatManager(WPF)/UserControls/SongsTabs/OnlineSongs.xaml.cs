﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Enums;
using BeatManager_WPF_.Interfaces;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.Models.BeatSaverAPI;
using BeatManager_WPF_.Models.SongFilterModels;
using BeatManager_WPF_.ViewModels;

namespace BeatManager_WPF_.UserControls.SongsTabs
{
    public partial class OnlineSongs : UserControl, INotifyPropertyChanged
    {
        private readonly Config _config;
        private readonly IBeatSaverAPI _beatSaverApi;

        public ObservableCollection<SongTile> Items { get; set; } = new ObservableCollection<SongTile>();

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

        public OnlineSongs(Config config, IBeatSaverAPI beatSaverApi)
        {
            _config = config;
            _beatSaverApi = beatSaverApi;

            InitializeComponent();
            this.DataContext = this;

            // var difficultyFilterButtons = DifficultyFilters;
            // foreach (Button button in difficultyFilterButtons.Children)
            // {
            //     var difficulty = button.Name[(button.Name.IndexOf('_') + 1)..];
            //
            //     Enum.TryParse(difficulty, true, out DifficultiesEnum difficultyEnum);
            //
            //     button.Click += (o, args) => DifficultyFilter_OnClick(o, args, difficultyEnum);
            // }
            //
            // var bpmFilterButtons = BPMFilters;
            // foreach (Button button in bpmFilterButtons.Children)
            // {
            //     var range = button.Name[(button.Name.IndexOf('_') + 1)..].Split('_');
            //
            //     var lowerRange = int.Parse(range[0]);
            //
            //     var hasUpperRange = range.Length > 1;
            //     var upperRange = hasUpperRange ? int.Parse(range[1]) : int.MaxValue;
            //
            //     var actualRange = new Range(new Index(lowerRange), new Index(upperRange));
            //
            //     button.Click += (o, args) => BPMFilter_OnClick(o, args, actualRange);
            // }

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


            var songs = _beatSaverApi.GetMaps(Filter.Sort.Option, CurrentPageNum).Result;

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

            var numSongs = songs.TotalSongs;

            var pageResult = (double)numSongs / NumOnPage;
            MaxPageNum = ((int) Math.Ceiling(pageResult)) - 1;

            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (var song in allOnlineSongs)
                {
                    var songInfoPanel = GenerateSongInfoPanel(song);
                    Items.Add(songInfoPanel);
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

        // private void DifficultyFilter_OnClick(object sender, RoutedEventArgs e, DifficultiesEnum? difficulty)
        // {
        //     Filter.Difficulty = difficulty;
        //     LoadSongs();
        // }
        //
        // private void BPMFilter_OnClick(object sender, RoutedEventArgs args, in Range actualRange)
        // {
        //     Filter.BpmRange = actualRange;
        //     LoadSongs();
        // }

        private void SortFilter_OnClick(object sender, RoutedEventArgs args, MapsSortOption sortOptionEnum, Button buttonClicked)
        {
            RemoveSymbolFromSortButtons();

            buttonClicked.Content = buttonClicked.Tag + " ▼";

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
    }
}