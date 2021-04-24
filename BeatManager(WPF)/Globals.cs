using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BeatManager_WPF_.Enums;
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

        public static bool RemoveSongFromPlaylist(Playlist playlist, string songHash)
        {
            var fullPath = playlist.FullPath;
            var existing = Playlists.FirstOrDefault(x => x.FullPath.Equals(fullPath, StringComparison.InvariantCultureIgnoreCase));
            if (existing == null)
            {
                MainWindow.ShowNotification("Could not match the full path of the playlist.", NotificationSeverityEnum.Error);
                return false;
            }
            var songToRemove = existing.Songs.FirstOrDefault(x => x.Hash.Equals(songHash, StringComparison.InvariantCulture));
            if (songToRemove == null)
            {
                MainWindow.ShowNotification("Could not find song in playlist.", NotificationSeverityEnum.Error);
                return false;
            }

            existing.Songs.Remove(songToRemove);
            playlist.Songs.Remove(songToRemove);
            File.WriteAllText(existing.FullPath, JsonConvert.SerializeObject(existing));
            MainWindow.ShowNotification("Song removed from playlist.", NotificationSeverityEnum.Success);
            return true;
        }

        public static bool AddSongToPlaylist(Playlist playlist, string songHash)
        {
            var fullPath = playlist.FullPath;
            var existing = Playlists.FirstOrDefault(x => x.FullPath.Equals(fullPath, StringComparison.InvariantCultureIgnoreCase));
            if (existing == null)
            {
                MainWindow.ShowNotification("Could not match the full path of the playlist.", NotificationSeverityEnum.Error);
                return false;
            }
            var songExists = existing.Songs.FirstOrDefault(x => x.Hash.Equals(songHash, StringComparison.InvariantCulture)) != null;
            if (songExists)
            {
                MainWindow.ShowNotification("Song already exists in playlist.", NotificationSeverityEnum.Error);
                return false;
            }

            existing.Songs.Add(new Playlist.Song{Hash = songHash});
            File.WriteAllText(existing.FullPath, JsonConvert.SerializeObject(existing));
            MainWindow.ShowNotification("Song added to playlist.", NotificationSeverityEnum.Success);
            return true;
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
