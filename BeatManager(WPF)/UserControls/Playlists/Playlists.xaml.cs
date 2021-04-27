using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BeatManager_WPF_.Interfaces;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.Models.SongFilterModels;

namespace BeatManager_WPF_.UserControls.Playlists
{
    public partial class Playlists : UserControl, INotifyPropertyChanged
    {
        private readonly Config _config;
        private readonly List<Playlist> _playlists;

        public ObservableCollection<PlaylistTile> Items { get; set; } = new ObservableCollection<PlaylistTile>();

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

        public Playlists(Config config)
        {
            _config = config;
            _playlists = Globals.Playlists;

            InitializeComponent();
            this.DataContext = this;


            var sortFilterButtons = SortFilters;
            foreach (Button button in sortFilterButtons.Children)
            {
                var sortOption = button.Name[(button.Name.IndexOf('_') + 1)..];

                Enum.TryParse(sortOption, true, out LocalSongsFilter.SortFilter.SortOptions sortOptionEnum);

                button.Click += (o, args) => SortFilter_OnClick(o, args, sortOptionEnum, button);
            }

            Task task = Task.Run(LoadPlaylists);

            this.Loaded += SetGridMaxHeight;
        }

        private void LoadPlaylists()
        {
            var numPlaylists = _playlists.Count;

            var pageResult = (double)numPlaylists / NumOnPage;
            MaxPageNum = (int)Math.Ceiling(pageResult);

            this.Dispatcher.Invoke(delegate
            {
                foreach (var playlist in _playlists.OrderBy(x => x.PlaylistTitle))
                {
                    var playlistInfoTile = GeneratePlaylistTile(playlist);
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

        private PlaylistTile GeneratePlaylistTile(Playlist playlist)
        {
            var tile = new PlaylistTile(_config, playlist);

            return tile;
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

        private void SortFilter_OnClick(object sender, RoutedEventArgs args, LocalSongsFilter.SortFilter.SortOptions sortOptionEnum, Button button)
        {
            throw new NotImplementedException();
        }

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void PageButtonFirst_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void PageButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void PageButtonForward_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void PageButtonLast_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
