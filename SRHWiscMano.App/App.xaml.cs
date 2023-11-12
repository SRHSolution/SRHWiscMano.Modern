using System;
using System.IO;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.DependencyInjection;
using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using SRHWiscMano.App.Controls;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.ViewModels;
using SRHWiscMano.App.Windows;
using SRHWiscMano.Core.Services;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider? serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            SRHWiscMano.Core.ServiceRegistration.ConfigureServices(services);
            SRHWiscMano.App.ServiceRegistration.ConfigureServices(services);


            serviceProvider = services.BuildServiceProvider();

            Ioc.Default.ConfigureServices(serviceProvider);


            // Logging 메시지를 back단에서 계속 받기 위해서 Instance를 미리 생성함
            Ioc.Default.GetRequiredService<LoggerWindow>();

            LogManager.GetCurrentClassLogger().Info("Application Started");

            LoadAppSettings();
        }

        private void LoadAppSettings()
        {
            var settings = Ioc.Default.GetRequiredService<IOptions<AppSettings>>().Value;

            ThemeManager.Current.ChangeThemeBaseColor(Application.Current, settings.BaseTheme!);
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, settings.AccentTheme!);
        }


        protected override void OnExit(ExitEventArgs exitEventArgs)
        {
            var settingViewModel = Ioc.Default.GetRequiredService<ISettingViewModel>();
            settingViewModel.UpdateSettingsCommand.Execute(null);

            LogManager.Shutdown();
        }
    }
}
