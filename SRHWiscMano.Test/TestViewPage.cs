using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace SRHWiscMano.Test
{
    [TestFixture, Apartment(ApartmentState.STA)]
    internal class TestViewPage
    {
        private IServiceProvider provider;
        private MetroWindow w = new();

        [OneTimeSetUp]
        public void SetupViewModel()
        {
            provider = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            // services.AddSingleton<IViewerPage, ViewerPage>();    
            var provider = services.BuildServiceProvider();
            return provider;
        }


        [SetUp]
        public void OnSetup()
        {
            w.Width = 800;
            w.Height = 600;

            w.Closed += (sender, args) =>
            {
                // Stop the Dispatcher when the window is closed.
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            };
        }

        [TearDown]
        public void OnTearDown()
        {
            if (Environment.GetEnvironmentVariable("SKIP_STA_TEST") == "true")
            {
                Console.WriteLine("Skipping this test as per environment variable setting.");
                return;
                // Assert.Ignore("Skipping this test as per environment variable setting.");
            }

            w.Show();
            // Start the dispatcher to make it interactive.
            Dispatcher.Run();
        }

        [Test]
        public void TestViewerPage()
        {
            ;
        }
    }
}