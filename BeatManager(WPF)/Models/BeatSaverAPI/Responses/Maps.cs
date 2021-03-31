using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BeatManager_WPF_.Models.BeatSaverAPI.Responses
{
    public class Maps
    {
        [JsonProperty("docs")]
        public List<Doc> Songs { get; set; }

        [JsonProperty("totalDocs")]
        public int TotalSongs { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }

        [JsonProperty("prevPage")]
        public int? PrevPage { get; set; }

        [JsonProperty("nextPage")]
        public int? NextPage { get; set; }

        public class Difficulties
        {
            [JsonProperty("easy")]
            public bool Easy { get; set; }

            [JsonProperty("normal")]
            public bool Normal { get; set; }

            [JsonProperty("hard")]
            public bool Hard { get; set; }

            [JsonProperty("expert")]
            public bool Expert { get; set; }

            [JsonProperty("expertPlus")]
            public bool ExpertPlus { get; set; }
        }

        public class CharacteristicDifficulties
        {
            [JsonProperty("easy")]
            public Easy Easy { get; set; }

            [JsonProperty("normal")]
            public Normal Normal { get; set; }

            [JsonProperty("hard")]
            public Hard Hard { get; set; }

            [JsonProperty("expert")]
            public Expert Expert { get; set; }

            [JsonProperty("expertPlus")]
            public ExpertPlus ExpertPlus { get; set; }
        }

        public class Easy
        {
            [JsonProperty("duration")]
            public double Duration { get; set; }

            [JsonProperty("length")]
            public int Length { get; set; }

            [JsonProperty("njs")]
            public double Njs { get; set; }

            [JsonProperty("njsOffset")]
            public double NjsOffset { get; set; }

            [JsonProperty("bombs")]
            public int Bombs { get; set; }

            [JsonProperty("notes")]
            public int Notes { get; set; }

            [JsonProperty("obstacles")]
            public int Obstacles { get; set; }
        }

        public class Normal
        {
            [JsonProperty("duration")]
            public double Duration { get; set; }

            [JsonProperty("length")]
            public int Length { get; set; }

            [JsonProperty("bombs")]
            public int Bombs { get; set; }

            [JsonProperty("notes")]
            public int Notes { get; set; }

            [JsonProperty("obstacles")]
            public int Obstacles { get; set; }

            [JsonProperty("njs")]
            public double Njs { get; set; }

            [JsonProperty("njsOffset")]
            public double NjsOffset { get; set; }
        }

        public class Hard
        {
            [JsonProperty("duration")]
            public double Duration { get; set; }

            [JsonProperty("length")]
            public int Length { get; set; }

            [JsonProperty("bombs")]
            public int Bombs { get; set; }

            [JsonProperty("notes")]
            public int Notes { get; set; }

            [JsonProperty("obstacles")]
            public int Obstacles { get; set; }

            [JsonProperty("njs")]
            public double Njs { get; set; }

            [JsonProperty("njsOffset")]
            public double NjsOffset { get; set; }
        }

        public class Expert
        {
            [JsonProperty("duration")]
            public double Duration { get; set; }

            [JsonProperty("length")]
            public int Length { get; set; }

            [JsonProperty("bombs")]
            public int Bombs { get; set; }

            [JsonProperty("notes")]
            public int Notes { get; set; }

            [JsonProperty("obstacles")]
            public int Obstacles { get; set; }

            [JsonProperty("njs")]
            public double Njs { get; set; }

            [JsonProperty("njsOffset")]
            public double NjsOffset { get; set; }
        }

        public class ExpertPlus
        {
            [JsonProperty("duration")]
            public double Duration { get; set; }

            [JsonProperty("length")]
            public int Length { get; set; }

            [JsonProperty("bombs")]
            public int Bombs { get; set; }

            [JsonProperty("notes")]
            public int Notes { get; set; }

            [JsonProperty("obstacles")]
            public int Obstacles { get; set; }

            [JsonProperty("njs")]
            public double Njs { get; set; }

            [JsonProperty("njsOffset")]
            public double NjsOffset { get; set; }
        }

        public class Characteristic
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("difficulties")]
            public CharacteristicDifficulties Difficulties { get; set; }
        }

        public class Metadata
        {
            [JsonProperty("difficulties")]
            public Difficulties Difficulties { get; set; }

            [JsonProperty("duration")]
            public int Duration { get; set; }

            [JsonProperty("automapper")]
            public object Automapper { get; set; }

            [JsonProperty("characteristics")]
            public List<Characteristic> Characteristics { get; set; }

            [JsonProperty("songName")]
            public string SongName { get; set; }

            [JsonProperty("songSubName")]
            public string SongSubName { get; set; }

            [JsonProperty("songAuthorName")]
            public string SongAuthorName { get; set; }

            [JsonProperty("levelAuthorName")]
            public string LevelAuthorName { get; set; }

            [JsonProperty("bpm")]
            public double Bpm { get; set; }
        }

        public class Stats
        {
            [JsonProperty("downloads")]
            public int Downloads { get; set; }

            [JsonProperty("plays")]
            public int Plays { get; set; }

            [JsonProperty("downVotes")]
            public int DownVotes { get; set; }

            [JsonProperty("upVotes")]
            public int UpVotes { get; set; }

            [JsonProperty("heat")]
            public double Heat { get; set; }

            [JsonProperty("rating")]
            public double Rating { get; set; }
        }

        public class Uploader
        {
            [JsonProperty("_id")]
            public string Id { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }
        }

        public class Doc
        {
            [JsonProperty("metadata")]
            public Metadata Metadata { get; set; }

            [JsonProperty("stats")]
            public Stats Stats { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("deletedAt")]
            public object DeletedAt { get; set; }

            [JsonProperty("_id")]
            public string Id { get; set; }

            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("uploader")]
            public Uploader Uploader { get; set; }

            [JsonProperty("uploaded")]
            public DateTime Uploaded { get; set; }

            [JsonProperty("hash")]
            public string Hash { get; set; }

            [JsonProperty("directDownload")]
            public string DirectDownload { get; set; }

            [JsonProperty("downloadURL")]
            public string DownloadURL { get; set; }

            [JsonProperty("coverURL")]
            public string CoverURL { get; set; }
        }
    }
}
