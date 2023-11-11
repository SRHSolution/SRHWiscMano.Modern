using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SRHWiscMano.App.Data;
using SRHWiscMano.App.Services;
using SRHWiscMano.App.ViewModels;
using SRHWiscMano.App.Windows;
using SRHWiscMano.Core.ViewModels;
using Microsoft.Extensions.Options;
using SRHWiscMano.Core.Services;
using System;
using System.IO;

namespace SRHWiscMano.App
{
    public static class ServiceRegistration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SharedService>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<IViewerViewModel, ViewerViewModel>();
            services.AddSingleton<IExplorerViewModel, ExplorerViewModel>();
            services.AddSingleton<IAnalyzerViewModel, AnalyzerViewModel>();
            services.AddSingleton<ISettingViewModel, SettingViewModel>();

            services.AddTransient<ColorRangeSliderViewModel>();
            services.AddSingleton<LoggerWindow>();

            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "configuration.json");
            var config = new ConfigurationBuilder()
                // .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configPath, optional: true, reloadOnChange: true)
                .Build();

            config.GetSection("AppSettings")["FilePath"] = configPath;

            services.Configure<AppSettings>(config.GetSection("AppSettings"));    // requires Microsoft.Extensions.ConfigurationExtensions, IOptions<AppSettings> 를 등록한다.

            // appsettings.json 파일을 로드한다.
            // var config = new ConfigurationBuilder()
            //     .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            //     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //     .Build();

            services.AddLogging(loggingBuilder =>
            {
                // loggingBuilder.ClearProviders();
                // loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                // loggingBuilder.AddNLog(config);
                loggingBuilder.AddNLog();
            });
        }

    }
}
