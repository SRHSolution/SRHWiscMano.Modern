using CommunityToolkit.Mvvm.DependencyInjection;
using MahApps.Metro.Controls;
using SRHWiscMano.App.Windows;

namespace SRHWiscMano.App
{
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = Ioc.Default.GetService<MainWindowViewModel>();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var logger = Ioc.Default.GetRequiredService<LoggerWindow>();
            logger.AllowClose = true;
            logger.Close();
        }
    }
}
