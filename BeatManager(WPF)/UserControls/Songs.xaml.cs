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
using Newtonsoft.Json;

namespace BeatManager_WPF_.UserControls
{
    public partial class Songs : UserControl
    {
        private readonly Config _config;
        public ObservableCollection<Card> Items { get; set; } = new ObservableCollection<Card>();

        public Songs(Config config)
        {
            _config = config;

            InitializeComponent();

            LoadSongs();
        }

        private async void LoadSongs(string searchQuery = null)
        {
            var rootDir = _config.BeatSaberLocation;

            var songDirs = Directory.GetDirectories($"{rootDir}/Beat Saber_Data/CustomLevels");

            var allLocalSongs = new List<SongInfoViewModel>();
            var filteredSongs = new List<SongInfoViewModel>();

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
                filteredSongs = allLocalSongs.OrderBy(x => x.SongName).Take(25).ToList();
            }
            else
            {
                filteredSongs = allLocalSongs.Where(x => x.SongName.ToLower().Contains(searchQuery.ToLower())).ToList();
                filteredSongs = filteredSongs.Concat(allLocalSongs.Where(x => x.Artist.ToLower().Contains(searchQuery.ToLower()))).ToList();
                filteredSongs = filteredSongs.Concat(allLocalSongs.Where(x => x.Mapper.ToLower().Contains(searchQuery.ToLower()))).ToList();

                filteredSongs = filteredSongs
                    .Take(25)
                    .ToList();
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (var song in filteredSongs)
                {
                    var songInfoPanel = GenerateSongInfoPanel(song);
                    Items.Add(songInfoPanel);
                }

                LocalSongsProgressBar.Visibility = Visibility.Hidden;
                GridLocalSongs.Visibility = Visibility.Visible;
            });
        }

        private Card GenerateSongInfoPanel(SongInfoViewModel song)
        {
            var card = new Card
            {
                UniformCornerRadius = 0,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 2,
                    ShadowDepth = 2,
                    Color = Colors.DimGray
                },
                Width = 150,
                Height = 150,
                Content = new Image
                {
                    Source = new BitmapImage(new Uri(song.FullImagePath))
                },
                ToolTip = song.SongName
            };
            
            return card;
        }
    }
}
