using Microsoft.Extensions.DependencyInjection;

namespace SRHWiscMano.Test
{
    [TestFixture]
    internal class TestSettingService : TestModelBase
    {
        private ServiceProvider provider;

        [Test]
        public void TestInjection()
        {
            var services = new ServiceCollection();
            SRHWiscMano.Core.ServiceRegistration.ConfigureServices(services);
            SRHWiscMano.App.ServiceRegistration.ConfigureServices(services);
            provider = services.BuildServiceProvider();
        }
    }
}
