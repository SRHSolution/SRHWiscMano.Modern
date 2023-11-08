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


    /// <summary>
    /// Logger에 등록할 수 있는 Target에 대한 정보, NLog.config 파일에 입력한다.
    /// https://github.com/NLog/NLog/wiki/Configuration-file#configuration-file-locations
    /// https://nlog-project.org/config/?tab=targets
    /// </summary>
    public class NLogService : ILoggerService
    {
        private readonly Logger _logger;

        public NLogService(string loggerName)
        {
            _logger = NLog.LogManager.GetLogger(loggerName);
        }

        public void LogInformation(string message)
        {
            _logger.Info(message);
        }   

        public void LogWarning(string message)
        {
            _logger.Warn(message);
        }

        public void LogError(string message, Exception ex)
        {
            _logger.Error(message, ex);
        }
    }
}
