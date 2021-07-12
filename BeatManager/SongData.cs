﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BeatManager.Enums;
using BeatManager.Models;
using BeatManager.ViewModels;
using Newtonsoft.Json;
using Sentry;

namespace BeatManager
{
    public static class SongData
    {
        public static List<Playlist> Playlists = new List<Playlist>();
        public static List<LocalSongInfoViewModel> LocalSongs = new List<LocalSongInfoViewModel>();

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

        public static bool DeleteSong(LocalSongInfoViewModel song)
        {
            try
            {
                var songDirectory = song.FullSongDir;

                var infoFilePath = Directory.GetFiles(songDirectory).FirstOrDefault(x =>
                    x.EndsWith("info.dat", StringComparison.InvariantCultureIgnoreCase));
                var stringToHash = File.ReadAllText(infoFilePath);
                var songInfo = JsonConvert.DeserializeObject<SongInfo>(stringToHash);
                foreach (var diffSet in songInfo.DifficultyBeatmapSets)
                {
                    foreach (var diff in diffSet.DifficultyBeatmaps)
                    {
                        var diffPath = $"{songDirectory}/{diff.BeatmapFilename}";
                        stringToHash += File.ReadAllText(diffPath);
                    }
                }

                var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
                var hashString = string.Concat(hash.Select(x => x.ToString("x2")));

                for (var i = Playlists.Count; i-- > 0;) // Remove song from all playlists regardless of if it's downloaded. //TODO: Should probably be an option on a settings page.
                {
                    var playlist = Playlists[i];
                    var allSongsInPlaylist = playlist.Songs.Select(x => x.Hash).ToList();
                    if (allSongsInPlaylist.Contains(hashString))
                    {
                        allSongsInPlaylist.Remove(hashString);
                        playlist.Songs = allSongsInPlaylist.Select(x => new Playlist.Song {Hash = x}).ToList();
                    }

                    Playlists[i] = playlist;
                    File.WriteAllText(playlist.FullPath, JsonConvert.SerializeObject(playlist));
                }

                LocalSongs.Remove(song);

                Directory.Delete(songDirectory, true);

                MainWindow.ShowNotification("Song successfully deleted.", NotificationSeverityEnum.Success);

                return true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowNotification("Song failed to be deleted.", NotificationSeverityEnum.Error);
                SentrySdk.CaptureException(ex);
            }

            return false;
        }

        public static bool DeletePlaylist(Playlist playlist)
        {
            var fullPath = playlist.FullPath;
            var existing = Playlists.FirstOrDefault(x => x.FullPath.Equals(fullPath, StringComparison.InvariantCultureIgnoreCase));
            if (existing == null)
            {
                MainWindow.ShowNotification("Could not match the full path of the playlist.", NotificationSeverityEnum.Error);
                return false;
            }

            Playlists.Remove(existing);
            File.Delete(fullPath);
            MainWindow.ShowNotification("Playlist successfully deleted!", NotificationSeverityEnum.Success);
            return true;
        }
    }
}