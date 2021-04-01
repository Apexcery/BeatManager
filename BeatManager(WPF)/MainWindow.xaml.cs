using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using BeatManager_WPF_.Enums;
using BeatManager_WPF_.Interfaces;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.UserControls;
using Newtonsoft.Json;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace BeatManager_WPF_
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Config _config;
        private readonly IBeatSaverAPI _beatSaverApi;

        private bool _isReady;
        public bool IsReady
        {
            get => _isReady;
            set
            {
                _isReady = value;
                OnPropertyChanged("IsReady");
            }
        }

        private readonly List<Playlist> _playlists = new List<Playlist>();

        public MainWindow(Config config, IBeatSaverAPI beatSaverApi)
        {
            _config = config;
            _beatSaverApi = beatSaverApi;

            InitializeComponent();
            this.DataContext = this;

            Task.WhenAll(Task.Run(LoadPlaylists)).ContinueWith((t) =>
            {
                IsReady = true;
                InitializeStartupPage();
            });
        }

        private void LoadPlaylists()
        {
            var playlistFiles = Directory.GetFiles($"{_config.BeatSaberLocation}/Playlists");

            foreach (var playlistDir in playlistFiles)
            {
                var playlist = JsonConvert.DeserializeObject<Playlist>(File.ReadAllText(playlistDir));

                if (playlist == null)
                    continue;

                _playlists.Add(playlist);
            }
        }

        private void InitializeStartupPage()
        {
            var startupPage = _config.StartupPage;
            var parseSuccessfully = Enum.TryParse(startupPage, true, out Config.Page startupPageEnum);

            if (!parseSuccessfully)
                return;

            Application.Current.Dispatcher.Invoke(delegate
            {
                switch (startupPageEnum)
                {
                    case Config.Page.Songs:
                        BtnSongs.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                        {
                            RoutedEvent = Mouse.MouseUpEvent,
                            Source = this,
                        });
                        break;
                    case Config.Page.Playlists:
                        BtnPlaylists.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                        {
                            RoutedEvent = Mouse.MouseUpEvent,
                            Source = this,
                        });
                        break;
                }
            });
        }

        public MainWindow(Config config, IBeatSaverAPI beatSaverApi, string notifMessage, NotificationSeverityEnum severity) : this(config, beatSaverApi)
        {
            if (!string.IsNullOrEmpty(notifMessage))
            {
                this.Loaded += (o, args) => ShowNotification(o, args, notifMessage, severity);
            }
        }

        private void ShowNotification(object sender, RoutedEventArgs e, string message, NotificationSeverityEnum severity)
        {
            var notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    Application.Current.MainWindow,
                    Corner.BottomRight,
                    10,
                    10);
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(5), MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });

            notifier.ShowSuccess(message, new MessageOptions { ShowCloseButton = true });
        }

        private void BtnOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            BtnCloseMenu.Visibility = Visibility.Visible;
            BtnOpenMenu.Visibility = Visibility.Collapsed;
            
        }

        private void BtnCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            if (BtnCloseMenu.Visibility != Visibility.Collapsed && BtnOpenMenu.Visibility != Visibility.Visible)
            {
                BtnCloseMenu.Visibility = Visibility.Collapsed;
                BtnOpenMenu.Visibility = Visibility.Visible;
            }
        }

        private void WindowContent_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (BtnCloseMenu.Visibility != Visibility.Collapsed && BtnOpenMenu.Visibility != Visibility.Visible)
            {
                BtnCloseMenu.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        private void BtnPopUpExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #region Nav Buttons

        private void AddBorderToButton(ListViewItem btn)
        {
            var listOfButtons = NavButtonList;
            foreach (var item in listOfButtons.Items)
            {
                if (item.GetType() == typeof(ListViewItem) && ((ListViewItem)item).Name.StartsWith("Btn"))
                {
                    var button = (ListViewItem)item;
                    button.BorderThickness = new Thickness(0, 0, 0, 0);
                }
            }

            btn.BorderThickness = new Thickness(0, 0, 2, 0);
        }

        private void ClearWindowContent()
        {
            WindowContent.Children.Clear();
        }

        private void BtnSongs_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            AddBorderToButton((ListViewItem) sender);
            ClearWindowContent();
            var songsControl = new Songs(_config, _beatSaverApi, _playlists)
            {
                Width = this.WindowContent.Width,
                Height = this.WindowContent.Height
            };
            WindowContent.Children.Add(songsControl);
        }

        private void BtnPlaylists_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            AddBorderToButton((ListViewItem)sender);
            ClearWindowContent();
            var playlistsControl = new Playlists(_config, _beatSaverApi, _playlists)
            {
                Width = this.WindowContent.Width,
                Height = this.WindowContent.Height
            };
            WindowContent.Children.Add(playlistsControl);
        }

        private void BtnAvatars_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnSabers_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnNotes_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnPlatforms_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnMods_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
