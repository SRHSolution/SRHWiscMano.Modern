using Microsoft.Extensions.DependencyInjection;
using SRHWiscMano.App.ViewModels;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.App
{
    public static class ServiceRegistration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IViewerViewModel, ViewerViewModel>();
            services.AddTransient<ColorRangeSliderViewModel>();
            
            // services.AddSingleton<I>
            // services.AddTransient<IResultsCalculator, ResultsCalculator>();
        }

    }
}
