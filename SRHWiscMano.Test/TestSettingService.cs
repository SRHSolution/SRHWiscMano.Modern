using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SRHWiscMano.Test
{
    [TestFixture]
    internal class TestSettingService : TestModelBase
    {
        private ServiceProvider provider;

        [OneTimeSetUp]
        public void SetupOneTime()
        {
            Console.WriteLine($"{this.GetType().Namespace}");
        }

        [TearDown]
        public void OneTimeTearDown()
        {
            provider.Dispose();
        }


        [Test]
        public void TestInjection()
        {
            Console.WriteLine($"Called {this.GetType().Namespace}.{MethodBase.GetCurrentMethod().Name}");
            var services = new ServiceCollection();
            SRHWiscMano.Core.ServiceRegistration.ConfigureServices(services);
            SRHWiscMano.App.ServiceRegistration.ConfigureServices(services);
            provider = services.BuildServiceProvider();
        }
    }
}
