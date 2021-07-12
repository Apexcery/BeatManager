using System;
using System.Windows;
using System.Windows.Controls;
using BeatManager.ViewModels;

namespace BeatManager.UserControls.Songs
{
    public partial class SongDetails : UserControl
    {
        public SongDetails(SongDetailsViewModel songInfo)
        {
            InitializeComponent();

            SongName.Text = $"Name: {songInfo.SongName}";
            SongArtist.Text = $"Artist: {songInfo.Artist}";
            SongMapper.Text = $"Mapper: {songInfo.Mapper}";

            SongDesc.Text = songInfo.Description;
            SongDownloads.Text = $"{songInfo.Downloads} Downloads";
            SongUpvotes.Text = $"{songInfo.Upvotes} Upvotes";
            SongDownvotes.Text = $"{songInfo.Downvotes} Downvotes";

            //TODO: Ensure full image url stored in SongDetailsViewModel is correct and can be used to display in the image control.
            //TODO: Display image.
            //TODO: Ensure download url being stored in SongDetailsViewModel is correct (think it has to be directdownload rather than download url?).
            //TODO: Create play button image to be used in the play preview image button.
            //TODO: Display closeable webview? which defaults to the preview for the current song (might need key rather than hash? in which case just add to the view model).
        }

        private void SongPlayPreview_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show("Preview not implemented yet.");
        }
    }
}
