using System;
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

            this.Loaded += showNotif;
        }

        private void showNotif(object sender, RoutedEventArgs e)
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
    }
}
