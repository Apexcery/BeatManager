using System;
using System.Windows;
using System.Windows.Threading;
using BeatManager_WPF_.Models;

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
            Application.Current.MainWindow = main;
            this.Close();
            main.Show();
            dt.Stop();
        }
    }
}
