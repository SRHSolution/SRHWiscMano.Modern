using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRHWiscMano.App.Data
{
    public class AppSettings
    {
        public string FilePath { get; set; }
        
        public string BaseTheme { get; set; } = "Light";
        
        public string AccentTheme { get; set; } = "Blue";
        
        public int MaxRecentFileSize { get; set; } = 5;
        
        public List<string> RecentFiles { get; set; }
    }
}
