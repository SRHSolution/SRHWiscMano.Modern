using Microsoft.Extensions.DependencyInjection;
using SRHWiscMano.Core.ViewModels;
using SRHWiscMano.Core.Helpers;
using System.Reflection;

namespace SRHWiscMano.Test
{
    [TestFixture]
    public class NLogServiceTest : TestModelBase
    {
        private ServiceProvider provider;
        [OneTimeSetUp]
        public void SetupConfigServices()
        {
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
        public void WriteLogInform()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");
            var vmViewer = provider.GetService<IViewerViewModel>();
        }

        [Test]
        public void GetPalettes()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");
            var dics = PaletteUtils.GetPredefinedPalettes();
        }
    }
}
