using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Resources;
using NodaTime;
using NodaTime.Text;
using Shouldly;
using SRHWiscMano.Core.Models;
using SRHWiscMano.Core.Services;

namespace SRHWiscMano.Test
{
    [TestFixture]
    internal class TestExamData : TestModelBase
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
        /// ExamData 을 읽는 기능을 테스트 
        /// </summary>
        [TestCase("100.txt")]
        public void TestImportExamData(string fileName)
        {
            var examImporter = provider.GetService<IImportService<IExamData>>();
            var examData = examImporter.ReadFromFile(LoadTestData(fileName));

            examData.ShouldNotBeNull();

            Console.WriteLine(examData.ToString());
        }

        [TestCase("100.txt")]
        public void TestExamDataExtensions(string fileName)
        {
            var examImporter = provider.GetService<IImportService<IExamData>>();
            var examData = examImporter.ReadFromFile(LoadTestData(fileName));

            Console.WriteLine($"Sensor Range : {examData.SensorCount()}");
            Console.WriteLine($"Tick Amount  : {examData.TickAmount()}");
            Console.WriteLine($"Total Interval : {examData.TotalTime()}");
            Console.WriteLine($"Total Duration : {examData.TotalDuration()}");
        }



    }

    
}
