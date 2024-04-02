using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OxyPlot;

namespace SRHWiscMano.App.Data
{
    public class AppSettings
    {
        public string FilePath { get; set; } = string.Empty;
        
        public string BaseTheme { get; set; } = "Light";
        public Color BaseBackColor { get; set; }
        public Color BaseForeColor { get; set; }

        public string AccentTheme { get; set; } = "Blue";
        public Color AccentThemeColor { get; set; }
        
        public int MaxRecentFileSize { get; set; } = 5;

        public long MainViewFrameRange { get; set; } = 20000;
        public int InterpolateSensorScale { get; set; } = 10;

        public bool UpdateSubRange { get; set; } = true;

        public List<string>? RecentFiles { get; set; }

        // public double MinSensorBound { get; set; } = 0;
        // public double MaxSensorBound { get; set; } = 36;

        /// <summary>
        /// TimeFrame의 시간간격
        /// </summary>
        public double TimeFrameDurationInMillisecond { get; set; } = 3000;

        public string SelectedPaletteKey { get; set; } = "Mid";
    }
}
