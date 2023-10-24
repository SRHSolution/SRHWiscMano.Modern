using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.Core.Services
{
    internal class AppSettings
    {

    }

    public class SettingsesService<T> : ISettingsService where T : class
    {
        private readonly IConfiguration _configuration;

        public SettingsesService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("configuration.json", optional: true, reloadOnChange: true);

            // _configuration = builder.Build().GetSection("sdsd").GetSection("").
            // _configuration.GetSection("").
            // _configuration.GetSection("Settings").
            //
            // builder..GetSection(nameof(TransientFaultHandlingOptions))
            //     .Bind(options);

        }

    }
}
        