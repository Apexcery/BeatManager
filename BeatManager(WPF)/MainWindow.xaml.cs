using System;
using System.Diagnostics;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace BeatManager_WPF_
{
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();

            LoadCustomSongs();
            LoadPlaylists();

            this.Loaded += showNotifs;
        }

        private void LoadPlaylists()
        {
           
        }

        private void LoadCustomSongs()
        {
           
        }

        private void showNotifs(object sender, RoutedEventArgs e)
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

            notifier.ShowSuccess("400 custom songs found.", new MessageOptions { ShowCloseButton = true });
            notifier.ShowSuccess("7 playlists found.", new MessageOptions { ShowCloseButton = true });
            notifier.ShowSuccess("7 playlists found.", new MessageOptions { ShowCloseButton = true });
            notifier.ShowSuccess("7 playlists found.", new MessageOptions { ShowCloseButton = true });
            notifier.ShowSuccess("7 playlists found.", new MessageOptions { ShowCloseButton = true });
            notifier.ShowSuccess("7 playlists found.", new MessageOptions { ShowCloseButton = true });
        }

        private void BtnOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            BtnCloseMenu.Visibility = Visibility.Visible;
            BtnOpenMenu.Visibility = Visibility.Collapsed;
            
        }

        private void BtnCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            BtnCloseMenu.Visibility = Visibility.Collapsed;
            BtnOpenMenu.Visibility = Visibility.Visible;
            
        }

        private void BtnPopUpExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TopBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void BtnSongs_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Trace.WriteLine("FUCK YOU");
            BtnSongs.BorderThickness = new Thickness(0, 0, 2, 0);
        }
    }
}
