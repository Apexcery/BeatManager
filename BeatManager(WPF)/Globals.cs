using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.ViewModels;
using Newtonsoft.Json;

namespace BeatManager_WPF_
{
    public static class Globals
    {
        public static List<Playlist> Playlists = new List<Playlist>();
        public static List<LocalSongInfoViewModel> LocalSongs = new List<LocalSongInfoViewModel>();

        public static async Task LoadPlaylists(string rootDir)
        {
            var playlistFiles = Directory.GetFiles($"{rootDir}/Playlists");

            foreach (var playlistDir in playlistFiles)
            {
                var playlist = JsonConvert.DeserializeObject<Playlist>(await File.ReadAllTextAsync(playlistDir));

                if (playlist == null)
                    continue;

                playlist.FullPath = playlistDir;

                Playlists.Add(playlist);
            }
        }

        public static async Task LoadLocalSongs(string rootDir)
        {
            var songDirectories = Directory.GetDirectories($"{rootDir}/Beat Saber_Data/CustomLevels");

            foreach (var songDir in songDirectories)
            {
                var files = Directory.GetFiles(songDir);

                var infoFilePath = files.FirstOrDefault(x =>
                    x.EndsWith("info.dat", StringComparison.InvariantCultureIgnoreCase));
                if (string.IsNullOrEmpty(infoFilePath))
                    continue;

                var songInfo = JsonConvert.DeserializeObject<SongInfo>(await File.ReadAllTextAsync(infoFilePath));
                if (songInfo == null)
                    continue;

                var stringToHash = await File.ReadAllTextAsync(infoFilePath);
                foreach (var diffSet in songInfo.DifficultyBeatmapSets)
                {
                    foreach (var diff in diffSet.DifficultyBeatmaps)
                    {
                        var diffPath = $"{songDir}/{diff.BeatmapFilename}";
                        stringToHash += await File.ReadAllTextAsync(diffPath);
                    }
                }
                var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
                var hashString = string.Concat(hash.Select(b => b.ToString("x2")));

                var songInfoViewModel = new LocalSongInfoViewModel
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
                    DateAcquired = File.GetCreationTimeUtc(infoFilePath),
                    Hash = hashString
                };

                LocalSongs.Add(songInfoViewModel);
            }
        }
    }
}
