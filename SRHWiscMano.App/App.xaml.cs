﻿using System;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Targets;
using SRHWiscMano.App.Controls;
using SRHWiscMano.App.Windows;

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

            // Logging 메시지를 back단에서 계속 받기 위해서 Instance를 미리 생성함
            Ioc.Default.GetRequiredService<LoggerWindow>();

            LogManager.GetCurrentClassLogger().Info("Application Started");
        }


        protected override void OnExit(ExitEventArgs exitEventArgs)
        {
            LogManager.Shutdown();
        }
    }
}
