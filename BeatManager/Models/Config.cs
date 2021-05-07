namespace BeatManager.Models
{
    public class Config
    {
        public string BeatSaberLocation { get; set; } = "";
        public string StartupPage { get; set; } = Page.Songs.ToString();

        public enum Page
        {
            Songs,
            Playlists
        }
    }
}
