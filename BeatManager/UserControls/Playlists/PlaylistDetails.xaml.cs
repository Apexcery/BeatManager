using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BeatManager.Enums;
using BeatManager.Models;
using BeatManager.Models.FilterModels;
using MoreLinq;
using Newtonsoft.Json;

namespace BeatManager.UserControls.Playlists
{
    public partial class PlaylistDetails : UserControl, INotifyPropertyChanged
    {
        private readonly Config _config;
        private readonly Playlist? _playlist;

        public ObservableCollection<PlaylistSongRowTile> Songs { get; set; } = new ObservableCollection<PlaylistSongRowTile>();

        public LocalSongsFilter Filter = new LocalSongsFilter();

        private int CurrentPageNum = 1;
        private int MaxPageNum = 1;
        private int NumOnPage = 10;

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

        public PlaylistDetails(Config config, Playlist? playlist)
        {
            _config = config;
            _playlist = playlist;

            InitializeComponent();

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

            this.Loaded += LoadContent;
        }

        private void DifficultyFilter_OnClick(object sender, RoutedEventArgs e, DifficultiesEnum? difficulty)
        {
            CurrentPageNum = 1;
            Filter.Difficulty = difficulty;
            LoadSongs();
        }

        private void BPMFilter_OnClick(object sender, RoutedEventArgs args, in Range actualRange)
        {
            CurrentPageNum = 1;
            Filter.BpmRange = actualRange;
            LoadSongs();
        }

        private void SortFilter_OnClick(object sender, RoutedEventArgs args, LocalSongsFilter.SortFilter.SortOptions sortOptionEnum, Button buttonClicked)
        {
            RemoveSymbolFromSortButtons();

            CurrentPageNum = 1;

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

        private void LoadContent(object sender, RoutedEventArgs e)
        {
            if (_playlist == null) // New playlist
            {
                TxtName.Text = "New Playlist";
                TxtAuthor.Text = "Beat Manager";
                TxtDesc.Text = "Playlist created by Beat Manager";
                return;
            }

            var base64 = _playlist.Image[(_playlist.Image.IndexOf(',') + 1)..];
            var byteBuffer = Convert.FromBase64String(base64);
            var stream = new MemoryStream(byteBuffer, 0, byteBuffer.Length);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            ImgPlaylist.Source = image;
            TxtName.Text = _playlist.PlaylistTitle;
            TxtAuthor.Text = _playlist.PlaylistAuthor;
            TxtDesc.Text = _playlist.PlaylistDescription;

            LoadSongs();
        }

        private void LoadSongs()
        {
            Songs.Clear();

            if (_playlist?.Songs == null || (!_playlist?.Songs.Any() ?? true))
                return;

            var allSongs = _playlist.Songs.Select(x =>
                SongData.LocalSongs.FirstOrDefault(z =>
                    z.Hash.Equals(x.Hash, StringComparison.InvariantCultureIgnoreCase))).Where(x => x != null).ToList();

            var filteredSongs = allSongs;

            if (!string.IsNullOrEmpty(Filter.SearchQuery))
            {
                filteredSongs = filteredSongs.Where(x => x.SongName.Contains(Filter.SearchQuery, StringComparison.InvariantCultureIgnoreCase)).ToList();
                filteredSongs = filteredSongs.Concat(allSongs.Where(x => x.Artist.Contains(Filter.SearchQuery, StringComparison.InvariantCultureIgnoreCase))).ToList();
                filteredSongs = filteredSongs.Concat(allSongs.Where(x => x.Mapper.Contains(Filter.SearchQuery, StringComparison.InvariantCultureIgnoreCase))).ToList();
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
                    var songInfoPanel = new PlaylistSongRowTile(song, _playlist, UpdateList);
                    Songs.Add(songInfoPanel);
                }

                TxtCurrentPage.Text = $"Page {CurrentPageNum} / {MaxPageNum}";

                var lowerBound = ((NumOnPage * CurrentPageNum) - NumOnPage) + 1;
                var upperBound = new[] { NumOnPage * CurrentPageNum, numSongs }.Min();
                TxtCurrentCount.Text = $"({lowerBound} to {upperBound}) out of {numSongs}";

                // ProgressBar.Visibility = Visibility.Collapsed; //TODO: Add loading spinner to song grid.
                // PageButtons.Visibility = Visibility.Visible;
                // GridSongs.Visibility = Visibility.Visible;

                this.OnPropertyChanged("HasPreviousPage");
                this.OnPropertyChanged("HasNextPage");
            });
        }

        public void UpdateList()
        {
            LoadSongs();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            byte[] bytes = null;
            var encoder = new PngBitmapEncoder();

            if (ImgPlaylist.Source is BitmapSource bitmapSource)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using var stream = new MemoryStream();
                encoder.Save(stream);
                bytes = stream.ToArray();
            }

            if (bytes == null)
            {
                MainWindow.ShowNotification("Failed to save playlist (Failed to encode image).", NotificationSeverityEnum.Error);
                return;
            }

            var base64Image = $"data:image/png;base64,{Convert.ToBase64String(bytes)}";

            var imageString = base64Image != _playlist?.Image ? base64Image : _playlist.Image;

            var titleString = "";
            if (!string.IsNullOrEmpty(TxtName.Text) && TxtName.Text != _playlist?.PlaylistTitle)
            {
                titleString = TxtName.Text;
            }
            else
            {
                titleString = string.IsNullOrEmpty(_playlist?.PlaylistTitle) ? $"New Playlist - {DateTime.UtcNow}" : _playlist.PlaylistTitle;
            }

            var authorString = "";
            if (!string.IsNullOrEmpty(TxtAuthor.Text) && TxtAuthor.Text != _playlist?.PlaylistAuthor)
            {
                authorString = TxtAuthor.Text;
            }
            else
            {
                if (!string.IsNullOrEmpty(_playlist?.PlaylistAuthor))
                {
                    authorString = _playlist.PlaylistTitle;
                }
            }

            var descString = "";
            if (!string.IsNullOrEmpty(TxtDesc.Text) && TxtDesc.Text != _playlist?.PlaylistDescription)
            {
                descString = TxtDesc.Text;
            }
            else
            {
                if (!string.IsNullOrEmpty(_playlist?.PlaylistDescription))
                {
                    descString = _playlist.PlaylistDescription;
                }
            }
            
            var editedPlaylist = new Playlist
            {
                Image = imageString,
                PlaylistTitle = titleString.Trim(),
                PlaylistAuthor = authorString.Trim(),
                PlaylistDescription = descString.Trim(),
                Songs = _playlist?.Songs ?? new List<Playlist.Song>()
            };

            if (!string.IsNullOrEmpty(_playlist?.FullPath))
            {
                File.Delete(_playlist.FullPath);
                var playlistToRemove = SongData.Playlists.FirstOrDefault(x => x.FullPath.Equals(_playlist.FullPath));
                if (playlistToRemove != null)
                    SongData.Playlists.Remove(playlistToRemove);
            }

            var saveLoc = _config.BeatSaberLocation + "/Playlists";
            if (!Directory.Exists(saveLoc))
                Directory.CreateDirectory(saveLoc);

            editedPlaylist.FullPath = Regex.Replace($"{saveLoc}/{editedPlaylist.PlaylistTitle}.json", @"\r\n?|\n", " ");

            File.WriteAllText(editedPlaylist.FullPath, JsonConvert.SerializeObject(editedPlaylist));
            SongData.Playlists.Add(editedPlaylist);

            TxtName.Text = TxtName.Text.Trim();
            TxtAuthor.Text = TxtAuthor.Text.Trim();
            TxtDesc.Text = TxtDesc.Text.Trim();

            MainWindow.ShowNotification("Playlist saved successfully.", NotificationSeverityEnum.Success);
        }

        private void ImgPlaylist_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ImgPlaylistOverlayColor.Visibility = Visibility.Visible;
            ImgPlaylistOverlayIcon.Visibility = Visibility.Visible;
        }

        private void ImgPlaylist_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ImgPlaylistOverlayColor.Visibility = Visibility.Hidden;
            ImgPlaylistOverlayIcon.Visibility = Visibility.Hidden;
        }

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            Filter.SearchQuery = TxtSearch.Text;

            Songs.Clear();

            LoadSongs();
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
