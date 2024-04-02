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
            services.AddSingleton<SharedService>();                         // For Shared Data
            services.AddSingleton<MainWindowViewModel>();                   // For MainWindow
            services.AddSingleton<IViewerViewModel, ViewerViewModel>();     // For ViewerView
            services.AddSingleton<IExplorerViewModel, ExplorerViewModel>(); // For ExplorerView
            services.AddSingleton<IReportViewModel, ReportViewModel>(); // For AnalyzerView
            services.AddSingleton<IAnalyzerViewModel, AnalyzerViewModel>(); // For AnalyzerView
            services.AddSingleton<ISettingViewModel, SettingViewModel>();   // For SettingView
            
            // services.AddTransient<ColorRangeSliderViewModel>();
            services.AddSingleton<LoggerWindow>();

            // AppSettings Configuration service.
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "configuration.json");
            var config = new ConfigurationBuilder()
                // .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configPath, optional: true, reloadOnChange: true)
                .Build();

            config.GetSection("AppSettings")["FilePath"] = configPath;

            // requires Microsoft.Extensions.ConfigurationExtensions, IOptions<AppSettings> 를 등록한다.
            // config에 지정된 파일 configuration.json 파일에서 각각의 Section의 정의를 지정된 클래스와 mapping하여 등록한다
            services.Configure<AppSettings>(config.GetSection("AppSettings"));    
            services.Configure<ConfigAlgorithms>(config.GetSection("ConfigAlgorithms"));    

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
