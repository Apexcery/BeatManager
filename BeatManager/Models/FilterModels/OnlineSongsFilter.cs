using BeatManager.Models.BeatSaverAPI;

namespace BeatManager.Models.FilterModels
{
    public class OnlineSongsFilter
    {
        public string SearchQuery { get; set; } = "";
        public SortFilter Sort { get; set; } = new SortFilter();

        public class SortFilter
        {
            public MapsSortOption Option { get; set; } = MapsSortOption.Hot;
        }
    }
}