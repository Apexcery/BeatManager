using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using BeatManager_WPF_.Models;
using Newtonsoft.Json;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = System.Windows.MessageBox;

namespace BeatManager_WPF_
{
    public partial class SplashScreen : Window
    {
        private readonly Config _config;
        readonly DispatcherTimer _dispatchTimer = new DispatcherTimer();

        public SplashScreen(Config config)
        {
            _config = config;
            InitializeComponent();
            DirectoryPanel.Visibility = Visibility.Hidden;
            
            

            if (!string.IsNullOrEmpty(_config.BeatSaberLocation))
            {
                StartChangeWindowTimer(3);

            }
            else
            {
                DirectoryPanel.Visibility = Visibility.Visible;
            }
        }

        private void StartChangeWindowTimer(int seconds, string notifMessage = null)
        {
            _dispatchTimer.Tick += new EventHandler((o, args) => ChangeWindow(o, args, notifMessage));
            _dispatchTimer.Interval = new TimeSpan(0, 0, seconds);
            _dispatchTimer.Start();
        }

        private void ChangeWindow(object sender, EventArgs e, string notifMessage = null)
        {
            var main = new MainWindow(notifMessage);
            Application.Current.MainWindow = main;
            this.Close();
            main.Show();
            _dispatchTimer.Stop();
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
                    DisableSaveButton();
                    return;
                }

                var directories = Directory.GetDirectories(selectedPath);
                
                if (!Directory.Exists(selectedPath + "\\Beat Saber_Data\\CustomLevels"))
                {
                    MessageBox.Show("Custom Levels directory not found.", "Invalid Directory");
                    DisableSaveButton();
                    return;
                }
                if (!Directory.Exists(selectedPath + "\\Playlists"))
                {
                    MessageBox.Show("Custom Levels directory not found.", "Invalid Directory");
                    DisableSaveButton();
                    return;
                }

                selectedPath = selectedPath.Replace(@"\", "/");
                TxtRootDirectory.Text = selectedPath;

                BtnSave.Cursor = Cursors.Hand;
                BtnSave.Tag = selectedPath;
                BtnSave.IsEnabled = true;
            }
        }

        private void DisableSaveButton()
        {
            BtnSave.Cursor = Cursors.AppStarting;
            BtnSave.IsEnabled = false;
            BtnSave.Tag = null;
            TxtRootDirectory.Text = "";
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;

            var beatSaberRootDir = (string) button.Tag;

            if (_config.BeatSaberLocation.Equals(beatSaberRootDir))
            {
                StartChangeWindowTimer(0);
                return;
            }

            _config.BeatSaberLocation = beatSaberRootDir;

            File.WriteAllText("./data/config.json", JsonConvert.SerializeObject(_config, Formatting.Indented));

            StartChangeWindowTimer(0, "Root directory saved successfully.");
        }
    }
}
