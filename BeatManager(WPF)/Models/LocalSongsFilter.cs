using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatManager_WPF_.Models
{
    public class LocalSongsFilter
    {
        public string SearchQuery { get; set; } = null;
        public DifficultyFilter? Difficulty { get; set; } = null;
        public Range? BpmRange { get; set; } = null;
        public SortFilter Sort { get; set; } = new SortFilter // Initialize with the default sort order.
        {
            Option = SortFilter.SortOptions.Name,
            Direction = SortFilter.SortDirection.Ascending
        };

        public enum DifficultyFilter
        {
            Easy,
            Normal,
            Hard,
            Expert,
            ExpertPlus
        }

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
