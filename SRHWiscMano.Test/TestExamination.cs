using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SRHWiscMano.Core.Helpers;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;
using System.Reflection;
using SRHWiscMano.App.Services;

namespace SRHWiscMano.Test
{
    [TestFixture]
    internal class TestExamination : TestModelBase
    {
        private ServiceProvider provider;
        [OneTimeSetUp]
        public void SetupConfigServices()
        {
            Console.WriteLine($"{this.GetType().Namespace}");

            var services = new ServiceCollection();
            SRHWiscMano.Core.ServiceRegistration.ConfigureServices(services);
            SRHWiscMano.App.ServiceRegistration.ConfigureServices(services);
            provider = services.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            provider.Dispose();
        }

        [Test]
        public void TestSensorRange()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");
            Range<int> range = new Range<int>(1, 3);
            foreach (var i in range.AsEnumerable())
            {
                Console.WriteLine($"Range {i}");
            }
        }

        /// <summary>
        /// TimeSeriesData 을 읽는 기능을 테스트 
        /// </summary>
        [TestCase("100.txt")]
        public void TestImportExamData(string fileName)
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name} : {fileName}");
            var examImporter = provider.GetService<IImportService<IExamination>>();
            var examData = examImporter.ReadFromFile(LoadTestData(fileName));

            examData.ShouldNotBeNull();

            Console.WriteLine(examData.ToString());
        }

        [TestCase("100.txt")]
        public void TestExamDataExtensions(string fileName)
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name} : {fileName}");
            var examImporter = provider.GetService<IImportService<IExamination>>();
            var examData = examImporter.ReadFromFile(LoadTestData(fileName));

            Console.WriteLine($"Sensor Range : {examData.SensorCount()}");
            Console.WriteLine($"Tick Amount  : {examData.TickAmount()}");
            Console.WriteLine($"Total Interval : {examData.TotalTime()}");
            Console.WriteLine($"Total Duration : {examData.TotalDuration()}");
        }


        [TestCase("100.txt")]
        public void TestExamDataInterpolation(string fileName)
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name} : {fileName}");
            var examImporter = provider.GetService<IImportService<IExamination>>();
            var examData = examImporter.ReadFromFile(LoadTestData(fileName));
            var sharedService = provider.GetService<SharedService>();

            PerfBenchmark bench = new PerfBenchmark("Loading ExamData");
            bench.Start();
            sharedService.SetExamData(examData);
            bench.Stop();
            Console.WriteLine(bench.GetCheckPointsInfos().ToStringJoin("\r\n"));


            Console.WriteLine($"Sensor Range : {examData.SensorCount()}");
            Console.WriteLine($"Tick Amount  : {examData.TickAmount()}");
            Console.WriteLine($"Total Interval : {examData.TotalTime()}");
            Console.WriteLine($"Total Duration : {examData.TotalDuration()}");
        }



    }

    
}
