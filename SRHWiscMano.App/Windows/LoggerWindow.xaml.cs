using MahApps.Metro.Controls;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NLog.Targets;
using SRHWiscMano.App.Controls;

namespace SRHWiscMano.App.Windows
{
    /// <summary>
    /// Interaction logic for LoggerWindow.xaml
    /// </summary>
    public partial class LoggerWindow : MetroWindow
    {
        private WpfLoggerTarget? wpfLogger;
        public bool AllowClose { get; set; }

        public LoggerWindow()
        {
            InitializeComponent();


            wpfLogger = LogManager.Configuration?.FindTargetByName<WpfLoggerTarget>("wpfLogger");
            if (wpfLogger != null)
            {
                wpfLogger.LogMessageReceived += UpdateLog;
            }
        }

        private void UpdateLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText(message + Environment.NewLine);
            });
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!AllowClose)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}
