using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;

namespace SRHWiscMano.Test
{
    [TestFixture]
    internal class TestExamination : TestModelBase
    {
        private ServiceProvider provider;
        [OneTimeSetUp]
        public void SetupConfigServices()
        {
            var services = new ServiceCollection();
            SRHWiscMano.Core.ServiceRegistration.ConfigureServices(services);
            provider = services.BuildServiceProvider();
        }


        /// <summary>
        /// TimeSeriesData 을 읽는 기능을 테스트 
        /// </summary>
        [TestCase("100.txt")]
        public void TestImportExamData(string fileName)
        {
            var examImporter = provider.GetService<IImportService<IExamination>>();
            var examData = examImporter.ReadFromFile(LoadTestData(fileName));

            examData.ShouldNotBeNull();

            Console.WriteLine(examData.ToString());
        }

        [TestCase("100.txt")]
        public void TestExamDataExtensions(string fileName)
        {
            var examImporter = provider.GetService<IImportService<IExamination>>();
            var examData = examImporter.ReadFromFile(LoadTestData(fileName));

            Console.WriteLine($"Sensor Range : {examData.SensorCount()}");
            Console.WriteLine($"Tick Amount  : {examData.TickAmount()}");
            Console.WriteLine($"Total Interval : {examData.TotalTime()}");
            Console.WriteLine($"Total Duration : {examData.TotalDuration()}");
        }



    }

    
}
