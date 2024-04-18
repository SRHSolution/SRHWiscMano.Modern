using System;
using System.IO;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
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
using SRHWiscMano.Core.Helpers;
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
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            SRHWiscMano.Core.ServiceRegistration.ConfigureServices(services);
            SRHWiscMano.App.ServiceRegistration.ConfigureServices(services);

            // SRHWiscMano.App Assembly 에 포함된 Singleton attribute를 갖는 모든 클래스를 검색하여 등록한다.
            services.AddSingletonTypes("SRHWiscMano.App");

            // Service Provider 를 build 한다.
            serviceProvider = services.BuildServiceProvider();

            // Service Provider를 Ioc 에 등록한다.
            Ioc.Default.ConfigureServices(serviceProvider);

            // Logging 메시지를 back단에서 계속 받기 위해서 Instance를 미리 생성함
            Ioc.Default.GetRequiredService<LoggerWindow>();

            // MainWindow 와 관련된 ViewModel 초기화를 미리 하기 위함
            Ioc.Default.GetService<MainWindowViewModel>();
            Ioc.Default.GetService<IViewerViewModel>();

            LogManager.GetCurrentClassLogger().Info("Application Started");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            LoadAppSettings();
        }

        private void LoadAppSettings()
        {
            var settings = Ioc.Default.GetRequiredService<IOptions<AppSettings>>().Value;
             
            ThemeManager.Current.ChangeThemeBaseColor(Application.Current, settings.BaseTheme!);
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, settings.AccentTheme!);

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                logger.Error(ex);
            }
        }


        protected override void OnExit(ExitEventArgs exitEventArgs)
        {
            var settingViewModel = Ioc.Default.GetRequiredService<ISettingViewModel>();
            settingViewModel.UpdateSettingsCommand.Execute(null);

            LogManager.Shutdown();
        }
    }
}
