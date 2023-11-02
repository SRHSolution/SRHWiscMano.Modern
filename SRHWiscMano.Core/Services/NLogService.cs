using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace SRHWiscMano.Core.Services
{
    public class NLogLoggerFactory : ILoggerFactory
    {
        public ILoggerService CreateLogger<T>()
        {
            return new NLogService(typeof(T).FullName);
        }
    }

    public class NLogService : ILoggerService
    {
        private readonly Logger _logger;

        public NLogService(string loggerName)
        {
            _logger = NLog.LogManager.GetLogger(loggerName);
        }

        public void LogInformation(string message)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string message)
        {
            throw new NotImplementedException();
        }

        public void LogError(string message, Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
