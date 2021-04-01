using BeatManager_WPF_.Models.BeatSaverAPI;

namespace BeatManager_WPF_.Models.SongFilterModels
{
    public class OnlineSongsFilter
    {
        public string SearchQuery { get; set; } = null;
        // public DifficultiesEnum? Difficulty { get; set; } = null;
        // public Range? BpmRange { get; set; } = null;
        public SortFilter Sort { get; set; } = new SortFilter();

        public class SortFilter
        {
            public MapsSortOption Option { get; set; } = MapsSortOption.Hot;
        }
    }
}