using Microsoft.Extensions.DependencyInjection;
using SRHWiscMano.Core.ViewModels;
using SRHWiscMano.Core.Helpers;

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

        [Test]
        public void WriteLogInform()
        {
            var vmViewer = provider.GetService<IViewerViewModel>();
        }

        [Test]
        public void GetPalettes()
        {
            var dics = PaletteUtils.GetPredefinedPalettes();
        }
    }
}
