using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SRHWiscMano.App.Services;
using SRHWiscMano.App.ViewModels;
using SRHWiscMano.App.Windows;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App
{
    public static class ServiceRegistration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SharedService>();
            services.AddSingleton<IViewerViewModel, ViewerViewModel>();
            services.AddTransient<ColorRangeSliderViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<LoggerWindow>();

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
