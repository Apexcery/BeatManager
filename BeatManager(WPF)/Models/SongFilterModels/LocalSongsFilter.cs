using System;
using BeatManager_WPF_.Enums;

namespace BeatManager_WPF_.Models.SongFilterModels
{
    public class LocalSongsFilter
    {
        public string SearchQuery { get; set; } = null;
        public DifficultiesEnum? Difficulty { get; set; } = null;
        public Range? BpmRange { get; set; } = null;
        public SortFilter Sort { get; set; } = new SortFilter // Initialize with the default sort order.
        {
            Option = SortFilter.SortOptions.Name,
            Direction = SortFilter.SortDirection.Ascending
        };

        public class SortFilter
        {
            public SortDirection? Direction { get; set; } = null;
            public SortOptions? Option { get; set; } = null;

            public enum SortDirection
            {
                Ascending,
                Descending
            }

            public enum SortOptions
            {
                Name,
                Artist,
                Difficulty,
                BPM,
                Date
            }
        }
    }
}
