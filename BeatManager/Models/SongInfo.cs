using System.Collections.Generic;
using Newtonsoft.Json;

namespace BeatManager.Models
{
    public class SongInfo
    {
        [JsonProperty("_version")]
        public string Version { get; set; }

        [JsonProperty("_songName")]
        public string SongName { get; set; }

        [JsonProperty("_songSubName")]
        public string SongSubName { get; set; }

        [JsonProperty("_songAuthorName")]
        public string SongAuthorName { get; set; }

        [JsonProperty("_levelAuthorName")]
        public string LevelAuthorName { get; set; }

        [JsonProperty("_beatsPerMinute")]
        public double BeatsPerMinute { get; set; }

        [JsonProperty("_songTimeOffset")]
        public int SongTimeOffset { get; set; }

        [JsonProperty("_shuffle")]
        public int Shuffle { get; set; }

        [JsonProperty("_shufflePeriod")]
        public double ShufflePeriod { get; set; }

        [JsonProperty("_previewStartTime")]
        public double PreviewStartTime { get; set; }

        [JsonProperty("_previewDuration")]
        public double PreviewDuration { get; set; }

        [JsonProperty("_songFilename")]
        public string SongFilename { get; set; }

        [JsonProperty("_coverImageFilename")]
        public string CoverImageFilename { get; set; }

        [JsonProperty("_environmentName")]
        public string EnvironmentName { get; set; }

        [JsonProperty("_customData")]
        public CustomDataObject CustomData { get; set; }

        [JsonProperty("_difficultyBeatmapSets")]
        public List<DifficultyBeatmapSet> DifficultyBeatmapSets { get; set; }

        public class CustomDataObject
        {
            [JsonProperty("_contributors")]
            public List<object> Contributors { get; set; }

            [JsonProperty("_customEnvironment")]
            public string CustomEnvironment { get; set; }

            [JsonProperty("_customEnvironmentHash")]
            public string CustomEnvironmentHash { get; set; }

            [JsonProperty("_difficultyLabel")]
            public string DifficultyLabel { get; set; }

            [JsonProperty("_editorOffset")]
            public int EditorOffset { get; set; }

            [JsonProperty("_editorOldOffset")]
            public int EditorOldOffset { get; set; }

            [JsonProperty("_warnings")]
            public List<object> Warnings { get; set; }

            [JsonProperty("_information")]
            public List<object> Information { get; set; }

            [JsonProperty("_suggestions")]
            public List<object> Suggestions { get; set; }

            [JsonProperty("_requirements")]
            public List<object> Requirements { get; set; }
        }

        public class DifficultyBeatmap
        {
            [JsonProperty("_difficulty")]
            public string Difficulty { get; set; }

            [JsonProperty("_difficultyRank")]
            public int DifficultyRank { get; set; }

            [JsonProperty("_beatmapFilename")]
            public string BeatmapFilename { get; set; }

            [JsonProperty("_noteJumpMovementSpeed")]
            public double NoteJumpMovementSpeed { get; set; }

            [JsonProperty("_noteJumpStartBeatOffset")]
            public double NoteJumpStartBeatOffset { get; set; }

            [JsonProperty("_customData")]
            public CustomDataObject CustomData { get; set; }
        }

        public class DifficultyBeatmapSet
        {
            [JsonProperty("_beatmapCharacteristicName")]
            public string BeatmapCharacteristicName { get; set; }

            [JsonProperty("_difficultyBeatmaps")]
            public List<DifficultyBeatmap> DifficultyBeatmaps { get; set; }
        }
    }
}
