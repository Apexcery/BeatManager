using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BeatManager_WPF_
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        DispatcherTimer dt = new DispatcherTimer();
        public SplashScreen()
        {
            InitializeComponent();
            dt.Tick += new EventHandler(ChangeWindow);
            dt.Interval = new TimeSpan(0, 0, 3);
            dt.Start();
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
