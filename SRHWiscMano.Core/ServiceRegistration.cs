using Microsoft.Extensions.DependencyInjection;
using NLog;
using NodaTime;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;
using SRHWiscMano.Core.ViewModels;

namespace SRHWiscMano.Core
{
    public static class ServiceRegistration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // services.AddSingleton<IClock, SystemClock>(sp => SystemClock.Instance);
            services.AddTransient<IExamViewModel, ExamViewModel>();
            // services.AddTransient<ISnapshotViewModel, SnapshotViewModel>();
            services.AddTransient<IImportService<ITimeSeriesData>, TimeSeriesDataTextReader>();
            // services.AddTransient<ISnapshotLabelsParser, SnapshotLabelsParser>();
            // services.AddTransient<IExaminationDataImporter, ExaminationDataTextReader>();
            // services.AddTransient<IExaminationDataSerializer, ExaminationDataSerializer>();
            // services.AddTransient<IRegionFinder, RegionFinder>();
            // services.AddTransient<IResultsCalculator, ResultsCalculator>();
        }

    }
}
