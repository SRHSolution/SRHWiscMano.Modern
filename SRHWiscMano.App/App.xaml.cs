using System;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace SRHWiscMano.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            SRHWiscMano.Core.ServiceRegistration.ConfigureServices(services);
            SRHWiscMano.App.ServiceRegistration.ConfigureServices(services);

            // ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            Ioc.Default.ConfigureServices(ServiceProvider);

            // var ss = ServiceProvider.GetService<IViewerPage>();
            // var ss2 = ServiceProvider.GetService<IViewerPage>();
            // ViewModelLocator.Initialize(ServiceProvider);
        }
    }
}
