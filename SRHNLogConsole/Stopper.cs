using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHNLogConsole
{
    internal class Stopper
    {
        private readonly ILogger<Stopper> _logger;

        public Stopper(ILogger<Stopper> logger)
        {
            _logger = logger;
        }

        public void DoAction(string name)
        {
            _logger.LogDebug(20, "Doing hard work! {Action}", name);
        }
    }
}
