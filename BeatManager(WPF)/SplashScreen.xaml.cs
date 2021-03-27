using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using BeatManager_WPF_.Models;
using MessageBox = System.Windows.MessageBox;

namespace BeatManager_WPF_
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        DispatcherTimer dt = new DispatcherTimer();
        public SplashScreen(Config config)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(config.BeatSaberLocation))
            {
                dt.Tick += new EventHandler(ChangeWindow);
                dt.Interval = new TimeSpan(0, 0, 3);
                dt.Start();
            }
            else
            {
                DirectoryPanel.Visibility = Visibility.Visible;
            }
        }

        private void ChangeWindow(object sender, EventArgs e)
        {
            MainWindow main = new MainWindow();
            App.Current.MainWindow = main;
            this.Close();
            main.Show();
            dt.Stop();
        }

        private void BtnBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer,
                Description = "Select your Beat Saber root directory.",
                ShowNewFolderButton = false,
                UseDescriptionForTitle = true
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                var selectedPath = dialog.SelectedPath;

                var exeFound = Directory.GetFiles(selectedPath).FirstOrDefault(x => x.EndsWith("Beat Saber.exe")) != null;
                if (!exeFound)
                {
                    MessageBox.Show("Please check your selected directory.", "Invalid Directory");
                    return;
                }

                var directories = Directory.GetDirectories(selectedPath);
                
                if (!Directory.Exists(selectedPath + "\\Beat Saber_Data\\CustomLevels"))
                {
                    MessageBox.Show("Custom Levels directory not found.", "Invalid Directory");
                    return;
                }
                if (!Directory.Exists(selectedPath + "\\Playlists"))
                {
                    MessageBox.Show("Custom Levels directory not found.", "Invalid Directory");
                    return;
                }

                TxtRootDirectory.Text = selectedPath;

                BtnSave.IsEnabled = true;
            }
        }
    }
}
