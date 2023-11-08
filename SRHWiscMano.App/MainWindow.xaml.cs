using CommunityToolkit.Mvvm.DependencyInjection;
using MahApps.Metro.Controls;

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


    }
}
