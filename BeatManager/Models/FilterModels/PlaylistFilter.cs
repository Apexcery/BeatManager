namespace BeatManager.Models.FilterModels
{
    public class PlaylistFilter
    {
        public string SearchQuery { get; set; } = "";
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
                Author,
                NumSongs
            }
        }
    }
}
