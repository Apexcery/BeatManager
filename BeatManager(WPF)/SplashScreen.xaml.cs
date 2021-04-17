using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using BeatManager_WPF_.Enums;
using BeatManager_WPF_.Interfaces;
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
        private readonly IBeatSaverAPI _beatSaverApi;

        readonly DispatcherTimer _dispatchTimer = new DispatcherTimer();

        public SplashScreen(Config config, IBeatSaverAPI beatSaverApi)
        {
            _config = config;
            _beatSaverApi = beatSaverApi;
            InitializeComponent();
            DirectoryPanel.Visibility = Visibility.Hidden;
            
            if (!string.IsNullOrEmpty(_config.BeatSaberLocation) && ValidateDirectory(_config.BeatSaberLocation))
            {
                StartChangeWindowTimer(3);

            }
            else
            {
                DirectoryPanel.Visibility = Visibility.Visible;
            }
        }

        private void StartChangeWindowTimer(int seconds, string notifMessage = null, NotificationSeverityEnum? severity = null)
        {
            _dispatchTimer.Tick += new EventHandler((o, args) => ChangeWindow(o, args, notifMessage, severity));
            _dispatchTimer.Interval = new TimeSpan(0, 0, seconds);
            _dispatchTimer.Start();
        }

        private void ChangeWindow(object sender, EventArgs e, string notifMessage = null, NotificationSeverityEnum? severity = null)
        {
            MainWindow main;
            if (notifMessage == null || severity == null)
                main = new MainWindow(_config, _beatSaverApi);
            else
                main = new MainWindow(_config, _beatSaverApi, notifMessage, (NotificationSeverityEnum) severity);

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

                if (!ValidateDirectory(selectedPath))
                    return;

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

            StartChangeWindowTimer(0, "Root directory saved successfully.", NotificationSeverityEnum.Success);
        }

        private bool ValidateDirectory(string directory)
        {
            var directoryExists = Directory.Exists(directory);
            if (!directoryExists)
            {
                MessageBox.Show("Saved directory not found.", "Invalid Directory");
                DisableSaveButton();
                return false;
            }

            var exeFound = Directory.GetFiles(directory).FirstOrDefault(x => x.EndsWith("Beat Saber.exe")) != null;
            if (!exeFound)
            {
                MessageBox.Show("Beat Saber exe file not found.", "Invalid Directory");
                DisableSaveButton();
                return false;
            }

            if (!Directory.Exists(directory + "\\Beat Saber_Data\\CustomLevels"))
            {
                MessageBox.Show("Custom Levels directory not found.", "Invalid Directory");
                DisableSaveButton();
                return false;
            }
            if (!Directory.Exists(directory + "\\Playlists"))
            {
                MessageBox.Show("Custom Levels directory not found.", "Invalid Directory");
                DisableSaveButton();
                return false;
            }

            return true;
        }
    }
}
