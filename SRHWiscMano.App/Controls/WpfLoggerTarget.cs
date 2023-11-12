using NLog.Targets;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.App.Controls
{
    [Target("WpfLogger")]
    public class WpfLoggerTarget : TargetWithLayout
    {
        public Action<string>? LogMessageReceived;

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = this.Layout.Render(logEvent);
            LogMessageReceived?.Invoke(logMessage);
        }
    }
}
