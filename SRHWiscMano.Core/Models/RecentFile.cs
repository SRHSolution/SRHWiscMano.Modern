using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SRHWiscMano.Core.Models
{
    public class RecentFile
    {
        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);

        public RecentFile(string filePath)
        {
            this.FilePath = filePath;
        }
    }
}
