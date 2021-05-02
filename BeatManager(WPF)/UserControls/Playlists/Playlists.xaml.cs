using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.Models.FilterModels;
using MoreLinq;

namespace BeatManager_WPF_.UserControls.Playlists
{
    public partial class Playlists : UserControl, INotifyPropertyChanged
    {
        private readonly Config _config;
        private readonly List<Playlist> _playlists;

        public ObservableCollection<PlaylistTile> Items { get; set; } = new ObservableCollection<PlaylistTile>();

        public PlaylistFilter Filter = new PlaylistFilter();

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

        public Playlists(Config config)
        {
            _config = config;
            _playlists = SongData.Playlists;

            InitializeComponent();
            this.DataContext = this;


            var sortFilterButtons = SortFilters;
            foreach (Button button in sortFilterButtons.Children)
            {
                var sortOption = button.Name[(button.Name.IndexOf('_') + 1)..];

                Enum.TryParse(sortOption, true, out PlaylistFilter.SortFilter.SortOptions sortOptionEnum);

                button.Click += (o, args) => SortFilter_OnClick(o, args, sortOptionEnum, button);
            }

            Task task = Task.Run(LoadPlaylists);

            this.Loaded += SetGridMaxHeight;
        }

        private void LoadPlaylists()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                ProgressBar.Visibility = Visibility.Visible;
                PageButtons.Visibility = Visibility.Hidden;
                GridPlaylists.Visibility = Visibility.Hidden;
            });

            if (Items.Count > 0)
            {
                Trace.WriteLine("--=[ Clearing Playlists ]=--\n");
                Items.Clear();
            }

            Trace.WriteLine($"--=[ Local Search Query: {Filter.SearchQuery} ]=--");
            Trace.WriteLine($"--=[ Local Sorting By: {Filter.Sort?.Option?.ToString()} ({Filter.Sort?.Direction?.ToString()}) ]=--");

            var filteredPlaylists = SongData.Playlists;

            if (!string.IsNullOrEmpty(Filter.SearchQuery))
            {
                var tempList = filteredPlaylists;

                filteredPlaylists = tempList.Where(x => x.PlaylistTitle.ToLower().Contains(Filter.SearchQuery.ToLower())).ToList();
                filteredPlaylists = filteredPlaylists.Concat(tempList.Where(x => x.PlaylistAuthor.ToLower().Contains(Filter.SearchQuery.ToLower()))).ToList();
                filteredPlaylists = filteredPlaylists.Concat(tempList.Where(x => x.PlaylistDescription.ToLower().Contains(Filter.SearchQuery.ToLower()))).ToList();
            }

            filteredPlaylists = filteredPlaylists.DistinctBy(x => new { x.PlaylistTitle, x.PlaylistAuthor, x.PlaylistDescription, x.FullPath }).ToList();

            if (Filter.Sort?.Option != null && Filter.Sort.Direction != null)
            {
                switch (Filter.Sort.Option)
                {
                    case PlaylistFilter.SortFilter.SortOptions.Name:
                        switch (Filter.Sort.Direction)
                        {
                            case PlaylistFilter.SortFilter.SortDirection.Ascending:
                                filteredPlaylists = filteredPlaylists.OrderBy(x => x.PlaylistTitle).ToList();
                                break;
                            case PlaylistFilter.SortFilter.SortDirection.Descending:
                                filteredPlaylists = filteredPlaylists.OrderByDescending(x => x.PlaylistTitle).ToList();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(PlaylistFilter.SortFilter.SortDirection));
                        }
                        break;
                    case PlaylistFilter.SortFilter.SortOptions.Author:
                        switch (Filter.Sort.Direction)
                        {
                            case PlaylistFilter.SortFilter.SortDirection.Ascending:
                                filteredPlaylists = filteredPlaylists.OrderBy(x => x.PlaylistAuthor).ToList();
                                break;
                            case PlaylistFilter.SortFilter.SortDirection.Descending:
                                filteredPlaylists = filteredPlaylists.OrderByDescending(x => x.PlaylistAuthor).ToList();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(PlaylistFilter.SortFilter.SortDirection));
                        }
                        break;
                    case PlaylistFilter.SortFilter.SortOptions.NumSongs:
                        switch (Filter.Sort.Direction)
                        {
                            case PlaylistFilter.SortFilter.SortDirection.Ascending:
                                filteredPlaylists = filteredPlaylists.OrderBy(x => x.Songs.Count).ToList();
                                break;
                            case PlaylistFilter.SortFilter.SortDirection.Descending:
                                filteredPlaylists = filteredPlaylists.OrderByDescending(x => x.Songs.Count).ToList();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(PlaylistFilter.SortFilter.SortDirection));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(PlaylistFilter.SortFilter.SortOptions));
                }
            }

            var numPlaylists = filteredPlaylists.Count;

            var pageResult = (double)numPlaylists / NumOnPage;
            MaxPageNum = (int)Math.Ceiling(pageResult);

            var toSkip = CurrentPageNum > 1;
            filteredPlaylists = filteredPlaylists.Skip(toSkip ? (CurrentPageNum - 1) * NumOnPage : 0).Take(NumOnPage).ToList();

            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (var playlist in filteredPlaylists)
                {
                    var playlistInfoTile = new PlaylistTile(_config, playlist, Items, LoadPlaylists);
                    Items.Add(playlistInfoTile);
                }

                TxtCurrentPage.Text = $"Page {CurrentPageNum} / {MaxPageNum}";

                var lowerBound = ((NumOnPage * CurrentPageNum) - NumOnPage) + 1;
                var upperBound = new[] { NumOnPage * CurrentPageNum, numPlaylists }.Min();
                TxtCurrentCount.Text = $"({lowerBound} to {upperBound}) out of {numPlaylists}";

                ProgressBar.Visibility = Visibility.Collapsed;
                PageButtons.Visibility = Visibility.Visible;
                GridPlaylists.Visibility = Visibility.Visible;

                this.OnPropertyChanged("HasPreviousPage");
                this.OnPropertyChanged("HasNextPage");
            });
        }

        private void SetGridMaxHeight(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow;
            var filterPanel = FilterPanel;
            var pagePanel = PagePanel;

            var maxHeight =
                mainWindow.ActualHeight -
                (double)Application.Current.Resources["TopBarHeight"] -
                UI.Padding.Top -
                UI.Padding.Bottom -
                filterPanel.ActualHeight -
                filterPanel.Margin.Top -
                filterPanel.Margin.Bottom -
                pagePanel.ActualHeight -
                pagePanel.Margin.Top -
                pagePanel.Margin.Bottom -
                5; // Minus an extra 5 for slight margin at bottom of screen.

            GridPlaylists.MaxHeight = maxHeight;
        }

        private void NewPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            var playlistDetails = new PlaylistDetails(_config, null);

            var windowContent = ((MainWindow)Application.Current.MainWindow)?.WindowContent;
            if (windowContent == null)
                return;

            windowContent.Children.Clear();
            windowContent.Children.Add(playlistDetails);
        }

        private void SortFilter_OnClick(object sender, RoutedEventArgs args, PlaylistFilter.SortFilter.SortOptions sortOptionEnum, Button buttonClicked)
        {
            RemoveSymbolFromSortButtons();

            CurrentPageNum = 1;

            if (Filter.Sort.Option == sortOptionEnum)
            {
                if (Filter.Sort.Direction == PlaylistFilter.SortFilter.SortDirection.Ascending)
                {
                    Filter.Sort.Direction = PlaylistFilter.SortFilter.SortDirection.Descending;
                    buttonClicked.Content = buttonClicked.Tag + " ▼";
                }
                else
                {
                    Filter.Sort.Direction = PlaylistFilter.SortFilter.SortDirection.Ascending;
                    buttonClicked.Content = buttonClicked.Tag + " ▲";
                }
            }
            else
            {
                Filter.Sort.Direction = PlaylistFilter.SortFilter.SortDirection.Ascending;
                buttonClicked.Content = buttonClicked.Tag + " ▲";
            }

            Filter.Sort.Option = sortOptionEnum;

            LoadPlaylists();
        }

        private void RemoveSymbolFromSortButtons()
        {
            var sortFilterButtons = SortFilters;
            foreach (Button button in sortFilterButtons.Children)
            {
                button.Content = button.Tag;
            }
        }

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            if (Filter.SearchQuery.Equals(TxtSearch.Text))
                return;

            Filter.SearchQuery = TxtSearch.Text;
            LoadPlaylists();
        }

        private void PageButtonFirst_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum <= 1)
                return;

            CurrentPageNum = 1;
            LoadPlaylists();
        }

        private void PageButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum <= 1)
                return;

            CurrentPageNum--;
            LoadPlaylists();
        }

        private void PageButtonForward_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum >= MaxPageNum)
                return;

            CurrentPageNum++;
            LoadPlaylists();
        }

        private void PageButtonLast_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPageNum >= MaxPageNum)
                return;

            CurrentPageNum = MaxPageNum;
            LoadPlaylists();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
